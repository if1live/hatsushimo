using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class World
    {
        /*
        const string EVENT_PLAYER_POSITIONS = "player-positions";


        public ReactiveProperty<PlayerContext[]> Players {
            get { return _players; }
        }
        readonly ReactiveProperty<PlayerContext[]> _players = new ReactiveProperty<PlayerContext[]>(new PlayerContext[] { });

        public void Setup()
        {
            var socket = SocketManager.Instance.MySocket;

            socket.On<PlayerContextList>(EVENT_PLAYER_POSITIONS, (ctx) =>
            {
                Players.Value = ctx.players;
            });
        }

        public void Cleanup()
        {
            var mgr = SocketManager.Instance;
            if(!mgr) { return; }

            var socket = mgr.MySocket;
            socket.Off(EVENT_PLAYER_POSITIONS);
        }
        */
    }
}
