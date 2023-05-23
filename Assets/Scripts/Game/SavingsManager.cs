using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SavingsManager : MonoBehaviour
{
    [SerializeField] private InventoryData _inventoryData;
    [SerializeField] private BuildingSystem _buildingSystem;
    [SerializeField] private TimeController _timeController;
    [SerializeField] private Terrain _terrain;

    [SerializeField] private Person basePerson;
    [SerializeField] private Building tempBuilding;
    [SerializeField] private List<Building> possibleBuildings;

    private int _prevHour = 0;

    private void Start()
    {
        int load = PlayerPrefs.GetInt("masterLoad");
        if (load == 1)
        {
            _ = LoadGameAsync();
            PlayerPrefs.SetInt("masterLoad", 0);
        }
        else
        {
            _ = LoadMapAsync("defaultMap.txt");
        }
    }

    private void Update()
    {
        if (_timeController.CurrentTime.Hour > _prevHour + 12)
        {
            _prevHour = _timeController.CurrentTime.Hour;
            _ = SaveGameAsync();
            Debug.Log("Game saved!");
        }
    }

    public async Task SaveGameAsync()
    {
        List<string> freeWorkers = new List<string>();
        foreach (Person person in _inventoryData.FreeWorkers)
        {
            freeWorkers.Add(person.ToJson());
        }

        List<string> totalWorkers = new List<string>();
        foreach (Person person in _inventoryData.FreeWorkers)
        {
            totalWorkers.Add(person.ToJson());
        }

        List<string> allBuildings = new List<string>();
        foreach (Building building in _buildingSystem.AllBuildings)
        {
            allBuildings.Add(building.ToJson());
        }

        SaveObject saveObject = new SaveObject()
        {
            Money = _inventoryData.Money,
            FreeWorkers = freeWorkers,
            TotalWorkers = totalWorkers,
            AllBuildings = allBuildings,
            CurrentTime = _timeController.CurrentTime.ToString()
        };

        string json = JsonUtility.ToJson(saveObject);
        await File.WriteAllTextAsync(Application.dataPath + "/save.txt", json);

        TerrainData terrainData = _terrain.terrainData;
        int w = terrainData.alphamapResolution;
        int h = terrainData.alphamapResolution;
        int q = terrainData.alphamapLayers;
        float[,,] tData = terrainData.GetAlphamaps(0, 0, w, h);
        var stringBuilder = new StringBuilder();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                for (var z = 0; z < q; z++)
                {
                    stringBuilder.Append(tData[x, y, z]).Append(';').Append('\n');
                }
            }
        }

        using (var file = File.Open(Application.dataPath + "/savedMap.txt", FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (var streamWriter = new StreamWriter(file, Encoding.UTF8))
            {
                await streamWriter.WriteAsync(stringBuilder.ToString());
            }
        }
    }

    public async Task LoadGameAsync()
    {
        if (File.Exists(Application.dataPath + "/save.txt"))
        {
            string saveString = await File.ReadAllTextAsync(Application.dataPath + "/save.txt");
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);

            List<Building> allBuildings = new List<Building>();
            foreach (string building in saveObject.AllBuildings)
            {
                foreach (Building posBuilding in possibleBuildings)
                {
                    Building controlBuilding = tempBuilding.FromJson(building);
                    if(controlBuilding.BuildingData.Name == posBuilding.BuildingData.Name)
                    {   
                        Building newBuilding = Instantiate(posBuilding, controlBuilding.BuildingTransform.position,
                            controlBuilding.BuildingTransform.rotation);
                        if(newBuilding.BuildingData.ObjectId == 100)
                        {
                            Person newPerson = Instantiate(newBuilding.Person, newBuilding.Person.PersonTransform.position, newBuilding.Person.PersonTransform.rotation);
                            newBuilding.AssignHousePerson(newPerson);
                            newPerson.HomePosition = newBuilding.BuildingTransform.position;
                            _inventoryData.FreeWorkers.Add(newPerson);
                            _inventoryData.TotalWorkers.Add(newPerson);
                        }
                        else if(newBuilding.BuildingData.ObjectId > 100)
                        {
                            Person newPerson = _inventoryData.FreeWorkers.First();
                            newBuilding.AssignWorkPerson(newPerson);
                            newPerson.WorkPosition = newBuilding.BuildingTransform.position;
                            _inventoryData.FreeWorkers.Remove(newBuilding.Person);
                        }
                        allBuildings.Add(newBuilding);
                    }
                }
            }

            _buildingSystem.AllBuildings = allBuildings;
            await LoadMapAsync("savedMap.txt");
        }
    }

    private async Task LoadMapAsync(string fileName)
    {
            var stringBuilder = new StringBuilder();
            TerrainData terrainData = _terrain.terrainData;
            int w = terrainData.alphamapResolution;
            int h = terrainData.alphamapResolution;
            int q = terrainData.alphamapLayers;
            float[,,] tData = terrainData.GetAlphamaps(0, 0, w, h);
            char[] result;

            using (FileStream file = File.Open(Application.dataPath + "/" + fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
                {
                    result = new char[streamReader.BaseStream.Length];
                    await streamReader.ReadAsync(result, 0, (int)streamReader.BaseStream.Length);
                    stringBuilder.Append(result);
                }
            }

            string map = stringBuilder.ToString();
            List<string> values = map.Split(";\n").ToList();
            int index = 0;
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    for (var z = 0; z < q; z++)
                    {
                        tData[x, y, z] = float.Parse(values[index]);
                        index++;
                    }
                }
            }

            _terrain.terrainData.SetAlphamaps(0, 0, tData);
    }

    [Serializable]
    private class SaveObject
    {
        public int Money;
        public List<string> FreeWorkers;
        public List<string> TotalWorkers;
        public List<string> AllBuildings;
        public string CurrentTime;
    }
}
