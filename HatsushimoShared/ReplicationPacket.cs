using System.Collections.Generic;
using System.IO;

namespace HatsushimoShared
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
        Delete,
    }

    public struct ReplicationActionPacket : IPacket
    {
        public ReplicationAction Action;
        public int ID;
        public ActorType ActorType;
        public float PosX;
        public float PosY;
        public float DirX;
        public float DirY;
        public float Speed;
        public string Extra;

        public PacketType Type => PacketType.ReplicationAction;

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

            r.Read(out PosX);
            r.Read(out PosY);
            r.Read(out DirX);
            r.Read(out DirY);
            r.Read(out Speed);
            r.Read(out Extra);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write((short)Action);
            w.Write(ID);
            w.Write((short)ActorType);
            w.Write(PosX);
            w.Write(PosY);
            w.Write(DirX);
            w.Write(DirY);
            w.Write(Speed);
            w.Write(Extra);
        }
    }


    public struct ReplicationBulkActionPacket : IPacket
    {
        public ReplicationActionPacket[] Actions;

        public PacketType Type => PacketType.ReplicationBulkAction;

        public IPacket CreateBlank()
        {
            return new ReplicationBulkActionPacket();
        }

        public void Deserialize(BinaryReader r)
        {
            short len = 0;
            r.Read(out len);

            var actions = new List<ReplicationActionPacket>();
            for(var i = 0 ; i < len ; i++) {
                var a = new ReplicationActionPacket();
                a.Deserialize(r);
                actions.Add(a);
            }
            Actions = actions.ToArray();
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write((short)Actions.Length);
            foreach(var a in Actions) {
                a.Serialize(w);
            }
        }
    }
}
