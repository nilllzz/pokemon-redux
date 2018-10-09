using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class AmnesiaAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 3;
        private static readonly int[] STAGE_HEIGHTS = new[] { 8, 16, 32 };
        private static readonly int[] STAGE_DELAYS = new[] { 8, 16, 16, 64 };
        private const int FRAME_WIDTH = 18;

        private int _stage = -1;
        private int _stageDelay = STAGE_DELAYS[0];

        public AmnesiaAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("amnesia");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_stage >= 0)
            {
                var center = GetCenter(_user.Side);
                var pokemonSize = GetPokemonSpriteSize();

                var textureX = _user.Side == PokemonSide.Player ? 0 : 18;
                var textureHeight = STAGE_HEIGHTS[_stage];
                var textureY = _texture.Height - textureHeight;
                var textureRect = new Rectangle(textureX, textureY, FRAME_WIDTH, textureHeight);

                var frameWidth = FRAME_WIDTH * Border.SCALE;
                var frameHeight = textureHeight * Border.SCALE;

                var posX = center.X;
                if (_user.Side == PokemonSide.Player)
                {
                    posX += pokemonSize * 0.25f;
                }
                else
                {
                    posX -= pokemonSize * 0.25f + frameWidth;
                }
                var posY = (int)(center.Y - frameHeight);

                batch.Draw(_texture, new Rectangle((int)posX, posY, (int)frameWidth, (int)frameHeight), textureRect, Color.White);
            }
        }

        public override void Update()
        {
            _stageDelay--;
            if (_stageDelay == 0)
            {
                _stage++;
                if (_stage == TOTAL_STAGES)
                {
                    Finish();
                }
                else
                {
                    _stageDelay = STAGE_DELAYS[_stage + 1];
                }
            }
        }
    }
}
