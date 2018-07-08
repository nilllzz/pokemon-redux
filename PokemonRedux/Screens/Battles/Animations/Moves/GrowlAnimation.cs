using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class GrowlAnimation : BattleAnimation
    {
        /**
         * Animation description
         * 
         * three growl "waves" emit from the user (player -> right, enemy -> left)
         * visible in 5 stages, alternating colors
         * repeat 3 times
         * afterwards, the target shimmers in dark color
         */

        private const int WAVE_STAGES = 10;
        private const int WAVE_COLORS = 2;
        private const int WAVE_REPEATS = 3;
        private const int WAVE_ANIMATION_DELAY = 2;
        private const int COLOR_FLICKERS = 6;
        private const float COLOR_SPEED = 0.1f;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _waveStage = 0;
        private int _waveRepeat = 0;
        private int _waveAnimationDelay = WAVE_ANIMATION_DELAY;
        private float _pokemonColor = 1f;
        private int _colorFlickers = 0;

        public GrowlAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/growl.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_waveRepeat < WAVE_REPEATS)
            {
                var side = BattlePokemon.ReverseSide(_target.Side);
                var center = GetCenter(side);
                var waveWidth = (int)(_texture.Width / 2f * Border.SCALE);

                var x = (int)center.X;
                var y = (int)center.Y;

                if (side == PokemonSide.Enemy)
                {
                    x -= waveWidth;
                    x -= (int)(_waveStage * 3 * Border.SCALE);
                }
                else
                {
                    x += (int)(_waveStage * 3 * Border.SCALE);
                }

                var effects = side == PokemonSide.Player ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                var waveColor = (_waveStage / 2) % 2;

                // center wave
                var centerWaveHeight = (int)(8 * Border.SCALE);
                batch.Draw(_texture, new Rectangle(x, y - centerWaveHeight / 2, waveWidth, centerWaveHeight),
                    new Rectangle(waveColor * 24, 20, 24, 8), Color.White);

                var outerWaveOffset = (int)(_waveStage * 2 * Border.SCALE);
                var outerWaveHeight = (int)(20 * Border.SCALE);
                // upper wave
                batch.Draw(_texture, new Rectangle(x, y - outerWaveHeight - outerWaveOffset - centerWaveHeight / 2, waveWidth, outerWaveHeight),
                    new Rectangle(waveColor * 24, 0, 24, 20), Color.White, 0f, Vector2.Zero, effects, 0f);
                // lower wave
                batch.Draw(_texture, new Rectangle(x, y + outerWaveOffset + centerWaveHeight / 2, waveWidth, outerWaveHeight),
                    new Rectangle(waveColor * 24, 28, 24, 20), Color.White, 0f, Vector2.Zero, effects, 0f);
            }
        }

        public override void Update()
        {
            if (_waveRepeat == WAVE_REPEATS)
            {
                if (_colorFlickers % 2 == 0)
                {
                    _pokemonColor -= COLOR_SPEED;
                    if (_pokemonColor <= 0f)
                    {
                        _pokemonColor = 0f;
                        _colorFlickers++;
                    }
                }
                else
                {
                    _pokemonColor += COLOR_SPEED;
                    if (_pokemonColor >= 1f)
                    {
                        _pokemonColor = 1f;
                        _colorFlickers++;
                    }
                }

                var color = (int)(255 * _pokemonColor);
                Battle.ActiveBattle.AnimationController.SetPokemonColor(_target.Side, new Color(color, color, color));
                if (_colorFlickers == COLOR_FLICKERS)
                {
                    IsFinished = true;
                    // show status again
                    Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
                }
            }
            else
            {
                _waveAnimationDelay--;
                if (_waveAnimationDelay == 0)
                {
                    _waveAnimationDelay = WAVE_ANIMATION_DELAY;
                    _waveStage++;
                    if (_waveStage == WAVE_STAGES)
                    {
                        _waveStage = 0;
                        _waveRepeat++;
                    }
                }
            }
        }
    }
}
