//
//Author: Tobias Stöckl
//
using UnityEngine;

public class ToggleUIElement : MonoBehaviour
{
    [SerializeField]
    public GameObject UiElement;
    private bool _isActive = true;


    public void ToggleElement()
    {
        _isActive = !_isActive;
        UiElement.SetActive(_isActive);
    }
}
