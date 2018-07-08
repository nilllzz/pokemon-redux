using Microsoft.Xna.Framework;
using PokemonRedux.Game.Overworld.Entities;
using System.Collections.Generic;

namespace PokemonRedux.Game.Data.Entities
{
    class EntityLoader
    {
        public static Entity[] InstantiateEntities(MapData levelData, EntityData data)
        {
            var def = levelData.GetDefinitionForId(data.id);
            if (def != null)
            {
                data.ApplyDefinitionData(def);
            }

            switch (data.EntityType)
            {
                case EntityType.Prop:
                    return new[] { new Prop(data) };
                case EntityType.Floor:
                    return new[] { new Floor(data) };
                case EntityType.Door:
                    return new[] { new Door(data) };
                case EntityType.Script:
                    return new[] { new ScriptTrigger(data) };
                case EntityType.Field:
                    if (data.entity != null)
                    {
                        var fieldEnt = data.entity.Clone();
                        var fieldEntDef = levelData.GetDefinitionForId(fieldEnt.id);
                        if (fieldEntDef != null)
                        {
                            fieldEnt.ApplyDefinitionData(fieldEntDef);
                        }

                        var size = data.Size2;
                        var steps = data.Steps;
                        var initialPosition = data.Position;
                        fieldEnt.AddToPosition(initialPosition);

                        if (fieldEnt.isOpaque)
                        {
                            return new[] { new Prop(fieldEnt, size, steps) };
                        }
                        else
                        {
                            // if the entity is not opaque, create multiple rows instead to ensure sorting
                            var result = new List<Entity>();
                            for (int z = 0; z < size.Y; z++)
                            {
                                var rowSize = new Vector2(size.X, 1);
                                var rowEnt = fieldEnt.Clone();
                                rowEnt.AddToPosition(new Vector3(0, 0, z * steps.Y));
                                result.Add(new Prop(rowEnt, rowSize, steps));
                            }

                            return result.ToArray();
                        }
                    }
                    return null;
                case EntityType.Struct:
                    {
                        if (data.entities == null || data.entities.Length == 0 ||
                            data.positions == null || data.positions.Length == 0)
                        {
                            return null;
                        }

                        var result = new List<Entity>();

                        var structParts = data.entities;
                        foreach (var offset in data.Positions)
                        {
                            foreach (var structPartData in structParts)
                            {
                                var instanceData = structPartData.Clone();
                                instanceData.AddToPosition(offset);
                                var e = InstantiateEntities(levelData, instanceData);
                                result.AddRange(e);
                            }
                        }

                        return result.ToArray();
                    }
            }
            return null;
        }

    }
}
