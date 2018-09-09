using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    class LevelUpAnimation : BattleAnimation
    {
        private const float ANIMATION_SPEED = 1f;
        private const float FINAL_STAGE = 14f;

        private Texture2D _texture;

        private float _stage = 0;

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/levelup.png");
        }

        public override void Draw(SpriteBatch batch)
        {
            var (startX, startY, unit) = GetScreenValues();
            var posX = startX + 9 * unit + 5 * Border.SCALE;
            var posY = startY + 11 * unit + 1 * Border.SCALE;
            var size = (int)(6 * Border.SCALE);

            var radius = _stage * Border.SCALE;
            for (var i = 0; i < 8; i++)
            {
                var angle = MathHelper.PiOver4 * i;
                var x = posX + radius * Math.Cos(angle);
                var y = posY + radius * Math.Sin(angle);

                batch.Draw(_texture, new Rectangle((int)x, (int)y, size, size), Color.White);
            }
        }

        public override void Update()
        {
            _stage += ANIMATION_SPEED;
            if (_stage >= FINAL_STAGE)
            {
                IsFinished = true;
            }
        }
    }
}
