using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class RazorLeafAnimation : BattleMoveAnimation
    {
        private const float RISE_PROGRESS_PER_FRAME = 0.05f;
        private const float FALL_PROGRESS_PER_FRAME = 0.013f;
        private const float FALL_X_PROGRESS_PER_FRAME = FALL_PROGRESS_PER_FRAME * 2.5f;
        private const int TOTAL_LEAF_X_OFFSET = 32;
        private const int TOTAL_LEAF_Y_OFFSET = 40;
        private const int ATTACK_MOVE_PER_FRAME = 8;
        private const int SECOND_WAVE_DELAY = 8;

        enum LeafState
        {
            Rising,
            Falling,
            Attacking
        }

        class Leaf
        {
            public const int SIZE = 8;
            private const int ATTACK_FRAMES = 16;

            public LeafState State = LeafState.Rising;
            public Vector2 Position;

            // rising
            public float RiseProgress = 0f;
            public Vector2 StartPosition;
            public Vector2 TargetPosition;

            // falling
            public float FallYProgress = 0f;
            public float FallXProgress = 0f;
            public int FallDelay = 0; // keeps falling for x amount of frames

            // attacking
            public int AttackFrames = ATTACK_FRAMES;
        }

        private readonly List<Leaf> _leafs = new List<Leaf>();
        private int _secondWaveDelay = SECOND_WAVE_DELAY;

        public RazorLeafAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("razorleaf");

            // add starting leafs
            var center = GetCenter(_user.Side);
            var leafSize = Leaf.SIZE * Border.SCALE;
            var startPos = center - new Vector2(leafSize / 2f);
            // top left
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(-21, -47) * Border.SCALE,
                FallXProgress = 1f,
                FallDelay = 16 // top left
            });
            // middle left
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(-30, -35) * Border.SCALE,
                FallXProgress = 0f,
                FallDelay = 4 // 2nd
            });
            // lower left
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(-38, -23) * Border.SCALE,
                FallXProgress = 1f,
                FallDelay = 20
            });
            // top right
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(13, -47) * Border.SCALE,
                FallXProgress = 0f,
                FallDelay = 12 // 4th
            });
            // middle right
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(13, -35) * Border.SCALE,
                FallXProgress = 1f,
                FallDelay = 20
            });
            // lower right
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(30, -23) * Border.SCALE,
                FallXProgress = 0f,
                FallDelay = 0 // first to attack
            });
        }

        public override void Draw(SpriteBatch batch)
        {
            var leafSize = (int)(Leaf.SIZE * Border.SCALE);
            foreach (var leaf in _leafs.Where(l => l.AttackFrames > 0))
            {
                var textureRectangle = default(Rectangle);
                var effect = SpriteEffects.None;
                if (leaf.State == LeafState.Rising || leaf.State == LeafState.Attacking)
                {
                    textureRectangle = new Rectangle(0, 0, Leaf.SIZE, Leaf.SIZE);
                    if (_user.Side == PokemonSide.Enemy)
                    {
                        effect = SpriteEffects.FlipHorizontally;
                        if (leaf.State == LeafState.Attacking)
                        {
                            effect |= SpriteEffects.FlipVertically;
                        }
                    }
                }
                else
                {
                    var progress = Math.Sin(MathHelper.Pi * leaf.FallXProgress);
                    if (progress < 0.2f && progress > -0.2f)
                    {
                        textureRectangle = new Rectangle(8, 8, 8, 8);
                    }
                    else if (progress >= 0.2f && progress < 0.8f)
                    {
                        textureRectangle = new Rectangle(0, 8, 8, 8);
                        effect = SpriteEffects.FlipHorizontally;
                    }
                    else if (progress >= 0.8f)
                    {
                        textureRectangle = new Rectangle(8, 0, 8, 8);
                        effect = SpriteEffects.FlipHorizontally;
                    }
                    else if (progress <= -0.2f && progress > -0.8f)
                    {
                        textureRectangle = new Rectangle(0, 8, 8, 8);
                    }
                    else if (progress <= -0.8f)
                    {
                        textureRectangle = new Rectangle(8, 0, 8, 8);
                    }
                }
                batch.Draw(_texture, new Rectangle((int)leaf.Position.X, (int)leaf.Position.Y, leafSize, leafSize),
                    textureRectangle, Color.White, 0f, Vector2.Zero, effect, 0f);
            }
        }

        public override void Update()
        {
            if (_secondWaveDelay > 0)
            {
                _secondWaveDelay--;
                if (_secondWaveDelay == 0)
                {
                    SpawnSecondWave();
                }
            }

            foreach (var leaf in _leafs)
            {
                if (leaf.State == LeafState.Rising)
                {
                    leaf.RiseProgress += RISE_PROGRESS_PER_FRAME;
                    if (leaf.RiseProgress >= 1f)
                    {
                        leaf.RiseProgress = 1f;
                        leaf.State = LeafState.Falling;
                        leaf.Position = leaf.TargetPosition;
                    }
                    else
                    {
                        leaf.Position = leaf.StartPosition + (leaf.TargetPosition - leaf.StartPosition) * leaf.RiseProgress;
                    }
                }
                else if (leaf.State == LeafState.Falling)
                {
                    leaf.FallXProgress += FALL_X_PROGRESS_PER_FRAME;
                    leaf.FallYProgress += FALL_PROGRESS_PER_FRAME;

                    if (leaf.FallYProgress >= 1f)
                    {
                        if (leaf.FallDelay > 0)
                        {
                            leaf.FallDelay--;
                        }
                        else
                        {
                            leaf.State = LeafState.Attacking;
                        }
                    }

                    var offset = Vector2.Zero;
                    offset.Y = leaf.FallYProgress * TOTAL_LEAF_Y_OFFSET;
                    offset.X = (float)Math.Sin(leaf.FallXProgress * MathHelper.Pi) * (TOTAL_LEAF_X_OFFSET / 2f);

                    leaf.Position = leaf.TargetPosition + offset * Border.SCALE;
                }
                else if (leaf.State == LeafState.Attacking)
                {
                    var xMove = ATTACK_MOVE_PER_FRAME;
                    var yMove = -ATTACK_MOVE_PER_FRAME * 0.75f;
                    var move = new Vector2(xMove, yMove) * Border.SCALE;
                    if (_user.Side == PokemonSide.Enemy)
                    {
                        move *= -1f;
                    }
                    leaf.Position += move;

                    if (leaf.AttackFrames > 0)
                    {
                        leaf.AttackFrames--;
                    }
                }
            }

            if (_leafs.All(l => l.AttackFrames == 0))
            {
                Finish();
            }
        }

        private void SpawnSecondWave()
        {
            var center = GetCenter(_user.Side);
            var leafSize = Leaf.SIZE * Border.SCALE;
            var startPos = center - new Vector2(leafSize / 2f);
            // middle left
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(-30, -35) * Border.SCALE,
                FallXProgress = 0f,
                FallDelay = 4
            });
            // lower left
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(-38, -23) * Border.SCALE,
                FallXProgress = 1f,
                FallDelay = 12
            });
            // middle right
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(13, -35) * Border.SCALE,
                FallXProgress = 1f,
                FallDelay = 8
            });
            // lower right
            _leafs.Add(new Leaf
            {
                StartPosition = startPos,
                TargetPosition = center + new Vector2(30, -23) * Border.SCALE,
                FallXProgress = 0f,
                FallDelay = 0
            });
        }
    }
}
