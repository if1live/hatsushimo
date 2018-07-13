import { createServer, Server } from 'http';
import { default as express } from 'express';
import { default as socketIo } from 'socket.io';

const app = express();
const server = createServer(app);
const io = socketIo(server);

io.on('connect', (client) => {
  console.log('a user connected');

  client.on('disconnect', () => {
    console.log('user disconnected');
  });

  client.on('status-ping', (data) => {
    client.emit('status-pong', data);
  })
});

server.listen(3000, () => {
  console.log('listening on *:3000');
});
