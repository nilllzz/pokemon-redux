using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class ConfusionAnimation : BattleMoveAnimation
    {
        private const float EFFECT_OFFSET_SPEED = 0.6f;
        private const int TOTAL_DURATION = 120;

        private Effect _shader;

        private float _effectOffset = 0f;
        private int _duration;

        public ConfusionAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            _shader = Controller.Content.Load<Effect>("Shaders/Battle/confusion");
        }

        public override void Show()
        {
            base.Show();

            // setup shader
            Battle.ActiveBattle.AnimationController.SetScreenEffect(_shader);

            var top = GetCenter(_target.Side).Y - GetPokemonSpriteSize() / 2f;
            _shader.Parameters["offset"].SetValue(_effectOffset);
            _shader.Parameters["startY"].SetValue(top);
            _shader.Parameters["endY"].SetValue(top + GetPokemonSpriteSize());
        }

        public override void Draw(SpriteBatch batch)
        { }

        public override void Update()
        {
            _effectOffset += EFFECT_OFFSET_SPEED;
            _shader.Parameters["offset"].SetValue(_effectOffset);

            _duration++;
            if (_duration == TOTAL_DURATION)
            {
                Finish();
                Battle.ActiveBattle.AnimationController.SetScreenEffect();
            }
        }
    }
}
