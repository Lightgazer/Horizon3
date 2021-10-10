using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Horizon3.GameScene
{
    public class GameTimer : Label
    {
        private double _timeLeft;

        public GameTimer(ContentManager content, double seconds) : base(content)
        {
            _timeLeft = seconds;
            Position = new Vector2(GameSettings.Width * 0.6f, 10);
        }

        public void Update(GameTime gameTime)
        {
            _timeLeft -= gameTime.ElapsedGameTime.TotalSeconds;
            Text = "Time Left: " + (int)_timeLeft;
            if (_timeLeft < 0) GlobalStateContext.LoadScene<EndMenu>();
        }

    }
}
