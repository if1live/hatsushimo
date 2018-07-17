using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Game.Types
{
    public class PlayerContext
    {
        public string playerID;
        public string roomID;
        public string nickname;

        public bool ready;

        public float posX;
        public float posY;

        public float dirX;
        public float dirY;
        public float speed;
    }

    public class PlayerPositionPacket
    {
        public PlayerContext[] players;

        public PlayerContext[] GetEnemyPlayers(string id) {
            return players.Where(x => x.playerID != id).ToArray();
        }

        public PlayerContext GetMyPlayer(string id)
        {
            return players.Where(x => x.playerID == id).First();
        }
    }
}
