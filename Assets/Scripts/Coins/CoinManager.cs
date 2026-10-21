using System;
using UnityEngine;

public class CoinManager
{
    private const string CoinsPlayerPrefsKey = "Coins";
    private const string StartingRunnersPlayerPrefsKey = "StartingRunners";

    private readonly CoinSO coinSettings;

    public int Coins { get; private set; }
    public int EarnedCoins { get; private set; }
    public int StartingRunners { get; private set; }
    public int MaxStartingRunners => coinSettings.MaxStartingRunners;

    public event Action<int> CoinsChanged;
    public event Action<int> EarnedCoinsChanged;

    public CoinManager(CoinSO settings)
    {
        coinSettings = settings;
        LoadCoins();
        LoadStartingRunners();
    }

    public void AddCoins(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Coins += amount;
        EarnedCoins += amount;
        SaveCoins();
        CoinsChanged?.Invoke(Coins);
        EarnedCoinsChanged?.Invoke(EarnedCoins);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (amount > Coins)
        {
            return false;
        }

        Coins -= amount;
        SaveCoins();
        CoinsChanged?.Invoke(Coins);
        return true;
    }

    public void AddBossReward()
    {
        AddCoins(coinSettings.BossReward);
    }

    public void AddPyramidReward(int stepIndex, int stepAmount)
    {
        AddCoins(CalculatePyramidReward(stepIndex, stepAmount));
    }

    public void AddFinalChestReward()
    {
        AddCoins(coinSettings.FinalChestReward);
    }

    public bool TryBuyStartingRunner()
    {
        if (StartingRunners >= coinSettings.MaxStartingRunners)
        {
            return false;
        }

        if (!TrySpendCoins(GetStartingRunnerUpgradePrice()))
        {
            return false;
        }

        StartingRunners++;
        SaveStartingRunners();
        return true;
    }

    public int GetStartingRunnerUpgradePrice()
    {
        if (StartingRunners >= coinSettings.MaxStartingRunners)
        {
            return 0;
        }

        int upgradeAmount = coinSettings.MaxStartingRunners - coinSettings.InitialStartingRunners;
        int upgradeIndex = StartingRunners - coinSettings.InitialStartingRunners;

        if (upgradeIndex == upgradeAmount - 1)
        {
            return coinSettings.FinalRunnerUpgradePrice;
        }

        float progress = upgradeIndex / (float)(upgradeAmount - 1);
        float priceMultiplier = coinSettings.FinalRunnerUpgradePrice /
                                (float)coinSettings.FirstRunnerUpgradePrice;
        float rawPrice = coinSettings.FirstRunnerUpgradePrice * Mathf.Pow(priceMultiplier, progress);
        return Mathf.CeilToInt(rawPrice / 10f) * 10;
    }

    public int CalculatePyramidReward(int stepIndex, int stepAmount)
    {
        float multiplier = GetPyramidMultiplier(stepIndex, stepAmount);
        return Mathf.RoundToInt(coinSettings.PyramidBaseReward * multiplier);
    }

    public float GetPyramidMultiplier(int stepIndex, int stepAmount)
    {
        if (stepAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stepAmount));
        }

        if (stepIndex < 0 || stepIndex >= stepAmount)
        {
            throw new ArgumentOutOfRangeException(nameof(stepIndex));
        }

        int firstMultiplierInTenths = Mathf.RoundToInt(coinSettings.FirstStepMultiplier * 10f);
        int finalMultiplierInTenths = Mathf.RoundToInt(coinSettings.FinalStepMultiplier * 10f);
        int availableMultiplierValues = finalMultiplierInTenths - firstMultiplierInTenths + 1;

        if (stepAmount > availableMultiplierValues)
        {
            throw new InvalidOperationException(
                $"Cannot assign {stepAmount} unique pyramid multipliers between " +
                $"x{coinSettings.FirstStepMultiplier:0.0} and x{coinSettings.FinalStepMultiplier:0.0}.");
        }

        if (stepIndex == 0)
        {
            return firstMultiplierInTenths / 10f;
        }

        int skippedLowValues = availableMultiplierValues - stepAmount;
        int multiplierInTenths = firstMultiplierInTenths + skippedLowValues + stepIndex;
        return multiplierInTenths / 10f;
    }

    private void LoadCoins()
    {
        Coins = PlayerPrefs.GetInt(CoinsPlayerPrefsKey, coinSettings.InitialCoins);
    }

    private void LoadStartingRunners()
    {
        StartingRunners = Mathf.Clamp(
            PlayerPrefs.GetInt(StartingRunnersPlayerPrefsKey, coinSettings.InitialStartingRunners),
            coinSettings.InitialStartingRunners,
            coinSettings.MaxStartingRunners);
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsPlayerPrefsKey, Coins);
        PlayerPrefs.Save();
    }

    private void SaveStartingRunners()
    {
        PlayerPrefs.SetInt(StartingRunnersPlayerPrefsKey, StartingRunners);
        PlayerPrefs.Save();
    }
}
