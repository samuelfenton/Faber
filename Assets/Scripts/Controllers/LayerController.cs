using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerController : MonoBehaviour
{
    public static int m_walkable = 0;

    public static int m_character = 0;

    static LayerController()
    {
        m_walkable = LayerMask.GetMask("Walkable");
        m_character = LayerMask.GetMask("Character");
    }
}
