using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3.GameScene
{
    public class GameTimer
    {
        private double _timeLeft;
        private Label _label;

        public GameTimer(ContentManager content, double seconds)
        {
            _timeLeft = seconds;
            _label = new Label(content, new Vector2(GameSettings.Width * 0.56f, 10));
        }

        public void Update(GameTime gameTime)
        {
            _timeLeft -= gameTime.ElapsedGameTime.TotalSeconds;
            _label.Text = "Time Left: " + (int)_timeLeft;
            if (_timeLeft < 0) GlobalStateContext.ChangeState<EndMenu>();
        }

        public void Draw(SpriteBatch spriteBatch)
            => _label.Draw(spriteBatch);
    }
}
