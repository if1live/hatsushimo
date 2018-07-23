using Hatsushimo.Types;
using UnityEngine;

namespace Assets.Game.Extensions
{
    public static class Vec2Extensions
    {
        public static Vector3 ToVector3(this Vec2 v)
        {
            return new Vector3(v.X, v.Y);
        }
    }
}
