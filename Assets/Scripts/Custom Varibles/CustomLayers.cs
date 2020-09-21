using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLayers : MonoBehaviour
{
    public static int m_enviroment = 0;
    public static int m_background = 0;

    //Collisions
    public static int m_hitBox = 0;
    public static int m_hurtBox = 0;
    public static int m_pushBox = 0;

    //-------------------
    //Get masks for future use
    //-------------------
    static CustomLayers()
    {
        m_enviroment = LayerMask.GetMask("Enviroment");
        m_background = LayerMask.GetMask("Background");

        m_hitBox = LayerMask.GetMask("Hit Box");
        m_hurtBox = LayerMask.GetMask("Hurt Box");
        m_pushBox = LayerMask.GetMask("Push Box");
    }
}
