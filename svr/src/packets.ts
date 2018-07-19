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

// 생성 기준 좌표만 알려주기
// 이동 정보같은건 다른 패킷으로 받을수 있다
export interface PlayerListPacket {
  players: {
    id: number;
    nickname: string;
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

export interface StaticItemListPacket {
  items: StaticItemCreatePacket[];
}

export interface StaticItemCreatePacket {
  type: string;
  id: number;
  pos_x: number;
  pos_y: number;
}

export interface StaticItemRemovePacket {
  type: string;
  id: number;
}
