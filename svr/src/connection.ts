import { Events } from './events';

const makeString = (ev: Events) => {
  return ev.toString();
}

export class Connection {
  sock: SocketIO.Socket;

  constructor(sock: SocketIO.Socket) {
    this.sock = sock;
  }

  on(ev: Events, listener: (...args: any[]) => void): this {
    const s = makeString(ev);
    this.sock.on(s, listener);
    return this;
  }

  emit(ev: Events, ...args: any[]): boolean {
    const s = makeString(ev);
    return this.sock.emit(s, ...args);
  }
}
