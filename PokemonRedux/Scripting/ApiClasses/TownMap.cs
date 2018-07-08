using Kolben;
using Kolben.Adapters;
using Kolben.Types;
using PokemonRedux.Screens;
using PokemonRedux.Screens.TownMap;
using static Core;

namespace PokemonRedux.Scripting.ApiClasses
{
    [ApiClass("TownMap")]
    class TownMap : ApiClass
    {
        public static SObject show(ScriptProcessor processor, SObject[] parameters)
        {
            // TODO: get correct location from world screen
            var screenManager = GetComponent<ScreenManager>();
            var townMapScreen = new TownMapScreen(screenManager.ActiveScreen, "johto", "New Bark Town");
            townMapScreen.LoadContent();
            screenManager.SetScreen(townMapScreen);

            ScriptManager.WaitUntil(() => !(screenManager.ActiveScreen is TownMapScreen));

            return ScriptInAdapter.GetUndefined(processor);
        }
    }
}
