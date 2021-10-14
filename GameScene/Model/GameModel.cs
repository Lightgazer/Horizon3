using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene.Model
{
    /// <summary>
    /// Пассивная модель. Модель отвечает исполнение правил игры. Игра представлена 
    /// поледовательностью ходов(раундов) разного типа, генерируемых моделью.
    /// </summary>
    public class GameModel
    {
        public const int NumberOfBlockTypes = 5;
        public const int GridSize = 8;
        private static readonly Random Random = new Random();

        public int Score { get; private set; } = 0;

        private readonly BlockData[,] _blocks = new BlockData[GridSize, GridSize];
        private readonly IEnumerator<ITurn> _enumerator;
        private SwapInfo _swap;

        public GameModel()
        {
            _blocks.ForEach((x, y) => _blocks[x, y] = CreateBlock());
            var iter = TurnIterator();
            _enumerator = iter.GetEnumerator();
        }

        public static bool IsIndexInBounds(Point index)
            => index.X >= 0 && index.X < GridSize && index.Y >= 0 && index.Y < GridSize;

        public ITurn GetNextTurn()
        {
            _enumerator.MoveNext();
            return _enumerator.Current;
        }

        public bool SwapBlocks(Point first, Point second)
        {
            if (IsAllBlocksAlive() && IsSwapAllowed(first, second))
            {
                var block1 = _blocks.GetValue(first);
                var block2 = _blocks.GetValue(second);
                block1.Suspect = true;
                block2.Suspect = true;
                _blocks.SetValue(first, block2);
                _blocks.SetValue(second, block1);
                _swap = new SwapInfo(first, second);
                return true;
            }
            return false;
        }

        private static bool IsSwapAllowed(Point first, Point second)
        {
            if (first.X == second.X)
                return Math.Abs(first.Y - second.Y) == 1;
            if (first.Y == second.Y)
                return Math.Abs(first.X - second.X) == 1;
            return false;
        }

        private IEnumerable<ITurn> TurnIterator()
        {
            while (true)
            {
                var matches = FindMatches();
                if (matches.Count > 0)
                {
                    while (matches.Count > 0)
                    {
                        var bonuses = CollectBonuses(matches);
                        Score += ExecuteMatches(matches);
                        var dead = CollectDead();
                        Score += ExecuteBonuses(bonuses);
                        yield return new AnimationTurn(_blocks, bonuses, dead);
                        RestoreBonusBlocks(dead);
                        CreateBlocksInFirstRow();
                        var drop = MakeDropList();
                        while (drop.Count > 0)
                        {
                            yield return new DropTurn(_blocks, drop);
                            DropBlocks(drop);
                            CreateBlocksInFirstRow();
                            drop = MakeDropList();
                        }
                        matches = FindMatches();
                    }
                }
                else if (_swap is { })
                {
                    ReturnSwapedBlocks();
                    yield return new SwapTurn(_blocks, _swap.Second, _swap.First);
                }
                _swap = null;
                ReleaseSuspects();

                while (_swap is null) yield return new IdleTurn(_blocks);

                yield return new SwapTurn(_blocks, _swap.First, _swap.Second);
            }
        }

        private void ReturnSwapedBlocks()
        {
            var block1 = _blocks.GetValue(_swap.First);
            var block2 = _blocks.GetValue(_swap.Second);
            _blocks.SetValue(_swap.First, block2);
            _blocks.SetValue(_swap.Second, block1);
        }

        private void ReleaseSuspects()
        {
            _blocks.ForEach(block => block.Suspect = false);
        }

        private bool IsAllBlocksAlive()
            => _blocks.Cast<BlockData>().All(block => block.Alive);

        private List<MatchChain> FindMatches()
        {
            var result = new List<MatchChain>();
            FindMatches(result, true);
            FindMatches(result, false);
            return result.Where(chain => chain.Blocks.Count > 2).ToList();
        }

        private void FindMatches(List<MatchChain> result, bool vertical)
        {
            for (var k = 0; k < GridSize; k++)
            {
                var currentType = -1;
                var matchChain = new MatchChain(vertical);
                for (var j = 0; j < GridSize; j++)
                {
                    var (x, y) = vertical ? (k, j) : (j, k);
                    var block = _blocks[x, y];
                    if (block.Bonus is { }) block.Bonus.Target = new Point(x, y);
                    if (currentType == block.Type)
                    {
                        matchChain.Add(block);
                    }
                    else
                    {
                        result.Add(matchChain);
                        currentType = block.Type;
                        matchChain = new MatchChain(vertical);
                        matchChain.Add(block);
                    }
                }

                result.Add(matchChain);
            }
        }

        private List<BonusLogic> CollectBonuses(List<MatchChain> matches)
        {
            return matches
                .SelectMany(chain => chain.Blocks)
                .Where(block => block.Bonus is { })
                .Select(block => block.Bonus)
                .ToList();
        }

        ///<summary>Элементы папавшие в матч теряют статус живых</summary>
        ///<returns>Очки за исполнение матчей, с учётом их возможных пересечений</returns>
        private int ExecuteMatches(List<MatchChain> matches)
            => matches.Select(match => match.Execute()).Sum();

        private static BlockData CreateBlock()
            => new BlockData { Type = Random.Next(NumberOfBlockTypes) };


        private List<Point> CollectDead()
        {
            var list = new List<Point>();
            _blocks.ForEach((block, point) =>
            {
                if (!block.Alive) list.Add(point);
            });
            return list;
        }

        /// <summary>
        /// Из списка попавших в матч восстонавливает тех кто получил бонус.
        /// </summary>
        private void RestoreBonusBlocks(List<Point> dead)
        {
            dead.ForEach(index => {
                var block = _blocks.GetValue(index);
                if (block.Bonus is { }) block.Alive = true;
            });
        }

        ///<returns>Очки за исполнение бонусов, учитываются только живые блоки</returns>
        private int ExecuteBonuses(List<BonusLogic> list)
           => list.Select(bonus => bonus.Execute(_blocks)).Sum();

        private List<Point> MakeDropList()
        {
            var list = new List<Point>();
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 1; y < GridSize; y++)
                {
                    if (!_blocks[x, y].Alive)
                    {
                        while (y > 0)
                            list.Add(new Point(x, --y));
                        break;
                    }
                }
            }
            return list;
        }

        private void DropBlocks(List<Point> list)
        {
            list.ForEach(point =>
            {
                var (x, y) = point;
                _blocks[x, y + 1] = _blocks[x, y];
                _blocks[x, y] = null;
            });
        }

        private void CreateBlocksInFirstRow()
        {
            for (int x = 0; x < GridSize; x++)
            {
                var block = _blocks[x, 0];
                if (block == null || block.Alive == false)
                    _blocks[x, 0] = CreateBlock();
            }
        }
    }
}
