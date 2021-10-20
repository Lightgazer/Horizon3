using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public class LineBonusAnimation : BonusAnimation
    {
        private const int BlockSize = GameSettings.BlockSize;
        private readonly Texture2D _texture;
        private readonly LineBonus _data;
        private readonly List<BonusAnimation> _childs;
        private readonly List<Point> _activeDead = new List<Point>();
        private readonly List<BonusAnimation> _activeChilds = new List<BonusAnimation>();

        private Point _destructorPosition;
        private float _destructorDisplacement = 0;

        public LineBonusAnimation(LineBonus bonus, ContentManager content)
        {
            _childs = bonus.Childs.Select(child => Create(child, content)).ToList();
            _texture = content.Load<Texture2D>("bonuses/line");
            _data = bonus;
            Target = bonus.Target;
        }

        public override void Update(GameTime gameTime, float[,] shrink)
        {
            if (Over) return;
            if (MoveDestructor(gameTime))
            {
                var destructor1 = Target + _destructorPosition;
                var destructor2 = Target - _destructorPosition;
                ActivateChildBonus(destructor1);
                ActivateChildBonus(destructor2);
                StartShrinkingBlock(destructor1);
                StartShrinkingBlock(destructor2);
            }

            ShrinkBlocks(gameTime, shrink);
            _activeChilds.ForEach(bonus => bonus.Update(gameTime, shrink));
            Over = IsOver(_data, shrink, _childs);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Over) return;
            DrawDestructor(spriteBatch, _destructorPosition, _destructorDisplacement);
            var negativePosition = new Point(-_destructorPosition.X, -_destructorPosition.Y);
            DrawDestructor(spriteBatch, negativePosition, -_destructorDisplacement);

            _activeChilds.ForEach(bonus => bonus.Draw(spriteBatch));
        }

        private void DrawDestructor(SpriteBatch spriteBatch, Point position, float displacement)
        {
            var rotation = _data.Vertical ? 0f : 1.57f;
            var blockIndex = Target + position;
            var screenPosition = blockIndex.ToVector2() * BlockSize + GameState.Padding;
            screenPosition += _data.Vertical ? new Vector2(0, displacement) : new Vector2(displacement, 0);
            spriteBatch.Draw(_texture, screenPosition + GameState.BlockOrigin, null, Color.White, rotation,
                GameState.BlockOrigin, 1, SpriteEffects.None, 0f);
        }

        private bool MoveDestructor(GameTime gameTime)
        {
            _destructorDisplacement += (float)gameTime.ElapsedGameTime.TotalSeconds * BlockSize * 2 * GameSettings.AnimationSpeed;
            if (_destructorDisplacement > BlockSize)
            {
                _destructorDisplacement -= BlockSize;
                if (_data.Vertical)
                    _destructorPosition.Y++;
                else
                    _destructorPosition.X++;
                return true;
            }
            return false;
        }

        private void ActivateChildBonus(Point index)
        {
            var child = _childs.Find(bonus => bonus.Target == index);
            if (child is { }) _activeChilds.Add(child);
        }

        private void StartShrinkingBlock(Point index)
        {
            if (_data.Dead.Contains(index))
                _activeDead.Add(index);
        }

        private void ShrinkBlocks(GameTime gameTime, float[,] shrink)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed;

            _activeDead.ForEach(index =>
            {
                var value = shrink.GetValue(index);
                shrink.SetValue(index, MyMath.MoveTowards(value, TargetSizeShrink, delta));
            });
        }
    }
}
