//
//Author: Tobias Stöckl
//
using UnityEngine;

public class CameraControls : MonoBehaviour
{

    private Vector3 _mainCamStartPosition;
    private Quaternion _mainCamStartRotation;
    private Camera _mainCam;
    [SerializeField]
    public float ScrollSpeed = 3.0f;
    private float _currentOffset = 0.0f;
    private int _mouseButton = 1;

    private float _currentCamXpos;
    private float _currentCamXoffset;

    private Vector3 camTopPosition = new Vector3(3, 2.5f, 2.5f);
    private Quaternion camTopRotation = Quaternion.Euler(90, 0, 0);
    public bool IsTopPosition = false;
    private bool lastFrameTopPos = false;
    private float timeCount = 0.0f;

    [SerializeField]
    private float _clampSliderLeft = -2f;
    [SerializeField]
    private float _clampSliderRight = 2f;

    void Start()
    {
        _mainCamStartPosition = Camera.main.transform.position;
        _mainCamStartRotation = Camera.main.transform.rotation;
        _currentCamXpos = Camera.main.transform.position.x;
        _mainCam = Camera.main;
        _currentCamXoffset = Camera.main.transform.position.x;
    }
    //simple script that handles where the camera position should be, so that every transition is always smooth
    void Update()
    {
        if (Input.GetMouseButtonDown(_mouseButton))
        {
            _currentOffset = Input.mousePosition.x;
            _currentCamXoffset = _mainCam.transform.position.x;
        }

        if (Input.GetMouseButton(_mouseButton))
        {
            float camxpos = ((Input.mousePosition.x -_currentOffset) / Screen.width) * ScrollSpeed;
            _currentCamXpos = -camxpos + _currentCamXoffset;
        }

        //camera smooth transition between 2 points
        _currentCamXpos = Mathf.Clamp(_currentCamXpos, _clampSliderLeft, _clampSliderRight);

        Quaternion rotateTo = camTopRotation;
        Vector3 moveTo = new Vector3(_currentCamXpos, camTopPosition.y, camTopPosition.z);
        Vector3 topBottom = new Vector3(_currentCamXpos, _mainCamStartPosition.y, _mainCamStartPosition.z);
        if(IsTopPosition == lastFrameTopPos)
        {
            timeCount += Time.deltaTime;
        }
        else
        {
            timeCount = 0.0f;
        }
        lastFrameTopPos = IsTopPosition;

        if (!IsTopPosition)
        {
            rotateTo = _mainCamStartRotation;
            moveTo = topBottom;
        }
        timeCount = Mathf.Clamp(timeCount, 0f, 1f);
        //set cam rotation & position
        _mainCam.transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, timeCount);
        _mainCam.transform.position = Vector3.Slerp(transform.position, moveTo, timeCount);
        

    }

    public void SetTopView(bool viewToSet)
    {
        IsTopPosition = viewToSet;
    }
}
