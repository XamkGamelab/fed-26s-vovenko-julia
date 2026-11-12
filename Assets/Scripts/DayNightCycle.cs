using System;
using UnityEngine;

[Serializable]
public class DayNightSaveData
{
    public float elapsedGameTime;
}

[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour, IJsonSaveable
{
    public bool IsDay { get; private set; }

    [SerializeField, Min(10f)] private float fullDayDuration = 60f;
    [SerializeField, Min(0f)] private float dayIntensity = 2f;
    [SerializeField, Min(0f)] private float nightIntensity = 0.02f;

    [SerializeField] private Color dayColor = new Color(1f, 0.92f, 0.72f);
    [SerializeField] private Color nightColor = new Color(0.3f, 0.38f, 0.6f);

    private Light sun;
    private Vector3 startingRotation;
    private float elapsedGameTime;

    private void Awake()
    {
        sun = GetComponent<Light>();
        startingRotation = transform.eulerAngles;
        sun.useColorTemperature = false;
    }

    private void Update()
    {
        elapsedGameTime += Time.deltaTime;
        ApplyCycle();
    }

    private void ApplyCycle()
    {
        float phase =
            (elapsedGameTime / fullDayDuration) * Mathf.PI * 2f;
        IsDay = Mathf.Sin(phase) > 0f;

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

    public string CaptureState()
    {
        DayNightSaveData data = new DayNightSaveData
        {
            elapsedGameTime = elapsedGameTime
        };

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("Day/night save data is empty.");
            return;
        }

        try
        {
            DayNightSaveData data =
                JsonUtility.FromJson<DayNightSaveData>(json);

            if (data == null)
            {
                Debug.LogError("Day/night save data could not be read.");
                return;
            }

            elapsedGameTime = Mathf.Max(0f, data.elapsedGameTime);
            ApplyCycle();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not load day/night data: {exception.Message}");
        }
    }
}