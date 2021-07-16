using System;
using UnityEngine;

public class DiamondCollector : MonoBehaviour
{
    private GameObject[] _diamonds;

    private int _collectedDiamonds;

    public int DiamondCount;


    [SerializeField] private GameObject[] stars = new GameObject[3];

    private void Start()
    {
        _diamonds = GameObject.FindGameObjectsWithTag("Diamond");
    }

    public void CollectDiamond()
    {
        _collectedDiamonds++;
        UpdateStars();
    }

    private void UpdateStars()
    {

    }
}
