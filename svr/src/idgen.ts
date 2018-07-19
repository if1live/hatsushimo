import * as C from './config';

export function* IDGenerator(firstID: number, range: number) {
  let nextID = firstID;

  while (true) {
    const retval = nextID;
    yield retval;

    nextID += 1;
    if (nextID >= firstID + range) {
      nextID = firstID;
    }
  }
}

export function makePlayerID() {
  return IDGenerator(C.ID_PLAYER_FIRST, C.ID_FOOD_RANGE);
}

export function makeFoodID() {
  return IDGenerator(C.ID_FOOD_FIRST, C.ID_FOOD_RANGE);
}
