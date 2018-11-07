using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class BellyDrumAnimation : BattleMoveAnimation
    {
        private const int DISPLAY_DELAY = 8;
        private static readonly int[] WAIT_DELAYS = new[] { 10, 16, 16, 6, 6, 16, 6, 6, 6 };

        private int _displayDelay = DISPLAY_DELAY;
        private int _waitDelay = WAIT_DELAYS[0];
        private int _waitIndex = 0;

        public BellyDrumAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("bellydrum");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_waitDelay == 0)
            {
                var center = GetCenter(_user.Side);
                var pokemonSize = GetPokemonSpriteSize();
                var frameSize = _texture.Height * Border.SCALE;
                var handPosition = center;
                if (_user.Side == PokemonSide.Player)
                {
                    handPosition += new Vector2(pokemonSize / 2f - frameSize, pokemonSize / 2f - frameSize);
                }
                else
                {
                    handPosition += new Vector2(-pokemonSize / 2f, pokemonSize / 2f - frameSize);
                }
                var notePosition = handPosition - new Vector2(0, frameSize);
                var noteProgress = (DISPLAY_DELAY - _displayDelay) * Border.SCALE;
                if (_user.Side == PokemonSide.Player)
                {
                    notePosition += new Vector2(noteProgress, -noteProgress);
                }
                else
                {
                    notePosition += new Vector2(-noteProgress);
                }

                var effect = _user.Side == PokemonSide.Player ?
                    SpriteEffects.None : SpriteEffects.FlipHorizontally;

                batch.Draw(_texture, new Rectangle((int)handPosition.X, (int)handPosition.Y, (int)frameSize, (int)frameSize),
                    new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, effect, 0f);
                batch.Draw(_texture, new Rectangle((int)notePosition.X, (int)notePosition.Y, (int)frameSize, (int)frameSize),
                    new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, effect, 0f);
            }
        }

        public override void Update()
        {
            if (_waitDelay > 0)
            {
                _waitDelay--;
            }
            else
            {
                _displayDelay--;
                if (_displayDelay == 0)
                {
                    _displayDelay = DISPLAY_DELAY;
                    _waitIndex++;
                    if (_waitIndex == WAIT_DELAYS.Length)
                    {
                        Finish();
                    }
                    else
                    {
                        _waitDelay = WAIT_DELAYS[_waitIndex];
                    }
                }
            }
        }
    }
}
