namespace PokemonRedux.Game.Overworld.Entities
{
    enum CollisionType
    {
        Test, // used to test if an object exists somewhere to interact with it
        Walk, // test if walking in a spot collides with an entity
        Grass // tests if the player collides with grass, ignores hasCollision prop
    }
}
