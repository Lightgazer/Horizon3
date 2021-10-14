using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public class BombBonusAnimation : BonusAnimation
    {
        private readonly List<BonusAnimation> _childs;
        private readonly BombBonus _data;
        private double _detonateTime = 250;
        private float _shrinkSize = 0;

        public BombBonusAnimation(BombBonus bonus, ContentManager content)
        {
            _childs = bonus.Childs.Select(child => Create(child, content)).ToList();
            _data = bonus;
        }

        public override void Update(GameTime gameTime, float[,] shrink)
        {
            if (Over) return;
            if (_detonateTime > 0)
            {
                _detonateTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                ShrinkBlocks(gameTime, shrink);
                _childs.ForEach(bonus => bonus.Update(gameTime, shrink));
                Over = IsOver(_data, shrink, _childs);
            }
        }

        public override void Draw(SpriteBatch spriteBatch) { }

        private void ShrinkBlocks(GameTime gameTime, float[,] shrink)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed;
            _shrinkSize = MyMath.MoveTowards(_shrinkSize, TargetSizeShrink, delta);
            _data.Dead.ForEach(index => shrink.SetValue(index, _shrinkSize));
        }
    }
}
