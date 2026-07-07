using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    [SerializeField] private BuildingPool buildingPool;
    [SerializeField] private Transform player;

    [Header("Spawn")]
    [SerializeField] private int initialBuildings = 30;
    [SerializeField] private float spawnAheadDistance = 100f;
    [SerializeField] private float despawnDistance = 20f;

    [Header("Building")]
    [SerializeField] private float minX = -40f;
    [SerializeField] private float maxX = 40f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 25f;

    private readonly List<GameObject> activeBuildings = new();

    private float nextSpawnZ;

    private void Start()
    {
        nextSpawnZ = player.position.z + 20f;

        for (int i = 0; i < initialBuildings; i++)
            SpawnBuilding();
    }

    private void Update()
    {
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
            SpawnBuilding();

        CheckBuildingsBehindPlayer();
    }

    /*
     * Take a building object from pool.
     * Takes a random height and random scale.x and scale.z.
     * Add it to active buildings.
     * 
     * checks when to spawn next set of buildings.     * 
     */
    private void SpawnBuilding()
    {
        GameObject building = buildingPool.getBuilding();

        if (building == null)
            return;

        float height = Random.Range(minHeight, maxHeight);

        building.transform.position = new Vector3(
            Random.Range(minX, maxX),
            height * 0.5f,
            nextSpawnZ);

        building.transform.localScale = new Vector3(
            Random.Range(2f, 8f),
            height,
            Random.Range(2f, 8f));

        activeBuildings.Add(building);

        nextSpawnZ += Random.Range(8f, 15f);
    }

    private void CheckBuildingsBehindPlayer()
    {
        for (int i = activeBuildings.Count - 1; i >= 0; i--)
        {
            if (activeBuildings[i].transform.position.z < player.position.z - despawnDistance)
            {
                // move building back to pool
                buildingPool.returnBuildingToPool(activeBuildings[i]);
                activeBuildings.RemoveAt(i);
            }
        }
    }
}