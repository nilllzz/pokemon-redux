namespace PokemonRedux.Game.Data.Entities
{
    enum GeometryType
    {
        Cube,
        Plane, // flat rectangle, horizontal, for dynamic heights through elevation
        Billboard, // similar to wall, but can be slanted and is in the center of the entity
        Walls, // similar to a cube but only with the 4 cardinal sides
        Wall, // single wall facing south by default
        Ramp, // ramp up with triangle vertices at the sides
        Corner, // 4th of a pyramid, corner element
        HouseInside, // left, right, bottom, back walls to act as a quick object for house insides when opening a door
        Tube, // cylinder without top/bottom vertices
        Cylinder
    }
}
