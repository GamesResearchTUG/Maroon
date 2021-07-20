//
//Author: Tobias Stöckl
//
using System.Collections.Generic;
using UnityEngine;
using Maroon.Physics;

public class LensMeshGenerator : MonoBehaviour
{
    private MeshFilter _meshFilter;
    public QuantityFloat CylinderThickness;

    private float _cylinderThicknessOld = 0.0f;
    private float _circRadius = 3.0f;
    private float _circRadius2 = 3.0f;

    public QuantityFloat RadiusInput1;
    public QuantityFloat RadiusInput2;
    public QuantityFloat LensRadius;

    private GameObject _lensHolder;
    private GameObject _lensRod;

    private float _radCalcOld = 0;
    private float _radCalcOld2 = 0;
    private float _lensRadiusOld;


    // not to change ingame
    [Range(3, 50)]
    [HideInInspector]
    public int sectionPoints = 10;
    [Range(3, 50)]
    [HideInInspector]
    public int domeSegments = 10;
    [HideInInspector]
    public OpticsLens ThisLensLens;



    public void SetLensPresetDropdown(int whatlens)
    {
        switch (whatlens)
        {
            case 0: SetPresetLens( 0.3f, -0.3f, 0.2f, 1.0f); break;
            case 1: SetPresetLens(-0.3f,  0.3f, 0.7f, 1.0f); break;
            case 2: SetPresetLens(0.01f, 0.01f, 0.2f, 1.0f); break;
            case 3: SetPresetLens(   1f,   -1f, 0.0f, 1.0f); break;
            case 4: SetPresetLens( 0.5f,  0.3f, 0.2f, 1.0f); break;
            case 5: SetPresetLens( 0.3f,  0.5f, 0.2f, 1.0f); break;
            case 6: SetPresetLens( 0.3f,    0f, 0.0f, 1.0f); break;
            case 7: SetPresetLens(-0.3f,    0f, 0.0f, 1.0f); break;
        }
        
    }

    public void SetPresetLens(float leftCircle, float rightCircle, float thickness, float diameter)
    {
        RadiusInput1.Value = leftCircle;
        RadiusInput2.Value = rightCircle;
        LensRadius.Value = diameter;
        CylinderThickness.Value = thickness; 
    }



    void Start()
    {
        if(gameObject.GetComponent<MeshFilter>() != null)
        {
            _meshFilter = gameObject.GetComponent<MeshFilter>();
        }
        else
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if(gameObject.GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }

        _lensHolder = transform.Find("lens_holder_holder").gameObject;
        _lensRod = transform.Find("rod_holder").gameObject;
    }



    float CalcRadiusFromTopEdge(float edgeInput, float lensRadius)
    {

        // tan alpha = lensradius/ edgeinput
        float edgeInputStart = edgeInput;

        // clamp forbidden values
        if (edgeInput >= 0.0f && edgeInput < 0.01f) edgeInput = 0.01f;
        if (edgeInput < 0.0f && edgeInput > -0.01f) edgeInput = -0.01f;
        if (edgeInput > 0.999f) edgeInput = 0.999f;
        if (edgeInput < -0.999f) edgeInput = -0.999f;

        float alpha = Mathf.Atan(lensRadius / edgeInput) * Mathf.Rad2Deg; 
        float radiusangle =  Mathf.Abs(alpha)*2 -90.0f;

        float radius = lensRadius / Mathf.Sin((radiusangle + 90) *Mathf.Deg2Rad); 
        if (edgeInputStart >= 0.0f) radius = radius * -1.0f;
        return radius;
    }



    float CalculateSectionAngle(float radius, float lensradius)
    {
        float angle =  Mathf.Rad2Deg* Mathf.Asin((lensradius/ radius));
        return angle;

    }
    //gets points of arc, starting at (0,0) and ending at (x, y(1.0))
    Vector3[] getSectionPoints(int numpoints, float sectionangle, float radius, float circrad)
    {
        Vector3 radvec = new Vector3(radius, 0.0f, 0.0f);

        Vector3[] points = new Vector3[numpoints];
        //iterate over all points
        for(int i = 0; i < numpoints; i++)
        {
            points[i] = Quaternion.Euler(0, sectionangle * (i + 1) / numpoints, 0) * radvec;
            points[i].x = points[i].x - circrad;
        }

        // return list of points in array
        // afterwards rotate whole section and connect them together
        return points;
    }

    List<Vector3[]> GetDomeVertices(Vector3[] slice, int numberofribs)
    {
        
        float rotAngle = 360 / numberofribs;

        // numberofribs - 1
        List<Vector3[]> dome = new List<Vector3[]>();
        dome.Add(slice);
        //create copies and rotate around the rib
        for (int i = 0; i < numberofribs -1; i++)
        {

            // new list, rotate
            Vector3[] rotated = (Vector3[])slice.Clone();

            //rotate
            for( int j = 0; j< rotated.Length; j++)
            {
                rotated[j]= Quaternion.Euler(rotAngle* (i+1), 0, 0) * rotated[j];
            }

            dome.Add(rotated);
        }

        return dome;
    }


    //makemesh function
    // also return vertex positions of outer ring
    (List<Vector3>, List<int>) MakeFacesAndVertices(List<Vector3[]> dome)
    {
        Vector3 startVec = new Vector3(0.0f, 0.0f, 0.0f);

        int dcount = dome[0].Length;

        List<Vector3> vertBuffer = new List<Vector3>();
        List<Vector3> normBuffer = new List<Vector3>();
        List<int> trisBuffer = new List<int>();
        vertBuffer.Add(startVec);
        //filled up list of verts
        for(int i = 0; i < dome.Count; i++)
        {
            vertBuffer.AddRange(dome[i]);
            //add normals here
        }
        for(int i = 0; i < dome.Count; i++)
        {
            //make middle tri
            trisBuffer.Add(0);
            trisBuffer.Add((i + 1)%dome.Count * dcount + 1);
            trisBuffer.Add(i * dcount + 1);
            
            for(int j = 0; j < dcount-1; j++)
            {
                //fill the rest of the triangles
                trisBuffer.Add((i + 1) % dome.Count * dcount + j + 1);
                trisBuffer.Add(i * dcount + 2 + j);
                trisBuffer.Add(i * dcount + 1 + j);


                trisBuffer.Add((i + 1) % dome.Count * dcount + j + 2);
                trisBuffer.Add(i * dcount + 2 + j);
                trisBuffer.Add((i + 1) % dome.Count * dcount + j + 1);

            }
        }
        return (vertBuffer, trisBuffer);
    } 


    (float, float) CalculateLensPosition(float leftdome, float rightdome, float additionalthickness)
    {
        // calculate the minimum safe distance between the 2 midpoints
        // additionalthickness is thickness @ edge of lens

        float domediff = leftdome - rightdome;
        float lensdistance;
        float avgoffset = (leftdome + rightdome) / 2.0f;

        if(domediff > 0.0f)
        {
            // then outer edge meets
            // domediff is the difference we have to separate the 2 midpoints from each other to get the outer edge to be flush
            // add the additional thickness
            lensdistance = domediff + additionalthickness;
        }
        else
        {
            // inner edges meet
            // domes just need to be moved so that the outer edges are symmetrical.
            // domediff is the value how much outer edges are separated. additionalthickness - (-domediff) is the amount we need to separate the inner edges to reach the desired outer edge separation
            if(-domediff > additionalthickness)
            {
                lensdistance = 0.0f;
            }
            else
            {
                lensdistance = additionalthickness + domediff;
            }
        }
        return (lensdistance, avgoffset);
    }

    (List<Vector3>,List<int>) MakeDomeConnection(List<Vector3> stackedDomeVerts)
    {
        List<int> domeConnects = new List<int>();
        List<Vector3> dupliRingVerts = new List<Vector3>();
        int startFirst = sectionPoints;
        int startSecond = sectionPoints + sectionPoints*domeSegments + 1;

        int firstLensCurrentVert = startFirst;
        int secondLensCurrentVert = startSecond;

        dupliRingVerts.Add(stackedDomeVerts[firstLensCurrentVert]);
        dupliRingVerts.Add(stackedDomeVerts[secondLensCurrentVert]);


        for(int i = 0; i < domeSegments; i++)
        {
            firstLensCurrentVert += sectionPoints;
            secondLensCurrentVert += sectionPoints;

            if(! (i == domeSegments - 1) )
            {
                dupliRingVerts.Add(stackedDomeVerts[firstLensCurrentVert]);
                dupliRingVerts.Add(stackedDomeVerts[secondLensCurrentVert]);

                domeConnects.Add(2 * i);
                domeConnects.Add(2 * i + 2);
                domeConnects.Add(2 * i + 1);
                

                domeConnects.Add(2 * i + 1);
                domeConnects.Add(2 * i + 2);
                domeConnects.Add(2 * i + 3);
                
            }
            else
            {
                domeConnects.Add(2 * i);
                domeConnects.Add(0);
                domeConnects.Add(2 * i + 1);
                

                domeConnects.Add(2 * i + 1);
                domeConnects.Add(0);
                domeConnects.Add(1);
                
            }
        }
        //return dupliringverts, return domeconnects
        return (dupliRingVerts, domeConnects);
    }

    public OpticsLens GetOpticsLens(float distance, float offset, float rad1, float rad2)
    {
        OpticsLens toReturn;

        if (rad1 < 0) toReturn.LeftLeftSegment = true;
        else toReturn.LeftLeftSegment = false;
        if (rad2 < 0) toReturn.RightLeftSegment = true;
        else toReturn.RightLeftSegment = false;

        toReturn.LeftCircle.Radius = Mathf.Abs(rad1);
        toReturn.RightCircle.Radius = Mathf.Abs(rad2);
        toReturn.Radius =  0.5f;
        toReturn.LeftCircle.MidPoint = new Vector2(-distance / 2.0f - offset - rad1, 0.0f);
        toReturn.RightCircle.MidPoint = new Vector2(+distance / 2.0f - offset - rad2, 0.0f);
        toReturn.CauchyA = 1.0f;
        toReturn.CauchyB = 1.0f;
        toReturn.Leftbound = 0.0f;
        toReturn.Rightbound = 0.0f;
        return toReturn;
    }
    void Update()
    {
        //check for lens update.
        if (Mathf.Approximately(RadiusInput1, _radCalcOld) && Mathf.Approximately(RadiusInput2, _radCalcOld2) && Mathf.Approximately(CylinderThickness, _cylinderThicknessOld) && Mathf.Approximately(LensRadius, _lensRadiusOld))
        {
            return;
        }
        else
        {
            _radCalcOld = RadiusInput1;
            _radCalcOld2 = RadiusInput2;
            _lensRadiusOld = LensRadius;
            _cylinderThicknessOld = CylinderThickness;
        }
        Mesh mymesh = new Mesh();
        // this makes the lens holder scale along with the lens
        _lensHolder.transform.localScale = new Vector3(LensRadius, LensRadius, LensRadius);
        // and moves the lens stand along with the lens thickness
        _lensRod.transform.position = new Vector3(_lensRod.transform.position.x, -LensRadius*transform.localScale.y + transform.position.y, _lensRod.transform.position.z);

        (float lensdist, float averageoffset) = CalculateLensPosition(RadiusInput1*LensRadius, RadiusInput2*LensRadius, CylinderThickness);

        _circRadius = CalcRadiusFromTopEdge(RadiusInput1*LensRadius, LensRadius);
        _circRadius2 = CalcRadiusFromTopEdge(RadiusInput2*LensRadius, LensRadius);

        ThisLensLens = GetOpticsLens(lensdist, averageoffset, _circRadius, _circRadius2);

        float sectionAngle = CalculateSectionAngle(_circRadius, LensRadius); //circradius
        float sectionAngle2 = CalculateSectionAngle(_circRadius2, LensRadius);


        var pts = getSectionPoints(sectionPoints, sectionAngle, _circRadius, _circRadius); //circradiuss
        var pts2 = getSectionPoints(sectionPoints, sectionAngle2, _circRadius2, _circRadius2);

        // first vertex is 0 0 0 
        var domeL = GetDomeVertices(pts, domeSegments);
        (List<Vector3> verts, List<int> tris) = MakeFacesAndVertices(domeL);

        var domeL2 = GetDomeVertices(pts2, domeSegments);
        (List<Vector3> verts2, List<int> tris2) = MakeFacesAndVertices(domeL2);

        List<Vector3> lensMeshVerts = new List<Vector3>();
        List<int> lensMeshTris = new List<int>();
        lensMeshTris.AddRange(tris);

        float leftLensDisplacement = -lensdist / 2.0f - averageoffset;
        float rightLensDisplacement = +lensdist / 2.0f - averageoffset;

        Vector3 leftLensMidpoint = new Vector3(leftLensDisplacement - _circRadius, 0.0f, 0.0f);
        Vector3 rightLensMidpoint = new Vector3(rightLensDisplacement - _circRadius2, 0.0f, 0.0f);
        List<Vector3> normals1 = new List<Vector3>(verts.Count);
        List<Vector3> normals2 = new List<Vector3>(verts.Count);

        List<Vector3> lensMeshNormals = new List<Vector3>();

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += new Vector3(leftLensDisplacement, 0.0f, 0.0f);
            normals1.Add(-(verts[i] - leftLensMidpoint).normalized);
        }
        lensMeshVerts.AddRange(verts);
        for (int i = 0; i < verts2.Count; i++)
        {
            verts2[i] += new Vector3(rightLensDisplacement, 0.0f, 0.0f);
            normals2.Add( -(verts2[i] - rightLensMidpoint).normalized);
        }

        lensMeshVerts.AddRange(verts2);

        for(int i = 0; i < tris2.Count; i++)
        {
            tris2[i] += verts.Count;
            //flip normals of 2nd
            if(i%3== 0)
            {
                int temppoly = tris2[i + 1];
                tris2[i + 1] = tris2[i + 2];
                tris2[i + 2] = temppoly; 
            }
        }
        lensMeshTris.AddRange(tris2);

        (var additionalVerts, var additionalTris)=MakeDomeConnection(lensMeshVerts);

        //make normalvectors for the additionalvertices

        Vector3 normalFirstDome  = new Vector3(additionalVerts[0].x, 0.0f, 0.0f);
        Vector3 normalSecondDome = new Vector3(additionalVerts[1].x, 0.0f, 0.0f);
        List<Vector3> additionalNormals = new List<Vector3>();
        for(int i = 0; i < additionalVerts.Count; i+=2)
        {
            additionalNormals.Add(additionalVerts[i] - normalFirstDome);
            additionalNormals.Add(additionalVerts[i + 1] - normalSecondDome);
        }

        for(int i = 0; i< additionalTris.Count; i++) {
            additionalTris[i] += lensMeshVerts.Count;
        }
        lensMeshVerts.AddRange(additionalVerts);
        lensMeshTris.AddRange(additionalTris); //connect the 2 lenscaps

        lensMeshNormals.AddRange(normals1);
        lensMeshNormals.AddRange(normals2);
        lensMeshNormals.AddRange(additionalNormals);

        //swap tris of second half

        List<int> invertedtris = new List<int>(lensMeshTris);

        for(int i= 0; i < invertedtris.Count; i += 3)
        {
            int temp = invertedtris[i];
            invertedtris[i] = invertedtris[i+1];
            invertedtris[i + 1] = temp;
        }

        List<int> duplicatedTris = new List<int>();
        duplicatedTris.AddRange(lensMeshTris);
        duplicatedTris.AddRange(invertedtris);


        mymesh.SetVertices(lensMeshVerts);
        mymesh.SetTriangles(lensMeshTris, 0);
        mymesh.SetNormals(lensMeshNormals);
        //meshfilter handling
        if(_meshFilter == null)
        {
            //add mesh filter if not added already
            _meshFilter = gameObject.GetComponent<MeshFilter>();

        }
        else
        {
            _meshFilter.mesh = mymesh;
            mymesh.RecalculateNormals();
        }
    }
}
