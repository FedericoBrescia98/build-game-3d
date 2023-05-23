using UnityEngine;

public class Utility : MonoBehaviour
{
    public static Vector3 MouseToTerrainPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        const float maxDistance = 100f;
        const int layerMask = 1 << 3;
        return Physics.Raycast(ray, out RaycastHit raycastHit, maxDistance, layerMask) ? raycastHit.point : Vector3.zero;
    }

    public static RaycastHit CameraRay()
    {
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit info, 100) ? info : new RaycastHit();
    }

    //public void ClipCheck()
    //{
    //    Ray ray = new Ray(parentObject.position, transform.forward);
    //    if(Physics.SphereCast(ray, 3, out RaycastHit hit, maxZoom))
    //    {
    //        if(hit.distance < newZoom + 3)
    //        {
    //            newZoom = hit.distance - 3;
    //        }
    //    }
    //}
}
