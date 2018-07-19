import * as P from './packets';
import { ReplicationActions } from './events';

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

  makeCreatePacket(): P.ReplicationActionPacket {
    return {
      action: ReplicationActions.Create,
      id: this.ID,
      pos_x: this.posX,
      pos_y: this.posY,
      type: this.type,
      // TODO remove optional field
      dir_x: 0,
      dir_y: 0,
      extra: '',
      speed: 0,
    };
  }

  makeRemovePacket(): P.ReplicationActionPacket {
    return P.makeReplicationRemovePacket(this.ID);
  }
}
