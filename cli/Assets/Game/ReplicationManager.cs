using Assets.Game.Packets;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class ReplicationManager : MonoBehaviour 
    {
        public Player prefab_enemy;
        public Player prefab_my;
        public Food prefab_food;

        Dictionary<int, Food> foodTable = new Dictionary<int, Food>();
        Dictionary<int, Player> playerTable = new Dictionary<int, Player>();
        Player myPlayer = null;

        IObservable<ReplicationPacket> ReplicationObservable {
            get { return replication.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<ReplicationPacket> replication = new ReactiveProperty<ReplicationPacket>(null);

        IObservable<StaticItemCreatePacket> ItemCreateObservable {
            get { return itemCreate.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<StaticItemCreatePacket> itemCreate = new ReactiveProperty<StaticItemCreatePacket>(null);

        IObservable<StaticItemRemovePacket> ItemRemoveObservable {
            get { return itemRemove.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<StaticItemRemovePacket> itemRemove = new ReactiveProperty<StaticItemRemovePacket>(null);

        IObservable<PlayerStatusPacket> PlayerStatusObservable {
            get { return playerStatus.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerStatusPacket> playerStatus = new ReactiveProperty<PlayerStatusPacket>(null);

        IObservable<PlayerSpawnPacket> PlayerSpawnObservable {
            get { return playerSpawn.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerSpawnPacket> playerSpawn = new ReactiveProperty<PlayerSpawnPacket>(null);

        IObservable<PlayerDeadPacket> PlayerDeadObservable {
            get { return playerDead.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerDeadPacket> playerDead = new ReactiveProperty<PlayerDeadPacket>(null);

        IObservable<PlayerLeavePacket> PlayerLeaveObservable {
            get { return playerLeave.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<PlayerLeavePacket> playerLeave = new ReactiveProperty<PlayerLeavePacket>(null);

        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;

            // 플레이어 자신은 항상 존재한다
            myPlayer = Instantiate(prefab_my);
            myPlayer.transform.SetParent(transform);
            myPlayer.id = conn.PlayerID;

            conn.On<ReplicationPacket>(Events.REPLICATION, (p) => replication.Value = p);

            conn.On<PlayerSpawnPacket>(Events.PLAYER_SPAWN, (p) => playerSpawn.Value = p);
            conn.On<PlayerDeadPacket>(Events.PLAYER_DEAD, (p) => playerDead.Value = p);
            conn.On<PlayerLeavePacket>(Events.PLAYER_LEAVE, (p) => playerLeave.Value = p);
            conn.On<PlayerStatusPacket>(Events.PLAYER_STATUS, (p) => playerStatus.Value = p);

            conn.On<StaticItemCreatePacket>(Events.STATIC_ITEM_CREATE, (p) => itemCreate.Value = p);
            conn.On<StaticItemRemovePacket>(Events.STATIC_ITEM_REMOVE, (p) => itemRemove.Value = p);

            conn.On(Events.PLAYER_READY, () =>
            {
                Debug.Log("player ready");
            });

            ReplicationObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                var myID = conn.PlayerID;

                // 내 플레이어 생성
                var my = ctx.GetMyPlayer(myID);
                myPlayer.ApplyReplication(my);

                // 다른 플레이어 생성
                foreach (var p in ctx.GetOtherPlayers(myID))
                {
                    var pos = new Vector3(p.pos_x, p.pos_y, 0);
                    var player = GetOrCreatePlayer(prefab_enemy, p.id, pos);
                    player.ApplyReplication(p);
                }

                // 아이템 생성
                foreach(var i in ctx.items)
                {
                    var pos = new Vector3(i.pos_x, i.pos_y, 0);
                    CreateFood(i.id, pos);
                }

            }).AddTo(gameObject);

            // TODO 좌표 정보 동기화
            PlayerStatusObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                var myID = conn.PlayerID;

                var myStatus = ctx.GetMyPlayer(myID);
                myPlayer.ApplyStatus(myStatus);

                foreach (var p in ctx.GetEnemyPlayers(myID))
                {
                    Player player = playerTable[p.id];
                    player.ApplyStatus(p);
                }
            }).AddTo(gameObject);


            PlayerSpawnObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                Debug.Assert(playerTable.ContainsKey(ctx.id) == false, "already id exist");
                var pos = new Vector3(ctx.pos_x, ctx.pos_y, 0);
                var player = CreatePlayer(prefab_enemy, ctx.id, pos);

            }).AddTo(gameObject);

            PlayerDeadObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                RemovePlayer(packet.id);
            }).AddTo(gameObject);

            PlayerLeaveObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                RemovePlayer(packet.id);
            }).AddTo(gameObject);

            ItemCreateObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                // TODO food
                var pos = new Vector3(packet.pos_x, packet.pos_y, 0);
                CreateFood(packet.id, pos);
            }).AddTo(gameObject);

            ItemRemoveObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                // TODO food
                RemoveFood(packet.id);
            }).AddTo(gameObject);
        }

        Food CreateFood(int id, Vector3 pos)
        {
            Debug.Assert(foodTable.ContainsKey(id) == false, $"food={id} already exist in food table");
            var food = Instantiate(prefab_food);
            foodTable[id] = food;
            food.transform.SetParent(transform);
            food.id = id;
            food.transform.position = pos;
            return food;
        }

        void RemoveFood(int id)
        {
            Debug.Assert(foodTable.ContainsKey(id) == true, $"food={id} not exist in food table");
            var item = foodTable[id];
            foodTable.Remove(id);
            Destroy(item.gameObject);
        }

        Player CreatePlayer(Player prefab, int id, Vector3 pos)
        {
            Debug.Assert(playerTable.ContainsKey(id) == false, $"player={id} already exist in player table");
            var player = Instantiate(prefab);
            playerTable[id] = player;
            player.transform.SetParent(transform);
            player.id = id;
            player.transform.position = pos;

            return player;
        }

        Player GetOrCreatePlayer(Player prefab, int id, Vector3 pos)
        {
            Player player = null;
            if (!playerTable.TryGetValue(id, out player))
            {
                player = Instantiate(prefab);
                player.transform.SetParent(transform);
                playerTable[id] = player;
            }
            return player;
        }

        void RemovePlayer(int id)
        {
            Debug.Assert(playerTable.ContainsKey(id) == true, $"player={id} not exist");
            var player = playerTable[id];
            playerTable.Remove(id);
            Destroy(player.gameObject);
        }

        private void OnDestroy()
        {
            var mgr = ConnectionManager.Instance;
            if (!mgr) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.PLAYER_STATUS);

            conn.Off(Events.PLAYER_READY);
            conn.Off(Events.PLAYER_SPAWN);
            conn.Off(Events.PLAYER_DEAD);
            conn.Off(Events.PLAYER_LEAVE);

            conn.Off(Events.STATIC_ITEM_CREATE);
            conn.Off(Events.STATIC_ITEM_REMOVE);
        }
    }
}
