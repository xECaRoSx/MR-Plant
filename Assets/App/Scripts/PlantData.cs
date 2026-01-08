using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "MRPlant/PlantData")]
public class PlantData : ScriptableObject
{
    [Header("Informations")]
    public string thaiName;
    public string scientificName; 
    public string family;
    public ConservationStatus conservationStatus;

    [Header("Prefab & Animations")]
    public PlantController plantPrefab;

    public AnimationClip idleAnimation;
    public List<AnimationClip> animationList;

    [Header("Audio")]
    public AudioClip plantSound;
    public AudioClip plantInfoVO;

    [Header("Spawn Settings")]
    public bool alwaysSpawn = false;
}
public enum ConservationStatus
{
    NotAvailableNA,
    ExtinctEX,
    ExtinctInTheWildEW,
    CriticallyEndangeredCR,
    EndangeredEN,
    VulnerableVU,
    NearThreatenedNT,
    LeastConcernLC
}