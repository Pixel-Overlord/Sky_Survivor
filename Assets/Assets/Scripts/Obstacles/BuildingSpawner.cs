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

        for (int i = 0; i < initialRows; i++)
            SpawnRow();
    }

    private void Update()
    {
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
            SpawnRow();

        CheckBuildingsBehindPlayer();
    }

    private void SpawnRow()
    {
        float laneWidth = groundWidth / laneCount;

        // Randomly decide how many buildings in this row
        bool[] occupied = new bool[laneCount];

        int buildingsInRow = Random.Range(2, laneCount);

        int spawned = 0;

        while (spawned < buildingsInRow)
        {
            int lane = Random.Range(0, laneCount);

            if (occupied[lane])
                continue;

            // Prevent 3 consecutive buildings
            bool left1 = lane > 0 && occupied[lane - 1];
            bool left2 = lane > 1 && occupied[lane - 2];
            bool right1 = lane < laneCount - 1 && occupied[lane + 1];
            bool right2 = lane < laneCount - 2 && occupied[lane + 2];

            if ((left1 && left2) || (right1 && right2))
                continue;

            occupied[lane] = true;
            spawned++;
        }

        float largestLength = 0f;

        for (int lane = 0; lane < laneCount; lane++)
        {
            if (occupied[lane])
                SpawnBuilding(lane, laneWidth, ref largestLength);
        }

        nextSpawnZ += largestLength + Random.Range(10f, 18f);
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