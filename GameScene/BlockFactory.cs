using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace Horizon3.GameScene
{
    public class BlockFactory {
        public const int NumberOfBlockTypes = 5;
        private static readonly Random Random = new Random();
        private readonly Texture2D[] _blockTextures;

        public BlockFactory(ContentManager content) {
            _blockTextures = LoadBlockTextures(content);
        }

        public Block CreateBlock()
        {
            var type = Random.Next(NumberOfBlockTypes);
            return new Block(type, _blockTextures[type]);
        }

        private static Texture2D[] LoadBlockTextures(ContentManager content)
        {
            var textures = new Texture2D[NumberOfBlockTypes];
            for (var index = 0; index < NumberOfBlockTypes; index++)
            {
                textures[index] = content.Load<Texture2D>("blocks/block" + index.ToString());
            }
            return textures;
        }
    }
}
