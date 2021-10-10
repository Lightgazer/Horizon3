using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Horizon3
{
    public class UIElement
    {
        public event Action OnClick;

        private readonly Texture2D _texture;
        private readonly Rectangle _rectangle;
        private MouseState _lastMouseState;

        public UIElement(Texture2D texture, Vector2 position)
        {
            _texture = texture;
            var size = new Vector2(texture.Width, texture.Height);
            var vector = position - size / 2;
            _rectangle = new Rectangle(vector.ToPoint(), size.ToPoint());
            _lastMouseState = Mouse.GetState();
        }

        public void Update()
        {
            if (IsClicked()) OnClick?.Invoke();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rectangle, Color.White);
        }

        private bool IsClicked()
        {
            var mouseState = Mouse.GetState();
            if (_lastMouseState.LeftButton == ButtonState.Released && 
                mouseState.LeftButton == ButtonState.Pressed && 
                _rectangle.Contains(mouseState.Position))
            {
                return true;
            }

            _lastMouseState = mouseState;
            return false;
        }
    }
}
