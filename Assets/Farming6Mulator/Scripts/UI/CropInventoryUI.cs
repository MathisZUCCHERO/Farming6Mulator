using TMPro;
using UnityEngine;

public class CropInventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerCropInventory inventory;
    [SerializeField] private PlantingManager plantingManager;

    [SerializeField] private Transform contentRoot;
    [SerializeField] private CropInventoryButton buttonPrefab;
    [SerializeField] private TMP_Text emptyText;

    private void OnEnable()
    {
        if (inventory != null)
            inventory.InventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.InventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (contentRoot == null || buttonPrefab == null || inventory == null)
            return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        var crops = inventory.GetOwnedCrops();

        if (emptyText != null)
            emptyText.gameObject.SetActive(crops.Count == 0);

        foreach (CropData crop in crops)
        {
            CropInventoryButton button = Instantiate(buttonPrefab, contentRoot);
            button.Initialize(crop, inventory.GetAmount(crop), plantingManager);
        }
    }
}