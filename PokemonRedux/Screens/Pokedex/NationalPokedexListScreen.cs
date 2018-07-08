using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Pokedex
{
    class NationalPokedexListScreen : PokedexListScreen
    {
        private Texture2D _overlay, _selector;

        protected override int VisibleEntries => 7;

        public NationalPokedexListScreen(Screen preScreen)
            : base(preScreen, PokedexListMode.National)
        { }

        internal override void LoadContent()
        {
            base.LoadContent();

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/nationalOverlay.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/selectorWide.png");

            _entries = PokedexEntry.GetNational();
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
                    listText += entry.Id.ToString("D3") + NewLine;
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
            _fontRenderer.LineGap = 0;
            _fontRenderer.DrawText(_batch, listText,
                new Vector2(startX + unit * 8 + 3 * Border.SCALE, unit), Border.DefaultWhite, Border.SCALE);

            // draw selector
            _batch.Draw(_selector, new Rectangle(
                (int)(startX + unit * 8 - Border.SCALE),
                (int)(unit + unit * _index * 2 - 3 * Border.SCALE),
                (int)(_selector.Width * Border.SCALE),
                (int)(_selector.Height * Border.SCALE)), Color.White);

            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            _batch.End();
        }
    }
}
