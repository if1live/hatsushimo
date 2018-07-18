export class Player {
  // 접속 정보
  client: SocketIO.Socket;

  ID: number;
  roomID: string | null;
  nickname: string;

  posX: number;
  posY: number;

  dirX: number;
  dirY: number;
  speed: number;

  score: number;

  constructor(client: SocketIO.Socket, id: number) {
    this.client = client;

    this.ID = id;
    this.roomID = null;
    this.nickname = '[BLANK]';

    this.setPosition(0, 0);
    this.setVelocity(0, 0, 1);
    this.score = 0;
  }

  toJson() {
    const obj = Object.assign({}, this);
    obj.client = undefined;
    return obj;
  }

  setPosition(x: number, y: number) {
    this.posX = x;
    this.posY = y;
  }

  setVelocity(dirX: number, dirY: number, speed: number) {
    this.dirX = dirX;
    this.dirY = dirY;
    this.speed = speed;
  }
}
