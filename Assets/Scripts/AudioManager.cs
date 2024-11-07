using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using AYellowpaper.SerializedCollections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager m_instance { get; private set; }
    [SerializedDictionary("Name", "Audio Clip")]
    public SerializedDictionary<string, AudioClip> m_oneShotAudioClips;

    private AudioSource m_audioSource;
    

    private void Awake()
    {
        if (m_instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }

        m_instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        m_audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    public static void PlayOneShotAudioClip(string clipName)
    {
        AudioClip audioClip;
        if (m_instance.m_oneShotAudioClips.TryGetValue(clipName, out audioClip))
        {
            m_instance.m_audioSource.clip = audioClip;
            m_instance.m_audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No audio clip \"" + clipName + "\" found");
        }
    }
}
