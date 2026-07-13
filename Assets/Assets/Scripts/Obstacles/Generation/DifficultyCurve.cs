using UnityEngine;

/// <summary>
/// The tunable knobs for one row of city. All generation decisions read
/// from this struct, so difficulty scaling lives in exactly one place.
/// </summary>
[System.Serializable]
public struct DifficultyParams
{
    public int corridorWidthLanes; // how many lanes the safe path spans
    public float rowSpacing;       // Z gap between rows inside a block
    public float blockSpacing;     // Z gap of the open "cross-street" between blocks
    public int rowsPerBlock;       // rows before an open block gap
    [Range(0f, 1f)] public float wallFillChance; // chance a closed lane actually gets a building
    public float minBuildingHeight;
    public float maxBuildingHeight;
}

/// <summary>
/// Interpolates generation parameters from "easy" to "hard" based on how
/// far the player has travelled. Exposed to the Inspector so designers can
/// retune the whole difficulty ramp without touching code.
/// </summary>
[System.Serializable]
public class DifficultyCurve
{
    [Tooltip("Distance (Z) over which difficulty ramps from easy to hard.")]
    [SerializeField] private float rampDistance = 2500f;

    [Header("Easy — start of run")]
    [SerializeField]
    private DifficultyParams easy = new DifficultyParams
    {
        corridorWidthLanes = 3,
        rowSpacing = 24f,
        blockSpacing = 45f,
        rowsPerBlock = 3,
        wallFillChance = 1f,
        minBuildingHeight = 12f,
        maxBuildingHeight = 28f,
    };

    [Header("Hard — after full ramp")]
    [SerializeField]
    private DifficultyParams hard = new DifficultyParams
    {
        corridorWidthLanes = 2,
        rowSpacing = 20f,
        blockSpacing = 30f,
        rowsPerBlock = 4,
        wallFillChance = 1f,
        minBuildingHeight = 16f,
        maxBuildingHeight = 34f,
    };

    public DifficultyParams Evaluate(float distanceTravelled)
    {
        float t = Mathf.Clamp01(distanceTravelled / Mathf.Max(1f, rampDistance));
        return Lerp(easy, hard, t);
    }

    private static DifficultyParams Lerp(DifficultyParams a, DifficultyParams b, float t)
    {
        return new DifficultyParams
        {
            corridorWidthLanes = Mathf.RoundToInt(Mathf.Lerp(a.corridorWidthLanes, b.corridorWidthLanes, t)),
            rowSpacing = Mathf.Lerp(a.rowSpacing, b.rowSpacing, t),
            blockSpacing = Mathf.Lerp(a.blockSpacing, b.blockSpacing, t),
            rowsPerBlock = Mathf.RoundToInt(Mathf.Lerp(a.rowsPerBlock, b.rowsPerBlock, t)),
            wallFillChance = Mathf.Lerp(a.wallFillChance, b.wallFillChance, t),
            minBuildingHeight = Mathf.Lerp(a.minBuildingHeight, b.minBuildingHeight, t),
            maxBuildingHeight = Mathf.Lerp(a.maxBuildingHeight, b.maxBuildingHeight, t),
        };
    }
}
