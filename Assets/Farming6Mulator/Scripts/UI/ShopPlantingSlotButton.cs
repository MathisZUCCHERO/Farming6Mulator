using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPlantingSlotButton : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private Button button;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(BuySlot);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void BuySlot()
    {
        if (shopUI == null)
            return;

        shopUI.BuyPlantingSlots();
        Refresh();
    }

    public void Refresh()
    {
        if (shopUI == null)
            return;

        bool canBuy = shopUI.CanBuyPlantingSlots();

        if (button != null)
            button.interactable = canBuy;

        if (nameText != null)
        {
            if (canBuy)
                nameText.text = "Buy Slot";
            else
                nameText.text = "Max Slots";
        }

        if (priceText != null)
        {
            if (canBuy)
                priceText.text = "$" + shopUI.GetPlantingSlotPrice().ToString("0");
            else
                priceText.text = "MAX";
        }
    }
}