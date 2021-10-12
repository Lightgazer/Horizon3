using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Horizon3.GameScene.Model;

namespace Horizon3.GameScene
{
    public class IdleState : GameState
    {
        private readonly Texture2D _frameTexture;
        private readonly IdleTurn _turn;
        private Point? _selectedIndex;
        private MouseState _lastMouseState;

        public IdleState(IdleTurn turn, ContentManager content) : base(content)
        {
            _frameTexture = content.Load<Texture2D>("frame");
            _turn = turn;
        }

        public override void Update(GameTime gameTime, GameGrid context)
        {
            ManagePlayerInput(context);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBlocks(spriteBatch);
            DrawFrame(spriteBatch);
        }

        private void DrawBlocks(SpriteBatch spriteBatch)
        {
            _turn.Blocks.ForEach((block, index) =>
            {
                var texture = BlockTextures[block.Type];
                var position = index.ToVector2() * BlockSize + Padding;
                spriteBatch.Draw(texture, position, Color.White);
                DrawBonusIcon(spriteBatch, block, position);
            });
        }

        private void DrawFrame(SpriteBatch spriteBatch)
        {
            if (_selectedIndex is { } pointIndex)
            {
                var position = new Vector2(pointIndex.X * BlockSize, pointIndex.Y * BlockSize) + Padding;
                spriteBatch.Draw(_frameTexture, position, Color.White);
            }
        }

        private void ManagePlayerInput(GameGrid context)
        {
            if (GetCellClick() is { } click)
            {
                if (_selectedIndex.HasValue && context.Model.SwapBlocks(_selectedIndex.Value, click))
                    context.NextTurn();

                _selectedIndex = click;
            }
        }

        private Point? GetCellClick()
        {
            var mouseState = Mouse.GetState();
            Point? option = null;
            if (_lastMouseState.LeftButton == ButtonState.Released &&
                mouseState.LeftButton == ButtonState.Pressed &&
                GridRectangle.Contains(mouseState.Position))
            {
                option = (mouseState.Position - GridRectangle.Location) / new Point(BlockSize);
            }

            _lastMouseState = mouseState;
            return option;
        }
    }
}
