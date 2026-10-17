using UnityEngine;

// O som continua mesmo depois de o item ou personagem desaparecer.
public static class GameAudio
{
    public static AudioSource Play(AudioClip clip, float volume = 0.65f, bool ignorePause = false)
    {
        if (clip == null) return null;
        GameObject go = new GameObject("SFX_" + clip.name);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = false;
        source.ignoreListenerPause = ignorePause;
        source.volume = volume;
        source.clip = clip;
        source.Play();
        Object.Destroy(go, clip.length + 0.2f);
        return source;
    }
}
