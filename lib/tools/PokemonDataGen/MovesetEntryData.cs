using Newtonsoft.Json;

namespace PokemonDataGen
{
    class MovesetEntryData
    {
        public string name;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int level;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool tm;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool breeding;
    }
}
