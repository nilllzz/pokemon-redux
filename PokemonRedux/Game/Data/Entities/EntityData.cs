using Microsoft.Xna.Framework;
using System;

namespace PokemonRedux.Game.Data.Entities
{
    class EntityData : CommonData, ICloneable
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public float[] position; // vec3 position
        public float[] rotation; // rotation of the prop, default 0-3 (north, west, south, east)

        // field
        public EntityData entity;
        public float[] steps; // steps at which the field places the entities
        public bool centered = false; // to center field, or start from the top left

        // struct
        public EntityData[] entities;
        public float[][] positions;

        // door
        public DoorData doorData;

        // script trigger
        public ScriptData script;
#pragma warning restore 0649

        public EntityData Clone()
        {
            var obj = (EntityData)MemberwiseClone();
            obj.position = (float[])position?.Clone();
            obj.rotation = (float[])rotation?.Clone();
            obj.textures = (int[][])textures?.Clone();
            obj.size = (float[])size?.Clone();
            obj.elevation = (float[])elevation?.Clone();
            obj.entities = (EntityData[])entities?.Clone();
            obj.positions = (float[][])positions?.Clone();
            obj.steps = (float[])steps?.Clone();
            obj.frames = (int[][])frames?.Clone();
            obj.doorData = (DoorData)doorData?.Clone();
            obj.script = (ScriptData)script?.Clone();
            return obj;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public void ApplyDefinitionData(EntityDefinitionData data)
        {
            if (string.IsNullOrWhiteSpace(type))
                type = data.type;
            if (string.IsNullOrWhiteSpace(textureFile))
                textureFile = data.textureFile;
            if (string.IsNullOrWhiteSpace(geometry))
                geometry = data.geometry;
            if (textures == null)
                textures = data.textures;
            if (size == null)
                size = data.size;
            if (elevation == null)
                elevation = data.elevation;
            if (frames == null)
                frames = data.frames;
            visible = data.visible;
            isStatic = data.isStatic;
            isOpaque = data.isOpaque;
            hasCollision = data.hasCollision;
            tileTexture = data.tileTexture;
            billboardTilt = data.billboardTilt;
            isAnimated = data.isAnimated;
            frameDelay = data.frameDelay;
            tubeResolution = data.tubeResolution;
            wrapTexture = data.wrapTexture;
        }

        public Rectangle[] Textures => DataHelper.GetRectangles(textures);
        public Vector2 Size2 => DataHelper.GetVector2(size, 1f);
        public Vector3 Size3 => DataHelper.GetVector3(size, 1f);
        public Vector3 Position => DataHelper.GetVector3(position, 0f);
        public Vector3 Rotation => DataHelper.GetVector3(rotation, 0f);
        public Vector3[] Positions => DataHelper.GetVector3Arr(positions, 0f);
        public EntityType EntityType => DataHelper.ParseEnum<EntityType>(type);
        public GeometryType GeometryType => DataHelper.ParseEnum<GeometryType>(geometry);
        public Vector2 Steps => DataHelper.GetVector2(steps, 1f);
        public Rectangle[] Frames => DataHelper.GetRectangles(frames);

        public void AddToPosition(Vector3 amount)
        {
            if (position == null)
            {
                position = new float[0];
            }
            switch (position.Length)
            {
                case 0:
                    // do nothing
                    break;
                case 1:
                    amount += new Vector3(position[0]);
                    break;
                case 2:
                    amount += new Vector3(position[0], 0, position[1]);
                    break;
                default: // >= 3
                    amount += new Vector3(position[0], position[1], position[2]);
                    break;
            }
            position = new[] { amount.X, amount.Y, amount.Z };
        }
    }
}
