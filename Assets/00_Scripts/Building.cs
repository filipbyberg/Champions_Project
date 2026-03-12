using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Building : MonoBehaviour
{
    public string Description => data.Description;
    public int Cost => data.Cost;
    private BuildingModel model;
    private BuildingData data;
    private List<Vector3> occupiedPositions;
    public void Setup(BuildingData data, float rotation, List<Vector3> positions)
    {
        this.data = data;

        // Store occupied grid positions
        occupiedPositions = positions;

        model = Instantiate(data.Model, transform.position, Quaternion.identity, transform);
        model.Rotate(rotation);
    }

    public List<Vector3> GetOccupiedPositions()
    {
        return occupiedPositions;
    }
}
