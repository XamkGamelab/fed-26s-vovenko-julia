using TMPro;
using UnityEngine;

public class StoneDepositLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string labelText = "STONE DEPOSIT";

    private void Awake()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();

        if (label != null)
            label.text = labelText;
    }

    private void LateUpdate()
    {
        Camera activeCamera = Camera.main;

        if (activeCamera != null)
            transform.rotation = activeCamera.transform.rotation;
    }
}