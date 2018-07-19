using System;
using System.Collections.Generic;
using Assets.Game.Packets;
using UniRx;
using UnityEngine;

namespace Assets.Game
{
    class ItemManager : MonoBehaviour
    {
        public Food prefab_food;

        Dictionary<int, Food> itemTable = new Dictionary<int, Food>();

        IObservable<StaticItemListPacket> ListObservable {
            get { return list.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<StaticItemListPacket> list = new ReactiveProperty<StaticItemListPacket>(null);

        IObservable<StaticItemCreatePacket> CreateObservable {
            get { return create.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<StaticItemCreatePacket> create = new ReactiveProperty<StaticItemCreatePacket>(null);

        IObservable<StaticItemRemovePacket> RemoveObservable {
            get { return remove.Where(x => x != null).AsObservable(); }
        }
        ReactiveProperty<StaticItemRemovePacket> remove = new ReactiveProperty<StaticItemRemovePacket>(null);

        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;

            conn.On<StaticItemListPacket>(Events.STATIC_ITEM_LIST, (p) => list.Value = p);
            conn.On<StaticItemCreatePacket>(Events.STATIC_ITEM_CREATE, (p) => create.Value = p);
            conn.On<StaticItemRemovePacket>(Events.STATIC_ITEM_REMOVE, (p) => remove.Value = p);

            ListObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                foreach(var p in packet.items)
                {
                    CreateItem(p);
                }
            }).AddTo(gameObject);

            CreateObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                Debug.Assert(itemTable.ContainsKey(packet.id) == false, "already id exist in item table");
                CreateItem(packet);
            }).AddTo(gameObject);

            RemoveObservable.ObserveOnMainThread().Subscribe(packet =>
            {
                Debug.Assert(itemTable.ContainsKey(packet.id) == true, "id not exist in item table");
                RemoveItem(packet);
            }).AddTo(gameObject);
        }

        void CreateItem(StaticItemCreatePacket packet)
        {
            if(packet.type == "food")
            {
                CreateFood(packet);
            }
        }

        void CreateFood(StaticItemCreatePacket packet)
        {
            var item = Instantiate(prefab_food);
            itemTable[packet.id] = item;
            item.transform.SetParent(transform);
            item.id = packet.id;

            var pos = new Vector3(packet.pos_x, packet.pos_y, 0);
            item.transform.position = pos;
        }

        void RemoveItem(StaticItemRemovePacket packet)
        {
            var item = itemTable[packet.id];
            itemTable.Remove(packet.id);
            Destroy(item.gameObject);
        }

        private void OnDestroy()
        {
            var mgr = ConnectionManager.Instance;
            if(!mgr) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.STATIC_ITEM_LIST);
            conn.Off(Events.STATIC_ITEM_CREATE);
            conn.Off(Events.STATIC_ITEM_REMOVE);
        }
    }
}
