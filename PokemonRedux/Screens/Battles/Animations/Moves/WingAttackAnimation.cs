using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class WingAttackAnimation : BattleMoveAnimation
    {
        private const int TEXTURE_SIZE = 24;
        private const float SPEED = 0.04f;

        private float _progress;

        public WingAttackAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("wingattack");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            var textureSize = (int)(TEXTURE_SIZE * Border.SCALE);
            var y = (int)center.Y;
            var xLeft = (int)(center.X - textureSize / 2f - (GetPokemonSpriteSize() / 2f * (1f - _progress)));
            var xRight = (int)(center.X - textureSize / 2f + (GetPokemonSpriteSize() / 2f * (1f - _progress)));
            batch.Draw(_texture, new Rectangle(xLeft, y, textureSize, textureSize), Color.White);
            batch.Draw(_texture, new Rectangle(xRight, y, textureSize, textureSize), Color.White);
        }

        public override void Update()
        {
            _progress += SPEED;

            if (_progress >= 1f)
            {
                Finish();
            }
        }
    }
}
