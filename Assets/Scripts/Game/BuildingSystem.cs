using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using CursorMode = UnityEngine.CursorMode;
using FilterMode = UnityEngine.FilterMode;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance;
    public Terrain Terrain;
    public InventoryData Inventory;
    public AudioPlayer AudioPlayer;

    private Building _building;
    private Building _ghost;

    public bool IsPlacing = false;
    public bool IsDestroying = false;
    public bool IsPainting = false;
    public List<Building> AllBuildings = new List<Building>();

    private Vector3 _buildPosition = Vector3.zero;
    private Renderer _ghostRender;
    private Color _ghostColor;
    private float[,,] _splat; // A splat map is what unity uses to overlay all of your paints on to the terrain
    public int AreaOfEffectSize = 10; // size of the brush
    private float[,] _brush; // this stores the brush.png pixel data
    private SpriteRenderer _paintSpriteRenderer;

    [SerializeField] private Texture2D _roadTexture;
    [SerializeField] private Texture2D _grassTexture;
    [SerializeField] private float _updateInventoryTime;
    [SerializeField] private Texture2D _demolishCursorTexture;
    [SerializeField] private GameObject _paintBrush;
    [SerializeField] private Person _person;
    [SerializeField] private TimeController _timeController;
    private int _prevHour = 0;

    #region Unity methods

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (IsPlacing)
        {
            ShowGhost();
            HandleRotationPlacement();

            if (Input.GetMouseButtonDown(0))
            {
                if (!CanBePlaced()) return;

                SpawnBuilding();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                DestroyGhost();
            }
        }

        if (IsDestroying)
        {
            if (Input.GetMouseButtonDown(1))
            {
                IsDestroying = false;

                Vector2 hotSpot = Vector2.zero;
                CursorMode cursorMode = CursorMode.Auto;

                UnityEngine.Cursor.SetCursor(null, hotSpot, cursorMode);

                foreach (Building building in AllBuildings)
                {
                    building.Click -= DestroyBuilding;
                }
            }
        }

        if (IsPainting)
        {
            PaintTerrain();
            if (Input.GetMouseButtonDown(1))
            {
                IsPainting = false;
                _paintSpriteRenderer.enabled = false;
            }
        }


        if (_timeController.CurrentTime.Hour > _prevHour)
        {
            _prevHour = _timeController.CurrentTime.Hour;
            foreach (Building building in AllBuildings)
            {
                Inventory.Money += building.BuildingData.MoneyCycleOutput;
            }
        }
    }

    #endregion

    #region Building Placement

    public void InitPlacing(Building building)
    {
        _ghost = Instantiate(building, _buildPosition, Quaternion.identity);
        _building = building;
        IsPlacing = true;
    }

    private void HandleRotationPlacement()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _ghost.Rotate(clockwise: true);
        }
    }

    private void ShowGhost()
    {
        _buildPosition = Utility.MouseToTerrainPosition();
        _ghost.transform.position = _buildPosition;

        foreach (NavMeshObstacle obstacle in _ghost.gameObject.GetComponentsInChildren<NavMeshObstacle>())
        {
            obstacle.enabled = false;
        }

        _ghostRender = _ghost.gameObject.GetComponent<Renderer>();
        _ghostColor = _ghostRender.material.color;
        _ghostRender.material.color = _ghost.BuildCollide || !_building.CanBuild(Inventory)
            ? new Color(1, 0, 0)
            : new Color(0, 1, 0);
    }

    private void DestroyGhost()
    {
        Destroy(_ghost.gameObject);
        _building = null;
        _ghost = null;
        IsPlacing = false;
    }

    private void SpawnBuilding()
    {
        if (!_building.CanBuild(Inventory))
        {
            AudioPlayer.PlayErrorSound();
            return;
        }

        AudioPlayer.PlayBuildSound();

        // remove building cost
        Inventory.RemoveResource("money", _building.BuildingData.MoneyCost);

        // Create Building
        _building = Instantiate(_building, _buildPosition, _ghost.transform.rotation);
        AllBuildings.Add(_building);
        _building.Build();

        // Manage People
        if (_building.BuildingData.ObjectId == 100)
        {
            Person newPerson = Instantiate(_person, _buildPosition, Quaternion.identity);
            newPerson.WorkPosition = Vector3.zero;
            _building.AssignHousePerson(newPerson);
            newPerson.HomePosition = _buildPosition;
            Inventory.FreeWorkers.Add(newPerson);
            Inventory.TotalWorkers.Add(newPerson);
        }
        else if (_building.BuildingData.ObjectId > 100)
        {
            Person person = Inventory.FreeWorkers.First();
            _building.AssignWorkPerson(person);
            person.WorkPosition = _buildPosition;
            Inventory.FreeWorkers.Remove(person);
        }

        // add building static output
        Inventory.Money += _building.BuildingData.MoneyStaticOutput;
    }

    public bool CanBePlaced()
    {
        return !_ghost.BuildCollide;
    }

    public void DestroyBuilding(object sender, EventArgs e)
    {
        Building building = (Building)sender;

        // Manage People
        if (building.BuildingData.ObjectId == 100)
        {
            Inventory.TotalWorkers.Remove(building.Person);
            Inventory.FreeWorkers.Remove(building.Person);
        }
        else if (building.BuildingData.ObjectId > 100)
        {            
            building.Person.WorkPosition = Vector3.zero;
            Inventory.FreeWorkers.Add(building.Person);
        }

        Destroy(building.Person.gameObject);
        AudioPlayer.PlayDestroySound();
        building.Destroy();
        AllBuildings.Remove(building);
    }

    public void InitDestroy()
    {
        Vector2 hotSpot = Vector2.zero;
        CursorMode cursorMode = CursorMode.Auto;

        UnityEngine.Cursor.SetCursor(_demolishCursorTexture, hotSpot, cursorMode);
        IsDestroying = true;
        foreach (Building building in AllBuildings)
        {
            building.Click += DestroyBuilding;
        }
    }

    #endregion

    #region Paint Terrain

    public void InitPainting()
    {
        IsPainting = true;
        _paintSpriteRenderer = _paintBrush.gameObject.GetComponent<SpriteRenderer>();
        _paintSpriteRenderer.enabled = true;
    }

    public void PaintTerrain()
    {
        _paintBrush.transform.position = Utility.MouseToTerrainPosition();

        if (Input.GetMouseButton(0))
        {
            _brush = GenerateBrush(_roadTexture,
                AreaOfEffectSize);
            GetTerrainCoordinates(Utility.MouseToTerrainPosition(), out int terX, out int terZ);
            ModifyTerrain(terX, terZ, 2);
        }

        if (Input.GetMouseButton(3) || Input.GetKey(KeyCode.F))
        {
            _brush = GenerateBrush(_grassTexture,
                AreaOfEffectSize);
            GetTerrainCoordinates(Utility.MouseToTerrainPosition(), out int terX, out int terZ);
            ModifyTerrain(terX, terZ, 0);
        }
    }

    private void GetTerrainCoordinates(Vector3 point, out int x, out int z)
    {
        int offset = AreaOfEffectSize / 2;
        Vector3 tempTerrainCoodinates = point;
        //This takes the world coords and makes them relative to the terrain
        Vector3 terrainCoordinates = new Vector3(
            tempTerrainCoodinates.x / Terrain.terrainData.size.x,
            tempTerrainCoodinates.y / Terrain.terrainData.size.y,
            tempTerrainCoodinates.z / Terrain.terrainData.size.z);
        // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
        Vector3 locationInTerrain = new Vector3
        (
            terrainCoordinates.x * Terrain.terrainData.heightmapResolution,
            0,
            terrainCoordinates.z * Terrain.terrainData.heightmapResolution
        );
        x = (int)locationInTerrain.x - offset;
        z = (int)locationInTerrain.z - offset;
    }

    public float[,] GenerateBrush(Texture2D texture, int size)
    {
        float[,] heightMap = new float[size, size]; //creates a 2d array which will store our brush
        Texture2D scaledBrush = ResizeBrush(texture, size, size);
        // this will iterate over the entire re-scaled image and convert the pixel color into a value between 0 and 1
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Color pixelValue = scaledBrush.GetPixel(x, y);
                heightMap[x, y] = pixelValue.grayscale / 255;
            }
        }

        return heightMap;
    }

    public static Texture2D ResizeBrush(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
    {
        Rect texR = new Rect(0, 0, width, height);
        _gpu_scale(src, width, height, mode);
        Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
        result.Reinitialize(width, height);
        result.ReadPixels(texR, 0, 0, true);
        return result;
    }

    static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
    {
        src.filterMode = fmode;
        src.Apply(true);
        RenderTexture rtt = new RenderTexture(width, height, 32);
        Graphics.SetRenderTarget(rtt);
        GL.LoadPixelMatrix(0, 1, 1, 0);
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
    }

    void ModifyTerrain(int x, int z, int paint)
    {
        int aoExMod = 0;
        int aoEzMod = 0;
        int aoExMod1 = 0;
        int aoEzMod1 = 0;
        int strength = 1;
        if (x < 0) // if the brush goes off the negative end of the x axis we set the mod == to it to offset the edited area
        {
            aoExMod = x;
        }
        else if
            (x + AreaOfEffectSize >
             Terrain.terrainData
                 .heightmapResolution) // if the brush goes off the posative end of the x axis we set the mod == to this
        {
            aoExMod1 = x + AreaOfEffectSize - Terrain.terrainData.heightmapResolution;
        }

        if (z < 0) //same as with x
        {
            aoEzMod = z;
        }
        else if (z + AreaOfEffectSize > Terrain.terrainData.heightmapResolution)
        {
            aoEzMod1 = z + AreaOfEffectSize - Terrain.terrainData.heightmapResolution;
        }

        _splat = Terrain.terrainData.GetAlphamaps(x - aoExMod, z - aoEzMod, AreaOfEffectSize + aoExMod,
            AreaOfEffectSize + aoEzMod); //grabs the splat map data for our brush area
        for (int xx = 0; xx < AreaOfEffectSize + aoEzMod; xx++)
        {
            for (int yy = 0; yy < AreaOfEffectSize + aoExMod; yy++)
            {
                float[]
                    weights = new float[Terrain.terrainData
                        .alphamapLayers]; //creates a float array and sets the size to be the number of paints your terrain has
                for (int zz = 0; zz < _splat.GetLength(2); zz++)
                {
                    weights[zz] = _splat[xx, yy, zz]; //grabs the weights from the terrains splat map
                }

                weights[paint] +=
                    _brush[xx - aoEzMod, yy - aoExMod] * strength *
                    2000; // adds weight to the paint currently selected with the int paint variable
                //this next bit normalizes all the weights so that they will add up to 1
                float sum = weights.Sum();
                for (int ww = 0; ww < weights.Length; ww++)
                {
                    weights[ww] /= sum;
                    _splat[xx, yy, ww] = weights[ww];
                }
            }
        }

        Terrain.terrainData.SetAlphamaps(x - aoExMod, z - aoEzMod, _splat);
        Terrain.Flush();
    }

    #endregion
}
