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

export interface PlayerListPacket {
  players: {
    id: number;
    nickname: string;
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
