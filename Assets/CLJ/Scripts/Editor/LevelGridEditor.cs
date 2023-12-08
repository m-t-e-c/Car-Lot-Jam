using UnityEngine;
using UnityEditor;

namespace CLJ
{
    [CustomEditor(typeof(LevelGrid))]
    public class LevelGridEditor : Editor
    {
        private LevelGrid _levelGrid;

        private void OnEnable()
        {
            _levelGrid = (LevelGrid)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField("Grid Properties", EditorStyles.boldLabel);
            DrawGridProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Object List", EditorStyles.boldLabel);
            DrawGridObjectListButtons();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            DrawGrid();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Save/Load", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                _levelGrid.SaveGrid();
            }
            if (GUILayout.Button("Load"))
            {
                _levelGrid.LoadGrid();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawGridProperties()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Grid"))
            {
                _levelGrid.GenerateGrid();
            }

            if (GUILayout.Button("Reset"))
            {
                _levelGrid.ResetGrid();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawGridObjectListButtons()
        {
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("None"))
            {
                _levelGrid.SetObjectToPlace(null);   
            }
            
            foreach (GridObject gridObject in _levelGrid.gridObjectList)
            {
                if (GUILayout.Button(gridObject.gridObjectType.ToString()))
                {
                    _levelGrid.SetObjectToPlace(gridObject);   
                }
            }
            EditorGUILayout.EndVertical();
        }

        void DrawGrid()
        {
            if (_levelGrid.grid == null || _levelGrid.grid.Length == 0)
                return;

            for (int y = 0; y < _levelGrid.gridHeight; y++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                for (int x = 0; x < _levelGrid.gridWidth; x++)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.button);
                    var cellText = "";

                    var cell = _levelGrid.grid[y * _levelGrid.gridWidth + x];
                    
                    if (cell == null || cell.gridObject == null)
                    {
                        style.normal.textColor = Color.white;
                    }
                    else
                    {
                        style.normal.textColor = cell.gridObject.color;
                        cellText = $"R{cell}";
                    }

                    if (GUILayout.Button(cellText, style, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        _levelGrid.GridButtonAction(x, y);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}