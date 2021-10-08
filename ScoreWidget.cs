using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    internal class ScoreWidget
    {
        public int Score { private get; set; } = 0;
        private readonly Vector2 position = new Vector2((GameSettings.Width - GameSettings.GridSize * GameSettings.BlockSize) / 2, 10);
        private readonly SpriteFont _font;

        public ScoreWidget(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Font");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "Score: " + Score, position, Color.White);
        }
    }
}
