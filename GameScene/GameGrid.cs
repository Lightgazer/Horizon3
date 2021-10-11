using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    public abstract class GameState
    {
        protected const int BlockSize = GameSettings.BlockSize;
        protected static Vector2 Origin = new Vector2(GameSettings.BlockSize / 2);

        protected readonly Texture2D[] BlockTextures;
        
        public GameState(ContentManager content)
        {
            BlockTextures = LoadBlockTextures(content);
        }

        private static Texture2D[] LoadBlockTextures(ContentManager content)
        {
            var textures = new Texture2D[Model.NumberOfBlockTypes];
            for (var index = 0; index < Model.NumberOfBlockTypes; index++)
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

        public IdleState(ContentManager content, IdleTurn turn) : base(content) {
            _frameTexture = content.Load<Texture2D>("frame");
            _turn = turn;
        }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch spriteBatch, Vector2 padding)
        {
            DrawBlocks(spriteBatch, padding);
            DrawFrame(spriteBatch, padding);
        }

        private void DrawBlocks(SpriteBatch spriteBatch, Vector2 padding)
        {
            for (var indexX = 0; indexX < Model.GridSize; indexX++)
            {
                for (var indexY = 0; indexY < Model.GridSize; indexY++)
                {
                    var block = _turn.Blocks[indexX, indexY];
                    var texture = BlockTextures[block.Type];
                    var position = new Vector2(indexX * BlockSize, indexY * BlockSize) + padding + Origin;
                    spriteBatch.Draw(texture, position, Color.White);
                    //var size = 1;
                    //spriteBatch.Draw(texture, position, null, Color.White, 0f, Origin, size, SpriteEffects.None, 0f);
                    if (block.Bonus is { })
                    {

                    }
                }
            }
        }

        private void DrawFrame(SpriteBatch spriteBatch, Vector2 padding)
        {
            if (_selectedIndex is { } pointIndex)
            {
                var position = new Vector2(pointIndex.X * BlockSize, pointIndex.Y * BlockSize) + padding;
                spriteBatch.Draw(_frameTexture, position, Color.White);
            }
        }
    }

    public class GameGrid
    {
        public int Score { get; private set; }

        private const int GridSize = GameSettings.GridSize;
        private const int BlockSize = GameSettings.BlockSize;

        private readonly ContentManager _content;
        private readonly Texture2D _frameTexture;
        private readonly Block[,] _grid = new Block[GridSize, GridSize];
        private readonly BlockFactory _factory;
        private MouseState _lastMouseState;
        private Rectangle _gridRectangle;
        private Point? _selectedIndex;

        public GameGrid(ContentManager content)
        {
            _content = content;
            _factory = new BlockFactory(content);
            _frameTexture = content.Load<Texture2D>("frame");
            PopulateGrid();

            const int sideLength = GridSize * BlockSize;
            _gridRectangle = new Rectangle(
                new Point((GameSettings.Width - sideLength) / 2, (GameSettings.Height - sideLength) / 2),
                new Point(sideLength)
            );

            Bonus.OnBonusShoot += TryKillBlock;
        }

        public void Unsubscribe()
        {
            Bonus.OnBonusShoot -= TryKillBlock;
        }

        public void Update(GameTime gameTime)
        {
            UpdateBlocks(gameTime);

            if (IsReadyForMatch())
            {
                TriggerMatches();
                ReleaseSuspects();
            }

            if (IsReadyForDrop()) TriggerDrop();
            if (IsIdle()) ManagePlayerInput();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var padding = new Vector2(_gridRectangle.Location.X, _gridRectangle.Location.Y);
            DrawBlocks(spriteBatch, padding);
            DrawBonuses(spriteBatch, padding);
            DrawFrame(spriteBatch, padding);
        }

        private static bool IsIndexInBounds(Point index)
        {
            return index.X >= 0 && index.X < GridSize && index.Y >= 0 && index.Y < GridSize;
        }

        private static bool IsSwapAllowed(Point first, Point second)
        {
            if (first.X == second.X)
                return Math.Abs(first.Y - second.Y) == 1;
            if (first.Y == second.Y)
                return Math.Abs(first.X - second.X) == 1;
            return false;
        }

        private void TryKillBlock(Point index)
        {
            if (IsIndexInBounds(index))
                KillBlock(_grid[index.X, index.Y]);
        }

        private void DrawFrame(SpriteBatch spriteBatch, Vector2 padding)
        {
            if (_selectedIndex is { } pointIndex)
            {
                var position = new Vector2(pointIndex.X * BlockSize, pointIndex.Y * BlockSize);
                position += padding;
                spriteBatch.Draw(_frameTexture, position, Color.White);
            }
        }

        private void DrawBlocks(SpriteBatch spriteBatch, Vector2 padding)
        {
            for (var indexX = 0; indexX < GridSize; indexX++)
            {
                for (var indexY = 0; indexY < GridSize; indexY++)
                {
                    var block = _grid[indexX, indexY];
                    var position = new Vector2((indexX * BlockSize), (indexY * BlockSize));
                    position += padding;
                    block.Draw(spriteBatch, position);
                }
            }
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

        private void TriggerMatches()
        {
            TriggerMatches(true);
            TriggerMatches(false);
            CleanCrossingFlags();
        }

        private void TriggerMatches(bool vertical)
        {
            for (var indexX = 0; indexX < GridSize; indexX++)
            {
                var currentType = -1;
                var matchChain = new List<Block>();
                for (var indexY = 0; indexY < GridSize; indexY++)
                {
                    var block = vertical ? _grid[indexY, indexX] : _grid[indexX, indexY];
                    if (currentType == block.Type)
                    {
                        matchChain.Add(block);
                    }
                    else
                    {
                        ProcessChain(matchChain, vertical);
                        currentType = block.Type;
                        matchChain.Clear();
                        matchChain.Add(block);
                    }
                }

                ProcessChain(matchChain, vertical);
            }
        }

        private void ProcessChain(List<Block> matchChain, bool vertical)
        {
            if (matchChain.Count < 3) return;
            matchChain.ForEach(block =>
            {
                Bonus nextBonus = null;
                if (block.CrossingFlag)
                {
                    nextBonus = new BombBonus(_content);
                }
                else if (block.State == BlockState.Suspect && matchChain.Count > 3)
                {
                    if (matchChain.Count > 4)
                        nextBonus = new BombBonus(_content);
                    else
                        nextBonus = new LineBonus(_content) { Vertical = vertical };
                }
                KillBlock(block, nextBonus);

                block.CrossingFlag = true;
            });
        }

        private void KillBlock(Block block, Bonus nextBonus = null)
        {
            Score++;
            block.ActivateBonus(_grid);
            block.State = BlockState.Dead;
            block.NextBonus = nextBonus;
        }

        private void CleanCrossingFlags()
        {
            _grid.ForEach(block => block.CrossingFlag = false);
        }

        private void TriggerDrop()
        {
            _grid.ForEach((block, point) =>
            {
                if (block.State == BlockState.Rotten) MarkDrop(point);
            });
        }

        private void MarkDrop(Point point)
        {
            var (x, y) = point;
            while (y > 0)
            {
                _grid[x, y] = _grid[x, y - 1];
                _grid[x, y].MoveFrom(new Vector2(0, -1));
                y--;
            }

            _grid[x, 0] = _factory.CreateBlock();
            _grid[x, 0].MoveFrom(new Vector2(0, -1));
        }

        private void UpdateBlocks(GameTime gameTime)
        {
            _grid.ForEach(block => block.Update(gameTime));
        }

        private bool IsReadyForMatch()
        {
            return _grid.Cast<Block>().All(block => block.State == BlockState.Idle || block.State == BlockState.Suspect);
        }

        private bool IsIdle()
        {
            return _grid.Cast<Block>().All(block => block.State == BlockState.Idle);
        }

        private bool IsReadyForDrop()
        {
            return _grid.Cast<Block>().Any(block => block.State == BlockState.Rotten)
                && _grid.Cast<Block>().All(block => block.State != BlockState.Moving);
        }

        private void SwapBlocks(Point first, Point second, BlockState setState)
        {
            var movementDirection = (first - second).ToVector2();
            var firstBlock = _grid[first.X, first.Y];
            var secondBlock = _grid[second.X, second.Y];
            firstBlock.State = setState;
            secondBlock.State = setState;
            firstBlock.MoveFrom(movementDirection);
            secondBlock.MoveFrom(-movementDirection);
            _grid[second.X, second.Y] = firstBlock;
            _grid[first.X, first.Y] = secondBlock;
        }

        private void ReleaseSuspects()
        {
            var indices = _grid.FindAllIndexOf(block => block.State == BlockState.Suspect);

            if (indices.Count == 2)
                SwapBlocks(indices[0], indices[1], BlockState.Idle);
            if (indices.Count == 1)
                _grid[indices[0].X, indices[0].Y].State = BlockState.Idle;
        }

        private void ManagePlayerInput()
        {
            if (GetCellClick() is { } click)
            {
                if (_selectedIndex is { } pointIndex)
                {
                    if (IsSwapAllowed(pointIndex, click))
                    {
                        SwapBlocks(pointIndex, click, BlockState.Suspect);
                        _selectedIndex = null;
                        return;
                    }
                }

                _selectedIndex = click;
            }
        }

        private Point? GetCellClick()
        {
            var mouseState = Mouse.GetState();
            Point? option = null;
            if (_lastMouseState.LeftButton == ButtonState.Released &&
                mouseState.LeftButton == ButtonState.Pressed &&
                _gridRectangle.Contains(mouseState.Position))
            {
                option = (mouseState.Position - _gridRectangle.Location) / new Point(BlockSize);
            }

            _lastMouseState = mouseState;
            return option;
        }

        private void PopulateGrid()
        {
            _grid.ForEach((x, y) => _grid[x, y] = _factory.CreateBlock());
        }
    }
}
