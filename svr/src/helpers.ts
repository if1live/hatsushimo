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

export const getLengthSquare2 = (x: number, y: number): number => {
  return x * x + y * y;
}

export const generateRandomPosition = (w: number, h: number): [number, number] => {
  const x = (Math.random() - 0.5) * w;
  const y = (Math.random() - 0.5) * h;
  return [x, y];
}

