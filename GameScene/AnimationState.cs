using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Horizon3.GameScene.Model;
using System;
using System.Linq;

namespace Horizon3.GameScene
{
    public class AnimationState : GameState
    {
        public const float TargetSizeShrink = 0.8f;
        private readonly AnimationTurn _turn;
        private readonly float[,] _shrinkGrid;
        private float _localShrink = 0;
        private List<BonusAnimation> _bonuses;

        public AnimationState(AnimationTurn turn, ContentManager content) : base(content)
        {
            _turn = turn;
            _shrinkGrid = new float[turn.Blocks.Length, turn.Blocks.Length];
        }

        public override void Update(GameTime gameTime, GameGrid context)
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
                spriteBatch.Draw(texture, position + Origin, null, Color.White, 0f, Origin, size, SpriteEffects.None, 0f);
                DrawBonusIcon(spriteBatch, block, position);
            });
        }

        private bool IsOver()
            => Math.Abs(_localShrink - TargetSizeShrink) < float.Epsilon && _bonuses.All(bonus => !bonus.IsActive());
    }
}
