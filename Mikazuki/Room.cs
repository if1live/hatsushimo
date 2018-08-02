using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using System.Diagnostics;
using NLog;
using System.Numerics;
using Hatsushimo.NetChan;

namespace Mikazuki
{


    public class Room
    {
        readonly Random rand = new Random();
        public string ID { get; private set; }

        static readonly Logger log = LogManager.GetLogger("Room");

        // 기본적으로 게임 로직은 싱글 쓰레드로 돌아간다
        // 주기적으로 게임 정보를 클라에 보내주는데 이것은 비동기로 돌아간다
        // 외부에서 정보를 직접 갖다쓰는 곳(주로 플레이어 리스트)은 락을 건다
        Object lockobj = new Object();

        List<Player> players = new List<Player>();
        // 로직에 직접 참가하지 않는 유저목록
        // 방에는 참가했지만 로딩이 끝나지 않는 경우를 처리하는게 목적
        List<Player> waitingPlayers = new List<Player>();
        List<Projectile> projectiles = new List<Projectile>();

        // 옵져버. 디버깅용
        // 게임 로직에는 참가하지 않고 모든 패킷을 받아본다
        List<Player> observers = new List<Player>();

        // TODO 죽은 플레이어 목록
        // 죽은 유저를 즉시 방에서 쫒아내는것보다는 잠시라도 방에 남아있다가 쫒아내고 싶다
        // 방에 남아있으면 킬캠같은걸 구현 가능하다
        // 또는 제자리에서 부활하거나

        readonly IDPool projectileIDPool = IDPool.MakeProjectileID();

        readonly FoodLayer foodLayer;

        Broadcaster broadcaster;

        public Room(string id)
        {
            this.ID = id;

            broadcaster = new Broadcaster(playerGrid);
            foodLayer = new FoodLayer(broadcaster);
        }

        public bool SpawnPlayer(Player player)
        {
            var found = waitingPlayers.FindIndex(p => p == player);
            if (found < 0) { return false; }

            // 대기자 목록에서 유저를 삭제
            waitingPlayers.RemoveAt(found);

            // 유저를 로직에 합류시킴
            // 다른 비동기 작업에서 유저 목록을 사용할수도 있다
            // 락을 걸어서 문제가 생기지 않도록 하자
            List<Player> prevPlayers = null;
            lock (lockobj)
            {
                prevPlayers = players.ToList();
                players.Add(player);
            }

            // 새로운 유저가 생겼으면 좌표 정보를 갱신
            RefreshPlayerGrid();

            // 신규 유저에게 월드 정보 알려주기
            player.Session.SendImmediate(GenerateReplicaitonAllPacket());

            // 접속한 유저에게 완료 신호 보냄
            // 게임 로직을 돌릴수 있다는 신호임
            player.Session.SendImmediate(new PlayerReadyPacket());

            // 기존 유저들에게 새로 생성된 플레이어 정보를 알려주기
            var spawnPacket = player.Packets.Create();
            prevPlayers.ForEach(p =>
            {
                p.Session.SendLazy(spawnPacket);
            });

            log.Info($"ready room: id={player.ID} room={ID} size={players.Count}");
            return true;
        }

        public void Join(Player newPlayer)
        {
            var pos = GenerateRandomPosition();
            newPlayer.SetPosition(pos);

            waitingPlayers.Add(newPlayer);

            log.Info($"room join: id={newPlayer.ID} room={ID} size={players.Count}");
        }

        public void Leave(Player player)
        {
            var found = players.FindIndex((x) => x.ID == player.ID);
            if (found > -1)
            {
                lock (lockobj)
                {
                    players.RemoveAt(found);
                }

                // 기존 유저가 나가면 격자 갱신
                RefreshPlayerGrid();
            }

            // 로딩 끝나기전에 나가는 경우 처리
            var waitingFound = waitingPlayers.FindIndex((x) => x.ID == player.ID);
            if (waitingFound > -1)
            {
                waitingPlayers.RemoveAt(waitingFound);
            }

            // 방을 나갔다는것을 다른 유저도 알아야한다
            var removePacket = player.GenerateRemovePacket();
            this.players.Where(p => p != player).ToList().ForEach(p =>
            {
                p.Session.SendLazy(removePacket);
            });

            log.Info($"leave room: id={player.ID} room={ID} size={players.Count}");
        }

        public Vector2 GenerateRandomPosition()
        {
            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            var x = (float)(rand.NextDouble() - 0.5) * w;
            var y = (float)(rand.NextDouble() - 0.5) * h;
            return new Vector2(x, y);
        }



        public void RegisterProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        public Projectile MakeProjectile(Player player)
        {
            log.Info($"projectile creat");
            var pos = player.Position;
            var dir = player.Direction;

            var id = projectileIDPool.Next();
            var ownerID = player.ID;

            var projectile = new Projectile(id, ownerID, pos, dir);
            return projectile;
        }


        ReplicationAllPacket GenerateReplicaitonAllPacket()
        {
            var players = this.players.Select(p => new PlayerStatus()
            {
                ID = p.ID,
                Nickname = p.Session.Nickname,
                Pos = p.Position,
                TargetPos = p.TargetPosition,
                Speed = p.Speed,
            });

            var projectiles = this.projectiles.Select(p => new ProjectileStatus()
            {
                ID = p.ID,
                Position = p.Position,
                FinalPosition = p.FinalPosition,
                LifeTimeMillis = (short)(p.LifeTime * 1000),
                MoveTimeMillis = (short)(p.MoveTime * 1000),
            });

            return new ReplicationAllPacket(
                players.ToArray(),
                foodLayer.StatusList.ToArray(),
                projectiles.ToArray()
            );
        }

        WorldJoinResultPacket GenerateRoomJoinPacket(Player player)
        {
            return new WorldJoinResultPacket(0, player.ID);
        }

        public void SendProjectileCreatePacket(Projectile projectile)
        {
            var packet = projectile.GenerateCreatePacket();
            players.ForEach(p =>
            {
                var session = p.Session;
                session.SendImmediate(packet);
            });
        }



        void PlayerUpdateLoop(float dt)
        {
            players.ForEach(player => player.UpdateMove(dt));
            RefreshPlayerGrid();
        }

        void ProjectileUpdateLoop(float dt)
        {
            projectiles.ForEach(p => p.Update(dt));

            var deadList = projectiles
            .Select((p, idx) => new { projectile = p, idx = idx })
            .Where(pair => pair.projectile.Alive == false)
            .OrderByDescending(pair => pair.idx).ToList();

            foreach (var pair in deadList)
            {
                projectiles.RemoveAt(pair.idx);
                projectileIDPool.Release(pair.projectile.ID);
            }
        }


        void CheckFoodLoop()
        {
            // 음식을 먹으면 점수를 올리고 음식을 목록에서 삭제
            // TODO quad tree 같은거 쓰면 최적화 가능
            players.ForEach(player =>
            {
                var ALLOW_DISTANCE = 1;
                var gainedFoods = foodLayer.GetFoods(player.Position, ALLOW_DISTANCE);

                // 먹은 플레이어는 점수 획득
                gainedFoods.Select(food =>
                {
                    player.GainFoodScore(Config.FoodScore);
                    return food;
                });

                var ids = gainedFoods.Select(food => food.ID);
                foodLayer.Remove(ids);
            });
        }

        void CheckKillLoop()
        {
            projectiles.ForEach(projectile =>
            {
                var candidates = this.players.Where(player => player.ID != projectile.OwnerID);

                // TODO 죽창 충돌처리 개선하기
                // 일단은 점-점으로 계산
                var hitPlayers = candidates.Select((players, idx) => new { player = players, idx = idx })
                .Where(pair =>
                {
                    var ALLOW_DISTANCE = 1;
                    var p1 = projectile.Position;
                    var p2 = pair.player.Position;
                    return VectorHelper.IsInRange(p1, p2, ALLOW_DISTANCE);
                }).ToList();

                if (hitPlayers.Count > 0)
                {
                    var owner = this.players.Find(p => p.ID == projectile.OwnerID);
                    if (owner != null)
                    {
                        // 창을 던진후 유저가 죽었을 가능성이 있다
                        owner.GainKillScore(hitPlayers.Count);
                    }

                    // TODO 유저가 죽었다는것과 유저가 나갔다는것을 구분해야한다
                    // 죽은 유저를 방에서 즉시 제거하는게 최선인가?

                    // 유저가 죽었다는걸 다른 유저에게 알려준다
                    // TODO 알려주는 범위 통제하면 대역폭을 아낄수 있다
                    // TODO 객체 삭제 패킷과 유저 죽음 패킷은 분리하는게 가능하다
                    var deadIds = hitPlayers.Select(p => p.player.ID);
                    var deadPacket = new ReplicationBulkRemovePacket(deadIds.ToArray());
                    players.ForEach(p => p.Session.SendImmediate(deadPacket));

                    // 죽은 유저는 유저 목록에서 삭제
                    var idxList = hitPlayers.Select(p => p.idx).OrderByDescending(x => x).ToList();
                    idxList.ForEach(idx => players.RemoveAt(idx));

                    // 플레이어가 죽었다면 좌표 정보도 영향 받는다
                    RefreshPlayerGrid();
                }
            });
        }

        public void GameLoop()
        {
            float dt = 1.0f / 60;
            PlayerUpdateLoop(dt);
            ProjectileUpdateLoop(dt);
            foodLayer.Update();
            CheckFoodLoop();
            CheckKillLoop();
        }

        // 비동기 작업을 위해 데이터에 접근하는 경우 기존 내용을 복사하기
        // TODO 리스트 안의 요속까지 통쨰로 복사해야하나?
        public int GetPlayers(ref List<Player> dst)
        {
            lock (lockobj)
            {
                dst.Clear();
                dst.AddRange(players);
                return players.Count;
            }
        }

        public int GetObservers(ref List<Player> dst)
        {
            lock (lockobj)
            {
                dst.Clear();
                dst.AddRange(observers);
                return observers.Count;
            }
        }

        // TODO 객체의 좌표 관련 정보가 바뀐후에는 갱신해야한다
        readonly Grid<Player> playerGrid = MakeGrid<Player>();

        // grid 생성시 필요한 인자를 숨기고 싶다
        static Grid<T> MakeGrid<T>()
        {
            return new Grid<T>(Config.CellSize, Config.RoomWidth / 2, Config.RoomHeight / 2);
        }

        void RefreshPlayerGrid()
        {
            playerGrid.Clear();
            foreach (var player in players)
            {
                playerGrid.Add(player.Position, player);
            }
        }
    }
}
