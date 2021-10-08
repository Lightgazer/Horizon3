using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    internal class SceneTimer
    {
        private readonly Vector2 position = new Vector2(GameSettings.Width * 0.6f, 10);
        private readonly SpriteFont _font;
        private double _timeLeft;

        public SceneTimer(ContentManager content, double seconds)
        {
            _font = content.Load<SpriteFont>("Font");
            _timeLeft = seconds;
        }

        public void Update(GameTime gameTime)
        {
            _timeLeft -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_timeLeft < 0) GlobalStateContext.LoadScene<EndMenu>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "Time Left: " + (int)_timeLeft, position, Color.White);
        }
    }
}
