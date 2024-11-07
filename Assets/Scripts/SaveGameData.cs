using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveGameData
{
    public int m_score;
    public int m_pairMatchScoreMultiplier;
    public int m_numberOfActiveCards;
    public float m_elapsedTime;
    public struct CardSaveData
    {
        public int m_spriteIndex;
        public bool m_isVisible;
    }

    public CardSaveData[,] m_cardMatrix;

    public SaveGameData() 
    {
        m_score = 0;
        m_pairMatchScoreMultiplier = 1;
        m_numberOfActiveCards = 0;
        m_elapsedTime = 0.0f;
        m_cardMatrix = null;
    }
}
