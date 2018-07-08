using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game;
using PokemonRedux.Game.Pokemons;
using static Core;

namespace PokemonRedux.Screens.Battles
{
    class PlayerStatus
    {
        private const int INDICATOR_HEALTHY = 0;
        private const int INDICATOR_STATUS = 1;
        private const int INDICATOR_FAINTED = 2;
        private const int INDICATOR_EMPTY = 3;

        private Texture2D _texture, _partyIndicators;
        private PokemonFontRenderer _fontRenderer;

        public bool Visible { get; set; }

        public void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Battle/playerState.png");
            _partyIndicators = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Battle/partyIndicators.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        public void UnloadContent()
        {

        }

        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                var (unit, startX, width, height) = Border.GetDefaultScreenValues();

                // main bar texture
                batch.Draw(_texture, new Rectangle(startX + unit * 9, BattleScreen.StartY + unit * 10,
                    (int)(_texture.Width * Border.SCALE),
                    (int)(_texture.Height * Border.SCALE)), Color.White);

                // party indicators
                for (int i = 0; i < Player.MAX_POKEMON; i++)
                {
                    int textureX;
                    if (Controller.ActivePlayer.PartyPokemon.Length <= i)
                    {
                        textureX = INDICATOR_EMPTY;
                    }
                    else
                    {
                        var pokemon = Controller.ActivePlayer.PartyPokemon[i];
                        if (pokemon.Status == PokemonStatus.FNT)
                        {
                            textureX = INDICATOR_FAINTED;
                        }
                        else if (pokemon.Status != PokemonStatus.OK)
                        {
                            textureX = INDICATOR_STATUS;
                        }
                        else
                        {
                            textureX = INDICATOR_HEALTHY;
                        }
                    }

                    batch.Draw(_partyIndicators, new Rectangle(
                        startX + unit * 11 + i * unit,
                        BattleScreen.StartY + unit * 10,
                        (int)(_partyIndicators.Height * Border.SCALE),
                        (int)(_partyIndicators.Height * Border.SCALE)),
                        new Rectangle(textureX * 8, 0, 8, 8), Color.White);
                }
            }
        }
    }
}
