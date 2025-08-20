using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;


[System.Serializable]
public class GridMapSetting
{
    public Vector2Int startMin = Vector2Int.zero;
    public Vector2Int startMax = Vector2Int.zero;
    public Vector2Int endMin = new Vector2Int(9, 9);
    public Vector2Int endMax = new Vector2Int(9, 9);

    public int sizeX = 10;
    public int sizeY = 10;
    public int minTurnCount = 5;
    public int numberOfMaps = 100;

    public Vector2Int GetRandomStart()
    {
        var x = Random.Range(startMin.x, startMax.x + 1);
        var y = Random.Range(startMin.y, startMax.y + 1);
        return new Vector2Int(x, y);
    }

    public Vector2Int GetRandomEnd()
    {
        var x = Random.Range(endMin.x, endMax.x + 1);
        var y = Random.Range(endMin.y, endMax.y + 1);
        return new Vector2Int(x, y);
    }
}

public class GridMapGeneratorWindow : EditorWindow
{
    private List<GridMapSetting> settings = new() { new GridMapSetting() };
    private string savePath = "Assets/GridDataCollection.asset";

    [MenuItem("Tools/Grid Map Generator")]
    public static void ShowWindow()
    {
        GetWindow<GridMapGeneratorWindow>("Grid Map Generator");
    }

    private Vector2 scrollPos;

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Grid Map Settings List", EditorStyles.boldLabel);

        for (int i = 0; i < settings.Count; i++)
        {
            var setting = settings[i];
            EditorGUILayout.LabelField($"Map Setting #{i + 1}", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Start Position Range");
            setting.startMin = EditorGUILayout.Vector2IntField("Start Min", setting.startMin);
            setting.startMax = EditorGUILayout.Vector2IntField("Start Max", setting.startMax);

            EditorGUILayout.LabelField("End Position Range");
            setting.endMin = EditorGUILayout.Vector2IntField("End Min", setting.endMin);
            setting.endMax = EditorGUILayout.Vector2IntField("End Max", setting.endMax);

            setting.sizeX = EditorGUILayout.IntField("Map Width (X)", setting.sizeX);
            setting.sizeY = EditorGUILayout.IntField("Map Height (Y)", setting.sizeY);
            setting.minTurnCount = EditorGUILayout.IntField("Min Direction Changes", setting.minTurnCount);
            setting.numberOfMaps = EditorGUILayout.IntField("Number of Maps", setting.numberOfMaps);

            if (GUILayout.Button("Remove This Setting") && settings.Count > 1)
            {
                settings.RemoveAt(i);
                break;
            }

            EditorGUILayout.Space(10);
        }

        if (GUILayout.Button("Add New Setting"))
        {
            settings.Add(new GridMapSetting());
        }

        savePath = EditorGUILayout.TextField("Asset Save Path", savePath);

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate and Save Maps"))
        {
            GenerateAndSave();
        }
    }

    private void GenerateAndSave()
    {
        var asset = ScriptableObject.CreateInstance<GridDataAsset>();
        asset.groups = new List<GridDataGroup>();
        var breakCount = 10000;
        var gridIndex = 0;
        foreach (var setting in settings)
        {
            var maxCount = setting.numberOfMaps;
            var currentBreakCount = 0;
            while (maxCount >= 1)
            {
                if (currentBreakCount++ > breakCount)
                {
                    break;
                }
                
                // 시작 위치, 종료 위치 지정
                var start = setting.GetRandomStart();
                var end = setting.GetRandomEnd();

                // grid 데이터 생성
                var grid = GridDataGenerator.CreateGridData(
                    gridIndex,
                    start,
                    end,
                    setting.sizeX,
                    setting.sizeY,
                    setting.minTurnCount);
                
                if (grid == null)
                {
                    Debug.LogError("Grid Map Generator Error");
                }
                else
                {
                    Debug.Log($"✅ Success with {grid.MinTurnCount} turns");
                    gridIndex++;
                    maxCount--;
                    var existingGroup = asset.groups.Find(x => x.minTurnCount == grid.MinTurnCount);
                    if (existingGroup == null)
                    {
                        asset.groups.Add(new GridDataGroup
                            { minTurnCount = grid.MinTurnCount, grids = new List<GridData>() { grid } });
                    }
                    else
                    {
                        existingGroup.grids.Add(grid);
                    }
                }
            }
        }

        asset.groups.Sort((a, b) => a.minTurnCount.CompareTo(b.minTurnCount));
        
        AssetDatabase.CreateAsset(asset, savePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Generated {settings.Count} map sets. Saved to {savePath}");
    }
}
