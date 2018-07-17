using UnityEngine;
using UniRx;
using System.Collections.Generic;
using Assets.Game.Types;

namespace Assets.Game
{
    class PlayerManager : MonoBehaviour 
    {
        public Player prefab_enemy;
        public Player prefab_my;

        Dictionary<string, Player> playerTable = new Dictionary<string, Player>();
        Player myPlayer = null;

        ReactiveProperty<PlayerPositionPacket> packet = new ReactiveProperty<PlayerPositionPacket>(null);

        const string EVENT_PLAYER_POSITIONS = "player-positions";

        private void Start()
        {
            // 플레이어 자신은 항상 존재한다
            myPlayer = Instantiate(prefab_my);
            myPlayer.transform.SetParent(transform);

            var socket = SocketManager.Instance.MySocket;
            socket.On<PlayerPositionPacket>(EVENT_PLAYER_POSITIONS, (ctx) =>
            {
                packet.Value = ctx;
            });

            packet.Where(x => x != null).ObserveOnMainThread().Subscribe(ctx =>
            {
                var myID = Connection.Instance.PlayerID;

                var myStatus = ctx.GetMyPlayer(myID);
                myPlayer.ApplyStatus(myStatus);

                foreach (var p in ctx.GetEnemyPlayers(myID))
                {
                    Player player = null;
                    if (!playerTable.TryGetValue(p.playerID, out player))
                    {
                        player = Instantiate(prefab_enemy);
                        player.transform.SetParent(transform);
                        playerTable[p.playerID] = player;
                    }

                    player.ApplyStatus(p);
                }
            });
        }

        private void OnDestroy()
        {
            var mgr = SocketManager.Instance;
            if (!mgr) { return; }

            var socket = mgr.MySocket;
            socket.Off(EVENT_PLAYER_POSITIONS);
        }
    }
}
