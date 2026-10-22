using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum GameStatus
{
    Menu,
    Gameplay,
    Pause,
    Roullete,
    Victory,
    GameOver
}

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameStatus initialStatus = GameStatus.Menu;
    [SceneDropdown] [SerializeField] private string sceneName;

    [Header("Audio")]
    [SerializeField] private AudioContainer audioContainer;
    [SerializeField] private AudioSource loopingSfxSource;
    [SerializeField] private AudioSource sfxSource;
    [Min(0)] [SerializeField] private int victorySfxIndex;
    [Min(0)] [SerializeField] private int gameOverSfxIndex;
    [Min(0)] [SerializeField] private int runnerUpgradeSfxIndex;

    [Header("Coins")]
    [SerializeField] private CoinSO coinSettings;

    [Header("Menu UI")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private TextMeshProUGUI menuCoinsText;
    [SerializeField] private Button UpgradeButton;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private Image CoinUpgrade;

    [Header("End Status UI")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private TextMeshProUGUI victoryCoinsText;
    [SerializeField] private GameObject gameOverUI;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject runProgressUI;

    private MovementController movementController;
    private CrowdSystem crowdSystem;
    private CrowdCount crowdCount;
    private Roulette roulette;
    private RunProgress runProgress;
    private AudioManager audioManager;
    private CoinManager coinManager;
    private DetectionManager detectionManager;

    public GameStatus currentStatus { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        crowdSystem = FindAnyObjectByType<CrowdSystem>();
        crowdSystem.CustomAwake();

        audioManager = new AudioManager(audioContainer);
        audioManager.Init(loopingSfxSource, sfxSource);
        ServiceProvider.RegisterService(audioManager);

        coinManager = new CoinManager(coinSettings);
        ServiceProvider.RegisterService(coinManager);
        coinManager.CoinsChanged += UpdateMenuCoinsText;
        coinManager.EarnedCoinsChanged += UpdateVictoryCoinsText;
        UpdateMenuCoinsText(coinManager.Coins);
        UpdateVictoryCoinsText(coinManager.EarnedCoins);

        detectionManager = new DetectionManager();
        detectionManager.CustomAwake();
    }

    private void Start()
    {
        crowdSystem.CustomStart();
        movementController = ServiceProvider.GetService<MovementController>();
        crowdCount = ServiceProvider.GetService<CrowdCount>();
        roulette = ServiceProvider.GetService<Roulette>();
        detectionManager.CustomStart();
        crowdSystem.SetStartingRunnersAmount(coinManager.StartingRunners);
        UpgradeButton.onClick.AddListener(BuyStartingRunner);
        UpdateUpgradeButton();
        SetGameStatus(initialStatus);
        victoryUI.SetActive(false);
        gameOverUI.SetActive(false);
    }

    private void Update()
    {
        crowdSystem.CustomUpdate();

        switch (currentStatus)
        {
            case GameStatus.Menu:
                UpdateMenuStatus();
                break;

            case GameStatus.Gameplay:
                UpdateGameplayStatus();
                break;

            case GameStatus.Pause:
                UpdatePauseStatus();
                break;

            case GameStatus.Roullete:
                UpdateRoulleteStatus();
                break;

            case GameStatus.Victory:
            case GameStatus.GameOver:
                UpdateEndStatus();
                break;
        }
    }

    private void LateUpdate()
    {
        crowdSystem.CustomLateUpdate();
    }

    public void SetGameStatus(GameStatus newStatus)
    {
        currentStatus = newStatus;
        menuUI.SetActive(currentStatus == GameStatus.Menu);

        switch (currentStatus)
        {
            case GameStatus.Menu:
                SetMenuStatus();
                break;

            case GameStatus.Gameplay:
                SetGameplayStatus();
                break;

            case GameStatus.Pause:
                SetPauseStatus();
                break;

            case GameStatus.Roullete:
                SetRoulleteStatus();
                break;

            case GameStatus.Victory:
                EnterVictoryStatus();
                break;

            case GameStatus.GameOver:
                EnterGameOverStatus();
                break;
        }
    }

    public void SetVictoryStatus()
    {
        SetGameStatus(GameStatus.Victory);
    }

    public void SetGameOverStatus()
    {
        SetGameStatus(GameStatus.GameOver);
    }

    private void SetMenuStatus()
    {
        runProgressUI.SetActive(false);
        crowdSystem.StopBehaviourSFX();
        movementController.SetSpeed(0f);
    }

    private void SetGameplayStatus()
    {
        Time.timeScale = 1f;
        runProgressUI.SetActive(true);
        runProgress = ServiceProvider.GetService<RunProgress>();
        runProgress.CustomStart();
        movementController.SetInputStartPosition();
        movementController.RestoreRegularSpeed();
        crowdSystem.PlayBehaviourSFX();
        var runners = crowdSystem.GetRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].SetAnimTrigger("Run");
        }
    }

    public void EnterVictoryStatus()
    {
        runProgressUI.SetActive(false);
        crowdSystem.StopBehaviourSFX();
        audioManager.PlaySFX(victorySfxIndex);
        victoryUI.SetActive(true);
        crowdCount.SetActive(false);
        crowdSystem.SetVictoryAnimation();
    }

    private void EnterGameOverStatus()
    {
        runProgressUI.SetActive(false);
        crowdSystem.StopBehaviourSFX();
        audioManager.PlaySFX(gameOverSfxIndex);
        gameOverUI.SetActive(true);

        var enemies = Enemy.ActiveEnemies;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].Victory();
        }
    }

    private void SetPauseStatus()
    {
        runProgressUI.SetActive(false);
        crowdSystem.StopBehaviourSFX();
    }

    private void SetRoulleteStatus()
    {
        runProgressUI.SetActive(false);
        crowdSystem.StopBehaviourSFX();
        Time.timeScale = 0f;
        movementController.SetSpeed(0f);
        roulette.StartRoulette();
    }

    private void UpdateMenuStatus()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUpgradeButton())
        {
            StartRangedAttackTimers();
            SetGameStatus(GameStatus.Gameplay);
        }
    }

    private bool IsPointerOverUpgradeButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
        {
            Transform hitTransform = raycastResults[i].gameObject.transform;

            if (hitTransform == UpgradeButton.transform || hitTransform.IsChildOf(UpgradeButton.transform))
            {
                return true;
            }
        }

        return false;
    }

    private void BuyStartingRunner()
    {
        if (!coinManager.TryBuyStartingRunner())
        {
            return;
        }

        audioManager.PlaySFX(runnerUpgradeSfxIndex);
        crowdSystem.SetStartingRunnersAmount(coinManager.StartingRunners);
        UpdateUpgradeButton();
    }

    private void UpdateUpgradeButton()
    {
        bool reachedMaximum = coinManager.StartingRunners >= coinManager.MaxStartingRunners;
        UpgradeButton.interactable = !reachedMaximum;
        CoinUpgrade.gameObject.SetActive(!reachedMaximum);
        upgradePriceText.text = reachedMaximum
            ? "MAX"
            : coinManager.GetStartingRunnerUpgradePrice().ToString();
    }

    private void StartRangedAttackTimers()
    {
        RangedAttack[] rangedAttacks = FindObjectsByType<RangedAttack>(FindObjectsInactive.Exclude);

        for (int i = 0; i < rangedAttacks.Length; i++)
        {
            rangedAttacks[i].StartShootActivationTimer();
        }
    }

    private void UpdateGameplayStatus()
    {
        movementController.CustomUpdate();
        detectionManager.CustomUpdate();
        runProgress.CustomUpdate();
    }

    private void UpdatePauseStatus()
    {

    }

    private void UpdateRoulleteStatus()
    {
        roulette.CustomUpdate();
    }

    private void UpdateEndStatus()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResetGame();
        }
    }

    private void UpdateMenuCoinsText(int amount)
    {
        menuCoinsText.text = amount.ToString();
    }

    private void UpdateVictoryCoinsText(int amount)
    {
        victoryCoinsText.text = $"+{amount}";
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        crowdSystem.CustomDestroy();
        ServiceProvider.ClearServices();
        Instance = null;
        Destroy(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
