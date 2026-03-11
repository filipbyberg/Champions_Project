using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    // State of preview (green = valid, red = invalid)
    public enum BuildingPreviewState
    {
        POSITIVE,
        NEGATIVE
    }

    // Materials used to indicate placement state
    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

    // Current state
    public BuildingPreviewState State { get; private set; } = BuildingPreviewState.NEGATIVE;

    // Data describing the building
    public BuildingData Data { get; private set; }

    // Instantiated visual model
    public BuildingModel BuildingModel { get; private set; }

    // Cached renderers for fast material swapping
    private List<Renderer> renderers = new();

    // Colliders are disabled in preview
    private List<Collider> colliders = new();

    public void Setup(BuildingData data)
    {
        Data = data;

        // Spawn building model inside preview
        BuildingModel = Instantiate(data.Model, transform.position, Quaternion.identity, transform);

        // Cache renderers + colliders
        renderers.AddRange(BuildingModel.GetComponentsInChildren<Renderer>());
        colliders.AddRange(BuildingModel.GetComponentsInChildren<Collider>());

        // Disable collisions for preview
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Apply initial material
        SetPreviewMaterial(State);
    }

    public void ChangeState(BuildingPreviewState newState)
    {
        // Skip if state hasn't changed
        if (newState == State) return;

        State = newState;

        // Update preview color
        SetPreviewMaterial(State);
    }

    public void Rotate(int rotationStep)
    {
        // Rotate the model wrapper
        BuildingModel.Rotate(rotationStep);
    }

    private void SetPreviewMaterial(BuildingPreviewState newState)
    {
        // Select correct material
        Material previewMat = newState == BuildingPreviewState.POSITIVE ? positiveMaterial : negativeMaterial;

        foreach (var rend in renderers)
        {
            // Replace all renderer materials
            Material[] mats = new Material[rend.sharedMaterials.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = previewMat;
            }

            rend.materials = mats;
        }
    }
}