//
//Author: Tobias Stöckl
//
using System.Collections.Generic;
using UnityEngine;
using Maroon.Physics;

public class LaserRenderer : MonoBehaviour
{

    [SerializeField]
    private LineRenderer[] _lineRenderers = new LineRenderer[500];
    public Material LaserMaterial;
    private List<OpticsSegment> _laserSegments;

    public GameObject[] LaserPointers;

    [SerializeField]
    private float _laserWidth = 0.003f;

    public QuantityFloat CauchyA;
    public QuantityFloat CauchyB;

    public QuantityFloat LensRadius1;
    public QuantityFloat LensRadius2;
    public QuantityFloat LensDiameter;

    public OpticsLens CurrentLens;

    private LensMeshGenerator _lensMeshGenerator;

    public QuantityFloat ReflectionVsRefraction;

    [SerializeField]
    [Range(0.0f, 0.99f)]
    private float _loss = 0.9f;
    private GameObject _thisLens;

    void Start()
    {
        // add line renderers to parent object
        _laserSegments = new List<OpticsSegment>();
        _thisLens = GameObject.FindGameObjectWithTag("Lens");
        _lensMeshGenerator = _thisLens.GetComponent<LensMeshGenerator>();


        for (int i = 0; i < _lineRenderers.Length; i++)
        {
            _lineRenderers[i] = new GameObject().AddComponent<LineRenderer>();
            _lineRenderers[i].material = LaserMaterial;
            _lineRenderers[i].transform.parent = gameObject.transform;

            _lineRenderers[i].startWidth = _laserWidth;
            _lineRenderers[i].endWidth = _laserWidth;
            _lineRenderers[i].numCapVertices = 5;
            _lineRenderers[i].useWorldSpace = false;

        }
    }


    void Update()
    {
        //update what the renderers showed, and change if it differs now
        if (_laserSegments == null)
        {
            _laserSegments = new List<OpticsSegment>();
        }
        _laserSegments.Clear();

        //get all laserpointers in scene 
        //could also be done by tracking deleted and added lasers
        LaserPointers = GameObject.FindGameObjectsWithTag("LaserPointer");
        OpticsLens currLens = _lensMeshGenerator.ThisLensLens;

        LensRadius1.Value = currLens.LeftCircle.Radius;
        LensRadius2.Value = currLens.RightCircle.Radius;
        currLens.Radius = _lensMeshGenerator.LensRadius;
        currLens.CauchyA = CauchyA;
        currLens.CauchyB = CauchyB;
        CurrentLens = currLens; //make current lens struct accessible to other scripts

        LensDiameter.Value = 0.0f;
        (float left, float right ) = _thisLens.GetComponent<OpticsSim>().GetBoundsHit(currLens);
        LensDiameter.Value = (right - left) + _thisLens.GetComponent<LensMeshGenerator>().RadiusInput1.Value - _thisLens.GetComponent<LensMeshGenerator>().RadiusInput2.Value;

        foreach (var laserPointer in LaserPointers)
        {
            Vector3 relLaserPos = gameObject.transform.InverseTransformPoint(laserPointer.transform.position);
            Vector3 relLaserDir = gameObject.transform.InverseTransformDirection(laserPointer.transform.up);
            var laserPointerProperties = laserPointer.GetComponent<LPProperties>();

            float intensity = laserPointerProperties.LaserIntensity;
            float wavelength = laserPointerProperties.LaserWavelength;

            OpticsRay laserRay = new OpticsRay(new Vector2(relLaserPos.x, relLaserPos.z), new Vector2(relLaserDir.x, relLaserDir.z), intensity, laserPointerProperties.LaserColor, wavelength);
            _thisLens.GetComponent<OpticsSim>().CalculateHitsRecursive(laserRay, currLens, true, ref _laserSegments, _loss, ReflectionVsRefraction);

        }
        UpdateLasers();

    }

    void UpdateLasers()
    {

        for(int i = 0; i < _lineRenderers.Length; i++)
        {
            if(i < _laserSegments.Count)
            {
                _lineRenderers[i].enabled = true;
                UpdateLR(_laserSegments[i], _lineRenderers[i]);
            }
            else
            {
                _lineRenderers[i].enabled = false;
            }
        }
    }

    void UpdateLR(OpticsSegment opticsSeg, LineRenderer Lr, bool useIntensity = true)
    {
        Lr.SetPosition(0, new Vector3( opticsSeg.P1.x, 0.0f, opticsSeg.P1.y)*_thisLens.transform.localScale.x + _thisLens.transform.position); 
        Lr.SetPosition(1, new Vector3( opticsSeg.P2.x, 0.0f, opticsSeg.P2.y)*_thisLens.transform.localScale.x + _thisLens.transform.position);
        if(useIntensity) Lr.material.SetColor("_TintColor", new Color(opticsSeg.SegmentColor.r, opticsSeg.SegmentColor.g, opticsSeg.SegmentColor.b, opticsSeg.Intensity));
    }

    public void SetCauchyVariables(int dropdownSelection) //todo maybe move this in extra class
    {

        if (dropdownSelection == 0) return;
        dropdownSelection--;

        List<float> cauchyAs = new List<float>();
        List<float> cauchyBs = new List<float>();

        cauchyAs.Add(1.7387f);
        cauchyAs.Add(1.5111f);
        cauchyAs.Add(1.4767f);
        cauchyAs.Add(1.3244f);
        cauchyAs.Add(2.3818f);
        cauchyAs.Add(1.7522f);

        cauchyAs.Add(1.5220f);
        cauchyAs.Add(1.7280f);
        cauchyAs.Add(1.4580f);
        cauchyAs.Add(1.5046f);

        cauchyBs.Add(0.0159f);
        cauchyBs.Add(0.00425f);
        cauchyBs.Add(0.0048f);
        cauchyBs.Add(0.0031f);
        cauchyBs.Add(0.0121f);
        cauchyBs.Add(0.0055f);

        cauchyBs.Add(0.00459f);
        cauchyBs.Add(0.01342f);
        cauchyBs.Add(0.00354f);
        cauchyBs.Add(0.00420f);


        CauchyA.Value = cauchyAs[dropdownSelection];
        CauchyB.Value = cauchyBs[dropdownSelection];
    }
}
