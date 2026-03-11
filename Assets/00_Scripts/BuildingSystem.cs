using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;
    [SerializeField] private BuildingData buildingData1;
    [SerializeField] private BuildingData buildingData2;
    [SerializeField] private BuildingData buildingData3;
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;
    [SerializeField] private BuildingGrid grid;
    private BuildingPreview preview;

    private void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        if (preview != null)
        {
            HandePreview(mousePos);
        }
        else
        {
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
        }
    }
    private void HandePreview(Vector3 mouseWorldPosition)
    {
        preview.transform.position = mouseWorldPosition;
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPositions();
        bool canBuild = grid.CanBuild(buildPosition);
        if (canBuild)
        {
            preview.transform.position = GetSnappedCenterPosition(buildPosition);
            preview.ChangeState(BuildingPreview.BuildingPreviewState.POSITIVE);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(buildPosition);
            }
        }
        else 
        {
            preview.ChangeState(BuildingPreview.BuildingPreviewState.NEGATIVE);
        }
        if  (Input.GetKeyDown(KeyCode.R))
        {
            preview.Rotate(90);
        }
    }
    private void PlaceBuilding(List<Vector3> buildingPosition)
    {
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity);
        building.Setup(preview.Data, preview.BuildingModel.Rotation);
        grid.SetBuilding(building, buildingPosition);
        Destroy(preview.gameObject);
        preview = null;
    }
    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPosition)
    {
        List<int> xs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.x)).ToList();
        List<int> zs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.z)).ToList();
        float centerX = (xs.Min() + xs.Max()) / 2f + CellSize / 2f;
        float centerZ = (zs.Min() + zs.Max()) / 2f + CellSize / 2f;
        return new(centerX, 0, centerZ);
    }
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
    private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
    {
        BuildingPreview buildingPreview = Instantiate(previewPrefab, position, Quaternion.identity);
        buildingPreview.Setup(data);
        return buildingPreview;
    }
}
