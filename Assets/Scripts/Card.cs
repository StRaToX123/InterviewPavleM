using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

[RequireComponent(typeof(Image))]
public class Card : MonoBehaviour
{
    // Automatically assigned by the GameplayManager
    public uint m_pairID;
    public Sprite m_backSideSprite;
    public Sprite m_frontSideSprite;
    [HideInInspector]
    public Image m_image;

    // False for backSideTexture, true for frontSideTexture
    private bool m_isFlipped = false;


    private void Start()
    {
        m_image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
    }

    // Called via an event inside the CardFlip animation clip
    private void Flip()
    {
        m_image.sprite = m_isFlipped ? m_frontSideSprite : m_backSideSprite;
        m_isFlipped = !m_isFlipped;
    }
}
