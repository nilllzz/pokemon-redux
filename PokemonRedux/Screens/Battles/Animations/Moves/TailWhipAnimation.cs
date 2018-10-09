using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class TailWhipAnimation : BattleMoveAnimation
    {
        private const int TOTAL_MOVES = 4;
        private const float PROGRESS_PER_FRAME = 0.02f;

        private float _progress = 0f;

        public TailWhipAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent() { }
        public override void Draw(SpriteBatch batch) { }

        public override void Update()
        {
            _progress += PROGRESS_PER_FRAME;
            if (_progress >= 1f)
            {
                _progress = 1f;
                Finish();
            }

            var offsetX = (float)Math.Sin(MathHelper.Pi * _progress * TOTAL_MOVES) * Border.SCALE * Border.UNIT;
            if (_user.Side == PokemonSide.Player)
            {
                offsetX *= -1f;
            }
            Battle.ActiveBattle.AnimationController.SetPokemonOffset(_user.Side, new Vector2(offsetX, 0));
        }
    }
}
