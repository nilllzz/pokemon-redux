using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Pokedex
{
    class RegionalPokedexListScreen : PokedexListScreen
    {
        private const int SCROLLBAR_HEIGHT = 126;

        private Texture2D _overlay, _selector, _scroll;

        protected override int VisibleEntries => 7;

        public RegionalPokedexListScreen(Screen preScreen, PokedexListMode listMode)
            : base(preScreen, listMode)
        { }

        internal override void LoadContent()
        {
            base.LoadContent();

            _overlay = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/regionalOverlay.png");
            _selector = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/selectorShort.png");
            _scroll = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/scroll.png");

            // load list
            SetListMode(ListMode);
        }

        public void SetListMode(PokedexListMode listMode)
        {
            ListMode = listMode;
            if (ListMode == PokedexListMode.Regional)
            {
                _entries = PokedexEntry.GetRegional();
            }
            else
            {
                _entries = PokedexEntry.GetRegionalAtoZ();
            }

            PresetSelection();
        }

        internal override void UnloadContent()
        {
            base.UnloadContent();
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
            for (var i = 0; i < VisibleEntries; i++)
            {
                if (i < visibleEntries.Length)
                {
                    var entry = visibleEntries[i];
                    if (entry.IsKnown)
                    {
                        listText += entry.Name;
                    }
                    else
                    {
                        listText += "-----";
                    }
                    listText += NewLine;

                    // draw pokeball
                    if (entry.IsCaught)
                    {
                        _batch.Draw(_pokeball, new Rectangle(
                            startX + unit * 8,
                            unit * 2 + unit * i * 2,
                            (int)(_pokeball.Width * Border.SCALE),
                            (int)(_pokeball.Height * Border.SCALE)), Color.White);
                    }

                    // draw selector
                    if (i == _index)
                    {
                        _batch.Draw(_selector, new Rectangle(
                            (int)(startX + unit * 8 - Border.SCALE),
                            unit + unit * i * 2,
                            (int)(_selector.Width * Border.SCALE),
                            (int)(_selector.Height * Border.SCALE)), Color.White);
                    }
                }
            }
            _fontRenderer.LineGap = 1;
            _fontRenderer.DrawText(_batch, listText,
                new Vector2(startX + unit * 9, unit * 2), Border.DefaultWhite, Border.SCALE);

            // overlay
            _batch.Draw(_overlay, new Rectangle(startX, 0, width, height), Color.White);

            // scroll bar
            var scrollPos = new Point((int)(startX + width - 6 * Border.SCALE),
                (int)(((double)(_index + _scrollIndex) / (_entries.Length - 1) * (SCROLLBAR_HEIGHT - 5) + 5) * Border.SCALE));

            _batch.Draw(_scroll, new Rectangle(scrollPos, new Point((int)(_scroll.Width * Border.SCALE))), Color.White);

            _batch.End();
        }
    }
}
