using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayback : MonoBehaviour
{
    [SerializeField] private bool stopOnGameOver;
    private AudioSource music;
    private void Awake() { music = GetComponent<AudioSource>(); music.loop = true; music.spatialBlend = 0f; }
    private void Update()
    {
        if (GameController.Paused) return; // AudioListener retoma a faixa da mesma posição.
        if (stopOnGameOver && GameController.gameOver)
        { if (music.isPlaying) music.Stop(); return; }
        // A primeira interação permite retomar áudio bloqueado pela política do navegador.
        bool interacted = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame));
        if (interacted && !music.isPlaying && music.clip != null) music.Play();
    }
}
