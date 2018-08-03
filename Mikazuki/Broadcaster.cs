using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hatsushimo.NetChan;

namespace Mikazuki
{
    public class Broadcaster
    {
        public Grid<Player> Grid { get; set; }

        // 특정 좌표에서 발생한 이벤트를 받을 필요가 있는 유저 목록 얻기
        IEnumerable<Player> FindPlayers(Vector2 pos)
        {
            var cellcoord = Grid.GetCellCoord(pos);
            var cellcoords = Grid.FilterCellCoords(
                CellCoord.Offsets.Select(offset => offset + cellcoord)
            );
            var cells = cellcoords.Select(coord => Grid.GetCell(coord));
            var players = cells.SelectMany(cell => cell.ToArray());
            return players;
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
    }
}
