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
