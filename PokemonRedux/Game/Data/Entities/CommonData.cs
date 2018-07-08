namespace PokemonRedux.Game.Data.Entities
{
    // contains data fields shared between defintions and entities
    abstract class CommonData
    {
        // disable warning for non-assignment for json properties
#pragma warning disable 0649
        public string id;

        public string type;
        public string textureFile; // texture source file
        public bool visible = true;

        // prop
        public string geometry; // type of prop geometry
        public int[][] textures; // list of texture rectangles
        public float[] size; // vec2/3 for size
        public float[] elevation; // for supported geometry, the elevations of certain points within the geometry
        public bool isStatic = true;
        public bool isOpaque = true;
        public bool hasCollision = true;
        public bool tileTexture = false;

        // billboard
        public float billboardTilt = 0.87079637f;

        // tube/cylinder geometry
        public bool wrapTexture = false; // wraps texture around the tube instead of applying it to each element
        public int tubeResolution = 8; // amount of elements in the tube of a Tube/Cylinder geometry

        // animation
        public int frameDelay = 0;
        public bool isAnimated = false;
        public int[][] frames;
#pragma warning restore 0649
    }
}
