using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Horizon3.GameScene.Model;

namespace Horizon3.GameScene
{
    public abstract class GameState
    {
        protected const int BlockSize = GameSettings.BlockSize;
        private const int sideLength = GameModel.GridSize * BlockSize;

        public static Vector2 Padding = new Point((GameSettings.Width - sideLength) / 2, (GameSettings.Height - sideLength) / 2).ToVector2();
        protected static Vector2 Origin = new Vector2(GameSettings.BlockSize / 2);

        protected readonly Texture2D[] BlockTextures;
        protected readonly Rectangle GridRectangle;

        private readonly Texture2D _BombTexture;
        private readonly Texture2D _LineTexture;

        public GameState(ContentManager content)
        {
            BlockTextures = LoadBlockTextures(content);
            GridRectangle = new Rectangle((int)Padding.X, (int)Padding.Y, sideLength, sideLength);
            _BombTexture = content.Load<Texture2D>("bonuses/bomb");
            _LineTexture = content.Load<Texture2D>("bonuses/line");
        }

        public abstract void Update(GameTime gameTime, GameGrid context);

        public abstract void Draw(SpriteBatch spriteBatch);

        protected void DrawBonusIcon(SpriteBatch spriteBatch, BlockData block, Vector2 position)
        {
            var texture = block.Bonus switch
            {
                LineBonus _ => _LineTexture,
                BombBonus _ => _BombTexture,
                null => null,
                _ => throw new NotImplementedException("bonus icon missing")
            };
            if (texture is { }) spriteBatch.Draw(texture, position, Color.White);
        }

        private static Texture2D[] LoadBlockTextures(ContentManager content)
        {
            var textures = new Texture2D[GameModel.NumberOfBlockTypes];
            for (var index = 0; index < GameModel.NumberOfBlockTypes; index++)
            {
                textures[index] = content.Load<Texture2D>("blocks/block" + index.ToString());
            }
            return textures;
        }
    }
}
