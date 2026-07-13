using UnityEngine;

/// <summary>
/// Pure math helper that maps discrete lane indices to world-space X.
/// Lanes are symmetric around x = 0, so lane 0 is left-most and
/// lane (LaneCount - 1) is right-most.
/// </summary>
public class LaneGrid
{
    public int LaneCount { get; }
    public float LaneWidth { get; }

    public LaneGrid(int laneCount, float laneWidth)
    {
        LaneCount = Mathf.Max(1, laneCount);
        LaneWidth = Mathf.Max(0.01f, laneWidth);
    }

    public float FieldWidth => LaneCount * LaneWidth;

    /// <summary>World-space X of the centre of a lane.</summary>
    public float LaneToWorldX(int lane)
    {
        float leftEdge = -FieldWidth * 0.5f;
        return leftEdge + (lane + 0.5f) * LaneWidth;
    }

    public int ClampLane(int lane) => Mathf.Clamp(lane, 0, LaneCount - 1);
}
