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

    [Header("Game")]
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
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
    }

    private void HandleMove()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 movement = new Vector3(input.x, 0f, input.y);

        transform.position += movement * moveSpeed * Time.deltaTime;
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

    private void OnSelect(InputAction.CallbackContext context)
    {
        Debug.Log("Action: Select called");
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("Action: Cancel / Back called");
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
}