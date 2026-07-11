using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Streams a deterministic city map around the player. Each chunk is generated from its grid coordinate,
/// allowing the player to steer through the city in any forward-facing direction without fixed courses.
/// </summary>
public class BuildingSpawner : MonoBehaviour
{
    [Serializable]
    private struct BuildingPreset
    {
        [Min(1f)] public float width;
        [Min(1f)] public float length;
        [Min(1f)] public float height;
    }

    /// <summary>Tracks the pooled objects that belong to one generated city chunk.</summary>
    private sealed class CityChunk
    {
        public readonly List<GameObject> Buildings = new();
    }

    [Header("References")]
    [SerializeField] private BuildingPool buildingPool;
    [SerializeField] private Transform player;

    [Header("Chunk Streaming")]
    [SerializeField, Min(40f)] private float chunkSize = 120f;
    [SerializeField, Min(1)] private int activeChunkRadius = 2;
    [SerializeField] private int citySeed = 7907;
    [SerializeField, Min(1f)] private float startingClearRadius = 28f;

    [Header("City Layout")]
    [SerializeField, Min(1)] private int minimumLotsPerAxis = 2;
    [SerializeField, Min(1)] private int maximumLotsPerAxis = 3;
    [SerializeField, Min(1f)] private float lotGap = 8f;
    [SerializeField, Range(0f, 0.6f)] private float openLotChance = 0.18f;
    [SerializeField, Range(0.3f, 1f)] private float minimumLotCoverage = 0.65f;
    [SerializeField, Range(0.3f, 1f)] private float maximumLotCoverage = 0.9f;

    [Header("Building Presets")]
    [SerializeField]
    private BuildingPreset[] presets =
    {
        new BuildingPreset { width = 12f, length = 12f, height = 20f },
        new BuildingPreset { width = 18f, length = 18f, height = 40f },
        new BuildingPreset { width = 10f, length = 10f, height = 80f },
        new BuildingPreset { width = 24f, length = 16f, height = 30f }
    };

    private readonly Dictionary<Vector2Int, CityChunk> activeChunks = new();
    private readonly List<Vector2Int> chunksToRelease = new();

    private Vector2 startingPosition;

    private void Start()
    {
        startingPosition = ToPlanarPosition(player.position);
        UpdateVisibleCity();
    }

    private void Update()
    {
        UpdateVisibleCity();
    }

    private void OnDisable()
    {
        ReleaseAllChunks();
    }

    /// <summary>Creates nearby chunks first, then returns chunks that are no longer relevant to the pool.</summary>
    private void UpdateVisibleCity()
    {
        Vector2Int playerChunk = GetChunkCoordinate(player.position);
        CreateChunksAround(playerChunk);
        ReleaseDistantChunks(playerChunk);
    }

    private void CreateChunksAround(Vector2Int centerChunk)
    {
        for (int x = -activeChunkRadius; x <= activeChunkRadius; x++)
        {
            for (int z = -activeChunkRadius; z <= activeChunkRadius; z++)
            {
                Vector2Int coordinate = centerChunk + new Vector2Int(x, z);

                if (!activeChunks.ContainsKey(coordinate))
                    activeChunks.Add(coordinate, GenerateChunk(coordinate));
            }
        }
    }

    /// <summary>
    /// Generates one irregular city block. Lot counts, empty plazas, footprints, and building heights are all
    /// coordinate-seeded, so adjacent chunks remain stable without relying on handcrafted patterns.
    /// </summary>
    private CityChunk GenerateChunk(Vector2Int coordinate)
    {
        CityChunk chunk = new CityChunk();
        System.Random random = CreateChunkRandom(coordinate);
        int lotsX = NextInt(random, GetMinimumLotsPerAxis(), GetMaximumLotsPerAxis());
        int lotsZ = NextInt(random, GetMinimumLotsPerAxis(), GetMaximumLotsPerAxis());
        Vector2 chunkOrigin = new Vector2(coordinate.x * chunkSize, coordinate.y * chunkSize);
        Vector2 lotSize = new Vector2(chunkSize / lotsX, chunkSize / lotsZ);

        for (int x = 0; x < lotsX; x++)
        {
            for (int z = 0; z < lotsZ; z++)
            {
                if (ShouldLeaveLotOpen(random))
                    continue;

                CreateLotBuilding(chunk, random, chunkOrigin, lotSize, x, z);
            }
        }

        return chunk;
    }

    private void CreateLotBuilding(CityChunk chunk, System.Random random, Vector2 chunkOrigin, Vector2 lotSize, int lotX, int lotZ)
    {
        Vector2 lotCenter = GetLotCenter(chunkOrigin, lotSize, lotX, lotZ);
        BuildingPreset preset = GetRandomPreset(random);
        Vector2 footprint = GetBuildingFootprint(random, lotSize, preset);
        Vector2 position = lotCenter + GetFootprintOffset(random, lotSize, footprint);

        if (IsInsideStartingClearArea(position, footprint))
            return;

        SpawnBuilding(chunk, random, position, footprint, preset);
    }

    /// <summary>Returns only chunks outside the release radius, leaving a buffer to avoid pool churn while turning.</summary>
    private void ReleaseDistantChunks(Vector2Int playerChunk)
    {
        chunksToRelease.Clear();
        int releaseRadius = activeChunkRadius + 1;

        foreach (KeyValuePair<Vector2Int, CityChunk> entry in activeChunks)
        {
            if (Mathf.Abs(entry.Key.x - playerChunk.x) > releaseRadius ||
                Mathf.Abs(entry.Key.y - playerChunk.y) > releaseRadius)
            {
                chunksToRelease.Add(entry.Key);
            }
        }

        foreach (Vector2Int coordinate in chunksToRelease)
            ReleaseChunk(coordinate);
    }

    private void ReleaseChunk(Vector2Int coordinate)
    {
        CityChunk chunk = activeChunks[coordinate];

        foreach (GameObject building in chunk.Buildings)
            buildingPool.ReturnBuildingToPool(building);

        activeChunks.Remove(coordinate);
    }

    private void ReleaseAllChunks()
    {
        foreach (CityChunk chunk in activeChunks.Values)
        {
            foreach (GameObject building in chunk.Buildings)
                buildingPool.ReturnBuildingToPool(building);
        }

        activeChunks.Clear();
    }

    private void SpawnBuilding(CityChunk chunk, System.Random random, Vector2 position, Vector2 footprint, BuildingPreset preset)
    {
        GameObject building = buildingPool.GetBuilding();

        if (building == null)
            return;

        float height = preset.height * NextFloat(random, 0.85f, 1.15f);

        building.transform.position = new Vector3(position.x, height * 0.5f, position.y);
        building.transform.localScale = new Vector3(footprint.x, height, footprint.y);
        chunk.Buildings.Add(building);
    }

    private BuildingPreset GetRandomPreset(System.Random random)
    {
        if (presets == null || presets.Length == 0)
            return new BuildingPreset { width = 1f, length = 1f, height = 1f };

        return presets[random.Next(0, presets.Length)];
    }

    private Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / chunkSize),
            Mathf.FloorToInt(worldPosition.z / chunkSize));
    }

    private Vector2 GetLotCenter(Vector2 chunkOrigin, Vector2 lotSize, int lotX, int lotZ)
    {
        return chunkOrigin + new Vector2((lotX + 0.5f) * lotSize.x, (lotZ + 0.5f) * lotSize.y);
    }

    private Vector2 GetBuildingFootprint(System.Random random, Vector2 lotSize, BuildingPreset preset)
    {
        float usableWidth = Mathf.Max(1f, lotSize.x - lotGap);
        float usableLength = Mathf.Max(1f, lotSize.y - lotGap);
        float minimumCoverage = Mathf.Min(minimumLotCoverage, maximumLotCoverage);
        float maximumCoverage = Mathf.Max(minimumLotCoverage, maximumLotCoverage);

        float width = usableWidth * NextFloat(random, minimumCoverage, maximumCoverage);
        float length = usableLength * NextFloat(random, minimumCoverage, maximumCoverage);

        return new Vector2(
            Mathf.Clamp(Mathf.Max(width, preset.width), 1f, usableWidth),
            Mathf.Clamp(Mathf.Max(length, preset.length), 1f, usableLength));
    }

    private Vector2 GetFootprintOffset(System.Random random, Vector2 lotSize, Vector2 footprint)
    {
        float horizontalSlack = Mathf.Max(0f, lotSize.x - lotGap - footprint.x) * 0.5f;
        float verticalSlack = Mathf.Max(0f, lotSize.y - lotGap - footprint.y) * 0.5f;

        return new Vector2(
            NextFloat(random, -horizontalSlack, horizontalSlack),
            NextFloat(random, -verticalSlack, verticalSlack));
    }

    private bool ShouldLeaveLotOpen(System.Random random)
    {
        return random.NextDouble() < openLotChance;
    }

    private bool IsInsideStartingClearArea(Vector2 position, Vector2 footprint)
    {
        float buildingRadius = Mathf.Max(footprint.x, footprint.y) * 0.5f;
        return Vector2.Distance(position, startingPosition) < startingClearRadius + buildingRadius;
    }

    private System.Random CreateChunkRandom(Vector2Int coordinate)
    {
        unchecked
        {
            int seed = citySeed;
            seed = (seed * 486187739) + coordinate.x;
            seed = (seed * 16777619) + coordinate.y;
            return new System.Random(seed);
        }
    }

    private int GetMinimumLotsPerAxis()
    {
        return Mathf.Min(minimumLotsPerAxis, maximumLotsPerAxis);
    }

    private int GetMaximumLotsPerAxis()
    {
        return Mathf.Max(minimumLotsPerAxis, maximumLotsPerAxis);
    }

    private static int NextInt(System.Random random, int minimumInclusive, int maximumInclusive)
    {
        return random.Next(minimumInclusive, maximumInclusive + 1);
    }

    private static float NextFloat(System.Random random, float minimum, float maximum)
    {
        return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }

    private static Vector2 ToPlanarPosition(Vector3 position)
    {
        return new Vector2(position.x, position.z);
    }
}
