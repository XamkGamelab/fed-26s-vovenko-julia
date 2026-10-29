using UnityEngine;

[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    [SerializeField, Min(10f)] private float fullDayDuration = 60f;
    [SerializeField, Min(0f)] private float dayIntensity = 2f;
    [SerializeField, Min(0f)] private float nightIntensity = 0.02f;

    [SerializeField] private Color dayColor = new Color(1f, 0.92f, 0.72f);
    [SerializeField] private Color nightColor = new Color(0.3f, 0.38f, 0.6f);

    private Light sun;
    private Vector3 startingRotation;

    private void Awake()
    {
        sun = GetComponent<Light>();
        startingRotation = transform.eulerAngles;
        sun.useColorTemperature = false;
    }

    private void Update()
    {
        float phase = (Time.time / fullDayDuration) * Mathf.PI * 2f;
        float daylight = Mathf.Clamp01(Mathf.Sin(phase));

        float sunAngle = phase * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(
            sunAngle,
            startingRotation.y,
            startingRotation.z
        );

        sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, daylight);
        sun.color = Color.Lerp(nightColor, dayColor, daylight);
    }
}