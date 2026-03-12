using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Building")]

public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }       // NEW: display name
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public BuildingModel Model { get; private set; }
}
