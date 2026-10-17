using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public GameObject endGameMenu;
    [SerializeField] private AudioClip titleVoice; // Opcional: arraste uma gravação existente aqui.
    [SerializeField, Min(0f)] private float titleDelay = 3f;
    private bool changingScene;
    private CanvasGroup menuGroup;
    private void Awake() => menuGroup = GetComponent<CanvasGroup>();
    public void IniciaJogo()
    {
        if (changingScene || ScreenTransition.IsBusy) return;
        changingScene = true;
        GameController.SetPaused(false);
        Time.timeScale = 1f;
        if (menuGroup != null) menuGroup.interactable = false;
        StartCoroutine(EntrarNoJogo());
    }
    private IEnumerator EntrarNoJogo()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            AudioSource voice = GameAudio.Play(titleVoice, 0.85f);
            float earliestExit = Time.unscaledTime + titleDelay;
            // Conserva o título visível e espera o som terminar antes do fade.
            while (Time.unscaledTime < earliestExit || (voice != null && voice.isPlaying))
                yield return null;
        }
        if (!ScreenTransition.Run(() => SceneManager.LoadScene("Arena01Estudio"), 0.35f, 0.1f, RestoreInput)) RestoreInput();
    }
    public void Menu()
    {
        if (changingScene || ScreenTransition.IsBusy) return;
        changingScene = true;
        GameController.SetPaused(false);
        Time.timeScale = 1f;
        if (!ScreenTransition.Run(() => SceneManager.LoadScene("MainMenu"), 0.2f, 0.05f, RestoreInput)) RestoreInput();
    }
    private void RestoreInput()
    {
        // Se a nova cena carregou, este objeto antigo já pode ter sido destruído.
        if (this == null) return;
        changingScene = false;
        if (menuGroup != null) menuGroup.interactable = true;
    }
}
