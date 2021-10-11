using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Horizon3.GameScene.Model;

namespace Horizon3.GameScene
{
    public abstract class GameState
    {
        protected const int BlockSize = GameSettings.BlockSize;
        protected static Vector2 Origin = new Vector2(GameSettings.BlockSize / 2);

        protected readonly Texture2D[] BlockTextures;
        protected readonly Vector2 Padding;
        protected readonly Rectangle GridRectangle;

        public GameState(ContentManager content)
        {
            BlockTextures = LoadBlockTextures(content);
            const int sideLength = GameModel.GridSize * BlockSize;
            var padding = new Point((GameSettings.Width - sideLength) / 2, (GameSettings.Height - sideLength) / 2);
            Padding = padding.ToVector2();
            GridRectangle = new Rectangle(padding, new Point(sideLength));
        }

        public abstract void Update(GameTime gameTime, GameGrid context);

        public abstract void Draw(SpriteBatch spriteBatch);

        private static Texture2D[] LoadBlockTextures(ContentManager content)
        {
            var textures = new Texture2D[GameModel.NumberOfBlockTypes];
            for (var index = 0; index < GameModel.NumberOfBlockTypes; index++)
            {
                textures[index] = content.Load<Texture2D>("blocks/block" + index.ToString());
            }
            return textures;
        }
    }

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
            _turn.Blocks.ForEach((block, point) =>
            {
                var texture = BlockTextures[block.Type];
                var position = new Vector2(point.X * BlockSize, point.Y * BlockSize) + Padding;
                spriteBatch.Draw(texture, position, Color.White);
                if (block.Bonus is { })
                {

                }
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

    public class AnimationState : GameState
    {
        private const float TargetSizeShrink = 0.8f;
        private readonly AnimationTurn _turn;
        private readonly float[,] _sizeShrink;
        private float _size = 0;
        private List<BonusAnimation> _bonuses;

        public AnimationState(AnimationTurn turn, ContentManager content) : base(content)
        {
            _turn = turn;
            _sizeShrink = new float[turn.Blocks.Length, turn.Blocks.Length];
        }

        public override void Update(GameTime gameTime, GameGrid context)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed;
            _size = MyMath.MoveTowards(_size, TargetSizeShrink, delta);
            _turn.Dead.ForEach(point => _sizeShrink.SetValue(point, _size));
            //bonus update
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBlocks(spriteBatch);
            //bonus draw
        }

        private void DrawBlocks(SpriteBatch spriteBatch)
        {
            _turn.Blocks.ForEach((block, point) =>
            {
                var texture = BlockTextures[block.Type];
                var position = new Vector2(point.X * BlockSize, point.Y * BlockSize) + Padding + Origin;
                var size = 1 - _sizeShrink.GetValue(point);
                spriteBatch.Draw(texture, position, null, Color.White, 0f, Origin, size, SpriteEffects.None, 0f);
                if (block.Bonus is { })
                {

                }
            });
        }
    }

    public class GameGrid
    {
        public int Score { get { return Model.Score; } }
        public readonly GameModel Model;

        private readonly ContentManager _content;
        private GameState _state;


        public GameGrid(ContentManager content, GameModel model)
        {
            Model = model;
            _content = content;
            NextTurn();
        }

        public void NextTurn()
        {
            _state = Model.GetNextTurn() switch
            {
                AnimationTurn turn => new AnimationState(turn, _content),
                IdleTurn turn => new IdleState(turn, _content),
                DropTurn turn => new DropState(turn, _content),
                SwapTurn turn => new SwapState(turn, _content),
                _ => throw new NotImplementedException("No state for this turn")
            };
        }

        public void Update(GameTime gameTime)
        {
            _state.Update(gameTime, this);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _state.Draw(spriteBatch);
        }

        private void DrawBonuses(SpriteBatch spriteBatch, Vector2 padding)
        {
            for (var indexX = 0; indexX < GridSize; indexX++)
            {
                for (var indexY = 0; indexY < GridSize; indexY++)
                {
                    var block = _grid[indexX, indexY];
                    var position = new Vector2((indexX * BlockSize), (indexY * BlockSize));
                    position += padding;
                    block.DrawBonus(spriteBatch, position);
                }
            }
        }
    }
}
