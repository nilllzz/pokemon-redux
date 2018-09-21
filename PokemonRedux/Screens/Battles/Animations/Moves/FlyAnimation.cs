using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    // basically two animations:
    // the beam animation for when the move starts, the hit animation for when it lands
    class FlyAnimation : BattleAnimation
    {
        private readonly static int[] BEAM_FRAMES = new[] { 0, 0, 1, 3, 2, 4, 5, 7, 6, 6, 7 };
        private const int BEAM_EMPTY_FRAMES = 20;
        private const int HIT_FRAMES = 15;

        private readonly BattlePokemon _user, _target;

        private Texture2D _texture;

        private int _beamState = 0;
        private int _beamEmptyFrames = BEAM_EMPTY_FRAMES;

        private int _hitFrames = HIT_FRAMES;

        private bool DoBeamAnimation => !_user.IsFlying; // flying state change happens after animation

        public FlyAnimation(BattlePokemon user, BattlePokemon target)
        {
            _user = user;
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/fly.png");
        }

        public override void Show()
        {
            if (Controller.GameOptions.BattleAnimations)
            {
                // hide status of user, only with battle animations enabled
                Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
            }
            if (DoBeamAnimation)
            {
                // hide user
                Battle.ActiveBattle.AnimationController.SetPokemonVisibility(_user.Side, false);
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            if (DoBeamAnimation)
            {
                if (_beamState < BEAM_FRAMES.Length)
                {
                    var center = GetCenter(_user.Side);
                    var frameWidth = 32 * Border.SCALE;
                    var frameHeight = 56 * Border.SCALE;
                    var posX = (int)(center.X - frameWidth / 2f);
                    var posY = (int)(center.Y - frameHeight / 2f);

                    batch.Draw(_texture, new Rectangle(posX, posY, (int)frameWidth, (int)frameHeight),
                        new Rectangle(BEAM_FRAMES[_beamState] * 32, 0, 32, 56), Color.White);
                }
            }
            else
            {
                var center = GetCenter(_target.Side);
                var frameSize = 24 * Border.SCALE;
                var posX = (int)(center.X - frameSize / 2f);
                var posY = (int)center.Y;

                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                    new Rectangle(0, 56, 24, 24), Color.White);
            }
        }

        public override void Update()
        {
            if (!Controller.GameOptions.BattleAnimations)
            {
                // if animations are disabled, finish the animation directly
                Finish();
                return;
            }

            if (DoBeamAnimation)
            {
                if (_beamState < BEAM_FRAMES.Length)
                {
                    _beamState++;
                }
                else
                {
                    if (_beamEmptyFrames > 0)
                    {
                        _beamEmptyFrames--;
                    }
                    else
                    {
                        Finish();
                    }
                }
            }
            else
            {
                _hitFrames--;
                if (_hitFrames == 0)
                {
                    Finish();
                }
            }
        }

        private void Finish()
        {
            IsFinished = true;
            // show status again
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
            if (!DoBeamAnimation)
            {
                // show user when the second part of the animation plays
                Battle.ActiveBattle.AnimationController.SetPokemonVisibility(_user.Side, true);
            }
        }
    }
}
