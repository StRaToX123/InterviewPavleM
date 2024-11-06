using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Image))]
public class Card : MonoBehaviour
{
    // Automatically assigned by the GameplayManager
    public uint m_pairID;
    public Texture m_backSideTexture;
    public Texture m_frontSideTexture;
    [HideInInspector]
    public Image m_image;

    // False for backSideTexture, true for frontSideTexture
    private bool m_isFlipped = false;


    private void Start()
    {
        m_image = this.gameObject.GetComponent<Image>();
    }

    // Called via an event inside the CardFlip animation clip
    private void Flip()
    {
        m_image.image = m_isFlipped ? m_frontSideTexture : m_backSideTexture;
        m_isFlipped = !m_isFlipped;
    }
}
