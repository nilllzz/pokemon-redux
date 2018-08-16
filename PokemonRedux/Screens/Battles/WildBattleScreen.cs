using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles.Animations;
using System.Threading.Tasks;
using static Core;

namespace PokemonRedux.Screens.Battles
{
    class WildBattleScreen : BattleScreen
    {
        private static readonly Color[] WILD_POKEMON_INTRO_PALETTE = new[]
        {
            new Color(0, 0, 0),
            new Color(16, 24, 24),
            new Color(56, 56, 56),
            new Color(248, 248, 248),
        };

        private Textbox _textbox;

        private float _introProgress; // 0-1, player/enemy pokemon intro progress
        private float _playerOutroProgress; // 0-1, player moves out of frame
        private bool _playerOutroStarted;

        public WildBattleScreen(Screen preScreen, Pokemon wildPokemon)
            : base(preScreen)
        {
            Battle.ActiveBattle = new Battle(this, this, wildPokemon);
            _enemyPokemonVisible = true;
            _enemyPokemonSize = 1f;
        }

        internal override void LoadContent()
        {
            base.LoadContent();

            _textbox = new Textbox();
            _textbox.LoadContent();
            _textbox.OffsetY = StartY + (int)(12 * Border.UNIT * Border.SCALE);
        }

        protected override void DrawScreen(GameTime gameTime)
        {
            var (unit, startX, width, height) = Border.GetDefaultScreenValues();

            // wild pokemon
            if (_enemyPokemonVisible && _enemyPokemonSize > 0f)
            {
                var characterAlpha = (int)((_introProgress * 2) * 255);
                if (characterAlpha > 255)
                {
                    characterAlpha = 255;
                }

                var pokemon = Controller.ActiveBattle.EnemyPokemon.Pokemon;
                Texture2D sprite;
                if (_introProgress < 1f)
                {
                    sprite = PokemonTextureManager.GetFront(pokemon.Id, WILD_POKEMON_INTRO_PALETTE, pokemon.UnownLetter);
                }
                else
                {
                    if (_enemyPokemonPalette == null)
                    {
                        sprite = pokemon.GetFrontSprite();
                    }
                    else
                    {
                        sprite = pokemon.GetFrontSprite(_enemyPokemonPalette);
                    }
                }
                var pokemonTargetX = startX + unit * 12;
                var pokemonStartX = -width - unit * 4;
                var pokemonX = (int)(pokemonTargetX + (pokemonStartX * (1f - _introProgress)));

                var offset = _enemyPokemonOffset;
                var spriteX = (int)(pokemonX + offset.X * Border.SCALE);
                var spriteY = (int)(StartY + offset.Y * Border.SCALE);

                var spriteRectangle = sprite.Bounds;
                var spriteHeight = (int)(sprite.Height * Border.SCALE);
                if (offset.Y > 0)
                {
                    spriteRectangle.Height -= (int)offset.Y;
                    spriteHeight -= (int)(offset.Y * Border.SCALE);
                }

                _batch.Draw(sprite, new Rectangle(spriteX, spriteY,
                    (int)(sprite.Width * Border.SCALE),
                    spriteHeight),
                    spriteRectangle,
                    new Color(_enemyPokemonColor.R, _enemyPokemonColor.G, _enemyPokemonColor.B, characterAlpha));
            }

            // player intro/outro
            if (_playerOutroProgress < 1f)
            {
                int characterAlpha;
                int playerX;

                if (_playerOutroStarted)
                {
                    var playerTargetX = unit * 10;
                    var playerStartX = startX + unit;
                    playerX = (int)(playerStartX - (playerTargetX * _playerOutroProgress));
                    characterAlpha = (int)(((1f - _playerOutroProgress) * 2) * 255);
                }
                else
                {
                    var playerTargetX = startX + unit;
                    var playerStartX = width + unit * 4;
                    playerX = (int)(playerTargetX + (playerStartX * (1f - _introProgress)));
                    characterAlpha = (int)((_introProgress * 2) * 255);
                }

                if (characterAlpha > 255)
                {
                    characterAlpha = 255;
                }

                var playerTextureX = _introProgress < 1f ? 1 : 0;
                _batch.Draw(_player, new Rectangle(playerX, StartY + unit * 5,
                    (int)(_player.Width / 2f * Border.SCALE),
                    (int)(_player.Height * Border.SCALE)),
                    new Rectangle(playerTextureX * 56, 0, 56, 56),
                    new Color(255, 255, 255, characterAlpha));
            }

            // player pokemon
            if (_playerPokemonVisible && _playerPokemonSize > 0f)
            {
                Texture2D sprite;
                if (_playerPokemonPalette == null)
                {
                    sprite = Controller.ActiveBattle.PlayerPokemon.Pokemon.GetBackSprite();
                }
                else
                {
                    sprite = Controller.ActiveBattle.PlayerPokemon.Pokemon.GetBackSprite(_playerPokemonPalette);
                }
                var spriteWidth = (int)((int)(sprite.Width * _playerPokemonSize) * Border.SCALE);
                var spriteHeight = spriteWidth;
                var spriteColor = new Color(_playerPokemonColor.R,
                    (int)(_playerPokemonColor.G * _playerPokemonSize),
                    (int)(_playerPokemonColor.B * _playerPokemonSize));

                var offset = _playerPokemonOffset;
                var spriteX = (int)(startX +
                    unit * 2 +
                    sprite.Width * Border.SCALE / 2f - spriteWidth / 2f +
                    offset.X);
                var spriteY = (int)(StartY +
                    unit * 5 +
                    (sprite.Height * Border.SCALE - spriteHeight) +
                    offset.Y * Border.SCALE);

                var spriteRectangle = sprite.Bounds;
                if (offset.Y > 0)
                {
                    spriteRectangle.Height -= (int)offset.Y;
                    spriteHeight -= (int)(offset.Y * Border.SCALE);
                }

                _batch.Draw(sprite, new Rectangle(spriteX, spriteY, spriteWidth, spriteHeight), spriteRectangle, spriteColor);
            }

            _textbox.Draw(_batch, Border.DefaultWhite);
        }

        internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_playerOutroStarted)
            {
                _playerOutroProgress += 0.03f;
                if (_playerOutroProgress >= 1f)
                {
                    _playerOutroProgress = 1f;
                    _playerOutroStarted = false;

                    StartPokemonIntro();
                }
            }

            if (_introProgress < 1f)
            {
                _introProgress += INTRO_PROGRESS_SPEED;
                if (_introProgress >= 1f)
                {
                    _introProgress = 1f;
                    _playerStatus.Visible = true;
                    _textbox.AlwaysDisplayContinueArrow = true;
                    _textbox.Show($"Wild {Battle.ActiveBattle.EnemyPokemon.Pokemon.DisplayName}\nappeared!");
                    _textbox.Closed += WildPokemonMessageClosed;
                }
            }

            _textbox.Update();
        }

        // wild X appeared! textbox closed
        private void WildPokemonMessageClosed()
        {
            _playerStatus.Visible = false;
            _textbox.Closed -= WildPokemonMessageClosed;
            _playerOutroStarted = true;
            _enemyPokemonStatus.Visible = true;
        }

        private void StartPokemonIntro()
        {
            // run battle animations to throw pokeball from separate thread
            Task.Run(() =>
            {
                SetPokemonVisibility(PokemonSide.Player, true);
                ShowMessageAndKeepOpen($"Go! {Battle.ActiveBattle.PlayerPokemon.GetDisplayName()}!", 10);
                ShowAnimation(new PokemonSizeChangeAnimation(Battle.ActiveBattle.PlayerPokemon, 0f, 1f, 0.07f), 6);
                ShowAnimationAndWait(new PokeballOpeningAnimation(Battle.ActiveBattle.PlayerPokemon));
                SetPokemonStatusVisible(PokemonSide.Player, true);
                ResetMenu();
            });
        }
    }
}
