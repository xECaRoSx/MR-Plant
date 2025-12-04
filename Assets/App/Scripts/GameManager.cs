using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    TitleScreenState,
    AnchoringState,
    PlantSelectionState,
    PlantInfoState,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    public EnableSeeThrough enableSeeThrough;

    public GameObject anchorRoot;

    [Header("Test Settings")]
    [SerializeField] private bool skipAnchoring = false;

    private bool hasPlayedSelectionVO = false;

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
        if (!skipAnchoring)
            SetState(GameState.TitleScreenState);
        else
            SetState(GameState.PlantSelectionState);
    }

    // =================== Button Functions: State Change ===================
    public void StartGame()
    {
        if (skipAnchoring)
        {
            SetState(GameState.PlantSelectionState);
            Debug.Log("Skipping Anchoring, starting game directly in Plant Selection");
        }
        else
        {
            SetState(GameState.AnchoringState);
            Debug.Log("Starting...");
        }
    }
    public void ConfirmButton()
    {
        AnchorManager.Instance.ConfirmAnchor();
        SetState(GameState.PlantSelectionState);
        Debug.Log("Anchor Confirmed");
    }
    public void ReturnToSelection()
    {
        SetState(GameState.PlantSelectionState);
        Debug.Log("Returning to Plant Selection");
    }
    public void ReturnToTitle()
    {
        SetState(GameState.TitleScreenState);
        Debug.Log("Returning to Title Screen");
    }
    public void QuitGame()
    {
        Debug.Log("Game Quit");
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
                UIManager.Instance.ShowSelectionScreen();
                PlantManager.Instance.ShowAllPlants();
                VFXManager.Instance.StopAllVFX();

                if (!hasPlayedSelectionVO)
                {
                    AudioManager.Instance.PlayVObyID("VO2");
                    hasPlayedSelectionVO = true;
                    Debug.Log("[GameManager] Playing first-time Selection VO");
                }
                break;
            case GameState.PlantInfoState:
                UIManager.Instance.ShowInformationScreen();
                VFXManager.Instance.PlayVFX(VFXTriggerType.OnEnterInfoState);
                break;
            default:
                Debug.LogWarning("Unhandled game state: " + newState);
                break;
        }
    }
}
