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

        public static bool operator ==(Rank a, Rank b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (ReferenceEquals(a, null)) { return false; }
            if (ReferenceEquals(b, null)) { return false; }

            return (a.ID == b.ID)
                && (a.Score == b.Score)
                && (a.Ranking == b.Ranking);
        }

        public static bool operator !=(Rank a, Rank b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((Rank)obj);
        }

        public bool Equals(Rank other)
        {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return ID.Equals(other.ID)
                && Score.Equals(other.Score)
                && Ranking.Equals(other.Ranking);
        }

        public override int GetHashCode()
        {
            int hash = ID.GetHashCode();
            hash = hash ^ Score.GetHashCode();
            hash = hash ^ Ranking.GetHashCode();
            return hash;
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
            r.ReadValues(out Top);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Players);
            w.WriteValues(Top);
        }
    }
}
