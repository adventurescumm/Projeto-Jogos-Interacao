using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(AudioSource))]
public class DeathEffect : MonoBehaviour
{
    [SerializeField] private Sprite[] frames = new Sprite[0];
    [SerializeField, Min(1f)] private float framesPerSecond = 32f;
    private SpriteRenderer image;
    private AudioSource sound;
    public bool Completed { get; private set; }
    private void Awake()
    {
        image = GetComponent<SpriteRenderer>(); sound = GetComponent<AudioSource>();
        sound.playOnAwake = false; sound.loop = false; sound.spatialBlend = 0f; sound.pitch = 1f;
    }
    public void Play(AudioClip clip, float durationMultiplier = 1f)
    {
        StopAllCoroutines();
        Completed = false;
        sound.clip = clip;
        if (clip != null) sound.Play();
        StartCoroutine(Animate(Mathf.Max(0.1f, durationMultiplier)));
    }
    private IEnumerator Animate(float durationMultiplier)
    {
        float elapsed = 0f;
        float fps = Mathf.Max(1f, framesPerSecond) / durationMultiplier;
        // Espera o AudioSource de fato terminar; não depende de um atraso fixo no HUD.
        do
        {
            int index = Mathf.FloorToInt(elapsed * fps);
            image.enabled = index < frames.Length;
            if (image.enabled) image.sprite = frames[index];
            yield return null;
            if (!GameController.Paused) elapsed += Time.unscaledDeltaTime;
        } while (GameController.Paused || elapsed < frames.Length / fps || sound.isPlaying);
        Completed = true;
        Destroy(gameObject);
    }
}
