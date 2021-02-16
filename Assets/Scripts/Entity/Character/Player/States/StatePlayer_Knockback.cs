using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePlayer_Knockback : State_Player
{
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

        float knockbackVelocity = 0.0f; //Velocity based off hity direction
        
        //Select animation, knockback impact is already set when being hit
        if (m_character.m_knockbackBodyHitDirection == Character.KNOCKBACK_DIR.FRONT)
        {
            m_customAnimator.PlayAnimation((int)CustomAnimation_Player.INTERRUPT_DEFINES.KNOCKBACK, CustomAnimation.LAYER.INTERRUPT);
            knockbackVelocity = m_character.m_knockbackVelocity;
        }
        else
        {
            m_customAnimator.PlayAnimation((int)CustomAnimation_Player.INTERRUPT_DEFINES.KNOCKFORWARD, CustomAnimation.LAYER.INTERRUPT);
            knockbackVelocity = m_character.m_knockforwardVelocity;
        }

        //Start knockback, modifiy direction based off spline allignment
        if (m_character.AllignedToSpline())
        {
            if (m_character.m_knockbackSpineDirection == Character.KNOCKBACK_SPLINE_DIR.NEGATIVE) //Knocked forwards
                knockbackVelocity *= -1.0f;
        }
        else
        {
            if (m_character.m_knockbackSpineDirection == Character.KNOCKBACK_SPLINE_DIR.POSITIVE) //Knocked backwards
                knockbackVelocity *= -1.0f;
        }

        m_character.m_splinePhysics.HardSetHorizontalVelocity(knockbackVelocity);

        if(m_character.m_splinePhysics.m_downCollision)//Grounded, dont worry about y-velocity
        {
            m_player.SetDesiredVelocity(new Vector2(knockbackVelocity, 0.0f));
        }
        else//In air, move downwards
        {
            m_player.SetDesiredVelocity(new Vector2(knockbackVelocity, -1.0f));
        }

    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.INTERRUPT);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_character.m_knockbackBodyHitDirection = Character.KNOCKBACK_DIR.NONE; //Reset flag
        m_character.m_knockbackSpineDirection = Character.KNOCKBACK_SPLINE_DIR.NONE; //Reset flag

        m_character.StartKnockbackRecover();

        m_player.m_splinePhysics.HardSetHorizontalVelocity(0.0f);
        m_player.SetDesiredVelocity(new Vector2(0.0f, m_player.m_splinePhysics.m_splineLocalVelocity.y));
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_character.m_knockbackBodyHitDirection != Character.KNOCKBACK_DIR.NONE && !m_inProgressFlag;
    }
}
