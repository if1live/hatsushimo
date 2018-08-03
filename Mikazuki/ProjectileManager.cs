using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using NLog;

namespace Mikazuki
{
    public class ProjectileManager
    {
        static readonly Logger log = LogManager.GetLogger("ProjectileManager");
        readonly Grid<Projectile> grid = new Grid<Projectile>(Config.CellSize, Config.RoomWidth / 2, Config.RoomHeight / 2);
        readonly IDPool projectileIDPool = IDPool.MakeProjectileID();
        readonly IBroadcaster broadcaster;
        readonly ProjectilePacketFactory packetFactory = new ProjectilePacketFactory();
        List<Projectile> projectiles = new List<Projectile>();

        public ProjectileManager(IBroadcaster broadcaster)
        {
            this.broadcaster = broadcaster;
        }

        public void BroadcastProjectileCreate(Projectile projectile)
        {
            var packet = packetFactory.Create(projectile);
            broadcaster.Broadcast(projectile.Position, packet);
        }

        public Projectile Create(Player player)
        {
            log.Info($"projectile create");
            var pos = player.Position;
            var dir = player.Direction;

            var id = projectileIDPool.Next();
            var ownerID = player.ID;

            var projectile = new Projectile(id, ownerID, pos, dir);
            projectiles.Add(projectile);
            RefreshGrid();

            return projectile;
        }

        public void RefreshGrid()
        {
            grid.Clear();
            foreach (var p in projectiles)
            {
                grid.Add(p.Position, p);
            }
        }


        public void Update(float dt)
        {
            foreach(var p in projectiles)
            {
                p.Update(dt);
            }
            RefreshGrid();

            var deadList = projectiles
            .Select((p, idx) => new { projectile = p, idx = idx })
            .Where(pair => pair.projectile.Alive == false)
            .OrderByDescending(pair => pair.idx);

            foreach (var pair in deadList)
            {
                projectiles.RemoveAt(pair.idx);
                projectileIDPool.Release(pair.projectile.ID);
            }
            if (deadList.Count() > 0)
            {
                RefreshGrid();
            }
        }

        public IEnumerable<ProjectileStatus> StatusList
        {
            get
            {
                return projectiles.Select(p => new ProjectileStatus()
                {
                    ID = p.ID,
                    Position = p.Position,
                    FinalPosition = p.FinalPosition,
                    LifeTimeMillis = (short)(p.LifeTime * 1000),
                    MoveTimeMillis = (short)(p.MoveTime * 1000),
                });
            }
        }

        public IEnumerable<Projectile> GetProjectiles(Vector2 pos, float range)
        {
            var cellcoord = grid.GetCellCoord(pos);
            var cellcoords = grid.FilterCellCoords(
                CellCoord.Offsets.Select(offset => offset + cellcoord)
            );
            var cells = cellcoords.Select(coord => grid.GetCell(coord));
            var founds = cells.SelectMany(cell => cell.ToArray())
                .Where(el => VectorHelper.IsInRange(el.Position, pos, range));
            return founds;
        }
    }

    class ProjectilePacketFactory
    {
        public ReplicationCreateProjectilePacket Create(Projectile p)
        {
            var status = new ProjectileStatus()
            {
                ID = p.ID,
                Position = p.Position,
                FinalPosition = p.FinalPosition,
                LifeTimeMillis = (short)(p.LifeTime * 1000),
                MoveTimeMillis = (short)(p.MoveTime * 1000),
            };
            return new ReplicationCreateProjectilePacket(status);
        }

    }
}
