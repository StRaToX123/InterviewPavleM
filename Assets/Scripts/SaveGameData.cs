using Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SaveGameData
{
    public int m_rowCount;
    public int m_columnCount;
    public int m_score;
    public int m_pairMatchScoreMultiplier;
    public int m_numberOfActiveCards;
    public float m_elapsedTime;
    public int m_cardsGridLayoutCellSize;
    public GridLayoutGroup.Constraint m_cardsGridLayoutConstraint;
    public int m_cardsGridLayoutConstraintCount;

    [System.Serializable]
    public class CardSaveData
    {
        public int m_spriteIndex;
        public bool m_isVisible;
    }

    // The matrix needs to be a list of lists instead of a 2D array
    // Because the JsonUtility cannot pickup multi dimensional arrays
    public TArray<CardSaveData> m_cardMatrix;

    public SaveGameData() 
    {
        m_score = 0;
        m_pairMatchScoreMultiplier = 1;
        m_numberOfActiveCards = 0;
        m_elapsedTime = 0.0f;
        m_cardsGridLayoutCellSize = 512;
        m_cardsGridLayoutConstraint = GridLayoutGroup.Constraint.FixedRowCount;
        m_cardsGridLayoutConstraintCount = 1;
    }
}
