using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    protected MasterController m_masterController = null;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public virtual void Init(MasterController p_masterController)
    {
        m_masterController = p_masterController;
    }
}
