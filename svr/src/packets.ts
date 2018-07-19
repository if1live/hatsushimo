import { ReplicationActions } from "./events";

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

export interface MoveRequestPacket {
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
  ranks: {
    id: number;
    score: number;
    rank: number;
  }[];
}
