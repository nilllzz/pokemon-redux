using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Overworld;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Naming;
using PokemonRedux.Screens.Overworld;
using PokemonRedux.Screens.Transition;
using System;
using static Core;

namespace PokemonRedux.Screens.NewGame
{
    class NewGameScreen : Screen
    {
        private const int INITIAL_TEXT_DELAY = 60;
        private const float BRIGHTNESS_INCREASE = 0.025f;
        private const float POKEMON_FADE_SPEED = 0.025f;
        private const int OAK_APPEAR_STAGES = 6;
        private const int OAK_TOTAL_STAGES = 8;
        private const int OAK_DELAY = 10;
        private const int PLAYER_APPEAR_STAGE = 5;
        private const int PLAYER_REAPPEAR_STAGE = 8;
        private const int PLAYER_TOTAL_STAGES = 14;
        private const int PLAYER_DELAY = 10;
        private const int POKEMON_APPEAR_DELAY = 30;
        private static readonly int[] POKEMON_PREVIEW_IDS = new[] { 161, 163, 165, 167, 179, 183, 187 };
        private const int PLAYER_TARGET_OFFSET = 48;
        private static readonly string[] PRESET_NAMES = new[] { "NEW NAME", "GOLD", "HIRO", "TAYLOR", "KARL" };

        private SpriteBatch _batch;
        private Textbox _textbox;
        private Texture2D _oak, _player, _namingIcon;
        private Pokemon _previewPokemon;
        private Random _random;
        private OptionsBox _nameSelection;
        private PokemonFontRenderer _fontRenderer;

        private int _initialTextDelay = INITIAL_TEXT_DELAY;
        private float _brightness = 0f;
        private bool _goBright = false;
        private bool _oakAppear = false;
        private bool _pokemonAppear = false;
        private int _oakStage = -1;
        private int _oakStageDelay = OAK_DELAY;
        private bool _pokemonVisible = false;
        private bool _oakTextContinue = false;
        private bool _playerAnimate = false;
        private int _playerStage = -1;
        private int _playerStageDelay = PLAYER_DELAY;
        private int _pokemonAppearDelay = POKEMON_APPEAR_DELAY;
        private int _playerOffset = 0;
        private bool _movePlayer = false;
        private bool _updateTextbox = true;
        private string _playerChosenName = "";
        private int _playerTargetStage;
        private bool _fadePokemon = false;
        private float _pokemonAlpha = 1f;

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _oak = Controller.Content.LoadDirect<Texture2D>("Textures/UI/NewGame/oak.png");
            _player = Controller.Content.LoadDirect<Texture2D>("Textures/UI/NewGame/player.png");
            _namingIcon = Controller.Content.LoadDirect<Texture2D>("Textures/UI/NewGame/namingIcon.png");

            _random = new Random();
            _previewPokemon = Pokemon.Get(POKEMON_PREVIEW_IDS[_random.Next(0, POKEMON_PREVIEW_IDS.Length)], 1);

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = GameController.RENDER_HEIGHT / 2 - (int)(Border.SCALE * Border.UNIT * Border.SCREEN_HEIGHT) / 2 + (int)(12 * Border.UNIT * Border.SCALE);

            _nameSelection = new OptionsBox();
            _nameSelection.LoadContent();
            _nameSelection.BufferUp = 1;
            _nameSelection.CanCancel = false;
        }

        internal override void UnloadContent()
        {
        }

        internal override void Draw(GameTime gameTime)
        {
            _batch.Begin(samplerState: SamplerState.PointClamp);

            var backgroundColor = (int)(Border.DefaultWhite.R * _brightness);
            _batch.DrawRectangle(Controller.ClientRectangle, new Color(backgroundColor, backgroundColor, backgroundColor));

            _textbox.Draw(_batch, Border.DefaultWhite);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();
            var startY = Controller.ClientRectangle.Height / 2 - Border.SCREEN_HEIGHT * unit / 2;
            if (_oakStage >= 0 && _oakStage < OAK_TOTAL_STAGES)
            {
                var oakWidth = (int)(40 * Border.SCALE);
                var oakHeight = (int)(56 * Border.SCALE);
                _batch.Draw(_oak, new Rectangle(Controller.ClientRectangle.Width / 2 - oakWidth / 2, startY + unit * 4, oakWidth, oakHeight),
                    new Rectangle(_oakStage * 40, 0, 40, 56), Color.White);
            }
            if (_playerStage >= 0 && _playerStage < PLAYER_TOTAL_STAGES)
            {
                var playerWidth = (int)(40 * Border.SCALE);
                var playerHeight = (int)(56 * Border.SCALE);
                _batch.Draw(_player,
                    new Rectangle(Controller.ClientRectangle.Width / 2 - playerWidth / 2 + (int)(_playerOffset * Border.SCALE), startY + unit * 4, playerWidth, playerHeight),
                    new Rectangle(_playerStage * 40, 0, 40, 56), Color.White);
            }
            if (_pokemonVisible && _pokemonAppearDelay == 0)
            {
                var texture = _previewPokemon.GetFrontSprite();
                var pokemonWidth = (int)(texture.Width * Border.SCALE);
                var pokemonHeight = (int)(texture.Height * Border.SCALE);

                var alpha = (int)(255 * _pokemonAlpha);
                _batch.Draw(texture,
                    new Rectangle(Controller.ClientRectangle.Width / 2 - pokemonWidth / 2, startY + unit * 4, pokemonWidth, pokemonHeight),
                    null, new Color(255, 255, 255, alpha), 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }

            _nameSelection.Draw(_batch, Border.DefaultWhite);
            if (_nameSelection.Visible)
            {
                Border.DrawCenter(_batch, _nameSelection.OffsetX + unit * 2, startY, 4, 1, Border.SCALE);
                _fontRenderer.DrawText(_batch, "NAME", new Vector2(_nameSelection.OffsetX + unit * 2, startY), Color.Black, Border.SCALE);
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (_initialTextDelay > 0)
            {
                _initialTextDelay--;
                if (_initialTextDelay == 0)
                {
                    ShowInitialText();
                }
            }

            if (_goBright)
            {
                _brightness += BRIGHTNESS_INCREASE;
                if (_brightness >= 1f)
                {
                    _brightness = 1f;
                    _oakAppear = true;
                }
            }

            if (_oakAppear && _oakStage < OAK_APPEAR_STAGES - 1)
            {
                _oakStageDelay--;
                if (_oakStageDelay == 0)
                {
                    _oakStageDelay = OAK_DELAY;
                    _oakStage++;
                    if (_oakStage == OAK_APPEAR_STAGES - 1)
                    {
                        _oakAppear = false;
                        if (_oakTextContinue)
                        {
                            ShowOakContinueText();
                        }
                        else
                        {
                            ShowWelcomeText();
                        }
                    }
                }
            }

            if (_pokemonAppear)
            {
                _oakStageDelay--;
                if (_oakStageDelay == 0)
                {
                    _oakStageDelay = OAK_DELAY;
                    _oakStage++;
                    if (_oakStage == OAK_TOTAL_STAGES)
                    {
                        _pokemonVisible = true;
                        _pokemonAppear = false;
                    }
                }
            }

            if (_pokemonVisible && _pokemonAppearDelay > 0)
            {
                _pokemonAppearDelay--;
                if (_pokemonAppearDelay == 0)
                {
                    ShowPokemonText();
                }
            }

            if (_playerAnimate)
            {
                if (_oakStage < OAK_TOTAL_STAGES)
                {
                    _oakStageDelay--;
                    if (_oakStageDelay == 0)
                    {
                        _oakStageDelay = OAK_DELAY;
                        _oakStage++;
                    }
                }
                else
                {
                    _playerStageDelay--;
                    if (_playerStageDelay == 0)
                    {
                        _playerStageDelay = PLAYER_DELAY;
                        _playerStage++;
                        if (_playerStage == _playerTargetStage)
                        {
                            _playerAnimate = false;
                            if (_playerStage == PLAYER_TOTAL_STAGES)
                            {
                                Close(gameTime);
                            }
                            else if (_playerChosenName != "")
                            {
                                PlayerNameSelected();
                            }
                            else
                            {
                                ShowPlayerText();
                            }
                        }
                    }
                }
            }

            if (_movePlayer)
            {
                if (_playerChosenName != "")
                {
                    _playerOffset--;
                    if (_playerOffset == 0)
                    {
                        _movePlayer = false;
                        PlayerNameSelected();
                    }
                }
                else
                {
                    _playerOffset++;
                    if (_playerOffset == PLAYER_TARGET_OFFSET)
                    {
                        _movePlayer = false;
                        _nameSelection.Show(PRESET_NAMES);
                        var unit = Border.UNIT * Border.SCALE;
                        var offset = Controller.ClientRectangle.Height / 2 - Border.SCREEN_HEIGHT * unit / 2;
                        _nameSelection.OffsetY = (int)(_nameSelection.Height * unit + offset);
                        _nameSelection.OptionSelected += NameOptionSelected;
                    }
                }
            }

            if (_fadePokemon)
            {
                _pokemonAlpha -= POKEMON_FADE_SPEED;
                if (_pokemonAlpha <= 0f)
                {
                    _pokemonAlpha = 0f;
                    _fadePokemon = false;
                    _oakAppear = true;
                    _oakStage = -1;
                    _pokemonVisible = false;
                    _oakTextContinue = true;
                }
            }

            if (_updateTextbox)
            {
                _textbox.Update();
            }
            _nameSelection.Update();
        }

        private void ShowInitialText()
        {
            var daytime = World.DetermineDaytime();
            var daytimeText = DateHelper.GetDisplayDaytime(daytime).ToUpper() + " " + DateTime.Now.ToString("h:mm") + "!\n";
            switch (daytime)
            {
                case Daytime.Morning:
                case Daytime.Day:
                    daytimeText += "I overslept!";
                    break;
                case Daytime.Night:
                    daytimeText += "No wonder it^'s so\ndark!";
                    break;
            }

            var introText = "^..^..^..^..^..^..^..^..^..^..^..^..\n^..^..^..^..^..^..^..^..^..^..^..^..\n\n" +
                "Zzz^.. Hm? Wha^..?\nYou woke me up!\n\n" + daytimeText;
            _textbox.Show(introText);

            _textbox.Closed += InitialTextFinished;
        }

        private void InitialTextFinished()
        {
            _textbox.Closed -= InitialTextFinished;
            _goBright = true;
        }

        private void ShowWelcomeText()
        {
            _textbox.Show("Hello! Sorry to\nkeep you waiting!\n\n" +
                "Welcome to the\nworld of POKéMON!\n\n" +
                "My name is OAK.\n\n" +
                "People call me the\nPOKéMON PROF.");
            _textbox.Closed += WelcomeTextFinished;
        }

        private void WelcomeTextFinished()
        {
            _textbox.Closed -= WelcomeTextFinished;
            _pokemonAppear = true;
        }

        private void ShowPokemonText()
        {
            _textbox.Show("This world is in-\nhabited by crea-\ntures that we call\nPOKéMON.\n\n" +
                "People and POKéMON\nlive together by\n\n" +
                "supporting each\nother.\n\n" +
                "Some people play\nwith POKéMON, some\nbattle with them.");
            _textbox.Closed += PokemonTextFinished;
        }

        private void PokemonTextFinished()
        {
            _textbox.Closed -= PokemonTextFinished;
            _fadePokemon = true;
        }

        private void ShowOakContinueText()
        {
            _textbox.Show("But we don^'t know\neverything about\nPOKéMON yet.\n\n" +
                "There are still\nmany mysteries to\nsolve.\n\n" +
                "That^'s why I study\nPOKéMON every day.");
            _textbox.Closed += OakContinueTextFinished;
        }

        private void OakContinueTextFinished()
        {
            _textbox.Closed -= OakContinueTextFinished;
            _playerAnimate = true;
            _playerTargetStage = PLAYER_APPEAR_STAGE;
        }

        private void ShowPlayerText()
        {
            _textbox.Show("Now, what did you\nsay your name was?");
            _textbox.Finished += PlayerTextFinished;
        }

        private void PlayerTextFinished()
        {
            _textbox.Finished -= PlayerTextFinished;
            _movePlayer = true;
            _updateTextbox = false;
        }

        private void NameOptionSelected(string option, int index)
        {
            _textbox.Close();
            if (index == 0)
            {
                // input own name
                var nameScreen = new PlayerNamingScreen(this);
                nameScreen.LoadContent();
                nameScreen.SetIcon(_namingIcon);
                nameScreen.NameSelected += PlayerCustomNameEntered;
                GetComponent<ScreenManager>().SetScreen(nameScreen);
            }
            else
            {
                // use preset name
                _playerChosenName = PRESET_NAMES[index];
                _movePlayer = true;
            }
        }

        private void PlayerCustomNameEntered(string name)
        {
            if (name.Length == 0)
            {
                // if an empty name was selected, use the first preset name
                name = PRESET_NAMES[1];
            }
            _playerChosenName = name;

            _playerOffset = 0;
            _playerStage++;
            _playerAnimate = true;
            _playerTargetStage = PLAYER_REAPPEAR_STAGE;
        }

        private void PlayerNameSelected()
        {
            var data = PlayerData.CreateNew(_playerChosenName);
            Controller.ActivePlayer = new Player();
            Controller.ActivePlayer.Load(data);

            _textbox.Show($"{_playerChosenName}, are you\nready?\n\n" +
                "Your very own\nPOKéMON story is\nabout to unfold.\n\n" +
                "You^'ll face fun\ntimes and tough\nchallenges.\n\n" +
                "A world of dreams\nand adventures\n\n" +
                "with POKéMON\nawaits! Let^'s go!\n\n" +
                "I^'ll be seeing you\nlater!");
            _updateTextbox = true;

            _textbox.Closed += PlayerNameSelectedFinished;
        }

        private void PlayerNameSelectedFinished()
        {
            _textbox.Closed -= PlayerNameSelectedFinished;
            _textbox.ShowAndSkip("I^'ll be seeing you\nlater!");

            _playerAnimate = true;
            _playerStage = PLAYER_REAPPEAR_STAGE + 1;
            _playerTargetStage = PLAYER_TOTAL_STAGES;
        }

        private void Close(GameTime gameTime)
        {
            _textbox.Close();

            var worldScreen = new WorldScreen();
            worldScreen.LoadContent();
            // update the world screen once to properly load the player entity
            worldScreen.Update(gameTime);

            var transitionScreen = new FadeTransitionScreen(this, worldScreen, 0.02f);
            transitionScreen.LoadContent();

            GetComponent<ScreenManager>().SetScreen(transitionScreen);
        }
    }
}
