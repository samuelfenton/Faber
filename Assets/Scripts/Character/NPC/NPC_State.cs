using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_State : State
{
    protected NPC_Character m_NPCCharacter = null;
    protected List<Pathing_Spline> m_path = new List<Pathing_Spline>();

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_NPCCharacter = (NPC_Character)p_character;
    }

    /// <summary>
    /// Is the character close enough based off current speed and slowdown rate
    ///     Kinematic equation
    ///     Vf^2 = Vi^2 + 2ad
    ///     d = -Vi^2 / 2a
    ///     As a is negitive -> d = Vi^2 / 2a
    /// </summary>
    /// <param name="p_target">Target to check against</param>
    /// <param name="p_distance">Distance from target that returns true</param>
    /// <returns>True when can stop in time for distance</returns>
    protected bool TargetWithinRange(Vector3 p_target, float p_distance)
    {
        float stoppingDistance = m_character.m_localVelocity.x == 0.0f ? 0 : Mathf.Pow(m_character.m_localVelocity.x, 2) / (2 * m_character.m_groundedHorizontalDeacceleration); //Close enough based off time to slow down

        return Vector3.Distance(m_character.transform.position, p_target) <= p_distance + stoppingDistance;
    }

    /// <summary>
    /// Move towards a spine
    /// </summary>
    /// <param name="p_spline">target spline</param>
    /// <returns>true when completed, invalid data, no path</returns>
    protected bool MoveTowardsSpline(Pathing_Spline p_spline)
    {
        if (p_spline == null)//Invalid destination
            return true;

        if (m_NPCCharacter.m_splinePhysics.m_currentSpline == p_spline)//already there
            return true;

        if(m_path.Count == 0)//Needs new path
        {
            m_path = PathfindingController.GetPath(m_character, p_spline);
        }

        if(m_path[m_path.Count-1] != p_spline)//goal changed
        {
            m_path = PathfindingController.GetPath(m_character, p_spline);
        }

        if (m_path.Count == 0)//no path
            return true;

        Pathing_Spline currentSpline = m_character.m_splinePhysics.m_currentSpline;
        if (currentSpline == m_path[0])//At next spline
            m_path.RemoveAt(0);

        if (m_path.Count == 0)//finished path
            return true;

        Pathing_Spline goalSpline = m_path[0];
        float desiredPecent = DetermineDesiredPercent(goalSpline);

        MoveTowardsPercent(desiredPecent);
        return false;
    }

    /// <summary>
    /// Move towards a given entity
    /// </summary>
    /// <returns>true when completed or invalid data</returns>
    protected bool MoveTowardsEntity(Entity p_entity)
    {
        if (p_entity == null)
            return true;

        return MoveTowardsSpline(p_entity.m_splinePhysics.m_currentSpline);
    }

    /// <summary>
    /// Setup velocity for moving towards a percent on a spline
    /// </summary>
    /// <param name="p_desiredPercent">Desired spline percent</param>
    protected void MoveTowardsPercent(float p_desiredPercent)
    {
        Pathing_Spline currentSpline = m_character.m_splinePhysics.m_currentSpline;

        Vector3 newVelocity = m_character.m_localVelocity;
        int input = DetermineDirectionInput(currentSpline, p_desiredPercent);

        newVelocity.x += m_character.m_groundHorizontalAccel * input * Time.deltaTime;
        newVelocity.x = Mathf.Clamp(newVelocity.x, -m_character.m_groundHorizontalMax, m_character.m_groundHorizontalMax);

        m_character.m_localVelocity = newVelocity;
    }


    /// <summary>
    /// Get the percent for the next spine 
    /// </summary>
    /// <param name="p_goalSpline">goal spline</param>
    /// <returns>0.0f for node A, defaults to 1.0f</returns>
    protected float DetermineDesiredPercent(Pathing_Spline p_goalSpline)
    {
        Pathing_Spline currentSpline = m_character.m_splinePhysics.m_currentSpline;

        if (currentSpline.m_nodeA.m_adjacentSplines.Contains(p_goalSpline))
            return 0.0f;
        return 1.0f;
    }

    /// <summary>
    /// Determine what "input" the NPC should have
    /// </summary>
    /// <param name="p_currentSpline">Current spline of NPC</param>
    /// <param name="p_desiredPercent">Target Percent</param>
    /// <returns></returns>
    protected int DetermineDirectionInput(Pathing_Spline p_currentSpline, float p_desiredPercent)
    {
        Vector3 splineForward = p_currentSpline.GetForwardDir(m_character.m_splinePhysics.m_currentSplinePercent);
        Vector3 characterForward = transform.forward;

        float forwardDot = Vector3.Dot(characterForward, splineForward);

        if (p_desiredPercent < 0.5f)//going to node A, same alingment go backwards or -1, otherwise 1
        {
            return forwardDot >= 0.0f ? -1 : 1;
        }
        else//going to node A, same alingment go forwards or 1, otherwise -1
        {
            return forwardDot >= 0.0f ? 1 : -1;
        }
    }
}
