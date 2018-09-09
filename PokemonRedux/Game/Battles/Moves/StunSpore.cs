using PokemonRedux.Game.Pokemons;
using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("STUN SPORE")]
    class StunSpore : BattleMove
    {
        public override double Accuracy => 0.75;
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new StunSporeAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.TryInflictStatusEffect(target, PokemonStatus.PAR);
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            return StatusMoveChecks.CheckPokemonStatus(target, PokemonStatus.PAR);
        }
    }
}
