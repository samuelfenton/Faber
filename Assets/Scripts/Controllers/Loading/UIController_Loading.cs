using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController_Loading : UIController
{

    [Header("UI Variables")]
    public GameObject m_readyButton = null;
    public TextMeshProUGUI m_textLoadingPercentage = null;

    private enum LOADING_STATE {LOADING, AWAITING_PLAYER_READY, AWAITING_SCENE_ACTIVATION}
    private LOADING_STATE m_currentLoadingState = LOADING_STATE.LOADING;

    private AsyncOperation m_asyncSceneLoading = null;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public override void Init()
    {
        base.Init();

        if (m_readyButton == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have its assigned UI objects");
#endif
            Destroy(gameObject);
            return;
        }

        m_asyncSceneLoading = MasterController.Instance.m_asyncSceneLoading;

        m_textLoadingPercentage.gameObject.SetActive(true);
        m_readyButton.SetActive(false);

        m_currentLoadingState = LOADING_STATE.LOADING;

        StartCoroutine(UpdateLoadingUI());
    }

    /// <summary>
    /// Update of loading screen
    /// Update loading animation
    /// </summary>
    private IEnumerator UpdateLoadingUI()
    {
        yield return null;

        switch (m_currentLoadingState)
        {
            case LOADING_STATE.LOADING:
                if(m_textLoadingPercentage != null)
                    m_textLoadingPercentage.text = m_asyncSceneLoading.progress/0.009f + "%";

                if (m_asyncSceneLoading.progress >= 0.9f)
                {
                    m_textLoadingPercentage.gameObject.SetActive(false);
                    m_readyButton.SetActive(true);
                    m_currentLoadingState = LOADING_STATE.AWAITING_PLAYER_READY;
                }
                break;
            case LOADING_STATE.AWAITING_PLAYER_READY:
                break;
            case LOADING_STATE.AWAITING_SCENE_ACTIVATION:
                if(m_asyncSceneLoading.isDone)
                {
                    MasterController.Instance.SceneLoaded();
                }
                break;
            default:
                break;
        }

        StartCoroutine(UpdateLoadingUI());
    }

    /// <summary>
    /// Button to go form loading to loaded scene
    /// </summary>
    public void Btn_ReadyToPlay()
    {
        m_asyncSceneLoading.allowSceneActivation = true;
        m_currentLoadingState = LOADING_STATE.AWAITING_SCENE_ACTIVATION;
    }
}
