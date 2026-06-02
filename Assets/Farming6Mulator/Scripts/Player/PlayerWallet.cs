using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private float startMoney = 100f;

    public float Money { get; private set; }

    public event Action<float> MoneyChanged;

    private void Awake()
    {
        Money = startMoney;
    }

    private void Start()
    {
        MoneyChanged?.Invoke(Money);
    }

    public bool CanAfford(float amount)
    {
        return Money >= amount;
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f)
            return false;

        if (!CanAfford(amount))
            return false;

        Money -= amount;
        MoneyChanged?.Invoke(Money);
        return true;
    }

    public void AddMoney(float amount)
    {
        if (amount <= 0f)
            return;

        Money += amount;
        MoneyChanged?.Invoke(Money);
    }
}