using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Screens.Battles
{
    class PokemonStats
    {
        private PokemonFontRenderer _fontRenderer;

        private Pokemon _pokemon;

        public bool Visible { get; set; } = false;

        public void LoadContent()
        {
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
            _fontRenderer.LineGap = 0;
        }

        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                var (unit, startX, width, height) = Border.GetDefaultScreenValues();
                var startY = BattleScreen.StartY;

                Border.Draw(batch, startX + unit * 9, startY, 11, 12, Border.SCALE);

                var text = "ATTACK\n" +
                    _pokemon.Attack.ToString().PadLeft(7) + "\n" +
                    "DEFENSE\n" +
                    _pokemon.Defense.ToString().PadLeft(7) + "\n" +
                    "SPCL.ATK\n" +
                    _pokemon.SpecialAttack.ToString().PadLeft(7) + "\n" +
                    "SPCL.DEF\n" +
                    _pokemon.SpecialDefense.ToString().PadLeft(7) + "\n" +
                    "SPEED\n" +
                    _pokemon.Speed.ToString().PadLeft(7);

                _fontRenderer.DrawText(batch, text,
                    new Vector2(startX + unit * 11, startY + unit), Color.Black, Border.SCALE);
            }
        }

        public void Update()
        {
            if (Visible)
            {
                // close
                if (GameboyInputs.APressed() || GameboyInputs.BPressed())
                {
                    Visible = false;
                }
            }
        }

        public void Show(Pokemon pokemon)
        {
            Visible = true;
            _pokemon = pokemon;
        }
    }
}
