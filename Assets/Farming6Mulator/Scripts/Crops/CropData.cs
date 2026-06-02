using UnityEngine;

[CreateAssetMenu(fileName = "NewCropData", menuName = "Farming6Mulator/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName = "Crop";

    public GameObject plantedPrefab;
    public GameObject phantomPrefab;
    public Sprite icon;

    public float buyPrice = 10f;
    public float sellRefund = 5f;

    public int inventorySlotsUsed = 1;

    public float incomePerSecond = 2f;
}