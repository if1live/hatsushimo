using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hatsushimo.NetChan;

namespace Mikazuki
{
    public interface IBroadcaster
    {
        void Broadcast<T>(Vector2 pos, T packet) where T : IPacket;
        void BroadcastLazy<T>(Vector2 pos, T packet) where T : IPacket;

        void BroadcastAll<T>(T packet) where T : IPacket;
        void BroadcastAllLazy<T>(T packet) where T : IPacket;
    }

    public class Broadcaster : IBroadcaster
    {
        public Grid<Player> Grid { get; set; }

        // 특정 좌표에서 발생한 이벤트를 받을 필요가 있는 유저 목록 얻기
        IEnumerable<Player> FindPlayers(Vector2 pos)
        {
            if (Grid == null) { return Enumerable.Empty<Player>(); }

            var cellcoord = Grid.GetCellCoord(pos);
            var cellcoords = Grid.FilterCellCoords(
                CellCoord.Offsets.Select(offset => offset + cellcoord)
            );
            var cells = cellcoords.Select(coord => Grid.GetCell(coord));
            var players = cells.SelectMany(cell => cell.ToArray());
            return players;
        }

        IEnumerable<Player> FindAllPlayers()
        {
            if (Grid == null) { return Enumerable.Empty<Player>(); }
            return Grid.GetCellEnumerable().SelectMany(cell => cell.ToArray());
        }

        // pos근처의 유저한테만 패킷을 전송하고싶다
        public void Broadcast<T>(Vector2 pos, T packet) where T : IPacket
        {
            var players = FindPlayers(pos);
            foreach (var p in players)
            {
                p.Session.SendImmediate(packet);
            }
        }

        public void BroadcastLazy<T>(Vector2 pos, T packet) where T : IPacket
        {
            var players = FindPlayers(pos);
            foreach (var p in players)
            {
                p.Session.SendLazy(packet);
            }
        }

        public void BroadcastAll<T>(T packet) where T : IPacket
        {
            var players = FindAllPlayers();
            foreach (var p in players) { p.Session.SendImmediate(packet); }
        }

        public void BroadcastAllLazy<T>(T packet) where T : IPacket
        {
            var players = FindAllPlayers();
            foreach (var p in players) { p.Session.SendLazy(packet); }
        }
    }

    // 좌표 검색 없이 모두에게 보낸다
    class ObserverBroadcaster : IBroadcaster
    {
        readonly List<Observer> observers = new List<Observer>();

        public void Add(Observer p) { observers.Add(p); }
        public void Remove(Observer p) { observers.Remove(p); }

        public void Broadcast<T>(Vector2 pos, T packet) where T : IPacket
        {
            foreach (var p in observers) { p.Session.SendImmediate(packet); }
        }

        public void BroadcastAll<T>(T packet) where T : IPacket
        {
            foreach (var p in observers) { p.Session.SendImmediate(packet); }
        }

        public void BroadcastAllLazy<T>(T packet) where T : IPacket
        {
            foreach (var p in observers) { p.Session.SendLazy(packet); }
        }

        public void BroadcastLazy<T>(Vector2 pos, T packet) where T : IPacket
        {
            foreach (var p in observers) { p.Session.SendLazy(packet); }
        }
    }

    public class BroadcasterGroup : IBroadcaster
    {
        readonly List<IBroadcaster> casters;

        public BroadcasterGroup(IEnumerable<IBroadcaster> broadcasters)
        {
            this.casters = new List<IBroadcaster>(broadcasters);
        }

        public void Broadcast<T>(Vector2 pos, T packet) where T : IPacket
        {
            foreach (var c in casters) { c.Broadcast(pos, packet); }
        }

        public void BroadcastAll<T>(T packet) where T : IPacket
        {
            foreach (var c in casters) { c.BroadcastAll(packet); }
        }

        public void BroadcastAllLazy<T>(T packet) where T : IPacket
        {
            foreach (var c in casters) { c.BroadcastAllLazy(packet); }
        }

        public void BroadcastLazy<T>(Vector2 pos, T packet) where T : IPacket
        {
            foreach (var c in casters) { c.BroadcastLazy(pos, packet); }
        }
    }
}
