using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    // Size of one grid cell (used by grid + snapping)
    public const float CellSize = 1f;
    private bool isOffset = false; // This keeps track of whether the layout is currently moved.

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject builderCamera;
    [SerializeField] private GameObject explorerPlayer;

    // Different building types that can be selected
    [SerializeField] private List<BuildingData> availableBuildings = new();

    // Prefabs for preview (ghost object) and final building
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;

    // Reference to the grid that tracks occupied cells
    [SerializeField] private BuildingGrid grid;

    // UI Content
    [SerializeField] private RectTransform buildingListContent; // ScrollView content
    [SerializeField] private GameObject buildingButtonPrefab;   // Button prefab for each building
    [SerializeField] private GameObject buildingUI;

    // Current active preview object
    private BuildingPreview preview;
    private List<Building> placedBuildings = new();

    private void Start()
    {
        CreateBuildingButtons();
    }
    private void Update()
    {
        // Move build to the Rig
        if (Input.GetKeyDown(KeyCode.I) && preview == null)
        {
            ToggleBuildingOffset();
        }

        // only update if builder camera is active
        if (!builderCamera.activeSelf) return;

        // Clear all buildings
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllBuildings();
        }

        // Open/close building UI with the "1" key
        if (!explorerPlayer.activeSelf && Input.GetKeyDown(KeyCode.Alpha1))
        {
            buildingUI.SetActive(!buildingUI.activeSelf);
        }

        // Get where the mouse hits the ground
        Vector3 mousePos = GetMouseWorldPosition();

        // Allow deleting buildings when not placing new ones
        if (preview == null && Input.GetMouseButtonDown(1))
        {
            Building hovered = GetHoveredBuilding();

            if (hovered != null)
            {
                grid.RemoveBuilding(hovered.GetOccupiedPositions());
                placedBuildings.Remove(hovered);
                Destroy(hovered.gameObject);
            }
        }

        // If preview exists -> move and validate it
        if (preview != null)
        {
            HandePreview(mousePos);
        }
        else
        {
            // If we want to just use key 1, 2, 3 etc for placing buildings
            //HandleBuildingSelection(mousePos);
        }
    }
    //Create a method to generate buttons for all availableBuilding
    private void CreateBuildingButtons()
    {
        foreach (Transform child in buildingListContent)
            Destroy(child.gameObject); // clear previous buttons

        for (int i = 0; i < availableBuildings.Count; i++)
        {
            BuildingData data = availableBuildings[i];

            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingListContent);
            buttonObj.name = "Button_" + data.name;

            // Set text label (if using Text or TMP)
            var textComponent = buttonObj.GetComponentInChildren<TMP_Text>(); // or TMP_Text
            if (textComponent != null)
                textComponent.text = data.Name; // Display the building's unique name

            UnityEngine.UI.Button btn = buttonObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    // Spawn preview at mouse position
                    Vector3 mousePos = GetMouseWorldPosition();
                    preview = CreatePreview(data, mousePos);

                    // Hide the building UI automatically
                    buildingUI.SetActive(false);
                });
            }
        }
    }
    // check all availableBuildings and assign keys dynamically
    private void HandleBuildingSelection(Vector3 mousePos)
    {
        for (int i = 0; i < availableBuildings.Count; i++)
        {
            // KeyCode.Alpha1 = 49, Alpha2 = 50, etc.
            KeyCode key = KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(key) && !isOffset)
            {
                preview = CreatePreview(availableBuildings[i], mousePos);
            }
        }
    }
    // Method for putting all buildings to the rig and reverse
    private void ToggleBuildingOffset()
    {
        Vector3 offset = new Vector3(45f, 22f, 20f);

        if (isOffset)
            offset = -offset;

        foreach (var building in placedBuildings)
        {
            building.transform.position += offset;
        }

        isOffset = !isOffset;

        ToggleExploreMode();
    }
    private void ToggleExploreMode()
    {
        if (explorerPlayer.activeSelf)
        {
            explorerPlayer.SetActive(false);
            builderCamera.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            explorerPlayer.SetActive(true);
            builderCamera.SetActive(false);

            // Reset position
            explorerPlayer.transform.position = new Vector3(32f, 24f, 20f);


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ClearAllBuildings()
    {
        // Destroy all placed buildings
        foreach (var building in placedBuildings)
        {
            Destroy(building.gameObject);
        }

        // Clear the list
        placedBuildings.Clear();

        // Reset the grid
        grid.ClearGrid();

        // Remove preview if it exists
        if (preview != null)
        {
            Destroy(preview.gameObject);
            preview = null;
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
        building.Setup(preview.Data, preview.BuildingModel.Rotation, buildingPosition);

        // Mark the grid cells as occupied
        grid.SetBuilding(building, buildingPosition);

        // Store the building so we can delete it later
        placedBuildings.Add(building);

        // Destroy preview and reset
        Destroy(preview.gameObject);
        preview = null;
    }
    private Building GetHoveredBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInParent<Building>();
        }

        return null;
    }

    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPosition)
    {
        // Convert world positions to grid coordinates
        List<int> xs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.x)).ToList();
        List<int> zs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.z)).ToList();

        // Find center of the building footprint
        float centerX = ((xs.Min() + xs.Max()) / 2f + CellSize / 2f);
        float centerZ = ((zs.Min() + zs.Max()) / 2f + CellSize / 2f);

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