using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    [SerializeField] private BuildingPool buildingPool;
    [SerializeField] private Transform player;

    [Header("Spawn")]
    [SerializeField] private int initialRows = 15;
    [SerializeField] private float spawnAheadDistance = 300f;
    [SerializeField] private float despawnDistance = 20f;

    [Header("Ground")]
    [SerializeField] private float groundWidth = 300f;
    [SerializeField] private int laneCount = 6;

    [System.Serializable]
    private struct BuildingPreset
    {
        public float width;
        public float length;
        public float height;
    }

    [Header("Building Presets")]
    [SerializeField]
    private BuildingPreset[] presets =
    {
        new BuildingPreset { width = 12, length = 12, height = 20 },
        new BuildingPreset { width = 18, length = 18, height = 40 },
        new BuildingPreset { width = 10, length = 10, height = 80 },
        new BuildingPreset { width = 24, length = 16, height = 30 }
    };

    [Header("City Block")]
    [SerializeField] private int minRowsPerBlock = 2;
    [SerializeField] private int maxRowsPerBlock = 4;

    [SerializeField] private float rowSpacing = 12f;
    [SerializeField] private float blockSpacing = 30f;

    private readonly List<GameObject> activeBuildings = new();

    private float nextSpawnZ;

    private void Start()
    {
        nextSpawnZ = player.position.z + 20f;

        while (nextSpawnZ < player.position.z + spawnAheadDistance)
            SpawnBlock();
    }

    private void Update()
    {
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
            SpawnBlock();

        CheckBuildingsBehindPlayer();
    }

    private void SpawnBlock()
    {
        int rows = Random.Range(minRowsPerBlock, maxRowsPerBlock + 1);

        for (int i = 0; i < rows; i++)
        {
            SpawnRow();

            if (i < rows - 1)
                nextSpawnZ += rowSpacing;
        }

        nextSpawnZ += blockSpacing;
    }

    private void SpawnRow()
    {
        float laneWidth = groundWidth / laneCount;

        // Randomly decide how many buildings in this row
        bool[] occupied = new bool[laneCount];

        int buildingsInRow = Random.Range(2, laneCount - 1);

        int spawned = 0;

        List<int> availableLanes = new();

        for (int i = 0; i < laneCount; i++)
            availableLanes.Add(i);

        for (int i = 0; i < buildingsInRow && availableLanes.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableLanes.Count);
            int lane = availableLanes[randomIndex];

            occupied[lane] = true;
            availableLanes.RemoveAt(randomIndex);
        }

        float largestLength = 0f;

        for (int lane = 0; lane < laneCount; lane++)
        {
            if (occupied[lane])
                SpawnBuilding(lane, laneWidth, ref largestLength);
        }
    }

    private void SpawnBuilding(int lane, float laneWidth, ref float largestLength)
    {
        GameObject building = buildingPool.getBuilding();

        if (building == null)
            return;

        BuildingPreset preset = presets[Random.Range(0, presets.Length)];

        float width = preset.width;
        float length = preset.length;
        float height = preset.height;

        largestLength = Mathf.Max(largestLength, length);

        float x =
            -groundWidth / 2f +
            laneWidth / 2f +
            lane * laneWidth;

        building.transform.position = new Vector3(
            x,
            0f,
            nextSpawnZ);

        building.transform.localScale = new Vector3(
            width,
            height,
            length);

        activeBuildings.Add(building);
    }

    private void CheckBuildingsBehindPlayer()
    {
        for (int i = activeBuildings.Count - 1; i >= 0; i--)
        {
            if (activeBuildings[i].transform.position.z < player.position.z - despawnDistance)
            {
                buildingPool.returnBuildingToPool(activeBuildings[i]);
                activeBuildings.RemoveAt(i);
            }
        }
    }
}