public interface IDungeonGenerator
{
    FloorPlan GenerateFloor(FloorSettings settings, GameConfig config);
}