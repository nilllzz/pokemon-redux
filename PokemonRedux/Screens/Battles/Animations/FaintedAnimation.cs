using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using PokemonRedux.Game.Pokemons;

namespace PokemonRedux.Screens.Battles.Animations
{
    // animation that lets the pokemon slip into the ground after they fainted
    class FaintedAnimation : BattleAnimation
    {
        private const float ANIMATION_SPEED = 0.04f;

        private readonly BattlePokemon _target;

        private float _y = 0;

        public FaintedAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        { }

        public override void Draw(SpriteBatch batch)
        { }

        public override void Update()
        {
            Battle.ActiveBattle.AnimationController.SetPokemonOffset(_target.Side, new Vector2(0, _y * PokemonTextureManager.TEXTURE_SIZE));
            _y += ANIMATION_SPEED;
            if (_y >= 1f)
            {
                _y = 1f;
                IsFinished = true;
            }
        }
    }
}
