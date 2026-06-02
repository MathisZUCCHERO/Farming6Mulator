using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text cropNameText;
    [SerializeField] private TMP_Text incomeText;
    [SerializeField] private TMP_Text refundText;

    [SerializeField] private Button deleteButton;

    private CropInstance currentCrop;

    private void Awake()
    {
        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSelectedCrop);

        Hide();
    }

    public void Show(CropInstance crop)
    {
        if (crop == null || crop.Data == null)
            return;

        currentCrop = crop;

        if (panel != null)
            panel.SetActive(true);

        cropNameText.text = crop.Data.cropName;
        incomeText.text = "Income: $" + crop.Data.incomePerSecond.ToString("0.##") + "/sec";
        refundText.text = "Refund: $" + crop.GetRefund().ToString("0");
    }

    public void Hide()
    {
        currentCrop = null;

        if (panel != null)
            panel.SetActive(false);
    }

    private void DeleteSelectedCrop()
    {
        if (currentCrop == null)
            return;

        currentCrop.Sell();
        Hide();
    }
}