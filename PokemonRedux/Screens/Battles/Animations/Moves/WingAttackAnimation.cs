using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class WingAttackAnimation : BattleAnimation
    {
        private const int TEXTURE_SIZE = 24;
        private const float SPEED = 0.04f;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private float _progress;

        public WingAttackAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/wingattack.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            var textureSize = (int)(TEXTURE_SIZE * Border.SCALE);
            var y = (int)(center.Y);
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
                IsFinished = true;
                // show status again
                Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
            }
        }
    }
}
