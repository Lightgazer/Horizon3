using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Horizon3.GameScene
{
    public class SwapState : GameState
    {
        private readonly Vector2 _direction;
        private readonly SwapTurn _turn;
        private float _displacement = 0;

        public SwapState(SwapTurn turn, ContentManager content) : base(content)
        {
            _turn = turn;
            _direction = (turn.Second - turn.First).ToVector2();
        }

        public override void Update(GameTime gameTime, GameGrid context)
        {
            const float target = BlockSize;
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * BlockSize * 2 *
                            GameSettings.AnimationSpeed;
            _displacement += MyMath.MoveTowards(_displacement, target, delta);
            if (Math.Abs(_displacement - target) < float.Epsilon) context.NextTurn();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBlocks(spriteBatch);
        }

        private void DrawBlocks(SpriteBatch spriteBatch)
        {
            _turn.Blocks.ForEach((block, index) =>
            {
                var texture = BlockTextures[block.Type];
                var position = index.ToVector2() * BlockSize + Padding;
                if (_turn.First == index) position += _direction * _displacement;
                spriteBatch.Draw(texture, position, Color.White);
                DrawBonusIcon(spriteBatch, block, position);
            });
        }
    }
}