using GameDevCommon.Drawing;
using GameDevCommon.Rendering;
using GameDevCommon.Rendering.Composers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Data.Entities;
using static Core;

namespace PokemonRedux.Game.Overworld.Entities
{
    class Character : Entity
    {
        private const int SPRITES_X = 3, SPRITES_Y = 4;
        private const int WALK_CYCLE_DELAY = 10;
        private static int[] WALK_CYCLE_FRAMES = new[] { 0, 1, 0, 2 };

        private Texture2D[,] _sprites;
        protected bool _walking = false;
        protected int _walkCycleFrame = 0;
        private int _walkCycleDelay = WALK_CYCLE_DELAY;

        public EntityFacing Facing { get; set; } = EntityFacing.South;
        public override bool IsStatic => false;

        public Character(EntityData data)
            : base(data)
        {
            IsOpaque = false;
            Rotation.X = -0.7f;
        }

        public override void LoadTexture()
        {
            // split texture into 3x4 sprites
            _sprites = new Texture2D[SPRITES_X, SPRITES_Y];
            var texture = Controller.Content.LoadDirect<Texture2D>("Textures/World/Characters/" + _data.textureFile + ".png");
            var spriteSize = new Point(texture.Width / SPRITES_X, texture.Height / SPRITES_Y);

            for (var y = 0; y < SPRITES_Y; y++)
            {
                for (var x = 0; x < SPRITES_X; x++)
                {
                    var spriteData = texture.GetData(new Rectangle(x * spriteSize.X, y * spriteSize.Y, spriteSize.X, spriteSize.Y));
                    var sprite = new Texture2D(Controller.GraphicsDevice, spriteSize.X, spriteSize.Y);
                    sprite.SetData(spriteData);

                    _sprites[x, y] = sprite;
                }
            }
        }

        public override void LoadContent()
        {
            LoadTexture();
            Texture = _sprites[0, 2];
            base.LoadContent();
        }

        protected override void CreateGeometry()
        {
            var vertices = RectangleComposer.Create(1f, 1f);
            VertexTransformer.Rotate(vertices, new Vector3(MathHelper.PiOver2, 0, 0));
            Geometry.AddVertices(vertices);
        }

        public override void Update()
        {
            if (_walking)
            {
                _walkCycleDelay--;
                if (_walkCycleDelay == 0)
                {
                    _walkCycleDelay = WALK_CYCLE_DELAY;
                    _walkCycleFrame++;
                    if (_walkCycleFrame == WALK_CYCLE_FRAMES.Length)
                    {
                        _walkCycleFrame = 0;
                    }
                }
            }

            Texture = _sprites[WALK_CYCLE_FRAMES[_walking ? _walkCycleFrame : 0], (int)Facing];
            base.Update();
        }
    }
}
