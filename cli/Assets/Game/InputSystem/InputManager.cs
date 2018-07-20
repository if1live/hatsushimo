using Assets.Game.NetChan;
using Assets.Game.Packets;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Assets.Game.InputSystem
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance = null;

        // action은 될수있는한 빨리 처리되면 좋겠다
        // 예를 들면 공격 명령같은거
        // 자주 발생하지 않으니 즉시 처리해도 부하가 심하지 않을것이다
        // TODO command가 한 프레임에 2개 발생하면 어떻게 처리되는가? 스트림이 알아서 잘 해주나?
        ReactiveProperty<InputAction> command = new ReactiveProperty<InputAction>(InputAction.CreateNone());
        IObservable<InputAction> CommandObservable {
            get { return command.Where(a => a.type != InputActionTypes.None).AsObservable(); }
        }

        // 이동 명령은 적당한 주기로 처리하기
        // 같은 방향으로 계속 이동하고 있으면 반복으로 보내지 않기
        // 방향이 유지되면 갱신할 필요가 없으니까
        Queue<InputAction> moveQueue = new Queue<InputAction>();
        InputAction lastMoveAction = InputAction.CreateMove(0, 0);

        public void PushCommand(InputAction action)
        {
            command.Value = action;
        }

        public void PushMove(InputAction action)
        {
            // 클라에서의 마지막 이동 요청은 서버로 계속 유지된다
            // 방향이 같은 경우 굳이 요청을 다시할 필요 없다
            if(lastMoveAction == action)
            {
                return;
            }

            moveQueue.Enqueue(action);
            lastMoveAction = action;
        }

        private void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
        }

        private void Start()
        {
            var conn = ConnectionManager.Instance.Conn;

            CommandObservable.Subscribe(action =>
            {
                SendCommandPacket(conn, action);
            });
        }

        private void OnDestroy()
        {
            Debug.Assert(Instance == this);
            Instance = null;

            var mgr = ConnectionManager.Instance;
            if (mgr == null) { return; }

            var conn = mgr.Conn;
            conn.Off(Events.INPUT_MOVE);
            conn.Off(Events.INPUT_COMMAND);
        }
        
        void SendMovePacket(Connection conn, InputAction action)
        {
            var p = new InputMovePacket
            {
                dir_x = action.dirx,
                dir_y = action.diry,
            };
            conn.Emit(Events.INPUT_MOVE, p);
        }

        void SendCommandPacket(Connection conn, InputAction action)
        {
            var p = new InputCommandPacket
            {
                mode = action.mode,
            };
            conn.Emit(Events.INPUT_COMMAND, p);
        }
    }
}
