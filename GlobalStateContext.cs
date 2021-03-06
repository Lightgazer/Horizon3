using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3
{
    public static class GlobalStateContext
    {
        private static readonly List<IGlobalState> Scenes = new List<IGlobalState>();
        private static IGlobalState _currentScene;

        public static void Add(IGlobalState scene)
        {
            Scenes.Add(scene);
        }

        public static void ChangeState<T>() where T : IGlobalState
        {
            _currentScene = Scenes.Find(scene => scene is T);
            _currentScene.Start();
        }

        public static void Update(GameTime gameTime)
        {
            _currentScene.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentScene.Draw(spriteBatch);
        }
    }
}
