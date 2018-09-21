using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class StrengthAnimation : BattleAnimation
    {
        private static readonly int[] SCREEN_SHAKE = new[] { -1, -1, -1, 1, 1, 1, -1, -1, -1, 1, 1, 1, -1, -1, -1 };
        private const float RAISE_PER_FRAME = 1 / 70f;
        private static readonly int[] ROCK_SHAKE = new[] { 0, -1, 1, -1, 1, -1, 1, -1, 1, -1, 1, -1, 0 };
        private const int ROCK_SHAKE_DELAY = 5;
        private const float THROW_PER_FRAME = 1 / 20f;
        private const int HIT_FRAMES = 10;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _shakeFrame = 0;
        private float _raiseState = 0f;
        private int _rockShakeState = 0;
        private int _rockShakeDelay = ROCK_SHAKE_DELAY;
        private float _throwState = 0f;
        private int _hitState = 0;

        public StrengthAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/strength.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var targetCenter = GetCenter(_target.Side);
            if (_throwState < 1f)
            {
                var userCenter = GetCenter(BattlePokemon.ReverseSide(_target.Side));
                var pokemonSize = GetPokemonSpriteSize();
                var rockWidth = 30 * Border.SCALE;
                var rockHeight = 24 * Border.SCALE;

                var rockX = userCenter.X - (_target.Side == PokemonSide.Enemy ? 0 : rockWidth);
                var rockY = userCenter.Y - _raiseState * pokemonSize * 0.5f;
                rockY += ROCK_SHAKE[_rockShakeState] * Border.SCALE;

                var throwTarget = new Vector2(targetCenter.X - rockWidth / 2f, targetCenter.Y - pokemonSize * 0.4f);
                rockX += (throwTarget.X - rockX) * _throwState;
                rockY += (throwTarget.Y - rockY) * _throwState;

                batch.Draw(_texture, new Rectangle((int)rockX, (int)rockY, (int)rockWidth, (int)rockHeight),
                    new Rectangle(0, 0, 30, 24), Color.White);
            }
            if (_hitState > 1)
            {
                var frameSize = 32 * Border.SCALE;
                var posX = (int)(targetCenter.X - frameSize / 2f);
                var posY = (int)(targetCenter.Y - frameSize / 2f);

                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                    new Rectangle(32, 0, 32, 32), Color.White);
            }
        }

        public override void Update()
        {
            if (_raiseState < 1f)
            {
                _raiseState += RAISE_PER_FRAME;
                if (_raiseState >= 1f)
                {
                    _raiseState = 1f;
                }

                // screen shake
                _shakeFrame++;
                if (_shakeFrame < SCREEN_SHAKE.Length)
                {
                    var yOffsetDirection = SCREEN_SHAKE[_shakeFrame];
                    Battle.ActiveBattle.AnimationController.SetScreenOffset(new Vector2(0, yOffsetDirection * Border.SCALE));
                }
                else
                {
                    Battle.ActiveBattle.AnimationController.SetScreenOffset(Vector2.Zero);
                }
            }
            else
            {
                if (_rockShakeState < ROCK_SHAKE.Length - 1)
                {
                    _rockShakeDelay--;
                    if (_rockShakeDelay == 0)
                    {
                        _rockShakeDelay = ROCK_SHAKE_DELAY;
                        _rockShakeState++;
                    }
                }
                else
                {
                    if (_throwState < 1f)
                    {
                        _throwState += THROW_PER_FRAME;
                        if (_throwState >= 1f)
                        {
                            _throwState = 1f;
                        }
                    }
                    else
                    {
                        _hitState++;
                        if (_hitState == HIT_FRAMES)
                        {
                            IsFinished = true;
                            // show status again
                            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
                        }
                    }
                }
            }
        }
    }
}
