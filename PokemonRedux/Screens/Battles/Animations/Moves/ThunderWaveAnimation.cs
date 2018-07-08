using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class ThunderWaveAnimation : BattleAnimation
    {
        private const int FRAME_WIDTH = 44;
        private const int FRAME_HEIGHT = 40;
        private const int STAGE_DELAY_BUILDUP = 12;
        private const int STAGE_DELAY = 4;
        private const int TOTAL_STAGES = 5;
        private const int INVERT_DELAY = 10;
        private const int TOTAL_FLASHES = 20;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _stage = 0;
        private int _stageDelay = STAGE_DELAY_BUILDUP;
        private bool _inverted = false;
        private int _invertDelay = INVERT_DELAY;
        private bool _flashing = false;
        private int _flashes = 0;

        public ThunderWaveAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/thunderwave.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var inverted = _inverted || _flashing;
            Battle.ActiveBattle.AnimationController.SetScreenColorInvert(inverted);

            if (_stage < TOTAL_STAGES) // last stage is invisible electricity
            {
                var texX = _stage * FRAME_WIDTH;
                var texY = inverted ? FRAME_HEIGHT : 0;

                var frameWidth = (int)(FRAME_WIDTH * Border.SCALE);
                var frameHeight = (int)(FRAME_HEIGHT * Border.SCALE);
                var center = GetCenter(_target.Side);
                var x = (int)(center.X - frameWidth / 2f);
                var y = (int)(center.Y - frameHeight / 2f);
                batch.Draw(_texture, new Rectangle(x, y, frameWidth, frameHeight),
                    new Rectangle(texX, texY, FRAME_WIDTH, FRAME_HEIGHT), Color.White);
            }
        }

        public override void Update()
        {
            if (!_flashing)
            {
                _stageDelay--;
                if (_stageDelay == 0)
                {
                    _stage++;
                    if (_stage == TOTAL_STAGES - 1)
                    {
                        _flashing = true;
                        _stageDelay = STAGE_DELAY;
                    }
                    else
                    {
                        _stageDelay = STAGE_DELAY_BUILDUP;
                    }
                }

                _invertDelay--;
                if (_invertDelay == 0)
                {
                    _invertDelay = INVERT_DELAY;
                    _inverted = !_inverted;
                }
            }
            else
            {
                _stageDelay--;
                if (_stageDelay == 0)
                {
                    _stageDelay = STAGE_DELAY;
                    _stage++;
                    if (_stage == TOTAL_STAGES)
                    {
                        _stage -= 3;
                    }
                    _flashes++;
                    if (_flashes == TOTAL_FLASHES)
                    {
                        IsFinished = true;
                        // show status again and disable color inversion
                        Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
                        Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
                    }
                }
            }
        }
    }
}
