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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((Rank)obj);
        }

        public bool Equals(Rank other)
        {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
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
        public int Players { get { return _players; } }
        int _players;

        public Rank[] Top { get { return _top; } }
        Rank[] _top;

        public LeaderboardPacket(int players, Rank[] top)
        {
            _players = players;
            _top = top;
        }

        public short Type => (short)PacketType.Leaderboard;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out _players);
            r.ReadValues(out _top);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Players);
            w.WriteValues(Top);
        }

        public static bool operator ==(LeaderboardPacket a, LeaderboardPacket b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (ReferenceEquals(a, null)) { return false; }
            if (ReferenceEquals(b, null)) { return false; }

            if (a.Players != b.Players) { return false; }

            if (a.Top == null && b.Top == null) { return true; }
            if (a.Top != null && b.Top == null) { return false; }
            if (a.Top == null && b.Top != null) { return false; }

            if (a.Top.Length != b.Top.Length) { return false; }
            for (var i = 0; i < a.Top.Length; i++)
            {
                var x = a.Top[i];
                var y = b.Top[i];
                if (x != y)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(LeaderboardPacket a, LeaderboardPacket b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((LeaderboardPacket)obj);
        }

        public bool Equals(LeaderboardPacket other)
        {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }

            if (Players != other.Players) { return false; }

            if (Top == null && other.Top == null) { return true; }
            if (Top != null && other.Top == null) { return false; }
            if (Top == null && other.Top != null) { return false; }

            if (Top.Length != other.Top.Length) { return false; }
            for (var i = 0; i < Top.Length; i++)
            {
                var x = Top[i];
                var y = other.Top[i];
                if (x != y)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = Players.GetHashCode();
            if (Top != null)
            {
                for (var i = 0; i < Top.Length; i++)
                {
                    hash = hash ^ Top[i].GetHashCode();
                }
            }
            return hash;
        }
    }
}
