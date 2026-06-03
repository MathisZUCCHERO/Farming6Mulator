using UnityEngine;

public class InventoryWorldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform playerCamera;

    [Header("Placement")]
    [SerializeField] private float distanceFromCamera = 1.5f;
    [SerializeField] private float heightOffset = -0.05f;
    [SerializeField] private float extraYawRotation = 180f;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Close();
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        PlaceInFrontOfPlayer();

        if (panel != null)
            panel.SetActive(true);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void PlaceInFrontOfPlayer()
    {
        if (playerCamera == null)
            return;

        Vector3 forward = playerCamera.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = playerCamera.transform.forward;

        forward.Normalize();

        transform.position =
            playerCamera.position +
            forward * distanceFromCamera +
            Vector3.up * heightOffset;

        Vector3 lookPosition = playerCamera.position;
        lookPosition.y = transform.position.y;

        transform.LookAt(lookPosition);
        transform.Rotate(0f, extraYawRotation, 0f);
    }
}