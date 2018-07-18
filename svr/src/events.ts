export const CONNECT = 'connect';
export const DISCONNECT = 'disconnect';

export const STATUS_PING = 'status-ping';
export const STATUS_PONG = 'status-pong';

export const ROOM_JOIN = 'room-join';
export const ROOM_LEAVE = 'room-leave';

// 리더보드 정보를 주기적으로 내려주기
export const LEADERBOARD = 'leaderboard';

export const MOVE = 'move';

export const PLAYER_LIST = 'player-list';
export const PLAYER_STATUS = 'player-status';

// 플레이어 객체의 생존을 관리하는 명령
// 객체의 위치 정보는 주기적으로 내려가지만
// 전체 목록은 별도 관리
export const PLAYER_SPAWN = 'player-spawn';
export const PLAYER_DEAD = 'player-dead';
export const PLAYER_LEAVE = 'player-leave';
export const PLAYER_READY = 'player-ready';


// 바닥에 배치되는 점수 아이템 같은거
// 움직이지 않는 아이템은 좌표 추적이 필요 없다
export const STATIC_ITEM_CREATE = 'static-item-create';
export const STATIC_ITEM_REMOVE = 'static-item-remove';

