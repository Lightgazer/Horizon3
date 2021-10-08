using Horizon3.GameScene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    internal class GameScreen : IGlobalState
    {
        private readonly ContentManager _content;

        private GameGrid grid;
        private ScoreWidget _scoreWidget;
        private SceneTimer _timer;

        public GameScreen(ContentManager content)
        {
            _content = content;
        }

        public void Start()
        {
            grid = new GameGrid(_content);
            _scoreWidget = new ScoreWidget(_content);
            _timer = new SceneTimer(_content, 60d);
        }

        public void Stop()
        {
            grid.Unsubscribe();
        }

        public void Update(GameTime gameTime)
        {
            grid.Update(gameTime);
            _timer.Update(gameTime);
            _scoreWidget.Score = grid.Score;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            grid.Draw(spriteBatch);
            _timer.Draw(spriteBatch);
            _scoreWidget.Draw(spriteBatch);
        }
    }
}
