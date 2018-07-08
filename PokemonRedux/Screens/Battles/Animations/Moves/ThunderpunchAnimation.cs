using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class ThunderpunchAnimation : BattleAnimation
    {
        private const int FLASH_STAGES = 4;
        private const int FLASH_ANIMATION_DELAY = 6;
        private const int PUNCH_POSITIONS = 2;
        private const int PUNCH_STAGES = 2;
        private const int PUNCH_ANIMATION_DELAY = 2;
        private const int TOTAL_PUNCHES = 6;
        private const int FLASH_WIDTH = 21;
        private const int FLASH_HEIGHT = 56;
        private const int PUNCH_SIZE = 24;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _flashStage = 0;
        private int _flashDelay = FLASH_ANIMATION_DELAY;
        private bool _punchVisible = false;
        private int _punchPosition = 0;
        private int _punchStage = 0;
        private int _punchDelay = PUNCH_ANIMATION_DELAY;
        private int _totalPunches = 0;

        public ThunderpunchAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/thunderpunch.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
            Battle.ActiveBattle.AnimationController.SetScreenColorInvert(true);
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);

            // flash
            var flashWidth = (int)(FLASH_WIDTH * Border.SCALE);
            var flashHeight = (int)(FLASH_HEIGHT * Border.SCALE);
            int flashX;
            if (_target.Side == PokemonSide.Enemy)
            {
                flashX = (int)(center.X + GetPokemonSpriteSize() / 2f - flashWidth);
            }
            else
            {
                flashX = (int)(center.X - GetPokemonSpriteSize() / 2f);
            }
            var flashY = (int)(center.Y - flashHeight / 2f);
            batch.Draw(_texture, new Rectangle(flashX, flashY, flashWidth, flashHeight),
                new Rectangle(_flashStage * FLASH_WIDTH, 0, FLASH_WIDTH, FLASH_HEIGHT), Color.White);

            // punch
            if (_punchVisible)
            {
                var punchSize = (int)(PUNCH_SIZE * Border.SCALE);
                int punchX;
                if (_punchPosition == 0)
                {
                    punchX = (int)(center.X - punchSize / 2f);
                }
                else
                {
                    if (_target.Side == PokemonSide.Enemy)
                    {
                        punchX = (int)center.X;
                    }
                    else
                    {
                        punchX = (int)(center.X - punchSize);
                    }
                }
                var punchY = (int)center.Y;
                batch.Draw(_texture, new Rectangle(punchX, punchY, punchSize, punchSize),
                    new Rectangle(_punchStage * PUNCH_SIZE, FLASH_HEIGHT, PUNCH_SIZE, PUNCH_SIZE), Color.White);
            }
        }

        public override void Update()
        {
            if (_flashStage < FLASH_STAGES - 1)
            {
                _flashDelay--;
                if (_flashDelay == 0)
                {
                    _flashDelay = FLASH_ANIMATION_DELAY;
                    _flashStage++;
                    if (_flashStage == FLASH_STAGES - 1)
                    {
                        Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
                    }
                }
            }

            _punchDelay--;
            if (_punchDelay == 0)
            {
                _punchDelay = PUNCH_ANIMATION_DELAY;
                if (!_punchVisible)
                {
                    _punchVisible = true;
                }
                else
                {
                    _punchStage++;
                    if (_punchStage == PUNCH_STAGES)
                    {
                        _punchStage = 0;
                        _punchVisible = false;
                        _totalPunches++;
                        _punchPosition++;
                        if (_punchPosition == PUNCH_POSITIONS)
                        {
                            _punchPosition = 0;
                        }
                        if (_totalPunches == TOTAL_PUNCHES)
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
