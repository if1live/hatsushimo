import { default as socketIo } from 'socket.io-client';

const socket = socketIo.connect('http://127.0.0.1:3000');

socket.on('connect', () => {
  console.log('client connection');

  checkPing();
});

socket.on('disconnect', () => {
  console.log('disconnection!');
});

// ping
socket.on('status-pong', (data: any) => {
  const now = new Date().getTime();
  const prev = data.ts;

  const diff = now - prev;
  console.log(`ping: ${diff}ms`);

  setTimeout(checkPing, 5000);
});

function checkPing() {
  const now = new Date().getTime();
  socket.emit('status-ping', {ts: now});
}
