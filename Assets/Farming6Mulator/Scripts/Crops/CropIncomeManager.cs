using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropIncomeManager : MonoBehaviour
{
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private float tickInterval = 1f;

    private readonly List<CropInstance> crops = new List<CropInstance>();

    private void OnEnable()
    {
        StartCoroutine(IncomeLoop());
    }

    public void RegisterCrop(CropInstance crop)
    {
        if (crop == null)
            return;

        if (!crops.Contains(crop))
            crops.Add(crop);
    }

    public void UnregisterCrop(CropInstance crop)
    {
        if (crop == null)
            return;

        crops.Remove(crop);
    }

    private IEnumerator IncomeLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(tickInterval);

        while (true)
        {
            yield return wait;

            float totalIncome = 0f;

            for (int i = crops.Count - 1; i >= 0; i--)
            {
                if (crops[i] == null)
                {
                    crops.RemoveAt(i);
                    continue;
                }

                totalIncome += crops[i].GetIncomeForSeconds(tickInterval);
            }

            if (wallet != null && totalIncome > 0f)
                wallet.AddMoney(totalIncome);
        }
    }
}