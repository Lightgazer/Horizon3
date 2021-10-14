using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Horizon3.GameScene.Model
{
    public interface ITurn { }

    public class AnimationTurn : ITurn
    {
        public readonly ReadOnlyBlocks Blocks;
        /// <summary> Список бонусов сработавших в этом раунде. </summary>
        public readonly List<Bonus> Bonuses;
        /// <summary> Индексы которые умерли в текущем раунде от матчей, без учёта бонусов. </summary>
        public readonly List<Point> Dead;
        public AnimationTurn(BlockData[,] blocks, List<Bonus> bonuses, List<Point> dead)
        {
            Blocks = new ReadOnlyBlocks(blocks);
            Bonuses = bonuses;
            Dead = dead;
        }
    }

    public class DropTurn : ITurn
    {
        public readonly ReadOnlyBlocks Blocks;
        /// <summary> Список блоков которые падают в текущем раунде. </summary>
        public readonly List<Point> Drop;
        public DropTurn(BlockData[,] blocks, List<Point> drop)
        {
            Blocks = new ReadOnlyBlocks(blocks);
            Drop = drop;
        }
    }

    public class IdleTurn : ITurn
    {
        public readonly ReadOnlyBlocks Blocks;
        public IdleTurn(BlockData[,] blocks)
        {
            Blocks = new ReadOnlyBlocks(blocks);
        }
    }

    public class SwapTurn : ITurn
    {
        public readonly ReadOnlyBlocks Blocks;
        public readonly Point First;
        public readonly Point Second;
        public SwapTurn(BlockData[,] blocks, Point first, Point second)
        {
            Blocks = new ReadOnlyBlocks(blocks);
            First = first;
            Second = second;
        }
    }
}
