using TMPro;
using UnityEngine;

public class CropInstance : MonoBehaviour
{
    [SerializeField] private TextMeshPro yieldText;
    [SerializeField] private Transform yieldAnchor;
    [SerializeField] private Vector3 fallbackYieldOffset = new Vector3(0f, 0.35f, 0f);

    public CropData Data { get; private set; }

    private PlantingZone zone;
    private CropIncomeManager incomeManager;
    private PlayerWallet wallet;
    private PlayerPlantingCapacity plantingCapacity;
    private Transform playerCamera;

    public void Initialize(
        CropData cropData,
        PlantingZone ownerZone,
        CropIncomeManager ownerIncomeManager,
        PlayerWallet ownerWallet,
        PlayerPlantingCapacity ownerPlantingCapacity,
        Transform playerCam
    )
    {
        Data = cropData;
        zone = ownerZone;
        incomeManager = ownerIncomeManager;
        wallet = ownerWallet;
        plantingCapacity = ownerPlantingCapacity;
        playerCamera = playerCam;

        if (yieldText == null)
            yieldText = GetComponentInChildren<TextMeshPro>(true);

        RefreshYieldText();
        PositionYieldText();
    }

    private void LateUpdate()
    {
        FaceYieldTextToCamera();
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
        yieldText.alignment = TextAlignmentOptions.Center;
    }

    private void PositionYieldText()
    {
        if (yieldText == null)
            return;

        if (yieldAnchor != null)
        {
            yieldText.transform.position = yieldAnchor.position;
            yieldText.transform.rotation = yieldAnchor.rotation;
        }
        else
        {
            yieldText.transform.localPosition = fallbackYieldOffset;
        }
    }

    private void FaceYieldTextToCamera()
    {
        if (yieldText == null || playerCamera == null)
            return;

        Vector3 direction = yieldText.transform.position - playerCamera.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            yieldText.transform.rotation = Quaternion.LookRotation(direction);
    }
}