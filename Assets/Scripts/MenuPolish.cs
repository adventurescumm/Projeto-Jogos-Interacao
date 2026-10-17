using UnityEngine;
using UnityEngine.UI;

public class MenuPolish : MonoBehaviour
{
    [SerializeField] private AudioClip confirmSound;
    private void Start()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            ButtonPulse pulse = button.GetComponent<ButtonPulse>() ?? button.gameObject.AddComponent<ButtonPulse>();
            pulse.Configure(confirmSound, null);
        }
    }
}
