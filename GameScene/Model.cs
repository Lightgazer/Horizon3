using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public class BlockData
    {
        public int Type { get; set; }
        public bool Alive { get; set; } = true;
        public bool Suspect { get; set; }
        public Bonus Bonus { get; set; }
    }

    public class Model
    {
        public const int GridSize = GameSettings.GridSize;
        private static readonly Random Random = new Random();

        public int Score { get; private set; } = 0;

        //индексы которые умерли в текущем раунде от матчей, без учёта бонусов
        public List<Point> Dead { get; private set; }
        //список бонусов сработавших в этом раунде
        public List<Bonus> Bonuses {  get; private set; }
        //список блоков которые падают в начале текущего раунда
        public List<Point> Drop { get; private set; }
        //Игровое поле после смертей, падений и бонусов в текущем раунде
        public BlockData[,] Blocks { get; } = new BlockData[GridSize, GridSize];

        public Model()
        {
            Blocks.ForEach((x, y) => Blocks[x, y] = CreateBlock());
            NextTurn();
        }

        public static bool IsIndexInBounds(Point index)
            => index.X >= 0 && index.X < GridSize && index.Y >= 0 && index.Y < GridSize;

        public void NextTurn()
        {
            if (IsIdle())
            {
                var matches = FindMatches();
                Bonuses = CollectBonuses(matches);
                Score += ExecuteMatches(matches);
                Dead = CollectDead();
                Score += ExecuteBonuses();
            }
            Drop = MakeDropList();
            DropBlocks();
        }

        public bool IsIdle()
            => Blocks.Cast<BlockData>().All(block => block != null && block.Alive == true);

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
                    var block = Blocks[x, y];
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
            Blocks.ForEach((block, point) => { 
                if (!block.Alive) list.Add(point);
                if (block.Bonus is { }) block.Alive = true;     //воскрешаем блоки получившие бонус
            });
            return list;
        }

        ///<returns>Очки за исполнение бонусов, учитываются только живые блоки</returns>
        private int ExecuteBonuses()
           => Bonuses.Select(bonus => bonus.Execute(Blocks)).Sum();

        private List<Point> MakeDropList()
        {
            var list = new List<Point>();
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (!Blocks[x, y].Alive)
                    {
                        list.Add(new Point(x, y));
                        break;
                    }
                }
            }
            return list;
        }

        private void DropBlocks()
        {
            Drop.ForEach(point =>
            {
                var (x, y) = point;
                while (y > 0)
                {
                    Blocks[x, y] = Blocks[x, y - 1];
                    y--;
                }

                Blocks[x, 0] = CreateBlock();
            });
        }
    }
}
