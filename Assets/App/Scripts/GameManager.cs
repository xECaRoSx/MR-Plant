using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    TitleScreenState,
    AnchoringState,
    PlantSelectionState,
    PlantInfoState,
    ResultState
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    public EnableSeeThrough enableSeeThrough;
    public GameObject anchorRoot;

    [Header("Game Settings")]
    public bool useRandomPlants = true;
    private bool hasPlayedSelectionVO = false;

    [Header("Time Settings")]
    public float playTime = 120f;

    [Header("Auto Pick Settings")]
    public float autoPickDelay = 30f;

    private float autoPickTimer = 0f;
    private bool autoPickRunning = false;

    private int score = 0;
    private int maxPlants = 0;
    private float timer = 0f;
    private bool timerRunning = false;
    private HashSet<PlantController> foundPlants = new HashSet<PlantController>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.TitleScreenState);
    }

    private void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;
            UIManager.Instance.UpdateTimer(timer);
            if (timer <= 0f)
            {
                timer = 0;
                timerRunning = false;
                EndGame();
            }
        }
        if (autoPickRunning && CurrentState == GameState.PlantSelectionState)
        {
            autoPickTimer -= Time.deltaTime;

            if (autoPickTimer <= 0f)
            {
                autoPickRunning = false;
                AutoPickPlant();
            }
        }
    }

    // =================== Button Functions: State Change ===================
    public void StartGame()
    {
        SetState(GameState.AnchoringState);
        Debug.Log("[GameManager] StartGame pressed -> Entering AnchoringState");
    }

    public void ConfirmButton()
    {
        AnchorManager.Instance.ConfirmAnchor();
        SetState(GameState.PlantSelectionState);
        Debug.Log("[GameManager] Anchor confirmed -> Entering PlantSelectionState");
    }
    public void ReturnToSelection()
    {
        SetState(GameState.PlantSelectionState);
        Debug.Log("[GameManager] Returning to PlantSelectionState");
    }
    public void ReturnToTitle()
    {
        SetState(GameState.TitleScreenState);
        Debug.Log("[GameManager] Returning to TitleScreenState");
    }
    public void EndGame()
    {
        Debug.Log("[GameManager] Time Out -> ResultState");
        UIManager.Instance.UpdateResult(score, maxPlants);
        SetState(GameState.ResultState);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void QuitGame()
    {
        Debug.Log("[GameManager] QuitGame called");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID || UNITY_STANDALONE
    Application.Quit();
#endif
    }
    // ======================================================================

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] GameState changed to: {newState}");

        switch (newState)
        {
            case GameState.TitleScreenState:
                UIManager.Instance.ShowTitleScreen();
                PlantManager.Instance.HideAllPlants();
                anchorRoot.SetActive(false);
                break;

            case GameState.AnchoringState:
                UIManager.Instance.ShowAnchoringScreen();
                AudioManager.Instance.PlayVObyID("VO1");
                AnchorManager.Instance.EnablePreview(true);
                enableSeeThrough.SeeThroughOn();
                break;

            case GameState.PlantSelectionState:
                anchorRoot.SetActive(true);
                PlantManager.Instance.SpawnPlants();
                UIManager.Instance.ShowSelectionScreen();
                UIManager.Instance.statusUI.SetActive(true);
                VFXManager.Instance.StopAllVFX();

                if (!hasPlayedSelectionVO)
                {
                    AudioManager.Instance.PlayVObyID("VO2");
                    hasPlayedSelectionVO = true;
                    Debug.Log("[GameManager] Playing first-time Selection VO");

                    timer = playTime;
                    timerRunning = true;
                }
                break;

            case GameState.PlantInfoState:
                UIManager.Instance.ShowInformationScreen();
                UIManager.Instance.statusUI.SetActive(true);
                VFXManager.Instance.PlayVFX(VFXTriggerType.OnEnterInfoState);
                break;

            case GameState.ResultState:
                UIManager.Instance.ShowResultScreen();
                anchorRoot.SetActive(false);
                break;

            default:
                Debug.LogWarning("Unhandled game state: " + newState);
                break;
        }
    }
    // ======================================================================
    public void SetMaxPlants(int count)
    {
        maxPlants = count;
        UIManager.Instance.UpdateScore(score, maxPlants);
        Debug.Log($"[GameManager] maxPlants set to {maxPlants}");
    }

    public void OnPlantFound(PlantController plant)
    {
        if (!foundPlants.Contains(plant))
        {
            foundPlants.Add(plant);
            score++;
            UIManager.Instance.UpdateScore(score, maxPlants);
        }
        StopAutoPickTimer();
    }
    private void StartAutoPickTimer()
    {
        autoPickTimer = autoPickDelay;
        autoPickRunning = true;
    }
    private void StopAutoPickTimer()
    {
        autoPickRunning = false;
    }
    private void AutoPickPlant()
    {
        Debug.Log("[GameManager] Auto Pick Triggered!");

        PlantController target = PlantManager.Instance.GetRandomUnfoundPlant();

        if (target != null)
        {
            target.OnSelected();
        }
        else
        {
            Debug.Log("[GameManager] No plant left to auto pick.");
        }
    }
}