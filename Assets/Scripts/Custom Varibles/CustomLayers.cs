using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLayers : MonoBehaviour
{
    public static int m_enviroment = 0;
    public static int m_background = 0;
    public static int m_character = 0;

    //-------------------
    //Get masks for future use
    //-------------------
    static CustomLayers()
    {
        m_enviroment = LayerMask.GetMask("Enviroment");
        m_background = LayerMask.GetMask("Background");
        m_character = LayerMask.GetMask("Character");
    }
}
