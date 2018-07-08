using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core;
using static System.Environment;

namespace PokemonRedux.Screens.Pokedex
{
    abstract class PokedexListScreen : Screen
    {
        protected static readonly Color POKEMON_PORTRAIT_BACKGROUND = new Color(88, 184, 0);
        protected static readonly Color POKEMON_UNKNOWN_BACKGROUND = new Color(40, 96, 0);
        private static readonly Color[] POKEMON_PORTRAIT_PALETTE = new[]
        {
            new Color(40, 96, 0),
            new Color(48, 128, 24),
            new Color(56, 136, 0),
            new Color(88, 184, 0)
        };

        protected Screen _preScreen;

        protected SpriteBatch _batch;
        protected PokemonFontRenderer _fontRenderer;
        protected Texture2D _pokeball;
        private Texture2D _unknown;

        private List<PokedexEntry> _entryStack = new List<PokedexEntry>();
        private List<double> _spriteAnimationStack = new List<double>();

        protected PokedexEntry[] _entries;
        protected int _index = 0;
        protected int _scrollIndex = 0;

        protected abstract int VisibleEntries { get; }
        protected virtual bool DrawPokedexCount => true;
        public PokedexListMode ListMode { get; protected set; }

        public PokedexListScreen(Screen preScreen, PokedexListMode listMode)
        {
            _preScreen = preScreen;
            ListMode = listMode;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _pokeball = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/pokeball.png");
            _unknown = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokedex/unknownPokemon.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var unit = (int)(Border.SCALE * Border.UNIT);
            var width = Border.SCREEN_WIDTH * unit;
            var height = Border.SCREEN_HEIGHT * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            // draw black background
            _batch.DrawRectangle(new Rectangle(startX, 0, width, height), Color.Black);

            var visibleEntries = _entries.Skip(_scrollIndex).Take(VisibleEntries).ToArray();
            var selectedEntry = visibleEntries[_index];

            // draw pokemon portrait
            DrawPokemonPortrait(selectedEntry);

            // seen/own count
            if (DrawPokedexCount)
            {
                _fontRenderer.LineGap = 0;
                _fontRenderer.DrawText(_batch,
                    "SEEN" + NewLine +
                    SeenCount.ToString().PadLeft(7) + NewLine + NewLine +
                    "OWN" + NewLine +
                    CaughtCount.ToString().PadLeft(7),
                    new Vector2(startX + 3 * Border.SCALE, unit * 11), Border.DefaultWhite, Border.SCALE);
            }

            // do not end spritebatch here, implementing screen does that
        }

        private void DrawPokemonPortrait(PokedexEntry entry)
        {
            var unit = (int)(Border.SCALE * Border.UNIT);
            var width = Border.SCREEN_WIDTH * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            if (_entryStack.Count == 0)
            {
                _entryStack.Insert(0, entry);
                _spriteAnimationStack.Insert(0, 1);
            }
            else if (_entryStack[0] != entry)
            {
                _entryStack.Insert(0, entry);
                _spriteAnimationStack.Insert(0, 0);
            }

            for (int i = _entryStack.Count - 1; i >= 0; i--)
            {
                var stackEntry = _entryStack[i];
                var animation = _spriteAnimationStack[i];

                Color backgroundColor;
                Texture2D sprite;
                if (stackEntry.IsKnown)
                {
                    backgroundColor = POKEMON_PORTRAIT_BACKGROUND;
                    sprite = GetPokemonPortrait(stackEntry.Id);
                }
                else
                {
                    backgroundColor = POKEMON_UNKNOWN_BACKGROUND;
                    sprite = _unknown;
                }

                var spriteSlice = (int)(animation * sprite.Width);
                var targetRect = new Rectangle(
                    (int)(startX + 3 * Border.SCALE), unit,
                    (int)(spriteSlice * Border.SCALE), unit * 7);

                _batch.DrawRectangle(targetRect, backgroundColor);
                _batch.Draw(sprite, targetRect,
                    new Rectangle(0, 0, spriteSlice, sprite.Height),
                    Color.White);
            }
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.DownPressed())
            {
                if (_index + _scrollIndex + 1 < _entries.Length)
                {
                    if (_index < VisibleEntries - 1)
                    {
                        _index++;
                    }
                    else
                    {
                        _scrollIndex++;
                    }
                }
            }
            else if (GameboyInputs.UpPressed())
            {
                if (_index > 0)
                {
                    _index--;
                }
                else if (_scrollIndex > 0)
                {
                    _scrollIndex--;
                }
            }
            else if (GameboyInputs.LeftPressed())
            {
                _scrollIndex -= 7;
                if (_scrollIndex < 0)
                {
                    _scrollIndex = 0;
                }
            }
            else if (GameboyInputs.RightPressed())
            {
                _scrollIndex += 7;
                if (_scrollIndex + VisibleEntries > _entries.Length)
                {
                    _scrollIndex = _entries.Length - VisibleEntries;
                }
            }

            var stackEntryIsComplete = false;
            for (int i = 0; i < _entryStack.Count; i++)
            {
                _spriteAnimationStack[i] += 0.2;
                if (_spriteAnimationStack[i] >= 1.0)
                {
                    if (stackEntryIsComplete)
                    {
                        _spriteAnimationStack.RemoveAt(i);
                        _entryStack.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        stackEntryIsComplete = true;
                        _spriteAnimationStack[i] = 1.0;
                    }
                }
            }

            if (GameboyInputs.APressed())
            {
                var selectedEntry = _entries[_scrollIndex + _index];
                if (selectedEntry.IsKnown)
                {
                    // open details screen
                    var screenManager = GetComponent<ScreenManager>();
                    var detailsScreen = new PokedexDetailsScreen(this, selectedEntry, GetNextEntry, GetPreviousEntry);
                    detailsScreen.LoadContent();
                    screenManager.SetScreen(detailsScreen);
                }
            }
            else if (GameboyInputs.BPressed())
            {
                Close();
            }
            else if (GameboyInputs.SelectPressed())
            {
                // open options screen
                var screenManager = GetComponent<ScreenManager>();
                var optionsScreen = new PokedexOptionScreen(_preScreen, this);
                optionsScreen.LoadContent();
                screenManager.SetScreen(optionsScreen);
            }
            else if (GameboyInputs.StartPressed())
            {
                // open search screen
                var screenManager = GetComponent<ScreenManager>();
                var searchScreen = new PokedexSearchScreen(_preScreen, this, ListMode);
                searchScreen.LoadContent();
                screenManager.SetScreen(searchScreen);
            }
        }

        protected static Texture2D GetPokemonPortrait(int id)
        {
            return PokemonTextureManager.GetFront(id, POKEMON_PORTRAIT_PALETTE);
        }

        protected static int SeenCount =>
            Controller.ActivePlayer.PokedexSeen.Length + Controller.ActivePlayer.PokedexCaught.Length;
        protected static int CaughtCount =>
            Controller.ActivePlayer.PokedexCaught.Length;

        protected void ScrollTo(int index)
        {
            if (index < VisibleEntries)
            {
                // entry to select is in the first visible entries
                _scrollIndex = 0;
                _index = index;
            }
            else
            {
                // selected entry is at the top of the list
                _index = 0;
                _scrollIndex = index;
                // if the entry is at the end of the list, do not scroll past
                if (_scrollIndex + VisibleEntries > _entries.Length)
                {
                    _scrollIndex = _entries.Length - VisibleEntries;
                    _index = index - _scrollIndex;
                }
            }
        }

        protected void PresetSelection()
        {
            var lastSelectedId = Controller.ActivePlayer.MenuStates.PokedexLastSelectedId;
            var selectIndex = Array.IndexOf(_entries, _entries.FirstOrDefault(e => e.Id == lastSelectedId));
            if (lastSelectedId > 0 && selectIndex > -1)
            {
                ScrollTo(selectIndex);
            }
            else
            {
                ScrollTo(0);
            }
        }

        // methods called from the details screen to go the next/previous entry
        // returns null when past list boundary
        public PokedexEntry GetNextEntry()
        {
            var i = _index + _scrollIndex + 1;
            var scrolled = 1;
            PokedexEntry entry = null;
            while (i < _entries.Length && entry == null)
            {
                if (_entries[i].IsKnown)
                {
                    entry = _entries[i];
                    _index += scrolled;
                    while (_index >= VisibleEntries)
                    {
                        _scrollIndex++;
                        _index--;
                    }
                    // do not want the image to animation when returning:
                    _entryStack.Clear();
                    _spriteAnimationStack.Clear();
                }
                else
                {
                    i++;
                    scrolled++;
                }
            }
            return entry;
        }

        public PokedexEntry GetPreviousEntry()
        {
            var i = _index + _scrollIndex - 1;
            var scrolled = 1;
            PokedexEntry entry = null;
            while (i >= 0 && entry == null)
            {
                if (_entries[i].IsKnown)
                {
                    entry = _entries[i];
                    _index -= scrolled;
                    while (_index < 0)
                    {
                        _scrollIndex--;
                        _index++;
                    }
                    // do not want the image to animation when returning:
                    _entryStack.Clear();
                    _spriteAnimationStack.Clear();
                }
                else
                {
                    i--;
                    scrolled++;
                }
            }
            return entry;
        }

        protected void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }
    }
}
