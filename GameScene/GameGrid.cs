using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon3.GameScene
{
    internal class GameGrid
    {
        public const int NumberOfBlockTypes = 5;
        public int Score { get; private set; }

        private const int GridSize = GameSettings.GridSize;
        private const int BlockSize = GameSettings.BlockSize;
        private static readonly Random Random = new Random();

        private readonly ContentManager _content;
        private readonly Texture2D[] _blockTextures;
        private readonly Texture2D _frameTexture;
        private readonly Block[,] grid = new Block[GridSize, GridSize];
        private MouseState lastMouseState;
        private Rectangle gridRectangle;
        private Point? selectedIndex;

        public GameGrid(ContentManager content)
        {
            _content = content;
            _blockTextures = LoadBlockTextures(content);
            _frameTexture = content.Load<Texture2D>("frame");
            PopulateGrid();

            const int sideLength = GridSize * BlockSize;
            gridRectangle = new Rectangle(
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
            var padding = new Vector2(gridRectangle.Location.X, gridRectangle.Location.Y);
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
                KillBlock(grid[index.X, index.Y]);
        }

        private void DrawFrame(SpriteBatch spriteBatch, Vector2 padding)
        {
            if (selectedIndex is { } pointIndex)
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
                    var block = grid[indexX, indexY];
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
                    var block = grid[indexX, indexY];
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
                    var block = vertical ? grid[indexY, indexX] : grid[indexX, indexY];
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
            block.ActivateBonus(grid);
            block.State = BlockState.Dead;
            block.NextBonus = nextBonus;
        }

        private void CleanCrossingFlags()
        {
            grid.ForEach(block => block.CrossingFlag = false);
        }

        private void TriggerDrop()
        {
            grid.ForEach((block, point) =>
            {
                if (block.State == BlockState.Rotten) MarkDrop(point);
            });
        }

        private void MarkDrop(Point point)
        {
            var (x, y) = point;
            while (y > 0)
            {
                grid[x, y] = grid[x, y - 1];
                grid[x, y].MoveFrom(new Vector2(0, -1));
                y--;
            }

            grid[x, 0] = CreateBlock();
            grid[x, 0].MoveFrom(new Vector2(0, -1));
        }

        private void UpdateBlocks(GameTime gameTime)
        {
            grid.ForEach(block => block.Update(gameTime));
        }

        private bool IsReadyForMatch()
        {
            return grid.Cast<Block>().All(block => block.State == BlockState.Idle || block.State == BlockState.Suspect);
        }

        private bool IsIdle()
        {
            return grid.Cast<Block>().All(block => block.State == BlockState.Idle);
        }

        private bool IsReadyForDrop()
        {
            return grid.Cast<Block>().Any(block => block.State == BlockState.Rotten)
                   && grid.Cast<Block>().All(block => block.State != BlockState.Moving);
        }

        private void SwapBlocks(Point first, Point second, BlockState setState)
        {
            var movementDirection = (first - second).ToVector2();
            var firstBlock = grid[first.X, first.Y];
            var secondBlock = grid[second.X, second.Y];
            firstBlock.State = setState;
            secondBlock.State = setState;
            firstBlock.MoveFrom(movementDirection);
            secondBlock.MoveFrom(-movementDirection);
            grid[second.X, second.Y] = firstBlock;
            grid[first.X, first.Y] = secondBlock;
        }

        private void ReleaseSuspects()
        {
            var indices = grid.FindAllIndexOf(block => block.State == BlockState.Suspect);

            if (indices.Count == 2)
                SwapBlocks(indices[0], indices[1], BlockState.Idle);
            if (indices.Count == 1)
                grid[indices[0].X, indices[0].Y].State = BlockState.Idle;
        }

        private void ManagePlayerInput()
        {
            if (GetCellClick() is { } click)
            {
                if (selectedIndex is { } pointIndex)
                {
                    if (IsSwapAllowed(pointIndex, click))
                    {
                        SwapBlocks(pointIndex, click, BlockState.Suspect);
                        selectedIndex = null;
                        return;
                    }
                }

                selectedIndex = click;
            }
        }

        private Point? GetCellClick()
        {
            var mouseState = Mouse.GetState();
            Point? option = null;
            if (lastMouseState.LeftButton == ButtonState.Released &&
                mouseState.LeftButton == ButtonState.Pressed &&
                gridRectangle.Contains(mouseState.Position))
            {
                option = (mouseState.Position - gridRectangle.Location) / new Point(BlockSize);
            }

            lastMouseState = mouseState;
            return option;
        }

        private void PopulateGrid()
        {
            grid.ForEach((x, y) => grid[x, y] = CreateBlock());
        }

        private Block CreateBlock()
        {
            var type = Random.Next(NumberOfBlockTypes);
            var block = new Block(type, _blockTextures[type]);
            return block;
        }

        private static Texture2D[] LoadBlockTextures(ContentManager content)
        {
            var textures = new Texture2D[NumberOfBlockTypes];
            for (var index = 0; index < NumberOfBlockTypes; index++)
            {
                textures[index] = content.Load<Texture2D>("blocks/block" + index.ToString());
            }
            return textures;
        }
    }
}
