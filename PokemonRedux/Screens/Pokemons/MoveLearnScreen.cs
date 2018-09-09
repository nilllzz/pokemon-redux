using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data;
using PokemonRedux.Game.Pokemons;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Pokemons
{
    class MoveLearnScreen : Screen
    {
        private readonly Screen _preScreen;
        private readonly Pokemon _pokemon;
        private readonly PokemonMoveData _moveData;
        private readonly int _offsetY;

        private SpriteBatch _batch;
        private Textbox _textbox;
        private OptionsBox _optionsBox, _moveSelectBox;

        public MoveLearnScreen(Screen preScreen, Pokemon pokemon, PokemonMoveData moveData, int offsetY)
        {
            _preScreen = preScreen;
            _pokemon = pokemon;
            _moveData = moveData;
            _offsetY = offsetY;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = _offsetY + (int)(12 * Border.UNIT * Border.SCALE);

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.OffsetY = _textbox.OffsetY;
            _optionsBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 14);

            _moveSelectBox = new OptionsBox();
            _moveSelectBox.LoadContent();
            _moveSelectBox.OffsetY = _textbox.OffsetY;
            _moveSelectBox.OffsetX += (int)(Border.SCALE * Border.UNIT * 5);
            _moveSelectBox.BufferUp = 1;

            ShowInitialQuestion();
        }

        internal override void UnloadContent()
        {

        }

        internal override void Draw(GameTime gameTime)
        {
            _preScreen.Draw(gameTime);

            _batch.Begin(samplerState: SamplerState.PointClamp);

            _textbox.Draw(_batch, Border.DefaultWhite);
            _optionsBox.Draw(_batch, Border.DefaultWhite);
            _moveSelectBox.Draw(_batch, Border.DefaultWhite);

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (_moveSelectBox.Visible)
            {
                _moveSelectBox.Update();
            }
            else if (_optionsBox.Visible)
            {
                _optionsBox.Update();
            }
            else if (_textbox.Visible)
            {
                _textbox.Update();
            }
        }

        private void ShowInitialQuestion()
        {
            var name = _pokemon.DisplayName;
            var move = _moveData.name;
            _textbox.Show($"{name} is\ntrying to learn\n{move}.\n\nBut {name}\ncan^t learn more\nthan four moves.\n\nDelete an older\nmove to make room\nfor {move}?");
            _textbox.Finished += OnInitialQuestionFinished;
        }

        private void OnInitialQuestionFinished()
        {
            _textbox.Finished -= OnInitialQuestionFinished;
            _optionsBox.Show(new[] { "YES", "NO" });
            _optionsBox.OptionSelected += InitialQuestionSelected;
        }

        private void InitialQuestionSelected(string option, int index)
        {
            _optionsBox.OptionSelected -= InitialQuestionSelected;
            switch (index)
            {
                case 0: // YES
                    ShowMoveQuestion();
                    break;
                case 1: // NO
                    ShowStopLearning();
                    break;
            }
        }

        private void ShowMoveQuestion()
        {
            _textbox.Show("Which move should\nbe forgotten?");
            _textbox.Finished += OnMoveQuestionFinished;
        }

        private void OnMoveQuestionFinished()
        {
            _textbox.Finished -= OnMoveQuestionFinished;
            _moveSelectBox.BufferRight = 12 - _pokemon.Moves.Max(m => m.name.Length);
            _moveSelectBox.Show(_pokemon.Moves.Select(m => m.name).ToArray(), _pokemon.Moves.Length);
            _moveSelectBox.OptionSelected += ForgetMoveSelected;
        }

        private void ForgetMoveSelected(string option, int index)
        {
            _moveSelectBox.OptionSelected -= ForgetMoveSelected;
            if (index == _pokemon.Moves.Length)
            {
                // canceled selection
                ShowStopLearning();
            }
            else
            {
                var oldMove = _pokemon.Moves[index];

                if (oldMove.GetMove().IsHM)
                {
                    // can't forget hm moves
                    _textbox.Show("HM moves can^t be\nforgotten now.");
                    _textbox.Closed += HMWarningClosed;
                }
                else
                {
                    _pokemon.RemoveMove(index);
                    _pokemon.AddMove(_moveData);
                    var name = _pokemon.DisplayName;
                    var move = _moveData.name;
                    _textbox.Show($"1, 2 and^.. Poof!\n\n{name} forgot\n{oldMove.name}.\n\nAnd^..\n\n{name} learned\n{move}!");
                    _textbox.Closed += ForgetMoveClosed;
                }
            }
        }

        private void HMWarningClosed()
        {
            _textbox.Closed += HMWarningClosed;
            ShowMoveQuestion();
        }

        private void ForgetMoveClosed()
        {
            _textbox.Finished -= ForgetMoveClosed;
            Close();
        }

        private void ShowStopLearning()
        {
            _textbox.Show($"Stop learning\n{_moveData.name}?");
            _textbox.Finished += OnStopQuestionFinished;
        }

        private void OnStopQuestionFinished()
        {
            _textbox.Finished -= OnStopQuestionFinished;
            _optionsBox.Show(new[] { "YES", "NO" });
            _optionsBox.OptionSelected += StopQuestionSelected;
        }

        private void StopQuestionSelected(string option, int index)
        {
            _optionsBox.OptionSelected -= StopQuestionSelected;
            switch (index)
            {
                case 0: // YES
                    _textbox.Show($"{_pokemon.Name}\ndid not learn\n{_moveData.name}.");
                    _textbox.Closed += CanceledMoveLearningClosed;
                    break;
                case 1: // NO
                    ShowInitialQuestion();
                    break;
            }
        }

        private void CanceledMoveLearningClosed()
        {
            _textbox.Closed -= CanceledMoveLearningClosed;
            Close();
        }

        private void Close()
        {
            GetComponent<ScreenManager>().SetScreen(_preScreen);
        }
    }
}
