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
        static readonly Logger log = LogManager.GetLogger("Room");
        readonly Random rand = new Random();
        public string ID { get; private set; }


        // 기본적으로 게임 로직은 싱글 쓰레드로 돌아간다
        // 주기적으로 게임 정보를 클라에 보내주는데 이것은 비동기로 돌아간다
        // 외부에서 정보를 직접 갖다쓰는 곳(주로 플레이어 리스트)은 락을 건다
        Object lock_player = new Object();
        object lock_observer = new object();

        // 로직에 직접 참가하지 않는 유저목록
        // 방에는 참가했지만 로딩이 끝나지 않는 경우를 처리하는게 목적
        List<Player> waitingPlayers = new List<Player>();

        // TODO 죽은 플레이어 목록
        // 죽은 유저를 즉시 방에서 쫒아내는것보다는 잠시라도 방에 남아있다가 쫒아내고 싶다
        // 방에 남아있으면 킬캠같은걸 구현 가능하다
        // 또는 제자리에서 부활하거나

        readonly FoodManager foodMgr;
        readonly PlayerManager playerMgr;
        readonly ProjectileManager projectileMgr;
        readonly ObserverManager observerMgr;

        readonly IBroadcaster broadcaster;
        readonly Broadcaster playerCaster;
        readonly ObserverBroadcaster observerCaster;

        public Room(string id)
        {
            this.ID = id;

            playerCaster = new Broadcaster();
            observerCaster = new ObserverBroadcaster();
            broadcaster = new BroadcasterGroup(new IBroadcaster[] { playerCaster, observerCaster });

            playerMgr = new PlayerManager(broadcaster);
            foodMgr = new FoodManager(broadcaster);
            projectileMgr = new ProjectileManager(broadcaster);
            observerMgr = new ObserverManager();

            playerCaster.Grid = playerMgr.Grid;
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
            lock (lock_player)
            {
                playerMgr.Add(player);
            }

            // 신규 유저에게 월드 정보 알려주기
            player.Session.SendImmediate(GenerateReplicaitonAllPacket());

            // 접속한 유저에게 완료 신호 보냄
            // 게임 로직을 돌릴수 있다는 신호임
            player.Session.SendImmediate(new PlayerReadyPacket());

            log.Info($"ready room: id={player.ID} room={ID} size={playerMgr.Count}");
            return true;
        }

        public void Join(Player newPlayer)
        {
            var pos = GenerateRandomPosition();
            newPlayer.SetPosition(pos);
            newPlayer.TargetPosition = pos;

            waitingPlayers.Add(newPlayer);

            log.Info($"room join: id={newPlayer.ID} room={ID} size={playerMgr.Count}");
        }

        public void Leave(Player player)
        {
            lock (lock_player)
            {
                playerMgr.Remove(player);
            }

            // 로딩 끝나기전에 나가는 경우 처리
            var waitingFound = waitingPlayers.FindIndex((x) => x.ID == player.ID);
            if (waitingFound > -1)
            {
                waitingPlayers.RemoveAt(waitingFound);
            }

            log.Info($"leave room: id={player.ID} room={ID} size={playerMgr.Count}");
        }

        public void Join(Observer observer)
        {
            lock (lock_observer)
            {
                observerMgr.Add(observer);
                observerCaster.Add(observer);
            }

            observer.Session.SendImmediate(GenerateReplicaitonAllPacket());

            log.Info($"join observer: id={observer.ID} room={ID} size={playerMgr.Count}");
        }

        public void Leave(Observer observer)
        {
            lock (lock_observer)
            {
                observerMgr.Remove(observer.ID);
                observerCaster.Remove(observer);
            }
            log.Info($"leave observer: id={observer.ID} room={ID} size={playerMgr.Count}");
        }

        public void LaunchProjectile(Player player)
        {
            var projectile = projectileMgr.Create(player);
            projectileMgr.BroadcastProjectileCreate(projectile);
        }

        public Vector2 GenerateRandomPosition()
        {
            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            var x = (float)(rand.NextDouble() - 0.5) * w;
            var y = (float)(rand.NextDouble() - 0.5) * h;
            return new Vector2(x, y);
        }

        ReplicationAllPacket GenerateReplicaitonAllPacket()
        {
            return new ReplicationAllPacket(
                playerMgr.StatusList.ToArray(),
                foodMgr.StatusList.ToArray(),
                projectileMgr.StatusList.ToArray()
            );
        }

        WorldJoinResultPacket GenerateRoomJoinPacket(Player player)
        {
            return new WorldJoinResultPacket(0, player.ID);
        }

        void CheckFoodLoop()
        {
            // 음식을 먹으면 점수를 올리고 음식을 목록에서 삭제
            // TODO quad tree 같은거 쓰면 최적화 가능
            var players = new List<Player>();
            playerMgr.GetPlayers(ref players);

            foreach (var player in players)
            {
                var ALLOW_DISTANCE = 1;
                var gainedFoods = foodMgr.GetFoods(player.Position, ALLOW_DISTANCE);

                // 먹은 플레이어는 점수 획득
                gainedFoods.Select(food =>
                {
                    player.GainFoodScore(Config.FoodScore);
                    return food;
                });

                var ids = gainedFoods.Select(food => food.ID);
                foodMgr.Remove(ids);
            };
        }

        void CheckKillLoop()
        {
            var players = new List<Player>();
            playerMgr.GetPlayers(ref players);

            foreach (var player in players)
            {
                var ALLOW_DISTANCE = 1;
                // 플레이어와 가까운 투사체 목록
                // TODO 죽창 충돌처리 개선하기
                // 일단은 점-점으로 계산
                var projectiles = projectileMgr.GetProjectiles(player.Position, ALLOW_DISTANCE);
                var hits = projectiles.Where(p => p.OwnerID != player.ID);

                // 투사체의 소유자는 점수를 킬카운트 증가
                foreach (var projectile in hits)
                {
                    var owner = players.Where(p => p.ID == projectile.OwnerID).First();
                    if (owner != null)
                    {
                        owner.GainKillScore(1);
                    }
                }

                // 플레이어는 죽창에 맞았으니 죽어야한다
                if (hits.Count() > 0)
                {
                    // 유저가 죽었다는걸 다른 유저에게 알려준다
                    // TODO 알려주는 범위 통제하면 대역폭을 아낄수 있다
                    // TODO 객체 삭제 패킷과 유저 죽음 패킷은 분리하는게 가능하다
                    var deadPacket = new ReplicationRemovePacket(player.ID);
                    playerCaster.Broadcast(player.Position, deadPacket);

                    // TODO 유저가 죽었다는것과 유저가 나갔다는것을 구분해야한다
                    // 죽은 유저를 방에서 즉시 제거하는게 최선인가?
                    // 죽은 유저는 유저 목록에서 삭제
                    playerMgr.Remove(player);
                }
            }
        }

        public void GameLoop()
        {
            float dt = 1.0f / 60;
            observerMgr.Update();
            playerMgr.Update(dt);
            projectileMgr.Update(dt);
            foodMgr.Update();
            CheckFoodLoop();
            CheckKillLoop();
        }

        // 비동기 작업을 위해 데이터에 접근하는 경우 기존 내용을 복사하기
        // TODO 리스트 안의 요속까지 통쨰로 복사해야하나?
        public int GetPlayers(ref List<Player> dst)
        {
            lock (lock_player)
            {
                return playerMgr.GetPlayers(ref dst);
            }
        }

        public int GetObservers(ref List<Observer> dst)
        {
            lock (lock_observer)
            {
                return observerMgr.GetObservers(ref dst);
            }
        }
    }
}
