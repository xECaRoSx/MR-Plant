using System;
using System.Collections;
using UnityEngine;

public class PlantController : MonoBehaviour
{
    public PlantData plantData;
    public Animator animator;

    [Header("Transform Settings")]
    [SerializeField] private float scaleFactor = 5.5f; // Scale factor for the plant when selected
    [SerializeField] private float transitionDuration = 0.5f; // Duration for the transition animation
    [SerializeField] private Collider[] plantCollider;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private AnimatorOverrideController overrideController;
    private Coroutine moveRoutine;
    private bool isSelected = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        if (plantData.idleAnimation != null)
        {
            overrideController["Idle"] = plantData.idleAnimation;
            animator.Play("Idle");
            Debug.Log($"[Override] Idle clip set for {plantData.scientificName}: {plantData.idleAnimation.name}");
        }
        else
        {
            Debug.LogWarning($"[PlantController] {plantData.scientificName} has no idleClip assigned.");
        }

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
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
        PlantManager.Instance.ShowOnlySelectedPlant(this);
        UIManager.Instance.ShowPlantInfo(plantData, this);
        AudioManager.Instance.PlaySFX(plantData.plantSound);

        Vector3 targetPos = new Vector3(0, originalPosition.y, 0);
        Quaternion targetRot = Quaternion.identity;
        Vector3 targetScale = originalScale * scaleFactor;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SmoothTransform(targetPos, targetRot, targetScale));
    }

    public void OnDeselect()
    {
        Debug.Log($"Deselected: {plantData.scientificName}");
        isSelected = false;

        GameManager.Instance.SetState(GameState.PlantSelectionState);
        PlantManager.Instance.ShowAllPlants();
        UIManager.Instance.ShowSelectionScreen();

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SmoothTransform(originalPosition, originalRotation, originalScale));

        StopAnimation();
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
        Debug.Log($"[Override] {stateName} clip set for {plantData.scientificName}: {targetClip.name}");
        animator.Play(stateName);
        //AudioManager.Instance.PlaySFX(plantData.plantSound);

        Debug.Log($"[PlantController] Playing {plantData.scientificName} : {stateName}");
    }

    public void StopAnimation()
    {
        animator.Play("Idle");
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
            yield return null; // Wait for the next frame
        }
        
        transform.localPosition = targetPosition; // Ensure exact value at the end
        transform.localRotation = targetRotation;
        transform.localScale = targetScale;
    }
}
