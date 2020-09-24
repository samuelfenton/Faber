using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [Header("Assigned Variables")]
    public MasterController.SCENE m_sceneDefine = MasterController.SCENE.SCENE_COUNT;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public virtual void Init()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (m_sceneDefine == MasterController.SCENE.SCENE_COUNT)//Load slot 0 as default when debugging
        {
            Debug.LogWarning(name + " hasnt had its scene defined");
        }
#endif
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
