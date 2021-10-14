using Microsoft.Xna.Framework;
using System;

namespace Horizon3.GameScene.Model
{
    /// <summary>
    /// С помощью этого класса модель отдаёт своему пользователю массив блоков
    /// выражая желание что не нужно двигать там блоки и добовлять бонусы. 
    /// </summary>
    public class ReadOnlyBlocks
    {
        private readonly IReadOnlyBlock[,] _blocks;

        public ReadOnlyBlocks(IReadOnlyBlock[,] blocks)
        {
            _blocks = blocks;
        }

        public void ForEach(Action<IReadOnlyBlock, Point> callback)
            => _blocks.ForEach((block, point) => callback(block, point));
    }
}
