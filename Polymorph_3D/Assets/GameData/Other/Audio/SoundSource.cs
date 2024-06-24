using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource _audioSource;

    public bool Playing { get => _audioSource.isPlaying; }

    public void Play() => _audioSource.Play();

    public void Stop() => _audioSource.Stop();


}
