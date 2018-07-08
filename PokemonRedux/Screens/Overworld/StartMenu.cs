using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Overworld
{
    class StartMenu
    {
        private static readonly IReadOnlyDictionary<string, string> DESCRIPTION_TEXTS = new Dictionary<string, string>()
        {
            { "POKéDEX", "POKéMON" + Environment.NewLine + "database" },
            { "POKéMON", "Party ^PK^MN" + Environment.NewLine + "status" },
            { "PACK", "Contains" + Environment.NewLine + "items" },
            { "^PO^KéGEAR", "Trainer^'s" + Environment.NewLine + "key device" },
            { "PLAYER", "Your own" + Environment.NewLine + "status" },
            { "SAVE", "Save your" + Environment.NewLine + "progress" },
            { "OPTION", "Change" + Environment.NewLine + "settings" },
            { "EXIT", "Close this" + Environment.NewLine + "menu" }
        };

        private const int WIDTH = 10;
        private const int UNIT = 8;
        private const float SCALE = 4f;

        private WorldScreen _parentScreen;

        private string[] _options;
        private int _index = 0;
        private int _lastSelectedIndex = 0; // the index the menu opens with, stores last selected entry
        private PokemonFontRenderer _fontRenderer;

        public bool Visible { get; private set; } = false;

        public StartMenu(WorldScreen parentScreen)
        {
            _parentScreen = parentScreen;
        }

        public void LoadContent()
        {
            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();
        }

        public void Show()
        {
            _index = _lastSelectedIndex;
            Visible = true;

            var options = new List<string>();
            if (Controller.ActivePlayer.HasPokedex)
            {
                options.Add("POKéDEX");
            }
            if (Controller.ActivePlayer.PartyPokemon.Length > 0)
            {
                options.Add("POKéMON");
            }
            options.Add("PACK");
            if (Controller.ActivePlayer.HasPokegear)
            {
                options.Add("^PO^KéGEAR");
            }
            options.AddRange(new[] {
                "PLAYER",
                "SAVE",
                "OPTION",
                "EXIT"
            });

            _options = options.ToArray();
        }

        private void Close()
        {
            Visible = false;
        }

        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                var unitsHeight = 2 + _options.Length * 2;

                var width = (int)(WIDTH * UNIT * SCALE);
                var height = (int)(unitsHeight * UNIT * SCALE);
                var unit = (int)(UNIT * SCALE);
                var startX = Controller.ClientRectangle.Width - width;
                var startY = 0;

                Border.Draw(batch, startX, startY, WIDTH, unitsHeight, SCALE);

                var screenManager = GetComponent<ScreenManager>();
                var selectorChar = screenManager.ActiveScreen == _parentScreen ? ">" : "^>>";

                var text = string.Join(Environment.NewLine,
                    _options.Select((t, i) => (i == _index ? selectorChar : " ") + t.Replace("PLAYER", Controller.ActivePlayer.Name)));

                _fontRenderer.DrawText(batch, text, new Vector2(startX + unit, startY + unit * 2), Color.Black, SCALE);

                // draw explanations
                if (Controller.ActivePlayer.MenuExplanations)
                {
                    Border.DrawCenter(batch, startX, startY + height, WIDTH, 5, SCALE);

                    var descText = DESCRIPTION_TEXTS[_options[_index]];

                    _fontRenderer.DrawText(batch, descText, new Vector2(startX, startY + height + unit), Color.Black, SCALE);
                }
            }
        }

        public void Update()
        {
            if (Visible)
            {
                if (GameboyInputs.UpPressed())
                {
                    _index--;
                    if (_index < 0)
                    {
                        _index = _options.Length - 1;
                    }
                }
                else if (GameboyInputs.DownPressed())
                {
                    _index++;
                    if (_index == _options.Length)
                    {
                        _index = 0;
                    }
                }
                else if (GameboyInputs.BPressed() || GameboyInputs.StartPressed())
                {
                    Close();
                }
                else if (GameboyInputs.APressed())
                {
                    _lastSelectedIndex = _index;
                    SelectOption(_options[_index]);
                }
            }
        }

        private void SelectOption(string option)
        {
            switch (option)
            {
                case "POKéDEX":
                    // open pokedex screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var pokedexScreen = new Pokedex.RegionalPokedexListScreen(_parentScreen, Pokedex.PokedexListMode.Regional);
                        pokedexScreen.LoadContent();
                        screenManager.SetScreen(pokedexScreen);
                    }
                    break;

                case "POKéMON":
                    // open party screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var partyScreen = new Pokemons.PartyScreen(_parentScreen);
                        partyScreen.LoadContent();
                        screenManager.SetScreen(partyScreen);
                    }
                    break;

                case "PACK":
                    // open inventory screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var packScreen = new Pack.PackScreen(_parentScreen, Pack.PackMode.Field);
                        packScreen.LoadContent();
                        screenManager.SetScreen(packScreen);
                    }
                    break;

                case "^PO^KéGEAR":
                    // open pokegear screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var pokegearScreen = new Pokegear.PokegearScreen(_parentScreen);
                        pokegearScreen.LoadContent();
                        screenManager.SetScreen(pokegearScreen);
                    }
                    break;

                case "PLAYER":
                    // open trainer card screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var trainerCardScreen = new TrainerCard.TrainerCardScreen(_parentScreen);
                        trainerCardScreen.LoadContent();
                        screenManager.SetScreen(trainerCardScreen);
                    }
                    break;

                case "SAVE":
                    // open save screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var saveScreen = new Save.SaveScreen(_parentScreen, _parentScreen.World.PlayerEntity);
                        saveScreen.LoadContent();
                        screenManager.SetScreen(saveScreen);
                    }
                    break;

                case "OPTION":
                    // open options screen
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var optionsScreen = new Options.OptionsScreen(_parentScreen);
                        optionsScreen.LoadContent();
                        screenManager.SetScreen(optionsScreen);
                    }
                    break;

                case "EXIT":
                    // TODO: remove
                    {
                        var screenManager = GetComponent<ScreenManager>();
                        var computerScreen = new Computer.ComputerSelectionScreen(_parentScreen);
                        computerScreen.LoadContent();
                        screenManager.SetScreen(computerScreen);
                    }
                    Close();
                    break;
            }
        }
    }
}
