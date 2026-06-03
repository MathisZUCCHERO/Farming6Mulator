using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropInventoryButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text incomeText;

    private CropData crop;
    private PlantingManager plantingManager;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(SelectCrop);
    }

    public void Initialize(CropData newCrop, int amount, PlantingManager manager)
    {
        crop = newCrop;
        plantingManager = manager;

        if (crop == null)
            return;

        if (nameText != null)
            nameText.text = crop.cropName;

        if (amountText != null)
            amountText.text = "x" + amount;

        if (incomeText != null)
            incomeText.text = "$" + crop.incomePerSecond.ToString("0.##") + "/sec";
    }

    private void SelectCrop()
    {
        if (plantingManager == null || crop == null)
            return;

        plantingManager.SelectCrop(crop);
    }
}