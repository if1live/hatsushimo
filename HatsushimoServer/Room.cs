using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;
using System.Diagnostics;

namespace HatsushimoServer
{

    public class Room
    {
        readonly Random rand = new Random();
        public string ID { get; private set; }

        // 기본적으로 게임 로직은 싱글 쓰레드로 돌아간다
        // 주기적으로 게임 정보를 클라에 보내주는데 이것은 비동기로 돌아간다
        // 외부에서 정보를 직접 갖다쓰는 곳(주로 플레이어 리스트)은 락을 건다
        Object lockobj = new Object();

        readonly List<Player> players = new List<Player>();
        // 로직에 직접 참가하지 않는 유저목록
        // 방에는 참가했지만 로딩이 끝나지 않는 경우를 처리하는게 목적
        readonly List<Player> waitingPlayers = new List<Player>();
        readonly List<Food> foods = new List<Food>();

        // 방에서만 사용하는 객체는 id를 따로 발급
        readonly IEnumerator<int> foodIDGen = IDGenerator.MakeFoodID().GetEnumerator();

        public Room(string id)
        {
            this.ID = id;

            // 방 만들때 음식 미리 만들기
            for (var i = 0; i < Config.FoodCount; i++)
            {
                var food = MakeFood();
                foods.Add(food);
            }
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

            // 신규 유저에게 월드 정보 알려주기
            player.Session.Send(GenerateReplicaitonAllPacket());

            // 접속한 유저에게 완료 신호 보냄
            // 게임 로직을 돌릴수 있다는 신호임
            player.Session.Send(new PlayerReadyPacket());

            // 기존 유저들에게 새로 생성된 플레이어 정보를 알려주기
            var spawnPacket = player.GenerateCreatePacket();
            prevPlayers.ForEach(p =>
            {
                p.Session.Send(spawnPacket);
            });

            Console.WriteLine($"ready room: id={player.ID} room={ID} size={players.Count}");
            return true;
        }

        public void Join(Player newPlayer)
        {
            var pos = GenerateRandomPosition();
            newPlayer.SetPosition(pos);

            waitingPlayers.Add(newPlayer);

            Console.WriteLine($"room join: id={newPlayer.ID} room={ID} size={players.Count}");
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
                p.Session.Send(removePacket);
            });

            Console.WriteLine($"leave room: id={player.ID} room={ID} size={players.Count}");
        }

        public Vec2 GenerateRandomPosition()
        {
            var w = Config.RoomWidth;
            var h = Config.RoomHeight;
            var x = (float)(rand.NextDouble() - 0.5) * w;
            var y = (float)(rand.NextDouble() - 0.5) * h;
            return new Vec2(x, y);
        }

        Food MakeFood()
        {
            var pos = GenerateRandomPosition();
            var score = 1;

            foodIDGen.MoveNext();
            var id = foodIDGen.Current;

            var food = new Food(id, pos, score);
            return food;
        }


        ReplicationAllPacket GenerateReplicaitonAllPacket()
        {
            var players = this.players.Select(p => new PlayerInitial()
            {
                ID = p.ID,
                Nickname = p.Session.Nickname,
                Pos = p.Position,
                TargetPos = p.TargetPosition,
                Speed = p.Speed,
            });

            var foods = this.foods.Select(f => new FoodInitial()
            {
                ID = f.ID,
                Pos = f.Position,
            });

            return new ReplicationAllPacket()
            {
                Players = players.ToArray(),
                Foods = foods.ToArray(),
            };
        }

        WorldJoinResponsePacket GenerateRoomJoinPacket(Player player)
        {
            return new WorldJoinResponsePacket()
            {
                WorldID = this.ID,
                PlayerID = player.ID,
                Nickname = player.Session.Nickname,
            };
        }

        void SendFoodCreatePacket(Food food)
        {
            // 모든 유저에게 아이템 생성 패킷 전송
            // TODO broadcast emit
            var packet = food.GenerateCreatePacket();
            players.ForEach(p =>
            {
                var session = p.Session;
                session.Send(packet);
            });
        }

        void SendFoodRemovePacket(Food food)
        {
            this.players.ForEach(p =>
            {
                var packet = food.GenerateRemovePacket();
                var session = p.Session;
                session.Send(packet);
                Console.WriteLine($"sent food remove packet: {packet.ID}");
            });
        }

        // 음식 생성. 맵에 어느정도의 먹을게 남아있도록 하는게 목적
        void GenerateFoodLoop()
        {
            var requiredFoodCount = Config.FoodCount - foods.Count;
            for (var i = 0; i < requiredFoodCount; i++)
            {
                var food = MakeFood();
                foods.Add(food);
                SendFoodCreatePacket(food);
            }
        }

        void PlayerUpdateLoop(float dt)
        {
            players.ForEach(player => player.UpdateMove(dt));
        }

        void CheckFoodLoop()
        {
            // 음식을 먹으면 점수를 올리고 음식을 목록에서 삭제
            // TODO quad tree 같은거 쓰면 최적화 가능
            players.ForEach(player =>
            {
                var gainedFoods = foods.Select((food, idx) => new { food = food, idx = idx })
                    .Where(pair =>
                    {
                        var ALLOW_DISTANCE = 1;
                        var p1 = pair.food.Position;
                        var p2 = player.Position;
                        return VectorHelper.IsInRange(p1, p2, ALLOW_DISTANCE);
                    });

                // 먹은 플레이어는 점수 획득
                gainedFoods.Select(pair => pair.food).ToList()
                    .ForEach(food => player.GainScore(food.Score));

                // 모든 플레이어에게 삭제 패킷 보내기
                gainedFoods.Select(pair => pair.food).ToList()
                    .ForEach(food =>
                {
                    SendFoodRemovePacket(food);
                });

                // 배열의 뒤에서부터 제거하면 검색으로 찾은 인덱스를 그대로 쓸수있다
                gainedFoods.ToList()
                    .OrderByDescending(pair => pair.idx).ToList()
                    .ForEach(pair => foods.RemoveAt(pair.idx));
            });
        }

        public void GameLoop()
        {
            float dt = 1.0f / 60;
            PlayerUpdateLoop(dt);
            GenerateFoodLoop();
            CheckFoodLoop();
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
    }
}
