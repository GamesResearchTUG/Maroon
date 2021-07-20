//
// Author: Michael Holly 
//
// Modifications: Tobias Stöckl
//
using UnityEngine;
using Maroon.Physics;
using Maroon.UI;

[RequireComponent(typeof(QuantityPropertyView))]
public class SliderBackgroundImage : MonoBehaviour
{
    [SerializeField] private Sprite _imageSource;


    // on function call, sets the backgroundimage of a quantitypropertyview of the object this script is attached to.
    // fillimage is set to transparent, so that one can see the whole backgroundimage.
    public void UpdateSliderBackgroundImage()
    {
        var quantityPropertyView = GetComponent<QuantityPropertyView>();
        var quantity = quantityPropertyView.quantity.Value;

        if (!(quantity is QuantityInt) && !(quantity is QuantityFloat))
            return;

        var slider = gameObject.GetComponentInChildren<Slider>();
        var backgroundImage = slider.transform.Find("Background").GetComponent<UnityEngine.UI.Image>();
        var fillImage = slider.transform.Find("Fill Area").transform.Find("Fill").GetComponent<UnityEngine.UI.Image>();
        fillImage.color = new Color(1, 1, 1, 0); //transparent
        backgroundImage.sprite = _imageSource;
    }
}