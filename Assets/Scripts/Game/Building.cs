using System;
using System.Collections;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData BuildingData;
    public Transform BuildingTransform;
    public bool IsHover = false;
    public bool Placed { get; private set; }
    public bool BuildCollide = false;
    public Person Person;

    public event EventHandler Click;

    private Renderer _buildingRender;

    [SerializeField] private Material _blinkMaterial;
    [SerializeField] private Person basePerson;

    #region Unity methods

    private void Awake()
    {
        BuildingTransform = this.transform;
        _buildingRender = transform.GetComponent<Renderer>();
    }

    private void Update()
    {
        if (IsHover && Input.GetMouseButtonDown(0))
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Building") || collider.CompareTag("Water"))
        {
            BuildCollide = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Building") || collider.CompareTag("Water"))
        {
            BuildCollide = false;
        }
    }

    private void OnMouseEnter()
    {
        IsHover = true;
    }

    private void OnMouseExit()
    {
        IsHover = false;
    }

    private void OnDrawGizmos()
    {
        int gizmoRadius = 5;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }

    #endregion

    public void Rotate(bool clockwise = true)
    {
        transform.Rotate(clockwise ? new Vector3(0, 90, 0) : new Vector3(0, -90, 0));
    }

    public void Build()
    {
        StartCoroutine(BuildLerp());
    }

    private IEnumerator BuildLerp()
    {
        const float buildTime = 1f;
        float timeElapsed = 0f;
        Vector3 buildPos = transform.localPosition;
        while (timeElapsed < buildTime)
        {
            transform.localPosition = Vector3.Lerp(new Vector3(buildPos.x, 0, buildPos.z),
                buildPos, timeElapsed / buildTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = buildPos;
        yield return StartCoroutine(BlinkBuilding());
        BuildingTransform = this.transform;
        Placed = true;
    }

    private IEnumerator BlinkBuilding()
    {
        Material startMaterial = _buildingRender.material;
        const float blinkTime = 0.1f;
        float timeElapsed = 0f;
        while (timeElapsed < blinkTime)
        {
            _buildingRender.material = _blinkMaterial;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _buildingRender.material = startMaterial;
    }

    public void AssignHousePerson(Person person)
    {
        if (this.BuildingData.ObjectId == 100)
        {
            Person = person;
        }
    }

    public void AssignWorkPerson(Person person)
    {
        if (this.BuildingData.ObjectId > 100)
        {
            Person = person;
        }
    }

    public bool CanBuild(InventoryData inventory)
    {
        BuildingData buildingBuildingData = BuildingData;

        bool i1 = inventory.RequestResource("workers", buildingBuildingData.WorkersCost);
        bool i2 = inventory.RequestResource("money", buildingBuildingData.MoneyCost);

        return (i1 && i2);
    }

    public void Destroy()
    {
        StartCoroutine(DestroyLerp());
    }

    private IEnumerator DestroyLerp()
    {
        const float buildTime = 1f;
        float timeElapsed = 0f;
        Vector3 buildPos = transform.localPosition;

        _buildingRender.material.color = new Color(0.5f, 0.5f, 0.5f);
        while (timeElapsed < buildTime)
        {
            transform.localPosition = Vector3.Lerp(buildPos, new Vector3(buildPos.x, 0, buildPos.z),
                timeElapsed / buildTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Placed = false;
        Destroy(this.gameObject);
    }

    [Serializable]
    private class SaveBuilding
    {
        public Vector3 position;
        public Quaternion rotation;
        public string buildingData;
        public bool placed;
        public string person;
    }

    public string ToJson()
    {
        SaveBuilding saveBuilding = new SaveBuilding()
        {
            position = BuildingTransform.position,
            rotation = BuildingTransform.rotation,
            buildingData = BuildingData.ToJson(),
            placed = Placed,
            person = Person == null ? "null" : Person.ToJson(),
        };
        return JsonUtility.ToJson(saveBuilding);
    }

    public Building FromJson(string json)
    {
        SaveBuilding saveBuilding = JsonUtility.FromJson<SaveBuilding>(json);
        BuildingTransform.position = saveBuilding.position;
        BuildingTransform.rotation = saveBuilding.rotation;
        BuildingData buildData = BuildingData.FromJson(saveBuilding.buildingData);
        this.BuildingData = buildData;
        Placed = saveBuilding.placed;
        if (saveBuilding.person != "null")
        {
            Person = basePerson.FromJson(saveBuilding.person);
        }
        return this;
    }
}
