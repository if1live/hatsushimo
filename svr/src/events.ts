export enum Events {
  WELCOME = 1,

  STATUS_PING,
  STATUS_PONG,

  ROOM_JOIN,
  ROOM_LEAVE,

  LEADERBOARD,

  INPUT_MOVE,
  INPUT_COMMAND,

  REPLICATION_ALL,
  REPLICATION_ACTION,
  REPLICATION_BULK_ACTION,

  PLAYER_READY,
}

export enum ReplicationActions {
  Create = 'create',
  Update = 'update',
  Remove = 'remove',
}
