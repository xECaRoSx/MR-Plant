using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Canvas References")]
    public GameObject titleUI;
    public GameObject anchoringUI;
    public GameObject selectionUI;
    public GameObject informationUI;
    public GameObject statusUI;
    public GameObject resultUI;

    [Header("Plant Selection Panel")]
    public GameObject plantTooltip;
    public TextMeshProUGUI tooltipText;

    [Header("Plant Information Panel")]
    public TextMeshProUGUI plantNameText;
    public TextMeshProUGUI scientificNameText;
    public TextMeshProUGUI familyText;
    public GameObject[] conservationIcons;
    [Range(20, 40)] public int maxNameLength = 31;

    [Header("Status Panel")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Result Panel")]
    public TextMeshProUGUI resultText;

    private PlantController currentPlant;

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
        HideAllScreens();
        ShowTitleScreen();
    }

    // ======================== UI Screen Management ========================
    public void ShowTitleScreen() => ActivateOnly(titleUI);
    public void ShowAnchoringScreen() => ActivateOnly(anchoringUI);
    public void ShowSelectionScreen() => ActivateOnly(selectionUI);
    public void ShowInformationScreen() => ActivateOnly(informationUI);
    public void ShowResultScreen() => ActivateOnly(resultUI);
    private void ActivateOnly(GameObject screen)
    {
        HideAllScreens();
        if (screen != null) screen.SetActive(true);
    }

    private void HideAllScreens()
    {
        titleUI.SetActive(false);
        anchoringUI.SetActive(false);
        selectionUI.SetActive(false);
        informationUI.SetActive(false);
        statusUI.SetActive(false);
        resultUI.SetActive(false);
    }
    // ================== Status Panel Updates ==================
    public void UpdateScore(int score, int maxPlants)
    {
        scoreText.text = $"{score}/{maxPlants}";
    }
    public void UpdateTimer(float sec)
    {
        int m = Mathf.FloorToInt(sec / 60f);
        int s = Mathf.FloorToInt(sec % 60f);
        timerText.text = $"{m:D2}:{s:D2}";
    }
    public void UpdateResult(int score, int maxPlants)
    {
        resultText.text = $"{score}/{maxPlants}";
    }
    // ==================== Button Functions: UI Actions ====================
    public void AnimationButton(int actionIndex)
    {
        if (currentPlant != null) currentPlant.PlayAnimation(actionIndex);
    }

    public void SeasonButton(int actionIndex)
    {
        if (currentPlant != null) currentPlant.PlaySeasonEffect(actionIndex);
    }

    public void ReturnButton()
    {
        if (currentPlant != null)
        {
            Debug.Log($"Current Plant: {currentPlant}");
            currentPlant.OnDeselect();
            currentPlant = null;
        }
    }
    // =========================== Plant Tooltip ===========================
    public void ShowPlantTooltip(PlantData data, PlantController plant)
    {
        currentPlant = plant;
        Debug.Log($"Current Plant: {currentPlant}");
        plantTooltip.SetActive(true);
        tooltipText.text = data.thaiName;
    }

    // ========================= Plant Information =========================
    public void ShowPlantInfo(PlantData data, PlantController plant)
    {
        currentPlant = plant;
        plantTooltip.SetActive(false);
        plantNameText.text = data.thaiName;
        scientificNameText.text = $"<i>{data.scientificName}</i>";
        familyText.text = data.family;

        // ================== Conservation Icons Management =================
        int statusIndex = (int)data.conservationStatus;
        bool isNA = statusIndex == 0;

        for (int i = 0; i < conservationIcons.Length; i++)
        {
            conservationIcons[i].SetActive(isNA ? i == 0 : i != 0);
            conservationIcons[i].transform.localScale = Vector3.one;
        }

        if (!isNA && statusIndex < conservationIcons.Length)
            conservationIcons[statusIndex].transform.localScale = Vector3.one * 1.5f;
        else if (!isNA)
            Debug.LogWarning($"[UIManager] Unknown conservation status: {data.conservationStatus}");
    }
    // ======================================================================

    public void ShowPopup(string msg, System.Action onYes, System.Action onNo)
    {
        Debug.Log($"[UI Popup] {msg}  (auto YES for now)");

        onYes?.Invoke();
    }

    public void ShowConfirm(string msg, System.Action onConfirm)
    {
        Debug.Log($"[UI Confirm] {msg}  (auto Confirm for now)");

        onConfirm?.Invoke();
    }
}