using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerController : MonoBehaviour
{
    public static int m_enviromentInclined = 0;
    public static int m_enviromentWalkable = 0;

    static LayerController()
    {
        m_enviromentInclined = LayerMask.GetMask("EnviromentInclined");
        m_enviromentWalkable = LayerMask.GetMask("EnviromentWalkable");
    }
}
