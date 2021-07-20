//
//Author: Tobias Stöckl
//
using UnityEngine;
using Maroon.Physics;
using Maroon.UI;
using GEAR.Localization;

public class LaserSelectionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]
    public GameObject CurrActiveLaser;
    public GameObject LaserControlPanel;
    public GameObject LaserPointerPrefab;

    public QuantityFloat ActiveLaserIntensity;
    public QuantityFloat ActiveLaserWavelength;

    public QuantityFloat ActiveLaserRefractiveIndex;

    private int _numLaserPointers = 0;
    public int MaxLasers = 100;


    public bool OnUIpanel { get; set; } = false;


    private Vector3 _laserArrayStartPoint;
    private Vector3 _laserArrayEndPoint;

    private Vector3 _lensPos;

    
    void Start()
    {
        CurrActiveLaser = null;

        _laserArrayStartPoint = GameObject.Find("LaserArrayStart").transform.position;
        _laserArrayEndPoint = GameObject.Find("LaserArrayEnd").transform.position;
        _lensPos = GameObject.Find("LensObject").transform.position;


        AddLaserArray();
    }
    // returns true for can add, false for cannot add
    private bool CheckLaserLimit(int toAdd)
    {
        if(_numLaserPointers + toAdd < MaxLasers)
        {
            _numLaserPointers += toAdd;
            return true;
        }
        // show dialogue for max number of lasers
        DialogueManager diagman = FindObjectOfType<DialogueManager>();
        LanguageManager langman = FindObjectOfType<LanguageManager>();

        string message = langman.GetString("maxnumberoflasers");
        diagman.ShowMessage(string.Format(message, (_numLaserPointers+1)));

        return false;
    }


    public void SetActiveIntensityAndWavelength(float intensity, float wavelength) 
    {
        ActiveLaserIntensity.Value = intensity;
        ActiveLaserWavelength.Value = wavelength;
    }


    public void SetActiveIntensity(float intensity)
    {
        
        if(CurrActiveLaser != null)
        {
            CurrActiveLaser.GetComponent<LPProperties>().LaserIntensity = intensity;
        }
    }
    public void SetActiveWavelength(float wavelength)
    {

        if (CurrActiveLaser != null)
        {
            CurrActiveLaser.GetComponent<LPProperties>().LaserWavelength = wavelength;
        }
    }
    public GameObject[] GetAllLaserPointers()
    {
        return GameObject.FindGameObjectsWithTag("LaserPointer"); ;
    }

    public void RemoveAllLaserPointers()
    {
        var lps = GetAllLaserPointers();

        foreach(var lp in lps)
        {
            Destroy(lp);
        }
        _numLaserPointers = 0;
    }

    public void AddLaserArray()
    {
        if (!CheckLaserLimit(1)) return;

        int lasers_in_array = 7;

        Vector3 offset = (_laserArrayEndPoint - _laserArrayStartPoint) / lasers_in_array;

        for(int i = 1; i < lasers_in_array; i++)
        {
            Instantiate(LaserPointerPrefab, _laserArrayStartPoint + i * offset, LaserPointerPrefab.transform.rotation);
        }

    }

    private void AddLaserArrayWithOptions(int numlasers, Vector3 startpoint, Vector3 endpoint, Vector3 focalpoint, float intensity, float wavelength)
    {
        if (!CheckLaserLimit(numlasers)) return;
        Vector3 offset = (endpoint - startpoint) / numlasers;

        for(int i = 1; i< numlasers; i++)
        {
            var lpprefab = Instantiate(LaserPointerPrefab, startpoint + i * offset, LaserPointerPrefab.transform.rotation);
            var lpprops = lpprefab.GetComponent<LPProperties>();
            lpprops.LaserIntensity = intensity;
            lpprops.LaserWavelength = wavelength;
            lpprops.SetLaserColor();

            Vector3 tofocalpoint = focalpoint - lpprefab.transform.position;  
            
            var angl = Vector3.SignedAngle(tofocalpoint, Vector3.right, Vector3.up);
            lpprefab.transform.rotation = Quaternion.Euler(0.0f, -(angl), -90.0f);
        }


    }

    public void AddFocusedLaserArray()
    {
        AddLaserArrayWithOptions(7, _laserArrayStartPoint, _laserArrayEndPoint, (_laserArrayStartPoint + _laserArrayEndPoint)/2 + Vector3.right*0.5f, 1.0f, 680f);
    }

    public void AddFocusedLaserArrayWithOffset()
    {
        Vector3 offsetVec = - Vector3.right * 0.3f;
        AddLaserArrayWithOptions(7, _laserArrayStartPoint +offsetVec, _laserArrayEndPoint +offsetVec , (_laserArrayStartPoint + _laserArrayEndPoint) / 2 + Vector3.right * 0.5f +offsetVec, 1.0f, 680f);
    }

    public void AddRGBLaserArray()
    {
        AddLaserArrayWithOptions(7, _laserArrayStartPoint, _laserArrayEndPoint, _laserArrayStartPoint + Vector3.right * 10000.0f, 1.0f, 450f);
        AddLaserArrayWithOptions(7, _laserArrayStartPoint - Vector3.right*0.2f, _laserArrayEndPoint - Vector3.right * 0.2f, _laserArrayStartPoint + Vector3.right * 10000.0f, 1.0f, 500f);
        AddLaserArrayWithOptions(7, _laserArrayStartPoint - Vector3.right * 0.4f, _laserArrayEndPoint - Vector3.right * 0.4f, _laserArrayStartPoint + Vector3.right * 10000.0f, 1.0f, 700f);
    }

    public void AddLaserPointer()
    {
        if (!CheckLaserLimit(1)) return;
        Instantiate(LaserPointerPrefab, new Vector3(-1.0f, _lensPos.y, 2.5f), LaserPointerPrefab.transform.rotation);
        _numLaserPointers++;
    }
    public void RemoveLaserPointer()
    {
        if(CurrActiveLaser != null)
        {
            Destroy(CurrActiveLaser);
            _numLaserPointers--;
        }

    }

    public void SetLaserArrangements(int arr)
    {

        if(!(arr == 0))
        {
            RemoveAllLaserPointers();
        }
        switch (arr)
        {
            case 0: break;
            case 1: AddLaserArray(); break;
            case 2: AddRGBLaserArray(); break;
            case 3: AddFocusedLaserArray(); break;
            case 4: AddFocusedLaserArrayWithOffset(); break;
            default: break;
        }
    }
    

    // Update is called once per frame
    void Update()
    {

        //if current active laser is null disable lasercontrolpanel
        if (CurrActiveLaser == null)
        {
            LaserControlPanel.SetActive(false);
        }
        else
        {
            LaserControlPanel.SetActive(true);
            CurrActiveLaser.GetComponent<LPProperties>().SetLaserColor();
        }

        if(Input.GetMouseButtonDown(0))
        {
            //shoot ray and collide with scene
            Ray testray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhit;
            Physics.Raycast(testray.origin, testray.direction, out rhit, Mathf.Infinity, Physics.AllLayers); // todo layermask?

            if (rhit.collider.tag == "LaserPointer")
            {
                if(CurrActiveLaser != null) CurrActiveLaser.GetComponent<DragLaserObject>().MakeInactive();
                CurrActiveLaser = rhit.collider.gameObject;
                CurrActiveLaser.GetComponent<DragLaserObject>().MakeActive();
            }
            else
            {
                if(! (rhit.collider.tag == "LPHandle") && CurrActiveLaser != null) //todo make klick on table deselect of laser.
                {
                    if (rhit.collider.gameObject.name == "OpticsTable" || rhit.collider.gameObject.name == "OpticsTable2") //todo change to optical tables
                    {
                        if (!OnUIpanel)
                        {
                            CurrActiveLaser.GetComponent<DragLaserObject>().MakeInactive();
                            CurrActiveLaser = null;
                        }
                       
                    }
                }
            }
        }
        SetActiveIntensity(ActiveLaserIntensity);
        SetActiveWavelength(ActiveLaserWavelength);
        if(CurrActiveLaser!= null)
        {
            ActiveLaserRefractiveIndex.Value = CurrActiveLaser.GetComponent<LPProperties>().GetCauchyForCurrentWavelength();
        }
    }
}
