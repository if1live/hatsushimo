using Assets.Game.Packets;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    public class ReplicationManager : MonoBehaviour 
    {
        public static ReplicationManager Instance;

        public Player prefab_enemy;
        public Player prefab_my;
        public Food prefab_food;

        Dictionary<int, Food> foodTable = new Dictionary<int, Food>();
        Dictionary<int, Player> playerTable = new Dictionary<int, Player>();

        IObservable<ReplicationAllPacket> ReplicationAllObservable {
            get { return replicationAll.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<ReplicationAllPacket> replicationAll = new ReactiveProperty<ReplicationAllPacket>(null);

        IObservable<ReplicationActionPacket> ReplicationObservable {
            get { return replication.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<ReplicationActionPacket> replication = new ReactiveProperty<ReplicationActionPacket>(null);

        IObservable<ReplicationBulkActionPacket> ReplicationBulkObservable {
            get { return replicationBulk.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<ReplicationBulkActionPacket> replicationBulk = new ReactiveProperty<ReplicationBulkActionPacket>(null);

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;

            conn.On<ReplicationAllPacket>(Events.REPLICATION_ALL, (p) => replicationAll.Value = p);
            conn.On<ReplicationActionPacket>(Events.REPLICATION_ACTION, (p) => replication.Value = p);
            conn.On<ReplicationBulkActionPacket>(Events.REPLICATION_BULK_ACTION, (p) => replicationBulk.Value = p);

            conn.On(Events.PLAYER_READY, () =>
            {
                Debug.Log("player ready");
            });

            ReplicationAllObservable.ObserveOnMainThread().Subscribe(ctx =>
            {
                // 플레이어 생성
                foreach (var p in ctx.players)
                {
                    var pos = new Vector3(p.pos_x, p.pos_y, 0);
                    var player = GetOrCreatePlayer(p.id, pos);
                    player.ApplyInitial(p);
                }

                // 아이템 생성
                foreach(var i in ctx.items)
                {
                    var pos = new Vector3(i.pos_x, i.pos_y, 0);
                    CreateFood(i.id, pos);
                }

            }).AddTo(gameObject);

            ReplicationBulkObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                foreach(var act in packet.actions)
                {
                    HandleReplicationAction(act);
                }
            }).AddTo(gameObject);

            ReplicationObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                HandleReplicationAction(packet);
            });
        }

        void HandleReplicationAction(ReplicationActionPacket packet)
        {
            switch (packet.action)
            {
                case ReplicationActions.Create:
                    HandleReplicationCreate(packet);
                    break;
                case ReplicationActions.Update:
                    HandleReplicationUpdate(packet);
                    break;
                case ReplicationActions.Remove:
                    HandleReplicationRemove(packet);
                    break;
                default:
                    break;
            };
        }

        void HandleReplicationCreate(ReplicationActionPacket packet)
        {
            var conn = ConnectionManager.Instance.Conn;
            var myid = conn.PlayerID;
            var id = packet.id;
            var x = packet.pos_x;
            var y = packet.pos_y;
            var pos = new Vector3(x, y, 0);

            if (packet.type == "player")
            {
                var player = CreatePlayer(id, pos);
                player.ApplyReplication(packet);
            }
            else if(packet.type == "food")
            {
                CreateFood(id, pos);
            }
        }

        void HandleReplicationUpdate(ReplicationActionPacket packet)
        {
            var id = packet.id;
            if(packet.type == "player")
            {
                var player = playerTable[id];
                player.ApplyReplication(packet);
            }
        }

        void HandleReplicationRemove(ReplicationActionPacket packet)
        {
            var conn = ConnectionManager.Instance.Conn;
            var myid = conn.PlayerID;
            var id = packet.id;

            if (myid == id)
            {
                // TODO 자신이 죽는 경우
                Debug.Log("TODO self remove");
            }
            else if(playerTable.ContainsKey(id))
            {
                RemovePlayer(id);
            }
            else if (foodTable.ContainsKey(id))
            {
                RemoveFood(id);
            }
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

        Player GetPlayerPrefab(int id)
        {
            var conn = ConnectionManager.Instance.Conn;
            var myid = conn.PlayerID;
            return (myid == id) ? prefab_my : prefab_enemy;
        }

        Player CreatePlayer(int id, Vector3 pos)
        {
            Debug.Assert(playerTable.ContainsKey(id) == false, $"player={id} already exist in player table");
            var prefab = GetPlayerPrefab(id);
            var player = Instantiate(prefab);
            playerTable[id] = player;
            player.transform.SetParent(transform);
            player.id = id;
            player.transform.position = pos;

            return player;
        }

        Player GetOrCreatePlayer(int id, Vector3 pos)
        {
            Player player = null;
            if (!playerTable.TryGetValue(id, out player))
            {
                player = CreatePlayer(id, pos);
            }
            player.transform.position = pos;
            return player;
        }

        public Player FindPlayer(int id)
        {
            return playerTable[id];
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
            Debug.Assert(Instance != null);
            Instance = null;

            var mgr = ConnectionManager.Instance;
            if (!mgr) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.PLAYER_READY);

            conn.Off(Events.REPLICATION_ALL);
            conn.Off(Events.REPLICATION_ACTION);
            conn.Off(Events.REPLICATION_BULK_ACTION);
        }
    }
}
