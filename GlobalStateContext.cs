using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3
{
    internal static class GlobalStateContext
    {
        public static IGlobalState CurrentScene { get; private set; }

        private static readonly List<IGlobalState> Scenes = new List<IGlobalState>();

        public static void Add(IGlobalState scene)
        {
            Scenes.Add(scene);
        }

        public static void LoadScene<T>() where T : IGlobalState
        {
            var index = Scenes.FindIndex(scene => scene is T);
            LoadScene(index);
        }

        public static void LoadScene(int index)
        {
            if (Scenes.ElementAtOrDefault(index) is { } scene)
            {
                CurrentScene?.Stop();
                CurrentScene = scene;
                CurrentScene.Start();
            }
        }

        public static void Update(GameTime gameTime)
        {
            CurrentScene.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            CurrentScene.Draw(spriteBatch);
        }
    }
}
