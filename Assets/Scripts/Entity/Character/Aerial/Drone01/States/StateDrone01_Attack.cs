using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDrone01_Attack : State_Drone01
{
    //NOTE
    //Although player state runs on the interupt animation layer, it does not behave like a interrupt state

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

        m_customAnimator.PlayAnimation(CustomAnimation.BuildManoeuvreString(Manoeuvre.MANOEUVRE_TYPE.INAIR, Manoeuvre.MANOEUVRE_STANCE.LIGHT, 1), CustomAnimation.LAYER.ATTACK, CustomAnimation.BLEND_TIME.INSTANT);

        m_drone01.RotateWeaponTowardsTarget(m_drone01.m_weaponObject, m_drone01.m_target.transform.position + Vector3.up, m_drone01.m_maxFiringAngle);

        PoolObject projectilePoolObject = m_drone01.m_objectPoolLightProjectile.RentObject(m_drone01.m_projectileSpawnAnchor.transform.position, m_drone01.m_weaponObject.transform.rotation);

        Projectile projectileScript = projectilePoolObject.GetComponent<Projectile>();
        if(projectileScript != null)
        {
            projectileScript.ProjectileStart(m_drone01, MOARMaths.ConvertFromVector3ToVector2(m_drone01.m_projectileSpawnAnchor.transform.position - transform.position), 1.0f);
        }
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

        return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.ATTACK);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_drone01.m_target != null && m_drone01.m_objectPoolLightProjectile.HasSpareObject();
    }
}
