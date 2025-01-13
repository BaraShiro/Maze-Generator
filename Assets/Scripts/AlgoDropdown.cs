using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A dropdown menu with the different maze generation algorithms.
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class AlgoDropdown : MonoBehaviour
{
    /// <summary>
    /// The different maze generation algorithms.
    /// </summary>
    public enum Algos
    {
        DFS = 0,
        Kruskal = 1,
        Prim = 2,
        AldousBroder = 3,
        Wilson = 4,
        AldousBroderWilson = 5,
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
