using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private PlayerInput playerInput;

    private TilePlacer tilePlacer;
    private PrefabPlacer prefabPlacer;

    private placementMode currentMode;

    public enum placementMode
    {
        DISABLED,
        ROAD_PLACEMENT,
        CAR_PLACEMENT
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        tilePlacer = GetComponent<TilePlacer>();
        prefabPlacer = GetComponent<PrefabPlacer>();

        tilePlacer.enabled = false;
        prefabPlacer.enabled = false;
        SetMode(placementMode.ROAD_PLACEMENT);
    }

    private void OnEnable()
    {
        playerInput.enabled = true; 
    }

    private void OnDisable()
    {
        playerInput.enabled = false;
    }

    public void SetMode(placementMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }

        currentMode = mode;

        switch (mode)
        {
            case placementMode.DISABLED:
                prefabPlacer.enabled = false;
                tilePlacer.enabled = false;
                break;

            case placementMode.ROAD_PLACEMENT :
                prefabPlacer.enabled = false;
                tilePlacer.enabled = true;
                break;

            case placementMode.CAR_PLACEMENT :
                tilePlacer.enabled = false;
                prefabPlacer.enabled = true;
                break;
        }
    }

}
