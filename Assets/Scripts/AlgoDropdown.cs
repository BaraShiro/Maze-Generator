using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class AlgoDropdown : MonoBehaviour
{
    public enum Algos
    {
        DFS = 0,
        Kruskal = 1,
        Prim = 2,
        AldousBroder = 3,
    }

    private TMP_Dropdown dropdown;
    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.options = new List<TMP_Dropdown.OptionData>();
        foreach (string algoName in Enum.GetNames(typeof(Algos)))
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(algoName));
        }
        
    }
}
