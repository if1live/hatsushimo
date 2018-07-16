import { createServer } from 'http';
import { default as express } from 'express';
import { default as socketIo } from 'socket.io';

const app = express();
const server = createServer(app);
const io = socketIo(server);

class User {
  socketId: string;
  nickname: string;
  ready: boolean;

  posX: number;
  posY: number;

  constructor(socketId: string) {
    this.socketId = socketId;
    this.nickname = '[BLANK]';
    this.ready = false;
  }  

  setPosition(posX: number, posY: number) {
    this.posX = posX;
    this.posY = posY;
  }
}

let users: User[] = [];

io.on('connect', (client) => {
  const user = new User(client.id);
  users.push(user);

  console.log(`user connected - id=${user.socketId}, current_user=${users.length}`);

  client.on('disconnect', () => {
    users = users.filter(x => x !== user);
    console.log(`user disconnected - id=${user.socketId}, current_user=${users.length}`);
  });

  client.on('status-ping', (data) => {
    client.emit('status-pong', data);
  });

  client.on('room-join-req', (data) => {
    const nickname = data.nickname;
    user.nickname = nickname;

    const room_id = 'todo-support-multi-room';
    // TODO: player 고유 번호는 소켓 대신 다른곳에서 가져오는게 좋을거같은데
    const player_id = client.id;

    client.emit('room-join-resp', {
      ok: true,
      room_id,
      player_id,
      nickname,
    });
  });

  // 방에 접속하면 클라이언트에서 게임씬 로딩을 시작한다
  // 로딩이 끝난 다음부터 객체가 의미있도록 만들고싶다
  // 게임 로딩이 끝나기전에는 무적으로 만드는게 목적
  client.on('ready-req', () => {
    user.ready = true;

    const x = (Math.random() - 0.5) * 10;
    const y = (Math.random() - 0.5) * 100;
    user.setPosition(x, y);

    console.log(`user ready - id=${user.socketId}`);

    client.emit('ready-resp', {
      posX: user.posX,
      posY: user.posY
    });
  });

  // 플레이어들의 위치를 주기적으로 보내주기
  // TODO: zone 구현하기. 몇명 없을때는 필요없을거같다
});

app.get('/status/users', (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  res.send(JSON.stringify(users, null, 2));
});

server.listen(3000, () => {
  console.log('listening on *:3000');
});
