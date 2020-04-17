using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLayers : MonoBehaviour
{
    public static int m_walkable = 0;

    public static int m_character = 0;

    //-------------------
    //Get masks for future use
    //-------------------
    static CustomLayers()
    {
        m_walkable = LayerMask.GetMask("Walkable");
        m_character = LayerMask.GetMask("Character");
    }
}
