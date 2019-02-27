using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserHandler_Trail_SpeedBased : VoxeliserHandler_Trail
{
    [Header("Custom Emitting")]
    public float m_minChanceToEmit = 0.0f;
    public float m_maxChanceToEmit = 5.0f;

    private float m_currentSpeedPercent = 0.0f;

    //--------------------
    //  Get trail direction
    //  param:
    //      p_velocity - Local velocity of character
    //      p_percentToMaxVelocity - How fast the character is moving relative to max speed, e.g. 1 = velocity is at max speed
    //--------------------
    public void UpdateTrailingVelocity(Vector3 p_velocity, float p_percentToMaxVelocity)
    {
        m_trailDirection = p_velocity; //Go backwards from the velocity
        m_chanceOfTrailing = m_minChanceToEmit + p_percentToMaxVelocity * (m_maxChanceToEmit - m_minChanceToEmit);
        m_currentSpeedPercent = p_percentToMaxVelocity;
    }

    //--------------------
    //  Get trail direction
    //  return:
    //      Vector3 - Direction for voxel to travel
    //--------------------
    public override Vector3 GetTrailDirection()
    {
        return m_trailDirection;
    }

    //--------------------
    //  Get voxel speed
    //  return:
    //      float - Varible m_trailingSpeed
    //--------------------
    public override float GetTrailSpeed()
    {
        return m_trailingSpeed * m_currentSpeedPercent;
    }
}
