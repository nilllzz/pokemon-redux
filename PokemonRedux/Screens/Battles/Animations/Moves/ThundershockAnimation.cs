using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class ThundershockAnimation : BattleMoveAnimation
    {
        private const int CIRCLE_DELAY = 3;
        private const int THUNDER_INITIAL_DELAY = CIRCLE_DELAY * 6;
        private const int THUNDER_STAGES = 2;
        private const int THUNDER_DELAY = 5;
        private const int TOTAL_THUNDER_STAGES = 21;
        private const int CIRCLE_SIZE = 16;
        private const int THUNDER_SIZE = 40;

        private bool _circleVisible = false;
        private int _circleDelay = CIRCLE_DELAY;
        private bool _thunderVisible = false;
        private int _thunderStage = -1;
        private int _totalThunderStages = 0;
        private int _thunderDelay = THUNDER_INITIAL_DELAY;

        public ThundershockAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("thundershock");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            if (_circleVisible)
            {
                var circleSize = CIRCLE_SIZE * Border.SCALE;
                var x = (int)(center.X - circleSize / 2);
                var y = (int)(center.Y - circleSize / 2);
                batch.Draw(_texture, new Rectangle(x, y, (int)circleSize, (int)circleSize),
                    new Rectangle(80, 0, CIRCLE_SIZE, CIRCLE_SIZE), Color.White);
            }
            if (_thunderVisible)
            {
                var thunderSize = THUNDER_SIZE * Border.SCALE;
                var x = (int)(center.X - thunderSize / 2);
                var y = (int)(center.Y - thunderSize / 2);
                batch.Draw(_texture, new Rectangle(x, y, (int)thunderSize, (int)thunderSize),
                    new Rectangle(THUNDER_SIZE * _thunderStage, 0, THUNDER_SIZE, THUNDER_SIZE), Color.White);
            }
        }

        public override void Update()
        {
            _circleDelay--;
            if (_circleDelay == 0)
            {
                _circleDelay = CIRCLE_DELAY;
                _circleVisible = !_circleVisible;
            }

            _thunderDelay--;
            if (_thunderDelay == 0)
            {
                _thunderVisible = true;
                _thunderDelay = THUNDER_DELAY;
                _thunderStage++;
                _totalThunderStages++;
                if (_thunderStage == THUNDER_STAGES)
                {
                    _thunderStage = 0;
                }

                if (_totalThunderStages == TOTAL_THUNDER_STAGES)
                {
                    Finish();
                }
            }
        }
    }
}
