import { Player } from './player';
import { makeFoodID } from './idgen';
import * as P from './packets';
import * as E from './events';
import * as H from './helpers';
import * as C from './config';

const INVALID_LOOP_HANDLER = -1;

export class Room {
  players: Player[];

  // 로직에 직접 참가하지 않는 유저목록
  // 방에는 참가했지만 로딩이 끝나지 않는 경우를 처리하는게 목적
  waitingPlayers: Player[];

  ID: string;

  foodIDGen: Generator;

  // handler
  gameLoopHandler: number;
  networkLoopHandler: number;

  constructor(id: string) {
    this.ID = id;
    this.players = [];
    this.waitingPlayers = [];

    // 방에서만 사용하는 객체는 id를 따로 발급
    this.foodIDGen = makeFoodID();

    this.gameLoopHandler = INVALID_LOOP_HANDLER;
    this.networkLoopHandler = INVALID_LOOP_HANDLER;
  }

  spawnPlayer(player: Player) {
    const found = this.waitingPlayers.findIndex(p => p == player);
    if (found < 0) { return; }

    // 기존 유저들에게 새로 생성된 플레이어 정보를 알려주기
    const spawnPacket: P.PlayerSpawnPacket = {
      id: player.ID,
      nickname: player.nickname,
      pos_x: player.posX,
      pos_y: player.posY,
    };
    this.players.map(p => {
      p.client.emit(E.PLAYER_SPAWN, spawnPacket);
    });
    console.log(`ready room - room=${this.ID} player=${player.ID} room_size=${this.players.length}`);

    this.waitingPlayers.splice(found, 1);
    this.players.push(player);

    // 접속한 유저에게 합류 메세지 보내기
    player.client.emit(E.PLAYER_READY);

    // 접속한 유저에게 유저 전체 목록 알려주기
    const players = this.players.map(p => ({
      id: p.ID,
      nickname: p.nickname,
      pos_x: p.posX,
      pos_y: p.posY,
    }));
    const listPacket: P.PlayerListPacket = {
      players: players,
    };
    player.client.emit(E.PLAYER_LIST, listPacket);
  }

  joinPlayer(newPlayer: Player): boolean {
    if (newPlayer.roomID !== null) { return false; }

    newPlayer.roomID = this.ID;
    const pos = H.generateRandomPosition(C.ROOM_WIDTH, C.ROOM_HEIGHT);
    newPlayer.setPosition(pos[0], pos[1]);
    this.waitingPlayers.push(newPlayer);

    console.log(`join room - room=${this.ID} player=${newPlayer.ID} room_size=${this.players.length}`);
    return true;
  }

  leavePlayer(player: Player): boolean {
    if (player.roomID === null) { return false; }

    const found = this.players.findIndex(x => x === player);
    if (found > -1) {
      this.players.splice(found, 1);
      player.roomID = null;
    }

    // 로딩 끝나기전에 나가는 경우 처리
    const foundwaiting = this.waitingPlayers.findIndex(x => x === player);
    if (foundwaiting > -1) {
      this.waitingPlayers.splice(foundwaiting, 1);
      player.roomID = null;
    }

    // 방을 나갔다는것을 다른 유저도 알아야한다
    const packet: P.PlayerLeavePacket = {
      id: player.ID,
    };
    this.players.map(p => {
      p.client.emit(E.PLAYER_LEAVE, packet);
    });

    console.log(`leave room - room=${this.ID} player=${player.ID} room_size=${this.players.length}`);
    return true;
  }

  gameLoop() {
    // TODO ready player 목록은 자주쓰니까 함수로 빼면 좋을거같다
    const dt = 1 / 60;
    this.players.forEach(player => {
      const dx = player.dirX * player.speed * dt;
      const dy = player.dirY * player.speed * dt;
      player.moveDelta(dx, dy);
    });
  }

  networkLoop() {
    this.players.forEach(player => {
      const packet: P.PlayerStatusPacket = {
        players: this.players.map(p => ({
          id: p.ID,
          pos_x: p.posX,
          pos_y: p.posY,
          dir_x: p.dirX,
          dir_y: p.dirY,
          speed: p.speed,
        })),
      };
      player.client.emit(E.PLAYER_STATUS, packet);
    });
  }
}

const DefaultRoomID = 'default';

export class RoomManager {
  rooms: Map<string, Room>;

  constructor() {
    this.rooms = new Map<string, Room>();
    this.rooms.set(DefaultRoomID, this.createRoom(DefaultRoomID));
  }

  private createRoom(roomID: string): Room {
    const room = new Room(roomID);
    room.gameLoopHandler = setInterval(room.gameLoop.bind(room), 1000 / 60);
    room.networkLoopHandler = setInterval(room.networkLoop.bind(room), 100);
    return room;
  }

  private removeRoom(roomID: string): boolean {
    const room = this.rooms.get(roomID);
    if (!room) { return false; }

    clearInterval(room.gameLoopHandler);
    clearInterval(room.networkLoopHandler);

    room.gameLoopHandler = INVALID_LOOP_HANDLER;
    room.networkLoopHandler = INVALID_LOOP_HANDLER;

    this.rooms.delete(roomID);
    return true;
  }

  getRoom(roomID: string): Room {
    const found = this.rooms.get(roomID);
    if (found) {
      return found;

    } else {
      const room = this.createRoom(roomID);
      this.rooms.set(roomID, room);
      return room;
    }
  }

  getDefaultRoom(): Room {
    return this.getRoom(DefaultRoomID);
  }
}
