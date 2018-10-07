using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class CharmAnimation : BattleMoveAnimation
    {
        private const float HEART_PROGRESS_PER_FRAME = 0.02f;
        private const float POKEMON_PROGRESS_PER_FRAME = 0.03f;

        private float _heartProgress = 0f;
        private float _pokemonProgress = 0f;

        public CharmAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("charm");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_user.Side);
            var frameWidth = _texture.Width * Border.SCALE;
            var frameHeight = _texture.Height * Border.SCALE;
            var pokemonSize = GetPokemonSpriteSize();
            var startX = center.X;
            if (_user.Side == PokemonSide.Player)
            {
                startX += pokemonSize / 2f - frameWidth;
            }
            else
            {
                startX -= pokemonSize / 2f;
            }

            var waveHeight = Border.SCALE * Border.UNIT * 0.5f;
            var posX = startX + Math.Sin(_heartProgress * MathHelper.Pi * 3) * waveHeight;
            var posY = center.Y - frameHeight - frameHeight * 2 * _heartProgress;

            batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)frameWidth, (int)frameHeight), Color.White);
        }

        public override void Update()
        {
            if (_heartProgress < 1f)
            {
                _heartProgress += HEART_PROGRESS_PER_FRAME;
                if (_heartProgress >= 1f)
                {
                    _heartProgress = 1f;
                    Finish();
                    Battle.ActiveBattle.AnimationController.SetPokemonOffset(_user.Side, Vector2.Zero);
                }
            }

            if (_pokemonProgress < 1f)
            {
                _pokemonProgress += POKEMON_PROGRESS_PER_FRAME;
                if (_pokemonProgress >= 1f)
                {
                    _pokemonProgress = 1f;
                }
                var offset = new Vector2((float)Math.Sin(_pokemonProgress * 4f * MathHelper.Pi) * Border.SCALE * Border.UNIT, 0);
                Battle.ActiveBattle.AnimationController.SetPokemonOffset(_user.Side, offset);
            }
        }
    }
}
