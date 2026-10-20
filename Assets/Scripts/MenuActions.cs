using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public GameObject endGameMenu;
    [SerializeField] private AudioClip titleVoice;
    private bool changingScene, narrationStarted;
    private AudioSource narrationSource;
    private CanvasGroup menuGroup;
    private void Awake() => menuGroup = GetComponent<CanvasGroup>();

    // Chamado no começo do flash do botão, antes de sua confirmação terminar.
    public void PrepararNarracao()
    {
        if (changingScene || narrationStarted || ScreenTransition.IsBusy
            || SceneManager.GetActiveScene().name != "MainMenu") return;
        narrationStarted = true;
        narrationSource = GameAudio.Play(titleVoice, 0.85f);
    }

    public void IniciaJogo()
    {
        if (changingScene || ScreenTransition.IsBusy) return;
        MatchSetupMenu setup = GetComponent<MatchSetupMenu>();
        if (setup == null) { ComecarPartida(); return; }
        // Também funciona quando IniciaJogo é chamado sem o ButtonPulse.
        PrepararNarracao();
        changingScene = true;
        if (menuGroup != null) menuGroup.interactable = false;
        StartCoroutine(AbrirDificuldadeAposNarracao(setup));
    }

    private IEnumerator AbrirDificuldadeAposNarracao(MatchSetupMenu setup)
    {
        // Flash, efeito de seleção e narrador já estão sobrepostos. Só espera
        // o que ainda faltar do clipe, sem somar um atraso fixo de três segundos.
        while (narrationSource != null && narrationSource.isPlaying) yield return null;
        narrationSource = null;
        narrationStarted = false;
        changingScene = false;
        if (menuGroup != null) menuGroup.interactable = true;
        if (setup != null) setup.OpenDifficulty();
    }

    public void ComecarPartida()
    {
        if (changingScene || ScreenTransition.IsBusy) return;
        changingScene = true;
        GameController.SetPaused(false);
        Time.timeScale = 1f;
        if (menuGroup != null) menuGroup.interactable = false;
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
        if (this == null) return;
        changingScene = false;
        if (menuGroup != null) menuGroup.interactable = true;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (narrationSource != null) narrationSource.Stop();
        narrationSource = null;
        narrationStarted = changingScene = false;
        if (menuGroup != null) menuGroup.interactable = true;
    }
}
