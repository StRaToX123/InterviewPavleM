using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class GameplayManager : MonoBehaviour, ISaveGame
{
    public int m_pairMatchScoreValue = 10;
    public MenuManager m_menuManager;
    public GameObject m_cardPrefab;
    public List<Sprite> m_cardSprites;
    public GridLayoutGroup m_cardsGridLayoutGroup;
    public RectTransform m_cardHolderRect;
    [HideInInspector]
    public int m_rowCount = 2;
    [HideInInspector]
    public int m_columnCount = 2;
    [HideInInspector]
    public Tuple<int, int> m_lastSelectedCardIndexes = null;



    private Card[,] m_cardMatrix;
    
    private int m_score = 0;
    private int m_pairMatchScoreMultiplier = 1;
    private int m_numberOfActiveCards;
    private float m_elapsedTime = 0.0f;

    void OnEnable()
    {
        if (m_cardPrefab == null)
        {
            throw new Exception("No card prefab set");
        }

        m_menuManager.m_gameplayUIScoreText.text = "0";
        m_menuManager.m_gameplayUIMultiplierText.text = "x1";
        m_cardMatrix = new Card[m_rowCount, m_columnCount];
        m_numberOfActiveCards = m_rowCount * m_columnCount;
        // In order to optimize randomly choosing two cards from the m_cardMatrix and assigning them the same pairID,
        // instead of calling Random.Range a bunch of times in order to select never before selected cards, we will
        // create this helper array which houses each cards indexes. We will then shuffle the contents of the array
        // (a fixed number of Random.Range calls) and then simply iterate over it, taking each two consecutive values
        // as a randomly selected pair of indexes
        List<Tuple<int, int>> cardIndexes = new List<Tuple<int, int>>(m_numberOfActiveCards);
        for (int i = 0; i < m_rowCount; i++)
        {
            for (int j = 0; j < m_columnCount; j++)
            {
                GameObject newCardGameObject = Instantiate(m_cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                Card newCard = newCardGameObject.GetComponent<Card>();
                m_cardMatrix[i, j] = newCard;
                newCard.m_gameplayManager = this;
                newCard.m_cardIndexes = new Tuple<int, int>(i, j);

                // Parent to newly instantiated prefab to the grid layout
                newCardGameObject.transform.SetParent(m_cardHolderRect.transform, false);
                cardIndexes.Add(new Tuple<int, int>(i, j));
            }
        }


        // Assign card ids in pairs.
        // First we need to choose two cards from the m_cardMatrix
        // To do this we will jumble up the cardIndexes array first
        // then iterate over it, we will treat two consecutive indexes as pairs
        Random.InitState((int)System.DateTime.Now.Ticks);
        for (int i = 0; i < cardIndexes.Count / 2; i++)
        {
            int randomIndex01 = Random.Range(0, cardIndexes.Count);
            int randomIndex02 = Math.Abs((randomIndex01 * 20) - 43) % cardIndexes.Count;
            Tuple<int, int> backup = cardIndexes[randomIndex01];
            cardIndexes[randomIndex01] = cardIndexes[randomIndex02];
            cardIndexes[randomIndex02] = backup;
        }

        int spriteIndex = 0;
        for (int i = 0; i < cardIndexes.Count; i += 2)
        {
            m_cardMatrix[cardIndexes[i].Item1, cardIndexes[i].Item2].m_spriteIndex = spriteIndex;
            m_cardMatrix[cardIndexes[i].Item1, cardIndexes[i].Item2].m_frontSideSprite = m_cardSprites[spriteIndex];
            m_cardMatrix[cardIndexes[i + 1].Item1, cardIndexes[i + 1].Item2].m_spriteIndex = spriteIndex;
            m_cardMatrix[cardIndexes[i + 1].Item1, cardIndexes[i + 1].Item2].m_frontSideSprite = m_cardSprites[spriteIndex];
            spriteIndex += 1;
        }


        // Position each card in the canvas
        // This will be done automatically by the m_cardsGridLayout component
        // But in order to garantee that all the cards will be on-screen,
        // only their size needs to be adjusted (while preserving their 1.0 aspect ration)
        // Calculate how large the cards have to be in order to fit on-screen
        int cardWidth = (int)m_cardHolderRect.rect.width - m_cardsGridLayoutGroup.padding.horizontal;
        cardWidth -= (int)((m_columnCount - 1) * m_cardsGridLayoutGroup.spacing.x);
        cardWidth = (int)(cardWidth / m_columnCount);

        int cardHeight = (int)m_cardHolderRect.rect.height - m_cardsGridLayoutGroup.padding.vertical;
        cardHeight -= (int)((m_rowCount - 1) * m_cardsGridLayoutGroup.spacing.y);
        cardHeight = (int)(cardHeight / m_rowCount);

        if (cardWidth < cardHeight)
        {
            m_cardsGridLayoutGroup.cellSize = new Vector2(cardWidth, cardWidth);
            m_cardsGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            m_cardsGridLayoutGroup.constraintCount = (int)m_columnCount;
        }
        else
        {
            m_cardsGridLayoutGroup.cellSize = new Vector2(cardHeight, cardHeight);
            m_cardsGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            m_cardsGridLayoutGroup.constraintCount = (int)m_rowCount;
        }   
    }

    public void SelectCard(Tuple<int, int> cardIndexes)
    {
        if (m_lastSelectedCardIndexes != null)
        {
            Card previousCard = m_cardMatrix[m_lastSelectedCardIndexes.Item1, m_lastSelectedCardIndexes.Item2];
            Card currentCard = m_cardMatrix[cardIndexes.Item1, cardIndexes.Item2];
            // If the previous card's pair id and the newly selected card's pair id is the same
            if (previousCard.m_spriteIndex == currentCard.m_spriteIndex)
            {
                m_score += m_pairMatchScoreValue * m_pairMatchScoreMultiplier;
                m_pairMatchScoreMultiplier++;
                m_menuManager.m_gameplayUIScoreText.text = "SCORE: " + m_score.ToString();
                // We cannot destroy these cards because that will rearange the grid layout.
                // Instead we will just diable the image component
                previousCard.m_image.enabled = false;
                currentCard.m_image.enabled = false;
                m_numberOfActiveCards -= 2;
                // Did we finish the game
                if (m_numberOfActiveCards == 0)
                {
                    ShowVictoryScreen();
                }
            }
            else
            {
                m_pairMatchScoreMultiplier = 1;
                // Playing the flip animation
                previousCard.m_animation.Stop();
                previousCard.m_animation.Play();
                currentCard.m_animation.Stop();
                currentCard.m_animation.Play();
            }

            m_menuManager.m_gameplayUIMultiplierText.text = "x" + m_pairMatchScoreMultiplier.ToString();
            m_lastSelectedCardIndexes = null;
        }
        else
        {
            m_lastSelectedCardIndexes = cardIndexes;
        }
    }

    private void ShowVictoryScreen()
    {
        m_menuManager.m_gameplayUI.SetActive(false);
        m_menuManager.m_victoryMenu.SetActive(true);
        m_menuManager.m_victoryMenuModeText.text = "MODE: " + m_rowCount.ToString() + "X" + m_columnCount.ToString();
        m_menuManager.m_victoryMenuFinalScoreText.text = "FINAL SCORE: " + m_score.ToString();
        m_menuManager.m_victoryMenuFinalTimeText.text = "FINAL TIME: " + Mathf.FloorToInt(m_elapsedTime) + "sec";
    }

    void Update()
    {
        // Update the timer
        m_elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(m_elapsedTime / 60);
        int seconds = Mathf.FloorToInt(m_elapsedTime % 60);
        m_menuManager.m_gameplayUITimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void LoadData(SaveGameData data)
    {
        
    }

    public void SaveData(SaveGameData data)
    {
        data.m_score = m_score;
        data.m_pairMatchScoreMultiplier = m_pairMatchScoreMultiplier;
        data.m_numberOfActiveCards = m_numberOfActiveCards;
        data.m_elapsedTime = m_elapsedTime;
        data.m_cardMatrix = new SaveGameData.CardSaveData[m_rowCount, m_columnCount];
        for (int i = 0; i < m_rowCount; i++)
        {
            for (int j = 0; j < m_columnCount; j++)
            {
                data.m_cardMatrix[i, j].m_spriteIndex = m_cardMatrix[i, j].m_spriteIndex;
                data.m_cardMatrix[i, j].m_isVisible = m_cardMatrix[i, j].m_image.enabled;
            }
        }
    }
}
