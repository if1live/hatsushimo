import { createServer } from 'http';
import { default as express } from 'express';
import { default as socketIo } from 'socket.io';

import { Player } from './player';
import { RoomManager } from './room';
import { makePlayerID } from './idgen';
import { Connection } from './connection';
import * as H from './helpers';
import * as P from './packets';
import { Events as E } from './events';
import * as C from './config';

const app = express();
const server = createServer(app);
const io = socketIo(server);

const players: Player[] = [];
const roomManager = new RoomManager();

// 플레이어에게는 고유번호 붙인다
// 어떤 방에 들어가든 id는 유지된다
const playerIDGenerator = makePlayerID();

io.on('connect', (client) => {
  const conn = new Connection(client);
  const id = playerIDGenerator.next().value;
  const player = new Player(conn, id);
  players.push(player);

  console.log(`user connect - id=${player.ID}, current_user=${players.length}`);

  client.on('disconnect', () => {
    if (player.roomID) {
      const room = roomManager.getRoom(player.roomID);
      room.leavePlayer(player);
    }

    const found = players.findIndex(x => x == player);
    players.splice(found, 1);
    console.log(`user disconnect - id=${player.ID}, current_user=${players.length}`);
  });

  // welcome
  const welcomePacket: P.WelcomePacket = {
    version: 1234,
    room_width: C.ROOM_WIDTH,
    room_height: C.ROOM_HEIGHT,
  };
  conn.emit(E.WELCOME, welcomePacket);

  // ping
  conn.on(E.STATUS_PING, (data: Buffer) => {
    const ping = new P.StatusPingPacket();
    ping.deserialize(data);

    const pong = new P.StatusPongPacket();
    pong.millis = ping.millis;
    conn.emit(E.STATUS_PONG, pong.serialize());
  });

  // room
  conn.on(E.ROOM_JOIN, (req: P.RoomJoinRequestPacket) => {
    player.reset();
    const nickname = req.nickname;
    player.nickname = nickname;

    const room = roomManager.getRoom(req.room_id);
    room.joinPlayer(player);

    conn.emit(E.ROOM_JOIN, room.makeRoomJoinResponsePacket(player));
  });

  conn.on(E.ROOM_LEAVE, () => {
    if (!player.roomID) { return; }
    const room = roomManager.getRoom(player.roomID);
    room.leavePlayer(player);
    conn.emit(E.ROOM_LEAVE);
  });

  // 방에 접속하면 클라이언트에서 게임씬 로딩을 시작한다
  // 로딩이 끝난 다음부터 객체가 의미있도록 만들고싶다
  // 게임 로딩이 끝나기전에는 무적으로 만드는게 목적
  conn.on(E.PLAYER_READY, () => {
    if (!player.roomID) { return; }
    const room = roomManager.getRoom(player.roomID);
    room.spawnPlayer(player);
  });

  conn.on(E.INPUT_MOVE, (req: P.InputMovePacket) => {
    const speed = 10;
    const len = H.getLengthVec2(req.dir_x, req.dir_y);

    if (len === 0) {
      player.setVelocity(0, 0, speed);

    } else {
      const dir = H.normalizeVec2(req.dir_x, req.dir_y);
      player.setVelocity(dir[0], dir[1], speed);
    }
  });

  conn.on(E.INPUT_COMMAND, (req: P.InputCommandPacket) => {
    // TODO exec action
    console.log(`input-command : ${JSON.stringify(req)}`);
  });
});


app.get('/status/users', (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  res.send(JSON.stringify(players.map(u => u.toJson()), null, 2));
});

const port = C.SERVER_PORT;
server.listen(port, () => {
  console.log(`listening on *:${port}`);
});
