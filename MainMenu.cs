using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    internal class MainMenu : IGlobalState
    {
        private readonly UIElement _button;

        public MainMenu(ContentManager content)
        {
            var center = new Vector2(GameSettings.Width / 2, GameSettings.Height / 2);
            var buttonTexture = content.Load<Texture2D>("buttons/play");
            _button = new UIElement(buttonTexture, center);
            _button.OnClick += ButtonOnClick;
        }

        public void Update(GameTime gameTime)
        {
            _button.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _button.Draw(spriteBatch);
        }

        private void ButtonOnClick()
        {
            GlobalStateContext.LoadScene<GameScreen>();
        }
    }
}
