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

        IObservable<PlayerStatusPacket> StatusObservable {
            get { return status.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerStatusPacket> status = new ReactiveProperty<PlayerStatusPacket>(null);

        IObservable<PlayerListPacket> ListObservable {
          get { return list.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerListPacket> list = new ReactiveProperty<PlayerListPacket>(null);

        IObservable<PlayerSpawnPacket> SpawnObservable {
            get { return spawn.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerSpawnPacket> spawn = new ReactiveProperty<PlayerSpawnPacket>(null);

        IObservable<PlayerDeadPacket> DeadObservable {
            get { return dead.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerDeadPacket> dead = new ReactiveProperty<PlayerDeadPacket>(null);

        IObservable<PlayerLeavePacket> LeaveObservable {
            get { return leave.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerLeavePacket> leave = new ReactiveProperty<PlayerLeavePacket>(null);


        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;

            // 플레이어 자신은 항상 존재한다
            myPlayer = Instantiate(prefab_my);
            myPlayer.transform.SetParent(transform);
            myPlayer.id = conn.PlayerID;

            conn.On(Events.PLAYER_READY, () =>
            {
                Debug.Log("player ready");
            });
            conn.On<PlayerListPacket>(Events.PLAYER_LIST, (ctx) => list.Value = ctx);
            conn.On<PlayerSpawnPacket>(Events.PLAYER_SPAWN, (ctx) => spawn.Value = ctx);
            conn.On<PlayerDeadPacket>(Events.PLAYER_DEAD, (ctx) => dead.Value = ctx);
            conn.On<PlayerLeavePacket>(Events.PLAYER_LEAVE, (ctx) => leave.Value = ctx);
            conn.On<PlayerStatusPacket>(Events.PLAYER_STATUS, (ctx) => status.Value = ctx);

            // packet handler. 게임 객체를 건드리는 작업은 메인 쓰레드에서 해야한다
            SpawnObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                Debug.Assert(playerTable.ContainsKey(ctx.id) == false, "already id exist");
                var player = Instantiate(prefab_enemy);
                playerTable[ctx.id] = player;
                player.transform.SetParent(transform);
                player.id = ctx.id;

                var pos = new Vector3(ctx.pos_x, ctx.pos_y, 0);
                player.transform.position = pos;
            });

            DeadObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                Debug.Assert(playerTable.ContainsKey(ctx.id) == true, "id not exist");
                var player = playerTable[ctx.id];
                playerTable.Remove(ctx.id);
                Destroy(player.gameObject);
            });

            LeaveObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                Debug.Assert(playerTable.ContainsKey(ctx.id) == true, "id not exist");
                var player = playerTable[ctx.id];
                playerTable.Remove(ctx.id);
                Destroy(player.gameObject);
            });

            ListObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                var myID = conn.PlayerID;

                var my = ctx.GetMyPlayer(myID);
                myPlayer.ApplyInfo(my);

                foreach(var p in ctx.GetEnemyPlayers(myID))
                {
                    Player player = null;
                    if (!playerTable.TryGetValue(p.id, out player))
                    {
                        player = Instantiate(prefab_enemy);
                        player.transform.SetParent(transform);
                        playerTable[p.id] = player;
                    }

                    player.ApplyInfo(p);
                }
            });

            StatusObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                var myID = conn.PlayerID;

                var myStatus = ctx.GetMyPlayer(myID);
                myPlayer.ApplyStatus(myStatus);

                foreach (var p in ctx.GetEnemyPlayers(myID))
                {
                    Player player = playerTable[p.id];
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
            conn.Off(Events.PLAYER_LIST);
            conn.Off(Events.PLAYER_READY);
            conn.Off(Events.PLAYER_SPAWN);
            conn.Off(Events.PLAYER_DEAD);
            conn.Off(Events.PLAYER_LEAVE);
        }
    }
}
