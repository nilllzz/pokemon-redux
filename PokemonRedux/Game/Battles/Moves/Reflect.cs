using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("REFLECT")]
    class Reflect : BattleMove
    {
        public override MoveCategory GetCategory(BattlePokemon user) => MoveCategory.Status;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new ReflectAnimation(user, target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool StatusMoveCheck(BattlePokemon user, BattlePokemon target)
        {
            if (user.ReflectTurns > 0)
            {
                Battle.ActiveBattle.UI.ShowMessageAndWait("But it failed!");
                return false;
            }
            return true;
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            Battle.ActiveBattle.UI.ShowMessageAndWait(user.GetDisplayName() + "^'s\nDEFENSE rose!");
            user.ReflectTurns = 5;
            return true;
        }
    }
}
