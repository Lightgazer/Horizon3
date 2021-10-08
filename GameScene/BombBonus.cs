using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Horizon3.GameScene
{
    internal class BombBonus : Bonus
    {
        private double detonateTime = 250;

        public BombBonus(ContentManager content)
        {
            Texture = content.Load<Texture2D>("bonuses/bomb");
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                detonateTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (detonateTime < 0)
                {
                    Active = false;

                    for (var x = -1; x <= 1; x++)
                    {
                        for (var y = -1; y <= 1; y++)
                        {
                            var target = new Point(index.X + x, index.Y + y);
                            TryShoot(target);
                        }
                    }
                }
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Texture, position, null, Color.White, 0f, Block.Origin, 1, SpriteEffects.None, 0f);
        }
    }
}
