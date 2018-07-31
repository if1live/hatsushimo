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

        public float Lifetime
        {
            set
            {
                _lifetime.Value = value;
                gameObject.UpdateAsObservable().Subscribe(_ =>
                {
                    var dt = Time.deltaTime;
                    _lifetime.Value -= dt;
                });

                _lifetime.AsObservable().SkipWhile(x => x > 0).Subscribe(_ =>
                {
                    var mgr = ReplicationManager.Instance;
                    mgr.Remove(id);
                    Destroy(gameObject);
                    //Debug.Log($"projectile destory id={id} ts={TimeUtils.NowTimestamp}");
                }).AddTo(this);
            }
        }
        [SerializeField]
        FloatReactiveProperty _lifetime = new FloatReactiveProperty();

        public Vector3 Velocity
        {
            set
            {
                _velocity.Value = value;

                gameObject.UpdateAsObservable().Subscribe(_ => {
                    var dt = Time.deltaTime;
                    var curr = transform.localPosition;
                    var diff = _velocity.Value * dt;
                    var next = curr + diff;
                    transform.localPosition = next;
                });
            }
        }
        [SerializeField]
        Vector3ReactiveProperty _velocity = new Vector3ReactiveProperty();
    }
}
