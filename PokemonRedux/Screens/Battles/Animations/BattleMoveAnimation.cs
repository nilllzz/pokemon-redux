using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    abstract class BattleMoveAnimation : BattleAnimation
    {
        protected readonly BattlePokemon _user, _target;
        protected Texture2D _texture;

        public BattleMoveAnimation(BattlePokemon user, BattlePokemon target)
        {
            _user = user;
            _target = target;
        }

        protected void LoadTexture(string filename)
        {
            _texture = Controller.Content.LoadDirect<Texture2D>($"Textures/Battle/Animations/{filename}.png");
        }

        public override void Show()
        {
            // hide status of user
            var side = _user?.Side ?? BattlePokemon.ReverseSide(_target.Side);
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(side, false);
        }

        protected virtual void Finish()
        {
            IsFinished = true;
            // show status again
            var side = _user?.Side ?? BattlePokemon.ReverseSide(_target.Side);
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(side, true);
        }
    }
}
