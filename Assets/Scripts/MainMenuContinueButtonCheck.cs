using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuContinueButtonCheck : MonoBehaviour
{
    public UnityEngine.UI.Button m_mainMenuContinueButton;

    void OnEnable()
    {
        if (SaveGameManager.DoesSaveGameExist())
        {
            m_mainMenuContinueButton.interactable = true;
        }
        else
        {
            m_mainMenuContinueButton.interactable = false;
        }
    }

}
