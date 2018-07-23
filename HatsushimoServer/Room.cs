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

        readonly List<Player> players = new List<Player>();
        // 로직에 직접 참가하지 않는 유저목록
        // 방에는 참가했지만 로딩이 끝나지 않는 경우를 처리하는게 목적
        readonly List<Player> waitingPlayers = new List<Player>();
        readonly List<Food> foods = new List<Food>();

        // 방에서만 사용하는 객체는 id를 따로 발급
        readonly IEnumerator<int> foodIDGen = IDGenerator.MakeFoodID().GetEnumerator();

        Leaderboard leaderboard;

        public Room(string id)
        {
            this.ID = id;
            leaderboard = new Leaderboard(players, Config.LeaderboardSize);

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

            // 유저를 로직에 합류시킴
            var prevPlayers = players.ToList();
            waitingPlayers.RemoveAt(found);

            // TODO lock required?
            players.Add(player);

            // 신규 유저에게 월드 정보 알려주기
            // 월드 정보 이외에도 리더보드 같이 변경될때만 알려주는 정보도 알려주기
            player.Session.Send(GenerateReplicaitonAllPacket());
            player.Session.Send(leaderboard.GenerateLeaderboardPacket());

            // 접속한 유저에게 완료 신호 보냄
            // 게임 로직을 돌릴수 있다는 신호임
            player.Session.Send(new PlayerReadyPacket());

            // 기존 유저들에게 새로 생성된 플레이어 정보를 알려주기
            var spawnPacket = player.GenerateCreatePacket();
            prevPlayers.ForEach(p =>
            {
                p.Session.Send(spawnPacket);
            });

            Console.WriteLine($"ready room: room={this.ID}, player={player.ID}, room_size={players.Count}");
            return true;
        }

        public void Join(Player newPlayer)
        {
            var pos = GenerateRandomPosition();
            newPlayer.SetPosition(pos);
            waitingPlayers.Add(newPlayer);

            Console.WriteLine($"room join: id={newPlayer.ID} room={ID}");
        }

        public void Leave(Player player)
        {
            var found = players.FindIndex((x) => x.ID == player.ID);
            if (found > -1)
            {
                players.RemoveAt(found);
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

            Console.WriteLine($"leave room: room={ID}, player={player.ID}");
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
            var clonedPlayers = this.players.ToList();
            var players = clonedPlayers.Select(p => new PlayerInitial()
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
            var clonedPlayers = players.ToList();
            clonedPlayers.ForEach(p =>
            {
                var session = p.Session;
                session.Send(packet);
            });
        }

        void SendFoodRemovePacket(Food food)
        {
            var clonedPlayers = players.ToList();
            clonedPlayers.ForEach(p =>
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
            var clonedPlayers = this.players.ToList();
            clonedPlayers.ForEach(player => player.UpdateMove(dt));
        }

        void CheckFoodLoop()
        {
            var clonedPlayers = this.players.ToList();
            // 음식을 먹으면 점수를 올리고 음식을 목록에서 삭제
            // TODO quad tree 같은거 쓰면 최적화 가능
            clonedPlayers.ForEach(player =>
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

        public void NetworkLoop()
        {
            var clonedPlayers = players.ToList();
            clonedPlayers.ForEach(player =>
            {
                var actions = clonedPlayers.Select(p => new ReplicationActionPacket()
                {
                    Action = ReplicationAction.Update,
                    ID = p.ID,
                    ActorType = p.Type,
                    Pos = p.Position,
                    TargetPos = p.TargetPosition,
                    Speed = p.Speed,
                    Extra = "",
                });

                var packet = new ReplicationBulkActionPacket()
                {
                    Actions = actions.ToArray(),
                };
                player.Session.Send(packet);
            });
        }

        public void LeaderboardLoop()
        {
            // 리더보드 변경 사항이 있는 경우에만 전송
            // 밑바닥 사람들의 점수는 몇점이든 별로 중요하지 않다
            // 상위 랭킹이 바뀐것만 리더보드로 취급하자
            var clonedPlayers = this.players.ToList();
            var newLeaderboard = new Leaderboard(clonedPlayers, Config.LeaderboardSize);
            if (!leaderboard.IsLeaderboardEqual(newLeaderboard))
            {
                leaderboard = newLeaderboard;
                var packet = newLeaderboard.GenerateLeaderboardPacket();
                clonedPlayers.ForEach(player =>
                {
                    player.Session.Send(packet);
                });
            }
        }
    }
}
