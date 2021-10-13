using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3
{
    public static class GlobalStateContext
    {
        private static readonly List<IGlobalState> Scenes = new List<IGlobalState>();
        private static IGlobalState CurrentScene;

        public static void Add(IGlobalState scene)
        {
            Scenes.Add(scene);
        }

        public static void ChangeState<T>() where T : IGlobalState
        {
            var index = Scenes.FindIndex(scene => scene is T);
            ChangeState(index);
        }

        public static void ChangeState(int index)
        {
            if (Scenes.ElementAtOrDefault(index) is { } scene)
            {
                CurrentScene = scene;
                CurrentScene.Start();
            }
        }

        public static void Update(GameTime gameTime)
        {
            CurrentScene?.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            CurrentScene?.Draw(spriteBatch);
        }
    }
}
