using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Pokedex
{
    class PokedexResultScreen : PokedexListScreen
    {
        private Texture2D _overlay, _selector;
        private PokemonType _type1, _type2;

        protected override int VisibleEntries => 4;
        protected override bool DrawPokedexCount => false;

        public PokedexResultScreen(Screen preScreen, PokemonType type1, PokemonType type2, PokedexListMode listMode)
            : base(preScreen, listMode)
        {
            _type1 = type1;
            _type2 = type2;
        }

        internal override void LoadContent()
        {
            base.LoadContent();

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/resultsOverlay.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/selectorWide.png");

            _entries = PokedexEntry.GetTypeFiltered(_type1, _type2, ListMode);
            PresetSelection();
        }

        internal override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // batch begins in base

            var unit = (int)(Border.SCALE * Border.UNIT);
            var width = Border.SCREEN_WIDTH * unit;
            var height = Border.SCREEN_HEIGHT * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            // list
            var visibleEntries = _entries.Skip(_scrollIndex).Take(VisibleEntries).ToArray();
            var listText = "";
            for (int i = 0; i < VisibleEntries; i++)
            {
                if (i < visibleEntries.Length)
                {
                    var entry = visibleEntries[i];
                    if (entry.IsKnown)
                    {
                        listText += " " + entry.Name;
                    }
                    else
                    {
                        listText += " -----";
                    }
                    listText += NewLine;

                    // draw pokeball
                    if (entry.IsCaught)
                    {
                        _batch.Draw(_pokeball, new Rectangle(
                            (int)(startX + unit * 8 + 3 * Border.SCALE),
                            unit * 2 + unit * i * 2,
                            (int)(_pokeball.Width * Border.SCALE),
                            (int)(_pokeball.Height * Border.SCALE)), Color.White);
                    }
                }
            }
            _fontRenderer.LineGap = 1;
            _fontRenderer.DrawText(_batch, listText,
                new Vector2(startX + unit * 8 + 3 * Border.SCALE, unit * 2), Border.DefaultWhite, Border.SCALE);

            // draw selector
            _batch.Draw(_selector, new Rectangle(
                (int)(startX + unit * 8 - Border.SCALE),
                unit + unit * _index * 2,
                (int)(_selector.Width * Border.SCALE),
                (int)(_selector.Height * Border.SCALE)), Color.White);

            // draw result stats
            string GetTypeStr(PokemonType type)
            {
                if (type == PokemonType.None)
                {
                    return "";
                }
                var typeStr = type.ToString().ToUpper();
                while (typeStr.Length < 8)
                {
                    if (typeStr.Length % 2 == 1)
                    {
                        typeStr += " ";
                    }
                    else
                    {
                        typeStr = " " + typeStr;
                    }
                }

                return typeStr;
            }
            var typeStr1 = GetTypeStr(_type1);
            var typeStr2 = GetTypeStr(_type2);
            if (typeStr2 != "")
            {
                typeStr2 = "/" + typeStr2;
            }

            _fontRenderer.LineGap = 0;
            _fontRenderer.DrawText(_batch,
                "SEARCH RESULTS" + NewLine + NewLine +
                "  TYPE  " + typeStr1 + NewLine +
                "         " + typeStr2 + NewLine +
                _entries.Length.ToString().PadLeft(3) + " FOUND!",
                new Vector2(startX + Border.SCALE * 3, unit * 12), Border.DefaultWhite, Border.SCALE);

            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            _batch.End();
        }
    }
}
