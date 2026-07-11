using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns the reusable building instances used by the streamed city.</summary>
public class BuildingPool : MonoBehaviour
{
    [SerializeField]
    private GameObject buildingPrefab;

    [SerializeField]
    private int poolSize = 50;

    [SerializeField] private bool expandWhenEmpty = true;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // Initialize pool
        for (int i = 0; i < poolSize; i++)
        {
            pool.Enqueue(CreateBuilding());
        }
    }

    /// <summary>Returns an active building instance, expanding the pool when the configured policy permits it.</summary>
    public GameObject GetBuilding()
    {
        if (pool.Count == 0)
        {
            if (!expandWhenEmpty)
                return null;

            GameObject newBuilding = CreateBuilding();
            newBuilding.SetActive(true);
            return newBuilding;
        }

        GameObject building = pool.Dequeue();
        building.SetActive(true);

        return building;
    }

    /// <summary>Deactivates a building and makes it available for another city chunk.</summary>
    public void ReturnBuildingToPool(GameObject building)
    {
        building.SetActive(false);
        pool.Enqueue(building);
    }

    private GameObject CreateBuilding()
    {
        GameObject building = Instantiate(buildingPrefab, transform);
        building.SetActive(false);
        return building;
    }
}
