using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Current;
    public GridLayout GridLayout;

    private Grid _grid;
    [SerializeField] private Tilemap _mainTilemap;
    [SerializeField] private TileBase _blackTile;
    public GameObject Prefab1;

    private PlaceableObject _objectToPlace;

    #region Unity nethods
    private void Awake()
    {
        Current = this;
        _grid = GridLayout.gameObject.GetComponent<Grid>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            InitializeWithObject(Prefab1);
        }

        if(!_objectToPlace)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.R) || Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            _objectToPlace.Rotate(clockwise: true);
        }
        if(Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            _objectToPlace.Rotate(clockwise: false);
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            if(CanBePlaced(_objectToPlace))
            {
                _objectToPlace.Place();
                Vector3Int start = GridLayout.WorldToCell(_objectToPlace.GetStartPosition());
                TakeArea(start, _objectToPlace.Size);
            }
            else
            {
                Destroy(_objectToPlace.gameObject);
            }
        }
        else if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(3))
        {
            Destroy(_objectToPlace.gameObject);
        }
    }
    #endregion

    #region Utils
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = GridLayout.WorldToCell(position);
        position = _grid.GetCellCenterWorld(cellPos);
        return position;
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach(var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }
        return array;
    }

    #endregion

    #region Building Placement

    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 position = SnapCoordinateToGrid(Vector3.zero);
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        _objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();
    }

    private bool CanBePlaced(PlaceableObject placeableObject)
    {
        BoundsInt area = new BoundsInt();
        area.position = GridLayout.WorldToCell(_objectToPlace.GetStartPosition());
        area.size = placeableObject.Size;
        TileBase[] baseArray = GetTilesBlock(area, _mainTilemap);
        foreach(var b in baseArray)
        {
            if(b == _blackTile)
            {
                return false;
            }
        }
        return true;
    }

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        _mainTilemap.BoxFill(start, _blackTile, start.x, start.y,
                            start.x + size.x, start.y + size.y);
    }

    #endregion
}
