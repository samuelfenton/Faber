using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLayers : MonoBehaviour
{
    public static int m_enviromentMask = 0;
    public static int m_enviromentLayer = 0;
    public static int m_backgroundMask = 0;
    public static int m_backgroundLayer = 0;

    //Collisions
    public static int m_hitBoxMask = 0;
    public static int m_hitBoxLayer = 0;
    public static int m_hurtBoxMask = 0;
    public static int m_hurtBoxLayer = 0;
    public static int m_pushBoxMask = 0;
    public static int m_pushBoxLayer = 0;


    //-------------------
    //Get masks for future use
    //-------------------
    static CustomLayers()
    {
        //Masks
        m_enviromentMask = LayerMask.GetMask("Enviroment");
        m_backgroundMask = LayerMask.GetMask("Background");

        m_hitBoxMask = LayerMask.GetMask("Hit Box");
        m_hurtBoxMask = LayerMask.GetMask("Hurt Box");
        m_pushBoxMask = LayerMask.GetMask("Push Box");

        //Layer
        m_enviromentLayer = (int)Mathf.Log(m_enviromentMask, 2);
        m_backgroundLayer = (int)Mathf.Log(m_backgroundMask, 2);

        m_hitBoxLayer = (int)Mathf.Log(m_hitBoxMask, 2);
        m_hurtBoxLayer = (int)Mathf.Log(m_hurtBoxMask, 2);
        m_pushBoxLayer = (int)Mathf.Log(m_pushBoxMask, 2);
    }
}
