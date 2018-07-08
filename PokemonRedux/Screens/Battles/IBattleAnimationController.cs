using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using PokemonRedux.Screens.Battles.Animations;

namespace PokemonRedux.Screens.Battles
{
    interface IBattleAnimationController
    {
        void ShowAnimationAndWait(BattleAnimation animation, int delay = 0);
        void ShowAnimation(BattleAnimation animation, int delay = 0);
        void WaitForAnimations();
        void SetPokemonVisibility(PokemonSide side, bool visible);
        void SetPokemonOffset(PokemonSide side, Vector2 offset);
        void SetScreenOffset(Vector2 offset); // screen shaking etc.
        void SetScreenColorInvert(bool invert);
        void SetPokemonSize(PokemonSide side, float size);
        void SetPokemonColor(PokemonSide side, Color color);
        void SetScreenEffect(Effect effect = null);
        Color[] SetPokemonPalette(PokemonSide side, Color[] palette); // returns previous palette
    }
}
