using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Screens.Pack;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens.Computer
{
    class ComputerSelectionScreen : Screen
    {
        private const int CLOSE_DELAY = 40;

        private readonly Screen _preScreen;

        private SpriteBatch _batch;
        private Textbox _textbox;
        private OptionsBox _optionsBox, _confirmationBox;

        private bool _textboxFocused; // update textbox, not options box
        private bool _isClosing;
        private int _closeDelay; // delay before the "Link closed" textbox disappears and the screen closes

        public ComputerSelectionScreen(Screen preScreen)
        {
            _preScreen = preScreen;
        }

        internal override void LoadContent()
        {
            _batch = new SpriteBatch(Controller.GraphicsDevice);

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = (int)(Border.SCREEN_HEIGHT * Border.SCALE * Border.UNIT - Textbox.HEIGHT * Border.UNIT * Border.SCALE);

            _optionsBox = new OptionsBox();
            _optionsBox.LoadContent();
            _optionsBox.OffsetY = 0;

            ShowMain();
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

            _batch.End();
        }

        internal override void Update(GameTime gameTime)
        {
            if (_isClosing)
            {
                if (_closeDelay > 0)
                {
                    _closeDelay--;
                    if (_closeDelay == 0)
                    {
                        GetComponent<ScreenManager>().SetScreen(_preScreen);
                    }
                }
                if (_textboxFocused)
                {
                    _textbox.Update();
                }
            }
            else
            {
                if (_textboxFocused)
                {
                    _textbox.Update();
                }
                else
                {
                    _optionsBox.Update();
                }
            }
        }

        private void ShowMain()
        {
            _textbox.ShowAndSkip("Access whose PC?");

            var options = new List<string>
            {
                "BILL^'s PC",
                Controller.ActivePlayer.Name + "^'s PC"
            };
            if (Controller.ActivePlayer.HasPokedex)
            {
                options.Add("PROF.OAK^'s PC");
            }
            if (Controller.ActivePlayer.HallOfFame.Length > 0)
            {
                options.Add("HALL OF FAME");
            }
            options.Add("TURN OFF");

            _optionsBox.BufferUp = 1;
            _optionsBox.BufferRight = 1;
            _optionsBox.CloseAfterSelection = false;
            _optionsBox.Show(options.ToArray());
            _optionsBox.OffsetY = (int)(_optionsBox.Height * Border.SCALE * Border.UNIT);
            _optionsBox.OptionSelected += MainOptionSelected;

            _textboxFocused = false;
        }

        private void MainOptionSelected(string option, int index)
        {
            if (option == Controller.ActivePlayer.Name + "^'s PC") // no constant expression in switch allowed
            {
                _optionsBox.OptionSelected -= MainOptionSelected;
                ShowPlayersPC();
            }
            else
            {
                switch (option)
                {
                    case "BILL^'s PC":
                        _optionsBox.OptionSelected -= MainOptionSelected;
                        ShowBillsPC();
                        break;
                    case "PROF.OAK^'s PC":
                        // TODO: prof oak evaluation
                        break;
                    case "HALL OF FAME":
                        {
                            var hallOfFameScreen = new HallOfFameScreen(this);
                            hallOfFameScreen.LoadContent();
                            GetComponent<ScreenManager>().SetScreen(hallOfFameScreen);
                        }
                        break;
                    case "TURN OFF":
                        _optionsBox.OptionSelected -= MainOptionSelected;
                        Close();
                        break;
                }
            }
        }

        #region Bills PC

        private void ShowBillsPC()
        {
            _textbox.Show("BILL^s PC\naccessed.\n\nPOKéMON Storage\nSystem opened.");
            _textboxFocused = true;
            _textbox.Closed += ShowBillsPCFinished;
        }

        private void ShowBillsPCFinished()
        {
            _textbox.Closed -= ShowBillsPCFinished;
            _textbox.ShowAndSkip("What?");
            _textboxFocused = false;

            _optionsBox.BufferUp = 1;
            _optionsBox.BufferRight = 1;
            _optionsBox.CloseAfterSelection = false;
            _optionsBox.Show(new[] { "WITHDRAW ^PK^MN", "DEPOSIT ^PK^MN", "CHANGE BOX", "MOVE ^PK^MN", "SEE YA!" });
            _optionsBox.OffsetY = (int)(_optionsBox.Height * Border.SCALE * Border.UNIT);
            _optionsBox.OptionSelected += BillsPCOptionSelected;
        }

        private void BillsPCOptionSelected(string option, int index)
        {
            switch (index)
            {
                case 0: // withdraw
                    {
                        var withdrawScreen = new StorageSystemWithdrawScreen(this);
                        withdrawScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(withdrawScreen);
                    }
                    break;
                case 1: // deposit
                    {
                        var depositScreen = new StorageSystemDepositScreen(this);
                        depositScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(depositScreen);
                    }
                    break;
                case 2: // change box
                    {
                        var chooseBoxScreen = new StorageSystemChooseBoxScreen(this);
                        chooseBoxScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(chooseBoxScreen);
                    }
                    break;
                case 3: // move
                    {
                        var moveScreen = new StorageSystemMoveScreen(this);
                        moveScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(moveScreen);
                    }
                    break;
                case 4: // close
                    _optionsBox.OptionSelected -= BillsPCOptionSelected;
                    ShowMain();
                    break;
            }
        }

        #endregion

        #region Player's PC

        private void ShowPlayersPC()
        {
            _textbox.Show("Accessed own PC.\n\nItem Storage\nSystem opened.");
            _textboxFocused = true;
            _textbox.Closed += ShowPlayersPCFinished;
        }

        private void ShowPlayersPCFinished()
        {
            _textbox.Closed -= ShowPlayersPCFinished;
            _textbox.ShowAndSkip("What do you want\nto do?");
            _textboxFocused = false;

            _optionsBox.BufferUp = 1;
            _optionsBox.BufferRight = 0;
            _optionsBox.CloseAfterSelection = false;
            _optionsBox.Show(new[] { "WITHDRAW ITEM", "DEPOSIT ITEM", "TOSS ITEM", "MAIL BOX", "LOG OFF" });
            _optionsBox.OffsetY = (int)(_optionsBox.Height * Border.SCALE * Border.UNIT);
            _optionsBox.OptionSelected += PlayersPCOptionSelected;
        }

        private void PlayersPCOptionSelected(string option, int index)
        {
            switch (index)
            {
                case 0: // withdraw
                    {
                        var withdrawScreen = new ComputerItemScreen(this, ComputerItemMode.Withdraw);
                        withdrawScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(withdrawScreen);
                    }
                    break;
                case 1: // deposit
                    {
                        var depositScreen = new PackScreen(this, PackMode.Deposit);
                        depositScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(depositScreen);
                    }
                    break;
                case 2: // toss
                    {
                        var tossScreen = new ComputerItemScreen(this, ComputerItemMode.Toss);
                        tossScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(tossScreen);
                    }
                    break;
                case 3: // mail box
                    {
                        var mailScreen = new MailboxScreen(this);
                        mailScreen.LoadContent();
                        GetComponent<ScreenManager>().SetScreen(mailScreen);
                    }
                    break;
                case 4: // log off
                    _optionsBox.OptionSelected -= PlayersPCOptionSelected;
                    ShowMain();
                    break;
            }
        }

        #endregion

        private void Close()
        {
            _textbox.Show("^..\nLink closed^..");
            _textbox.Finished += CloseFinished;
            _isClosing = true;
            _textboxFocused = true;
        }

        private void CloseFinished()
        {
            _textbox.Finished -= CloseFinished;
            _textboxFocused = false;
            _closeDelay = CLOSE_DELAY;
        }
    }
}
