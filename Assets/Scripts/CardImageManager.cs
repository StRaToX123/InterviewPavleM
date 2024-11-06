using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Represents a pool of all available card images.
// Also provides a list of all the possible row x column count combinations
// based on the amount of card images available
public class CardImageManager : MonoBehaviour
{
    public List<Texture> m_cardImages;
}
