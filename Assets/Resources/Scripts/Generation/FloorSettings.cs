public class FloorSettings
{
    public int floorIndex;                            // Текущий слой (0..2)
    public int targetRoomsCount;                      // Целевое количество комнат
    public float difficultyMultiplier;                // Общий множитель сложности (от игрока)
    public DungeonModifiers mlModifiers;              // Модификаторы от ML-агента
    public int seed;                                  // Seed для детерминизма (опционально)
}