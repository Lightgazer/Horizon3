using Horizon3.GameScene.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public abstract class BonusAnimation
    {
        protected const float TargetSizeShrink = AnimationState.TargetSizeShrink;
        public Point Target { get; protected set; }
        public bool Over { get; protected set; }

        public static BonusAnimation Create(BonusLogic bonus, ContentManager content)
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

        protected static bool IsOver(BonusLogic bonus, float[,] shrink, List<BonusAnimation> childs)
        {
            return bonus.Dead.All(index => Math.Abs(shrink.GetValue(index) - TargetSizeShrink) < float.Epsilon)
                && childs.All(child => child.Over);
        }
    }
}
