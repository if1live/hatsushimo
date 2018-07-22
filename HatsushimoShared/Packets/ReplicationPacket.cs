using System.Collections.Generic;
using System.IO;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;
using Hatsushimo.Types;

namespace Hatsushimo.Packets
{
    public enum ActorType
    {
        Player,
        Food,
    }

    public enum ReplicationAction
    {
        Create,
        Update,
        Remove,
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
            r.Read(out Extra);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write((short)Action);
            w.Write(ID);
            w.Write((short)ActorType);
            w.Write(Pos);
            w.Write(Dir);
            w.Write(Speed);
            w.Write(Extra);
        }

        public static ReplicationActionPacket MakeRemovePacket(int id)
        {
            return new ReplicationActionPacket()
            {
                Action = ReplicationAction.Remove,
                ID = id,
                ActorType = ActorType.Food,
                Pos = Vec2.Zero,
                Dir = Vec2.Zero,
                Speed = 0,
                Extra = null,
            };
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
            short len = 0;
            r.Read(out len);

            var actions = new List<ReplicationActionPacket>();
            for (var i = 0; i < len; i++)
            {
                var a = new ReplicationActionPacket();
                a.Deserialize(r);
                actions.Add(a);
            }
            Actions = actions.ToArray();
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write((short)Actions.Length);
            foreach (var a in Actions)
            {
                a.Serialize(w);
            }
        }
    }
}
