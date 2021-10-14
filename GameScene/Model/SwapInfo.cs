using Microsoft.Xna.Framework;

namespace Horizon3.GameScene.Model
{
    public class SwapInfo
    {
        public readonly Point First;
        public readonly Point Second;

        public SwapInfo(Point first, Point second)
        {
            First = first;
            Second = second;
        }
    }
}
