using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Horizon3.GameScene.Model;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public abstract class BonusAnimation
    {
        protected const float TargetSizeShrink = AnimationState.TargetSizeShrink;

        public static BonusAnimation Create(Bonus bonus, ContentManager content)
        {
            return bonus switch
            {
                LineBonus b => new LineBonusAnimation(b, content),
                BombBonus b => new BombBonusAnimation(b, content),
                _ => throw new NotImplementedException("bonus animation missing")
            };
        }

        public abstract void Update(GameTime gameTime, float[,] shrink);

        public abstract void Draw(SpriteBatch spriteBatch);

        public abstract bool IsActive();
    }

    public class LineBonusAnimation : BonusAnimation
    {
        private const float diff = 0.1f;
        private const int _endEffectPosition = GameSettings.BlockSize * GameModel.GridSize;
        private readonly float _targetShrink;
        private readonly Texture2D _texture;
        private readonly LineBonus _bn;
        private float _shrinkSize = 0;
        private float _effectPosition = 0;
        private List<BonusAnimation> _childs;

        public LineBonusAnimation(LineBonus bonus, ContentManager content)
        {
            _childs = bonus.Childs.Select(bonus => Create(bonus, content)).ToList();
            _texture = content.Load<Texture2D>("bonuses/line");
            _bn = bonus;
            var gridPos = bonus.Vertical ? bonus.Target.Y : bonus.Target.X;
            var mostDistantBlock = gridPos > GameModel.GridSize / 2 ? GameModel.GridSize - gridPos : gridPos;
            _targetShrink = TargetSizeShrink + diff * mostDistantBlock;
        }

        public override void Update(GameTime gameTime, float[,] shrink)
        {
            _effectPosition += (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.BlockSize * 2 * GameSettings.AnimationSpeed;

            if (_effectPosition > _endEffectPosition / 3) ShrinkBlocks(gameTime, shrink);
            _childs.ForEach(bonus => bonus.Update(gameTime, shrink));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var rotation = _bn.Vertical ? 1.57f : 0f;
            var vector = _bn.Vertical ? new Vector2(_effectPosition, 0) : new Vector2(0, _effectPosition);
            var target = _bn.Target;
            var position = new Vector2(target.X * GameSettings.BlockSize, target.Y * GameSettings.BlockSize) + GameState.Padding;
            spriteBatch.Draw(_texture, position + vector, null, Color.White, rotation, Block.Origin, 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(_texture, position - vector, null, Color.White, rotation, Block.Origin, 1, SpriteEffects.None, 0f);

            _childs.ForEach(bonus => bonus.Draw(spriteBatch));
        }

        public override bool IsActive()
        {
            return _effectPosition < _endEffectPosition
                || Math.Abs(_targetShrink - _shrinkSize) > float.Epsilon
                || _childs.Any(bonus => bonus.IsActive());
        }

        private void ShrinkBlocks(GameTime gameTime, float[,] shrink)
        {
            const float coefficient = 1.3f; // коэфициент связаный с тем что блоки умершие от этого бонуса уменьшаются чуть быстрее
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed * coefficient;
            _shrinkSize = MyMath.MoveTowards(_shrinkSize, _targetShrink, delta);

            _bn.Dead.ForEach(point =>
            {
                var target = _bn.Vertical ? _bn.Target.Y : _bn.Target.X;
                var current = _bn.Vertical ? point.Y : point.X;
                var index = Math.Abs(target - current);
                shrink.SetValue(point, Math.Clamp(_shrinkSize - index * diff, 0, TargetSizeShrink));
            });
        }
    }

    public class BombBonusAnimation : BonusAnimation
    {
        private readonly List<BonusAnimation> _childs;
        private readonly BombBonus _bn;
        private double _detonateTime = 250;
        private float _shrinkSize = 0;

        public BombBonusAnimation(BombBonus bonus, ContentManager content) {
            _childs = bonus.Childs.Select(bonus => Create(bonus, content)).ToList();
            _bn = bonus;
        }

        public override void Update(GameTime gameTime, float[,] shrink)
        {
            if (_detonateTime > 0)
            {
                _detonateTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                ShrinkBlocks(gameTime, shrink);
                _childs.ForEach(bonus => bonus.Update(gameTime, shrink));
            }
        }

        public override void Draw(SpriteBatch spriteBatch) { }

        public override bool IsActive()
            => Math.Abs(TargetSizeShrink - _shrinkSize) > float.Epsilon || _childs.Any(bonus => bonus.IsActive());

        private void ShrinkBlocks(GameTime gameTime, float[,] shrink)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed;
            _shrinkSize = MyMath.MoveTowards(_shrinkSize, TargetSizeShrink, delta);
            _bn.Dead.ForEach(index => shrink.SetValue(index, _shrinkSize));
        }
    }
}
