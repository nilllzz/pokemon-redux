using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class HardenAnimation : BattleMoveAnimation
    {
        private const float PROGRESS_SPEED = 0.03f;
        private const float PROGRESS_START = 0.1f;
        private const float TOTAL_AMOUNTS = 2;
        private static readonly Color[] HARDEN_PALETTE = new[]
        {
            new Color(0, 0, 0),
            new Color(0, 0, 0),
            new Color(0, 0, 0),
            new Color(248, 248, 248),
        };

        private int _amounts;
        private float _progress = PROGRESS_START;
        private Color[] _previousPalette;

        public HardenAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("harden");
        }

        public override void Show()
        {
            base.Show();
            // set monochromatic palette
            _previousPalette = Battle.ActiveBattle.AnimationController.SetPokemonPalette(BattlePokemon.ReverseSide(_target.Side), HARDEN_PALETTE);
        }

        public override void Draw(SpriteBatch batch)
        {
            var progress = _progress * 2;
            if (progress > 1f) // adjust to scale 0 -> 1 -> 0
            {
                progress = 2f - progress;
            }

            var sourceSize = (int)Math.Ceiling(progress * _texture.Width);
            var textureSize = (int)(Math.Ceiling(progress * _texture.Width) * Border.SCALE);
            var center = GetCenter(BattlePokemon.ReverseSide(_target.Side));

            if (_progress <= 0.5f)
            {
                var origin = center + new Vector2(GetPokemonSpriteSize() / 2f, -GetPokemonSpriteSize() / 2f);
                var pos = origin + new Vector2(-textureSize, 0);
                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, textureSize, textureSize),
                    new Rectangle(0, _texture.Height - sourceSize, sourceSize, sourceSize), Border.DefaultWhite);
            }
            else
            {
                var origin = center + new Vector2(-GetPokemonSpriteSize() / 2f, GetPokemonSpriteSize() / 2f);
                var pos = origin + new Vector2(0, -textureSize);
                batch.Draw(_texture, new Rectangle((int)pos.X, (int)pos.Y, textureSize, textureSize),
                    new Rectangle(_texture.Width - sourceSize, 0, sourceSize, sourceSize), Border.DefaultWhite);
            }
        }

        public override void Update()
        {
            _progress += PROGRESS_SPEED;
            if (_progress >= 1f)
            {
                _progress = PROGRESS_START;
                _amounts++;
                if (_amounts == TOTAL_AMOUNTS)
                {
                    Finish();
                    // reset palette
                    Battle.ActiveBattle.AnimationController.SetPokemonPalette(BattlePokemon.ReverseSide(_target.Side), _previousPalette);
                }
            }
        }
    }
}
