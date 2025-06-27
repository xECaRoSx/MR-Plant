using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance { get; private set; }

    private PlantController[] plantLists;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        plantLists = FindObjectsOfType<PlantController>();
    }

    public void ShowOnlySelectedPlant(PlantController selected)
    {
        foreach (var plant in plantLists)
        {
            bool isSelected = plant == selected;
            plant.SetColliderActive(isSelected);
            plant.gameObject.SetActive(isSelected);

        }
    }

    public void ShowAllPlants()
    {
        foreach (var plant in plantLists)
        {
            plant.SetColliderActive(true);
            plant.gameObject.SetActive(true);
        }
    }

    public void HideAllPlants()
    {
        foreach (var plant in plantLists)
        {
            plant.SetColliderActive(false);
            plant.gameObject.SetActive(false);
        }
    }
}
