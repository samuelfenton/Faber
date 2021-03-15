using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDrone01_Attack : State_Drone01
{
    private int m_currentFireCount = 0;
    private Character.ATTACK_INPUT_STANCE attackStance = Character.ATTACK_INPUT_STANCE.NONE;

    private int m_totalFireCount = 0;
    private ObjectPool m_objectPoolToUse = null;
    private string m_manoeuvreString = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_drone01.RotateWeaponTowardsTarget(m_drone01.m_weaponObject, m_drone01.m_target.transform.position + Vector3.up, m_drone01.m_maxFiringAngle);
        
        //TODO decide on what fire mode to take
        attackStance = Character.ATTACK_INPUT_STANCE.LIGHT;

        //Store variables for later usage
        if(attackStance == Character.ATTACK_INPUT_STANCE.LIGHT)
        {
            m_totalFireCount = m_drone01.m_lightProjectileCount;
            m_objectPoolToUse = m_drone01.m_objectPoolLightProjectile;
            m_manoeuvreString = CustomAnimation.BuildManoeuvreString(Manoeuvre.MANOEUVRE_TYPE.INAIR, Manoeuvre.MANOEUVRE_STANCE.LIGHT, 1);
        }
        else
        {
            m_totalFireCount = m_drone01.m_heavyProjectileCount;
            m_objectPoolToUse = m_drone01.m_objectPoolHeavyProjectile;
            m_manoeuvreString = CustomAnimation.BuildManoeuvreString(Manoeuvre.MANOEUVRE_TYPE.INAIR, Manoeuvre.MANOEUVRE_STANCE.HEAVY, 1);
        }

        m_customAnimator.PlayAnimation(m_manoeuvreString, CustomAnimation.LAYER.ATTACK, CustomAnimation.BLEND_TIME.INSTANT);
        m_drone01.FireProjectile(m_objectPoolToUse, m_drone01.m_projectileSpawnAnchor.transform.position, m_drone01.m_projectileSpawnAnchor.transform.rotation);

        m_drone01.SetDesiredVelocity(new Vector2(0.0f, 0.0f));

        m_currentFireCount = 1;
    }

/// <summary>
/// State update, perform any actions for the given state
/// </summary>
/// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
public override bool StateUpdate()
    {
        base.StateUpdate();

        if (m_drone01.m_target == null || !m_drone01.m_target.IsAlive())
            return true;

        m_drone01.RotateWeaponTowardsTarget(m_drone01.m_weaponObject, m_drone01.m_target.transform.position + Vector3.up, m_drone01.m_maxFiringAngle);

        if (m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.ATTACK))//End of current attack
        {
            if (m_currentFireCount >= m_totalFireCount)//Fired all needed shots
            {
                return true;
            }
            else
            {
                m_customAnimator.PlayAnimation(m_manoeuvreString, CustomAnimation.LAYER.ATTACK, CustomAnimation.BLEND_TIME.INSTANT);
                m_drone01.FireProjectile(m_objectPoolToUse, m_drone01.m_projectileSpawnAnchor.transform.position, m_drone01.m_projectileSpawnAnchor.transform.rotation);
                m_currentFireCount++;
            }

        }

        return false;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_drone01.StartAttackDelay();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_drone01.m_canAttackFlag && m_drone01.m_target != null && m_drone01.m_target.IsAlive() && m_drone01.m_objectPoolLightProjectile.HasSpareObject() && Pathfinding.GetDistance(m_drone01, m_drone01.m_target) <= m_drone01.m_attackEnterValue;
    }
}
