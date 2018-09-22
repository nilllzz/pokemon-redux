using GameDevCommon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles.Animations;
using PokemonRedux.Screens.Pokemons;
using PokemonRedux.Screens.Transition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Core;

namespace PokemonRedux.Screens.Battles
{
    abstract class BattleScreen : Screen, IBattleUI, IBattleAnimationController, ITextboxScreen
    {
        protected const float INTRO_PROGRESS_SPEED = 0.015f;
        private const float FADE_OUT_SPEED = 0.05f;
        private static readonly Color BACKGROUND_COLOR = new Color(248, 248, 248);
        private static readonly string[] MENU_OPTIONS = new[] { "FIGHT", "^PK^MN", "PACK", "RUN" };
        private static readonly BlendState INVERT_COLORS_BLENDSTATE = new BlendState()
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceColor
        };

        private readonly Screen _preScreen;

        private RenderTarget2D _sceneTarget;
        protected SpriteBatch _batch;
        protected Texture2D _player, _pokeballOpening;
        protected PokemonFontRenderer _fontRenderer;
        protected EnemyPokemonStatus _enemyPokemonStatus;
        protected PlayerPokemonStatus _playerPokemonStatus;
        protected PlayerStatus _playerStatus;
        private Textbox _battleTextbox;
        private PokemonStats _pokemonStats;

        protected BattleMenuState _menuState = BattleMenuState.Off;
        private int _mainMenuIndex, _moveMenuIndex;
        private bool _keepTextboxOpen = false;
        private int _framesBeforeContinuing = 0;

        // animations
        private List<BattleAnimation> _animations = new List<BattleAnimation>();
        protected bool _playerPokemonVisible, _enemyPokemonVisible;
        private Vector2 _screenOffset = Vector2.Zero;
        private bool _invertColors = false;
        protected Vector2 _playerPokemonOffset = Vector2.Zero, _enemyPokemonOffset = Vector2.Zero;
        private bool _fadeOut;
        private float _fadeOutProgress = 0f;
        protected float _playerPokemonSize, _enemyPokemonSize; // sizes for pokeball emerging animation
        protected Color _playerPokemonColor = Color.White, _enemyPokemonColor = Color.White;
        private Effect _screenEffect;
        protected Color[] _playerPokemonPalette, _enemyPokemonPalette; // uses default palette when not set

        public static int StartY
            => GameController.RENDER_HEIGHT / 2 - (int)(Border.SCALE * Border.UNIT * Border.SCREEN_HEIGHT) / 2;

        public BattleScreen(Screen preScreen)
        {
            _preScreen = preScreen;
        }

        internal override void LoadContent()
        {
            _sceneTarget = RenderTargetManager.CreateScreenTarget();

            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _player = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/player.png");
            _pokeballOpening = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/pokeballOpening.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _battleTextbox = new Textbox();
            _battleTextbox.LoadContent();
            _battleTextbox.OffsetY = StartY + (int)(12 * Border.UNIT * Border.SCALE);

            _enemyPokemonStatus = new EnemyPokemonStatus();
            _enemyPokemonStatus.LoadContent();
            _playerPokemonStatus = new PlayerPokemonStatus();
            _playerPokemonStatus.LoadContent();
            _playerStatus = new PlayerStatus();
            _playerStatus.LoadContent();
            _pokemonStats = new PokemonStats();
            _pokemonStats.LoadContent();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            // clear backbuffer
            Controller.GraphicsDevice.Clear(BACKGROUND_COLOR);

            var previousTargets = Controller.GraphicsDevice.GetRenderTargets();
            RenderTargetManager.BeginRenderToTarget(_sceneTarget);
            Controller.GraphicsDevice.Clear(BACKGROUND_COLOR);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // draw empty box at bottom of the screen
            Border.Draw(_batch, startX, StartY + unit * 12, Border.SCREEN_WIDTH, 6, Border.SCALE);

            DrawScreen(gameTime);

            // status
            _enemyPokemonStatus.Draw(_batch);
            _playerPokemonStatus.Draw(_batch);
            _playerStatus.Draw(_batch);
            _pokemonStats.Draw(_batch);

            // battle messages
            _battleTextbox.Draw(_batch, Border.DefaultWhite);

            // main menu
            if (_menuState == BattleMenuState.Main)
            {
                Border.Draw(_batch, startX + unit * 8, StartY + unit * 12, 12, 6, Border.SCALE);

                var menuStr = "";
                for (var i = 0; i < MENU_OPTIONS.Length; i++)
                {
                    if (i == _mainMenuIndex)
                    {
                        menuStr += ">";
                    }
                    else
                    {
                        menuStr += " ";
                    }

                    menuStr += MENU_OPTIONS[i].PadRight(5);

                    if (i == 1)
                    {
                        menuStr += "\n";
                    }
                }
                _fontRenderer.LineGap = 1;
                _fontRenderer.DrawText(_batch, menuStr,
                    new Vector2(startX + unit * 9, StartY + unit * 14), Color.Black, Border.SCALE);
            }
            else if (_menuState == BattleMenuState.Fight)
            {
                Border.Draw(_batch, startX + unit * 4, StartY + unit * 12, 16, 6, Border.SCALE);

                // move list
                var pokemon = Battle.ActiveBattle.PlayerPokemon.Pokemon;
                var moveListStr = "";
                for (var i = 0; i < Pokemon.MAX_MOVES; i++)
                {
                    if (i == _moveMenuIndex)
                    {
                        moveListStr += ">";
                    }
                    else
                    {
                        moveListStr += " ";
                    }
                    if (pokemon.Moves.Length > i)
                    {
                        moveListStr += pokemon.Moves[i].name;
                    }
                    else
                    {
                        moveListStr += "-";
                    }
                    moveListStr += "\n";
                }
                _fontRenderer.LineGap = 0;
                _fontRenderer.DrawText(_batch, moveListStr,
                    new Vector2(startX + unit * 5, StartY + unit * 13), Color.Black, Border.SCALE);

                // move info
                Border.Draw(_batch, startX, StartY + unit * 8, 11, 5, Border.SCALE);

                var selectedMove = pokemon.Moves[_moveMenuIndex];
                // disabled?
                if (Battle.ActiveBattle.PlayerPokemon.DisabledMove == selectedMove &&
                    Battle.ActiveBattle.PlayerPokemon.DisabledTurns > 0)
                {
                    _fontRenderer.DrawText(_batch, "Disabled!",
                        new Vector2(startX + unit, StartY + unit * 10), Color.Black, Border.SCALE);
                }
                else
                {
                    _fontRenderer.DrawText(_batch,
                        "TYPE/\n" +
                        " " + selectedMove.GetMove().Type.ToString().ToUpper() + "\n" +
                        new string(' ', 4) + selectedMove.pp.ToString().PadLeft(2) + "/" +
                        selectedMove.maxPP.ToString().PadLeft(2),
                        new Vector2(startX + unit, StartY + unit * 9), Color.Black, Border.SCALE);
                }
            }

            // animations
            lock (_animations)
            {
                foreach (var animation in _animations)
                {
                    if (!animation.IsFinished)
                    {
                        animation.Draw(_batch);
                    }
                }
            }

            _batch.End();

            Controller.GraphicsDevice.SetRenderTargets(previousTargets);

            if (_invertColors)
            {
                _batch.Begin(samplerState: SamplerState.PointClamp, blendState: INVERT_COLORS_BLENDSTATE, effect: _screenEffect);
            }
            else
            {
                _batch.Begin(samplerState: SamplerState.PointClamp, effect: _screenEffect);
            }

            var color = Color.White;
            if (_fadeOut)
            {
                color = new Color(255, 255, 255, (int)(255 * (1f - _fadeOutProgress)));
            }
            _batch.Draw(_sceneTarget, _screenOffset, color);

            _batch.End();
        }

        protected abstract void DrawScreen(GameTime gameTime);

        internal override void Update(GameTime gameTime)
        {
            if (_menuState == BattleMenuState.Main)
            {
                if (StartMultiTurnMove())
                {
                    return;
                }

                if (GameboyInputs.RightPressed() && _mainMenuIndex % 2 == 0)
                {
                    _mainMenuIndex++;
                }
                else if (GameboyInputs.LeftPressed() && _mainMenuIndex % 2 == 1)
                {
                    _mainMenuIndex--;
                }
                else if (GameboyInputs.DownPressed() && _mainMenuIndex < 2)
                {
                    _mainMenuIndex += 2;
                }
                else if (GameboyInputs.UpPressed() && _mainMenuIndex > 1)
                {
                    _mainMenuIndex -= 2;
                }

                if (GameboyInputs.APressed())
                {
                    switch (_mainMenuIndex)
                    {
                        case 0: // FIGHT
                            // TODO: struggle
                            _menuState = BattleMenuState.Fight;
                            break;
                        case 1: // PKMN
                            {
                                var partyScreen = new PartyScreen(this, Battle.ActiveBattle.PlayerPokemon.Pokemon);
                                partyScreen.LoadContent();
                                partyScreen.SelectedPokemon += SelectedPokemonForSwitch;
                                GetComponent<ScreenManager>().SetScreen(partyScreen);
                            }
                            break;
                        // TODO: inventory
                        case 3: // RUN
                            // TODO: trainer battles
                            _menuState = BattleMenuState.Off;
                            Battle.ActiveBattle.StartRound(new BattleAction
                            {
                                ActionType = BattleActionType.Run
                            });
                            break;
                    }
                }
            }
            else if (_menuState == BattleMenuState.Fight)
            {
                if (GameboyInputs.DownPressed())
                {
                    _moveMenuIndex++;
                    if (_moveMenuIndex == Battle.ActiveBattle.PlayerPokemon.Pokemon.Moves.Length)
                    {
                        _moveMenuIndex = 0;
                    }
                }
                else if (GameboyInputs.UpPressed())
                {
                    _moveMenuIndex--;
                    if (_moveMenuIndex == -1)
                    {
                        _moveMenuIndex = Battle.ActiveBattle.PlayerPokemon.Pokemon.Moves.Length - 1;
                    }
                }

                if (GameboyInputs.APressed())
                {
                    _menuState = BattleMenuState.Off;
                    Battle.ActiveBattle.StartRound(new BattleAction
                    {
                        ActionType = BattleActionType.Move,
                        MoveName = Battle.ActiveBattle.PlayerPokemon.Pokemon.Moves[_moveMenuIndex].name
                    });
                }
                else if (GameboyInputs.BPressed())
                {
                    _menuState = BattleMenuState.Main;
                }
            }
            else
            {
                if (!_keepTextboxOpen)
                {
                    _battleTextbox.Update();
                }
                else
                {
                    if (_framesBeforeContinuing > 0)
                    {
                        _framesBeforeContinuing--;
                    }
                }

                _enemyPokemonStatus.Update();
                _playerPokemonStatus.Update();
                _pokemonStats.Update();

                lock (_animations)
                {
                    foreach (var animation in _animations)
                    {
                        if (!animation.IsFinished)
                        {
                            if (animation.WaitDelay > 0)
                            {
                                animation.WaitDelay--;
                            }
                            else
                            {
                                animation.Update();
                            }
                        }
                    }
                }
            }

            if (_fadeOut)
            {
                _fadeOutProgress += FADE_OUT_SPEED;
                if (_fadeOutProgress >= 1f)
                {
                    _fadeOutProgress = 1f;
                }
            }
        }

        private bool StartMultiTurnMove()
        {
            // checks if any multi turn move (fly, rollout, etc.) is active and runs it
            // returns whether such a move was initiated

            if (Battle.ActiveBattle.PlayerPokemon.IsFlying)
            {
                _menuState = BattleMenuState.Off;
                Battle.ActiveBattle.StartRound(new BattleAction
                {
                    ActionType = BattleActionType.Move,
                    MoveName = "FLY",
                });
                return true;
            }
            return false;
        }

        private void SelectedPokemonForSwitch(int partyIndex)
        {
            _menuState = BattleMenuState.Off;
            Battle.ActiveBattle.StartRound(new BattleAction
            {
                ActionType = BattleActionType.Switch,
                SwitchToIndex = partyIndex
            });
        }

        private void Close()
        {
            var transitionScreen = new FadeTransitionScreen(this, _preScreen, FADE_OUT_SPEED);
            transitionScreen.LoadContent();
            GetComponent<ScreenManager>().SetScreen(transitionScreen);
        }

        #region interop methods from battle thread

        // sets menu to main
        public void ResetMenu()
        {
            _battleTextbox.Close();
            _menuState = BattleMenuState.Main;
        }

        public void ShowMessageAndWait(string message)
        {
            _keepTextboxOpen = false;
            _battleTextbox.Show(message);
            _battleTextbox.AlwaysDisplayContinueArrow = true;
            SpinWait.SpinUntil(() => !_battleTextbox.Visible);
        }

        public void ShowMessageAndKeepOpen(string message, int frames = 0)
        {
            _keepTextboxOpen = false;
            _framesBeforeContinuing = frames;
            _battleTextbox.Show(message);
            _battleTextbox.AlwaysDisplayContinueArrow = false;
            _battleTextbox.Finished += MessageboxShowMessageFinished;
            SpinWait.SpinUntil(() => _keepTextboxOpen && _framesBeforeContinuing == 0);
        }

        private void MessageboxShowMessageFinished()
        {
            _battleTextbox.Finished -= MessageboxShowMessageFinished;
            _keepTextboxOpen = true;
        }

        public void AnimateEnemyHPAndWait()
        {
            _enemyPokemonStatus.AnimateToTarget();
            SpinWait.SpinUntil(() => _enemyPokemonStatus.AnimationFinished);
        }

        public void AnimatePlayerHPAndWait()
        {
            _playerPokemonStatus.AnimateToTarget();
            SpinWait.SpinUntil(() => _playerPokemonStatus.AnimationFinished);
        }

        public void AnimatePlayerExpAndWait(bool instant = false)
        {
            if (instant)
            {
                _playerPokemonStatus.SkipToTarget();
            }
            else
            {
                _playerPokemonStatus.AnimateToTarget();
                SpinWait.SpinUntil(() => _playerPokemonStatus.AnimationFinished);
            }
        }

        public void ShowAnimationAndWait(BattleAnimation animation, int delay = 0)
        {
            ShowAnimation(animation, delay);
            WaitForAnimations();
        }

        public void ShowAnimation(BattleAnimation animation, int delay = 0)
        {
            animation.LoadContent();
            animation.WaitDelay = delay;
            lock (_animations)
            {
                _animations.Add(animation);
            }
            animation.Show();
        }

        public void WaitForAnimations()
        {
            SpinWait.SpinUntil(() => _animations.All(a => a.IsFinished));
            // after all animations completed, clear list of finished animations
            lock (_animations)
            {
                _animations.Clear();
            }
        }

        public void SetPokemonStatusVisible(PokemonSide side, bool visible)
        {
            if (side == PokemonSide.Enemy)
            {
                _enemyPokemonStatus.Visible = visible;
            }
            else
            {
                _playerPokemonStatus.Visible = visible;
            }
        }

        public void EndBattle(bool won)
        {
            // TODO: lost

            _fadeOut = true;
            SpinWait.SpinUntil(() => _fadeOutProgress == 1f);
            Close();

            // terminate battle thread properly
            Battle.ActiveBattle.BattleThread.Abort();
        }

        public void SetPokemonVisibility(PokemonSide side, bool visible)
        {
            if (side == PokemonSide.Enemy)
            {
                _enemyPokemonVisible = visible;
            }
            else
            {
                _playerPokemonVisible = visible;
            }
        }

        public void SetScreenOffset(Vector2 offset)
        {
            _screenOffset = offset;
        }

        public void SetScreenColorInvert(bool invert)
        {
            _invertColors = invert;
        }

        public void SetPokemonOffset(PokemonSide side, Vector2 offset)
        {
            if (side == PokemonSide.Enemy)
            {
                _enemyPokemonOffset = offset;
            }
            else
            {
                _playerPokemonOffset = offset;
            }
        }

        void IBattleUI.SetPokemonStatusOffset(PokemonSide side, Vector2 offset)
        {
            if (side == PokemonSide.Enemy)
            {
                _enemyPokemonStatus.Offset = offset;
            }
            else
            {
                _playerPokemonStatus.Offset = offset;
            }
        }

        void ITextboxScreen.ShowTextbox(string text, bool skip)
        {
            ShowMessageAndWait(text);
        }

        public void SetPokemonSize(PokemonSide side, float size)
        {
            if (side == PokemonSide.Enemy)
            {
                _enemyPokemonSize = size;
            }
            else
            {
                _playerPokemonSize = size;
            }
        }

        public void SetPokemonColor(PokemonSide side, Color color)
        {
            if (side == PokemonSide.Enemy)
            {
                _enemyPokemonColor = color;
            }
            else
            {
                _playerPokemonColor = color;
            }
        }

        public void SetScreenEffect(Effect effect = null)
        {
            _screenEffect = effect;
        }

        public Color[] SetPokemonPalette(PokemonSide side, Color[] palette)
        {
            if (side == PokemonSide.Enemy)
            {
                var currentPalette = _enemyPokemonPalette;
                _enemyPokemonPalette = palette;
                return currentPalette;
            }
            else
            {
                var currentPalette = _playerPokemonPalette;
                _playerPokemonPalette = palette;
                return currentPalette;
            }
        }

        public void SetPokemonArtificialLevelUp(bool active)
        {
            _playerPokemonStatus.ArtificialLevelUpActive = active;
        }

        public void ShowPokemonStatsAndWait(Pokemon pokemon)
        {
            _pokemonStats.Show(pokemon);
            SpinWait.SpinUntil(() => !_pokemonStats.Visible);
        }

        public void ShowLearnMoveScreen(Pokemon pokemon, PokemonMoveData moveData)
        {
            var screen = new MoveLearnScreen(this, pokemon, moveData, StartY);
            screen.LoadContent();
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(screen);
            SpinWait.SpinUntil(() => screenManager.ActiveScreen.GetType() != typeof(MoveLearnScreen));
        }

        public void SkipPokemonStatusUI()
        {
            _playerPokemonStatus.SkipToTarget();
            _enemyPokemonStatus.SkipToTarget();
        }

        #endregion
    }
}
