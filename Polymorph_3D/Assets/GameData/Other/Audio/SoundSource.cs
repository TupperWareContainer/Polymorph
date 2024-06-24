using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour
{
 
    [SerializeField] private AudioSource _audioSource;
    [Header("Preferences")]
    [SerializeField] private bool _destroyOnFinish;
    public bool Playing { get => _audioSource.isPlaying; }


    private void Awake()
    {
        if (_destroyOnFinish)
        {
            StartCoroutine(DestroyOnFinish());
        }
    }

    IEnumerator DestroyOnFinish()
    {
        yield return new WaitUntil(() => _audioSource.isPlaying);
        yield return new WaitUntil(() => !_audioSource.isPlaying);
        Destroy(gameObject);
    }
    public void Play() => _audioSource.Play();

    public void Stop() => _audioSource.Stop();


}
