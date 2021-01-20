﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance = null;
    private static float vol = 1f;
    private static float musicVol = 1f;
    private static float soundsVol = 1f;
    private static float UISoundsVol = 1f;

    private static Dictionary<int, Audio> musicAudio;
    private static Dictionary<int, Audio> soundsAudio;
    private static Dictionary<int, Audio> UISoundsAudio;

    private static bool initialized = false;

    private static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (SoundManager)FindObjectOfType(typeof(SoundManager));
                if (_instance == null)
                {
                    // Create gameObject and add component
                    _instance = (new GameObject("SoundManager")).AddComponent<SoundManager>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// The gameobject that the sound manager is attached to
    /// </summary>
    public static GameObject SoundObject { get { return Instance.gameObject; } }

    /// <summary>
    /// When set to true, new Audios that have the same audio clip as any other Audio, will be ignored
    /// </summary>
    public static bool IgnoreDuplicateMusic { get; set; }

    /// <summary>
    /// When set to true, new Audios that have the same audio clip as any other Audio, will be ignored
    /// </summary>
    public static bool IgnoreDuplicateSounds { get; set; }

    /// <summary>
    /// When set to true, new Audios that have the same audio clip as any other Audio, will be ignored
    /// </summary>
    public static bool IgnoreDuplicateUISounds { get; set; }

    /// <summary>
    /// Global volume
    /// </summary>
    public static float GlobalVolume
    {
        get
        {
            return vol;
        }
        set
        {
            vol = value;
        }
    }

    /// <summary>
    /// Global music volume
    /// </summary>
    public static float GlobalMusicVolume
    {
        get
        {
            return musicVol;
        }
        set
        {
            musicVol = value;
        }
    }

    /// <summary>
    /// Global sounds volume
    /// </summary>
    public static float GlobalSoundsVolume
    {
        get
        {
            return soundsVol;
        }
        set
        {
            soundsVol = value;
        }
    }

    /// <summary>
    /// Global UI sounds volume
    /// </summary>
    public static float GlobalUISoundsVolume
    {
        get
        {
            return UISoundsVol;
        }
        set
        {
            UISoundsVol = value;
        }
    }

    void Awake()
    {
        Instance.Init();

        // Add the delegate to be called when the scene is loaded, between Awake and Start.
        SceneManager.sceneLoaded += SceneLoaded;
    }

    void OnDestroy()
    {
        // Remove the delegate when the object is destroyed
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        List<int> keys;

        // Stop and remove all non-persistent music audio
        keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = musicAudio[key];
            if (!audio.Persist && audio.Activated)
            {
                Destroy(audio.AudioSource);
                musicAudio.Remove(key);
            }
        }

        // Stop and remove all sound fx
        keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = soundsAudio[key];
            Destroy(audio.AudioSource);
            soundsAudio.Remove(key);
        }

        // Stop and remove all UI sound fx
        keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = UISoundsAudio[key];
            Destroy(audio.AudioSource);
            UISoundsAudio.Remove(key);
        }
    }

    //void OnLevelWasLoaded(int level)
    //{
    //    List<int> keys;
    //
    //    // Stop and remove all non-persistent music audio
    //    keys = new List<int>(musicAudio.Keys);
    //    foreach (int key in keys)
    //    {
    //        Audio audio = musicAudio[key];
    //        if (!audio.persist && audio.activated)
    //        {
    //            Destroy(audio.audioSource);
    //            musicAudio.Remove(key);
    //        }
    //    }
    //
    //    // Stop and remove all sound fx
    //    keys = new List<int>(soundsAudio.Keys);
    //    foreach (int key in keys)
    //    {
    //        Audio audio = soundsAudio[key];
    //        Destroy(audio.audioSource);
    //        soundsAudio.Remove(key);
    //    }
    //
    //    // Stop and remove all UI sound fx
    //    keys = new List<int>(UISoundsAudio.Keys);
    //    foreach (int key in keys)
    //    {
    //        Audio audio = UISoundsAudio[key];
    //        Destroy(audio.audioSource);
    //        UISoundsAudio.Remove(key);
    //    }
    //}

    void Update()
    {
        List<int> keys;

        // Update music
        keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = musicAudio[key];
            audio.Update();

            // Remove all music clips that are not playing
            if (!audio.Playing && !audio.Paused)
            {
                Destroy(audio.AudioSource);
                //musicAudio.Remove(key);
            }
        }

        // Update sound fx
        keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = soundsAudio[key];
            audio.Update();

            // Remove all sound fx clips that are not playing
            if (!audio.Playing && !audio.Paused)
            {
                Destroy(audio.AudioSource);
                //soundsAudio.Remove(key);
            }
        }

        // Update UI sound fx
        keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = UISoundsAudio[key];
            audio.Update();

            // Remove all UI sound fx clips that are not playing
            if (!audio.Playing && !audio.Paused)
            {
                Destroy(audio.AudioSource);
                //UISoundsAudio.Remove(key);
            }
        }
    }

    void Init()
    {
        if (!initialized)
        {
            musicAudio = new Dictionary<int, Audio>();
            soundsAudio = new Dictionary<int, Audio>();
            UISoundsAudio = new Dictionary<int, Audio>();

            IgnoreDuplicateMusic = false;
            IgnoreDuplicateSounds = false;
            IgnoreDuplicateUISounds = false;

            initialized = true;
            DontDestroyOnLoad(this);
        }
    }

    #region GetAudio Functions

    /// <summary>
    /// Returns the Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the Audio to be retrieved</param>
    /// <returns>Audio that has as its id the audioID, null if no such Audio is found</returns>
    public static Audio GetAudio(int audioID)
    {
        Audio audio;

        audio = GetMusicAudio(audioID);
        if (audio != null)
        {
            return audio;
        }

        audio = GetSoundAudio(audioID);
        if (audio != null)
        {
            return audio;
        }

        audio = GetUISoundAudio(audioID);
        if (audio != null)
        {
            return audio;
        }

        return null;
    }

    /// <summary>
    /// Returns the first occurrence of Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the Audio to be retrieved</param>
    /// <returns>First occurrence of Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static Audio GetAudio(AudioClip audioClip)
    {
        Audio audio = GetMusicAudio(audioClip);
        if (audio != null)
        {
            return audio;
        }

        audio = GetSoundAudio(audioClip);
        if (audio != null)
        {
            return audio;
        }

        audio = GetUISoundAudio(audioClip);
        if (audio != null)
        {
            return audio;
        }

        return null;
    }

    /// <summary>
    /// Returns the music Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the music Audio to be returned</param>
    /// <returns>Music Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    public static Audio GetMusicAudio(int audioID)
    {
        List<int> keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            if (audioID == key)
            {
                return musicAudio[key];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first occurrence of music Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the music Audio to be retrieved</param>
    /// <returns>First occurrence of music Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static Audio GetMusicAudio(AudioClip audioClip)
    {
        List<int> keys;
        keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = musicAudio[key];
            if (audio.Clip == audioClip)
            {
                return audio;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the sound fx Audio to be returned</param>
    /// <returns>Sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    public static Audio GetSoundAudio(int audioID)
    {
        List<int> keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            if (audioID == key)
            {
                return soundsAudio[key];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first occurrence of sound Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the sound Audio to be retrieved</param>
    /// <returns>First occurrence of sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static Audio GetSoundAudio(AudioClip audioClip)
    {
        List<int> keys;
        keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = soundsAudio[key];
            if (audio.Clip == audioClip)
            {
                return audio;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the UI sound fx Audio that has as its id the audioID if one is found, returns null if no such Audio is found
    /// </summary>
    /// <param name="audioID">The id of the UI sound fx Audio to be returned</param>
    /// <returns>UI sound fx Audio that has as its id the audioID if one is found, null if no such Audio is found</returns>
    public static Audio GetUISoundAudio(int audioID)
    {
        List<int> keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            if (audioID == key)
            {
                return UISoundsAudio[key];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first occurrence of UI sound Audio that plays the given audioClip. Returns null if no such Audio is found
    /// </summary>
    /// <param name="audioClip">The audio clip of the UI sound Audio to be retrieved</param>
    /// <returns>First occurrence of UI sound Audio that has as plays the audioClip, null if no such Audio is found</returns>
    public static Audio GetUISoundAudio(AudioClip audioClip)
    {
        List<int> keys;
        keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = UISoundsAudio[key];
            if (audio.Clip == audioClip)
            {
                return audio;
            }
        }

        return null;
    }

    #endregion

    #region Play Functions

    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayMusic(AudioClip clip)
    {
        return PlayMusic(clip, 1f, false, false, 1f, 1f, -1f, null);
    }

    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayMusic(AudioClip clip, float volume)
    {
        return PlayMusic(clip, volume, false, false, 1f, 1f, -1f, null);
    }

    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <param name="loop">Wether the music is looped</param>
    /// <param name = "persist" > Whether the audio persists in between scene changes</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayMusic(AudioClip clip, float volume, bool loop, bool persist)
    {
        return PlayMusic(clip, volume, loop, persist, 1f, 1f, -1f, null);
    }

    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <param name="loop">Wether the music is looped</param>
    /// <param name="persist"> Whether the audio persists in between scene changes</param>
    /// <param name="fadeInValue">How many seconds it needs for the audio to fade in/ reach target volume (if higher than current)</param>
    /// <param name="fadeOutValue"> How many seconds it needs for the audio to fade out/ reach target volume (if lower than current)</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayMusic(AudioClip clip, float volume, bool loop, bool persist, float fadeInSeconds, float fadeOutSeconds)
    {
        return PlayMusic(clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds, -1f, null);
    }

    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <param name="loop">Wether the music is looped</param>
    /// <param name="persist"> Whether the audio persists in between scene changes</param>
    /// <param name="fadeInValue">How many seconds it needs for the audio to fade in/ reach target volume (if higher than current)</param>
    /// <param name="fadeOutValue"> How many seconds it needs for the audio to fade out/ reach target volume (if lower than current)</param>
    /// <param name="currentMusicfadeOutSeconds"> How many seconds it needs for current music audio to fade out. It will override its own fade out seconds. If -1 is passed, current music will keep its own fade out seconds</param>
    /// <param name="sourceTransform">The transform that is the source of the music (will become 3D audio). If 3D audio is not wanted, use null</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayMusic(AudioClip clip, float volume, bool loop, bool persist, float fadeInSeconds, float fadeOutSeconds, float currentMusicfadeOutSeconds, Transform sourceTransform)
    {
        if (clip == null)
        {
            Debug.LogError("Sound Manager: Audio clip is null, cannot play music", clip);
        }

        if (IgnoreDuplicateMusic)
        {
            List<int> keys = new List<int>(musicAudio.Keys);
            foreach (int key in keys)
            {
                if (musicAudio[key].AudioSource.clip == clip)
                {
                    return musicAudio[key].AudioID;
                }
            }
        }

        Instance.Init();

        // Stop all current music playing
        StopAllMusic(currentMusicfadeOutSeconds);

        // Create the audioSource
        //AudioSource audioSource = instance.gameObject.AddComponent<AudioSource>() as AudioSource;
        Audio audio = new Audio(Audio.AudioType.Music, clip, loop, persist, volume, fadeInSeconds, fadeOutSeconds, sourceTransform);

        // Add it to music list
        musicAudio.Add(audio.AudioID, audio);

        return audio.AudioID;
    }

    /// <summary>
    /// Play a sound fx
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlaySound(AudioClip clip)
    {
        return PlaySound(clip, 1f, false, null);
    }

    /// <summary>
    /// Play a sound fx
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlaySound(AudioClip clip, float volume)
    {
        return PlaySound(clip, volume, false, null);
    }

    /// <summary>
    /// Play a sound fx
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="loop">Wether the sound is looped</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlaySound(AudioClip clip, bool loop)
    {
        return PlaySound(clip, 1f, loop, null);
    }

    /// <summary>
    /// Play a sound fx
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <param name="loop">Wether the sound is looped</param>
    /// <param name="sourceTransform">The transform that is the source of the sound (will become 3D audio). If 3D audio is not wanted, use null</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlaySound(AudioClip clip, float volume, bool loop, Transform sourceTransform)
    {
        if (clip == null)
        {
            Debug.LogError("Sound Manager: Audio clip is null, cannot play music", clip);
        }

        if (IgnoreDuplicateSounds)
        {
            List<int> keys = new List<int>(soundsAudio.Keys);
            foreach (int key in keys)
            {
                if (soundsAudio[key].AudioSource.clip == clip)
                {
                    return soundsAudio[key].AudioID;
                }
            }
        }

        Instance.Init();

        // Create the audioSource
        //AudioSource audioSource = Instance.gameObject.AddComponent<AudioSource>() as AudioSource;
        Audio audio = new Audio(Audio.AudioType.Sound, clip, loop, false, volume, 0f, 0f, sourceTransform);

        // Add it to music list
        soundsAudio.Add(audio.AudioID, audio);

        return audio.AudioID;
    }

    /// <summary>
    /// Play a UI sound fx
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayUISound(AudioClip clip)
    {
        return PlayUISound(clip, 1f);
    }

    /// <summary>
    /// Play a UI sound fx
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume"> The volume the music will have</param>
    /// <returns>The ID of the created Audio object</returns>
    public static int PlayUISound(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            Debug.LogError("Sound Manager: Audio clip is null, cannot play music", clip);
        }

        if (IgnoreDuplicateUISounds)
        {
            List<int> keys = new List<int>(UISoundsAudio.Keys);
            foreach (int key in keys)
            {
                if (UISoundsAudio[key].AudioSource.clip == clip)
                {
                    return UISoundsAudio[key].AudioID;
                }
            }
        }

        Instance.Init();

        // Create the audioSource
        //AudioSource audioSource = instance.gameObject.AddComponent<AudioSource>() as AudioSource;
        Audio audio = new Audio(Audio.AudioType.UISound, clip, false, false, volume, 0f, 0f, null);

        // Add it to music list
        UISoundsAudio.Add(audio.AudioID, audio);

        return audio.AudioID;
    }

    #endregion

    #region Stop Functions

    /// <summary>
    /// Stop all audio playing
    /// </summary>
    public static void StopAll()
    {
        StopAll(-1f);
    }

    /// <summary>
    /// Stop all audio playing
    /// </summary>
    /// <param name="fadeOutSeconds"> How many seconds it needs for all music audio to fade out. It will override  their own fade out seconds. If -1 is passed, all music will keep their own fade out seconds</param>
    public static void StopAll(float fadeOutSeconds)
    {
        StopAllMusic(fadeOutSeconds);
        StopAllSounds();
        StopAllUISounds();
    }

    /// <summary>
    /// Stop all music playing
    /// </summary>
    public static void StopAllMusic()
    {
        StopAllMusic(-1f);
    }

    /// <summary>
    /// Stop all music playing
    /// </summary>
    /// <param name="fadeOutSeconds"> How many seconds it needs for all music audio to fade out. It will override  their own fade out seconds. If -1 is passed, all music will keep their own fade out seconds</param>
    public static void StopAllMusic(float fadeOutSeconds)
    {
        List<int> keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = musicAudio[key];
            if (fadeOutSeconds > 0)
            {
                audio.FadeOutSeconds = fadeOutSeconds;
            }
            audio.Stop();
        }
    }

    /// <summary>
    /// Stop all sound fx playing
    /// </summary>
    public static void StopAllSounds()
    {
        List<int> keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = soundsAudio[key];
            audio.Stop();
        }
    }

    /// <summary>
    /// Stop all UI sound fx playing
    /// </summary>
    public static void StopAllUISounds()
    {
        List<int> keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = UISoundsAudio[key];
            audio.Stop();
        }
    }

    #endregion

    #region Pause Functions

    /// <summary>
    /// Pause all audio playing
    /// </summary>
    public static void PauseAll()
    {
        PauseAllMusic();
        PauseAllSounds();
        PauseAllUISounds();
    }

    /// <summary>
    /// Pause all music playing
    /// </summary>
    public static void PauseAllMusic()
    {
        List<int> keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = musicAudio[key];
            audio.Pause();
        }
    }

    /// <summary>
    /// Pause all sound fx playing
    /// </summary>
    public static void PauseAllSounds()
    {
        List<int> keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = soundsAudio[key];
            audio.Pause();
        }
    }

    /// <summary>
    /// Pause all UI sound fx playing
    /// </summary>
    public static void PauseAllUISounds()
    {
        List<int> keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = UISoundsAudio[key];
            audio.Pause();
        }
    }

    #endregion

    #region Resume Functions

    /// <summary>
    /// Resume all audio playing
    /// </summary>
    public static void ResumeAll()
    {
        ResumeAllMusic();
        ResumeAllSounds();
        ResumeAllUISounds();
    }

    /// <summary>
    /// Resume all music playing
    /// </summary>
    public static void ResumeAllMusic()
    {
        List<int> keys = new List<int>(musicAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = musicAudio[key];
            audio.Resume();
        }
    }

    /// <summary>
    /// Resume all sound fx playing
    /// </summary>
    public static void ResumeAllSounds()
    {
        List<int> keys = new List<int>(soundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = soundsAudio[key];
            audio.Resume();
        }
    }

    /// <summary>
    /// Resume all UI sound fx playing
    /// </summary>
    public static void ResumeAllUISounds()
    {
        List<int> keys = new List<int>(UISoundsAudio.Keys);
        foreach (int key in keys)
        {
            Audio audio = UISoundsAudio[key];
            audio.Resume();
        }
    }

    #endregion
}

public class Audio
{
    private static int audioCounter = 0;
    private float volume;
    private float targetVolume;
    private float initTargetVolume;
    private float tempFadeSeconds;
    private float fadeInterpolater;
    private float onFadeStartVolume;
    private AudioType audioType;
    private AudioClip initClip;
    private Transform sourceTransform;

    /// <summary>
    /// The ID of the Audio
    /// </summary>
    public int AudioID { get; private set; }

    /// <summary>
    /// The audio source that is responsible for this audio
    /// </summary>
    public AudioSource AudioSource { get; private set; }

    /// <summary>
    /// Audio clip to play/is playing
    /// </summary>
    public AudioClip Clip
    {
        get
        {
            return AudioSource == null ? initClip : AudioSource.clip;
        }
    }

    /// <summary>
    /// Whether the audio will be lopped
    /// </summary>
    public bool Loop { get; set; }

    /// <summary>
    /// Whether the audio persists in between scene changes
    /// </summary>
    public bool Persist { get; set; }

    /// <summary>
    /// How many seconds it needs for the audio to fade in/ reach target volume (if higher than current)
    /// </summary>
    public float FadeInSeconds { get; set; }

    /// <summary>
    /// How many seconds it needs for the audio to fade out/ reach target volume (if lower than current)
    /// </summary>
    public float FadeOutSeconds { get; set; }

    /// <summary>
    /// Whether the audio is currently playing
    /// </summary>
    public bool Playing { get; set; }

    /// <summary>
    /// Whether the audio is paused
    /// </summary>
    public bool Paused { get; private set; }

    /// <summary>
    /// Whether the audio is stopping
    /// </summary>
    public bool Stopping { get; private set; }

    /// <summary>
    /// Whether the audio is created and updated at least once. 
    /// </summary>
    public bool Activated { get; private set; }

    public enum AudioType
    {
        Music,
        Sound,
        UISound
    }

    public Audio(AudioType audioType, AudioClip clip, bool loop, bool persist, float volume, float fadeInValue, float fadeOutValue, Transform sourceTransform)
    {
        if (sourceTransform == null)
        {
            this.sourceTransform = SoundManager.SoundObject.transform;
        }
        else
        {
            this.sourceTransform = sourceTransform;
        }

        this.AudioID = audioCounter;
        audioCounter++;

        this.audioType = audioType;
        this.initClip = clip;
        this.Loop = loop;
        this.Persist = persist;
        this.targetVolume = volume;
        this.initTargetVolume = volume;
        this.tempFadeSeconds = -1;
        this.volume = 0f;
        this.FadeInSeconds = fadeInValue;
        this.FadeOutSeconds = fadeOutValue;

        this.Playing = false;
        this.Paused = false;
        this.Activated = false;

        CreateAudiosource(clip, loop);
        Play();
    }

    void CreateAudiosource(AudioClip clip, bool loop)
    {
        AudioSource = sourceTransform.gameObject.AddComponent<AudioSource>() as AudioSource;

        AudioSource.clip = clip;
        AudioSource.loop = loop;
        AudioSource.volume = 0f;
        if (sourceTransform != SoundManager.SoundObject.transform)
        {
            AudioSource.spatialBlend = 1;
        }
    }

    /// <summary>
    /// Start playing audio clip from the beggining
    /// </summary>
    public void Play()
    {
        Play(initTargetVolume);
    }

    /// <summary>
    /// Start playing audio clip from the beggining
    /// </summary>
    /// <param name="volume">The target volume</param>
    public void Play(float volume)
    {
        if (AudioSource == null)
        {
            CreateAudiosource(initClip, Loop);
        }

        AudioSource.Play();
        Playing = true;

        fadeInterpolater = 0f;
        onFadeStartVolume = this.volume;
        targetVolume = volume;
    }

    /// <summary>
    /// Stop playing audio clip
    /// </summary>
    public void Stop()
    {
        fadeInterpolater = 0f;
        onFadeStartVolume = volume;
        targetVolume = 0f;

        Stopping = true;
    }

    /// <summary>
    /// Pause playing audio clip
    /// </summary>
    public void Pause()
    {
        AudioSource.Pause();
        Paused = true;
    }

    /// <summary>
    /// Resume playing audio clip
    /// </summary>
    public void UnPause()
    {
        AudioSource.UnPause();
        Paused = false;
    }

    /// <summary>
    /// Resume playing audio clip
    /// </summary>
    public void Resume()
    {
        AudioSource.UnPause();
        Paused = false;
    }

    /// <summary>
    /// Sets the audio volume
    /// </summary>
    /// <param name="volume">The target volume</param>
    public void SetVolume(float volume)
    {
        if (volume > targetVolume)
        {
            SetVolume(volume, FadeOutSeconds);
        }
        else
        {
            SetVolume(volume, FadeInSeconds);
        }
    }

    /// <summary>
    /// Sets the audio volume
    /// </summary>
    /// <param name="volume">The target volume</param>
    /// <param name="fadeSeconds">How many seconds it needs for the audio to fade in/out to reach target volume. If passed, it will override the Audio's fade in/out seconds, but only for this transition</param>
    public void SetVolume(float volume, float fadeSeconds)
    {
        SetVolume(volume, fadeSeconds, this.volume);
    }

    /// <summary>
    /// Sets the audio volume
    /// </summary>
    /// <param name="volume">The target volume</param>
    /// <param name="fadeSeconds">How many seconds it needs for the audio to fade in/out to reach target volume. If passed, it will override the Audio's fade in/out seconds, but only for this transition</param>
    /// <param name="startVolume">Immediately set the volume to this value before beginning the fade. If not passed, the Audio will start fading from the current volume towards the target volume</param>
    public void SetVolume(float volume, float fadeSeconds, float startVolume)
    {
        targetVolume = Mathf.Clamp01(volume);
        fadeInterpolater = 0;
        onFadeStartVolume = startVolume;
        tempFadeSeconds = fadeSeconds;
    }

    /// <summary>
    /// Sets the Audio 3D max distance
    /// </summary>
    /// <param name="max">the max distance</param>
    public void Set3DMaxDistance(float max)
    {
        AudioSource.maxDistance = max;
    }

    /// <summary>
    /// Sets the Audio 3D min distance
    /// </summary>
    /// <param name="max">the min distance</param>
    public void Set3DMinDistance(float min)
    {
        AudioSource.minDistance = min;
    }

    /// <summary>
    /// Sets the Audio 3D distances
    /// </summary>
    /// <param name="min">the min distance</param>
    /// <param name="max">the max distance</param>
    public void Set3DDistances(float min, float max)
    {
        Set3DMinDistance(min);
        Set3DMaxDistance(max);
    }

    public void Update()
    {
        if (AudioSource == null)
        {
            return;
        }

        Activated = true;

        if (volume != targetVolume)
        {
            float fadeValue;
            fadeInterpolater += Time.unscaledDeltaTime;
            if (volume > targetVolume)
            {
                fadeValue = tempFadeSeconds != -1 ? tempFadeSeconds : FadeOutSeconds;
            }
            else
            {
                fadeValue = tempFadeSeconds != -1 ? tempFadeSeconds : FadeInSeconds;
            }

            volume = Mathf.Lerp(onFadeStartVolume, targetVolume, fadeInterpolater / fadeValue);
        }
        else if (tempFadeSeconds != -1)
        {
            tempFadeSeconds = -1;
        }

        switch (audioType)
        {
            case AudioType.Music:
                {
                    AudioSource.volume = volume * SoundManager.GlobalMusicVolume * SoundManager.GlobalVolume;
                    break;
                }
            case AudioType.Sound:
                {
                    AudioSource.volume = volume * SoundManager.GlobalSoundsVolume * SoundManager.GlobalVolume;
                    break;
                }
            case AudioType.UISound:
                {
                    AudioSource.volume = volume * SoundManager.GlobalUISoundsVolume * SoundManager.GlobalVolume;
                    break;
                }
        }

        if (volume == 0f && Stopping)
        {
            AudioSource.Stop();
            Stopping = false;
            Playing = false;
            Paused = false;
        }

        // Update playing status
        if (AudioSource.isPlaying != Playing)
        {
            Playing = AudioSource.isPlaying;
        }
    }
}