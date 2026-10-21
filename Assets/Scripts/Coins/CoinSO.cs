using UnityEngine;

[CreateAssetMenu(fileName = "CoinSO", menuName = "Scriptable Objects/CoinSO")]
public class CoinSO : ScriptableObject
{
    [Header("Balance")]
    [Min(0)] [SerializeField] private int initialCoins;

    [Header("Rewards")]
    [Min(0)] [SerializeField] private int bossReward = 200;
    [Min(0)] [SerializeField] private int pyramidBaseReward = 100;
    [Min(0)] [SerializeField] private int finalChestReward = 200;

    [Header("Pyramid Multipliers")]
    [Min(0f)] [SerializeField] private float firstStepMultiplier = 1f;
    [Min(0f)] [SerializeField] private float finalStepMultiplier = 4f;

    [Header("Starting Runners Upgrade")]
    [Min(1)] [SerializeField] private int initialStartingRunners = 1;
    [Min(1)] [SerializeField] private int maxStartingRunners = 100;
    [Min(1)] [SerializeField] private int firstRunnerUpgradePrice = 100;
    [Min(1)] [SerializeField] private int finalRunnerUpgradePrice = 100000;

    public int InitialCoins => initialCoins;
    public int BossReward => bossReward;
    public int PyramidBaseReward => pyramidBaseReward;
    public int FinalChestReward => finalChestReward;
    public float FirstStepMultiplier => firstStepMultiplier;
    public float FinalStepMultiplier => finalStepMultiplier;
    public int InitialStartingRunners => initialStartingRunners;
    public int MaxStartingRunners => maxStartingRunners;
    public int FirstRunnerUpgradePrice => firstRunnerUpgradePrice;
    public int FinalRunnerUpgradePrice => finalRunnerUpgradePrice;
}
