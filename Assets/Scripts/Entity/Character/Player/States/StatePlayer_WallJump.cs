using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePlayer_WallJump : State_Player
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

        m_customAnimator.PlayAnimation((int)CustomAnimation_Player.BASE_DEFINES.WALL_JUMP, CustomAnimation.LAYER.BASE);

        //Allow player to jump again
        m_character.m_doubleJumpFlag = m_character.m_characterStatistics.HasAbility(CharacterStatistics.ABILITY.DOUBLE_JUMP);
        m_character.m_inAirDashFlag = m_character.m_characterStatistics.HasAbility(CharacterStatistics.ABILITY.IN_AIR_DASH);

        //Velocity
        m_entity.m_splinePhysics.m_gravity = false;
        m_character.m_splinePhysics.HardSetVelocity(m_character.m_wallJumpVelocity);

        m_character.SwapFacingDirection();
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        m_character.SetDesiredVelocity(m_character.m_wallJumpVelocity);//Keep desired velocity till jump is done
        return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE);

    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_entity.m_splinePhysics.m_gravity = true;

        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        //Not grounded, holding direction of wall in direction of travel.
        return !m_character.m_splinePhysics.m_downCollision && MovingTowardWalls() && m_player.m_customInput.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED;
    }

    /// <summary>
    /// Is players input towards the wall
    /// </summary>
    /// <returns>true when players input is towards wall</returns>
    public bool MovingTowardWalls()
    {
        if(m_player.m_followCamera.m_currentOrientation == FollowCamera.CAMERA_ORIENTATION.FORWARD)
        {
            return (m_player.m_customInput.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL) > 0.0f && m_character.m_splinePhysics.m_forwardCollision && m_character.m_splinePhysics.m_forwardCollisionType == SplinePhysics.COLLISION_TYPE.ENVIROMENT) ||
            (m_player.m_customInput.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL) < 0.0f && m_character.m_splinePhysics.m_backCollision && m_character.m_splinePhysics.m_backCollisionType == SplinePhysics.COLLISION_TYPE.ENVIROMENT);
        }
        else
        {
            return (m_player.m_customInput.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL) < 0.0f && m_character.m_splinePhysics.m_forwardCollision && m_character.m_splinePhysics.m_forwardCollisionType == SplinePhysics.COLLISION_TYPE.ENVIROMENT) ||
            (m_player.m_customInput.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL) > 0.0f && m_character.m_splinePhysics.m_backCollision && m_character.m_splinePhysics.m_backCollisionType == SplinePhysics.COLLISION_TYPE.ENVIROMENT);
        }
    }
}
