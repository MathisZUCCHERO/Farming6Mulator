using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private PlayerCropInventory inventory;
    [SerializeField] private PlayerPlantingCapacity plantingCapacity;

    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text slotsText;
    [SerializeField] private TMP_Text selectedCropText;

    private void OnEnable()
    {
        if (wallet != null)
            wallet.MoneyChanged += OnMoneyChanged;

        if (inventory != null)
            inventory.InventoryChanged += RefreshInventory;

        if (plantingCapacity != null)
            plantingCapacity.CapacityChanged += RefreshPlantingSlots;
    }

    private void OnDisable()
    {
        if (wallet != null)
            wallet.MoneyChanged -= OnMoneyChanged;

        if (inventory != null)
            inventory.InventoryChanged -= RefreshInventory;

        if (plantingCapacity != null)
            plantingCapacity.CapacityChanged -= RefreshPlantingSlots;
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnMoneyChanged(float money)
    {
        if (moneyText != null)
            moneyText.text = "$" + money.ToString("0");
    }

    private void RefreshInventory()
    {
        RefreshPlantingSlots();
    }

    private void RefreshPlantingSlots()
    {
        if (slotsText == null || plantingCapacity == null)
            return;

        slotsText.text =
            plantingCapacity.UsedPlantingSlots + " / " +
            plantingCapacity.MaxPlantingSlots + " slots occupied";
    }

    public void SetSelectedCrop(CropData crop)
    {
        if (selectedCropText == null)
            return;

        if (crop == null)
        {
            selectedCropText.text = "Selected: None";
            return;
        }

        int amount = inventory != null ? inventory.GetAmount(crop) : 0;
        selectedCropText.text = "Selected: " + crop.cropName + " x" + amount;
    }

    private void RefreshAll()
    {
        if (wallet != null)
            OnMoneyChanged(wallet.Money);

        RefreshPlantingSlots();
    }
}