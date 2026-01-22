using UnityEngine;

public static class AudioManager
{
    static AudioSource _source;

    public static void Init(AudioSource source)
    {
        _source = source;
    }

    public static void PlayClue(string clueFileName)
    {
        if (_source.isPlaying)
            _source.Stop();
        
        _source.PlayOneShot(Resources.Load<AudioClip>("Audio/" + clueFileName));
    }
    
    public static void StopPlayingClue()
    {
        if (_source.isPlaying)
            _source.Stop();
    }

}
