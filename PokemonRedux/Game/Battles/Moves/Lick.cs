using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("LICK")]
    class Lick : BattleMove
    {
        public override double EffectChance => 0.3;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new LickAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.TryInflictStatusEffect(target, PokemonStatus.PAR);
        }
    }
}
