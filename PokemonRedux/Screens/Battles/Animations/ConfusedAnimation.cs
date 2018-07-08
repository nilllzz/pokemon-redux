using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    // animation with birds to show a pokemon is confused
    class ConfusedAnimation : BattleAnimation
    {
        private class Bird
        {
            public float X;
            public bool Direction; // true => right, false => left
        }

        private const float BIRD_SPEED = 0.04f;
        private const int FRAME_SIZE = 14;
        private const int TOTAL_TURNS = 6;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private Bird[] _birds;
        private int _totalTurns;

        public ConfusedAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/confused.png");

            _birds = new[]
            {
                new Bird { X = 0, Direction = true },
                new Bird { X = 0.8f, Direction = true },
                new Bird { X = 0.5f, Direction = false }
            };
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            var frameSize = (int)(FRAME_SIZE * Border.SCALE);
            foreach (var bird in _birds)
            {
                var pos = center;
                pos.X -= GetPokemonSpriteSize() / 4f + frameSize / 2f;
                pos.X += GetPokemonSpriteSize() / 2f * bird.X;

                var offset = Math.Abs(bird.X - 0.5f);
                if (bird.Direction)
                {
                    offset = Math.Abs(offset - 1f);
                }
                pos.Y -= GetPokemonSpriteSize() / 2f + frameSize;
                pos.Y += offset * frameSize * 0.5f;

                var effects = SpriteEffects.None;
                if (bird.Direction)
                {
                    effects = SpriteEffects.FlipHorizontally;
                }
                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, frameSize, frameSize),
                    null, Color.White, 0f, Vector2.Zero, effects, 0f);
            }
        }

        public override void Update()
        {
            foreach (var bird in _birds)
            {
                if (bird.Direction)
                {
                    bird.X += BIRD_SPEED;
                    if (bird.X >= 1f)
                    {
                        bird.X = 1f;
                        bird.Direction = false;
                    }
                }
                else
                {
                    bird.X -= BIRD_SPEED;
                    if (bird.X <= 0f)
                    {
                        bird.X = 0f;
                        bird.Direction = true;
                        _totalTurns++;
                        if (_totalTurns == TOTAL_TURNS)
                        {
                            IsFinished = true;
                        }
                    }
                }
            }
        }
    }
}
