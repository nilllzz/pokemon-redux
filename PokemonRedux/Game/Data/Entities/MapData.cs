using Microsoft.Xna.Framework;
using PokemonRedux.Game.Data;
using System.Linq;

namespace PokemonRedux.Game.Data.Entities
{
    class MapData
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string name;
        public EntityDefinitionData[] definitions;
        public EntityData[] entities;
        public float[] worldOffset;
        public string[] loadMaps;
#pragma warning restore 0649

        public Vector3 WorldOffset => DataHelper.GetVector3(worldOffset, 0f);

        public EntityDefinitionData GetDefinitionForId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            return definitions?.FirstOrDefault(d => d.id == id);
        }
    }
}
