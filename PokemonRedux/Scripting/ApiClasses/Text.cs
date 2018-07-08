using Kolben;
using Kolben.Adapters;
using Kolben.Types;
using PokemonRedux.Screens;
using PokemonRedux.Screens.Overworld;
using static Core;

namespace PokemonRedux.Scripting.ApiClasses
{
    [ApiClass("Text")]
    sealed class Text : ApiClass
    {
        public static SObject show(ScriptProcessor processor, SObject[] parameters)
        {
            if (EnsureTypeContract(parameters, new[] { typeof(string) }, out var netObjects))
            {
                var paramHelper = new ParamHelper(netObjects);
                var message = paramHelper.Pop<string>();

                var screen = GetComponent<ScreenManager>().ActiveScreen;
                if (screen is WorldScreen wScreen)
                {
                    wScreen.ShowTextbox(message, false);
                    ScriptManager.WaitUntil(() => !wScreen.Textbox.Visible);
                }
            }

            return ScriptInAdapter.GetUndefined(processor);
        }
    }
}
