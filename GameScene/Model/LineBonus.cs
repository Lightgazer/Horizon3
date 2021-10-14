using Microsoft.Xna.Framework;
using System;

namespace Horizon3.GameScene.Model
{
    public class LineBonus : Bonus
    {
        public readonly bool Vertical;

        public LineBonus(bool vertical)
        {
            Vertical = vertical;
        }

        protected override void FindTarget(Action<Point> callback)
        {
            var line = Vertical ? Target.X : Target.Y;
            for(int i = 0; i < GameModel.GridSize; i++)
            {
                var target = Vertical ? new Point(line, i) : new Point(i, line);
                callback(target);
            }
        }
    }
}
