using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3.GameScene
{
    public class ScoreWidget
    {
        public int Score { set => _label.Text = "Score: " + value; }
        private readonly Label _label;

        public ScoreWidget(ContentManager content) {
            _label = new Label(content, new Vector2(GameSettings.Width * 0.18f, 10));
        }

        public void Draw(SpriteBatch spriteBatch)
            => _label.Draw(spriteBatch);
    }
}
