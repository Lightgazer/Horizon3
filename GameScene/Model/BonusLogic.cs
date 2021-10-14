using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene.Model
{
    public abstract class BonusLogic
    {
        public Point Target { get; set; }
        public readonly List<Point> Dead = new List<Point>();
        public readonly List<BonusLogic> Childs = new List<BonusLogic>();

        private int _score;

        public int Execute(BlockData[,] blocks)
        {
            _score = 0;
            FindTarget(target => OnEachTarget(target, blocks));

            _score += Childs.Select(bonus => bonus.Execute(blocks)).Sum();
            return _score;
        }

        protected abstract void FindTarget(Action<Point> callback);

        private void OnEachTarget(Point target, BlockData[,] blocks)
        {
            if (CheckTarget(target, blocks))
            {
                _score++;
                var block = blocks.GetValue(target);
                block.Alive = false;
                Dead.Add(target);
                if (block.Bonus is { }) Childs.Add(block.Bonus);
            }
        }

        private static bool CheckTarget(Point index, BlockData[,] blocks)
            => GameModel.IsIndexInBounds(index) && blocks[index.X, index.Y].Alive;
    }
}
