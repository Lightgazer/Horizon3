using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Horizon3.GameScene
{
    /// <summary>
    /// В этом состоянии игра отображает анимацию падения блоков на пустые места.
    /// </summary>
    public class DropState : GameState
    {
        private readonly DropTurn _turn;
        private float _displacement = 0;

        public DropState(DropTurn turn, ContentManager content) : base(content)
        {
            _turn = turn;
        }

        public override void Update(GameTime gameTime, GameContext context)
        {
            const float target = BlockSize;
            if (Math.Abs(_displacement - target) < float.Epsilon)
            {
                context.NextTurn();
                return;
            }
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * BlockSize * 2 *
                            GameSettings.AnimationSpeed;
            _displacement = MyMath.MoveTowards(_displacement, target, delta);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBlocks(spriteBatch);
        }

        private void DrawBlocks(SpriteBatch spriteBatch)
        {
            _turn.Blocks.ForEach((block, index) =>
            {
                if (block.Alive is false) return;
                var texture = BlockTextures[block.Type];
                var position = index.ToVector2() * BlockSize + Padding;
                if (_turn.Drop.Contains(index)) position += new Vector2(0, _displacement);
                spriteBatch.Draw(texture, position, Color.White);
                DrawBonusIcon(spriteBatch, block, position);
            });
        }
    }
}