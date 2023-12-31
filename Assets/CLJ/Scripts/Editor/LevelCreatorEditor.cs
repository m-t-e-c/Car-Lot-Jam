﻿using System;
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
        private int _selectedColorIndex = -1;
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
            if (
                ReferenceEquals(grid, null) ||
                ReferenceEquals(grid.cells, null) ||
                !grid.width.Equals(_levelCreator.gridWidth) ||
                !grid.height.Equals(_levelCreator.gridHeight)
            )
            {
                EditorGUILayout.HelpBox("Please regenerate the Grid!", MessageType.Error);
                return;
            }

            if (grid.cells.Length.Equals(0))
            {
                return;
            }

            DrawGridObjectListButtons();
            DrawColorButtons();
            DrawDirectionButtons();
            DrawGrid();
            DrawSaveLoadButtons();

            serializedObject.ApplyModifiedProperties();
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
                ResetButtons();
                _levelCreator.GenerateGrid();
            }

            if (GUILayout.Button("Reset"))
            {
                ResetButtons();
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
                    GUI.backgroundColor = Color.gray;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(gridObject.gridObjectType.ToString()))
                {
                    _levelCreator.SetObjectToPlace(gridObject);
                    _selectedGridObjectIndex = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }


        private void DrawColorButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Object Color:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            for (int i = -1; i < Enum.GetValues(typeof(CellColor)).Length; i++)
            {
                if (i.Equals(0))
                {
                    continue;
                }

                if (i.Equals(-1))
                {
                    if (_selectedColorIndex.Equals(-1))
                    {
                        GUI.backgroundColor = Color.gray;
                    }

                    if (GUILayout.Button("None"))
                    {
                        _levelCreator.SetObjectColor(CellColor.None);
                        _selectedColorIndex = -1;
                    }

                    continue;
                }

                CellColor color = (CellColor)i;
                if (_selectedColorIndex.Equals((int)color))
                {
                    GUI.backgroundColor = Color.gray + GetColorByColorType(color);
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(color.ToString()))
                {
                    _levelCreator.SetObjectColor(color);
                    _selectedColorIndex = (int)color;
                }
            }

            EditorGUILayout.EndHorizontal();
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

                if (GUILayout.Button(((CellDirection)i).ToString()))
                {
                    _levelCreator.SetObjectDirection((CellDirection)i);
                    _selectedDirectionIndex = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 8;
            var grid = _levelCreator.GetGrid();
            if (ReferenceEquals(grid, null) || grid.cells.Length.Equals(0))
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
                        GUI.backgroundColor = GetColorByColorType(cell.cellColor);
                        cellText = $"{cell.gridObject.gridObjectType} {cell.cellDirection}";
                    }

                    if (GUILayout.Button(cellText, style, GUILayout.Width(80), GUILayout.Height(80)))
                    {
                        _levelCreator.GridButtonAction(x, y);
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = Color.white;
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

        private void ResetButtons()
        {
            _levelCreator.SetObjectColor(CellColor.None);
            _levelCreator.SetObjectDirection(CellDirection.Left);
            _levelCreator.SetObjectToPlace(null);
            _selectedColorIndex = -1;
            _selectedDirectionIndex = 0;
            _selectedGridObjectIndex = -1;
        }

        private Color32 GetColorByColorType(CellColor color)
        {
            switch (color)
            {
                case CellColor.None:
                    return Color.white;
                case CellColor.Red:
                    return new Color32(255, 0, 0, 255);
                case CellColor.Green:
                    return new Color32(0, 255, 0, 255);
                case CellColor.Blue:
                    return new Color32(0, 0, 255, 255);
                case CellColor.Yellow:
                    return new Color32(255, 255, 0, 255);
                case CellColor.Purple:
                    return new Color32(128, 0, 128, 255);
                case CellColor.Orange:
                    return new Color32(255, 165, 0, 255);
                case CellColor.Black:
                    return new Color32(50, 50, 50, 255);
                case CellColor.Pink:
                    return new Color32(255, 125, 230, 255);
                default:
                    return Color.gray;
            }
        }
    }
}