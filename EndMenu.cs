using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    public class EndMenu : IGlobalState
    {
        private readonly UIElement _button;
        private readonly UIElement _message;

        public EndMenu(ContentManager content)
        {
            var center = new Vector2(GameSettings.Width / 2, GameSettings.Height / 2);
            var buttonTexture = content.Load<Texture2D>("buttons/ok");
            var messageTexture = content.Load<Texture2D>("gameover");
            var someSpace = new Vector2(0, center.Y / 4);
            _message = new UIElement(messageTexture, center - someSpace);
            _button = new UIElement(buttonTexture, center + someSpace);
            _button.OnClick += ButtonOnClick;
        }

        public void Update(GameTime gameTime)
        {
            _message.Update();
            _button.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _message.Draw(spriteBatch);
            _button.Draw(spriteBatch);
        }

        private void ButtonOnClick()
        {
            GlobalStateContext.LoadScene<MainMenu>();
        }
    }
}
