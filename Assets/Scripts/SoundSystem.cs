using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu]
public class SoundSystem : ScriptableObject
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string VolumeVariableName;

    private float Volume { get; set; } = 1;

    public void SetVolume(float v)
    {
        Volume = v;
        var volumeLvl = v > float.Epsilon ? 20 * Mathf.Log10(Volume) : -144f;
        _audioMixer.SetFloat(VolumeVariableName, volumeLvl);
    }

    public float GetVolume()
    {
        return Volume;
    }
    
    private AudioSource _musicAudio;
    public void PlayMusicClip(AudioClip clip, float fadeTime = 0.5f)
    {
        if (_musicAudio == null)
        {
            _musicAudio = (new GameObject()).AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(_musicAudio);
            _musicAudio.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Music")[0];
        }

        _musicAudio.DOFade(0f, fadeTime)
            .OnComplete(() =>
            {
                _musicAudio.clip = clip;
                _musicAudio.loop = true;
                _musicAudio.Play();

                _musicAudio.DOFade(1f, fadeTime);
            });
    }

    private AudioSource _oneShotAudio;
    public void PlayOneShot(AudioClip clip, float volScale = 1f)
    {
        if (_oneShotAudio == null)
        {
            _oneShotAudio = (new GameObject()).AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(_oneShotAudio);
            _oneShotAudio.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Sfx")[0];
        }
        
        _oneShotAudio.PlayOneShot(clip, volScale);
    }
}
