using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class HornAttackAnimation : BattleMoveAnimation
    {
        private const float PROGRESS_PER_FRAME = 0.1f;
        private const int HIT_DELAY = 10;
        private const int END_DELAY = 8;
        private const int HORN_WIDTH = 22;
        private const int HORN_HEIGHT = 16;

        private float _progress = 0f;
        private int _hitDelay = HIT_DELAY;
        private int _endDelay = END_DELAY;

        public HornAttackAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("hornattack");
        }

        public override void Draw(SpriteBatch batch)
        {
            var userCenter = GetCenter(_user.Side);
            var targetCenter = GetCenter(_target.Side);
            var pokemonSize = GetPokemonSpriteSize();

            var startPos = userCenter;
            var endPos = (userCenter + targetCenter) / 2f;
            if (_user.Side == PokemonSide.Player)
            {
                startPos += new Vector2(pokemonSize / 2f, -Border.SCALE * Border.UNIT);
            }
            else
            {
                startPos += new Vector2(-pokemonSize / 2f, Border.SCALE * Border.UNIT * 0.5f);
            }

            var effect = _user.Side == PokemonSide.Player ?
                SpriteEffects.None : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

            var pos = startPos + (endPos - startPos) * _progress;
            var hornWidth = HORN_WIDTH * Border.SCALE;
            var hornHeight = HORN_HEIGHT * Border.SCALE;
            var hornX = (int)(pos.X - hornWidth / 2f);
            var hornY = (int)(pos.Y - hornHeight / 2f);

            batch.Draw(_texture, new Rectangle(hornX, hornY, (int)hornWidth, (int)hornHeight),
                new Rectangle(0, 0, HORN_WIDTH, HORN_HEIGHT), Color.White);

            if (_progress == 1f && _hitDelay > 0)
            {
                var frameSize = _texture.Width * Border.SCALE;
                var posX = (int)(targetCenter.X - frameSize / 2f);
                var posY = (int)(targetCenter.Y - frameSize / 2f);
                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                    new Rectangle(0, HORN_HEIGHT, _texture.Width, _texture.Width), Color.White);
            }
        }

        public override void Update()
        {
            if (_progress < 1f)
            {
                _progress += PROGRESS_PER_FRAME;
                if (_progress >= 1f)
                {
                    _progress = 1f;
                }
            }
            else if (_hitDelay > 0)
            {
                _hitDelay--;
            }
            else if (_endDelay > 0)
            {
                _endDelay--;
                if (_endDelay == 0)
                {
                    Finish();
                }
            }
        }
    }
}
