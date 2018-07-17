import { createServer } from 'http';
import { default as express } from 'express';
import { default as socketIo } from 'socket.io';
import { default as uuidv1 } from 'uuid/v1';

import { Player } from './player';
import { RoomManager } from './room';

const app = express();
const server = createServer(app);
const io = socketIo(server);

const players: Player[] = [];
const roomManager = new RoomManager();

interface RoomJoinRequest {
  nickname: string;
  room_id: string;
}

interface RoomJoinResponse {
  ok: boolean;
  room_id: string;
  player_id: string;
  nickname: string;
}

interface MoveRequest {
  dirX: number;
  dirY: number;
}

io.on('connect', (client) => {
  const uuid = uuidv1();
  const player = new Player(client, uuid);
  players.push(player);

  console.log(`user connect - id=${player.playerID}, current_user=${players.length}`);

  client.on('disconnect', () => {
    if(player.roomID) {
      const room = roomManager.getRoom(player.roomID);
      room.LeavePlayer(player);
    }

    const found = players.findIndex(x => x == player);
    players.splice(found, 1);
    console.log(`user disconnect - id=${player.playerID}, current_user=${players.length}`);
  });

  client.on('status-ping', (data) => {
    client.emit('status-pong', data);
  });

  client.on('room-join', (req: RoomJoinRequest) => {
    const nickname = req.nickname;
    player.nickname = nickname;

    const room = roomManager.getRoom(req.room_id);
    room.JoinPlayer(player);

    const resp: RoomJoinResponse = {
      ok: true,
      room_id: room.roomID,
      player_id: player.playerID,
      nickname,
    };
    client.emit('room-join', resp);
  });

  // 방에 접속하면 클라이언트에서 게임씬 로딩을 시작한다
  // 로딩이 끝난 다음부터 객체가 의미있도록 만들고싶다
  // 게임 로딩이 끝나기전에는 무적으로 만드는게 목적
  client.on('ready', () => {
    player.ready = true;

    const x = (Math.random() - 0.5) * 10;
    const y = (Math.random() - 0.5) * 10;
    player.setPosition(x, y);

    console.log(`user ready - id=${player.playerID}`);

    client.emit('ready');
  });

  client.on('move', (req: MoveRequest) => {
    const speed = 10;
    const len = Math.sqrt(req.dirX * req.dirX + req.dirY * req.dirY);
    
    if(len === 0) {
      player.setVelocity(0, 0, speed);

    } else {
      const dirX = req.dirX / len;
      const dirY = req.dirY / len;
      player.setVelocity(dirX, dirY, speed);
    }
  });
});


app.get('/status/users', (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  res.send(JSON.stringify(players.map(u => u.toJson()), null, 2));
});

server.listen(3000, () => {
  console.log('listening on *:3000');
});
