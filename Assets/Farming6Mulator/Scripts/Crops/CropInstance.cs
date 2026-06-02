using TMPro;
using UnityEngine;

public class CropInstance : MonoBehaviour
{
    [SerializeField] private TMP_Text yieldText;

    public CropData Data { get; private set; }

    private PlantingZone zone;
    private CropIncomeManager incomeManager;
    private PlayerWallet wallet;
    private PlayerPlantingCapacity plantingCapacity;

    public void Initialize(
        CropData cropData,
        PlantingZone ownerZone,
        CropIncomeManager ownerIncomeManager,
        PlayerWallet ownerWallet,
        PlayerPlantingCapacity ownerPlantingCapacity
    )
    {
        Data = cropData;
        zone = ownerZone;
        incomeManager = ownerIncomeManager;
        wallet = ownerWallet;
        plantingCapacity = ownerPlantingCapacity;

        if (yieldText == null)
            yieldText = GetComponentInChildren<TMP_Text>();

        RefreshYieldText();
    }

    public float GetIncomeForSeconds(float seconds)
    {
        if (Data == null)
            return 0f;

        return Data.incomePerSecond * seconds;
    }

    public float GetRefund()
    {
        if (Data == null)
            return 0f;

        return Data.sellRefund;
    }

    public void Sell()
    {
        if (wallet != null)
            wallet.AddMoney(GetRefund());

        if (incomeManager != null)
            incomeManager.UnregisterCrop(this);

        if (zone != null)
            zone.RemoveCrop(this);

        if (plantingCapacity != null)
            plantingCapacity.ReleasePlantingSlot();

        Destroy(gameObject);
    }

    private void RefreshYieldText()
    {
        if (yieldText == null || Data == null)
            return;

        yieldText.text = "$" + Data.incomePerSecond.ToString("0.##") + "/sec";
    }
}