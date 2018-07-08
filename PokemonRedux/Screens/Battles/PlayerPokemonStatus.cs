using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using static Core;

namespace PokemonRedux.Screens.Battles
{
    // TODO: status
    class PlayerPokemonStatus
    {
        private const float ANIMATION_SPEED = 0.03f;
        private const int EXP_BAR_WIDTH = 64;
        private const int HP_BAR_WIDTH = 48;
        private const int HP_BAR_OFFSET_X = 24;
        private const int BAR_OFFSET_Y = 3;
        private const int BAR_HEIGHT = 2;
        private static readonly Color EXPBAR_COLOR = new Color(32, 136, 248);

        private Texture2D _texture;
        private PokemonFontRenderer _fontRenderer;

        public bool Visible { get; set; }
        public bool AnimationFinished => _animationState == 1f;
        public Vector2 Offset { get; set; } = Vector2.Zero;

        private float _animationState = 1f;
        private double _hpState;

        public void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Battle/playerPokemonState.png");

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

                var pokemon = Controller.ActiveBattle.PlayerPokemon.Pokemon;

                // name
                _fontRenderer.DrawText(batch, pokemon.DisplayName,
                    new Vector2(startX + unit * 10, startY + unit * 7), Color.Black, Border.SCALE);

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
                    new Vector2(startX + unit * 14, startY + unit * 8), Color.Black, Border.SCALE);

                // main bar texture
                batch.Draw(_texture, new Rectangle(startX + unit * 9, startY + unit * 9,
                    (int)(_texture.Width * Border.SCALE),
                    (int)(_texture.Height * Border.SCALE)), Color.White);

                // hp
                var hp = (int)(GetCurrentHPState() * pokemon.MaxHP);
                var hpStr = hp.ToString().PadLeft(3) + "/" + pokemon.MaxHP.ToString().PadLeft(3);
                _fontRenderer.DrawText(batch, hpStr,
                    new Vector2(startX + unit * 11, startY + unit * 10), Color.Black, Border.SCALE);

                // hp bar
                var barWidth = (int)(Math.Ceiling(HP_BAR_WIDTH * Border.SCALE * GetCurrentHPState()));
                var barHeight = (int)(BAR_HEIGHT * Border.SCALE);

                var barColor = PokemonStatHelper.GetHPBarColor(PokemonStatHelper.GetPokemonHealth(hp, pokemon.MaxHP));
                batch.DrawRectangle(new Rectangle(
                    (int)(HP_BAR_OFFSET_X * Border.SCALE) + startX + unit * 9,
                    (int)(BAR_OFFSET_Y * Border.SCALE) + startY + unit * 9,
                    barWidth, barHeight), barColor);

                // exp bar
                if (pokemon.Level < Pokemon.MAX_LEVEL)
                {
                    var expCurrentLv = PokemonStatHelper.GetExperienceForLevel(pokemon.ExperienceType, pokemon.Level);
                    var expProgress = (double)(pokemon.Experience - expCurrentLv) /
                        (PokemonStatHelper.GetExperienceForLevel(pokemon.ExperienceType, pokemon.Level + 1) - expCurrentLv);
                    var expBarWidth = (int)(Math.Ceiling(EXP_BAR_WIDTH * Border.SCALE * expProgress));
                    batch.DrawRectangle(new Rectangle(
                        (int)(startX + unit * 10 + EXP_BAR_WIDTH * Border.SCALE - expBarWidth),
                        (int)(startY + unit * 11 + Border.SCALE * BAR_OFFSET_Y),
                        expBarWidth,
                        (int)(Border.SCALE * BAR_HEIGHT)), EXPBAR_COLOR);
                }
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
            var pokemon = Controller.ActiveBattle.PlayerPokemon.Pokemon;
            return pokemon.HP / (double)pokemon.MaxHP;
        }
    }
}
