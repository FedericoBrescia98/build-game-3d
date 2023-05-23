using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New BuildingData", menuName = "BuildingData")]
public class BuildingData : ScriptableObject
{
    public string Name = "Building";
    public int ObjectId = 0;

    // set outputs per cycle
    public int MoneyCycleOutput = 0;
    public int WorkersCycleOutput = 0;

    // set static outputs
    public int MoneyStaticOutput = 0;
    public int WorkersStaticOutput = 0;

    // set construction costs
    public int MoneyCost = 0;
    public int WorkersCost = 0;

    // set prefab properties
    public bool RequiresRoad = false;

    [Serializable]
    private class SaveBuildingData
    {
        public string name = "Building";
        public int objectId = 0;
        public int moneyCycleOutput = 0;
        public int workersCycleOutput = 0;
        public int moneyStaticOutput = 0;
        public int workersStaticOutput = 0;
        public int moneyCost = 0;
        public int workersCost = 0;
        public bool requiresRoad = false;
    }

    public string ToJson()
    {
        SaveBuildingData saveBuilding = new SaveBuildingData()
        {
            name = Name,
            objectId = ObjectId,
            moneyCycleOutput = MoneyCycleOutput,
            workersCycleOutput = WorkersCycleOutput,
            moneyStaticOutput = MoneyStaticOutput,
            workersStaticOutput = WorkersStaticOutput,
            moneyCost = MoneyCost,
            workersCost = WorkersCost,
            requiresRoad = RequiresRoad
        };
        return JsonUtility.ToJson(saveBuilding);
    }

    public BuildingData FromJson(string json)
    {
        SaveBuildingData saveBuildingData = JsonUtility.FromJson<SaveBuildingData>(json);
        Name = saveBuildingData.name;
        ObjectId = saveBuildingData.objectId;
        MoneyCost = saveBuildingData.moneyCost;
        MoneyCycleOutput = saveBuildingData.moneyCycleOutput;
        MoneyStaticOutput = saveBuildingData.moneyStaticOutput;
        WorkersCost = saveBuildingData.workersCost;
        WorkersCycleOutput = saveBuildingData.workersCycleOutput;
        WorkersStaticOutput = saveBuildingData.workersStaticOutput;
        RequiresRoad = saveBuildingData.requiresRoad;

        return this;
    }
}
