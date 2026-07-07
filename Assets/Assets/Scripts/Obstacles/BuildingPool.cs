using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPool : MonoBehaviour
{
    [SerializeField]
    private GameObject buildingPrefab;

    [SerializeField]
    private int poolSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // Initialize pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject building = Instantiate(buildingPrefab, transform);
            building.SetActive(false);
            pool.Enqueue(building);
        }
    }

    // gets an object from queue and sets its active state true.
    public GameObject getBuilding()
    {
        if (pool.Count == 0)
            return null;

        GameObject building = pool.Dequeue();
        building.SetActive(true);

        return building;
    }

    public void returnBuildingToPool(GameObject building)
    {
        building.SetActive(false);
        pool.Enqueue(building);
    }
}
