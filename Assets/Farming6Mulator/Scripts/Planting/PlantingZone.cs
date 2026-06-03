using System.Collections.Generic;
using UnityEngine;

public class PlantingZone : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridDepth = 10;
    [SerializeField] private float cellSize = 0.4f;
    [SerializeField] private float cropYOffset = 0.02f;
    [SerializeField] private float surfaceLocalY = 0f;
    [SerializeField] private float colliderThickness = 0.05f;
    [SerializeField] private float colliderAboveSurfaceOffset = 0.03f;

    [Header("References")]
    [SerializeField] private Transform cropRoot;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private GameObject cellPrefab;

    [Header("Tile Visual")]
    [SerializeField] private float tileYOffset = 0f;
    [SerializeField] private Vector3 tileRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 tileScale = Vector3.one;

    private readonly Dictionary<int, CropInstance> cropsByCell = new Dictionary<int, CropInstance>();
    private readonly Dictionary<CropInstance, int> cellsByCrop = new Dictionary<CropInstance, int>();

    public int MaxCells => gridWidth * gridDepth;

    public void RefreshVisibleCells(int unlockedSlots)
    {
        EnsureRoots();
        ClearVisuals();

        if (cellPrefab == null)
            return;

        int visibleCount = Mathf.Clamp(unlockedSlots, 0, MaxCells);
        Quaternion tileRotation = transform.rotation * Quaternion.Euler(tileRotationOffset);

        for (int index = 0; index < visibleCount; index++)
        {
            IndexToCoord(index, out int x, out int z);

            Vector3 worldPos = GetCellCenterWorld(x, z);
            worldPos.y = transform.position.y + tileYOffset;

            GameObject tile = Instantiate(cellPrefab, worldPos, tileRotation, visualRoot);
            tile.transform.localScale = tileScale;
            tile.name = "Cell_" + x + "_" + z;

            DisableTileColliders(tile);
        }
    }
    
    private void AlignTileTopToSurface(GameObject tile, float targetTopY)
    {
        if (tile == null)
            return;

        if (!TryGetRendererBounds(tile, out Bounds bounds))
            return;

        float offset = targetTopY - bounds.max.y;
        tile.transform.position += Vector3.up * offset;
    }

    private bool TryGetRendererBounds(GameObject target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
        {
            bounds = default;
            return false;
        }

        bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
    }

    public bool TryGetPlantCell(
        Vector3 worldPoint,
        int unlockedSlots,
        out Vector3 plantPosition,
        out int cellIndex
    )
    {
        plantPosition = Vector3.zero;
        cellIndex = -1;

        int visibleCount = Mathf.Clamp(unlockedSlots, 0, MaxCells);

        if (visibleCount <= 0)
            return false;

        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

        float totalWidth = gridWidth * cellSize;
        float totalDepth = gridDepth * cellSize;

        float shiftedX = localPoint.x + totalWidth * 0.5f;
        float shiftedZ = localPoint.z + totalDepth * 0.5f;

        int x = Mathf.FloorToInt(shiftedX / cellSize);
        int z = Mathf.FloorToInt(shiftedZ / cellSize);

        if (x < 0 || x >= gridWidth || z < 0 || z >= gridDepth)
            return false;

        cellIndex = z * gridWidth + x;

        if (cellIndex >= visibleCount)
            return false;

        if (cropsByCell.ContainsKey(cellIndex))
            return false;

        plantPosition = GetCellCenterWorld(x, z);
        plantPosition.y = transform.position.y + cropYOffset;

        return true;
    }

    public CropInstance Plant(
        CropData cropData,
        Vector3 hitPoint,
        int unlockedSlots,
        Quaternion rotation,
        CropIncomeManager incomeManager,
        PlayerWallet wallet,
        PlayerPlantingCapacity plantingCapacity,
        Transform playerCamera
    )
    {
        if (cropData == null || cropData.plantedPrefab == null)
            return null;

        if (!TryGetPlantCell(hitPoint, unlockedSlots, out Vector3 plantPosition, out int cellIndex))
            return null;

        Transform parent = cropRoot != null ? cropRoot : transform;

        GameObject instance = Instantiate(cropData.plantedPrefab, plantPosition, rotation, parent);

        CropInstance cropInstance = instance.GetComponent<CropInstance>();

        if (cropInstance == null)
            cropInstance = instance.AddComponent<CropInstance>();

        cropInstance.Initialize(
            cropData,
            this,
            incomeManager,
            wallet,
            plantingCapacity,
            playerCamera
        );

        cropsByCell[cellIndex] = cropInstance;
        cellsByCrop[cropInstance] = cellIndex;

        return cropInstance;
    }

    public void RemoveCrop(CropInstance crop)
    {
        if (crop == null)
            return;

        if (!cellsByCrop.TryGetValue(crop, out int cellIndex))
            return;

        cellsByCrop.Remove(crop);
        cropsByCell.Remove(cellIndex);
    }

    private void EnsureRoots()
    {
        if (cropRoot == null)
        {
            Transform existing = transform.Find("CropRoot");

            if (existing != null)
                cropRoot = existing;
            else
            {
                GameObject go = new GameObject("CropRoot");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                cropRoot = go.transform;
            }
        }

        if (visualRoot == null)
        {
            Transform existing = transform.Find("VisualRoot");

            if (existing != null)
                visualRoot = existing;
            else
            {
                GameObject go = new GameObject("VisualRoot");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                visualRoot = go.transform;
            }
        }
    }

    private void ClearVisuals()
    {
        if (visualRoot == null)
            return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = visualRoot.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private void DisableTileColliders(GameObject tile)
    {
        Collider[] colliders = tile.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
            col.enabled = false;
    }

    private void IndexToCoord(int index, out int x, out int z)
    {
        x = index % gridWidth;
        z = index / gridWidth;
    }

    private Vector3 GetCellCenterWorld(int x, int z)
    {
        return transform.TransformPoint(GetCellCenterLocal(x, z));
    }

    private Vector3 GetCellCenterLocal(int x, int z)
    {
        float totalWidth = gridWidth * cellSize;
        float totalDepth = gridDepth * cellSize;

        float centeredX = -totalWidth * 0.5f + cellSize * 0.5f + x * cellSize;
        float centeredZ = -totalDepth * 0.5f + cellSize * 0.5f + z * cellSize;

        return new Vector3(centeredX, 0f, centeredZ);
    }

    private float GetSurfaceY()
    {
        return transform.position.y;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int z = 0; z < gridDepth; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 center = GetCellCenterLocal(x, z);
                Vector3 size = new Vector3(cellSize * 0.95f, 0.02f, cellSize * 0.95f);
                Gizmos.DrawWireCube(center + Vector3.up * 0.05f, size);
            }
        }
    }
}