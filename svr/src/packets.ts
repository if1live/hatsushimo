import { ReplicationActions } from "./events";
import { Rank } from './leaderboard';

export interface Serializable {
  serialize(): Buffer;
}

export interface Deserializable {
  deserialize(data: Buffer): void;
}

export class StatusPingPacket implements Deserializable {
  millis: number;

  deserialize(data: Buffer) {
    this.millis = data.readUInt32LE(0);
  }
}

export class StatusPongPacket implements Serializable {
  millis: number;

  serialize(): Buffer {
    const buffer = new Buffer(4);
    buffer.writeInt32LE(this.millis, 0);
    return buffer;
  }
}

// 게임 작동에 필요한 상수 및 검증 코드 알려주기
export interface WelcomePacket {
  version: number;

  room_width: number;
  room_height: number;
}

export interface RoomJoinRequestPacket {
  nickname: string;
  room_id: string;
}

export interface RoomJoinResponsePacket {
  room_id: string;
  player_id: number;
  nickname: string;
}

export interface RoomLeavePacket {
  player_id: number;
}

export interface InputCommandPacket {
  mode: number;
}

export interface InputMovePacket {
  dir_x: number;
  dir_y: number;
}

export interface ReplicationAllPacket {
  players: {
    id: number;
    nickname: string;
    pos_x: number;
    pos_y: number;
  }[];
  items: {
    id: number;
    type: string;
    pos_x: number;
    pos_y: number;
  }[];
}

export interface ReplicationActionPacket {
  action: ReplicationActions;
  id: number;
  type?: string;
  pos_x?: number;
  pos_y?: number;
  dir_x?: number;
  dir_y?: number;
  speed?: number;
  extra?: string;
}

export const makeReplicationRemovePacket = (id: number): ReplicationActionPacket => {
  return {
    action: ReplicationActions.Remove,
    id: id,
  };
}

export interface ReplicationBulkActionPacket {
  actions: ReplicationActionPacket[];
}

export interface LeaderboardPacket {
  top: Rank[];
  players: number;
}
