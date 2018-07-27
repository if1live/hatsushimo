using Assets.Game.Extensions;
using Assets.NetChan;
using Hatsushimo.Packets;
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

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            var dispatcher = PacketDispatcher.Instance;

            dispatcher.PlayerReady.Received.Subscribe(_ =>
            {
                Debug.Log("player ready");
            });

            dispatcher.ReplicationAll.Received.Subscribe(packet =>
            {
                Debug.Log("replicaiton all received");

                // 플레이어 생성
                foreach (var p in packet.Players)
                {
                    var pos = p.Pos.ToVector3();
                    var player = GetOrCreatePlayer(p.ID, pos);
                    player.ApplyInitial(p);
                }

                foreach (var i in packet.Foods)
                {
                    var pos = i.Pos.ToVector3();
                    CreateFood(i.ID, pos);
                }
            }).AddTo(gameObject);

            dispatcher.ReplicationBulk.Received.Subscribe(packet =>
            {
                foreach(var act in packet.Actions)
                {
                    HandleReplicationAction(act);
                }
            }).AddTo(gameObject);

            dispatcher.Replication.Received.ObserveOnMainThread().Subscribe(packet =>
            {
                HandleReplicationAction(packet);
            });
        }

        void HandleReplicationAction(ReplicationActionPacket packet)
        {
            switch (packet.Action)
            {
                case ReplicationAction.Create:
                    HandleReplicationCreate(packet);
                    break;
                case ReplicationAction.Update:
                    HandleReplicationUpdate(packet);
                    break;
                case ReplicationAction.Remove:
                    HandleReplicationRemove(packet);
                    break;
                default:
                    break;
            };
        }

        void HandleReplicationCreate(ReplicationActionPacket packet)
        {
            //Debug.Log($"replicaiton create id={packet.ID}");

            var info = ConnectionInfo.Info;
            var myid = info.PlayerID;
            var id = packet.ID;
            var pos = packet.Pos.ToVector3();

            if (packet.ActorType == ActorType.Player)
            {
                var player = CreatePlayer(id, pos);
                player.ApplyReplication(packet);
            }
            else if(packet.ActorType == ActorType.Food)
            {
                CreateFood(id, pos);
            }
        }

        void HandleReplicationUpdate(ReplicationActionPacket packet)
        {
            //Debug.Log($"replicaiton update id={packet.ID}");

            var id = packet.ID;
            if(packet.ActorType == ActorType.Player)
            {
                var player = playerTable[id];
                player.ApplyReplication(packet);
            }
        }

        void HandleReplicationRemove(ReplicationActionPacket packet)
        {
            //Debug.Log($"replicaiton remove id={packet.ID}");

            var info = ConnectionInfo.Info;
            var myid = info.PlayerID;
            var id = packet.ID;

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
            var info = ConnectionInfo.Info;
            var myid = info.PlayerID;
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
    }
}
