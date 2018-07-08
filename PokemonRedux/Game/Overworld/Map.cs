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
        public World World { get; }
        public Vector3 Offset { get; private set; }
        public string MapFile { get; }
        public MapData Data { get; private set; }
        public string Name => Data.name;

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
            var mapDef = Controller.Content.LoadDirect<string>(MapFile);
            Data = JsonConvert.DeserializeObject<MapData>(mapDef);

            Offset = Data.WorldOffset;

            foreach (var entityData in Data.entities)
            {
                var entities = EntityLoader.InstantiateEntities(Data, entityData);
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
            ContentLoader.ClearBuffer(MapFile);
            World.ChangeMap(MapFile);
        }
    }
}
