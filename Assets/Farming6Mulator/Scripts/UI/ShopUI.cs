using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private PlayerCropInventory inventory;

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
}