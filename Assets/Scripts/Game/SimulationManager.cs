using UnityEngine;
using Unity.Cinemachine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private GameObject worldCursorPrefab;
    [SerializeField] private GameObject cameraPrefab;

    private GameObject worldCursorInstance;
    private GameObject cameraInstance;

    public void Initialize()
    {
        worldCursorInstance = Instantiate(
            worldCursorPrefab,
            Vector3.zero,
            Quaternion.identity,
            transform
        );

        cameraInstance = Instantiate(
            cameraPrefab,
            transform
        );

        CinemachineCamera cinemachineCamera =
            cameraInstance.GetComponent<CinemachineCamera>();

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget =
                worldCursorInstance.transform;
        }
    }
}