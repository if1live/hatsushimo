import { createServer } from 'http';
import { default as express } from 'express';
import { default as socketIo } from 'socket.io';

import { Player } from './player';
import { RoomManager } from './room';
import { IDGenerator } from './idgen';
import * as H from './helpers';
import * as P from './packets';

const app = express();
const server = createServer(app);
const io = socketIo(server);

const players: Player[] = [];
const roomManager = new RoomManager();

// 플레이어에게는 고유번호 붙인다
// 어떤 방에 들어가든 id는 유지된다
const playerIDGenerator = IDGenerator(1, 100000);

io.on('connect', (client) => {
  const id = playerIDGenerator.next().value;
  const player = new Player(client, id);
  players.push(player);

  console.log(`user connect - id=${player.ID}, current_user=${players.length}`);

  client.on('disconnect', () => {
    if (player.roomID) {
      const room = roomManager.getRoom(player.roomID);
      room.LeavePlayer(player);
    }

    const found = players.findIndex(x => x == player);
    players.splice(found, 1);
    console.log(`user disconnect - id=${player.ID}, current_user=${players.length}`);
  });

  client.on('status-ping', (data) => {
    client.emit('status-pong', data);
  });

  client.on('room-join', (req: P.RoomJoinRequestPacket) => {
    const nickname = req.nickname;
    player.nickname = nickname;

    const room = roomManager.getRoom(req.room_id);
    room.JoinPlayer(player);

    const resp: P.RoomJoinResponsePacket = {
      room_id: room.ID,
      player_id: player.ID,
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

    console.log(`user ready - id=${player.ID}`);

    client.emit('ready');
  });

  client.on('move', (req: P.MoveRequestPacket) => {
    const speed = 10;
    const len = H.getLengthVec2(req.dir_x, req.dir_y);

    if (len === 0) {
      player.setVelocity(0, 0, speed);

    } else {
      const dir = H.normalizeVec2(req.dir_x, req.dir_y);
      player.setVelocity(dir[0], dir[1], speed);
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
