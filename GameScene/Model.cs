﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public class BlockData
    {
        public int Type { get; set; }
        public bool Alive { get; set; } = true;
        /// <summary>
        /// Флаг которым пемечается последний передвинутый блок, на его месте может возникнуть бонус.
        /// </summary>
        public bool Suspect { get; set; }
        public Bonus Bonus { get; set; }
    }

    public abstract class Turn { }

    public class AnimationTurn : Turn
    {
        public AnimationTurn(BlockData[,] blocks, List<Bonus> bonuses, List<Point> dead) { }
    }

    public class DropTurn : Turn
    {
        public DropTurn(BlockData[,] blocks, List<Point> drop) { }
    }

    public class IdleTurn : Turn
    {
        public IdleTurn(BlockData[,] blocks) { }
    }

    public class SwapTurn : Turn
    {
        public SwapTurn(BlockData[,] blocks, Point first, Point second) { }
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

    /// <summary>
    /// Модель отвечает исполнение правил игры. Игра представлена поледовательностью ходов(раундов)
    /// разного вида, генерируемых моделью.
    /// </summary>
    public class Model
    {
        private const int GridSize = GameSettings.GridSize;
        private static readonly Random Random = new Random();

        public int Score { get; private set; } = 0;

        //индексы которые умерли в текущем раунде от матчей, без учёта бонусов
        //public List<Point> Dead { get; private set; }
        //список бонусов сработавших в этом раунде
        //public List<Bonus> Bonuses { get; private set; }
        //список блоков которые падают в начале текущего раунда
        //public List<Point> Drop { get; private set; }
        //Игровое поле после смертей, падений и бонусов в текущем раунде
        private readonly BlockData[,] _blocks = new BlockData[GridSize, GridSize];
        private readonly IEnumerator<Turn> _enumerator;

        private SwapInfo _swap;

        public Model()
        {
            _blocks.ForEach((x, y) => _blocks[x, y] = CreateBlock());
            var iter = TurnIterator();
            _enumerator = iter.GetEnumerator();
        }

        public static bool IsIndexInBounds(Point index)
            => index.X >= 0 && index.X < GridSize && index.Y >= 0 && index.Y < GridSize;

        public Turn GetNextTurn()
        {
            _enumerator.MoveNext();
            return _enumerator.Current;
        }

        public bool SwapBlocks(Point first, Point second)
        {
            if (IsSwapAllowed(first, second))
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

        private IEnumerable<Turn> TurnIterator()
        {
            while (true)
            {
                var matches = FindMatches();
                if (matches.Count > 0)
                {
                    var bonuses = CollectBonuses(matches);
                    Score += ExecuteMatches(matches);
                    var dead = CollectDead();
                    Score += ExecuteBonuses(bonuses);
                    yield return new AnimationTurn(_blocks, bonuses, dead);
                    while (!IsAllBlocksAlive())
                    {
                        var drop = MakeDropList();
                        yield return new DropTurn(_blocks, drop);
                        DropBlocks(drop);
                    }
                }
                else if (_swap is { })
                {
                    ReturnSwapedBlocks();
                    yield return new SwapTurn(_blocks, _swap.Second, _swap.First);
                    _swap = null;
                }
                ReleaseSuspects();

                while (_swap is null) yield return new IdleTurn(_blocks);

                yield return new SwapTurn(_blocks, _swap.First, _swap.Second);
            }
        }

        private static bool IsSwapAllowed(Point first, Point second)
        {
            if (first.X == second.X)
                return Math.Abs(first.Y - second.Y) == 1;
            if (first.Y == second.Y)
                return Math.Abs(first.X - second.X) == 1;
            return false;
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
                    var (x, y) = vertical ? (j, k) : (k, j);
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

        private List<Bonus> CollectBonuses(List<MatchChain> matches)
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
            => new BlockData { Type = Random.Next() };


        private List<Point> CollectDead()
        {
            var list = new List<Point>();
            _blocks.ForEach((block, point) =>
            {
                if (!block.Alive) list.Add(point);
                if (block.Bonus is { }) block.Alive = true;     //воскрешаем блоки получившие бонус
            });
            return list;
        }

        ///<returns>Очки за исполнение бонусов, учитываются только живые блоки</returns>
        private int ExecuteBonuses(List<Bonus> list)
           => list.Select(bonus => bonus.Execute(_blocks)).Sum();

        private List<Point> MakeDropList()
        {
            var list = new List<Point>();
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
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
            });
            list
                .Select(point => point.X)
                .Distinct()
                .ToList()
                .ForEach(x => _blocks[x, 0] = CreateBlock());
        }
    }
}
