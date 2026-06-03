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

    [Header("World Position")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    private CropInstance currentCrop;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSelectedCrop);

        Hide();
    }

    private void LateUpdate()
    {
        if (currentCrop == null || panel == null || !panel.activeSelf)
            return;

        transform.position = currentCrop.transform.position + worldOffset;

        if (playerCamera == null)
            return;

        Vector3 direction = transform.position - playerCamera.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    public void Show(CropInstance crop)
    {
        if (crop == null || crop.Data == null)
            return;

        currentCrop = crop;

        if (panel != null)
            panel.SetActive(true);

        if (cropNameText != null)
            cropNameText.text = crop.Data.cropName;

        if (incomeText != null)
            incomeText.text = "Income: $" + crop.Data.incomePerSecond.ToString("0.##") + "/sec";

        if (refundText != null)
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