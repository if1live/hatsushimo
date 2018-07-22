using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.Utils;

namespace HatsushimoServer
{
    public class RoomManager
    {
        public const string DefaultRoomID = "default";
        readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        public RoomManager()
        {
            // create default room
            var _ = GetRoom(DefaultRoomID);
        }

        public Room CreateRoom(string roomID)
        {
            var room = new Room(roomID);

            room.Running = true;
            StartGameLoop(room);
            StartNetworkLoop(room);
            StartLeaderboardLoop(room);

            return room;
        }

        public Room GetRoom(string roomID)
        {
            Room room = null;
            if (!rooms.TryGetValue(roomID, out room))
            {
                room = CreateRoom(roomID);
                rooms[roomID] = room;
            }
            return room;
        }

        public void RemoveRoom(string roomID)
        {
            Room room = null;
            if (rooms.TryGetValue(roomID, out room))
            {
                room.Running = false;
                rooms.Remove(roomID);
            }
        }

        async void StartGameLoop(Room room)
        {
            var interval = 1000 / 60;
            while (room.Running)
            {
                var a = TimeUtils.NowMillis;
                room.GameLoop();
                var b = TimeUtils.NowMillis;

                var diff = b - a;
                var wait = interval - diff;
                if (wait < 0) { wait = 0; }
                var delay = TimeSpan.FromMilliseconds(wait);
                await Task.Delay(delay);
            }
        }

        async void StartNetworkLoop(Room room)
        {
            var interval = 1000 / Config.SendRateCoord;
            while (room.Running)
            {
                var a = TimeUtils.NowMillis;
                room.NetworkLoop();
                var b = TimeUtils.NowMillis;

                var diff = b - a;
                var wait = interval - diff;
                if (wait < 0) { wait = 0; }
                var delay = TimeSpan.FromMilliseconds(wait);
                await Task.Delay(delay);
            }
        }

        async void StartLeaderboardLoop(Room room)
        {
            var interval = 1000 / Config.SendRateLeaderboard;
            while (room.Running)
            {
                var a = TimeUtils.NowMillis;
                room.LeaderboardLoop();
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
