using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SayPromoPrefab : MonoBehaviour {

	public GameObject box;
	VideoPlayer videoPlayer;
	public Text titleText;
	public Text buttonText;

	// Use this for initialization
	void Awake () {
		init();
	}

	void init() {
		videoPlayer = GetComponent<VideoPlayer>();
		GetComponent<Button>().onClick.AddListener(Click);
		box.SetActive(false);
		
		videoPlayer.prepareCompleted += (source) => {
			StartCoroutine(showBox());
		};
	}

	IEnumerator showBox() {
		// wait 1 frame
		yield return 0;
		box.SetActive(true);
	}

	public void Show() {
		if (videoPlayer == null) {
			init();
		}
		
		if (videoPlayer != null) {
			SayPromo.show(videoPlayer, titleText, buttonText);
		}
	}

	public void Hide() {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
		box.SetActive(false);
	}

	void Click() {
		if (box.activeSelf) {
			SayPromo.click();
		}
	}

	void OnVideoPlayerError(VideoPlayer source, string message) {
		Hide();
		SayKit.trackEvent("cross_error", message);
	}
	
}
