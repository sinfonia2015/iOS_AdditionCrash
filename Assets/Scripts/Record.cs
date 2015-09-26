using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Fresvii.AppSteroid;
using Fresvii.AppSteroid.Models;

public class Record : MonoBehaviour {

	// ボタンオブジェクト
	public Button startRecBtn;
	public Button stopRecBtn;

	// 録画制限時間
	public float limitTime = 10.0f;

	// 録画制限時間の一時保管変数
	float limitTimeTemp;

	// 「RECORD」ボタンを押して録画
	public void RecVideo() {
		//Debug.Log("録画開始");
		if (FASPlayVideo.StartRecording()) {
			startRecBtn.gameObject.SetActive(false);
			stopRecBtn.gameObject.SetActive(true);
		}
	}
	
	// 録画ストップ
	public void StopRec() {
		FASPlayVideo.StopRecording();
		startRecBtn.gameObject.SetActive(true);
		stopRecBtn.gameObject.SetActive(false);
		//		Debug.Log("録画終了");
		StartCoroutine("ShareMovie");
		limitTimeTemp = limitTime;
	}

	// シェア画面を開くコルーチン
	IEnumerator ShareMovie() {
		while(!FASPlayVideo.LatestVideoExists())
			yield return 1;
		if (!FASPlayVideo.ShowLatestVideoSharingGUIWithUGUI(Application.loadedLevelName))
		{
			Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog("Error : Recorded video does not exist", delegate(bool del) { });
		}
	}

	void Start () {
		limitTimeTemp = limitTime;
	}
	
	void Update () {
		if(FASPlayVideo.IsRecording()) {
			limitTimeTemp -= Time.deltaTime;
			
			if(limitTimeTemp <= 0.0f) {
				StopRec();
			}
		}
	}
}
