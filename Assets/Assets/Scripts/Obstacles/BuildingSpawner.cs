using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurally builds an endless city as a sequence of rows.
///
/// Instead of scattering buildings randomly, a random-walking CorridorPath
/// carves a safe flying corridor that drifts left/right at most one lane per
/// row. Every lane NOT inside the corridor is walled with a pooled building.
/// Because the corridor wanders, the player cannot survive by parking in one
/// lane; because it only drifts one lane per row (and rows are spaced so the
/// player can always cross a lane in time), the level is always solvable.
///
/// Responsibilities are split out on purpose:
///   - LaneGrid       : lane ↔ world-X math
///   - DifficultyCurve: how the knobs scale with distance
///   - CorridorPath   : where the safe path is this row
///   - IRowDecorator  : optional per-row content (coins, power-ups, ...)
/// This class only orchestrates them and manages the object pool.
/// </summary>
public class BuildingSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BuildingPool buildingPool;
    [SerializeField] private Transform player;

    [Header("Lane grid")]
    [SerializeField] private int laneCount = 9;
    [SerializeField] private float laneWidth = 8f;
    [Tooltip("Fraction of a lane the building fills (leaves a small visual gap).")]
    [Range(0.5f, 1f)][SerializeField] private float laneFill = 0.95f;
    [SerializeField] private float buildingDepth = 6f;

    [Header("Streaming")]
    [SerializeField] private float spawnAheadDistance = 140f;
    [SerializeField] private float despawnBehindDistance = 25f;

    [Header("Difficulty")]
    [SerializeField] private DifficultyCurve difficulty = new DifficultyCurve();

    [Header("Solvability guard")]
    [Tooltip("Match PlaneController.forwardSpeed.")]
    [SerializeField] private float playerForwardSpeed = 20f;
    [Tooltip("Match PlaneController.movementSpeed (horizontal).")]
    [SerializeField] private float playerHorizontalSpeed = 10f;
    [Tooltip("Extra headroom on the minimum solvable row spacing (>1).")]
    [SerializeField] private float reachabilitySafety = 1.25f;

    private LaneGrid grid;
    private CorridorPath corridor;
    private IRowDecorator[] decorators;

    private readonly List<GameObject> activeBuildings = new();

    private float startZ;
    private float nextRowZ;
    private int rowsInCurrentBlock;

    private void Awake()
    {
        grid = new LaneGrid(laneCount, laneWidth);
        corridor = new CorridorPath(grid, laneCount / 2);
        decorators = GetComponentsInChildren<IRowDecorator>();
    }

    private void Start()
    {
        startZ = player.position.z;
        nextRowZ = startZ + 20f;
        PrewarmAhead();
    }

    private void Update()
    {
        PrewarmAhead();
        RecycleBehindPlayer();
    }

    // --- Streaming --------------------------------------------------------

    private void PrewarmAhead()
    {
        while (nextRowZ < player.position.z + spawnAheadDistance)
            SpawnNextRow();
    }

    private void SpawnNextRow()
    {
        float distance = nextRowZ - startZ;
        DifficultyParams d = difficulty.Evaluate(distance);

        corridor.Step(d.corridorWidthLanes);

        if (rowsInCurrentBlock >= d.rowsPerBlock)
        {
            StartNewBlock(d); // insert an open cross-street, no buildings
            return;
        }

        PlaceWallRow(nextRowZ, d);
        NotifyDecorators(nextRowZ, d);

        rowsInCurrentBlock++;
        nextRowZ += SolvableRowSpacing(d);
    }

    private void StartNewBlock(DifficultyParams d)
    {
        rowsInCurrentBlock = 0;
        nextRowZ += d.blockSpacing;
    }

    // --- Row population ---------------------------------------------------

    // Wall off every lane outside the corridor. The corridor lanes are left
    // open, which is what guarantees a continuous, solvable path.
    private void PlaceWallRow(float z, DifficultyParams d)
    {
        for (int lane = 0; lane < grid.LaneCount; lane++)
        {
            if (corridor.IsOpen(lane))
                continue;

            if (Random.value > d.wallFillChance)
                continue;

            SpawnBuilding(lane, z, d);
        }
    }

    private void SpawnBuilding(int lane, float z, DifficultyParams d)
    {
        GameObject building = buildingPool.getBuilding();
        if (building == null)
            return;

        float height = Random.Range(d.minBuildingHeight, d.maxBuildingHeight);

        building.transform.position = new Vector3(grid.LaneToWorldX(lane), 0f, z);
        building.transform.localScale = new Vector3(
            grid.LaneWidth * laneFill,
            height,
            buildingDepth);

        activeBuildings.Add(building);
    }

    private void NotifyDecorators(float z, DifficultyParams d)
    {
        if (decorators.Length == 0)
            return;

        var ctx = new RowContext(z, grid, corridor.MinOpenLane, corridor.MaxOpenLane, d);
        for (int i = 0; i < decorators.Length; i++)
            decorators[i].OnRowSpawned(ctx);
    }

    // --- Recycling --------------------------------------------------------

    private void RecycleBehindPlayer()
    {
        float cutoff = player.position.z - despawnBehindDistance;
        for (int i = activeBuildings.Count - 1; i >= 0; i--)
        {
            if (activeBuildings[i].transform.position.z < cutoff)
            {
                buildingPool.returnBuildingToPool(activeBuildings[i]);
                activeBuildings.RemoveAt(i);
            }
        }
    }

    // --- Solvability ------------------------------------------------------

    // The player can only cross one lane per row if the row's Z advance gives
    // them enough time to travel one lane width sideways. This clamps the
    // difficulty-driven spacing so a one-lane drift is always reachable.
    private float SolvableRowSpacing(DifficultyParams d)
    {
        float speedRatio = playerForwardSpeed / Mathf.Max(0.01f, playerHorizontalSpeed);
        float minSpacing = grid.LaneWidth * speedRatio * reachabilitySafety;
        return Mathf.Max(d.rowSpacing, minSpacing);
    }
}
