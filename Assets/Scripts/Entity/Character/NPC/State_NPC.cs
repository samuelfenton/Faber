using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_NPC : State_Character
{
    protected Character_NPC m_NPCCharacter = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);

        m_NPCCharacter = (Character_NPC)p_entity;
    }

    /// <summary>
    /// Check distance based off position, much faster, but wont be accurate
    /// </summary>
    /// <param name="p_target">Target to check against</param>
    /// <param name="p_distance">Distance from target that returns true</param>
    /// <returns>True when distance is close enough</returns>
    protected bool FastTargetWithinRange(Entity p_target, float p_distance)
    {
        return (p_target.transform.position - m_entity.transform.position).magnitude < p_distance;
    }

    /// <summary>
    /// Slow distance check, builds path and uses spline percents
    /// </summary>
    /// <param name="p_target">Target to check against</param>
    /// <param name="p_distance">Distance from target that returns true</param>
    /// <returns>True when distance is close enough</returns>
    protected bool SlowTargetWithinRange(Entity p_target, float p_distance)
    {
        if (p_target == null)
            return false;

        Pathing_Spline currentSpline = m_entity.m_splinePhysics.m_currentSpline;
        Pathing_Spline targetSpline = p_target.m_splinePhysics.m_currentSpline;
        float currentPercent = m_entity.m_splinePhysics.m_currentSplinePercent;
        float targetPercent = p_target.m_splinePhysics.m_currentSplinePercent;

        if (currentSpline == targetSpline) //Same spline compare difference in percents
        {
            float splineDistance = Mathf.Abs(currentPercent - targetPercent) * currentSpline.m_splineLength;

            return splineDistance < p_distance;
        }
        else //Get path, add up all
        {
            List<Pathing_Spline> path = ControllerPathfinding.GetPath(m_entity, targetSpline);

            if(path.Count == 0)
            {
                return false;
            }

            path.RemoveAt(path.Count - 1);//Dont want to add in last spline distance

            float totalDistance = 0.0f;
            
            if(path.Count == 0) //Theres no paths inbetween
            {
                float currentToNodeDistance = Mathf.Abs(currentPercent - DetermineDesiredPercent(targetSpline));
                totalDistance += currentToNodeDistance;

                float targetToNodeDistance = Mathf.Abs(targetPercent - DetermineDesiredPercent(currentSpline));
                totalDistance += targetToNodeDistance;
            }
            else //Include pathing
            {
                float currentToNodeDistance = Mathf.Abs(m_entity.m_splinePhysics.m_currentSplinePercent - DetermineDesiredPercent(path[0]));
                totalDistance += currentToNodeDistance;

                float targetToNodeDistance = Mathf.Abs(p_target.m_splinePhysics.m_currentSplinePercent - DetermineDesiredPercent(path[path.Count -1]));
                totalDistance += targetToNodeDistance;

                foreach (Pathing_Spline spline in path)
                {
                    totalDistance += spline.m_splineLength;
                }
            }

            return totalDistance < p_distance;
        }
    }

    /// <summary>
    /// Uses both methods to ensure entity is in range, first fast to ensure checking is worth it, and then slow
    /// </summary>
    /// <param name="p_target">Target to check against</param>
    /// <param name="p_distance">Distance from target that returns true</param>
    /// <returns>True when distance is close enough</returns>
    protected bool SmartTargetWithinRange(Entity p_target, float p_distance)
    {
        if(FastTargetWithinRange(p_target, p_distance))
            return SlowTargetWithinRange(p_target, p_distance);
        return false;
    }

    /// <summary>
    /// Move towards a spine
    /// </summary>
    /// <param name="p_spline">target spline</param>
    /// <param name="p_speed">Desired speed to move at</param>
    /// <returns>true when completed, invalid data, no path</returns>
    protected void MoveTowardsSpline(Pathing_Spline p_spline, float p_speed)
    {
        if (p_spline == null)//Invalid destination
            return;

        if (m_NPCCharacter.m_splinePhysics.m_currentSpline == p_spline)//already there
            return;

        if(m_NPCCharacter.m_path.Count == 0 || m_NPCCharacter.m_path[m_NPCCharacter.m_path.Count - 1] != p_spline)//Needs new path
        {
            m_NPCCharacter.m_path = ControllerPathfinding.GetPath(m_entity, p_spline);
        }

        if (m_NPCCharacter.m_path.Count == 0)//no path
            return;

        Pathing_Spline currentSpline = m_entity.m_splinePhysics.m_currentSpline;
        if (currentSpline == m_NPCCharacter.m_path[0])//At next spline
            m_NPCCharacter.m_path.RemoveAt(0);

        if (m_NPCCharacter.m_path.Count == 0)//finished path
            return;

        Pathing_Spline currentGoalSpline = m_NPCCharacter.m_path[0];

        float desiredPecent = DetermineDesiredPercent(currentGoalSpline);

        MoveTowardsPercent(desiredPecent, p_speed);
        return;
    }

    /// <summary>
    /// Move towards a given entity
    /// </summary>
    /// <param name="p_entity">Entity to move towards</param>
    /// <param name="p_speed">Desired speed to move at</param>
    /// <returns>true when completed or invalid data</returns>
    protected void MoveTowardsEntity(Entity p_entity, float p_speed)
    {
        if (p_entity == null)
            return;

        Pathing_Spline currentSpline = m_entity.m_splinePhysics.m_currentSpline;
        Pathing_Spline targetSpline = p_entity.m_splinePhysics.m_currentSpline;

        if (currentSpline == targetSpline)
            MoveTowardsPercent(p_entity.m_splinePhysics.m_currentSplinePercent, p_speed);
        else
            MoveTowardsSpline(targetSpline, p_speed);
    }

    /// <summary>
    /// Setup velocity for moving towards a percent on a spline
    /// </summary>
    /// <param name="p_desiredPercent">Desired spline percent</param>
    /// <param name="p_speed">Desired speed to move at</param>
    protected void MoveTowardsPercent(float p_desiredPercent, float p_speed)
    {
        Pathing_Spline currentSpline = m_entity.m_splinePhysics.m_currentSpline;

        int input = DetermineDirectionInput(currentSpline, p_desiredPercent);
        m_character.SetDesiredVelocity(input * p_speed);
    }


    /// <summary>
    /// Get the percent for the next spine 
    /// </summary>
    /// <param name="p_goalSpline">goal spline</param>
    /// <returns>0.0f for node A, defaults to 1.0f</returns>
    protected float DetermineDesiredPercent(Pathing_Spline p_goalSpline)
    {
        Pathing_Spline currentSpline = m_entity.m_splinePhysics.m_currentSpline;

        if (currentSpline.m_nodePrimary.m_adjacentSplines.Contains(p_goalSpline))
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
        Vector3 splineForward = p_currentSpline.GetForwardDir(m_entity.m_splinePhysics.m_currentSplinePercent);
        Vector3 characterForward = transform.forward;

        float forwardDot = Vector3.Dot(characterForward, splineForward);

        if (p_desiredPercent < m_entity.m_splinePhysics.m_currentSplinePercent)// towards A, same forward allingment use -1
        {
            return forwardDot >= 0.0f ? -1 : 1;
        }
        else// towards B, same forward allingment use -1
        {
            return forwardDot >= 0.0f ? 1 : -1;
        }
    }
}
