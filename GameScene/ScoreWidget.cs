using Microsoft.Xna.Framework.Content;

namespace Horizon3
{
    internal class ScoreWidget : Label
    {
        public int Score
        {
            set
            {
                Text = "Score: " + value;
            }
        }

        public ScoreWidget(ContentManager content) : base(content) { }
    }
}
