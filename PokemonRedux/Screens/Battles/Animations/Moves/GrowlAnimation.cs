using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class GrowlAnimation : BattleMoveAnimation
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

        private int _waveStage = 0;
        private int _waveRepeat = 0;
        private int _waveAnimationDelay = WAVE_ANIMATION_DELAY;
        private float _pokemonColor = 1f;
        private int _colorFlickers = 0;

        public GrowlAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("growl");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_waveRepeat < WAVE_REPEATS)
            {
                var center = GetCenter(_user.Side);
                var waveWidth = (int)(_texture.Width / 2f * Border.SCALE);

                var x = (int)center.X;
                var y = (int)center.Y;

                if (_target.Side == PokemonSide.Player)
                {
                    x -= waveWidth;
                    x -= (int)(_waveStage * 3 * Border.SCALE);
                }
                else
                {
                    x += (int)(_waveStage * 3 * Border.SCALE);
                }

                var effects = _target.Side == PokemonSide.Enemy ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

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
                    Finish();
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
