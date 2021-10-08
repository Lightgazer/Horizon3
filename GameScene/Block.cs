using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Horizon3.GameScene
{
    internal enum BlockState
    {
        ///<summary>Двигается, либо падение, либо свап</summary>
        Moving,

        ///<summary>Свапнутый блок, ждёт проверку на матч, затем возвращется или умирает</summary>
        Suspect,

        ///<summary>Умер, постепенно исчезает</summary>
        Dead,

        ///<summary>Разложился, можно падать на его место</summary>
        Rotten,

        ///<summary>Ожидает, если все блоки ожидают игрок может сделать свап</summary>
        Idle
    }

    internal class Block
    {
        private const float RottenSize = 0.2f;

        public readonly int Type;
        public BlockState State = BlockState.Idle;
        public bool CrossingFlag = false;
        public static Vector2 Origin = new Vector2(GameSettings.BlockSize / 2);
        public Bonus NextBonus { get; set; }
        private Bonus bonus;
        private float size = 1;
        private Vector2 movementDisplacement = new Vector2(0, 0);

        private readonly Texture2D texture;
        private BlockState lastState;

        public Block(int type, Texture2D texture)
        {
            Type = type;
            this.texture = texture;
        }

        public void ActivateBonus(Block[,] grid)
        {
            var index = grid.Cast<Block>().ToList().FindIndex(block => block == this);
            if (!IsBonusActive())
                bonus?.Activate(new Point(index / GameSettings.GridSize, index % GameSettings.GridSize));
        }

        public bool IsBonusActive()
        {
            return bonus is { Active: true };
        }

        public void MoveFrom(Vector2 direction)
        {
            if (State == BlockState.Moving) return;
            direction.Normalize();
            movementDisplacement = direction * GameSettings.BlockSize;
            lastState = State;
            State = BlockState.Moving;
        }

        public void Update(GameTime gameTime)
        {
            ManageSize(gameTime);
            ManageMovement(gameTime);
            bonus?.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            position += Origin + movementDisplacement;
            spriteBatch.Draw(texture, position, null, Color.White, 0f, Origin, size, SpriteEffects.None, 0f);
        }

        public void DrawBonus(SpriteBatch spriteBatch, Vector2 position)
        {
            position += Origin + movementDisplacement;
            bonus?.Draw(spriteBatch, position);
        }

        private void ManageMovement(GameTime gameTime)
        {
            if (State == BlockState.Moving)
            {
                var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.BlockSize * 2 *
                            GameSettings.AnimationSpeed;
                movementDisplacement = MyMath.MoveTowards(movementDisplacement, new Vector2(0), delta);
                if (movementDisplacement == new Vector2(0))
                {
                    State = lastState;
                }
            }
        }

        private void ManageSize(GameTime gameTime)
        {
            var targetSize = State == BlockState.Dead ? RottenSize : 1f;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * GameSettings.AnimationSpeed;
            size = MyMath.MoveTowards(size, targetSize, delta);
            if (Math.Abs(size - RottenSize) < float.Epsilon && !IsBonusActive())
                DeathTrial();
        }

        private void DeathTrial()
        {
            if (NextBonus is { })
            {
                bonus = NextBonus;
                NextBonus = null;
                State = BlockState.Idle;
                return;
            }

            State = BlockState.Rotten;
        }
    }
}