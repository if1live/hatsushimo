using Assets.Game.Extensions;
using Assets.NetChan;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
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
        public Projectile prefab_projectile;

        Dictionary<int, Food> foodTable = new Dictionary<int, Food>();
        Dictionary<int, Player> playerTable = new Dictionary<int, Player>();
        Dictionary<int, Projectile> projectileTable = new Dictionary<int, Projectile>();

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            var conn = ConnectionManager.Instance;

            conn.PlayerReady.Received.Subscribe(_ =>
            {
                Debug.Log("player ready");
            });

            conn.ReplicationAll.Received.Subscribe(packet =>
            {
                //Debug.Log("replicaiton all received");
                foreach (var p in packet.Players) { CreatePlayer(p); }
                foreach (var f in packet.Foods) { CreateFood(f); }
                foreach (var p in packet.Projectiles) { CreateProjectile(p); }
            }).AddTo(this);

            conn.CreateFood.Received.Subscribe(p => CreateFood(p.status));
            conn.CreatePlayer.Received.Subscribe(p => CreatePlayer(p.status));
            conn.CreateProjectile.Received.Subscribe(p => CreateProjectile(p.status));

            conn.ReplicationRemove.Received.Subscribe(p => Remove(p.ID));
            conn.ReplicationBulkRemove.Received.Subscribe(packet =>
            {
                foreach (var id in packet.IDList) { Remove(id); }
            });
        }

        public Food CreateFood(FoodStatus status)
        {
            var id = status.ID;
            Debug.Assert(foodTable.ContainsKey(id) == false, $"food={id} already exist in food table");
            var food = Instantiate(prefab_food);
            foodTable[id] = food;
            food.transform.SetParent(transform);
            food.id = id;
            food.transform.position = status.Pos.ToVector3();
            return food;
        }

        public Player CreatePlayer(PlayerStatus status)
        {
            var id = status.ID;
            var pos = status.Pos.ToVector3();
            var player = GetOrCreatePlayer(id, pos);
            player.ApplyStatus(status);
            return player;
        }

        Projectile CreateProjectile(ProjectileStatus status)
        {
            //Debug.Log($"create projectile id={status.ID} ts={TimeUtils.NowTimestamp}");

            var id = status.ID;
            Debug.Assert(projectileTable.ContainsKey(id) == false, $"projectile={id} already exists in projectile table");
            var projectile = Instantiate(prefab_projectile);
            projectileTable[id] = projectile;
            projectile.transform.SetParent(transform);
            projectile.id = id;
            projectile.transform.position = status.Position.ToVector3();
            projectile.finalPosition = status.FinalPosition.ToVector3();
            projectile.velocity = status.Direction.ToVector3() * Config.ProjectileSpeed;
            projectile.moveTime = status.MoveTimeMillis * 0.001f;
            projectile.lifeTime = status.LifeTimeMillis * 0.001f;
            projectile.Subscribe();
            return projectile;
        }

        public void Remove(int id)
        {
            if (foodTable.ContainsKey(id)) { RemoveFood(id); }
            if (playerTable.ContainsKey(id)) { RemovePlayer(id); }
            if (projectileTable.ContainsKey(id)) { RemoveProjectile(id); }
        }

        void RemoveFood(int id)
        {
            Debug.Assert(foodTable.ContainsKey(id) == true, $"food={id} not exist in food table");
            var item = foodTable[id];
            foodTable.Remove(id);
            Destroy(item.gameObject);
        }

        void RemoveProjectile(int id)
        {
            Debug.Assert(projectileTable.ContainsKey(id) == true, $"projectile={id} not exist in projectile table");
            var found = projectileTable[id];
            projectileTable.Remove(id);
            Destroy(found.gameObject);
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

        public bool TryGetPlayer(int id, out Player player)
        {
            return playerTable.TryGetValue(id, out player);
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
