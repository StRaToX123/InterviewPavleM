using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class GameplayManager : MonoBehaviour
{
    public GameObject m_cardPrefab;
    public List<Texture> m_cardTextures;
    public GridLayoutGroup m_cardsGridLayoutGroup;
    
    public uint rowCount
    {
        get
        {
            return m_rowCount;
        }
        set
        {
            m_rowCount = value;
        }
    }

    public uint columnCount
    {
        get
        {
            return m_columnCount;
        }
        set
        {
            if ((m_rowCount * value) % 2 > 0)
            {
                Debug.Log("owrCount and columnCount multiplication needs to result with an even number");
                return;
            }

            m_columnCount = value;
        }
    }


    private Card[,] m_cardMatrix;
    private uint m_rowCount;
    private uint m_columnCount;
    private RectTransform m_cardHolder;

    void Start()
    {
        if (m_cardPrefab == null)
        {
            throw new Exception("No card prefab set");
        }

        m_cardHolder = m_cardsGridLayoutGroup.gameObject.GetComponent<RectTransform>();
        m_cardMatrix = new Card[m_rowCount, m_columnCount];
        // In order to optimize randomly choosing two cards from the m_cardMatrix and assigning them the same pairID,
        // instead of calling Random.Range a bunch of times in order to select to never before selected cards, we will
        // create this helper array which houses each cards flattened index. We will then shuffle the contents of the array
        // (a fixed number of Random.Range calls) and then simply iterate over it, taking each two conse consecutive values
        // as a randomly selected pair of indexes
        List<Tuple<uint, uint>> cardIndexes = new List<Tuple<uint, uint>>((int)(m_rowCount * m_columnCount));
        for (uint i = 0; i < m_rowCount; i++)
        {
            for (uint j = 0; j < m_columnCount; j++)
            {
                GameObject newCard = Instantiate(m_cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                m_cardMatrix[i, j] = newCard.GetComponent<Card>();
                // Parent to newly instantiated prefab to the grid layout
                newCard.transform.SetParent(m_cardHolder.transform, false);
                cardIndexes.Add(new Tuple<uint, uint>(i, j));
            }
        }


        // Assign card ids in pairs.
        // First we need to choose two cards from the m_cardMatrix
        // To do this we will jumble up the cardIndexes array first
        // then iterate over it, we will treat two consecutive indexes as pairs
        int iterationCount = Random.Range(5, 11);
        for (int i = 0; i < iterationCount; i++)
        {
            int randomIndex01 = Random.Range(0, cardIndexes.Count);
            int randomIndex02 = ((randomIndex01 * 20) - 43) % cardIndexes.Count;
            Tuple<uint, uint> backup = cardIndexes[randomIndex01];
            cardIndexes[randomIndex01] = cardIndexes[randomIndex02];
            cardIndexes[randomIndex02] = backup;
        }

        uint pairID = 0;
        for (int i = 0; i < cardIndexes.Count; i += 2)
        {
            m_cardMatrix[cardIndexes[i].Item1, cardIndexes[i].Item2].m_pairID = pairID;
            m_cardMatrix[cardIndexes[i + 1].Item1, cardIndexes[i + 1].Item2].m_pairID = pairID;

        }


        // Position each card in the canvas
        // This will be done automatically by the m_cardsGridLayout component
        // But in order to garantee that all the cards will be on-screen,
        // only their size needs to be adjusted (while preserving their 1.0 aspect ration)
        // Calculate how large the cards have to be in order to fit on-screen horizontally
        int cardWidth = (int)m_cardHolder.sizeDelta.x - m_cardsGridLayoutGroup.padding.horizontal;
        cardWidth -= (int)((m_columnCount - 1) * m_cardsGridLayoutGroup.spacing.x);
        cardWidth = (int)(cardWidth / m_columnCount);

        int cardHeight = (int)m_cardHolder.sizeDelta.y - m_cardsGridLayoutGroup.padding.vertical;
        cardHeight -= (int)((m_rowCount - 1) * m_cardsGridLayoutGroup.spacing.y);
        cardHeight = (int)(cardHeight / m_rowCount);

        int cardSize = cardWidth < cardHeight ? cardWidth : cardHeight;
        m_cardsGridLayoutGroup.cellSize = new Vector2(cardSize, cardSize);
    }




    void Update()
    {
        
    }
}
