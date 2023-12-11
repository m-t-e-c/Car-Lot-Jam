using CLJ.Scripts.Runtime.Level;
using UnityEditor;
using UnityEngine;

namespace CLJ.Scripts.Editor
{
    [CustomEditor(typeof(LevelGrid))]
    public class LevelGridEditor : UnityEditor.Editor
    {
        private LevelGrid _levelGrid;
        private int _levelIndex;
        private int _selectedGridObjectIndex;
        private int _selectedDirectionIndex;

        private void OnEnable()
        {
            _levelGrid = (LevelGrid)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            if (ReferenceEquals(_levelGrid.gridObjectsGroup, null))
            {
                EditorGUILayout.HelpBox("Please assign a Grid Objects Group to Level Grid", MessageType.Warning);
                return;
            }
            
            DrawGridProperties();
            
            if (_levelGrid.grid == null || _levelGrid.grid.Length != (_levelGrid.gridWidth * _levelGrid.gridHeight))
            {
                EditorGUILayout.HelpBox("Please regenerate the Grid!", MessageType.Error);
                return;
            }

            if (_levelGrid.grid.Length.Equals(0))
            {
                return;
            }
            DrawGridObjectListButtons();
            DrawDirectionButtons();
            DrawGrid();
            DrawSaveLoadButtons();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawDirectionButtons()
        {
            if (ReferenceEquals(_levelGrid.gridObjectsGroup, null))
            {
                return;
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Object Direction:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            for (var i = 0; i < 4; i++)
            {
                if (_selectedDirectionIndex == i)
                {
                    GUI.backgroundColor = Color.gray;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(((GridObjectDirection)i).ToString()))
                {
                    _levelGrid.SetObjectDirection((GridObjectDirection)i);
                    _selectedDirectionIndex = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawGridProperties()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Width");
            _levelGrid.gridWidth = EditorGUILayout.IntField(_levelGrid.gridWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Height");
            _levelGrid.gridHeight = EditorGUILayout.IntField(_levelGrid.gridHeight);
            EditorGUILayout.EndHorizontal();


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
            if (ReferenceEquals(_levelGrid.gridObjectsGroup, null))
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Object To Place:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            for (var i = -1; i < _levelGrid.gridObjectsGroup.GridObjects.Count; i++)
            {
                if (i == -1)
                {
                    if (_selectedGridObjectIndex == -1)
                    {
                        GUI.backgroundColor = Color.gray;
                    }

                    if (GUILayout.Button("None"))
                    {
                        _levelGrid.SetObjectToPlace(null);
                        _selectedGridObjectIndex = -1;
                    }

                    continue;
                }

                var gridObject = _levelGrid.gridObjectsGroup.GridObjects[i];

                if (_selectedGridObjectIndex == i)
                {
                    GUI.backgroundColor = Color.black + GetColorByColorType(gridObject.gridObjectColor);
                }
                else
                {
                    GUI.backgroundColor = Color.white + GetColorByColorType(gridObject.gridObjectColor);
                }

                if (GUILayout.Button(gridObject.gridObjectType.ToString()))
                {
                    _levelGrid.SetObjectToPlace(gridObject);
                    _selectedGridObjectIndex = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawGrid()
        {
            if (_levelGrid.grid == null || _levelGrid.grid.Length == 0)
                return;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

            for (int y = 0; y < _levelGrid.gridHeight; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int x = 0; x < _levelGrid.gridWidth; x++)
                {
                    var cellText = x + "x" + y;
                    var cell = _levelGrid.GetCell(x,y);

                    if (cell == null || cell.gridObject == null)
                    {
                        GUI.backgroundColor = Color.gray;
                    }
                    else
                    {
                        GUI.backgroundColor = GetColorByColorType(cell.gridObject.gridObjectColor);
                        cellText = $"{cell.gridObject.gridObjectType} {cell.objectDirection}";
                    }

                    if (GUILayout.Button(cellText, GUILayout.Width(60), GUILayout.Height(60)))
                    {
                        _levelGrid.GridButtonAction(x, y);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawSaveLoadButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Don't forget the save grid!", MessageType.Warning);
            EditorGUILayout.LabelField("Save/Load", EditorStyles.boldLabel);

            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Index");
            _levelIndex = EditorGUILayout.IntField(_levelIndex);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save"))
            {
                _levelGrid.SaveGrid(_levelIndex);
            }

            if (GUILayout.Button("Load"))
            {
                _levelGrid.LoadGrid(_levelIndex);
            }

            EditorGUILayout.EndHorizontal();
        }

        Color32 GetColorByColorType(GridObjectColor color)
        {
            switch (color)
            {
                case GridObjectColor.Red:
                    return Color.red;
                case GridObjectColor.Green:
                    return Color.green;
                case GridObjectColor.Blue:
                    return Color.blue;
                case GridObjectColor.Yellow:
                    return Color.yellow;
                case GridObjectColor.Purple:
                    return new Color32(128, 0, 128, 255);
                case GridObjectColor.Orange:
                    return new Color32(255, 165, 0, 255);
                default:
                    return Color.white;
            }
        }
    }
}