using System;
using System.Collections.Generic;
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

    [Header("Input")]
    [SerializeField] private InputActionReference plantAction;
    [SerializeField] private InputActionReference toggleInventoryAction;
    [SerializeField] private InputActionReference selectCropAction;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask plantingZoneMask;
    [SerializeField] private LayerMask cropMask;

    private CropData selectedCrop;
    private int selectedIndex;
    private float nextSelectionTime;

    private GameObject phantomInstance;
    private CropData phantomCrop;

    public event Action<CropData> SelectedCropChanged;

    private void OnEnable()
    {
        if (plantAction != null)
            plantAction.action.performed += OnPlantAction;

        if (toggleInventoryAction != null)
            toggleInventoryAction.action.performed += OnToggleInventoryAction;
    }

    private void OnDisable()
    {
        if (plantAction != null)
            plantAction.action.performed -= OnPlantAction;

        if (toggleInventoryAction != null)
            toggleInventoryAction.action.performed -= OnToggleInventoryAction;
    }

    private void Update()
    {
        HandleCropSelection();
        UpdatePhantom();
    }

    private void OnPlantAction(InputAction.CallbackContext context)
    {
        TryInteract();
    }

    private void OnToggleInventoryAction(InputAction.CallbackContext context)
    {
        if (inventoryPanel == null)
            return;

        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    private void HandleCropSelection()
    {
        if (inventory == null || selectCropAction == null)
            return;

        List<CropData> crops = inventory.GetOwnedCrops();

        if (crops.Count == 0)
        {
            SetSelectedCrop(null);
            return;
        }

        if (selectedCrop == null || !crops.Contains(selectedCrop))
        {
            selectedIndex = 0;
            SetSelectedCrop(crops[selectedIndex]);
            return;
        }

        Vector2 input = selectCropAction.action.ReadValue<Vector2>();

        if (Time.time < nextSelectionTime)
            return;

        if (input.x > 0.6f)
        {
            selectedIndex++;
            if (selectedIndex >= crops.Count)
                selectedIndex = 0;

            SetSelectedCrop(crops[selectedIndex]);
            nextSelectionTime = Time.time + 0.25f;
        }
        else if (input.x < -0.6f)
        {
            selectedIndex--;
            if (selectedIndex < 0)
                selectedIndex = crops.Count - 1;

            SetSelectedCrop(crops[selectedIndex]);
            nextSelectionTime = Time.time + 0.25f;
        }
    }

    private void SetSelectedCrop(CropData crop)
    {
        if (selectedCrop == crop)
            return;

        selectedCrop = crop;

        if (playerHUD != null)
            playerHUD.SetSelectedCrop(selectedCrop);

        SelectedCropChanged?.Invoke(selectedCrop);
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
            return;

        if (!plantingCapacity.HasFreeSlot())
            return;

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, plantingZoneMask))
            return;

        PlantingZone zone = hit.collider.GetComponentInParent<PlantingZone>();

        if (zone == null)
            return;

        if (!zone.CanPlant(hit.point))
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
            Quaternion.identity,
            incomeManager,
            wallet,
            plantingCapacity
        );

        if (crop == null)
        {
            plantingCapacity.ReleasePlantingSlot();
            inventory.Add(selectedCrop, 1);
            return;
        }

        incomeManager.RegisterCrop(crop);
    }

    private void UpdatePhantom()
    {
        if (pointerOrigin == null || selectedCrop == null || inventory == null)
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

        if (zone == null || !zone.CanPlant(hit.point))
        {
            ClearPhantom();
            return;
        }

        EnsurePhantom(selectedCrop);

        if (phantomInstance == null)
            return;

        phantomInstance.SetActive(true);
        phantomInstance.transform.position = hit.point;
        phantomInstance.transform.rotation = Quaternion.identity;
    }

    private void EnsurePhantom(CropData crop)
    {
        if (crop == null)
            return;

        if (phantomInstance != null && phantomCrop == crop)
            return;

        ClearPhantom();

        GameObject prefab = crop.phantomPrefab != null ? crop.phantomPrefab : crop.plantedPrefab;

        if (prefab == null)
            return;

        phantomInstance = Instantiate(prefab);
        phantomCrop = crop;

        Collider[] colliders = phantomInstance.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
            col.enabled = false;
    }

    private void ClearPhantom()
    {
        if (phantomInstance != null)
            Destroy(phantomInstance);

        phantomInstance = null;
        phantomCrop = null;
    }
}