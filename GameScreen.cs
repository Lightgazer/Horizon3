using Horizon3.GameScene;
using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    public class GameScreen : IGlobalState
    {
        private readonly ContentManager _content;

        private GameContext _grid;
        private ScoreWidget _scoreWidget;
        private GameTimer _timer;

        public GameScreen(ContentManager content)
        {
            _content = content;
        }

        public void Start()
        {
            var model = new GameModel();
            _grid = new GameContext(_content, model);
            _scoreWidget = new ScoreWidget(_content);
            _timer = new GameTimer(_content, 60d);
        }

        public void Update(GameTime gameTime)
        {
            _grid.Update(gameTime);
            _timer.Update(gameTime);
            _scoreWidget.Score = _grid.Score;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _grid.Draw(spriteBatch);
            _timer.Draw(spriteBatch);
            _scoreWidget.Draw(spriteBatch);
        }
    }
}
