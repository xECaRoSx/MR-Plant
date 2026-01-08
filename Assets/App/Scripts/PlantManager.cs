using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public bool randomPlants = false;
    public int spawnCount = 9;
    public List<Transform> spawnPoints;

    private PlantController[] allPlants;
    private List<PlantController> plantsToSpawn = new List<PlantController>();

    private bool hasSpawned = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        allPlants = FindObjectsOfType<PlantController>();

        foreach (var p in allPlants)
            p.gameObject.SetActive(false);
    }

    public void SpawnPlants()
    {
        if (!hasSpawned)
        {
            plantsToSpawn.Clear();

            foreach (var a in allPlants)
            {
                if (a.plantData != null && a.plantData.alwaysSpawn)
                    plantsToSpawn.Add(a);
            }

            if (plantsToSpawn.Count > spawnCount)
            {
                Debug.LogError("[PlantManager] alwaysSpawn is more than spawnCount!");
                return;
            }

            if (randomPlants)
            {
                List<PlantController> selectable = new List<PlantController>();

                foreach (var a in allPlants)
                {
                    if (!plantsToSpawn.Contains(a))
                        selectable.Add(a);
                }

                int needed = spawnCount - plantsToSpawn.Count;

                if (needed > selectable.Count)
                    needed = selectable.Count;

                for (int i = 0; i < needed; i++)
                {
                    int r = Random.Range(0, selectable.Count);
                    plantsToSpawn.Add(selectable[r]);
                    selectable.RemoveAt(r);
                }
            }

            PlacePlantsAtPoints();
            hasSpawned = true;
            GameManager.Instance.SetMaxPlants(plantsToSpawn.Count);
        }

        foreach (var a in plantsToSpawn)
        {
            a.gameObject.SetActive(true);
            a.SetColliderActive(true);
        }

        foreach (var a in allPlants)
        {
            if (!plantsToSpawn.Contains(a))
            {
                a.gameObject.SetActive(false);
                a.SetColliderActive(false);
            }
        }
    }

    private void PlacePlantsAtPoints()
    {
        List<Transform> freePoints = new List<Transform>(spawnPoints);

        foreach (var plant in plantsToSpawn)
        {
            int id = Random.Range(0, freePoints.Count);
            Transform point = freePoints[id];
            freePoints.RemoveAt(id);

            plant.transform.SetPositionAndRotation(point.position, point.rotation);

            plant.SetOriginalTransform(
                plant.transform.localPosition,
                plant.transform.localRotation,
                plant.transform.localScale
            );
        }
    }

    public void ShowOnlySelectedPlant(PlantController selected)
    {
        foreach (var plant in allPlants)
        {
            bool isSelected = plant == selected;
            plant.SetColliderActive(isSelected);
            plant.gameObject.SetActive(isSelected);
        }
    }

    public void HideAllPlants()
    {
        foreach (var plant in allPlants)
        {
            plant.SetColliderActive(false);
            plant.gameObject.SetActive(false);
        }
    }

    public PlantController GetRandomUnfoundPlant()
    {
        List<PlantController> available = new List<PlantController>();

        foreach (var a in plantsToSpawn)
        {
            if (a.gameObject.activeSelf && !a.IsSelected())
            {
                available.Add(a);
            }
        }

        if (available.Count == 0)
            return null;

        int r = Random.Range(0, available.Count);
        return available[r];
    }
}