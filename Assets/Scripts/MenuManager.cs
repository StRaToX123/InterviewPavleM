using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject m_mainMenu;
    public GameObject m_newGameMenu;
    public GameObject m_victoryMenu;
    public GameObject m_cardHolder;
    public TMP_Dropdown m_newGameMenuRowCountDropDown;
    public TMP_Dropdown m_newGameMenuColumnCountDropDown;
    public Button m_newGameMenuStartButton;
    public GameplayManager m_gameplayManager;

    void Start()
    {
        // Populate the rowCount dropdown
        m_newGameMenuRowCountDropDown.options.Clear();
        m_newGameMenuColumnCountDropDown.interactable = false;
        m_newGameMenuStartButton.interactable = false;

        m_newGameMenuRowCountDropDown.options.Capacity = m_gameplayManager.m_cardSprites.Count;
        m_newGameMenuColumnCountDropDown.options.Capacity = m_gameplayManager.m_cardSprites.Count;

        for (int i = 1; i <= m_gameplayManager.m_cardSprites.Count; i++)
        {
            m_newGameMenuRowCountDropDown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
    }

    public void OnMainMenuNewGameButtonClick()
    {
        m_mainMenu.SetActive(false);
        m_newGameMenu.SetActive(true);
    }

    public void OnMainMenuContinueButtonClick()
    {
        
    }

    public void OnNewGameMenuBackButtonClick()
    {
        m_newGameMenu.SetActive(false);
        m_mainMenu.SetActive(true);
    }

    public void OnNewGameMenuRowCountChanged()
    {
        m_newGameMenuColumnCountDropDown.options.Clear();
        m_newGameMenuColumnCountDropDown.interactable = true;
        for (int i = 1; i <= m_gameplayManager.m_cardSprites.Count; i++)
        {
            // If the row count times the column count, equals an even number than this
            // column count option is valid and should be added do the column cound dropdown menu
            int multValue = (m_newGameMenuRowCountDropDown.value + 1) * i;
            if ((multValue % 2 == 0) && (multValue <= m_gameplayManager.m_cardSprites.Count * 2))
            {
                m_newGameMenuColumnCountDropDown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
            }
        }

        m_newGameMenuColumnCountDropDown.onValueChanged.Invoke(0);
    }

    public void OnNewGameMenuColumnCountChanged()
    {
        m_newGameMenuStartButton.interactable = true;
    }

    public void OnNewGameMenuStartButtonClick()
    {
        m_gameplayManager.m_rowCount = m_newGameMenuRowCountDropDown.value + 1;
        int columnCount = 0;
        if (int.TryParse(m_newGameMenuColumnCountDropDown.options[m_newGameMenuColumnCountDropDown.value].text, out columnCount))
        {
            m_gameplayManager.m_columnCount = columnCount;
        }
        else
        {
            Debug.Log("Failed parsing the column count number");
        }

        m_newGameMenuColumnCountDropDown.interactable = false;
        m_newGameMenuStartButton.interactable = false;
        m_newGameMenu.SetActive(false);
        m_cardHolder.SetActive(true);
        m_gameplayManager.enabled = true;
        m_gameplayManager.SetGameplayUIVisibility(true);
    }

    public void OnGamePlayBackButtonClick()
    {
        m_gameplayManager.enabled = false;
        m_gameplayManager.SetGameplayUIVisibility(false);
        m_cardHolder.SetActive(false);
        m_mainMenu.SetActive(true);
    }

    public void OnVictoryMenuBackButtonClick()
    {
        OnGamePlayBackButtonClick();
    }
}
