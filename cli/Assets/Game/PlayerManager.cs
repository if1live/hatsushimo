using UnityEngine;
using UniRx;
using System.Collections.Generic;
using Assets.Game.Packets;
using System;

namespace Assets.Game
{
    class PlayerManager : MonoBehaviour
    {
        public Player prefab_enemy;
        public Player prefab_my;

        Dictionary<int, Player> playerTable = new Dictionary<int, Player>();
        Player myPlayer = null;

        IObservable<PlayerPacket> PacketObservable {
            get { return packet.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerPacket> packet = new ReactiveProperty<PlayerPacket>(null);



        private void Start()
        {
            // 플레이어 자신은 항상 존재한다
            myPlayer = Instantiate(prefab_my);
            myPlayer.transform.SetParent(transform);

            var conn = ConnectionManager.Instance.Conn;
            conn.On<PlayerPacket>(Events.PLAYER_STATUS, (ctx) =>
            {
                packet.Value = ctx;
            });

            PacketObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                var myID = conn.PlayerID;

                var myStatus = ctx.GetMyPlayer(myID);
                myPlayer.ApplyStatus(myStatus);

                foreach (var p in ctx.GetEnemyPlayers(myID))
                {
                    Player player = null;
                    if (!playerTable.TryGetValue(p.id, out player))
                    {
                        player = Instantiate(prefab_enemy);
                        player.transform.SetParent(transform);
                        playerTable[p.id] = player;
                    }

                    //TODO 객체 생성과 위치 정보는 분리되어있다
                    player.ApplyStatus(p);
                }
            });
        }

        private void OnDestroy()
        {
            var mgr = ConnectionManager.Instance;
            if (!mgr) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.PLAYER_STATUS);
        }
    }
}
