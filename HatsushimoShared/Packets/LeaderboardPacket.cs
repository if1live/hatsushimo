using System.Collections.Generic;
using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public interface IRankable
    {
        int ID { get; }
        int Score { get; }
    }

    public struct Rank : ISerialize
    {
        public int ID;
        public int Score;
        public int Ranking;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.Read(out Score);
            r.Read(out Ranking);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.Write(Score);
            w.Write(Ranking);
        }
    }

    public struct LeaderboardPacket : IPacket
    {
        public int Players;
        public Rank[] Top;

        public short Type => (short)PacketType.Leaderboard;

        public IPacket CreateBlank()
        {
            return new LeaderboardPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(out Players);

            short len = 0;
            r.Read(out len);

            List<Rank> tops = new List<Rank>();
            for (var i = 0; i < len; i++)
            {
                var rank = new Rank();
                rank.Deserialize(r);
                tops.Add(rank);
            }
            Top = tops.ToArray();
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Players);
            w.Write((short)Top.Length);
            for (var i = 0; i < Top.Length; i++)
            {
                Top[i].Serialize(w);
            }
        }
    }
}
