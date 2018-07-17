export class Player {
  // 접속 정보
  client: SocketIO.Socket;

  playerID: string;
  roomID: string | null;
  nickname: string;

  ready: boolean;

  // 게임 상태
  posX: number;
  posY: number;

  // 방향
  dirX: number;
  dirY: number;
  speed: number;

  constructor(client: SocketIO.Socket, uuid: string) {
    this.client = client;

    this.playerID = uuid;
    this.roomID = null;
    this.nickname = '[BLANK]';

    this.ready = false;

    this.setPosition(0, 0);
    this.setVelocity(0, 0, 1);
  }

  toJson() {
    const obj = Object.assign({}, this);
    obj.client = undefined;
    return obj;
  }

  setPosition(posX: number, posY: number) {
    this.posX = posX;
    this.posY = posY;
  }

  setVelocity(dirX: number, dirY: number, speed: number) {
    this.dirX = dirX;
    this.dirY = dirY;
    this.speed = speed;
  }
}
