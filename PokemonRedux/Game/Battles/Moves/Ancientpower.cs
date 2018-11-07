using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("ANCIENTPOWER")]
    class Ancientpower : BattleMove
    {
        public override double EffectChance => 0.1;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new AncientpowerAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            var result = Battle.ActiveBattle.ChangeStat(user, PokemonStat.Attack, PokemonStatChange.Increase);
            result |= Battle.ActiveBattle.ChangeStat(user, PokemonStat.Defense, PokemonStatChange.Increase);
            result |= Battle.ActiveBattle.ChangeStat(user, PokemonStat.SpecialAttack, PokemonStatChange.Increase);
            result |= Battle.ActiveBattle.ChangeStat(user, PokemonStat.SpecialDefense, PokemonStatChange.Increase);
            result |= Battle.ActiveBattle.ChangeStat(user, PokemonStat.Speed, PokemonStatChange.Increase);
            return result;
        }
    }
}
