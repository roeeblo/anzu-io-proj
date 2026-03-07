using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public static class FindMissingScripts
{
	[MenuItem("Tools/Find Missing Scripts In Project")]
	public static void FindInProject()
	{
		var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
		var sceneGuids = AssetDatabase.FindAssets("t:Scene");
		var list = new List<string>();
		int totalMissing = 0;

		foreach (string guid in prefabGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			int n = CountMissingOnPrefab(path);
			if (n > 0)
			{
				totalMissing += n;
				list.Add($"Prefab: {path} ({n} missing)");
			}
		}

		foreach (string guid in sceneGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			int n = CountMissingInScene(path);
			if (n > 0)
			{
				totalMissing += n;
				list.Add($"Scene: {path} ({n} missing)");
			}
		}

		if (list.Count == 0)
		{
			Debug.Log("No missing scripts found in prefabs or scenes.");
			return;
		}

		Debug.LogWarning($"[FindMissingScripts] Total missing script refs: {totalMissing}. See list below.");
		foreach (var line in list)
			Debug.LogWarning(line);
	}

	static int CountMissingOnPrefab(string assetPath)
	{
		int count = 0;
		GameObject[] roots = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<GameObject>().ToArray();
		foreach (var go in roots)
		{
			if (go == null) continue;
			foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
				count += CountMissingOnGameObject(t.gameObject);
		}
		return count;
	}

	static int CountMissingInScene(string scenePath)
	{
		string currentPath = EditorSceneManager.GetActiveScene().path;
		EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		int count = 0;
		foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
		{
			foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
				count += CountMissingOnGameObject(t.gameObject);
		}
		if (!string.IsNullOrEmpty(currentPath) && currentPath != scenePath)
			EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);
		return count;
	}

	static int CountMissingOnGameObject(GameObject go)
	{
		int count = 0;
		var components = go.GetComponents<Component>();
		foreach (var c in components)
		{
			if (c == null)
				count++;
		}
		return count;
	}
}
