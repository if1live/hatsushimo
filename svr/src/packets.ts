export interface RoomJoinRequestPacket {
  nickname: string;
  room_id: string;
}

export interface RoomJoinResponsePacket {
  room_id: string;
  player_id: number;
  nickname: string;
}

export interface MoveRequestPacket {
  dir_x: number;
  dir_y: number;
}

export interface ReplicationPacket {
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

export interface PlayerStatusPacket {
  players: {
    id: number;

    pos_x: number;
    pos_y: number;
    dir_x: number;
    dir_y: number;
    speed: number;
  }[];
}

export interface LeaderboardPacket {
  players: {
    id: number;
    score: number;
    ranking: number;
  }[];
}

export interface PlayerSpawnPacket {
  id: number;
  nickname: string;
  pos_x: number;
  pos_y: number;
}

export interface PlayerDeadPacket {
  id: number;
}

export interface PlayerLeavePacket {
  id: number;
}

export interface StaticItemCreatePacket {
  type: string;
  id: number;
  pos_x: number;
  pos_y: number;
}

export interface StaticItemRemovePacket {
  id: number;
}
