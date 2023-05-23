using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PeopleController : MonoBehaviour
{
    [SerializeField] private BuildingSystem buildingSystem;
    private List<Building> _buildings = new List<Building>();
    private List<Person> _agents = new List<Person>();

    void Awake()
    {
        _buildings = buildingSystem.AllBuildings;
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
