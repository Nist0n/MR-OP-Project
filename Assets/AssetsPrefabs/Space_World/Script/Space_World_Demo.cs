using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Space_World_Demo : MonoBehaviour {

	List<GameObject> models = new List<GameObject>();

	int currentModelIndex = 0;
	float buttonSize = 60f;
	float angle = 0;
	float speed = 10f;

	// Use this for initialization
	void Start() {
		// fill models list
		foreach (Transform child in transform) {
			models.Add(child.gameObject);
		}
		ShowSelected();
	}

	// Update is called once per frame
	void Update() {
		MouseController();
		KeyboardController();
		RecalcPos();
	}

	void MouseController() {
		if (Input.GetMouseButton(1) == false)
			return;
		float mouseX = Input.GetAxis("Mouse X");
		angle += mouseX * speed;
	}

	void KeyboardController() {
		float keyX = 0f;
		if (Input.GetKey(KeyCode.UpArrow)) {
			keyX = -Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.DownArrow)) {
			keyX = Time.deltaTime;
		}
		angle += keyX * speed * 3;
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			SafeIncCurrectModelIndex();
			ShowSelected();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			SafeDecCurrectModelIndex();
			ShowSelected();
		}
	}

	void OnGUI() {
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(" < ", GUILayout.Width(buttonSize), GUILayout.Height(buttonSize))) {
			SafeDecCurrectModelIndex();
			ShowSelected();
		};

		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.fontSize = 30;
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.normal.textColor = Color.red;
		GUILayout.Label(models[currentModelIndex].name, labelStyle, GUILayout.Width(Screen.width - buttonSize * 2.15f), GUILayout.Height(buttonSize));

		if (GUILayout.Button(" > ", GUILayout.Width(buttonSize), GUILayout.Height(buttonSize))) {
			SafeIncCurrectModelIndex();
			ShowSelected();
		};
		GUILayout.EndHorizontal();
	}

	void SafeIncCurrectModelIndex() {
		currentModelIndex++;
		if (currentModelIndex >= models.Count) {
			currentModelIndex = 0;
		}
	}

	void SafeDecCurrectModelIndex() {
		currentModelIndex--;
		if (currentModelIndex < 0) {
			currentModelIndex = models.Count - 1;
		}
	}

	void ShowSelected() {
		for (int i = 0; i < models.Count; i++) {
			models[i].SetActive(i == currentModelIndex ? true : false);
		}
	}

	void RecalcPos() {
		angle += Time.deltaTime * speed;
		transform.localRotation = Quaternion.Euler(0, angle, 0);
	}
}
