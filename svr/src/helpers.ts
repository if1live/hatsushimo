export const normalizeVec2 = (x: number, y: number): [number, number] => {
  const len = getLengthVec2(x, y);
  if (len === 0) {
    return [0, 0];

  } else {
    return [x / len, y / len];
  }
}

export const getLengthVec2 = (x: number, y: number): number => {
  const len = Math.sqrt(x * x + y * y);
  return len;
}
