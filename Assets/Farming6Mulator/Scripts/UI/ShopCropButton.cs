using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCropButton : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private CropData crop;
    [SerializeField] private int quantity = 1;

    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(Buy);
    }

    private void Start()
    {
        Refresh();
    }

    private void Buy()
    {
        if (shopUI == null || crop == null)
            return;

        shopUI.BuyCrop(crop, quantity);
    }

    private void Refresh()
    {
        if (crop == null)
            return;

        if (nameText != null)
            nameText.text = crop.cropName + " x" + quantity;

        if (priceText != null)
            priceText.text = "$" + (crop.buyPrice * quantity).ToString("0");
    }
}