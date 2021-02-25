using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController_InGame : SceneController
{
    [Tooltip("Prefab used to define a spline")]
    public GameObject m_splinePrefab = null;
    public Interactable_SavePoint m_defaultSavePoint = null;

    private Character_Player m_playerCharacter = null;

    private List<Entity> m_entities = new List<Entity>();
    private List<Interactable> m_interactables = new List<Interactable>();

    [HideInInspector]
    public CustomInput m_customInput = null;

    [HideInInspector]
    public enum INGAME_STATE {IN_GAME, PAUSED}
    private INGAME_STATE m_inGameState = INGAME_STATE.IN_GAME;

    private List<Entity> m_additionalEntities = new List<Entity>();
    private List<Entity> m_entitiesToRemove = new List<Entity>();

    /// <summary>
    /// On setting game state, resets input
    /// </summary>
    public INGAME_STATE InGameState
    {
        get { return m_inGameState; }
        set { 
            m_inGameState = value;
            m_customInput.ResetInput();
        }
    }

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();

        if(m_splinePrefab == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(name + " has no assigned spline prefab");
#endif
            Destroy(this);
        }
        if (m_defaultSavePoint == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(name + " doesnt have an assigned default spawn point, dying will result in returning to main menu");
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (MasterController.Instance.m_currentSaveSlot == -1)//Load slot 0 as default when debugging
        {
            MasterController.Instance.m_currentSaveSlot = 0;
            DataController.LoadInGameSaveData();
        }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (m_sceneDefine == MasterController.SCENE.MAIN_MENU || m_sceneDefine == MasterController.SCENE.LOADING)//Load slot 0 as default when debugging
        {
            Debug.LogWarning(name + " Has its scene defined, but as loading or main menu, this is incorrect");
        }
#endif
        m_customInput = gameObject.AddComponent<CustomInput>();
        m_playerCharacter = FindObjectOfType<Character_Player>();

        m_interactables.AddRange(FindObjectsOfType<Interactable>());
        m_entities.AddRange(FindObjectsOfType<Entity>());
        
        //Pathing
        Pathing_Node[] pathingNodes = FindObjectsOfType<Pathing_Node>();
        for (int nodeIndex = 0; nodeIndex < pathingNodes.Length; nodeIndex++)
        {
            pathingNodes[nodeIndex].InitNode(m_splinePrefab);
        }

        //Entities
        for (int entityIndex = 0; entityIndex < m_entities.Count; entityIndex++)
        {
            m_entities[entityIndex].InitEntity();
        }

        //Add all additional entities created during original entity initialization
        //These should already be setup
        m_entities.AddRange(m_additionalEntities);
        m_additionalEntities.Clear();

        //Interactables
        for (int interactableIndex = 0; interactableIndex < m_interactables.Count; interactableIndex++)
        {
            m_interactables[interactableIndex].InitInteractable(m_playerCharacter);
        }

        RespawnPlayer(true);

        DataController.InGameSaveData inGameSaveData = MasterController.Instance.m_inGameSaveData;

        //Add in save point data
        for (int unlockedSavePointIndex = 0; unlockedSavePointIndex < inGameSaveData.m_unlockedSavePoints.Length; unlockedSavePointIndex++)
        {
            DataController.SavePointData unlockedSavePoint = inGameSaveData.m_unlockedSavePoints[unlockedSavePointIndex];
            if (unlockedSavePoint.IsValid() && unlockedSavePoint.m_saveSceneIndex == (int)m_sceneDefine) //Save point is in this scene, and its unlocked
            {
                Interactable_SavePoint savePoint = GetSavePoint(unlockedSavePoint.m_savePointID);
                if (savePoint != null)
                {
                    savePoint.ToggleLit();
                }
            }
        }
    }

    /// <summary>
    /// Update the scene controller as needed
    /// should be called form master controller
    /// </summary>
    public override void UpdateSceneController()
    {
        if (m_customInput == null)
            return;

        m_customInput.UpdateInput();
        if(m_inGameState == INGAME_STATE.IN_GAME)
        {
            foreach (Entity entity in m_entities)
            {
                if (entity.isActiveAndEnabled)
                    entity.UpdateEntity();
            }

            foreach (Interactable interactable in m_interactables)
            {
                if (interactable.isActiveAndEnabled)
                    interactable.UpdateInteractable();
            }

            //Remove as needed
            foreach (Entity entity in m_entitiesToRemove)
            {
                m_entities.Remove(entity);
            }
            m_entitiesToRemove.Clear();
        }
    }

    /// <summary>
    /// Update the scene controller as needed
    /// should be called form master controller
    /// Fixed update only
    /// </summary>
    public override void FixedUpdateSceneController()
    {
        if (m_inGameState == INGAME_STATE.IN_GAME)
        {
            foreach (Entity entity in m_entities)
            {
                if(entity.isActiveAndEnabled)
                    entity.FixedUpdateEntity();
            }
        }
    }

    /// <summary>
    /// Add a list of entities to the in game controllers list. This could be used as entities are created on awake, e.g. object pools
    /// </summary>
    /// <param name="p_entitiesToAdd">All entities to adf</param>
    public void AddEntities(List<Entity> p_entitiesToAdd)
    {
        m_additionalEntities.AddRange(p_entitiesToAdd);
    }

    /// <summary>
    /// Remove an entity from the list of all entities
    /// </summary>
    /// <param name="p_entityToRemove">Entity to remove</param>
    public void RemoveEntity(Entity p_entityToRemove)
    {
        m_entitiesToRemove.Add(p_entityToRemove);
    }

    /// <summary>
    /// Attempt to use save point
    /// Will add to list of unlocked save points
    /// </summary>
    /// <param name="p_savePoint">Point to set as current and add to unlocked</param>
    public void UseSavePoint(Interactable_SavePoint p_savePoint)
    {
        DataController.SaveInGameSaveData(p_savePoint);
    }

    /// <summary>
    /// Player can respanw in three cases
    /// On death
    /// On respawn
    /// On load of scene
    /// </summary>
    /// <param name="p_snapCamera">Should the camera snap into place?</param>
    public void RespawnPlayer(bool p_snapCamera)
    {
        //Attempt to place player
        DataController.LoadInGameSaveData();
        DataController.InGameSaveData m_inGameSaveData = MasterController.Instance.m_inGameSaveData;

        Interactable_SavePoint savePoint = null;

        if (m_inGameSaveData.m_lastSavePoint.IsValid()) //Valid save
        {
            if (m_inGameSaveData.m_lastSavePoint.m_saveSceneIndex == (int)m_sceneDefine)//correct scene, teleport to
            {
                savePoint = GetSavePoint(m_inGameSaveData.m_lastSavePoint.m_savePointID);
            }
            else
            {
                MasterController.Instance.LoadScene((MasterController.SCENE)m_inGameSaveData.m_lastSavePoint.m_saveSceneIndex, true);
            }
        }
        else if(m_defaultSavePoint != null) //New game
        {
            DataController.SaveInGameSaveData(m_defaultSavePoint);
            DataController.SaveCharacterStatistics(m_playerCharacter.m_characterStatistics);

            savePoint = m_defaultSavePoint;
        }

        if (savePoint != null)
        {
            m_playerCharacter.m_splinePhysics.m_nodeA = savePoint.m_nodeA;
            m_playerCharacter.m_splinePhysics.m_nodeB = savePoint.m_nodeB;
            m_playerCharacter.m_splinePhysics.m_currentSplinePercent = savePoint.m_splinePercent;

            //force updatye of player position so camera will follow
            m_playerCharacter.transform.position = m_playerCharacter.m_splinePhysics.m_currentSpline.GetPosition(m_playerCharacter.m_splinePhysics.m_currentSplinePercent);

            if (p_snapCamera)
            {
                m_playerCharacter.m_followCamera.ForceSnap();
            }
        }
    }

    /// <summary>
    /// Attempt to get interactive save point
    /// </summary>
    /// <param name="p_uniqueID">ID to look for</param>
    /// <returns>Script for interactive point, default to null</returns>
    public Interactable_SavePoint GetSavePoint(int p_uniqueID)
    {
        //Attempt to find level save point
        for (int interactableIndex = 0; interactableIndex < m_interactables.Count; interactableIndex++)
        {
            if (m_interactables[interactableIndex].m_uniqueID.m_val == p_uniqueID)
            {
                return (Interactable_SavePoint)m_interactables[interactableIndex];
            }
        }

        return null;
    }
}
