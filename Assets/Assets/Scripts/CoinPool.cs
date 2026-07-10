using System.Collections.Generic;
using UnityEngine;

public class CoinPool : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int poolSize = 50;

    private readonly Queue<GameObject> pool = new();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform);
            coin.SetActive(false);
            pool.Enqueue(coin);
        }
    }

    public GameObject GetCoin()
    {
        if (pool.Count == 0)
            return null;

        GameObject coin = pool.Dequeue();
        coin.SetActive(true);

        return coin;
    }

    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
        pool.Enqueue(coin);
    }
}