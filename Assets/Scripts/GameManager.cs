using UnityEngine;

public enum GameStatus
{
    Menu,
    Gameplay,
    Pause,
    Roullete,
    Victory,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameStatus initialStatus = GameStatus.Menu;
    [SceneDropdown] [SerializeField] private string sceneName;

    [Header("End Status UI")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;

    private MovementController movementController;
    private CrowdSystem crowdSystem;
    private CrowdCount crowdCount;
    private Roulette roulette;

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
    }

    private void Start()
    {
        movementController = ServiceProvider.GetService<MovementController>();
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        crowdCount = ServiceProvider.GetService<CrowdCount>();
        roulette = ServiceProvider.GetService<Roulette>();
        SetGameStatus(initialStatus);
        victoryUI.SetActive(false);
        gameOverUI.SetActive(false);
    }

    private void Update()
    {
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

    public void SetGameStatus(GameStatus newStatus)
    {
        currentStatus = newStatus;

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
        movementController.SetSpeed(0f);
    }

    private void SetGameplayStatus()
    {
        Time.timeScale = 1f;
        movementController.SetInputStartPosition();
        movementController.RestoreRegularSpeed();
        var runners = crowdSystem.GetRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].SetAnimTrigger("Run");
        }
    }

    public void EnterVictoryStatus()
    {
        victoryUI.SetActive(true);
        crowdCount.gameObject.SetActive(false);
        crowdSystem.SetVictoryAnimation();
    }

    private void EnterGameOverStatus()
    {
        gameOverUI.SetActive(true);

        var enemies = Enemy.ActiveEnemies;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].Victory();
        }
    }

    private void SetPauseStatus()
    {
    }

    private void SetRoulleteStatus()
    {
        Time.timeScale = 0f;
        movementController.SetSpeed(0f);
        roulette.StartRoulette();
    }

    private void UpdateMenuStatus()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartRangedAttackTimers();
            SetGameStatus(GameStatus.Gameplay);
        }
    }

    private void StartRangedAttackTimers()
    {
        RangedAttack[] rangedAttacks = FindObjectsByType<RangedAttack>(FindObjectsSortMode.None);

        for (int i = 0; i < rangedAttacks.Length; i++)
        {
            rangedAttacks[i].StartShootActivationTimer();
        }
    }

    private void UpdateGameplayStatus()
    {
        movementController.CustomUpdate();
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

    public void ResetGame()
    {
        Time.timeScale = 1f;
        ServiceProvider.ClearServices();
        Instance = null;
        Destroy(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
