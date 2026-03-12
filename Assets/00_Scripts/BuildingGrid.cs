using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGrid : MonoBehaviour
{
    // Number of cells in the X direction
    [SerializeField] private int width;

    // Number of cells in the Z direction
    [SerializeField] private int height;

    // 2D array storing all grid cells
    private BuildingGridCell[,] grid;

    private void Start()
    {
        // Create the grid array with the defined width and height
        grid = new BuildingGridCell[width, height];

        // Initialize every grid cell with an empty BuildingGridCell object
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new();
            }
        }
    }

    public void ClearGrid()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new BuildingGridCell();
            }
        }
    }

    // Marks the grid cells as occupied by a building
    public void SetBuilding(Building building, List<Vector3> allBuildingPositions)
    {
        // Loop through every world position the building occupies
        foreach (var p in allBuildingPositions)
        {
            // Convert world position to grid coordinates
            (int x, int y) = WorldToGridPosition(p);

            // Store the building in that grid cell
            grid[x, y].SetBuilding(building);
        }
    }

    public void RemoveBuilding(List<Vector3> positions)
    {
        foreach (var p in positions)
        {
            (int x, int y) = WorldToGridPosition(p);
            grid[x, y] = new BuildingGridCell();
        }
    }

    // Checks if a building can be placed on the given grid positions
    public bool CanBuild(List<Vector3> allBuildingPositions)
    {
        foreach (var p in allBuildingPositions)
        {
            // Convert world position to grid coordinates
            (int x, int y) = WorldToGridPosition(p);

            // Check if position is outside the grid boundaries
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            // Check if the cell already contains a building
            if (!grid[x, y].IsEmpty())
                return false;
        }

        // All positions are valid
        return true;
    }

    // Converts a world-space position into a grid coordinate
    private (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        // Calculate X index based on cell size and grid origin
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / BuildingSystem.CellSize);

        // Calculate Y index (grid Z direction)
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / BuildingSystem.CellSize);

        return (x, y);
    }

    private void OnDrawGizmos()
    {
        // Set grid line color in the Scene view
        Gizmos.color = Color.yellow;

        // Avoid drawing if grid settings are invalid
        if (BuildingSystem.CellSize <= 0 || width <= 0 || height <= 0) return;

        // Grid origin point in world space
        Vector3 origin = transform.position;

        // Draw horizontal grid lines
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * BuildingSystem.CellSize);
            Vector3 end = origin + new Vector3(width * BuildingSystem.CellSize, 0.01f, y * BuildingSystem.CellSize);

            Gizmos.DrawLine(start, end);
        }

        // Draw vertical grid lines
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, height * BuildingSystem.CellSize);

            Gizmos.DrawLine(start, end);
        }
    }
}


// Represents a single cell in the building grid
public class BuildingGridCell
{
    // Reference to the building occupying this cell
    private Building building;

    // Assigns a building to this cell
    public void SetBuilding(Building building)
    {
        this.building = building;
    }

    // Returns true if the cell does not contain a building
    public bool IsEmpty()
    {
        return building == null;
    }
}