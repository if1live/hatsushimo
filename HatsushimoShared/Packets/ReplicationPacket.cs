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

    public struct PlayerInitial : ISerialize {
        public int ID;
        public string Nickname;
        public Vec2 Pos;
        public Vec2 Dir;
        public float Speed;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadString(out Nickname);
            r.Read(ref Pos);
            r.Read(ref Dir);
            r.Read(out Speed);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteString(Nickname);
            w.Write(Pos);
            w.Write(Dir);
            w.Write(Speed);
        }
    }

    public struct FoodInitial : ISerialize {
        public int ID;
        public Vec2 Pos;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.Read(ref Pos);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.Write(Pos);
        }
    }

    public struct ReplicationAllPacket : IPacket
    {
        public PlayerInitial[] Players;
        public FoodInitial[] Foods;

        public short Type => (short)PacketType.ReplicationAll;

        public IPacket CreateBlank()
        {
            return new ReplicationAllPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(ref Players);
            r.Read(ref Foods);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Players);
            w.Write(Foods);
        }
    }

    public struct ReplicationActionPacket : IPacket
    {
        public ReplicationAction Action;
        public int ID;
        public ActorType ActorType;
        public Vec2 Pos;
        public Vec2 Dir;
        public float Speed;
        public string Extra;

        public short Type => (short)PacketType.ReplicationAction;

        public IPacket CreateBlank()
        {
            return new ReplicationActionPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            short actionVal = 0;
            r.Read(out actionVal);
            Action = (ReplicationAction)actionVal;

            r.Read(out ID);

            short actorTypeVal = 0;
            r.Read(out actorTypeVal);
            ActorType = (ActorType)actorTypeVal;

            r.Read(ref Pos);
            r.Read(ref Dir);
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
            w.Write(Pos);
            w.Write(Dir);
            w.Write(Speed);
            w.WriteString(Extra);
        }
    }


    public struct ReplicationBulkActionPacket : IPacket
    {
        public ReplicationActionPacket[] Actions;

        public short Type => (short)PacketType.ReplicationBulkAction;

        public IPacket CreateBlank()
        {
            return new ReplicationBulkActionPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            r.Read(ref Actions);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(Actions);
        }
    }
}
