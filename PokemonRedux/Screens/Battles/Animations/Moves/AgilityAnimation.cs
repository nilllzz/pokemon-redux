using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class AgilityAnimation : BattleMoveAnimation
    {
        private const int TOTAL_PALETTE_SWAPS = 4;
        private const int PALETTE_SWAP_DELAY = 5;
        private const int STAGES_PER_SWAP = 5;
        private const int BEAM_SPAWN_CHANCE = 25; // %
        private const int CENTER_BEAM_SPEED_PER_FRAME = 6;

        private Color[] _palette1, _palette2;

        private int _swap = 0;
        private int _swapStage = 0;
        private int _swapDelay = PALETTE_SWAP_DELAY;
        private List<Vector2> _beams = new List<Vector2>();

        public AgilityAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            _palette1 = PokemonTextureManager.ShiftPalette(_user.Pokemon.GetPalette(), false, 1);
            _palette2 = PokemonTextureManager.ShiftPalette(_user.Pokemon.GetPalette(), false, 2);

            LoadTexture("agility");
        }

        public override void Draw(SpriteBatch batch)
        {
            var beamWidth = _texture.Width * Border.SCALE;
            var beamHeight = _texture.Height * Border.SCALE;

            // determine beam height range 
            var enemyCenter = GetCenter(PokemonSide.Enemy);
            var playerCenter = GetCenter(PokemonSide.Player);
            var pokemonSize = GetPokemonSpriteSize();
            var startY = enemyCenter.Y - pokemonSize / 2f;
            var endY = playerCenter.Y + pokemonSize / 2f;
            var beamFieldHeight = endY - startY;

            var screen = GetScreenValues();

            for (var i = 0; i < _beams.Count; i++)
            {
                var beam = _beams[i];
                if (beam.X >= screen.startX && beam.X <= screen.startX + Border.SCREEN_WIDTH * Border.SCALE * Border.UNIT)
                {
                    var normalizedY = beam.Y - startY;
                    var distanceToCenter = Math.Abs(beamFieldHeight / 2f - normalizedY) / beamFieldHeight * 2f;
                    var speed = CENTER_BEAM_SPEED_PER_FRAME * (1 + distanceToCenter);
                    if (_user.Side == PokemonSide.Enemy)
                    {
                        speed *= -1;
                    }

                    _beams[i] += new Vector2(speed * Border.SCALE, 0);

                    var posX = (int)(beam.X - beamWidth / 2f);
                    var posY = (int)(beam.Y - beamHeight / 2f);

                    batch.Draw(_texture, new Rectangle(posX, posY, (int)beamWidth, (int)beamHeight), Color.White);
                }
            }

            // spawn new beam
            if (_swap < TOTAL_PALETTE_SWAPS - 1 && Battle.ActiveBattle.Random.Next(0, 100) < BEAM_SPAWN_CHANCE)
            {
                var y = Battle.ActiveBattle.Random.NextDouble() * beamFieldHeight + startY;
                var x = 0f;
                if (_user.Side == PokemonSide.Enemy)
                {
                    x = screen.startX + Border.SCREEN_WIDTH * Border.SCALE * Border.UNIT;
                }
                else
                {
                    x = screen.startX;
                }
                _beams.Add(new Vector2(x, (float)y));
            }
        }

        public override void Update()
        {
            _swapDelay--;
            if (_swapDelay == 0)
            {
                _swapDelay = PALETTE_SWAP_DELAY;
                _swapStage++;
                if (_swapStage == STAGES_PER_SWAP)
                {
                    _swapStage = 0;
                    _swap++;
                    if (_swap == TOTAL_PALETTE_SWAPS)
                    {
                        Finish();
                    }
                }

                var palette = _user.Pokemon.GetPalette();
                switch (_swapStage)
                {
                    case 1:
                    case 3:
                        palette = _palette1;
                        break;
                    case 2:
                        palette = _palette2;
                        break;
                }
                Battle.ActiveBattle.AnimationController.SetPokemonPalette(_user.Side, palette);
            }
        }
    }
}
