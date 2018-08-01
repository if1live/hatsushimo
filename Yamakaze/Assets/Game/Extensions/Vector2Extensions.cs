using UnityEngine;

namespace Assets.Game.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector3 ToVector3(this System.Numerics.Vector2 v)
        {
            return new Vector3(v.X, v.Y);
        }
    }
}
