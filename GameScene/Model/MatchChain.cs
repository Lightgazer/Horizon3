using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene.Model
{
    public class MatchChain
    {
        public readonly List<BlockData> Blocks;
        public readonly bool Vertical;

        public MatchChain(bool vertical)
        {
            Blocks = new List<BlockData>();
            Vertical = vertical;
        }

        public void Add(BlockData block) => Blocks.Add(block);

        public int Execute()
        {
            int score = Blocks.Where(block => block.Alive).Count();
            Blocks.ForEach(block =>
            {
                Bonus bonus = null;
                if (!block.Alive)
                {
                    bonus = new BombBonus();
                }
                else if (block.Suspect && Blocks.Count > 3)
                {
                    if (Blocks.Count > 4)
                        bonus = new BombBonus();
                    else
                        bonus = new LineBonus(Vertical);
                }

                block.Alive = false;
                block.Bonus = bonus;
            });
            return score;
        }
    }
}
