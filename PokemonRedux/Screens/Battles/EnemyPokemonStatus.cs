using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Battles
{
    // TODO: status
    class EnemyPokemonStatus
    {
        private const float ANIMATION_SPEED = 0.03f;
        private const int BAR_WIDTH = 48;
        private const int BAR_HEIGHT = 2;
        private const int BAR_OFFSET_X = 16;
        private const int BAR_OFFSET_Y = 3;

        private Texture2D _texture, _caughtIndicator;
        private PokemonFontRenderer _fontRenderer;

        public bool Visible { get; set; }
        public bool AnimationFinished => _animationState == 1f;
        public Vector2 Offset { get; set; } = Vector2.Zero;

        private float _animationState = 1f;
        private double _hpState;

        public void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Battle/enemyPokemonState.png");
            _caughtIndicator = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Battle/caughtIndicator.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _hpState = GetTargetHPState();
        }

        public void UnloadContent()
        {

        }

        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                var (unit, startX, width, height) = Border.GetDefaultScreenValues();
                startX = (int)(startX + Offset.X * Border.SCALE);
                var startY = (int)(BattleScreen.StartY + Offset.Y * Border.SCALE);

                var pokemon = Controller.ActiveBattle.EnemyPokemon.Pokemon;
                // name
                _fontRenderer.DrawText(batch, pokemon.DisplayName,
                    new Vector2(startX + unit, startY), Color.Black, Border.SCALE);

                // caught
                if (Controller.ActiveBattle.IsWildBattle &&
                    Controller.ActivePlayer.PokedexCaught.Contains(pokemon.Id))
                {
                    batch.Draw(_caughtIndicator, new Rectangle(startX + unit, startY + unit,
                        (int)(_caughtIndicator.Width * Border.SCALE),
                        (int)(_caughtIndicator.Height * Border.SCALE)), Color.White);
                }

                // level/gender
                string levelStr;
                if (pokemon.Level == Pokemon.MAX_LEVEL)
                {
                    levelStr = pokemon.Level.ToString();
                }
                else
                {
                    levelStr = "^:L" + pokemon.Level.ToString().PadRight(2);
                }
                _fontRenderer.DrawText(batch, levelStr + PokemonStatHelper.GetGenderChar(pokemon.Gender),
                    new Vector2(startX + unit * 6, startY + unit), Color.Black, Border.SCALE);

                // hp bar texture
                batch.Draw(_texture, new Rectangle(startX + unit, startY + unit * 2,
                    (int)(_texture.Width * Border.SCALE),
                    (int)(_texture.Height * Border.SCALE)), Color.White);

                // hp bar
                var barWidth = (int)(Math.Ceiling(BAR_WIDTH * Border.SCALE * GetCurrentHPState()));
                var barHeight = (int)(BAR_HEIGHT * Border.SCALE);
                var hp = (int)(GetCurrentHPState() * pokemon.MaxHP);

                var barColor = PokemonStatHelper.GetHPBarColor(PokemonStatHelper.GetPokemonHealth(hp, pokemon.MaxHP));
                batch.DrawRectangle(new Rectangle(
                    (int)(BAR_OFFSET_X * Border.SCALE) + startX + unit * 2,
                    (int)(BAR_OFFSET_Y * Border.SCALE) + startY + unit * 2,
                    barWidth, barHeight), barColor);
            }
        }

        public void Update()
        {
            if (Visible)
            {
                if (_animationState < 1f)
                {
                    _animationState += ANIMATION_SPEED;
                    if (_animationState >= 1f)
                    {
                        _animationState = 1f;
                        _hpState = GetTargetHPState();
                    }
                }
            }
        }

        public void AnimateToTarget()
        {
            _animationState = 0;
        }

        private double GetCurrentHPState()
        {
            var diff = _hpState - GetTargetHPState();
            return _hpState - diff * _animationState;
        }

        private double GetTargetHPState()
        {
            var pokemon = Controller.ActiveBattle.EnemyPokemon.Pokemon;
            return pokemon.HP / (double)pokemon.MaxHP;
        }
    }
}
