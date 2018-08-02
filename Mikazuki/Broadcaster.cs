using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hatsushimo.NetChan;

namespace Mikazuki
{
    public class Broadcaster
    {
        readonly Grid<Player> grid;

        public Broadcaster(Grid<Player> grid)
        {
            this.grid = grid;
        }

        // 특정 좌표에서 발생한 이벤트를 받을 필요가 있는 유저 목록 얻기
        List<Player> FindPlayers(Vector2 pos)
        {
            var cellcoord = grid.GetCellCoord(pos);
            var cellcoords = grid.FilterCellCoords(
                CellCoord.Offsets.Select(offset => offset + cellcoord)
            );
            var cells = cellcoords.Select(coord => grid.GetCell(coord));
            var players = cells.SelectMany(cell => cell.ToArray());
            return players.ToList();
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
