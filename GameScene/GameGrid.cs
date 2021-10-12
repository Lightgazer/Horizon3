using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Linq;
using Horizon3.GameScene.Model;

namespace Horizon3.GameScene
{
    public class GameGrid
    {
        public int Score { get { return Model.Score; } }
        public readonly GameModel Model;

        private readonly ContentManager _content;
        private GameState _state;


        public GameGrid(ContentManager content, GameModel model)
        {
            Model = model;
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
