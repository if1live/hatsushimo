using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.InputSystem
{
    class CommandButton : MonoBehaviour
    {
        public int mode = 1;
        public float cooltime = 1.0f;

        TimeSpan CoolTimeSpan { get { return TimeSpan.FromSeconds(cooltime); } }

        IObservable<bool> ReadyObservable {
            get { return _fired.Where(x => x == true).Delay(CoolTimeSpan).AsObservable(); }
        }
        ReactiveProperty<bool> _fired = new ReactiveProperty<bool>();

        public Button button = null;

        private void Awake()
        {
            Debug.Assert(button != null);
        }

        private void Start()
        {
            button.OnClickAsObservable()
                .ThrottleFirst(CoolTimeSpan)
                .Subscribe(_ =>
            {
                var action = InputAction.CreateCommand(mode);
                var mgr = InputManager.Instance;
                mgr.PushCommand(action);

                // 연타 방지
                button.interactable = false;
                _fired.Value = true;
            });

            // TODO 사용 가능시간이 된것은 서버에서 신호 주는게 더 좋을듯
            // 클라에서의 요청을 그대로 쓰면 쿨타임 신뢰할수 없다
            ReadyObservable.Subscribe(_ =>
            {
                button.interactable = true;
                _fired.Value = false;
            });
        }
    }
}
