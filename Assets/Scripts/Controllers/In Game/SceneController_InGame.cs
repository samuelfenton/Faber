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

    private Entity[] m_entities;
    private Interactable[] m_interactables;

    [HideInInspector]
    public CustomInput m_customInput = null;

    [HideInInspector]
    public enum INGAME_STATE {IN_GAME, PAUSED}
    private INGAME_STATE m_inGameState = INGAME_STATE.IN_GAME;

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
            DataController.LoadGameSavingPoint();
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

        m_interactables = FindObjectsOfType<Interactable>();
        m_entities = FindObjectsOfType<Entity>();
        
        //Pathing
        Pathing_Node[] pathingNodes = FindObjectsOfType<Pathing_Node>();
        for (int nodeIndex = 0; nodeIndex < pathingNodes.Length; nodeIndex++)
        {
            pathingNodes[nodeIndex].InitNode(m_splinePrefab);
        }

        //Entities
        for (int entityIndex = 0; entityIndex < m_entities.Length; entityIndex++)
        {
            m_entities[entityIndex].InitEntity();
        }


        //Interactables
        for (int interactableIndex = 0; interactableIndex < m_interactables.Length; interactableIndex++)
        {
            m_interactables[interactableIndex].InitInteractable(m_playerCharacter);
        }

        RespawnPlayer(true) ;
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
                entity.UpdateEntity();
            }

            foreach (Interactable interactable in m_interactables)
            {
                interactable.UpdateInteractable();
            }
        }
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
        DataController.InGameSaveData m_inGameSaveData = MasterController.Instance.m_inGameSaveData;

        if(m_inGameSaveData.IsValid())
        {
            if (m_inGameSaveData.m_saveSceneIndex == (int)m_sceneDefine)//correct scene, teleport to
            {
                //Attempt to find level save point
                for (int interactableIndex = 0; interactableIndex < m_interactables.Length; interactableIndex++)
                {
                    if (m_interactables[interactableIndex].m_uniqueID == m_inGameSaveData.m_savePointID)
                    {
                        Interactable_SavePoint savePoint = (Interactable_SavePoint)m_interactables[interactableIndex];

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
                }
            }
            else
            {
                MasterController.Instance.LoadScene((MasterController.SCENE)m_inGameSaveData.m_saveSceneIndex, true);
            }
        }
    }
}
