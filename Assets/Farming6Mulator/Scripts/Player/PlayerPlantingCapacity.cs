using System;
using UnityEngine;

public class PlayerPlantingCapacity : MonoBehaviour
{
    [SerializeField] private int maxPlantingSlots = 10;

    public int MaxPlantingSlots => maxPlantingSlots;
    public int UsedPlantingSlots { get; private set; }
    public int RemainingPlantingSlots => maxPlantingSlots - UsedPlantingSlots;

    public event Action CapacityChanged;

    private void Start()
    {
        CapacityChanged?.Invoke();
    }

    public bool HasFreeSlot()
    {
        return UsedPlantingSlots < maxPlantingSlots;
    }

    public bool TryUsePlantingSlot()
    {
        if (!HasFreeSlot())
            return false;

        UsedPlantingSlots++;
        CapacityChanged?.Invoke();
        return true;
    }

    public void ReleasePlantingSlot()
    {
        if (UsedPlantingSlots <= 0)
            return;

        UsedPlantingSlots--;
        CapacityChanged?.Invoke();
    }

    public void AddPlantingSlots(int amount)
    {
        if (amount <= 0)
            return;

        maxPlantingSlots += amount;
        CapacityChanged?.Invoke();
    }
}