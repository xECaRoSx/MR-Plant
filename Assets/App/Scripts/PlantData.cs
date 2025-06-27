using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "MRPlant/PlantData")]
public class PlantData : ScriptableObject
{
    public string thaiName;
    public string scientificName; 
    public string family;
    public ConservationStatus conservationStatus;

    public GameObject plantPrefab;

    public AnimationClip idleAnimation;
    public List<AnimationClip> animationList;

    public AudioClip plantSound;
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