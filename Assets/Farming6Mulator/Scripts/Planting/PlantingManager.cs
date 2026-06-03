using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlantingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform pointerOrigin;
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private PlayerCropInventory inventory;
    [SerializeField] private PlayerPlantingCapacity plantingCapacity;
    [SerializeField] private CropIncomeManager incomeManager;
    [SerializeField] private CropInfoUI cropInfoUI;
    [SerializeField] private PlayerHUD playerHUD;
    [SerializeField] private PlantingZone[] plantingZones;

    [Header("Input")]
    [SerializeField] private InputActionReference plantAction;
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("UI")]
    [SerializeField] private InventoryWorldUI inventoryUI;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask plantingZoneMask;
    [SerializeField] private LayerMask cropMask;

    private CropData selectedCrop;
    private GameObject phantomInstance;
    private CropData phantomCrop;

    private void OnEnable()
    {
        if (plantAction != null)
        {
            plantAction.action.Enable();
            plantAction.action.performed += OnPlantAction;
        }

        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.Enable();
            toggleInventoryAction.action.performed += OnToggleInventoryAction;
        }

        if (plantingCapacity != null)
            plantingCapacity.CapacityChanged += RefreshZoneVisuals;

        RefreshZoneVisuals();
    }

    private void OnDisable()
    {
        if (plantAction != null)
            plantAction.action.performed -= OnPlantAction;

        if (toggleInventoryAction != null)
            toggleInventoryAction.action.performed -= OnToggleInventoryAction;

        if (plantingCapacity != null)
            plantingCapacity.CapacityChanged -= RefreshZoneVisuals;
    }

    private void Update()
    {
        UpdatePhantom();
    }

    public void SelectCrop(CropData crop)
    {
        selectedCrop = crop;

        if (playerHUD != null)
            playerHUD.SetSelectedCrop(selectedCrop);

        if (inventoryUI != null)
            inventoryUI.Close();

        if (cropInfoUI != null)
            cropInfoUI.Hide();
    }

    public void ClearSelectedCrop()
    {
        selectedCrop = null;

        if (playerHUD != null)
            playerHUD.SetSelectedCrop(null);

        ClearPhantom();
    }

    private void OnPlantAction(InputAction.CallbackContext context)
    {
        if (IsBlockingWorldInteraction())
            return;

        TryInteract();
    }

    private void OnToggleInventoryAction(InputAction.CallbackContext context)
    {
        Debug.Log("QUEST TOGGLE INVENTORY DETECTED");
        
        if (inventoryUI == null)
        {
            Debug.LogError("Inventory UI is not assigned in PlantingManager");
            return;
        }

        inventoryUI.Toggle();

        if (inventoryUI.IsOpen && cropInfoUI != null)
            cropInfoUI.Hide();
    }

    private bool IsBlockingWorldInteraction()
    {
        if (inventoryUI != null && inventoryUI.IsOpen)
            return true;

        if (cropInfoUI != null && cropInfoUI.IsOpen)
            return true;

        return false;
    }

    private void TryInteract()
    {
        if (pointerOrigin == null)
            return;

        Ray ray = new Ray(pointerOrigin.position, pointerOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit cropHit, maxDistance, cropMask))
        {
            CropInstance crop = cropHit.collider.GetComponentInParent<CropInstance>();

            if (crop != null && cropInfoUI != null)
            {
                cropInfoUI.Show(crop);
                return;
            }
        }

        TryPlant(ray);
    }

    private void TryPlant(Ray ray)
    {
        if (selectedCrop == null)
            return;

        if (inventory == null || incomeManager == null || wallet == null || plantingCapacity == null)
            return;

        if (!inventory.HasCrop(selectedCrop))
        {
            ClearSelectedCrop();
            return;
        }

        if (!plantingCapacity.HasFreeSlot())
            return;

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, plantingZoneMask))
            return;

        PlantingZone zone = hit.collider.GetComponentInParent<PlantingZone>();

        if (zone == null)
            return;

        int unlockedSlotsForThisZone = GetUnlockedSlotsForZone(zone);

        if (!zone.TryGetPlantCell(
                hit.point,
                unlockedSlotsForThisZone,
                out Vector3 plantPosition,
                out int cellIndex
            ))
            return;

        if (!inventory.TryRemove(selectedCrop, 1))
            return;

        if (!plantingCapacity.TryUsePlantingSlot())
        {
            inventory.Add(selectedCrop, 1);
            return;
        }

        CropInstance crop = zone.Plant(
            selectedCrop,
            hit.point,
            unlockedSlotsForThisZone,
            Quaternion.identity,
            incomeManager,
            wallet,
            plantingCapacity,
            Camera.main != null ? Camera.main.transform : null
        );

        if (crop == null)
        {
            plantingCapacity.ReleasePlantingSlot();
            inventory.Add(selectedCrop, 1);
            return;
        }

        incomeManager.RegisterCrop(crop);

        if (!inventory.HasCrop(selectedCrop))
            ClearSelectedCrop();
        else if (playerHUD != null)
            playerHUD.SetSelectedCrop(selectedCrop);
    }

    private void UpdatePhantom()
    {
        if (IsBlockingWorldInteraction())
        {
            ClearPhantom();
            return;
        }

        if (pointerOrigin == null || selectedCrop == null || inventory == null || plantingCapacity == null)
        {
            ClearPhantom();
            return;
        }

        if (!inventory.HasCrop(selectedCrop))
        {
            ClearPhantom();
            return;
        }

        if (!Physics.Raycast(pointerOrigin.position, pointerOrigin.forward, out RaycastHit hit, maxDistance, plantingZoneMask))
        {
            ClearPhantom();
            return;
        }

        PlantingZone zone = hit.collider.GetComponentInParent<PlantingZone>();

        if (zone == null)
        {
            ClearPhantom();
            return;
        }

        int unlockedSlotsForThisZone = GetUnlockedSlotsForZone(zone);

        if (!zone.TryGetPlantCell(
                hit.point,
                unlockedSlotsForThisZone,
                out Vector3 plantPosition,
                out int cellIndex
            ))
        {
            ClearPhantom();
            return;
        }

        EnsurePhantom(selectedCrop);
        UpdatePhantomText(selectedCrop);

        if (phantomInstance == null)
            return;

        phantomInstance.SetActive(true);
        phantomInstance.transform.position = plantPosition;
        phantomInstance.transform.rotation = Quaternion.identity;
    }
    
    private void UpdatePhantomText(CropData crop)
    {
        if (phantomInstance == null || crop == null)
            return;

        TMP_Text[] texts = phantomInstance.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            text.text = "$" + crop.incomePerSecond.ToString("0.##") + "/sec";
            text.alignment = TextAlignmentOptions.Center;
            text.gameObject.SetActive(true);
        }
    }

    private void EnsurePhantom(CropData crop)
    {
        if (crop == null)
            return;

        if (phantomInstance != null && phantomCrop == crop)
        {
            UpdatePhantomText(crop);
            return;
        }

        ClearPhantom();

        GameObject prefab = crop.phantomPrefab != null ? crop.phantomPrefab : crop.plantedPrefab;

        if (prefab == null)
            return;

        phantomInstance = Instantiate(prefab);
        phantomCrop = crop;

        Collider[] colliders = phantomInstance.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
            col.enabled = false;

        CropInstance cropInstance = phantomInstance.GetComponent<CropInstance>();

        if (cropInstance != null)
            Destroy(cropInstance);

        UpdatePhantomText(crop);
    }

    private void ClearPhantom()
    {
        if (phantomInstance != null)
            Destroy(phantomInstance);

        phantomInstance = null;
        phantomCrop = null;
    }
    
    private void RefreshZoneVisuals()
    {
        if (plantingZones == null || plantingCapacity == null)
            return;

        int remainingSlots = plantingCapacity.MaxPlantingSlots;

        for (int i = 0; i < plantingZones.Length; i++)
        {
            PlantingZone zone = plantingZones[i];

            if (zone == null)
                continue;

            int unlockedForThisZone = Mathf.Clamp(remainingSlots, 0, zone.MaxCells);
            zone.RefreshVisibleCells(unlockedForThisZone);

            remainingSlots -= unlockedForThisZone;
        }
    }

    private int GetUnlockedSlotsForZone(PlantingZone targetZone)
    {
        if (plantingZones == null || plantingCapacity == null || targetZone == null)
            return 0;

        int remainingSlots = plantingCapacity.MaxPlantingSlots;

        for (int i = 0; i < plantingZones.Length; i++)
        {
            PlantingZone zone = plantingZones[i];

            if (zone == null)
                continue;

            int unlockedForThisZone = Mathf.Clamp(remainingSlots, 0, zone.MaxCells);

            if (zone == targetZone)
                return unlockedForThisZone;

            remainingSlots -= unlockedForThisZone;
        }

        return 0;
    }
}