using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Horizon3.GameScene
{
    public abstract class GameState
    {
        protected const int BlockSize = GameSettings.BlockSize;
        private const int SideLength = GameModel.GridSize * BlockSize;

        public static Vector2 Padding = new Point((GameSettings.Width - SideLength) / 2, (GameSettings.Height - SideLength) / 2).ToVector2();
        public static Vector2 Origin = new Vector2(BlockSize / 2);

        protected readonly Texture2D[] BlockTextures;
        protected readonly Rectangle GridRectangle;

        private readonly Texture2D _bombTexture;
        private readonly Texture2D _lineTexture;

        protected GameState(ContentManager content)
        {
            BlockTextures = LoadBlockTextures(content);
            GridRectangle = new Rectangle((int)Padding.X, (int)Padding.Y, SideLength, SideLength);
            _bombTexture = content.Load<Texture2D>("bonuses/bomb");
            _lineTexture = content.Load<Texture2D>("bonuses/line");
        }

        public abstract void Update(GameTime gameTime, GameContext context);

        public abstract void Draw(SpriteBatch spriteBatch);

        protected void DrawBonusIcon(SpriteBatch spriteBatch, IReadOnlyBlock block, Vector2 position)
        {
            var texture = block.Bonus switch
            {
                LineBonus _ => _lineTexture,
                BombBonus _ => _bombTexture,
                null => null,
                _ => throw new NotImplementedException("bonus icon missing")
            };
            var rotation = 0f;
            if (block.Bonus is LineBonus line && line.Vertical == false)
                rotation = 1.57f;

            if (texture is { })
                spriteBatch.Draw(texture, position + Origin, null, Color.White, rotation, Origin, 
                    1, SpriteEffects.None, 0f);
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
