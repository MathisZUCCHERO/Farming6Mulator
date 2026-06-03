using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private PlayerCropInventory inventory;
    [SerializeField] private PlayerPlantingCapacity plantingCapacity;

    [Header("Planting Zones")]
    [SerializeField] private PlantingZone[] plantingZones;

    [Header("Slot Upgrade")]
    [SerializeField] private float plantingSlotPrice = 50f;
    [SerializeField] private int slotsPerPurchase = 1;

    public bool BuyCrop(CropData crop, int quantity)
    {
        if (crop == null || quantity <= 0)
            return false;

        if (wallet == null || inventory == null)
            return false;

        float totalPrice = crop.buyPrice * quantity;

        if (!wallet.TrySpend(totalPrice))
            return false;

        inventory.Add(crop, quantity);
        return true;
    }

    public bool CanBuyPlantingSlots()
    {
        if (plantingCapacity == null)
            return false;

        return plantingCapacity.MaxPlantingSlots < GetTotalAvailablePlantingSlots();
    }

    public bool BuyPlantingSlots()
    {
        if (wallet == null || plantingCapacity == null)
            return false;

        if (!CanBuyPlantingSlots())
            return false;

        int remainingAvailableSlots =
            GetTotalAvailablePlantingSlots() - plantingCapacity.MaxPlantingSlots;

        int amountToBuy = Mathf.Min(slotsPerPurchase, remainingAvailableSlots);

        float totalPrice = plantingSlotPrice * amountToBuy;

        if (!wallet.TrySpend(totalPrice))
            return false;

        plantingCapacity.AddPlantingSlots(amountToBuy);
        return true;
    }

    public float GetPlantingSlotPrice()
    {
        return plantingSlotPrice * slotsPerPurchase;
    }

    public int GetTotalAvailablePlantingSlots()
    {
        if (plantingZones == null || plantingZones.Length == 0)
            return 0;

        int total = 0;

        foreach (PlantingZone zone in plantingZones)
        {
            if (zone == null)
                continue;

            total += zone.MaxCells;
        }

        return total;
    }

    public int GetRemainingLockedPlantingSlots()
    {
        if (plantingCapacity == null)
            return 0;

        return Mathf.Max(0, GetTotalAvailablePlantingSlots() - plantingCapacity.MaxPlantingSlots);
    }
}