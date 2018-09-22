using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using static Core;

namespace PokemonRedux.Screens.Pokemons
{
    class SummaryScreen : Screen
    {
        private const int PAGE_COUNT = 3;
        private static readonly Color[] PAGE_COLORS = new[] {
            new Color(248, 152, 248),
            new Color(168, 248, 112),
            new Color(136, 248, 248) };
        private static readonly Color EXPBAR_COLOR = new Color(32, 136, 248);

        private readonly Screen _preScreen;
        private readonly Func<Pokemon> _nextPokemon, _previousPokemon;

        private SpriteBatch _batch;
        private Texture2D _pages, _expBar;
        private PokemonFontRenderer _fontRenderer;
        private HPBar _hpbar;

        private Pokemon _pokemon;
        private int _pageIndex;

        public SummaryScreen(Screen preScreen, Pokemon pokemon, Func<Pokemon> nextPokemon, Func<Pokemon> previousPokemon)
        {
            _preScreen = preScreen;
            _pokemon = pokemon;
            _nextPokemon = nextPokemon;
            _previousPokemon = previousPokemon;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _pages = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/summaryPages.png");
            _expBar = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/expBar.png");

            _fontRenderer = new PokemonFontRenderer();
            _fontRenderer.LoadContent();

            _hpbar = new HPBar();
            _hpbar.LoadContent();
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
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            Border.DrawCenter(_batch, startX, 0, Border.SCREEN_WIDTH, 8, Border.SCALE);

            // pokemon portrait
            _batch.Draw(_pokemon.GetFrontSprite(),
                new Rectangle(
                    startX, 0,
                    (int)(56 * Border.SCALE),
                    (int)(56 * Border.SCALE)),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);

            // basic info
            var lvText = "";
            if (_pokemon.Level == Pokemon.MAX_LEVEL)
            {
                lvText = Pokemon.MAX_LEVEL.ToString().PadRight(4);
            }
            else
            {
                lvText = "^:L" + _pokemon.Level.ToString().PadRight(3);
            }

            _fontRenderer.LineGap = 1;
            _fontRenderer.DrawText(_batch,
                "^NO." + _pokemon.Id.ToString("D3") +
                " " + lvText +
                PokemonStatHelper.GetGenderChar(_pokemon.Gender) + Environment.NewLine +
                _pokemon.DisplayName + Environment.NewLine +
                " /" + _pokemon.Name,
                new Vector2(startX + unit * 8, 0), Color.Black, Border.SCALE);

            // page selector
            _fontRenderer.DrawText(_batch,
                "<      >",
                new Vector2(startX + unit * 12, unit * 6), Color.Black, Border.SCALE);
            for (var i = 0; i < PAGE_COUNT; i++)
            {
                _batch.Draw(_pages, new Rectangle(startX + unit * 13 + i * 2 * unit, unit * 5, unit * 2, unit * 2),
                    new Rectangle(i * 16, i == _pageIndex ? 16 : 0, 16, 16), Color.White);
            }

            // separator line
            _batch.DrawRectangle(new Rectangle(startX, (int)(unit * 8 - Border.SCALE * 2), width, (int)(Border.SCALE)), Color.Black);

            // background bottom
            _batch.DrawRectangle(new Rectangle(startX, unit * 8, width, unit * 10), PAGE_COLORS[_pageIndex]);

            // draw page content
            switch (_pageIndex)
            {
                case 0:
                    // page 1
                    {
                        // hp bar
                        _hpbar.Draw(_batch, new Vector2(startX, unit * 9), _pokemon.HP, _pokemon.MaxHP, Border.SCALE);
                        _fontRenderer.DrawText(_batch,
                            _pokemon.HP.ToString().PadLeft(3) + "/" + _pokemon.MaxHP.ToString().PadLeft(3),
                            new Vector2(startX + unit, unit * 10), Color.Black, Border.SCALE);

                        // status and type
                        var statusText =
                            "STATUS/" + Environment.NewLine +
                            _pokemon.Status.ToString().ToUpper().PadLeft(8) + Environment.NewLine +
                            "TYPE/" + Environment.NewLine +
                            " " + _pokemon.Type1.ToString().ToUpper();
                        if (_pokemon.Type2 != PokemonType.None)
                        {
                            statusText += Environment.NewLine + " " + _pokemon.Type2.ToString().ToUpper();
                        }

                        _fontRenderer.LineGap = 0;
                        _fontRenderer.DrawText(_batch, statusText,
                            new Vector2(startX, unit * 12), Color.Black, Border.SCALE);

                        // divider line
                        _batch.DrawRectangle(new Rectangle(
                            startX + unit * 9,
                            unit * 8,
                            (int)(Border.SCALE * 2),
                            unit * 10),
                            Color.Black);

                        // exp and level
                        var toLvText = "";
                        if (_pokemon.Level == Pokemon.MAX_LEVEL)
                        {
                            toLvText = ("TO " + Pokemon.MAX_LEVEL.ToString()).PadLeft(10);
                        }
                        else
                        {
                            // two extra chars for ^:
                            var toLv = _pokemon.Level + 1;
                            toLvText = ("TO ^:L" + toLv.ToString()).PadLeft(12);
                        }

                        _fontRenderer.DrawText(_batch,
                            "EXP POINTS" + Environment.NewLine +
                            _pokemon.Experience.ToString().PadLeft(10) + Environment.NewLine + Environment.NewLine +
                            "LEVEL UP" + Environment.NewLine +
                            _pokemon.NeededExperience.ToString().PadLeft(10) + Environment.NewLine +
                            toLvText,
                            new Vector2(startX + unit * 10, unit * 9), Color.Black, Border.SCALE);

                        // exp bar
                        _batch.Draw(_expBar, new Rectangle(
                            startX + unit * 10, unit * 16,
                            (int)(_expBar.Width * Border.SCALE),
                            (int)(_expBar.Height * Border.SCALE)),
                            Color.White);
                        if (_pokemon.Level < Pokemon.MAX_LEVEL)
                        {
                            var expCurrentLv = PokemonStatHelper.GetExperienceForLevel(_pokemon.ExperienceType, _pokemon.Level);
                            var expProgress = (double)(_pokemon.Experience - expCurrentLv) /
                                (PokemonStatHelper.GetExperienceForLevel(_pokemon.ExperienceType, _pokemon.Level + 1) - expCurrentLv);
                            var barWidth = (int)(Math.Ceiling(64 * Border.SCALE * expProgress));
                            _batch.DrawRectangle(new Rectangle(
                                (int)(startX + unit * 11 + 64 * Border.SCALE - barWidth),
                                (int)(unit * 16 + Border.SCALE * 3),
                                barWidth,
                                (int)(Border.SCALE * 2)), EXPBAR_COLOR);
                        }
                    }
                    break;
                case 1:
                    // page 2
                    {
                        var itemText = "";
                        if (_pokemon.HeldItem == null)
                        {
                            itemText = "---";
                        }
                        else
                        {
                            itemText = _pokemon.HeldItem.Name;
                        }

                        var moveListText = "";
                        for (var i = 0; i < Pokemon.MAX_MOVES; i++)
                        {
                            if (i == 0)
                            {
                                moveListText += "MOVE    ";
                            }
                            else
                            {
                                moveListText += "        ";
                            }

                            if (_pokemon.Moves.Length > i)
                            {
                                var move = _pokemon.Moves[i];
                                moveListText += move.name + Environment.NewLine +
                                    new string(' ', 12) + "^PP^PP " + move.pp.ToString().PadLeft(2) + "/" + move.maxPP.ToString().PadLeft(2);
                            }
                            else
                            {
                                moveListText += "-" + Environment.NewLine +
                                    new string(' ', 12) + "--";
                            }
                            moveListText += Environment.NewLine;
                        }

                        _fontRenderer.LineGap = 0;
                        _fontRenderer.DrawText(_batch,
                            "ITEM  " + itemText + Environment.NewLine + Environment.NewLine +
                            moveListText,
                            new Vector2(startX, unit * 8), Color.Black, Border.SCALE);

                    }
                    break;
                case 2:
                    // page 3
                    {
                        // ot/trainer id
                        var trainerText = "^ID^NO." + Environment.NewLine +
                            "  " + _pokemon.TrainerID.ToString("D5") + Environment.NewLine + Environment.NewLine +
                            "OT/" + Environment.NewLine +
                            "  " + _pokemon.OT;

                        _fontRenderer.LineGap = 0;
                        _fontRenderer.DrawText(_batch, trainerText,
                            new Vector2(startX, unit * 9), Color.Black, Border.SCALE);

                        // divider line
                        _batch.DrawRectangle(new Rectangle(
                            startX + unit * 10,
                            unit * 8,
                            (int)(Border.SCALE * 2),
                            unit * 10),
                            Color.Black);

                        var statText = "ATTACK" + Environment.NewLine +
                            _pokemon.Attack.ToString().PadLeft(9) + Environment.NewLine +
                            "DEFENSE" + Environment.NewLine +
                            _pokemon.Defense.ToString().PadLeft(9) + Environment.NewLine +
                            "SPCL.ATK" + Environment.NewLine +
                            _pokemon.SpecialAttack.ToString().PadLeft(9) + Environment.NewLine +
                            "SPCL.DEF" + Environment.NewLine +
                            _pokemon.SpecialDefense.ToString().PadLeft(9) + Environment.NewLine +
                            "SPEED" + Environment.NewLine +
                            _pokemon.Speed.ToString().PadLeft(9) + Environment.NewLine;

                        _fontRenderer.DrawText(_batch, statText,
                            new Vector2(startX + unit * 11, unit * 8), Color.Black, Border.SCALE);
                    }
                    break;
            }

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (GameboyInputs.RightPressed())
            {
                _pageIndex++;
                if (_pageIndex == PAGE_COUNT)
                {
                    _pageIndex = 0;
                }
            }
            else if (GameboyInputs.LeftPressed())
            {
                _pageIndex--;
                if (_pageIndex == -1)
                {
                    _pageIndex = PAGE_COUNT - 1;
                }
            }
            else if (GameboyInputs.UpPressed())
            {
                var newPokemon = _previousPokemon();
                if (newPokemon != null)
                {
                    _pokemon = newPokemon;
                }
            }
            else if (GameboyInputs.DownPressed())
            {
                var newPokemon = _nextPokemon();
                if (newPokemon != null)
                {
                    _pokemon = newPokemon;
                }
            }

            if (GameboyInputs.APressed())
            {
                if (_pageIndex < PAGE_COUNT - 1)
                {
                    _pageIndex++;
                }
                else
                {
                    Close();
                }
            }
            else if (GameboyInputs.BPressed())
            {
                Close();
            }
        }

        private void Close()
        {
            var screenManager = GetComponent<ScreenManager>();
            screenManager.SetScreen(_preScreen);
        }
    }
}
