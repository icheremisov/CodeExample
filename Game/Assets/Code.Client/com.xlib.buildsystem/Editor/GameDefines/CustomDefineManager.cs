#pragma warning disable IDE0017 // Simplify object initialization

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XLib.BuildSystem.GameDefines
{
    public partial class CustomDefineManager : EditorWindow
    {
        private static Vector2 _scrollPos;
        protected List<Directive> _directives = new List<Directive>();

        Color _guiColor;
        Color _guiBackgroundColor;

        void OnEnable()
        {
            Reload();
        }

        void OnGUI()
        {
            _guiColor = GUI.color;
            _guiBackgroundColor = GUI.backgroundColor;

            var directiveLineStyle = new GUIStyle(EditorStyles.toolbar);
            directiveLineStyle.fixedHeight = 0;
            directiveLineStyle.padding = new RectOffset(8, 8, 0, 0);

            var headerStyle = new GUIStyle(EditorStyles.largeLabel);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;

            GUI.color = new Color(0.5f, 0.5f, 0.5f);

            EditorGUILayout.BeginHorizontal(directiveLineStyle, GUILayout.Height(20));
            {
                GUI.color = _guiColor;
                EditorGUILayout.LabelField("", GUILayout.Width(31));
                EditorGUILayout.LabelField("Custom Define Manager", headerStyle, GUILayout.Height(20));
            }
            EditorGUILayout.EndHorizontal();

            var directivesToRemove = new List<Directive>();

            RenderTableHeader();

            var textFieldStyle = new GUIStyle(EditorStyles.toolbarTextField);
            textFieldStyle.alignment = TextAnchor.MiddleLeft;
            textFieldStyle.fixedHeight = 0;
            textFieldStyle.padding = new RectOffset(4, 4, 4, 4);
            textFieldStyle.fontSize = 12;
            textFieldStyle.margin = new RectOffset(0, 0, 1, 1);

            var platformsStyles = new GUIStyle(directiveLineStyle);
            platformsStyles.padding = new RectOffset(4, 4, 4, 4);

            var removeButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            removeButtonStyle.normal.textColor = Color.red;
            removeButtonStyle.fixedHeight = 0;
            removeButtonStyle.margin = new RectOffset(0, 0, 1, 1);

            var toggleStyle = new GUIStyle(EditorStyles.toggle);
            toggleStyle.alignment = TextAnchor.MiddleCenter;
            toggleStyle.fixedWidth = 0;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false, GUIStyle.none,
                GUI.skin.verticalScrollbar, GUIStyle.none);
            foreach (var directive in _directives)
            {
                EditorStyles.helpBox.alignment = TextAnchor.MiddleLeft;

                GUI.color = new Color(0.65f, 0.65f, 0.65f);

                EditorGUILayout.BeginHorizontal(directiveLineStyle, GUILayout.Height(24), GUILayout.ExpandWidth(true));
                {
                    GUI.color = _guiColor;

                    if (GUILayout.Button(new GUIContent("X", "Remove this directive"), removeButtonStyle,
                            GUILayout.Width(32), GUILayout.Height(24)))
                    {
                        directivesToRemove.Add(directive);
                    }

                    GUILayout.Space(4);

                    directive._name = EditorGUILayout.TextField(directive._name, textFieldStyle, GUILayout.Width(350),
                        GUILayout.Height(24));

                    GUILayout.Space(7);

                    EditorGUILayout.BeginHorizontal(platformsStyles, GUILayout.Height(24), GUILayout.Width(150));
                    {
                        foreach (CdmBuildTargetGroup targetGroup in Enum.GetValues(typeof(CdmBuildTargetGroup)))
                        {
                            var platformButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
                            platformButtonStyle.fontStyle = FontStyle.Bold;

                            var hasFlag = directive._targets.HasFlag(targetGroup);

                            if (!hasFlag)
                            {
                                GUI.backgroundColor = new Color(1, 1, 1, 0.25f);
                                GUI.color = new Color(1, 1, 1, 0.25f);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color(0f, 1f, 0f, 1f);
                            }

                            GUIContent buttonContent = null;

                            var icon = EditorGUIUtility.IconContent("BuildSettings." + targetGroup.ToIconName());
                            if (icon != null)
                            {
                                buttonContent = new GUIContent(icon.image, targetGroup.ToString());
                            }
                            else
                            {
                                buttonContent = new GUIContent(targetGroup.ToString()[0].ToString(),
                                    targetGroup.ToString());
                            }

                            if (GUILayout.Button(buttonContent, platformButtonStyle, GUILayout.Width(24),
                                    GUILayout.Height(18)))
                            {
                                if (hasFlag)
                                {
                                    directive._targets &= ~targetGroup;
                                }
                                else
                                {
                                    directive._targets |= targetGroup;
                                }
                            }

                            GUI.backgroundColor = _guiBackgroundColor;
                            GUI.color = _guiColor;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(7);

                    if (!directive._enabled)
                    {
                        GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
                    }

                    EditorGUILayout.BeginHorizontal(platformsStyles, GUILayout.Height(24), GUILayout.Width(80));
                    {
                        GUI.backgroundColor = _guiBackgroundColor;

                        GUILayout.Space(25);
                        directive._enabled = GUILayout.Toggle(directive._enabled, new GUIContent(), toggleStyle);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
            }

            RenderNewDirectiveLine();
            EditorGUILayout.EndScrollView();

            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            EditorGUILayout.BeginHorizontal(directiveLineStyle, GUILayout.Height(24), GUILayout.ExpandWidth(true));
            {
                GUI.color = _guiColor;

                GUILayout.Label("", GUILayout.Width(31));

                if (GUILayout.Button("Apply", GUILayout.Width(350))) SaveDirectives(_directives);

                GUILayout.Space(2);

                if (GUILayout.Button("Revert", GUILayout.Width(150))) Reload();
            }
            EditorGUILayout.EndHorizontal();

            if (directivesToRemove.Any())
            {
                foreach (var directiveToRemove in directivesToRemove)
                {
                    _directives.Remove(directiveToRemove);
                }
            }
        }

        void RenderTableHeader()
        {
            var style = new GUIStyle(EditorStyles.toolbar);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedHeight = 0;

            GUI.color = new Color(0.5f, 0.5f, 0.5f);

            EditorGUILayout.BeginHorizontal(style, GUILayout.Height(20));

            GUI.color = _guiColor;

            EditorGUILayout.LabelField("", GUILayout.Width(31));

            GUILayout.Space(4);

            EditorGUILayout.LabelField("Directive", style, GUILayout.Width(248), GUILayout.Height(20));

            GUILayout.Space(4);

            EditorGUILayout.LabelField("Platforms", style, GUILayout.Width(150), GUILayout.Height(20));

            GUILayout.Space(4);

            EditorGUILayout.LabelField("Enabled", style, GUILayout.Width(80), GUILayout.Height(20));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        void RenderNewDirectiveLine()
        {
            var directiveLineStyle = new GUIStyle(EditorStyles.toolbar);
            directiveLineStyle.fixedHeight = 0;
            directiveLineStyle.padding = new RectOffset(8, 8, 0, 0);

            var addButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            addButtonStyle.fixedHeight = 0;
            addButtonStyle.margin = new RectOffset(0, 0, 1, 1);

            GUI.color = new Color(0.75f, 0.75f, 0.75f);

            EditorGUILayout.BeginHorizontal(directiveLineStyle, GUILayout.Height(24));

            GUI.color = _guiColor;

            if (GUILayout.Button(new GUIContent("+", "Add new Directive"), addButtonStyle, GUILayout.Width(32),
                    GUILayout.Height(24)))
            {
                var lastDirective = _directives.LastOrDefault();
                var newDirective = new Directive();

                if (lastDirective != null)
                {
                    newDirective._targets = lastDirective._targets;
                }

                _directives.Add(newDirective);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void Reload()
        {
            _scrollPos = Vector2.zero;
            _directives = LoadDirectives();
        }
    }
}