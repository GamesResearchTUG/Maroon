//
//Author: Tobias Stöckl
//
using UnityEngine;

//handles the laserpointer rotation handle ;)
public class HandleHandler : MonoBehaviour
{

    private Camera _mainCamera;
    private Plane _rotatePlane;
    private Collider _thisColl;
    private Vector3 _offset = Vector3.zero;
    private Vector3 _dragAnchor = Vector3.zero;
    private float _yRotation;

    private Vector3 _rotationAnchor;
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _thisColl = GetComponent<Collider>();
        new Plane(Vector3.up, new Vector3(0.0f, 0.0f, 0.0f));
    }


    private void OnMouseDown()
    {
        Ray objIntersectRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit thisHit;
        _thisColl.Raycast(objIntersectRay, out thisHit, Mathf.Infinity);

        _rotatePlane.SetNormalAndPosition(Vector3.up, thisHit.point);

        _dragAnchor = transform.parent.position;
        _offset = thisHit.point - transform.position;
        _rotationAnchor = transform.parent.up;
        _yRotation = transform.parent.eulerAngles.y;
    }
    //on mouse drag, we get the direction of the cursor from the midpoint, project it and rotate the laser object accordingly
    private void OnMouseDrag()
    {
        float dist;
        Ray distRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
        _rotatePlane.Raycast(distRay, out dist);

        Vector3 pointonplane = distRay.GetPoint(dist);
        
        Vector3 rotVector = pointonplane - _offset;

        Vector3 direction = rotVector - _dragAnchor;

        var angl = Vector3.SignedAngle(direction, _rotationAnchor, Vector3.up);
        transform.parent.rotation = Quaternion.Euler(0.0f, -(angl) + 180.0f + _yRotation, -90.0f);
    }
}
