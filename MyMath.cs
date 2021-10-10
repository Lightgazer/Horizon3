using System;
using Microsoft.Xna.Framework;

namespace Horizon3
{
    public static class MyMath
    {
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathF.Abs(target - current) <= maxDelta)
            {
                return target;
            }
            return current + MathF.Sign(target - current) * maxDelta;
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            var toVector = target - current;

            var sqDist = toVector.X * toVector.X + toVector.Y * toVector.Y;

            if (MathF.Abs(sqDist) < float.Epsilon || (maxDistanceDelta >= 0 && sqDist <= maxDistanceDelta * maxDistanceDelta))
                return target;

            var dist = MathF.Sqrt(sqDist);

            return new Vector2(
                current.X + toVector.X / dist * maxDistanceDelta,
                current.Y + toVector.Y / dist * maxDistanceDelta
            );
        }
    }
}