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

    private MovementController movementController;
    private CrowdSystem crowdSystem;
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
        roulette = ServiceProvider.GetService<Roulette>();
        SetGameStatus(initialStatus);
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
                break;

            case GameStatus.GameOver:
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
                SetVictoryStatus();
                break;

            case GameStatus.GameOver:
                SetGameOverStatus();
                break;
        }
    }

    private void SetMenuStatus()
    {
        movementController.SetSpeed(0f);
    }

    private void SetGameplayStatus()
    {
        Time.timeScale = 1f;
        movementController.RestoreRegularSpeed();
        var runners = crowdSystem.GetRunners();

        for (int i = 0; i < runners.Count; i++)
        {
            runners[i].SetAnimTrigger("Run");
        }
    }

    private void SetVictoryStatus()
    {
    }

    private void SetGameOverStatus()
    {
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
            movementController.SetInputStartPosition();
            SetGameStatus(GameStatus.Gameplay);
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
}
