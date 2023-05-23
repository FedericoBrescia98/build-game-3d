using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private TextMeshProUGUI _workersText;
    public static int ResourceMax = 5000000;

    public int Money
    {
        get => _money;
        set => _money = value > ResourceMax ? ResourceMax : value;
    }

    private int _money;
    public List<Person> FreeWorkers = new List<Person>();
    public List<Person> TotalWorkers = new List<Person>();

    #region Unity methods

    public void Awake()
    {
        ResetInventoryToBase();
    }

    public void Update()
    {
        _workersText.text = "Free workers: " + FreeWorkers.Count + "/" + TotalWorkers.Count;
        _moneyText.text = "Money: $" + _money.ToString();
    }

    #endregion

    public void ResetInventoryToBase()
    {
        Money = 500000;
    }

    public bool RemoveResource(string resource, int amountRequested)
    {
        if (amountRequested == 0)
        {
            return true;
        }

        // switch based on resource name
        // try and subtract amount requested from resource
        // return true on success, false otherwise
        switch (resource.ToLower())
        {
            case "money":
                Money -= amountRequested;
                return true;
            default:
                return false;
        }
    }

    public bool RequestResource(string resource, int amountRequested)
    {
        if (amountRequested.Equals(0))
        {
            return true;
        }

        // switch based on resource name
        // check amount requested from resource
        // return true if available, false otherwise
        return resource.ToLower() switch
        {
            "workers" => amountRequested <= FreeWorkers.Count,
            "money" => amountRequested <= Money,
            _ => false,
        };
    }

    public void RemoveBuildingCosts(Building building)
    {
        BuildingData buildingBuildingData = building.BuildingData;

        bool i1 = RemoveResource("money", buildingBuildingData.MoneyCost);

        if (i1) return;
        Exception e = new Exception("Inventory error");
        Console.WriteLine("Error removing resources: " + e.Message);
        throw e;
    }
}
