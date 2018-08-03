using System;
using System.Collections.Generic;
using System.Linq;
using Hatsushimo;
using Hatsushimo.Packets;
using NLog;

namespace Mikazuki
{
    public class PlayerManager
    {
        static readonly Logger log = LogManager.GetLogger("PlayerLayer");
        readonly Grid<Player> grid = new Grid<Player>(Config.CellSize, Config.RoomWidth / 2, Config.RoomHeight / 2);
        public Grid<Player> Grid { get { return grid; } }

        readonly Broadcaster broadcaster;
        readonly PlayerPacketFactory packetFactory = new PlayerPacketFactory();

        List<Player> players = new List<Player>();

        public PlayerManager(Broadcaster broadcaster)
        {
            this.broadcaster = broadcaster;
        }

        void RefreshGrid()
        {
            grid.Clear();
            foreach (var player in players)
            {
                grid.Add(player.Position, player);
            }
        }

        public void Update(float dt)
        {
            foreach (var player in players)
            {
                player.UpdateMove(dt);
            }
            RefreshGrid();
        }

        public IEnumerable<PlayerStatus> StatusList
        {
            get
            {
                return players.Select(p => new PlayerStatus()
                {
                    ID = p.ID,
                    Nickname = p.Session.Nickname,
                    Pos = p.Position,
                    TargetPos = p.TargetPosition,
                    Speed = p.Speed,
                });
            }
        }

        public void Add(Player player)
        {
            players.Add(player);
            RefreshGrid();

            // 기존 유저들에게 새로 생성된 플레이어 정보를 알려주기
            var packet = packetFactory.Create(player);
            broadcaster.BroadcastLazy(player.Position, packet);
        }

        public bool Remove(Player player)
        {
            var retval = players.Remove(player);
            if (retval)
            {
                RefreshGrid();

                // 방을 나갔다는것을 다른 유저도 알아야한다
                var packet = new ReplicationRemovePacket(player.ID);
                broadcaster.BroadcastLazy(player.Position, packet);
            }
            return retval;
        }

        public int Count { get { return players.Count; } }

        public int GetPlayers(ref List<Player> dst)
        {
            dst.Clear();
            dst.AddRange(players);
            return players.Count;
        }
    }

    class PlayerPacketFactory
    {
        public ReplicationCreatePlayerPacket Create(Player player)
        {
            var status = new PlayerStatus()
            {
                ID = player.ID,
                Pos = player.Position,
                TargetPos = player.TargetPosition,
                Speed = player.Speed,
                Nickname = player.Session.Nickname,
            };
            return new ReplicationCreatePlayerPacket(status);
        }
    }
}
