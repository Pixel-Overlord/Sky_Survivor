using UnityEngine;

/// <summary>
/// Example IRowDecorator: drops a pooled coin inside the safe corridor on
/// some rows, rewarding the player for following the intended path. This
/// exists to demonstrate the extension seam — power-ups, moving obstacles or
/// district props would be written the same way, with zero changes to
/// BuildingSpawner.
///
/// Put this component on the same GameObject as BuildingSpawner (or a child)
/// and it is picked up automatically.
/// </summary>
public class CoinRowDecorator : MonoBehaviour, IRowDecorator
{
    [SerializeField] private CoinPool coinPool;
    [Range(0f, 1f)][SerializeField] private float spawnChancePerRow = 0.35f;
    [SerializeField] private float coinHeight = 6f;

    // NOTE: for a shipping build the coin, like the building, needs to be
    // returned to its pool once it passes behind the player (e.g. a small
    // self-recycling component on the coin prefab). Omitted here to keep the
    // example focused on the decorator seam.
    public void OnRowSpawned(in RowContext ctx)
    {
        if (coinPool == null || Random.value > spawnChancePerRow)
            return;

        int lane = Random.Range(ctx.CorridorMinLane, ctx.CorridorMaxLane + 1);
        GameObject coin = coinPool.GetCoin();
        if (coin == null)
            return;

        coin.transform.position = new Vector3(ctx.Grid.LaneToWorldX(lane), coinHeight, ctx.Z);
    }
}
