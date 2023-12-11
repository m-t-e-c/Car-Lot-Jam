using CLJ.Runtime.Level;
using UnityEditor;
using UnityEngine;

namespace CLJ
{
    [CustomEditor(typeof(LevelCreator))]
    public class LevelCreatorEditor : Editor
    {
        private LevelCreator _levelCreator;
        private int _levelIndex;
        private int _selectedGridObjectIndex = -1;
        private int _selectedDirectionIndex;

        private void OnEnable()
        {
            _levelCreator = (LevelCreator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            if (ReferenceEquals(_levelCreator.gridObjectsGroup, null))
            {
                EditorGUILayout.HelpBox("Please assign a Grid Objects Group to Level Grid", MessageType.Warning);
                return;
            }

            DrawGridProperties();

            var grid = _levelCreator.GetGrid();
            if (ReferenceEquals(grid, null) || !grid.Cells.Length.Equals(grid.Width * grid.Height))
            {
                EditorGUILayout.HelpBox("Please regenerate the Grid!", MessageType.Error);
                return;
            }

            if (grid.Cells.Length.Equals(0))
            {
                return;
            }

            DrawGridObjectListButtons();
            DrawDirectionButtons();
            DrawGrid();
            DrawSaveLoadButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDirectionButtons()
        {
            if (ReferenceEquals(_levelCreator.gridObjectsGroup, null))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Object Direction:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            for (var i = 0; i < 4; i++)
            {
                if (_selectedDirectionIndex.Equals(i))
                {
                    GUI.backgroundColor = Color.gray;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(((GridObjectDirection)i).ToString()))
                {
                    _levelCreator.SetObjectDirection((GridObjectDirection)i);
                    _selectedDirectionIndex = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGridProperties()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Width");
            _levelCreator.gridWidth = EditorGUILayout.IntField(_levelCreator.gridWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Height");
            _levelCreator.gridHeight = EditorGUILayout.IntField(_levelCreator.gridHeight);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Grid"))
            {
                _levelCreator.GenerateGrid();
            }

            if (GUILayout.Button("Reset"))
            {
                _levelCreator.ResetGrid();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGridObjectListButtons()
        {
            if (ReferenceEquals(_levelCreator.gridObjectsGroup, null))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Object To Place:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            for (var i = -1; i < _levelCreator.gridObjectsGroup.GridObjects.Count; i++)
            {
                if (i.Equals(-1))
                {
                    if (_selectedGridObjectIndex.Equals(-1))
                    {
                        GUI.backgroundColor = Color.gray;
                    }

                    if (GUILayout.Button("None"))
                    {
                        _levelCreator.SetObjectToPlace(null);
                        _selectedGridObjectIndex = -1;
                    }

                    continue;
                }

                var gridObject = _levelCreator.gridObjectsGroup.GridObjects[i];

                if (_selectedGridObjectIndex.Equals(i))
                {
                    GUI.backgroundColor = Color.black + GetColorByColorType(gridObject.gridObjectColor);
                }
                else
                {
                    GUI.backgroundColor = Color.white + GetColorByColorType(gridObject.gridObjectColor);
                }

                if (GUILayout.Button(gridObject.gridObjectType.ToString()))
                {
                    _levelCreator.SetObjectToPlace(gridObject);
                    _selectedGridObjectIndex = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            var grid = _levelCreator.GetGrid();
            if (ReferenceEquals(grid, null) || grid.Cells.Length.Equals(0))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

            for (int y = 0; y < _levelCreator.gridHeight; y++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int x = 0; x < _levelCreator.gridWidth; x++)
                {
                    var cellText = x + "x" + y;
                    var cell = _levelCreator.GetCell(x, y);

                    if (ReferenceEquals(cell, null) || ReferenceEquals(cell.gridObject, null))
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
                        _levelCreator.GridButtonAction(x, y);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSaveLoadButtons()
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
                _levelCreator.SaveGrid(_levelIndex);
            }

            if (GUILayout.Button("Load"))
            {
                _levelCreator.LoadGrid(_levelIndex);
            }

            EditorGUILayout.EndHorizontal();
        }

        private Color32 GetColorByColorType(GridObjectColor color)
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