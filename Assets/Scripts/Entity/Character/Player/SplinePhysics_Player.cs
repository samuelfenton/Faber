using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePhysics_Player : SplinePhysics_Character
{
    private Character_Player m_player = null;
    public override void Init()
    {
        base.Init();
        m_player = GetComponent<Character_Player>();
    }

    /// <summary>
    /// Swap facing direction
    /// </summary>
    public override void SwapFacingDirection()
    {
        base.SwapFacingDirection();

        m_player.m_followCamera.FlipCamera();
    }
}
