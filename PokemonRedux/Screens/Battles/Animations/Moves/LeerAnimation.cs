using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class LeerAnimation : BattleMoveAnimation
    {
        private const int SWITCH_AMOUNT = 6;
        private const int SWITCH_DELAY = 4;

        private int _index = 0;
        private int _switchDelay = SWITCH_DELAY;

        public LeerAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("leer");
        }

        public override void Draw(SpriteBatch batch)
        {
            var (startX, startY, unit) = GetScreenValues();
            var x = startX + unit * 6;
            var y = (int)(startY + unit * 2.75);
            var effects = SpriteEffects.None;
            if (_target.Side != PokemonSide.Enemy)
            {
                effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
            batch.Draw(_texture, new Rectangle(x, y,
                (int)(_texture.Width * Border.SCALE),
                (int)(_texture.Height * Border.SCALE * 0.5)),
                new Rectangle(0, _index % 2 * _texture.Height / 2, _texture.Width, _texture.Height / 2),
                Color.White, 0f, Vector2.Zero, effects, 0f);
        }

        public override void Update()
        {
            _switchDelay--;
            if (_switchDelay == 0)
            {
                _switchDelay = SWITCH_DELAY;
                _index++;
                if (_index == SWITCH_AMOUNT)
                {
                    Finish();
                }
            }
        }
    }
}
