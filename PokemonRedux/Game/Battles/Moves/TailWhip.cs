﻿using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("TAIL WHIP")]
    class TailWhip : BattleMove
    {
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new TailWhipAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            return Battle.ActiveBattle.ChangeStat(target, PokemonStat.Defense, PokemonStatChange.Decrease);
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            return MoveHelper.CheckStatChange(target, PokemonStat.Defense, PokemonStatChange.Decrease);
        }
    }
}
