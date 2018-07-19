export const CONNECT = 'connect';
export const DISCONNECT = 'disconnect';

export const STATUS_PING = 'status-ping';
export const STATUS_PONG = 'status-pong';

export const ROOM_JOIN = 'room-join';
export const ROOM_LEAVE = 'room-leave';

// 리더보드 정보를 주기적으로 내려주기
export const LEADERBOARD = 'leaderboard';

export const MOVE = 'move';

export const REPLICATION_ALL = 'replication-all';
export const REPLICATION_ACTION = 'replication-action';
export const REPLICATION_BULK_ACTION = 'replication-bulk-action';

export enum ReplicationActions {
  Create = 'create',
  Update = 'update',
  Remove = 'remove',
}

export const PLAYER_READY = 'player-ready';
