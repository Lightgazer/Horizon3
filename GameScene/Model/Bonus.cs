using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene.Model
{
    public abstract class Bonus
    {
        public Point Target { get; set; }
        public readonly List<Point> Dead = new List<Point>();
        public readonly List<Bonus> Childs = new List<Bonus>();

        private int _score = 0;

        public int Execute(BlockData[,] blocks)
        {
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
                var block = blocks[target.X, target.Y];
                block.Alive = false;
                Dead.Add(target);
                if (block.Bonus is { }) Childs.Add(block.Bonus);
            }
        }

        private static bool CheckTarget(Point index, BlockData[,] blocks)
            => GameModel.IsIndexInBounds(index) && blocks[index.X, index.Y].Alive;
    }
}
