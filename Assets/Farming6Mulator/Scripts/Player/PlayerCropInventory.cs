using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCropInventory : MonoBehaviour
{
    private readonly Dictionary<CropData, int> cropAmounts = new Dictionary<CropData, int>();

    public event Action InventoryChanged;

    public void Add(CropData crop, int quantity)
    {
        if (crop == null || quantity <= 0)
            return;

        if (!cropAmounts.ContainsKey(crop))
            cropAmounts.Add(crop, 0);

        cropAmounts[crop] += quantity;
        InventoryChanged?.Invoke();
    }

    public bool HasCrop(CropData crop)
    {
        return GetAmount(crop) > 0;
    }

    public int GetAmount(CropData crop)
    {
        if (crop == null)
            return 0;

        if (!cropAmounts.ContainsKey(crop))
            return 0;

        return cropAmounts[crop];
    }

    public bool TryRemove(CropData crop, int quantity)
    {
        if (crop == null || quantity <= 0)
            return false;

        if (!cropAmounts.ContainsKey(crop))
            return false;

        if (cropAmounts[crop] < quantity)
            return false;

        cropAmounts[crop] -= quantity;

        if (cropAmounts[crop] <= 0)
            cropAmounts.Remove(crop);

        InventoryChanged?.Invoke();
        return true;
    }

    public List<CropData> GetOwnedCrops()
    {
        List<CropData> crops = new List<CropData>();

        foreach (KeyValuePair<CropData, int> pair in cropAmounts)
        {
            if (pair.Key != null && pair.Value > 0)
                crops.Add(pair.Key);
        }

        return crops;
    }
}