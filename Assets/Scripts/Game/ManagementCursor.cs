using UnityEngine;
using UnityEngine.InputSystem;

public class ManagementCursor : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference selectAction;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private InputActionReference toggleTimeAction;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minZ = -20f;
    [SerializeField] private float maxZ = 20f;

    [Header("Game")]
    [SerializeField] private GameManager gameManager;

    private Vector3 startPosition;


    [Header("Building Selection")]
    [SerializeField] private BuildingData woodcutterData;
    [SerializeField] private BuildingData quarryData;
    [SerializeField] private BuildingData smelterData;

    private BuildingData selectedBuilding;
    private SimulationManager simulationManager;


    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        selectAction.action.Enable();
        cancelAction.action.Enable();
        toggleTimeAction.action.Enable();

        selectAction.action.performed += OnSelect;
        cancelAction.action.performed += OnCancel;
        toggleTimeAction.action.performed += OnToggleTime;
    }

    private void OnDisable()
    {
        selectAction.action.performed -= OnSelect;
        cancelAction.action.performed -= OnCancel;
        toggleTimeAction.action.performed -= OnToggleTime;

        moveAction.action.Disable();
        selectAction.action.Disable();
        cancelAction.action.Disable();
        toggleTimeAction.action.Disable();
    }

    private void Update()
    {
        HandleMove();
        HandleRawInput();
        HandleBuildingSelection();
    }

    private void HandleMove()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 movement = new Vector3(input.x, 0f, input.y);

        Vector3 newPosition =
            transform.position + movement * moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(
            newPosition.x,
            startPosition.x + minX,
            startPosition.x + maxX
        );

        newPosition.z = Mathf.Clamp(
            newPosition.z,
            startPosition.z + minZ,
            startPosition.z + maxZ
        );

        transform.position = newPosition;
    }

    private void HandleRawInput()
    {
        if (Keyboard.current != null &&
            Keyboard.current.homeKey.wasPressedThisFrame)
        {
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;

            Debug.Log("Raw Input: Cursor reset");
        }
    }


    private void HandleBuildingSelection()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectBuilding(woodcutterData);
            Debug.Log("Selected Woodcutter");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectBuilding(quarryData);
            Debug.Log("Selected Quarry");
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectBuilding(smelterData);
            Debug.Log("Selected Smelter");
        }

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame &&
            gameManager != null &&
            gameManager.CurrentState == GameState.Gameplay &&
            selectedBuilding != null)
                {
                    SelectBuilding(null);
                    GameEventSystem.PlayerMessage(
                        "Building mode cancelled. Select a built building to upgrade it."
                    );
                }
    }


    private void OnSelect(InputAction.CallbackContext context)
    {
        if (gameManager == null ||
            gameManager.CurrentState != GameState.Gameplay)
            return;

        if (simulationManager == null)
            simulationManager = GetComponentInParent<SimulationManager>();

        if (simulationManager == null)
        {
            Debug.LogError("SimulationManager not found!");
            return;
        }

        Vector2Int gridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        if (selectedBuilding == null)
        {
            ProductionBuilding building =
                simulationManager.GetProductionBuildingAt(gridPosition);

            if (building == null)
            {
                GameEventSystem.PlayerMessage(
                    "No building here. Select a building to place or inspect."
                );
                return;
            }

            GameEventSystem.ProductionBuildingSelected(building);
            GameEventSystem.PlayerMessage($"{building.Data.Type} selected.");
            return;
        }

        simulationManager.TryPlaceBuilding(selectedBuilding, gridPosition);
    }


    private void OnCancel(InputAction.CallbackContext context)
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState == GameState.Gameplay)
        {
            if (selectedBuilding != null)
            {
                SelectBuilding(null);
                GameEventSystem.PlayerMessage(
                    "Building mode cancelled. Select a built building to upgrade it."
                );
            }

            return;
        }

        if (gameManager.CurrentState == GameState.Paused)
            gameManager.ChangeState(GameState.MainMenu);
    }

    private void OnToggleTime(InputAction.CallbackContext context)
    {
        Debug.Log("Action: Time Paused/Resumed");

        if (gameManager == null)
        {
            Debug.LogError("GameManager is not assigned in ManagementCursor!");
            return;
        }

        if (gameManager.CurrentState == GameState.Gameplay)
        {
            gameManager.ChangeState(GameState.Paused);
        }
        else if (gameManager.CurrentState == GameState.Paused)
        {
            gameManager.ChangeState(GameState.Gameplay);
        }
    }

    private void SelectBuilding(BuildingData data)
    {
        selectedBuilding = data;
        GameEventSystem.ProductionBuildingSelected(null);

        if (selectedBuilding == null)
        {
            GameEventSystem.BuildingDataSelected(null);
            return;
        }

        GameEventSystem.BuildingSelected(selectedBuilding.Type);
        GameEventSystem.BuildingDataSelected(selectedBuilding);
    }
}