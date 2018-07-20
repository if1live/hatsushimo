import * as P from './packets';

export interface Rankable {
  ID: number;
  score: number;
}

export interface Rank {
  id: number;
  score: number;
  rank: number;
}

export class Leaderboard {
  // 유저별 랭킹을 보여주려면 어차피 전체 랭킹을 들고있어야한다
  private all: Rank[];
  private topSize: number;

  constructor(players: Rankable[], topSize: number) {
    const sortedPlayers = players.sort((a, b) => b.score - a.score);
    const allRanks = sortedPlayers.map((p, idx): Rank => {
      const rank = idx + 1;
      return {
        id: p.ID,
        score: p.score,
        rank,
      };
    });

    this.all = allRanks;
    this.topSize = topSize;
  }

  getTopRanks(size: number): Rank[] {
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
      top: this.getTopRanks(this.topSize),
    }
  }

  isLeaderboardEqual(o: Leaderboard): boolean {
    // 접속자 목록이 바뀔때도 갱신
    if (this.all.length !== o.all.length) { return false; }

    const thisArr = this.getTopRanks(this.topSize);
    const otherArr = o.getTopRanks(this.topSize);
    if (thisArr.length !== otherArr.length) { return false; }

    for (var i = 0; i < thisArr.length; i++) {
      const a = thisArr[i];
      const b = otherArr[i];

      if (a.id != b.id) { return false; }
      if (a.rank != b.rank) { return false; }
      if (a.score != b.score) { return false; }
    }
    return true;
  }
}
