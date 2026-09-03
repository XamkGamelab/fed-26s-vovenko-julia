using UnityEngine;
using UnityEngine.InputSystem;

public class ManagementCursor : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private InputActionReference selectAction;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private InputActionReference rotateAction;
    [SerializeField] private InputActionReference toggleTimeAction;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;

    private void OnEnable()
    {
        moveAction.action.Enable();
        zoomAction.action.Enable();
        selectAction.action.Enable();
        cancelAction.action.Enable();
        rotateAction.action.Enable();
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
        zoomAction.action.Disable();
        selectAction.action.Disable();
        cancelAction.action.Disable();
        rotateAction.action.Disable();
        toggleTimeAction.action.Disable();
    }

    private void Update()
    {
        HandleMove();
        HandleZoom();
        HandleRotation();
        HandleRawInput();
    }

    private void HandleMove()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 movement = new Vector3(input.x, 0f, input.y);

        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        float input = zoomAction.action.ReadValue<float>();

        if (Mathf.Abs(input) < 0.01f)
            return;

        float newScale = transform.localScale.x + input * zoomSpeed * Time.deltaTime;

        newScale = Mathf.Clamp(newScale, minScale, maxScale);

        transform.localScale = Vector3.one * newScale;
    }

    private void HandleRotation()
    {
        float input = rotateAction.action.ReadValue<float>();

        transform.Rotate(
            Vector3.up,
            input * rotationSpeed * Time.deltaTime
        );
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
    }
}