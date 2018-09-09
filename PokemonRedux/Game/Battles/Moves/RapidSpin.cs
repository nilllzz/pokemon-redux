using PokemonRedux.Screens.Battles.Animations.Moves;

namespace PokemonRedux.Game.Battles.Moves
{
    [BattleMove("RAPID SPIN")]
    class RapidSpin : BattleMove
    {
        public override double EffectChance => 1f;

        public override void ShowAnimation(BattlePokemon user, BattlePokemon target)
        {
            var animation = new RapidSpinAnimation(target);
            Battle.ActiveBattle.AnimationController.ShowAnimationAndWait(animation);
        }

        public override bool ExecuteSecondaryEffect(BattlePokemon user, BattlePokemon target)
        {
            // TODO: remove binding moves
            // Bind, Clamp, Fire Spin, Leech Seed, Spikes, Whirlpool, Wrap
            // message: <name>\nshed <affliction>
            return true;
        }
    }
}
