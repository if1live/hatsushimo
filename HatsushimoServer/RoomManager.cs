using System.Collections.Generic;

namespace HatsushimoServer
{
    public class RoomManager
    {
        public const string DefaultRoomID = "default";
        readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>();

        public RoomManager()
        {
            CreateRoom(DefaultRoomID);
        }

        public Room CreateRoom(string roomID)
        {
            var room = new Room(roomID);
            rooms[roomID] = room;

            room.Start();

            return room;
        }

        public Room GetRoom(string roomID)
        {
            Room room = null;
            if (!rooms.TryGetValue(roomID, out room))
            {
                room = CreateRoom(roomID);
            }
            return room;
        }

        public void RemoveRoom(string roomID)
        {
            Room room = null;
            if (rooms.TryGetValue(roomID, out room))
            {
                rooms.Remove(roomID);
            }
        }
    }
}
