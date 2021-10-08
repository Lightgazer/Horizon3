using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Horizon3.GameScene
{
    internal class LineBonus : Bonus
    {
        public bool Vertical { set; get; }
        private float effectPosition = 0;

        public LineBonus(ContentManager content)
        {
            Texture = content.Load<Texture2D>("bonuses/line");
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                effectPosition += (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.BlockSize * 4 * GameSettings.AnimationSpeed;
                var stepsToStart = Vertical ? index.X : index.Y;
                var stepsToEnd = GameSettings.GridSize - stepsToStart;
                var stepsNeeded = stepsToStart > stepsToEnd ? stepsToStart : stepsToEnd;

                var effectIndex = (int)(effectPosition / GameSettings.BlockSize);
                var forwardIndex = Vertical ? new Point(index.X + effectIndex, index.Y) : new Point(index.X, index.Y + effectIndex);
                TryShoot(forwardIndex);

                var backwardIndex = Vertical ? new Point (index.X - effectIndex, index.Y) : new Point (index.X, index.Y - effectIndex);
                TryShoot(backwardIndex);

                if (effectPosition / GameSettings.BlockSize > stepsNeeded)
                    Active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            var rotation = Vertical ? 1.57f : 0f;
            spriteBatch.Draw(Texture, position, null, Color.White, rotation, Block.Origin, 1, SpriteEffects.None, 0f);
            if (Active)
            {
                var vector = Vertical ? new Vector2(effectPosition, 0) : new Vector2(0, effectPosition);
                spriteBatch.Draw(Texture, position + vector, null, Color.White, rotation, Block.Origin, 1, SpriteEffects.None, 0f);
                spriteBatch.Draw(Texture, position - vector, null, Color.White, rotation, Block.Origin, 1, SpriteEffects.None, 0f);
            }
        }
    }
}
