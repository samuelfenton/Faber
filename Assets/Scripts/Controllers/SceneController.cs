using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [Header("Assigned Variables")]
    public MasterController.SCENE m_sceneDefine = MasterController.SCENE.MAIN_MENU;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// Update the scene controller as needed
    /// should be called form master controller
    /// </summary>
    public virtual void UpdateSceneController()
    {

    }

    /// <summary>
    /// Update the scene controller as needed
    /// should be called form master controller
    /// Fixed update only
    /// </summary>
    public virtual void FixedUpdateSceneController()
    {

    }
}
