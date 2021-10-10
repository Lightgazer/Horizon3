using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene.Bonuses
{
    public class BombBonus : Bonus
    {
        protected override void FindTarget(Action<Point> callback)
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    var target = new Point(Target.X + x, Target.Y + y);
                    callback(target);
                }
            }
        }
    }
}
