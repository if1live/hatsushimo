import * as C from './config';
import * as P from './packets';
import { ReplicationActions } from './events';

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

    this.reset();
  }

  reset() {
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

  moveDelta(dx: number, dy: number) {
    const halfw = C.ROOM_WIDTH * 0.5;
    const halfh = C.ROOM_HEIGHT * 0.5;

    this.posX += dx;
    this.posY += dy;
    if (this.posX < -halfw) {
      this.posX = -halfw;
    }
    if (this.posX > halfw) {
      this.posX = halfw;
    }
    if (this.posY < -halfh) {
      this.posY = -halfh;
    }
    if (this.posY > halfh) {
      this.posY = halfh;
    }
  }

  makeSpawnPacket(): P.ReplicationActionPacket {
    return {
      action: ReplicationActions.Create,
      id: this.ID,
      type: 'player',
      pos_x: this.posX,
      pos_y: this.posY,
      dir_x: this.dirX,
      dir_y: this.dirY,
      speed: this.speed,
      extra: this.nickname,
    };
  }

  makeDeadPacket(): P.ReplicationActionPacket {
    return P.makeReplicationRemovePacket(this.ID);
  }
}
