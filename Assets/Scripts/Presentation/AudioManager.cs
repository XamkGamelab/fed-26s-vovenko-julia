using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public enum MusicType
    {
        Village
    }

    public enum SfxType
    {
        BuildingPlace,
        InvalidPlacement,
        BuildingComplete,
        WoodSawing,
        StoneMining
    }

    [Serializable]
    private class MusicClipEntry
    {
        public MusicType type;
        public AudioClip clip;
    }

    [Serializable]
    private class SoundClipEntry
    {
        public SfxType type;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [SerializeField] private float constructionTapInterval = 0.5f;

    private readonly HashSet<ProductionBuilding> buildingsUnderConstruction =
        new HashSet<ProductionBuilding>();

    private readonly Dictionary<SfxType, float> soundVolumes =
    new Dictionary<SfxType, float>();

    private readonly List<ProductionBuilding> buildingsToClean =
    new List<ProductionBuilding>();

    private Coroutine constructionSoundRoutine;

    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Spatial Ambience")]
    [SerializeField] private AudioMixerGroup ambienceMixerGroup;
    [SerializeField, Min(1)] private int initialAmbientSources = 2;

    private readonly Stack<AudioSource> pooledAmbientSources =
        new Stack<AudioSource>();

    private readonly Dictionary<ProductionBuilding, AudioSource> activeAmbientSources =
        new Dictionary<ProductionBuilding, AudioSource>();

    [Header("Clips")]
    [SerializeField] private List<MusicClipEntry> musicClips = new List<MusicClipEntry>();
    [SerializeField] private List<SoundClipEntry> soundClips = new List<SoundClipEntry>();

    private readonly Dictionary<MusicType, AudioClip> musicByType =
        new Dictionary<MusicType, AudioClip>();

    private readonly Dictionary<SfxType, AudioClip> soundByType =
        new Dictionary<SfxType, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (MusicClipEntry entry in musicClips)
        {
            if (entry.clip != null)
                musicByType[entry.type] = entry.clip;
        }

        foreach (SoundClipEntry entry in soundClips)
        {
            if (entry.clip != null)
            { 
                soundByType[entry.type] = entry.clip;
                soundVolumes[entry.type] = entry.volume;
            }
        }

        InitializeAmbientPool();
    }

    private void Start()
    {
        PlayMusic(MusicType.Village);
    }

    public void PlayMusic(MusicType type)
    {
        if (!musicByType.TryGetValue(type, out AudioClip clip))
        {
            Debug.LogWarning($"Music clip not found: {type}");
            return;
        }

        if (musicSource == null)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySfx(SfxType type)
    {
        if (!soundByType.TryGetValue(type, out AudioClip clip))
        {
            Debug.LogWarning($"Sound clip not found: {type}");
            return;
        }

        if (soundSource == null)
            return;

        float volume = soundVolumes.TryGetValue(type, out float configuredVolume)
            ? configuredVolume
            : 1f;

        soundSource.PlayOneShot(clip, volume);
    }

    public void SetMasterVolume(float volume)
    {
        SetMixerVolume("MasterVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        SetMixerVolume("SFXVolume", volume);
    }

    public void SetAmbienceVolume(float volume)
    {
        SetMixerVolume("AmbienceVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetMixerVolume("MusicVolume", volume);
    }

    private void SetMixerVolume(string parameterName, float volume)
    {
        if (audioMixer == null)
            return;

        volume = Mathf.Clamp01(volume);
        float decibels = volume <= 0.0001f
            ? -80f
            : 20f * Mathf.Log10(volume);

        audioMixer.SetFloat(parameterName, decibels);
    }

    private void OnEnable()
    {
        GameEventSystem.OnBuildingPlaced += HandleBuildingPlaced;
        GameEventSystem.OnBuildingConstructionCompleted += HandleConstructionCompleted;
        GameEventSystem.OnInvalidBuildingPlacement += HandleInvalidBuildingPlacement;
        GameEventSystem.OnProductionCompleted += HandleProductionCompleted;
    }

    private void OnDisable()
    {
        GameEventSystem.OnBuildingPlaced -= HandleBuildingPlaced;
        GameEventSystem.OnBuildingConstructionCompleted -= HandleConstructionCompleted;
        GameEventSystem.OnInvalidBuildingPlacement -= HandleInvalidBuildingPlacement;
        GameEventSystem.OnProductionCompleted -= HandleProductionCompleted;

        if (constructionSoundRoutine != null)
        {
            StopCoroutine(constructionSoundRoutine);
            constructionSoundRoutine = null;
        }
        foreach (AudioSource source in activeAmbientSources.Values)
            ReturnAmbientSource(source);

        activeAmbientSources.Clear();

        buildingsUnderConstruction.Clear();
    }

    private void HandleBuildingPlaced(ProductionBuilding building)
    {
        if (building == null)
            return;

        buildingsUnderConstruction.Add(building);
        PlaySfx(SfxType.BuildingPlace);

        if (constructionSoundRoutine == null)
            constructionSoundRoutine = StartCoroutine(PlayConstructionTaps());
    }

    private void HandleConstructionCompleted(ProductionBuilding building)
    {
        buildingsUnderConstruction.Remove(building);
        PlaySfx(SfxType.BuildingComplete);
        PlayBuildingAmbience(building);
    }

    private IEnumerator PlayConstructionTaps()
    {
        while (buildingsUnderConstruction.Count > 0)
        {
            yield return new WaitForSeconds(constructionTapInterval);

            buildingsUnderConstruction.RemoveWhere(building => building == null);

            if (buildingsUnderConstruction.Count > 0)
                PlaySfx(SfxType.BuildingPlace);
        }

        constructionSoundRoutine = null;
    }

    private void HandleInvalidBuildingPlacement()
    {
        PlaySfx(SfxType.InvalidPlacement);
    }

    private void HandleProductionCompleted(ProductionBuilding building)
    {
        if (building == null || building.Data == null)
            return;

        if (building.Data.ProducedResource == ResourceType.Wood)
            PlaySfx(SfxType.WoodSawing);
        else
            PlaySfx(SfxType.StoneMining);
    }

    private void InitializeAmbientPool()
    {
        for (int i = 0; i < initialAmbientSources; i++)
            pooledAmbientSources.Push(CreateAmbientSource());
    }

    private AudioSource CreateAmbientSource()
    {
        GameObject sourceObject = new GameObject("Pooled Ambient Source");
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.outputAudioMixerGroup = ambienceMixerGroup;

        sourceObject.SetActive(false);
        return source;
    }

    private void PlayBuildingAmbience(ProductionBuilding building)
    {
        if (building == null || building.Data == null)
            return;

        if (!building.Data.UseAmbientAudio ||
            building.Data.AmbientProfile == null ||
            activeAmbientSources.ContainsKey(building))
            return;

        AudioClip[] clips = building.Data.AmbientProfile.Clips;
        if (clips == null || clips.Length == 0)
            return;

        AudioSource source = pooledAmbientSources.Count > 0
            ? pooledAmbientSources.Pop()
            : CreateAmbientSource();

        AmbientAudioProfile profile = building.Data.AmbientProfile;
        source.transform.position = building.transform.position;
        source.outputAudioMixerGroup = ambienceMixerGroup;
        source.clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        source.volume = profile.Volume;
        source.minDistance = profile.MinDistance;
        source.maxDistance = profile.MaxDistance;
        source.loop = true;
        source.spatialBlend = 1f;

        source.gameObject.SetActive(true);
        source.Play();

        activeAmbientSources.Add(building, source);
    }

    private void Update()
    {
        buildingsToClean.Clear();

        foreach (KeyValuePair<ProductionBuilding, AudioSource> entry
                 in activeAmbientSources)
        {
            if (entry.Key == null)
            {
                ReturnAmbientSource(entry.Value);
                buildingsToClean.Add(entry.Key);
            }
        }

        foreach (ProductionBuilding building in buildingsToClean)
            activeAmbientSources.Remove(building);
    }

    private void ReturnAmbientSource(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        pooledAmbientSources.Push(source);
    }
}