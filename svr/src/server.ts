import { createServer, Server } from 'http';
import { default as express } from 'express';
import { default as socketIo } from 'socket.io';

const app = express();
const server = createServer(app);
const io = socketIo(server);

class User {
  socketId: string;

  constructor(socketId: string) {
    this.socketId = socketId;
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
  })
});

server.listen(3000, () => {
  console.log('listening on *:3000');
});
