using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    class PokeballOpeningAnimation : BattleAnimation
    {
        private const int FRAME_SIZE = 40;
        private const int ANIMATION_DELAY = 4;
        private const int STAGES = 4;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _stage;
        private int _animationDelay = ANIMATION_DELAY;

        public PokeballOpeningAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/pokeballOpening.png");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_stage < STAGES)
            {
                var center = GetCenter(_target.Side);
                var frameSize = (int)(FRAME_SIZE * Border.SCALE);
                var x = (int)(center.X - frameSize / 2f);
                var y = (int)(center.Y - frameSize / 2f + 10 * Border.SCALE); // offset to +10

                batch.Draw(_texture, new Rectangle(x, y, frameSize, frameSize),
                    new Rectangle(_stage * FRAME_SIZE, 0, FRAME_SIZE, FRAME_SIZE),
                    Color.White);
            }
        }

        public override void Update()
        {
            _animationDelay--;
            if (_animationDelay == 0)
            {
                _animationDelay = ANIMATION_DELAY;
                _stage++;
                if (_stage == STAGES)
                {
                    IsFinished = true;
                }
            }
        }
    }
}
