using System;
using System.Collections;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    public PlantData plantData;
    public Animator animator;

    [Header("Transform Settings")]
    [SerializeField] private float scaleFactor = 5.5f;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Collider[] plantCollider;

    [Header("Fade In Materials")]
    public Material[] fadeMaterials;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float fadeDelay = 1f;

    [Header("VFX + Animation")]
    public GameObject summerVFX;
    public GameObject rainyVFX;
    public GameObject winterVFX;

    [Header("VO Explanations")]
    public AudioClip summerVO;
    public AudioClip rainyVO;
    public AudioClip winterVO;
    public AudioClip noVFX_VO;

    private GameObject[] seasonEffects;
    private AudioClip[] seasonVOs; 

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private AnimatorOverrideController overrideController;
    private Coroutine moveRoutine;
    private bool isSelected = false;
    private bool hasFadeInPlayed = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        if (plantData.idleAnimation != null)
        {
            overrideController["Idle"] = plantData.idleAnimation;
            animator.Play("Idle");
        }
        seasonEffects = new GameObject[] { summerVFX, rainyVFX, winterVFX };
        seasonVOs = new AudioClip[] { summerVO, rainyVO, winterVO };
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    // ==================== XR Interaction Event Hooks =====================
    public void OnFocus()
    {
        if (GameManager.Instance.CurrentState != GameState.PlantSelectionState) return; 
        UIManager.Instance.ShowPlantTooltip(plantData, this);
    }

    public void OnSelected()
    {
        if (isSelected) return;

        Debug.Log($"[Select] {plantData.scientificName}");
        isSelected = true;

        GameManager.Instance.SetState(GameState.PlantInfoState);
        GameManager.Instance.OnPlantFound(this);
        PlantManager.Instance.ShowOnlySelectedPlant(this);
        UIManager.Instance.ShowPlantInfo(plantData, this);

        if (plantData.plantSound != null)
            AudioManager.Instance.PlaySFX(plantData.plantSound);
        if (plantData.plantInfoVO != null)
            AudioManager.Instance.PlayVObyClip(plantData.plantInfoVO);

        Vector3 targetPos = new Vector3(0, originalPosition.y, 0);
        Quaternion targetRot = Quaternion.identity;
        Vector3 targetScale = originalScale * scaleFactor;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SmoothTransform(targetPos, targetRot, targetScale));

        PlayAnimation(0);
    }

    public void OnDeselect()
    {
        Debug.Log($"Deselected: {plantData.scientificName}");
        isSelected = false;

        GameManager.Instance.SetState(GameState.PlantSelectionState);
        UIManager.Instance.ShowSelectionScreen();
        UIManager.Instance.statusUI.SetActive(true);

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SmoothTransform(originalPosition, originalRotation, originalScale));

        StopAnimation();
        StopAllEffects();
    }

    // ===================== Animation Management =========================
    public void PlayAnimation(int actionIndex)
    {
        if (plantData.animationList == null || actionIndex < 0 || actionIndex >= plantData.animationList.Count)
        {
            Debug.LogWarning($"[PlantController] Invalid animation index {actionIndex} for {plantData.scientificName}");
            return;
        }

        AnimationClip targetClip = plantData.animationList[actionIndex];
        if (targetClip == null)
        {
            Debug.LogWarning($"[PlantController] Animation at index {actionIndex} is null for {plantData.scientificName}");
            return;
        }

        string stateName = $"Action{actionIndex + 1}";
        overrideController[stateName] = targetClip;
        animator.Play(stateName);

        if (plantData.plantSound != null)
            AudioManager.Instance.PlaySFX(plantData.plantSound);

        VFXManager.Instance.PlayVFX(VFXTriggerType.OnPlayAnimation);
        Debug.Log($"[PlantController] Playing {plantData.scientificName} : {stateName}");
    }

    public void StopAnimation()
    {
        animator.Play("Idle");
    }

    public void PlaySeasonEffect(int seasonIndex)
    {
        StopAllEffects();

        if (seasonIndex < 0 || seasonIndex > 2)
        {
            Debug.LogWarning("Invalid season index");
            return;
        }

        GameObject effect = seasonEffects[seasonIndex];
        AudioClip vo = seasonVOs[seasonIndex];

        if (effect != null)
        {
            effect.SetActive(true);

            Animator fxAnimator = effect.GetComponent<Animator>();
            if (fxAnimator != null)
                fxAnimator.Play("Play");
        }
        else
        {
            if (noVFX_VO != null)
                AudioManager.Instance.PlayVObyClip(noVFX_VO);
        }

        if (vo != null)
        {
            AudioManager.Instance.PlayVObyClip(vo);
        }
        
    }

    public void StopAllEffects()
    {
        foreach (GameObject fx in seasonEffects)
        {
            if (fx != null)
                fx.SetActive(false);
        }
    }

    // =================== Collider Management Helper ======================
    public void SetColliderActive(bool isActive)
    {
        foreach (var col in plantCollider)
        {
            if (col != null) col.enabled = isActive;
        }
    }

    // ======================= Transform Tweening ==========================
    private IEnumerator SmoothTransform(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, bool returnToSelection = false)
    {
        float elapsedTime = 0f;

        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            transform.localPosition = Vector3.Lerp(startPos, targetPosition, t);
            transform.localRotation = Quaternion.Slerp(startRot, targetRotation, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localRotation = targetRotation;
        transform.localScale = targetScale;
    }

    public void SetOriginalTransform(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        originalPosition = pos;
        originalRotation = rot;
        originalScale = scale;
    }
    public void StartFadeIn()
    {
        if (hasFadeInPlayed) return;
        if (fadeMaterials == null || fadeMaterials.Length == 0)
            return;

        hasFadeInPlayed = true;
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float time = 0f;

        foreach (var mat in fadeMaterials)
        {
            if (mat == null) continue;

            SetMaterialTransparent(mat);

            Color c = mat.color;
            c.a = 0f;
            mat.color = c;
        }
        if (fadeDelay > 0f)
            yield return new WaitForSeconds(fadeDelay);

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            foreach (var mat in fadeMaterials)
            {
                if (mat == null) continue;

                Color c = mat.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                mat.color = c;
            }

            time += Time.deltaTime;
            yield return null;
        }

        foreach (var mat in fadeMaterials)
        {
            if (mat == null) continue;

            Color c = mat.color;
            c.a = 1f;
            mat.color = c;

            SetMaterialOpaque(mat);
        }
    }

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private void SetMaterialOpaque(Material mat)
    {
        mat.SetFloat("_Surface", 0); // Opaque
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = -1;
    }
}
