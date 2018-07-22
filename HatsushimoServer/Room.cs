using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Types;
using Hatsushimo.Utils;

namespace HatsushimoServer
{

    public class Room
    {
        readonly Random rand = new Random();
        public string ID { get; private set; }

        readonly List<Player> players = new List<Player>();
        readonly List<Player> waitingPlayers = new List<Player>();
        readonly List<Food> foods = new List<Food>();

        readonly IEnumerator<int> foodIDGen = IDGenerator.MakeFoodID().GetEnumerator();

        public Room(string id)
        {
            this.ID = id;

            for (var i = 0; i < Config.FoodCount; i++)
            {
                var food = MakeFood();
                foods.Add(food);
            }
        }

        public bool JoinPlayer(Player newPlayer)
        {
            if (newPlayer.RoomID != null) { return false; }

            newPlayer.RoomID = ID;
            var pos = GenerateRandomPosition();
            newPlayer.SetPosition(pos);
            waitingPlayers.Add(newPlayer);

            Console.WriteLine($"join room = room={ID} player={newPlayer.ID}");
            return true;
        }

        public bool SpawnPlayer(Player player)
        {
            // TODO
            return false;
        }

        public bool LeavePlayer(Player player)
        {
            if (player.RoomID != ID) { return false; }

            var found = players.FindIndex((x) => x.ID == player.ID);
            if (found > -1)
            {
                players.RemoveAt(found);
                player.RoomID = null;
            }

            // 로딩 끝나기전에 나가는 경우 처리
            var waitingFound = waitingPlayers.FindIndex((x) => x.ID == player.ID);
            if (waitingFound > -1)
            {
                waitingPlayers.RemoveAt(waitingFound);
                player.RoomID = null;
            }

            // 방을 나갔다는것을 다른 유저도 알아야한다
            var leavePacket = new RoomLeavePacket()
            {
                PlayerID = player.ID,
            };
            // TODO send?
            Console.WriteLine($"leave room = room={ID} player={player.ID}");
            return true;
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

        void SendFoodCreatePacket(Food food) {

        }


        void GameLoop()
        {

        }

        void NetworkLoop()
        {

        }

        void LeaderboardLoop()
        {

        }

        public bool Running { get; private set; } = false;
        public void Stop()
        {
            Running = false;
        }
        public void Start()
        {
            Running = true;
            StartGameLoop();
            StartNetworkLoop();
            StartLeaderboardLoop();
        }

        async void StartGameLoop()
        {
            var interval = 1000 / 60;
            while (Running)
            {
                var a = TimeUtils.NowMillis;
                GameLoop();
                var b = TimeUtils.NowMillis;

                var diff = b - a;
                var wait = interval - diff;
                if (wait < 0) { wait = 0; }
                var delay = TimeSpan.FromMilliseconds(wait);
                await Task.Delay(delay);
            }
        }

        async void StartNetworkLoop()
        {
            var interval = 1000 / Config.SendRateCoord;
            while (Running)
            {
                var a = TimeUtils.NowMillis;
                NetworkLoop();
                var b = TimeUtils.NowMillis;

                var diff = b - a;
                var wait = interval - diff;
                if (wait < 0) { wait = 0; }
                var delay = TimeSpan.FromMilliseconds(wait);
                await Task.Delay(delay);
            }
        }

        async void StartLeaderboardLoop()
        {
            var interval = 1000 / Config.SendRateLeaderboard;
            while (Running)
            {
                var a = TimeUtils.NowMillis;
                LeaderboardLoop();
                var b = TimeUtils.NowMillis;

                var diff = b - a;
                var wait = interval - diff;
                if (wait < 0) { wait = 0; }
                var delay = TimeSpan.FromMilliseconds(wait);
                await Task.Delay(delay);
            }
        }
    }
}
