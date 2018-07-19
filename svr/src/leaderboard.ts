import { Player } from "./player";
import * as P from './packets';

const TOP_SIZE = 5;

export class Leaderboard {
  // 유저별 랭킹을 보여주려면 어차피 전체 랭킹을 들고있어야한다
  private all: P.RankElement[];

  constructor(players: Player[]) {
    const sortedPlayers = players.sort((a, b) => b.score - a.score);
    const allRanks = sortedPlayers.map((p, idx): P.RankElement => {
      const rank = idx + 1;
      return {
        id: p.ID,
        score: p.score,
        rank,
      };
    });

    this.all = allRanks;
  }

  getTopRanks(size: number): P.RankElement[] {
    return this.all.slice(0, size);
  }

  getRank(id: number): number {
    const found = this.all.findIndex(rank => rank.id == id);
    const rank = found + 1;
    return rank;
  }

  makeLeaderboardPacket(): P.LeaderboardPacket {
    return {
      players: this.all.length,
      top: this.getTopRanks(TOP_SIZE),
    }
  }

  isLeaderboardEqual(o: Leaderboard): boolean {
    // 접속자 목록이 바뀔때도 갱신
    if (this.all.length != o.all.length) { return false; }

    const thisArr = this.getTopRanks(TOP_SIZE);
    const otherArr = o.getTopRanks(TOP_SIZE);
    if (thisArr.length !== otherArr.length) { return false; }

    for (var i = 0; i < thisArr.length; i++) {
      const a = thisArr[i];
      const b = otherArr[i];
      // TODO 더 빠른 객체 비교?
      if (JSON.stringify(a) !== JSON.stringify(b)) {
        return false;
      }
    }
    return true;
  }
}
