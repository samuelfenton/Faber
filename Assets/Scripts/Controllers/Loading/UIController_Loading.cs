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

    private enum LOADING_STATE {LOADING, LOADING_DONE}
    private LOADING_STATE m_currentLoadingState = LOADING_STATE.LOADING;

    private AsyncOperation m_asyncSceneLoading = null;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public override void Init(MasterController p_masterController)
    {
        base.Init(p_masterController);

        if (m_readyButton == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have its assigned UI objects");
#endif
            Destroy(gameObject);
            return;
        }

        m_asyncSceneLoading = p_masterController.m_asyncSceneLoading;

        m_textLoadingPercentage.gameObject.SetActive(true);
        m_readyButton.SetActive(false);

        m_currentLoadingState = LOADING_STATE.LOADING;
    }

    /// <summary>
    /// Update of loading screen
    /// Update loading animation
    /// </summary>
    private void Update()
    {
        switch (m_currentLoadingState)
        {
            case LOADING_STATE.LOADING:
                if(m_textLoadingPercentage != null)
                    m_textLoadingPercentage.text = m_asyncSceneLoading.progress + "%";

                if (m_asyncSceneLoading.isDone)
                {
                    m_textLoadingPercentage.gameObject.SetActive(false);
                    m_readyButton.SetActive(true);
                    m_currentLoadingState = LOADING_STATE.LOADING_DONE;
                }
                break;
            case LOADING_STATE.LOADING_DONE:
                break;
            default:
                break;
        }
    }
    
    public void Btn_ReadyToPlay()
    {
        MasterController.Instance.SceneLoaded();
    }
}
