using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Hatsushimo.Types;

namespace Assets.Game.InputSystem
{
    public enum InputActionTypes
    {
        None = 0,
        Move,
        Command,
    }

    public struct InputAction
    {
        public InputActionTypes type;

        // move
        public float dirx;
        public float diry;

        // action
        public int mode;

        public IPacket ToPacket()
        {
            switch(type)
            {
                case InputActionTypes.Command:
                    return new InputCommandPacket
                    {
                        Mode = mode,
                    };
                case InputActionTypes.Move:
                    return new InputMovePacket
                    {
                        Dir = new Vec2(dirx, diry),
                    };
                default:
                    return null;
            }
        }

        public static InputAction CreateNone()
        {
            return new InputAction()
            {
                type = InputActionTypes.None,
            };
        }

        public static InputAction CreateMove(float x, float y)
        {
            return new InputAction()
            {
                type = InputActionTypes.Move,
                dirx = x,
                diry = y,
            };
        }

        public static InputAction CreateCommand(int mode)
        {
            return new InputAction()
            {
                type = InputActionTypes.Command,
                mode = mode,
            };
        }

        // https://stackoverflow.com/questions/25461585/operator-overloading-equals
        public static bool operator==(InputAction a, InputAction b)
        {
            if(ReferenceEquals(a, b)) { return true; }
            if(ReferenceEquals(a, null)) { return false; }
            if(ReferenceEquals(b, null)) { return false; }

            return (a.type == b.type)
                && (a.dirx == b.dirx)
                && (a.diry == b.diry)
                && (a.mode == b.mode);
        }

        public static bool operator!=(InputAction a, InputAction b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((InputAction)obj);
        }

        public bool Equals(InputAction other)
        {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return type.Equals(other.type)
                && dirx.Equals(other.dirx)
                && diry.Equals(other.diry)
                && mode.Equals(other.mode);
        }

        public override int GetHashCode()
        {
            int hash = type.GetHashCode();
            hash = hash ^ dirx.GetHashCode();
            hash = hash ^ diry.GetHashCode();
            hash = hash ^ mode.GetHashCode();
            return hash;
        }

    }
}
