import { Player } from './player';
import { IDGenerator } from './idgen';
import * as P from './packets';

export class Room {
  players: Player[];
  ID: string;

  foodIDGen: Generator;

  // handler
  gameLoopHandler: number;
  networkLoopHandler: number;

  constructor(id: string) {
    this.ID = id;
    this.players = [];

    // 방에서만 사용하는 객체는 id를 따로 발급
    this.foodIDGen = IDGenerator(100001, 100000);

    this.gameLoopHandler = -1;
    this.networkLoopHandler = -1;
  }

  JoinPlayer(player: Player): boolean {
    if (player.roomID !== null) { return false; }

    this.players.push(player);
    player.roomID = this.ID;
    console.log(`join room - room=${this.ID} player=${player.ID} room_size=${this.players.length}`);
    return true;
  }

  LeavePlayer(player: Player): boolean {
    if (player.roomID === null) { return false; }

    const found = this.players.findIndex(x => x == player);
    if (found < 0) { return false; }

    this.players.splice(found, 1);
    player.roomID = null;
    console.log(`leave room - room=${this.ID} player=${player.ID} room_size=${this.players.length}`);
    return true;
  }

  gameLoop() {
    // TODO ready player 목록은 자주쓰니까 함수로 빼면 좋을거같다
    const dt = 1 / 60;
    const players = this.players.filter(player => player.ready);
    players.forEach(player => {
      const dx = player.dirX * player.speed * dt;
      const dy = player.dirY * player.speed * dt;
      player.posX += dx;
      player.posY += dy;
    });
  }

  networkLoop() {
    // 준비가 되지 않은 플레이어 정보는 필요없다
    const players = this.players.filter(player => player.ready);
    players.forEach(player => {
      const packet: P.PlayerStatusPacket = {
        players: players.map(p => ({
          id: p.ID,
          pos_x: p.posX,
          pos_y: p.posY,
          dir_x: p.dirX,
          dir_y: p.dirY,
          speed: p.speed,
        })),
      };
      player.client.emit(`player-positions`, packet);
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

    room.gameLoopHandler = -1;
    room.networkLoopHandler = -1;

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
