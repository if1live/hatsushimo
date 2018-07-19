import * as P from './packets';

const ITEM_FOOD = 'food';

export class Food {
  ID: number;
  type: string;
  posX: number;
  posY: number;
  score: number;

  constructor(id: number, posX: number, posY: number, score: number) {
    this.ID = id;
    this.type = ITEM_FOOD;
    this.posX = posX;
    this.posY = posY;
    this.score = score;
  }

  makeCreatePacket(): P.StaticItemCreatePacket {
    return {
      id: this.ID,
      pos_x: this.posX,
      pos_y: this.posY,
      type: this.type,
    };
  }

  makeRemovePacket(): P.StaticItemRemovePacket {
    return {
      id: this.ID,
    };
  }
}
