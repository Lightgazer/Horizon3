using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Horizon3
{
    public interface IGlobalState
    {
        public void Start() { }
        public void Stop() { }
        public void Update(GameTime gameTime);
        public void Draw(SpriteBatch spriteBatch);
    }
}
