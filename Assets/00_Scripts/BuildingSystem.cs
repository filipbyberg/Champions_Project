using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    // Size of one grid cell (used by grid + snapping)
    public const float CellSize = 1f;

    // Different building types that can be selected
    [SerializeField] private BuildingData buildingData1;
    [SerializeField] private BuildingData buildingData2;
    [SerializeField] private BuildingData buildingData3;
    [SerializeField] private BuildingData buildingData4;
    [SerializeField] private BuildingData buildingData5;
    [SerializeField] private BuildingData buildingData6;
    [SerializeField] private BuildingData buildingData7;

    // Prefabs for preview (ghost object) and final building
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;

    // Reference to the grid that tracks occupied cells
    [SerializeField] private BuildingGrid grid;

    // Current active preview object
    private BuildingPreview preview;

    private void Update()
    {
        // Get where the mouse hits the ground
        Vector3 mousePos = GetMouseWorldPosition();

        if (preview != null)
        {
            // If preview exists -> move and validate it
            HandePreview(mousePos);
        }
        else
        {
            // If no preview exists -> allow player to select building type
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                preview = CreatePreview(buildingData1, mousePos);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                preview = CreatePreview(buildingData2, mousePos);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                preview = CreatePreview(buildingData3, mousePos);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                preview = CreatePreview(buildingData4, mousePos);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                preview = CreatePreview(buildingData5, mousePos);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                preview = CreatePreview(buildingData6, mousePos);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                preview = CreatePreview(buildingData7, mousePos);
            }
        }
    }

    private void HandePreview(Vector3 mouseWorldPosition)
    {
        // Move preview to the mouse position
        preview.transform.position = mouseWorldPosition;

        // Get all grid positions occupied by this building
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPositions();

        // Ask the grid if the building can be placed there
        bool canBuild = grid.CanBuild(buildPosition);

        if (canBuild)
        {
            // Snap preview to grid center
            preview.transform.position = GetSnappedCenterPosition(buildPosition);

            // Change preview color to green
            preview.ChangeState(BuildingPreview.BuildingPreviewState.POSITIVE);

            // Left click -> place building
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(buildPosition);
            }
        }
        else
        {
            // Show red preview if placement is invalid
            preview.ChangeState(BuildingPreview.BuildingPreviewState.NEGATIVE);
        }

        // Rotate building with R
        if (Input.GetKeyDown(KeyCode.R))
        {
            preview.Rotate(90);
        }
    }

    private void PlaceBuilding(List<Vector3> buildingPosition)
    {
        // Create the final building object
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity);

        // Initialize it with building data and rotation
        building.Setup(preview.Data, preview.BuildingModel.Rotation);

        // Mark the grid cells as occupied
        grid.SetBuilding(building, buildingPosition);

        // Destroy preview and reset
        Destroy(preview.gameObject);
        preview = null;
    }

    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPosition)
    {
        // Convert world positions to grid coordinates
        List<int> xs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.x)).ToList();
        List<int> zs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.z)).ToList();

        // Find center of the building footprint
        float centerX = ((xs.Min() + xs.Max()) / 2f + CellSize / 2f)/1f;
        float centerZ = ((zs.Min() + zs.Max()) / 2f + CellSize / 2f)/1f;

        return new(centerX, 0, centerZ);
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Cast a ray from the camera through the mouse cursor
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Define a horizontal plane at y = 0
        Plane groundPlane = new(Vector3.up, Vector3.zero);

        // If ray hits the plane, return hit point
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
    {
        // Spawn preview object
        BuildingPreview buildingPreview = Instantiate(previewPrefab, position, Quaternion.identity);

        // Initialize preview with selected building data
        buildingPreview.Setup(data);

        return buildingPreview;
    }
}