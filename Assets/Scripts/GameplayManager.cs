using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    private int m_score;
    private int m_pairMatchScoreMultiplier;
    private int m_numberOfActiveCards;
    private float m_elapsedTime;


    public void StartNewGame()
    {
        ClearData();
        m_score = 0;
        m_pairMatchScoreMultiplier = 1;
        m_numberOfActiveCards = m_rowCount * m_columnCount;
        m_elapsedTime = 0.0f;
        UpdateGameplayUI();
         m_cardMatrix = new Card[m_rowCount, m_columnCount];
        // In order to optimize randomly choosing two cards from the m_cardMatrix and assigning them the same spriteIndex,
        // instead of calling Random.Range a bunch of times in order to select never before selected cards, we will
        // create this helper array which houses each cards indexes. We will then shuffle the contents of the array
        // (a fixed number of Random.Range calls) and then simply iterate over it, taking each two consecutive values
        // as a randomly selected pair of indexes
        List<Tuple<int, int>> cardIndexes = new List<Tuple<int, int>>(m_numberOfActiveCards);
        CreateCardMatrix(m_rowCount, m_columnCount, card => {
            cardIndexes.Add(new Tuple<int, int>(card.m_cardIndexes.Item1, card.m_cardIndexes.Item2));
            card.m_isVisible = true;
        });
        
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

    private void CreateCardMatrix(int rowCount, int columnCount, Action<Card> cardModificationFunction)
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                GameObject newCardGameObject = Instantiate(m_cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                Card newCard = newCardGameObject.GetComponent<Card>();
                m_cardMatrix[i, j] = newCard;
                newCard.m_gameplayManager = this;
                newCard.m_cardIndexes = new Tuple<int, int>(i, j);
                cardModificationFunction?.Invoke(newCard);

                // Parent to newly instantiated prefab to the grid layout
                newCardGameObject.transform.SetParent(m_cardHolderRect.transform, false);
            }
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
                // We cannot destroy these cards because that will rearange the grid layout.
                // Instead we will just diable the fro component
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

            UpdateGameplayUI();
            m_lastSelectedCardIndexes = null;
        }
        else
        {
            m_lastSelectedCardIndexes = cardIndexes;
        }
    }

    private void UpdateGameplayUI()
    {
        m_menuManager.m_gameplayUIScoreText.text = "SCORE: " + m_score.ToString();
        m_menuManager.m_gameplayUIMultiplierText.text = "MULT: x" + m_pairMatchScoreMultiplier.ToString();
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
        ClearData();
        m_rowCount = data.m_rowCount;
        m_columnCount = data.m_columnCount;
        m_score = data.m_score;
        m_pairMatchScoreMultiplier = data.m_pairMatchScoreMultiplier;
        m_numberOfActiveCards = data.m_numberOfActiveCards;
        m_elapsedTime = data.m_elapsedTime;
        m_cardMatrix = new Card[m_rowCount, m_columnCount];
        CreateCardMatrix(m_rowCount, m_columnCount, card =>
        {
            // Assign the sprite index and the visibility status from the loaded data
            card.m_spriteIndex = data.m_cardMatrix[card.m_cardIndexes.Item1, card.m_cardIndexes.Item2].m_spriteIndex;
            card.m_frontSideSprite = m_cardSprites[card.m_spriteIndex];
            card.m_isVisible = data.m_cardMatrix[card.m_cardIndexes.Item1, card.m_cardIndexes.Item2].m_isVisible;
        });

        m_cardsGridLayoutGroup.cellSize = new Vector2(data.m_cardsGridLayoutCellSize, data.m_cardsGridLayoutCellSize);
        m_cardsGridLayoutGroup.constraint = data.m_cardsGridLayoutConstraint;
        m_cardsGridLayoutGroup.constraintCount = data.m_cardsGridLayoutConstraintCount;

        UpdateGameplayUI();
    }

    public void SaveData(SaveGameData data)
    {
        data.m_rowCount = m_rowCount;
        data.m_columnCount = m_columnCount;
        data.m_score = m_score;
        data.m_pairMatchScoreMultiplier = m_pairMatchScoreMultiplier;
        data.m_numberOfActiveCards = m_numberOfActiveCards;
        data.m_elapsedTime = m_elapsedTime;
        data.m_cardMatrix = new TArray<SaveGameData.CardSaveData>(m_rowCount, m_columnCount);
        for (int i = 0; i < m_rowCount; i++)
        {
            for (int j = 0; j < m_columnCount; j++)
            {
                SaveGameData.CardSaveData newCardSaveData = new SaveGameData.CardSaveData();
                newCardSaveData.m_spriteIndex = m_cardMatrix[i, j].m_spriteIndex;
                newCardSaveData.m_isVisible = m_cardMatrix[i, j].m_image.enabled;
                data.m_cardMatrix[i, j] = newCardSaveData;
            }
        }

        data.m_cardsGridLayoutCellSize = (int)m_cardsGridLayoutGroup.cellSize.x;
        data.m_cardsGridLayoutConstraint = m_cardsGridLayoutGroup.constraint;
        data.m_cardsGridLayoutConstraintCount = m_cardsGridLayoutGroup.constraintCount;
    }

    public void ClearData()
    {
        if (m_cardMatrix != null)
        {
            for (int i = 0; i < m_cardMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < m_cardMatrix.GetLength(1); j++)
                {
                    Destroy(m_cardMatrix[i, j].gameObject);
                }
            }
        }

        m_cardMatrix = null;
    }

    private void ShowVictoryScreen()
    {
        SaveGameManager.DeleteSaveGame();
        m_menuManager.m_gameplayUI.SetActive(false);
        m_menuManager.m_victoryMenu.SetActive(true);
        m_menuManager.m_victoryMenuModeText.text = "MODE: " + m_rowCount.ToString() + "X" + m_columnCount.ToString();
        m_menuManager.m_victoryMenuFinalScoreText.text = "FINAL SCORE: " + m_score.ToString();
        m_menuManager.m_victoryMenuFinalTimeText.text = "FINAL TIME: " + Mathf.FloorToInt(m_elapsedTime) + "sec";
    }
}
