using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Horizon3.GameScene
{
    /// <summary>
    /// Игровое поле реализовано паттерном состояние. Состояния сами решают когда они
    /// заканчиваются вызывая NextTurn у контекста. Контекст используя модель
    /// выбирает следующие состояние.
    /// </summary>
    public class GameContext
    {
        public int Score => Model.Score;
        public readonly GameModel Model;

        private readonly ContentManager _content;
        private GameState _state;


        public GameContext(ContentManager content)
        {
            Model = new GameModel();
            _content = content;
            NextTurn();
        }

        public void NextTurn()
        {
            _state = Model.GetNextTurn() switch
            {
                AnimationTurn turn => new AnimationState(turn, _content),
                IdleTurn turn => new IdleState(turn, _content),
                DropTurn turn => new DropState(turn, _content),
                SwapTurn turn => new SwapState(turn, _content),
                _ => throw new NotImplementedException("No state for this turn")
            };
        }

        public void Update(GameTime gameTime)
        {
            _state.Update(gameTime, this);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _state.Draw(spriteBatch);
        }
    }
}
