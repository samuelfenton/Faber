using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Flyer_Drone : NPC_Flyer
{
    public GameObject m_bodyObject = null;
    public GameObject m_hoverComponentObject = null;
    public GameObject m_weaponObject = null;

    public float m_degreesOfTilt = 30.0f;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        if (m_bodyObject == null || m_hoverComponentObject == null || m_weaponObject == null)
        {
#if UNITY_EDITOR
            Debug.Log(gameObject.name + " drone has been setup incorrectly, has missing assigned parts");
#endif
        }
    }
}
