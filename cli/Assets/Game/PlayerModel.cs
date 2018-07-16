using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Assets.Game
{
    class PlayerModel
    {
        public readonly static PlayerModel Instance = new PlayerModel();

        public string PlayerID { get; set; }
        public string RoomID { get; set; }
        public string Nickname { get; set; }

        // TODO float? reactive programming?
        public float PosX { get; set; }
        public float PosY { get; set; }

        public ReactiveProperty<bool> IsReady {
            get { return _isReady; }
        }
        ReactiveProperty<bool> _isReady = new ReactiveProperty<bool>(false);

        public PlayerModel()
        {
            Reset();
        }

        public void Reset()
        {
            PlayerID = "";
            RoomID = "";
            Nickname = "";
            IsReady.Value = false;
            PosX = 0;
            PosY = 0;
        }
    }
}
