using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundSetup : MonoBehaviour
{
    [Header("Primary Buttons")]
    [SerializeField] private Button[] primaryButtons;

    [Header("Secondary Buttons")]
    [SerializeField] private Button[] secondaryButtons;

    private void Start()
    {
        foreach (Button btn in primaryButtons)
            if (btn != null)
                btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClickPrimary());

        foreach (Button btn in secondaryButtons)
            if (btn != null)
                btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClickSecondary());
    }
}
