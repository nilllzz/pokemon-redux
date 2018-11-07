using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Pokemons;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class AuroraBeamAnimation : BattleMoveAnimation
    {
        private const int TOTAL_PALETTE_CYCLES = 7;
        private const int PALETTE_DELAY = 3;
        private static readonly int[] PALETTE_INDICES = new[] { 0, 1, 2, 1, 0, 3, 4, 3 };
        private const int BEAM_INITIAL_DELAY = 64;
        private const int BEAM_END_DELAY = 24;
        private const float BEAM_PROGRESS_PER_FRAME = 0.04f;
        private const float BEAM_RETRACT_PER_FRAME = 0.06f;
        private const int TOTAL_BEAM_PARTS = 32;

        private readonly Color[][] _userPalettes = new Color[5][];
        private readonly Color[][] _targetPalettes = new Color[5][];

        private int _paletteState = 0;
        private int _paletteCycle = 0;
        private int _paletteDelay = PALETTE_DELAY;
        private int _beamState = 0;
        private int _beamDelay = BEAM_INITIAL_DELAY;
        private float _beamProgress = 0f;
        private bool _beamRetract = false;

        public AuroraBeamAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("aurorabeam");

            _userPalettes[0] = _user.Pokemon.GetPalette();
            _userPalettes[1] = PokemonTextureManager.ShiftPalette(_userPalettes[0], true, 1); // dark
            _userPalettes[2] = PokemonTextureManager.ShiftPalette(_userPalettes[0], true, 2); // darker
            _userPalettes[3] = PokemonTextureManager.ShiftPalette(_userPalettes[0], false, 1); // light
            _userPalettes[4] = PokemonTextureManager.ShiftPalette(_userPalettes[0], false, 2); // lighter

            _targetPalettes[0] = _target.Pokemon.GetPalette();
            _targetPalettes[1] = PokemonTextureManager.ShiftPalette(_targetPalettes[0], true, 1); // dark
            _targetPalettes[2] = PokemonTextureManager.ShiftPalette(_targetPalettes[0], true, 2); // darker
            _targetPalettes[3] = PokemonTextureManager.ShiftPalette(_targetPalettes[0], false, 1); // light
            _targetPalettes[4] = PokemonTextureManager.ShiftPalette(_targetPalettes[0], false, 2); // lighter
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_beamProgress > 0f)
            {
                var texX = (_beamState % 2) * 16;

                var parts = (int)Math.Floor(TOTAL_BEAM_PARTS * _beamProgress);
                var skippedParts = 0;
                if (_beamRetract)
                {
                    skippedParts = TOTAL_BEAM_PARTS - parts;
                }

                var partWidth = 2 * Border.SCALE;
                var partHeight = 9 * Border.SCALE;

                var center = GetCenter(_user.Side);
                var posX = center.X;
                if (_user.Side == PokemonSide.Player)
                {
                    posX += Border.UNIT * Border.SCALE;
                }
                else
                {
                    posX -= Border.UNIT * Border.SCALE;
                }
                var posY = center.Y;
                var stepX = _user.Side == PokemonSide.Player ? partWidth : -partWidth;
                var stepY = _user.Side == PokemonSide.Player ? -Border.SCALE : Border.SCALE;

                // skip retracted parts
                posX += skippedParts * stepX;
                posY += skippedParts * stepY;

                for (var i = 0; i < parts; i++)
                {
                    batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)partWidth, (int)partHeight),
                        new Rectangle(texX, 40, 2, 9), Color.White);
                    posX += stepX;
                    posY += stepY;
                }

                // draw cone
                if (_beamProgress < 0.9f && !_beamRetract)
                {
                    var coneSize = Border.SCALE * 16;
                    var effect = _user.Side == PokemonSide.Player ?
                        SpriteEffects.None : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

                    var offsetX = 0f;
                    var offsetY = 0f;
                    if (_user.Side == PokemonSide.Enemy)
                    {
                        offsetX = -coneSize - stepX;
                    }
                    else
                    {
                        offsetY = partHeight - coneSize;
                    }

                    batch.Draw(_texture, new Rectangle((int)(posX + offsetX), (int)(posY + offsetY), (int)coneSize, (int)coneSize),
                        new Rectangle(texX, 0, 16, 16), Color.White, 0f, Vector2.Zero, effect, 0f);
                }

                // hit
                if (_beamProgress >= 0.85f || _beamRetract)
                {
                    var effect = _user.Side == PokemonSide.Player ?
                        SpriteEffects.None : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

                    var hitWidth = 16 * Border.SCALE;
                    var hitHeight = 24 * Border.SCALE;

                    var offsetX = -stepX;
                    var offsetY = 0f;
                    if (_user.Side == PokemonSide.Enemy)
                    {
                        offsetX -= hitWidth;
                        offsetY = -4 * Border.SCALE;
                    }
                    else
                    {
                        offsetY = -10 * Border.SCALE;
                    }

                    batch.Draw(_texture, new Rectangle((int)(posX + offsetX), (int)(posY + offsetY), (int)hitWidth, (int)hitHeight),
                        new Rectangle(texX, 16, 16, 24), Color.White, 0f, Vector2.Zero, effect, 0f);
                }
            }
        }

        public override void Update()
        {
            _paletteDelay--;
            if (_paletteDelay == 0)
            {
                _paletteDelay = PALETTE_DELAY;
                _paletteState++;
                if (_paletteState == PALETTE_INDICES.Length)
                {
                    _paletteState = 0;
                    _paletteCycle++;
                    if (_paletteCycle == TOTAL_PALETTE_CYCLES)
                    {
                        Finish();
                    }
                }

                var paletteIndex = PALETTE_INDICES[_paletteState];
                Battle.ActiveBattle.AnimationController.SetPokemonPalette(_user.Side, _userPalettes[paletteIndex]);
                Battle.ActiveBattle.AnimationController.SetPokemonPalette(_target.Side, _targetPalettes[paletteIndex]);

                // change beam color
                _beamState++;
            }

            if (_beamDelay > 0)
            {
                _beamDelay--;
            }
            else
            {
                if (_beamRetract)
                {
                    if (_beamProgress > 0f)
                    {
                        _beamProgress -= BEAM_RETRACT_PER_FRAME;
                        if (_beamProgress <= 0f)
                        {
                            _beamProgress = 0f;
                        }
                    }
                }
                else
                {
                    if (_beamProgress < 1f)
                    {
                        _beamProgress += BEAM_PROGRESS_PER_FRAME;
                        if (_beamProgress >= 1f)
                        {
                            _beamProgress = 1f;
                            _beamDelay = BEAM_END_DELAY;
                            _beamRetract = true;
                        }
                    }
                }
            }
        }
    }
}
