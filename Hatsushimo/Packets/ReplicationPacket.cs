using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Hatsushimo.Extensions;
using Hatsushimo.NetChan;

namespace Hatsushimo.Packets
{
    public enum ActorType
    {
        None = 0,
        Player,
        Food,
        Projectile,
    }

    public enum ReplicationAction
    {
        None = 0,
        Create,
        Update,
        Remove,
    }

    public struct PlayerStatus : ISerialize
    {
        public int ID;
        public string Nickname;
        public Vector2 Pos;
        public Vector2 TargetPos;
        public float Speed;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadString(out Nickname);
            r.ReadVector(ref Pos);
            r.ReadVector(ref TargetPos);
            r.Read(out Speed);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteString(Nickname);
            w.WriteVector(Pos);
            w.WriteVector(TargetPos);
            w.Write(Speed);
        }
    }

    public struct FoodStatus : ISerialize
    {
        public int ID;
        public Vector2 Pos;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadVector(ref Pos);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteVector(Pos);
        }
    }

    // 게임에 진입하는 순간에 투사체 정보가 없다면
    // 뜬금없이 죽는 타인이 보일것이다
    // 시작점과 끝점과 날아가는 시간을 알면
    // 속도를 계산할수 있다
    public struct ProjectileStatus : ISerialize
    {
        public int ID;
        public Vector2 Position;
        public Vector2 FinalPosition;
        public short MoveTimeMillis;
        public short LifeTimeMillis;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
            r.ReadVector(ref Position);
            r.ReadVector(ref FinalPosition);
            r.Read(out MoveTimeMillis);
            r.Read(out LifeTimeMillis);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
            w.WriteVector(Position);
            w.WriteVector(FinalPosition);
            w.Write(MoveTimeMillis);
            w.Write(LifeTimeMillis);
        }

        public Vector2 Direction
        {
            get
            {
                var diff = FinalPosition - Position;
                return Vector2.Normalize(diff);
            }
        }
    }

    public struct ReplicationAllPacket : IPacket
    {
        public PlayerStatus[] Players;
        public FoodStatus[] Foods;
        public ProjectileStatus[] Projectiles;

        public short Type => (short)PacketType.ReplicationAll;

        public void Deserialize(BinaryReader r)
        {
            r.ReadValues(out Players);
            r.ReadValues(out Foods);
            r.ReadValues(out Projectiles);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteValues(Players);
            w.WriteValues(Foods);
            w.WriteValues(Projectiles);
        }
    }

    // 객체의 ID만 알아도 지울수 있다
    public struct ReplicationBulkRemovePacket : IPacket
    {
        public int[] IDList;

        public short Type => (short)PacketType.ReplicationBulkRemove;

        public void Deserialize(BinaryReader r)
        {
            r.ReadArray(out IDList);
        }

        public void Serialize(BinaryWriter w)
        {
            w.WriteArray(IDList);
        }
    }

    // remove bulk로 묶어서 처리하는 것을 구현하기 전까지는 낱개 삭제를 쓰자
    public struct ReplicationRemovePacket : IPacket
    {
        public int ID;

        public short Type => (short)PacketType.ReplicationRemove;

        public void Deserialize(BinaryReader r)
        {
            r.Read(out ID);
        }

        public void Serialize(BinaryWriter w)
        {
            w.Write(ID);
        }
    }

    public struct ReplicationCreateFoodPacket : IPacket
    {
        public FoodStatus status;
        public short Type => (short)PacketType.ReplicationCreateFood;
        public void Deserialize(BinaryReader r) { r.ReadValue(ref status); }
        public void Serialize(BinaryWriter w) { w.WriteValue(status); }
    }

    public struct ReplicationCreatePlayerPacket : IPacket
    {
        public PlayerStatus status;

        public short Type => (short)PacketType.ReplicationCreatePlayer;
        public void Deserialize(BinaryReader r) { r.ReadValue(ref status); }
        public void Serialize(BinaryWriter w) { w.WriteValue(status); }
    }

    public struct ReplicationCreateProjectilePacket : IPacket
    {
        public ProjectileStatus status;

        public short Type => (short)PacketType.ReplicationCreateProjectile;
        public void Deserialize(BinaryReader r) { r.ReadValue(ref status); }
        public void Serialize(BinaryWriter w) { w.WriteValue(status); }
    }
}