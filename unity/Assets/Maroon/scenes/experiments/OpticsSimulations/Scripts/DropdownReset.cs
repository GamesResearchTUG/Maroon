//
//Author: Tobias Stöckl
//
using UnityEngine;
using Maroon.UI;

public class DropdownReset : MonoBehaviour
{

    public void ResetDropDownMenu()
    {
        var dropDown = gameObject.GetComponent<Dropdown>();

        dropDown.ResetObject();
    }

}
