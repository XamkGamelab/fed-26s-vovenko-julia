using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAmbientAudioProfile",
    menuName = "Audio/Ambient Audio Profile"
)]
public class AmbientAudioProfile : ScriptableObject
{
    [SerializeField] private AudioClip[] clips;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.2f;

    [Min(0.1f)]
    [SerializeField] private float minDistance = 2f;

    [Min(0.2f)]
    [SerializeField] private float maxDistance = 15f;

    public AudioClip[] Clips => clips;
    public float Volume => volume;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
}