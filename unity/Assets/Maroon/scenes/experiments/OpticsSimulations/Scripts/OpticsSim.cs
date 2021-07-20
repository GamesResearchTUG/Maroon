//
//Author: Tobias Stöckl
//
using System.Collections.Generic;
using UnityEngine;

public struct OpticsRay
{
    public Vector2 Origin;
    public Vector2 Direction;
    public float Intensity;
    public Color RayColor;
    public float WavelengthMultiplier;



    public OpticsRay(Vector2 origin, Vector2 dir, float intensity)
    {
        this.Origin = origin;
        this.Direction = dir;
        this.Intensity = intensity;
        this.RayColor = Color.red;
        this.WavelengthMultiplier = 1.0f;
    }
    public OpticsRay(Vector2 origin, Vector2 dir, float intensity, Color rayColor, float wavelengthmult = 886.0f)
    {
        this.Origin = origin;
        this.Direction = dir;
        this.Intensity = intensity;
        this.RayColor = rayColor;
        this.WavelengthMultiplier = wavelengthmult;
    }
}

public struct OpticsSegment
{
    public Vector2 P1;
    public Vector2 P2;
    public float Intensity;
    public Color SegmentColor;

    public OpticsSegment(Vector2 p1, Vector2 p2, float intensity)
    {
        this.P1 = p1;
        this.P2 = p2;
        this.Intensity = intensity;
        this.SegmentColor = Color.red;
    }
}

public struct OpticsCircle
{
    public Vector2 MidPoint;
    public float Radius; 
}

public struct OpticsLens
{
    public OpticsCircle LeftCircle;
    public bool LeftLeftSegment;
    public OpticsCircle RightCircle;
    public bool RightLeftSegment;
    public float Radius;
    public float CauchyA;
    public float CauchyB;
    public float Leftbound;
    public float Rightbound;
}


public class OpticsSim : MonoBehaviour
{

    public static float AIR_REFRACTIVE_INDEX = 1.01f;

    public (bool didhit, Vector2 inter1, Vector2 inter2) IntersectRay(OpticsRay toIntersect, OpticsCircle interCircle)
    {

        Vector2 localP1 = toIntersect.Origin - interCircle.MidPoint;
        Vector2 localP2 = toIntersect.Origin + toIntersect.Direction - interCircle.MidPoint;

        Vector2 p2minp1 = localP2 - localP1;

        float a = p2minp1.x * p2minp1.x + p2minp1.y * p2minp1.y;
        float b = 2.0f * ((p2minp1.x * localP1.x) + (p2minp1.y * localP1.y));
        float c = (localP1.x * localP1.x) + localP1.y * localP1.y - interCircle.Radius*interCircle.Radius;

        float delta = b * b - (4 * a * c);

        if (delta < 0.0f)
        {
            return (false, new Vector2(0, 0), new Vector2(0, 0));
        }

        float sqrtdel = Mathf.Sqrt(delta);
        float u1 = (-b + sqrtdel) / (2 * a);
        float u2 = (-b - sqrtdel) / (2 * a);


        return (true, toIntersect.Origin + (u1*p2minp1), toIntersect.Origin + (u2*p2minp1));
    }

    public int GetNearestRayHit(List<Vector2> hitPoints, OpticsRay rayToCheck) // todo could be not correctly implemented yet
    {

        //iterate over distances, check for shortest distance. remove all hitpoints that are behind the ray. 
        int nearestIndex = -1;
        float magn = float.MaxValue;

        int idx = 0;
        foreach(var hitPoint in hitPoints)
        {
            Vector2 fromto = hitPoint - rayToCheck.Origin;

            if(fromto.magnitude < magn && Vector2.Dot(fromto, rayToCheck.Direction) > 0.0f)
            {
                magn = fromto.magnitude;
                nearestIndex = idx;
            }
            idx++;
        }
        return nearestIndex;
    }

    public bool PointInRectangleBounds(Vector2 topLeft, Vector2 botRight, Vector2 p)
    {
        if (p.x > topLeft.x && p.x < botRight.x && p.y < topLeft.y && p.y > botRight.y) return true;

        return false;
    }

    public (float,float) GetBoundsHit(OpticsLens lens)
    {
        Vector2 upperLensBound = lens.LeftCircle.MidPoint + new Vector2(0, lens.Radius);
        OpticsRay upperBound = new OpticsRay(upperLensBound, new Vector2(1, 0), 1.0f);

        float leftBound;
        float rightBound;

        (_, Vector2 int1, Vector2 int2) = IntersectRay(upperBound, lens.LeftCircle);

        if (lens.LeftLeftSegment)
        {
            leftBound = int2.x;
        }
        else
        {
            leftBound = int1.x;
        }

        (_, int1, int2) = IntersectRay(upperBound, lens.RightCircle);

        if (lens.RightLeftSegment)
        {
            rightBound = int2.x;
        }
        else
        {
            rightBound = int1.x;
        }
        return (leftBound, rightBound);
    }

    public (bool, bool, Vector2, Vector2, Vector2, Vector2) GetOuterRingHit(OpticsRay currentRay, OpticsLens lens)
    {
        //get if parallel, if not intersect, then test for in bounds

        if(currentRay.Direction.y < 0.00001f && currentRay.Direction.y > -0.00001f)
        {
            return (false, false, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero); // return no hitpoint
        }
        //else intersect the 1 ray with the other 2, get the 2 hitpoints, check if they are in the respective correct x bounds
        //and return bool bool point point normal normal
        (float leftb, float rightb) = GetBoundsHit(lens);

        float upperRay = lens.LeftCircle.MidPoint.y + lens.Radius;
        float lowerRay = lens.LeftCircle.MidPoint.y - lens.Radius;

        float inc = currentRay.Direction.y / currentRay.Direction.x;
        
        float offs = currentRay.Origin.y + (-currentRay.Origin.x) * inc;

        float upperHitPoint = (upperRay - offs) * (1.0f / inc);
        float lowerHitPoint = (lowerRay - offs) * (1.0f / inc);

        bool upperIn = false;
        bool lowerIn = false;

        if (upperHitPoint > leftb && upperHitPoint < rightb) upperIn = true;
        if (lowerHitPoint > leftb && lowerHitPoint < rightb) lowerIn = true;


        return (upperIn, lowerIn, new Vector2(upperHitPoint, upperRay), new Vector2(lowerHitPoint, lowerRay), Vector2.down, Vector2.up);

    }
    public (bool, bool,  Vector2, Vector2, Vector2 nor1, Vector2 nor2) GetSegmentHit(OpticsRay currentRay, OpticsCircle circ, bool isLeftSegment, float diameter = 1.0f)
    {

        (bool didHit, Vector2 int1, Vector2 int2) = IntersectRay(currentRay, circ);
        // if hit and there is atleast one intersection on the correct side and in the limits of the diameter (y-coord), add it
        Vector2 topLeft;
        Vector2 botRight;

        Vector2 nor1 = new Vector2(0.0f, 0.0f);
        Vector2 nor2 = new Vector2(0.0f, 0.0f);
        
        // make hitbox for inside-check
        if (isLeftSegment)
        {
            topLeft = new Vector2(circ.MidPoint.x - circ.Radius * 2.0f,circ.MidPoint.y + diameter);
            botRight = new Vector2(circ.MidPoint.x, circ.MidPoint.y + diameter*-1.0f);
        }else
        {
            topLeft = new Vector2(circ.MidPoint.x , circ.MidPoint.y + diameter);
            botRight = new Vector2(circ.MidPoint.x+ circ.Radius*2.0f, circ.MidPoint.y+ diameter*-1.0f);
        }

        bool h1 = false, h2 = false;
        if (didHit)
        {
            h1 = PointInRectangleBounds(topLeft, botRight, int1);
            h2 = PointInRectangleBounds(topLeft, botRight, int2);
            nor1 = int1 - circ.MidPoint;
            nor2 = int2 - circ.MidPoint;
        }
        return (h1, h2, int1, int2, nor1, nor2);
    }

    public Vector2 GetReflection(Vector2 norm, Vector2 entryDir)
    {
        norm = norm.normalized;
        entryDir = entryDir.normalized;
        float nor_length = Vector2.Dot(entryDir, norm);

        Vector2 ret1 = -nor_length*norm*2.0f + entryDir;
        return ret1;
    }

    public float GetCauchy(float A, float B, float wavelength)
    {
        float wlcauchy = wavelength / 1000;
        return A + B / (wlcauchy * wlcauchy);
    }

    public (Vector2, bool totalinternalreflection) GetRefraction(Vector2 normal, Vector2 entryDir, float refractiveIndex1, float refractiveIndex2)
    {

        //refractive multiplier, add to the refracive indices to account for wavelength differences
        // get entry angle

        float angle = Mathf.Acos(Vector2.Dot( entryDir.normalized, normal.normalized)); 

        if(angle*Mathf.Rad2Deg > 90.0f)
        {
            normal = -normal;
        }
        float sign_angle = Vector2.SignedAngle(normal.normalized, entryDir.normalized);
        float t1 = Mathf.Sin(angle) * refractiveIndex1;

        float t2 = t1 / refractiveIndex2;
        float res = Mathf.Asin(t2);

        if(float.IsNaN(res)) //nan -> total internal reflection
        {
            return (new Vector2(0, 0), true);
        }
        // no total reflection, now we calc refraction direction
        if (sign_angle < 0)
        {
            res = -res;
        }
        return (RotateVector(normal, res), false);
    }

    // https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
    private Vector2 RotateVector(Vector2 toRotate, float rad)
    {

        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        float tx = toRotate.x;
        float ty = toRotate.y;

        toRotate.x = (cos * tx) - (sin * ty);
        toRotate.y = (sin * tx) + (cos * ty);

        return toRotate;
    }



    public (Vector2, Vector2, bool) GetFirstLensHit(OpticsRay r, OpticsLens lens)
    {
        // returns hitpoint and normal, or miss (bool) 
        bool[] hit = new bool[6];
        Vector2[] intersects = new Vector2[6];
        Vector2[] normals = new Vector2[6];

        List<Vector2> hits = new List<Vector2>(6);
        List<Vector2> hitnormals = new List<Vector2>(6);

        (hit[0], hit[1], intersects[0], intersects[1], normals[0], normals[1]) = GetSegmentHit(r, lens.LeftCircle, lens.LeftLeftSegment, lens.Radius);
        (hit[2], hit[3], intersects[2], intersects[3], normals[2], normals[3]) = GetSegmentHit(r, lens.RightCircle, lens.RightLeftSegment, lens.Radius);
        (hit[4], hit[5], intersects[4], intersects[5], normals[4], normals[5]) = GetOuterRingHit(r, lens);

        for(int i = 0; i < hit.Length; i++)
        {
            if (hit[i])
            {
                hits.Add(intersects[i]);
                hitnormals.Add(normals[i]);
            }
        }

        int hitidx = GetNearestRayHit(hits, r);     

        if(hitidx < 0)
        {
            return ( Vector2.zero, Vector2.zero, false); // nothing hit
        }
        else
        {
            return ( hits[hitidx], hitnormals[hitidx], true); // hits, hitsnormals @ hitidx
        }
    }




    public void CalculateHitsRecursive(OpticsRay myRay, OpticsLens myLens, bool outside, ref List<OpticsSegment> segs, float loss = 0.9f, float reflectionVsRefraction = 0.3f)
    {
        if (myRay.Intensity < 0.02) return; // dont go further if intensity lower than 0.02

        (Vector2 hit, Vector2 norm, bool didHit) = GetFirstLensHit(myRay, myLens);

        if (!didHit)
        {
            OpticsSegment addEnd = new OpticsSegment(myRay.Origin, myRay.Origin + 30.0f * myRay.Direction, myRay.Intensity);
            addEnd.SegmentColor = myRay.RayColor;
            segs.Add(addEnd);
            return; // we are done here... nothing hit.
        }

        OpticsSegment toAdd = new OpticsSegment(myRay.Origin, hit, myRay.Intensity);
        toAdd.SegmentColor = myRay.RayColor;
        segs.Add(toAdd);

        var innerref = GetCauchy(myLens.CauchyA, myLens.CauchyB, myRay.WavelengthMultiplier);
        var outerref = AIR_REFRACTIVE_INDEX;

        if (!outside)
        {
            var temp = innerref;
            innerref = outerref;
            outerref = temp;
        }

        (Vector2 refracDir, bool wasTotalRefraction) = GetRefraction(norm, myRay.Direction, outerref, innerref);

        Vector2 reflecDir = GetReflection(-norm, myRay.Direction);

        if (wasTotalRefraction)
        { // was  a total reflection, only follow the reflection vector.
            OpticsRay reflectedRay = new OpticsRay(hit + 0.0001f * reflecDir.normalized, reflecDir.normalized, myRay.Intensity * 1.0f *loss, myRay.RayColor, myRay.WavelengthMultiplier);
            CalculateHitsRecursive(reflectedRay, myLens, outside, ref segs, loss, reflectionVsRefraction);
            return;
        }
        else
        {
            OpticsRay reflectedRay = new OpticsRay(hit + 0.0001f * reflecDir.normalized, reflecDir.normalized, myRay.Intensity * reflectionVsRefraction*loss, myRay.RayColor, myRay.WavelengthMultiplier);
            OpticsRay refractedRay = new OpticsRay(hit + 0.0001f * refracDir.normalized, refracDir.normalized, myRay.Intensity * (1.0f - reflectionVsRefraction)*loss, myRay.RayColor, myRay.WavelengthMultiplier);

            CalculateHitsRecursive(reflectedRay, myLens, outside, ref segs, loss, reflectionVsRefraction);
            CalculateHitsRecursive(refractedRay, myLens, !outside, ref segs, loss, reflectionVsRefraction);
        }
    }
}

