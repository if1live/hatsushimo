import { Player } from './player';

export class Room {
  players: Player[];
  roomID: string;

  constructor(roomID: string) {
    this.roomID = roomID;
    this.players = [];
  }

  JoinPlayer(player: Player): boolean {
    if (player.roomID !== null) { return false; }

    this.players.push(player);
    player.roomID = this.roomID;
    console.log(`join room - room=${this.roomID} player=${player.playerID} room_size=${this.players.length}`);
    return true;
  }

  LeavePlayer(player: Player): boolean {
    if (player.roomID === null) { return false; }

    const found = this.players.findIndex(x => x == player);
    if (found < 0) { return false; }

    this.players.splice(found, 1);
    player.roomID = null;
    console.log(`leave room - room=${this.roomID} player=${player.playerID} room_size=${this.players.length}`);
    return true;
  }

  gameLoop() {
    // TODO ready player 목록은 자주쓰니까 함수로 빼면 좋을거같다
    const players = this.players.filter(player => player.ready);
    players.forEach(player => {
      const dt = 1 / 60;
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
      const ctx = {
        players: players.map(u => u.toJson()),
      }
      player.client.emit(`player-positions`, ctx);
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
    setInterval(room.gameLoop.bind(room), 1000 / 60)
    setInterval(room.networkLoop.bind(room), 100);
    return room;
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
