using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class ReflectAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 13;
        private const int TOTAL_PASSES = 2;
        private const int STAGE_DELAY = 2;
        private const int REVERT_INVERT_FRAME = 6;
        private static readonly int[] ALTERNATIVE_COLOR_STAGES = new[] { 2, 3, 6, 7, 10, 11 };
        private static readonly Color MAIN_COLOR = new Color(248, 248, 56);
        private static readonly Color ALTERNATIVE_COLOR = new Color(248, 128, 8);

        private int _stageDelay = STAGE_DELAY;
        private int _invertDelay = REVERT_INVERT_FRAME;
        private int _stage = 0;
        private int _pass = 0;

        public ReflectAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("reflect");
        }

        public override void Show()
        {
            base.Show();

            Battle.ActiveBattle.AnimationController.SetScreenColorInvert(true);
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_user.Side);
            var frameWidth = 40 * Border.SCALE;
            var frameHeight = 56 * Border.SCALE;

            var posX = center.X;
            if (_user.Side == PokemonSide.Enemy)
            {
                posX -= frameWidth;
            }
            var posY = center.Y - frameHeight / 2f - Border.SCALE / 2f * Border.UNIT;

            Color color;
            if (ALTERNATIVE_COLOR_STAGES.Contains(_stage))
            {
                color = ALTERNATIVE_COLOR;
            }
            else
            {
                color = MAIN_COLOR;
            }
            // invert color
            if (_invertDelay > 0)
            {
                color = new Color(255 - color.R, 255 - color.G, 255 - color.B);
            }

            batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)frameWidth, (int)frameHeight),
                new Rectangle(40 * _stage, 0, 40, 56), color);
        }

        public override void Update()
        {
            if (_invertDelay > 0)
            {
                _invertDelay--;
            }
            Battle.ActiveBattle.AnimationController.SetScreenColorInvert(_invertDelay > 0);

            _stageDelay--;
            if (_stageDelay == 0)
            {
                _stageDelay = STAGE_DELAY;
                _stage++;
                if (_stage == TOTAL_STAGES)
                {
                    _pass++;
                    _stage = 0;
                    _invertDelay = REVERT_INVERT_FRAME;
                    if (_pass == TOTAL_PASSES)
                    {
                        Finish();
                        Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
                    }
                }
            }
        }
    }
}
