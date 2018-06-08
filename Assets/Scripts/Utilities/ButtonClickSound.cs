using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ButtonClickSound : MonoBehaviour
{
    [SerializeField]
    private ClickSound clickSound = ClickSound.CLICK;
    private enum ClickSound { CLICK, SUCCESS }

    private AudioManager audioManager;
    private Button button;

    void Start()
    {
        audioManager = AudioManager.Instance;
        button = GetComponent<Button>();

        // Add listener
        button.onClick.AddListener(() => PlaySound());
    }

    public void PlaySound()
    {
        switch (clickSound)
        {
            case ClickSound.CLICK:
                audioManager.PlaySound(audioManager.audio_click);
                break;

            case ClickSound.SUCCESS:
                audioManager.PlaySound(audioManager.audio_clickSuccess);
                break;
        }
    }
}
