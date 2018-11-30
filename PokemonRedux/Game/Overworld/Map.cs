using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using PokemonRedux.Content;
using PokemonRedux.Game.Data.Entities;
using PokemonRedux.Game.Overworld.Entities;
using static Core;

namespace PokemonRedux.Game.Overworld
{
    class Map
    {
        private MapData _data;

        public World World { get; }
        public Vector3 Offset { get; private set; }
        public string MapFile { get; }
        public string Name => _data.name;
        public string[] LoadMaps => _data.loadMaps;
        public string EncounterData => _data.encounterData;

        public Map(World world, string mapFile)
        {
            MapFile = mapFile.ToLower();
            World = world;
        }

        public void LoadContent()
        {
            LoadMap();
        }

        private void LoadMap()
        {
            var mapDef = Controller.Content.LoadDirect<string>($"Data/Maps/{MapFile}");
            _data = JsonConvert.DeserializeObject<MapData>(mapDef);

            Offset = _data.WorldOffset;

            foreach (var entityData in _data.entities)
            {
                var entities = EntityLoader.InstantiateEntities(_data, entityData);
                if (entities != null)
                {
                    foreach (var entity in entities)
                    {
                        AddEntity(entity);
                    }
                }
            }
        }

        public void AddEntity(Entity entity)
        {
            entity.Map = this;
            if (!(entity is PlayerCharacter))
            {
                entity.Position += Offset;
            }

            entity.LoadContent();

            World.AddEntity(entity);
        }

        public void Reload()
        {
            World.DisposeMap(this);
            ContentLoader.CopyContent();
            ContentLoader.ClearBuffer($"Data/Maps/{MapFile}");
            World.ChangeMap(MapFile);
        }
    }
}
