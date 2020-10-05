using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_WallJump : State_Player
{
    private enum WALL_JUMP_STATE {LAND, HANG, JUMP}
    private WALL_JUMP_STATE m_currentState = WALL_JUMP_STATE.LAND;

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

        m_entity.m_splinePhysics.HardSetVelocity(Vector2.zero);
        m_character.m_splinePhysics.m_gravity = false;
        m_currentState = WALL_JUMP_STATE.LAND;
        m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.WALL_LAND);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        if(!InputTowardsWall())//player let go, so drop
        {
            return true;
        }

        //Allow player to jump and move
        switch (m_currentState)
        {
            case WALL_JUMP_STATE.LAND:
                if(m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
                {
                    m_currentState = WALL_JUMP_STATE.HANG;
                    m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.WALL_HANG);
                }
                if(m_player.m_customInput.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_currentState = WALL_JUMP_STATE.JUMP;
                    m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.WALL_JUMP);
                    m_character.m_splinePhysics.HardSetVelocity(m_character.m_wallJumpVelocity);
                }
                break;
            case WALL_JUMP_STATE.HANG:
                if (m_player.m_customInput.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_currentState = WALL_JUMP_STATE.JUMP;
                    m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.WALL_JUMP);
                    m_character.m_splinePhysics.HardSetVelocity(m_character.m_wallJumpVelocity);
                }
                break;
            case WALL_JUMP_STATE.JUMP:
                return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE);
        }

        return false;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_character.m_splinePhysics.m_gravity = true;

        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        //Not grounded, holding direction of wall in direction of travel.
        return !m_character.m_splinePhysics.m_downCollision && InputTowardsWall();
    }

    /// <summary>
    /// Is players input towards the wall
    /// </summary>
    /// <returns>true when players input is towards wall</returns>
    public bool InputTowardsWall()
    {
        if(m_player.m_followCamera.m_currentOrientation == FollowCamera.CAMERA_ORIENTATION.INITIAL)
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
