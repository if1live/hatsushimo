using System.Collections.Generic;
using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;
using Hatsushimo.Types;

namespace Hatsushimo.Packets
{
    public enum ActorType
    {
        None = 0,
        Player,
        Food,
    }

    public enum ReplicationAction
    {
        None = 0,
        Create,
        Update,
        Remove,
    }

    public struct PlayerInitial : ISerialize
    {
        public int ID;
        public string Nickname;
        public Vec2 Pos;
        public Vec2 TargetPos;
        public float Speed;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadString(out Nickname);
            r.ReadValue(ref Pos);
            r.ReadValue(ref TargetPos);
            r.Read(out Speed);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteString(Nickname);
            w.WriteValue(Pos);
            w.WriteValue(TargetPos);
            w.Write(Speed);
        }
    }

    public struct FoodInitial : ISerialize
    {
        public int ID;
        public Vec2 Pos;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadValue(ref Pos);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteValue(Pos);
        }
    }

    public struct ReplicationAllPacket : IPacket
    {
        public PlayerInitial[] Players;
        public FoodInitial[] Foods;

        public short Type => (short)PacketType.ReplicationAll;

        public void Deserialize(BinaryReader r)
        {
            r.ReadValues(out Players);
            r.ReadValues(out Foods);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteValues(Players);
            w.WriteValues(Foods);
        }
    }

    public struct ReplicationActionPacket : IPacket
    {
        public ReplicationAction Action;
        public int ID;
        public ActorType ActorType;
        public Vec2 Pos;
        public Vec2 TargetPos;
        public float Speed;
        public string Extra;

        public short Type => (short)PacketType.ReplicationAction;

        public void Deserialize(BinaryReader r)
        {
            short actionVal = 0;
            r.Read(out actionVal);
            Action = (ReplicationAction)actionVal;

            r.Read(out ID);

            short actorTypeVal = 0;
            r.Read(out actorTypeVal);
            ActorType = (ActorType)actorTypeVal;

            r.ReadValue(ref Pos);
            r.ReadValue(ref TargetPos);
            r.Read(out Speed);
            r.ReadString(out Extra);
        }

        public void Serialize(BinaryWriter w)
        {
            // TODO string=null인 경우 터진다
            // 이를 serialize 차원에서 우회하고싶다
            w.Write((short)Action);
            w.Write(ID);
            w.Write((short)ActorType);
            w.WriteValue(Pos);
            w.WriteValue(TargetPos);
            w.Write(Speed);
            w.WriteString(Extra);
        }

        public static bool operator ==(ReplicationActionPacket a, ReplicationActionPacket b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (ReferenceEquals(a, null)) { return false; }
            if (ReferenceEquals(b, null)) { return false; }

            return (a.Action == b.Action)
                && (a.ID == b.ID)
                && (a.ActorType == b.ActorType)
                && (a.Pos == b.Pos)
                && (a.TargetPos == b.TargetPos)
                && (a.Speed == b.Speed)
                && (a.Extra == b.Extra);
        }

        public static bool operator !=(ReplicationActionPacket a, ReplicationActionPacket b)
        {
            return !(a == b);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((ReplicationActionPacket)obj);
        }

        public bool Equals(ReplicationActionPacket other)
        {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return Action.Equals(other.Action)
                && ID.Equals(other.ID)
                && ActorType.Equals(other.ActorType)
                && Pos.Equals(other.Pos)
                && TargetPos.Equals(other.TargetPos)
                && Speed.Equals(other.Speed)
                && Extra.Equals(other.Extra);
        }

        public override int GetHashCode()
        {
            int hash = Action.GetHashCode();
            hash = hash ^ ID.GetHashCode();
            hash = hash ^ ActorType.GetHashCode();
            hash = hash ^ Pos.GetHashCode();
            hash = hash ^ TargetPos.GetHashCode();
            hash = hash ^ Speed.GetHashCode();
            hash = hash ^ Extra.GetHashCode();
            return hash;
        }
    }


    public struct ReplicationBulkActionPacket : IPacket
    {
        public ReplicationActionPacket[] Actions;

        public short Type => (short)PacketType.ReplicationBulkAction;

        public void Deserialize(BinaryReader r)
        {
            r.ReadValues(out Actions);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteValues(Actions);
        }

        public static bool operator ==(ReplicationBulkActionPacket a, ReplicationBulkActionPacket b)
        {
            if (ReferenceEquals(a, b)) { return true; }
            if (ReferenceEquals(a, null)) { return false; }
            if (ReferenceEquals(b, null)) { return false; }

            if (a.Actions == null && b.Actions == null) { return true; }
            if (a.Actions != null && b.Actions == null) { return false; }
            if (a.Actions == null && b.Actions != null) { return false; }

            if (a.Actions.Length != b.Actions.Length) { return false; }

            for (var i = 0; i < a.Actions.Length; i++)
            {
                var x = a.Actions[i];
                var y = a.Actions[i];
                if (x != y) { return false; }
            }
            return true;
        }

        public static bool operator !=(ReplicationBulkActionPacket a, ReplicationBulkActionPacket b)
        {
            return !(a == b);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((ReplicationBulkActionPacket)obj);
        }

        public bool Equals(ReplicationBulkActionPacket other)
        {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }

            if (Actions == null && other.Actions == null) { return true; }
            if (Actions != null && other.Actions == null) { return false; }
            if (Actions == null && other.Actions != null) { return false; }

            if (Actions.Length != other.Actions.Length) { return false; }

            for (var i = 0; i < Actions.Length; i++)
            {
                var x = Actions[i];
                var y = other.Actions[i];
                if (x != y) { return false; }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            if (Actions != null)
            {
                for (var i = 0; i < Actions.Length; i++)
                {
                    hash = hash ^ Actions[i].GetHashCode();
                }
            }

            return hash;
        }
    }
}
