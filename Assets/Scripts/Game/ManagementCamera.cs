using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class ManagementCamera : MonoBehaviour
{
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private InputActionReference rotateAction;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float rotationSpeed = 100f;

    private CinemachineFollow follow;

    private void Awake()
    {
        follow = GetComponent<CinemachineFollow>();
    }

    private void OnEnable()
    {
        zoomAction.action.Enable();
        rotateAction.action.Enable();
    }

    private void OnDisable()
    {
        zoomAction.action.Disable();
        rotateAction.action.Disable();
    }

    private void Update()
    {
        HandleZoom();
        HandleRotation();
    }

    private void HandleZoom()
    {
        float input = zoomAction.action.ReadValue<float>();

        if (Mathf.Abs(input) < 0.01f)
            return;

        Vector3 offset = follow.FollowOffset;

        float distance = offset.magnitude;
        distance -= input * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        follow.FollowOffset = offset.normalized * distance;
    }

    private void HandleRotation()
    {
        float input = rotateAction.action.ReadValue<float>();

        if (Mathf.Abs(input) < 0.01f)
            return;

        Vector3 offset = follow.FollowOffset;

        Quaternion rotation = Quaternion.AngleAxis(
            input * rotationSpeed * Time.deltaTime,
            Vector3.up
        );

        follow.FollowOffset = rotation * offset;
    }

}