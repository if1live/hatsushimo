import { Player } from './player';
import { makeFoodID } from './idgen';
import * as P from './packets';
import * as E from './events';
import * as H from './helpers';
import * as C from './config';

const INVALID_LOOP_HANDLER = -1;

class Food {
  ID: number;
  type: string;
  posX: number;
  posY: number;
  score: number;

  constructor(id: number, posX: number, posY: number, score: number) {
    this.ID = id;
    this.type = 'food';
    this.posX = posX;
    this.posY = posY;
    this.score = score;
  }
}

export class Room {
  foods: Food[];
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
    this.foods = [];
    this.players = [];
    this.waitingPlayers = [];

    // 방에서만 사용하는 객체는 id를 따로 발급
    this.foodIDGen = makeFoodID();

    this.gameLoopHandler = INVALID_LOOP_HANDLER;
    this.networkLoopHandler = INVALID_LOOP_HANDLER;

    // 방 만들때 음식 미리 만들기
    for (var i = 0; i < C.FOOD_COUNT; i++) {
      const food = this.makeFood();
      this.foods.push(food);
    }
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

    // 신규 유저에게 유저 전체 목록 알려주기
    const players = this.players.map(p => ({
      id: p.ID,
      nickname: p.nickname,
      pos_x: p.posX,
      pos_y: p.posY,
    }));
    const playerListPacket: P.PlayerListPacket = {
      players: players,
    };
    player.client.emit(E.PLAYER_LIST, playerListPacket);

    // 신규 유저에게 아이템 목록 알려주기
    const items = this.foods.map(f => ({
      id: f.ID,
      type: f.type,
      pos_x: f.posX,
      pos_y: f.posY,
    }));
    const itemListPacket: P.StaticItemListPacket = {
      items: items,
    };
    player.client.emit(E.STATIC_ITEM_LIST, itemListPacket);
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
    const dt = 1 / 60;

    this.players.forEach(player => {
      const dx = player.dirX * player.speed * dt;
      const dy = player.dirY * player.speed * dt;
      player.moveDelta(dx, dy);
    });

    // 음식 생성
    const requiredFoodCount = C.FOOD_COUNT - this.foods.length;
    for (var i = 0; i < requiredFoodCount; i++) {
      const food = this.makeFood();
      this.foods.push(food);
      this.sendFoodCreatePacket(food);
    }

    // 음식을 먹으면 점수를 올리고 음식을 목록에서 삭제
    // TODO quad tree 같은거 쓰면 최적화 가능
    this.players.forEach(player => {
      const gainedFoods = this.foods.map((food, idx) => ({
        food: food,
        index: idx,
      })).filter(pair => {
        const food = pair.food;
        const p1 = [player.posX, player.posY];
        const p2 = [food.posX, food.posY];
        const diffx = p1[0] - p2[0];
        const diffy = p1[1] - p2[1];
        const lenSquare = H.getLengthSquare2(diffx, diffy);
        const ALLOW_DISTANCE = 1;
        return lenSquare < ALLOW_DISTANCE * ALLOW_DISTANCE;
      });

      // 먹은 플레이어는 점수 획득
      gainedFoods.map(pair => pair.food).forEach(food => {
        player.score += food.score;
      });

      // 모든 플레이어에게 삭제 패킷 보내기
      gainedFoods.map(pair => pair.food).forEach(food => {
        this.sendFoodRemovePacket(food);
      });

      // 배열의 뒤에서부터 제거하면 검색으로 찾은 인덱스를 그대로 쓸수있다
      gainedFoods.map(pair => pair.index).sort((a, b) => b - a).forEach(idx => {
        this.foods.splice(idx, 1);
      });
    });
  }

  makeFood(): Food {
    const pos = H.generateRandomPosition(C.ROOM_WIDTH, C.ROOM_HEIGHT);
    const score = 1;
    const id = this.foodIDGen.next().value;
    const food = new Food(id, pos[0], pos[1], score);
    return food;
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

  sendFoodCreatePacket(food: Food) {
    // 모든 유저에게 아이템 생성 패킷 전송
    // TODO broadcast emit
    const packet: P.StaticItemCreatePacket = {
      id: food.ID,
      pos_x: food.posX,
      pos_y: food.posY,
      type: food.type,
    };
    this.players.map(p => p.client).forEach(client => {
      client.emit(E.STATIC_ITEM_CREATE, packet);
    });
  }

  sendFoodRemovePacket(food: Food) {
    this.players.forEach(p => {
      const packet: P.StaticItemRemovePacket = {
        id: food.ID,
        type: food.type,
      };
      const client = p.client;
      client.emit(E.STATIC_ITEM_REMOVE, packet);
      console.log(`sent food remove packet : ${JSON.stringify(packet)}`)
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
