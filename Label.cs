﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    public abstract class Label
    {
        protected string Text = "";
        protected Vector2 Position;
        private readonly SpriteFont _font;

        protected Label(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Font");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, Text, Position, Color.White);
        }
    }
}