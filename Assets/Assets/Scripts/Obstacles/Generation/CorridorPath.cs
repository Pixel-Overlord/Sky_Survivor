using UnityEngine;

/// <summary>
/// A random-walking "safe corridor" through the lane grid.
///
/// Each Step() drifts the corridor centre by at most one lane (-1, 0 or +1).
/// Because the drift per row is bounded by a single lane, the open region of
/// consecutive rows is always adjacent or overlapping — which is exactly what
/// makes the level solvable. Because the drift is random, the corridor wanders
/// and any fixed X the player parks on will eventually fall outside it,
/// forcing the player to steer.
/// </summary>
public class CorridorPath
{
    private readonly LaneGrid grid;
    private int centreLane;

    public int MinOpenLane { get; private set; }
    public int MaxOpenLane { get; private set; }

    public CorridorPath(LaneGrid grid, int startCentreLane)
    {
        this.grid = grid;
        centreLane = grid.ClampLane(startCentreLane);
    }

    /// <summary>Advance one row. <paramref name="width"/> = number of open lanes.</summary>
    public void Step(int width)
    {
        width = Mathf.Clamp(width, 1, grid.LaneCount);
        centreLane += Random.Range(-1, 2); // -1, 0 or +1  → drift ≤ 1 lane
        ClampCentreForWidth(width);
        RecomputeBounds(width);
    }

    public bool IsOpen(int lane) => lane >= MinOpenLane && lane <= MaxOpenLane;

    // Keep the whole corridor inside the field so it never clips the edge.
    private void ClampCentreForWidth(int width)
    {
        int halfLow = (width - 1) / 2;
        int halfHigh = width / 2;
        centreLane = Mathf.Clamp(centreLane, halfLow, grid.LaneCount - 1 - halfHigh);
    }

    private void RecomputeBounds(int width)
    {
        MinOpenLane = centreLane - (width - 1) / 2;
        MaxOpenLane = MinOpenLane + width - 1;
    }
}
