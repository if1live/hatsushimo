using UnityEngine;
using UniRx.Triggers;
using UniRx;
using System;
using Hatsushimo.Utils;

namespace Assets.Game
{
    public class Projectile : MonoBehaviour
    {
        public int id;
        public Vector3 finalPosition;

        // 투사체가 날아가는 시간
        public float moveTime;

        // 투사체가 멈춘후 살아있는 시간
        public float lifeTime;

        public Vector3 velocity;

        public void Subscribe()
        {
            var moveFinishStream = Observable.Timer(TimeSpan.FromSeconds(moveTime));
            var lifeFinishStream = Observable.Timer(TimeSpan.FromSeconds(lifeTime));

            // 정지시점이 되기전까지 이동
            gameObject.UpdateAsObservable().TakeUntil(moveFinishStream).Subscribe(_ =>
            {
                var dt = Time.deltaTime;
                var curr = transform.localPosition;
                var diff = velocity * dt;
                var next = curr + diff;
                transform.localPosition = next;
            });

            // 정지시점이 되면 최종 위치로 확실히 이동
            moveFinishStream.First().Subscribe(_ =>
            {
                transform.localPosition = finalPosition;
            }).AddTo(this);

            // 살아있는 시간이 끝나면 정지
            lifeFinishStream.First().Subscribe(_ =>
            {
                var mgr = ReplicationManager.Instance;
                mgr.Remove(id);
                Destroy(gameObject);
                //Debug.Log($"projectile destory id={id} ts={TimeUtils.NowTimestamp}");
            }).AddTo(this);
        }
    }
}
