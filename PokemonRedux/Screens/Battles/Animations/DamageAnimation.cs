using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    // animation that flickers pokemon after it received damage
    class DamageAnimation : BattleAnimation
    {
        private const int FLICKER_DELAY_PLAYER = 3;
        private const int FLICKER_DELAY_ENEMY = 6;
        private const int FLICKER_AMOUNT_PLAYER = 10;
        private const int FLICKER_AMOUNT_ENEMY = 6;

        private readonly BattlePokemon _target;

        private int _flickerAmount = 0;
        private int _flickerDelay;

        public DamageAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            if (_target.Side == PokemonSide.Enemy)
            {
                _flickerDelay = FLICKER_DELAY_ENEMY;
            }
            else
            {
                _flickerDelay = FLICKER_DELAY_PLAYER;
            }
        }

        public override void Draw(SpriteBatch batch)
        { }

        public override void Update()
        {
            _flickerDelay--;
            if (_flickerDelay == 0)
            {
                if (_target.Side == PokemonSide.Enemy)
                {
                    _flickerDelay = FLICKER_DELAY_ENEMY;
                }
                else
                {
                    _flickerDelay = FLICKER_DELAY_PLAYER;
                }
                _flickerAmount++;
                if (_flickerAmount == (_target.Side == PokemonSide.Enemy ? FLICKER_AMOUNT_ENEMY : FLICKER_AMOUNT_PLAYER))
                {
                    IsFinished = true;
                    // reset visibility after animation
                    if (_target.Side == PokemonSide.Enemy)
                    {
                        Battle.ActiveBattle.AnimationController.SetPokemonVisibility(_target.Side, true);
                    }
                    else
                    {
                        Battle.ActiveBattle.AnimationController.SetScreenOffset(Vector2.Zero);
                    }
                }
                else
                {
                    if (_target.Side == PokemonSide.Enemy)
                    {
                        Battle.ActiveBattle.AnimationController.SetPokemonVisibility(_target.Side, _flickerAmount % 2 == 0);
                    }
                    else
                    {
                        float yOffset;
                        if (_flickerAmount % 2 == 0)
                        {
                            yOffset = -Border.SCALE;
                        }
                        else
                        {
                            yOffset = Border.SCALE;
                        }
                        Battle.ActiveBattle.AnimationController.SetScreenOffset(new Vector2(0, yOffset));
                    }
                }
            }
        }
    }
}
