using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("ACID")]
    class Acid : BattleMove
    {
        public override double EffectChance => 0.1;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new AcidAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.ChangeStat(target, PokemonStat.Defense, PokemonStatChange.Decrease);
        }
    }
}
