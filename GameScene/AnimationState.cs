using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    /// <summary>
    /// В этом состоянии игра отображает анимацию уменьшения блоков попавших в матч 
    /// и анимацию бонусов. 
    /// </summary>
    public class AnimationState : GameState
    {
        public const float TargetSizeShrink = 0.8f;
        private readonly AnimationTurn _turn;
        private readonly float[,] _shrinkGrid = new float[GameModel.GridSize, GameModel.GridSize];
        private readonly List<BonusAnimation> _bonuses;
        private float _localShrink = 0;

        public AnimationState(AnimationTurn turn, ContentManager content) : base(content)
        {
            _turn = turn;
            _bonuses = turn.Bonuses.Select(b => BonusAnimation.Create(b, content)).ToList();
        }

        public override void Update(GameTime gameTime, GameContext context)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed;
            _localShrink = MyMath.MoveTowards(_localShrink, TargetSizeShrink, delta);
            _turn.Dead.ForEach(point => _shrinkGrid.SetValue(point, _localShrink));
            _bonuses.ForEach(bonus => bonus.Update(gameTime, _shrinkGrid));

            if (IsOver())
                context.NextTurn();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBlocks(spriteBatch);
            _bonuses.ForEach(bonus => bonus.Draw(spriteBatch));
        }

        private void DrawBlocks(SpriteBatch spriteBatch)
        {
            _turn.Blocks.ForEach((block, index) =>
            {
                var texture = BlockTextures[block.Type];
                var position = index.ToVector2() * BlockSize + Padding;
                var size = 1 - _shrinkGrid.GetValue(index);
                spriteBatch.Draw(texture, position + BlockOrigin, null, Color.White, 0f, BlockOrigin, size, SpriteEffects.None, 0f);
                DrawBonusIcon(spriteBatch, block, position);
            });
        }

        private bool IsOver()
            => Math.Abs(_localShrink - TargetSizeShrink) < float.Epsilon && _bonuses.All(bonus => bonus.Over);
    }
}
