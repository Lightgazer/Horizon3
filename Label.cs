using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    public class Label
    {
        public string Text { private get; set; } = "";
        protected Vector2 _position;
        private readonly SpriteFont _font;

        public Label(ContentManager content, Vector2 position)
        {
            _font = content.Load<SpriteFont>("Font");
            _position = position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, Text, _position, Color.White);
        }
    }
}