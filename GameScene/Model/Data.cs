using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Horizon3.GameScene.Model
{
    public interface ITurn { }

    public class AnimationTurn : ITurn
    {
        public readonly BlockData[,] Blocks;
        /// <summary> Список бонусов сработавших в этом раунде. </summary>
        public readonly List<BonusLogic> Bonuses;
        /// <summary> Индексы которые умерли в текущем раунде от матчей, без учёта бонусов. </summary>
        public readonly List<Point> Dead;
        public AnimationTurn(BlockData[,] blocks, List<BonusLogic> bonuses, List<Point> dead)
        {
            Blocks = blocks;
            Bonuses = bonuses;
            Dead = dead;
        }
    }

    public class DropTurn : ITurn
    {
        public readonly BlockData[,] Blocks;
        /// <summary> Список блоков которые падают в текущем раунде. </summary>
        public readonly List<Point> Drop;
        public DropTurn(BlockData[,] blocks, List<Point> drop)
        {
            Blocks = blocks;
            Drop = drop;
        }
    }

    public class IdleTurn : ITurn
    {
        public readonly BlockData[,] Blocks;
        public IdleTurn(BlockData[,] blocks)
        {
            Blocks = blocks;
        }
    }

    public class SwapTurn : ITurn
    {
        public readonly BlockData[,] Blocks;
        public readonly Point First;
        public readonly Point Second;
        public SwapTurn(BlockData[,] blocks, Point first, Point second)
        {
            Blocks = blocks;
            First = first;
            Second = second;
        }
    }

    public class BlockData
    {
        public int Type { get; set; }
        public bool Alive { get; set; } = true;
        /// <summary>
        /// Флаг которым пемечается последний передвинутый блок, на его месте может возникнуть бонус.
        /// </summary>
        public bool Suspect { get; set; }
        public BonusLogic Bonus { get; set; }
    }

    public class SwapInfo
    {
        public readonly Point First;
        public readonly Point Second;

        public SwapInfo(Point first, Point second)
        {
            First = first;
            Second = second;
        }
    }
}
