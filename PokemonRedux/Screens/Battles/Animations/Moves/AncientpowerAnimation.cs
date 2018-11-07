using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class AncientpowerAnimation : BattleMoveAnimation
    {
        private const int ROCK_AMOUNT = 7;
        private const float PROGRESS_PER_FRAME = 0.03f;
        private const int HIT_DELAY = 10;

        private readonly List<float> _rocks = new List<float>();
        private int _hitDelay = 0;

        public AncientpowerAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("ancientpower");

            _rocks.Add(0f);
        }

        public override void Draw(SpriteBatch batch)
        {
            var targetCenter = GetCenter(_target.Side);
            var hitSize = 32 * Border.SCALE;
            var pokemonSize = GetPokemonSpriteSize();

            if (_hitDelay > 0)
            {
                var hitX = (int)(targetCenter.X - hitSize / 2f);
                var hitY = (int)(targetCenter.Y + pokemonSize / 2f - hitSize);
                batch.Draw(_texture, new Rectangle(hitX, hitY, (int)hitSize, (int)hitSize),
                    new Rectangle(16, 0, 32, 32), Color.White);
            }

            var userCenter = GetCenter(_user.Side);
            var rocksStartX = userCenter.X;
            var rocksEndX = targetCenter.X;
            var rockSize = 16 * Border.SCALE;
            if (_target.Side == PokemonSide.Enemy)
            {
                rocksStartX += pokemonSize / 2f - rockSize;
                rocksEndX -= rockSize;
            }
            else
            {
                rocksStartX -= pokemonSize / 2f;
            }
            var rocksStartY = userCenter.Y - pokemonSize / 2f - rockSize / 2f;
            var rocksEndY = targetCenter.Y - rockSize / 2f;

            for (var i = 0; i < _rocks.Count; i++)
            {
                var progress = _rocks[i];
                if (progress < 1f)
                {
                    var rockX = (int)(rocksStartX + (rocksEndX - rocksStartX) * ((float)i / (ROCK_AMOUNT - 1)));
                    var rockYNormal = rocksStartY + (rocksEndY - rocksStartY) * ((float)i / (ROCK_AMOUNT - 1));
                    var rockYOffset = (pokemonSize - rockSize) * (float)(1 - Math.Sin(progress * MathHelper.Pi));
                    var rockY = (int)(rockYNormal + rockYOffset);

                    batch.Draw(_texture, new Rectangle(rockX, rockY, (int)rockSize, (int)rockSize),
                        new Rectangle(0, 0, 16, 16), Color.White);
                }
            }
        }

        public override void Update()
        {
            if (_hitDelay > 0)
            {
                _hitDelay--;
                if (_hitDelay == 0)
                {
                    Finish();
                }
            }

            for (var i = 0; i < _rocks.Count; i++)
            {
                if (_rocks[i] < 1f)
                {
                    _rocks[i] += PROGRESS_PER_FRAME;
                    if (_rocks[i] >= 1f)
                    {
                        _rocks[i] = 1f;
                    }
                    else if (_rocks.Count < ROCK_AMOUNT && i == _rocks.Count - 1 && _rocks[i] >= 0.375f)
                    {
                        // spawn new rock
                        _rocks.Add(0f);
                    }
                    else if (_rocks.Count == ROCK_AMOUNT && _rocks[i] >= 0.375f && _hitDelay == 0)
                    {
                        // show hit
                        _hitDelay = HIT_DELAY;
                    }
                }
            }
        }
    }
}
