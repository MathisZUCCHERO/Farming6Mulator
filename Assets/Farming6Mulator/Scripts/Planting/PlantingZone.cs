using System.Collections.Generic;
using UnityEngine;

public class PlantingZone : MonoBehaviour
{
    [SerializeField] private Transform cropRoot;
    [SerializeField] private float minDistanceBetweenCrops = 1f;

    private readonly List<CropInstance> plantedCrops = new List<CropInstance>();

    public bool CanPlant(Vector3 position)
    {
        float minDistanceSqr = minDistanceBetweenCrops * minDistanceBetweenCrops;

        for (int i = plantedCrops.Count - 1; i >= 0; i--)
        {
            if (plantedCrops[i] == null)
            {
                plantedCrops.RemoveAt(i);
                continue;
            }

            float distanceSqr = (plantedCrops[i].transform.position - position).sqrMagnitude;

            if (distanceSqr < minDistanceSqr)
                return false;
        }

        return true;
    }

    public CropInstance Plant(
        CropData cropData,
        Vector3 position,
        Quaternion rotation,
        CropIncomeManager incomeManager,
        PlayerWallet wallet,
        PlayerPlantingCapacity plantingCapacity
    )
    {
        if (cropData == null || cropData.plantedPrefab == null)
            return null;

        if (!CanPlant(position))
            return null;

        Transform parent = cropRoot != null ? cropRoot : transform;

        GameObject instance = Instantiate(cropData.plantedPrefab, position, rotation, parent);
        CropInstance cropInstance = instance.GetComponent<CropInstance>();

        if (cropInstance == null)
            cropInstance = instance.AddComponent<CropInstance>();

        cropInstance.Initialize(
            cropData,
            this,
            incomeManager,
            wallet,
            plantingCapacity
        );

        plantedCrops.Add(cropInstance);

        return cropInstance;
    }

    public void RemoveCrop(CropInstance crop)
    {
        if (crop == null)
            return;

        plantedCrops.Remove(crop);
    }
}