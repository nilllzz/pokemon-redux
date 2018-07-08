using GameDevCommon.Rendering.Composers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Data.Entities;
using static Core;

namespace PokemonRedux.Game.Overworld.Entities
{
    class ScriptTrigger : Entity
    {
        private ScriptTriggerType _triggerType;

        public ScriptTrigger(EntityData data)
            : base(data)
        {
            IsVisible = false;
            IsVisualObject = false;
            Size = _data.Size3;

            // generate offset to match position of props
            var offset = new Vector3(((Size.X * 1) - 1f) / 2f, (Size.Y - 1f) / 2f, ((Size.Z * 1) - 1f) / 2f);
            _data.AddToPosition(offset);
            Position = _data.Position;

            _triggerType = _data.script.TriggerType;
        }

        public override void LoadTexture()
        {
            var colors = new[] { Color.Red };
            var tex = new Texture2D(Controller.GraphicsDevice, 1, 1);
            tex.SetData(colors);
            Texture = tex;
        }

        public override void LoadContent()
        {
            LoadTexture();
            base.LoadContent();
        }

        protected override void CreateGeometry()
        {
            Geometry.AddVertices(CuboidComposer.Create(Size.X, Size.Y, Size.Z));
        }

        public override bool DoesCollide(CollisionType collisionType, Vector3 position, Vector3 size, Entity other)
        {
            if (collisionType == CollisionType.Test)
            {
                return base.DoesCollide(collisionType, position, size, other);
            }
            else
            {
                if (_triggerType == ScriptTriggerType.Collide && base.DoesCollide(collisionType, position, size, other))
                {
                    // run script
                    Map.World.StartScript(_data.script.file);
                }
                return false;
            }
        }

        public override void Interact()
        {
            if (_triggerType == ScriptTriggerType.Interact)
            {
                // run script
                Map.World.StartScript(_data.script.file);
            }
        }
    }
}
