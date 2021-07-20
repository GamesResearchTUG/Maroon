//
//Author: Tobias Stöckl
//
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LPProperties : MonoBehaviour
{

    public Color LaserColor = Color.red;
    public float LaserIntensity = 1.0f;
    public float LaserWavelength = 680f;


    public Color[] SpectrumColors = new Color[20];

    private Color LinearInterpolate(Color col1, Color col2, float interpolFactor)
    {
        return col1*(1-interpolFactor) + col2*(interpolFactor);
    }

    public void SetLaserColor()
    {
        SetLaserColor(LaserWavelength);
    }

    public void SetLaserColor(float wavelength)
    {
        //wavelength between 0 and 1 from 380-720nm
        wavelength = (wavelength - 380.0f) / 340.0f;
        wavelength = Mathf.Clamp(wavelength, 0.0f, 0.9999f); //just to be sure

        List<Color> colors = new List<Color>();

        colors.AddRange(SpectrumColors);

        int toInterpolate = colors.Count - 1;

        float interpolationFactor = wavelength * toInterpolate;
        int firstColorIndex = (int)Mathf.Floor(interpolationFactor);

        float interpolatePercent = interpolationFactor - firstColorIndex;

        Color interpolatedColor = LinearInterpolate(colors[firstColorIndex], colors[firstColorIndex + 1], interpolatePercent);

        LaserColor = interpolatedColor;
        return;
    }


    public float GetCauchyForCurrentWavelength()
    {

        OpticsLens currentLens = FindObjectOfType<LaserRenderer>().CurrentLens;

        OpticsSim os = FindObjectOfType<OpticsSim>();

        return os.GetCauchy(currentLens.CauchyA, currentLens.CauchyB, LaserWavelength);
    }

}
