import { Leaderboard, Rankable } from '../src/Leaderboard';

const makePlayer = (id: number, score: number): Rankable => {
  return {
    ID: id,
    score: score,
  };
};

describe('Leaderboard', () => {
  const p1 = makePlayer(1, 10);
  const p2 = makePlayer(2, 20);
  const p3 = makePlayer(3, 30);
  const players = [p1, p2, p3];

  it('getTopRanks - sorted', () => {
    const board = new Leaderboard(players, 2);
    const tops = board.getTopRanks(2);

    expect(tops[0].id).toBe(p3.ID);
    expect(tops[1].id).toBe(p2.ID);
  });

  it('getTopRanks - 1st rank is 1', () => {
    const board = new Leaderboard(players, 2);
    const tops = board.getTopRanks(2);

    expect(tops[0].rank).toBe(1);
    expect(tops[1].rank).toBe(2);
  });

  it('isLeaderboardEqual - equal', () => {
    const boardA = new Leaderboard(players, 2);
    const boardB = new Leaderboard(players, 2);
    expect(boardA.isLeaderboardEqual(boardB)).toBeTruthy();
  });

  it('isLeaderboardEqual - different player size', () => {
    const boardA = new Leaderboard([p1, p2, p3], 2);
    const boardB = new Leaderboard([p2, p3], 2);

    expect(boardA.isLeaderboardEqual(boardB)).toBeFalsy();
  });

  it('isLeaerboardEqual - top ranks equal', () => {
    const top1 = makePlayer(1, 100);
    const top2 = makePlayer(2, 200);
    const remain1 = makePlayer(3, 1);
    const remain2 = makePlayer(4, 2);

    const boardA = new Leaderboard([top1, top2, remain1], 2);
    const boardB = new Leaderboard([top1, top2, remain2], 2);

    expect(boardA.isLeaderboardEqual(boardB)).toBeTruthy();
  })
});
