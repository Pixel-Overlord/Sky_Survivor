/// <summary>
/// Extension seam for anything that wants to add content per generated row
/// (coins, power-ups, moving obstacles, district props, ...). The spawner
/// calls every decorator it finds after it has placed a row's buildings,
/// handing over the row's geometry and the open-corridor bounds.
///
/// Implement this on a MonoBehaviour sitting on (or under) the spawner's
/// GameObject; the spawner auto-discovers it. This keeps BuildingSpawner
/// closed for modification but open for extension.
/// </summary>
public interface IRowDecorator
{
    void OnRowSpawned(in RowContext ctx);
}

/// <summary>Read-only description of a freshly spawned row.</summary>
public readonly struct RowContext
{
    public readonly float Z;
    public readonly LaneGrid Grid;
    public readonly int CorridorMinLane;
    public readonly int CorridorMaxLane;
    public readonly DifficultyParams Difficulty;

    public RowContext(float z, LaneGrid grid, int corridorMin, int corridorMax, DifficultyParams difficulty)
    {
        Z = z;
        Grid = grid;
        CorridorMinLane = corridorMin;
        CorridorMaxLane = corridorMax;
        Difficulty = difficulty;
    }

    public bool IsOpen(int lane) => lane >= CorridorMinLane && lane <= CorridorMaxLane;
}
