using Microsoft.Xna.Framework;
using System;

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
            var delta = MathF.Sign(target - current) * maxDelta;
            return current + delta;
        }
    }
}