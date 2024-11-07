using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(Button))]
public class Card : MonoBehaviour
{
    // Automatically assigned by the GameplayManager
    public int m_spriteIndex;
    [HideInInspector]
    public Tuple<int, int> m_cardIndexes;
    public Sprite m_backSideSprite;
    public Sprite m_frontSideSprite;
    [HideInInspector]
    public Image m_image;
    [HideInInspector]
    public Animation m_animation;
    [HideInInspector]
    private UnityEngine.UI.Button m_button;
    [HideInInspector]
    public GameplayManager m_gameplayManager;

    // False for backSideSprite, true for frontSideSprite
    private bool m_isFlipped = false;
    

    private void Start()
    {
        m_image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
        m_animation = this.gameObject.GetComponent<Animation>();
        m_button = this.gameObject.GetComponent<UnityEngine.UI.Button>();
    }

    public void OnCardClick()
    {
        // The card is not cliclable again until a second one is selected
        // The GameplayManager will set the flip animation for the second time
        // if a missmatch occured
        if (m_isFlipped == false)
        {
            m_animation.Play();
        }
    }

    // Called via an event inside the CardFlip animation clip
    public void OnFlipAnimationEvent()
    {
        m_isFlipped = !m_isFlipped;
        if (m_isFlipped == true)
        {
            m_image.sprite = m_frontSideSprite;
            m_button.interactable = false;
        }
        else
        {
            m_image.sprite = m_backSideSprite;
        }
    }

    public void OnFlipAnimationComplete()
    {
        if (m_isFlipped == true)
        {
            m_gameplayManager.SelectCard(m_cardIndexes);
        }

        m_button.interactable = true;
    }
}
