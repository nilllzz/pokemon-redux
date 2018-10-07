using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("ROCK SMASH")]
    class RockSmash : BattleMove
    {
        public override double EffectChance => 0.5;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new RockSmashAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.ChangeStat(target, PokemonStat.Defense, PokemonStatChange.Decrease);
        }
    }
}
