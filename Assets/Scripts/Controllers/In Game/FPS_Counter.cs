using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS_Counter : MonoBehaviour
{
    private TextMeshProUGUI m_text = null;

    private float m_udateRate = 0.5f;

    private void Start()
    {
        m_text = GetComponent<TextMeshProUGUI>();
		StartCoroutine(FPSLoop());
    }

	private IEnumerator FPSLoop()
	{
		// Capture frame-per-second
		int lastFrameCount = Time.frameCount;
		float lastTime = Time.realtimeSinceStartup;
		yield return new WaitForSeconds(m_udateRate);
		float timeSpan = Time.realtimeSinceStartup - lastTime;
		int frameCount = Time.frameCount - lastFrameCount;

		// Display it
		int FPS = Mathf.RoundToInt(frameCount / timeSpan);
		m_text.text = FPS.ToString() + " fps";

		StartCoroutine(FPSLoop());
	}
}
