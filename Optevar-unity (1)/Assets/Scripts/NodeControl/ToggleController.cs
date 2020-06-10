using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
	private RectTransform handleRect;
	private RectTransform buttonRect;

	// for setting handle position 
	private float buttonSize;
	private float handleSize;
	private float handleOffset = 12.0f;

	// canvasGroup for setting text transparency
	private CanvasGroup placingCG;
	private CanvasGroup moniteringCG;

	// for handle moving
	private float speed = 1.0f;
	private float t = 0.0f;

	// handle position
	private float onPlacing;		
	private float onMonitering;

	// on click
	private bool switching = false;

	void Start()
	{
		placingCG	 = this.gameObject.transform.GetChild(0).GetComponent<CanvasGroup>();
		moniteringCG = this.gameObject.transform.GetChild(1).GetComponent<CanvasGroup>();
		handleRect   = this.gameObject.transform.GetChild(2).GetComponent<RectTransform>();
		buttonRect	 = this.gameObject.GetComponent<RectTransform>();

		buttonSize = buttonRect.sizeDelta.x;
		handleSize = handleRect.sizeDelta.x;
		onMonitering = (buttonSize - handleSize) / 2 - handleOffset;
		onPlacing = onMonitering * -1;

		if (FunctionManager.IsPlacingMode)
		{
			placingCG.alpha = 1.0f;
			moniteringCG.alpha = 0.0f;
			handleRect.localPosition = new Vector3(onPlacing, 0.0f, 0.0f);
		}
		else
		{
			placingCG.alpha = 1.0f;
			moniteringCG.alpha = 0.0f;
			handleRect.localPosition = new Vector3(onMonitering, 0.0f, 0.0f);
		}
	}

	void Update()
	{
		if (switching)
		{
			Toggle(FunctionManager.IsPlacingMode);
		}
	}

	public void SwitchModeValue()
	{
		switching = true;
	}


	public void Toggle(bool isPlacingMode)
	{
		if (!isPlacingMode)
		{
			Debug.Log(handleRect.localPosition.x);
			SetTextTransparency(placingCG, 1.0f, 0.0f);
			SetTextTransparency(moniteringCG, 0.0f, 1.0f);
			handleRect.localPosition = SmoothMove(onPlacing, onMonitering);
		}
		else
		{
			SetTextTransparency(placingCG, 0.0f, 1.0f);
			SetTextTransparency(moniteringCG, 1.0f, 0.0f);
			handleRect.localPosition = SmoothMove(onMonitering, onPlacing);
		}

	}

	Vector3 SmoothMove(float startPosX, float endPosX)
	{
		Vector3 position = new Vector3(Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
		StopSwitching();
		return position;
	}

	void SetTextTransparency(CanvasGroup cg, float startAlpha, float endAlpha)
	{
		cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t += (speed+1.5f) * Time.deltaTime);
	}

	void StopSwitching()
	{
		if (t > 1.0f)
		{
			switching = false;
			t = 0.0f;
		}
	}
}