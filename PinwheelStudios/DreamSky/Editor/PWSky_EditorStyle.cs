// ============================================================================
// Pinwheel DreamSky — Shared Obsidian & Warm Gold Editor UI Styles
// Pinwheel Studios | Created by Bhavishya Awasthi
// ============================================================================

using UnityEditor;
using UnityEngine;

namespace PinwheelStudios.DreamSky.Editor
{
    public static class PWSky_EditorStyle
    {
        // Pinwheel Studios Obsidian & Warm Gold Palette
        public static readonly Color ObsidianBg      = new Color(0.10f, 0.095f, 0.115f, 1f);
        public static readonly Color ObsidianCard    = new Color(0.135f, 0.13f, 0.155f, 1f);
        public static readonly Color GoldAccent      = new Color(0.78f, 0.63f, 0.35f, 1f);
        public static readonly Color GoldAccentMuted = new Color(0.78f, 0.63f, 0.35f, 0.35f);
        public static readonly Color WarmOffWhite    = new Color(0.95f, 0.92f, 0.86f, 1f);
        public static readonly Color MutedWarmGrey   = new Color(0.68f, 0.63f, 0.57f, 1f);

        private static GUIStyle _titleStyle;
        private static GUIStyle _subtitleStyle;
        private static GUIStyle _sectionTitleStyle;
        private static GUIStyle _footerLeftStyle;
        private static GUIStyle _footerRightStyle;

        private static void EnsureStyles()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                _titleStyle.normal.textColor = WarmOffWhite;
            }

            if (_subtitleStyle == null)
            {
                _subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                _subtitleStyle.normal.textColor = MutedWarmGrey;
            }

            if (_sectionTitleStyle == null)
            {
                _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft
                };
                _sectionTitleStyle.normal.textColor = WarmOffWhite;
            }

            if (_footerLeftStyle == null)
            {
                _footerLeftStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleLeft
                };
                _footerLeftStyle.normal.textColor = MutedWarmGrey;
            }

            if (_footerRightStyle == null)
            {
                _footerRightStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight
                };
                _footerRightStyle.normal.textColor = MutedWarmGrey;
            }
        }

        /// <summary>
        /// Draws the standard 46px Obsidian banner with a 2px Warm Gold bottom border.
        /// </summary>
        public static void DrawBanner(string toolTitleUppercase, string shortTagline)
        {
            EnsureStyles();

            Rect bannerRect = GUILayoutUtility.GetRect(0f, 46f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(bannerRect, ObsidianBg);

            Rect goldLineRect = new Rect(bannerRect.x, bannerRect.yMax - 2f, bannerRect.width, 2f);
            EditorGUI.DrawRect(goldLineRect, GoldAccent);

            Rect titleRect = new Rect(bannerRect.x + 12f, bannerRect.y + 6f, bannerRect.width - 24f, 18f);
            Rect subtitleRect = new Rect(bannerRect.x + 12f, bannerRect.y + 24f, bannerRect.width - 24f, 16f);

            GUI.Label(titleRect, toolTitleUppercase, _titleStyle);
            GUI.Label(subtitleRect, $"{shortTagline}  •  Pinwheel Studios", _subtitleStyle);
        }

        /// <summary>
        /// Draws the standard bottom studio footer bar for EditorWindows.
        /// </summary>
        public static void DrawFooterBar()
        {
            EnsureStyles();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                GUILayout.Label("Pinwheel DreamSky  •  Pinwheel Studios", _footerLeftStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Created by Bhavishya Awasthi", _footerRightStyle);
            }
        }

        /// <summary>
        /// Draws a styled collapsible section header for ShaderGUI and Tool Windows.
        /// </summary>
        public static bool DrawSectionHeader(string title, bool expanded)
        {
            EnsureStyles();

            Rect rect = GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ObsidianCard);

            Rect accentBar = new Rect(rect.x, rect.y, 3f, rect.height);
            EditorGUI.DrawRect(accentBar, expanded ? GoldAccent : GoldAccentMuted);

            Rect labelRect = new Rect(rect.x + 10f, rect.y, rect.width - 28f, rect.height);
            string arrow = expanded ? "▼  " : "►  ";
            GUI.Label(labelRect, arrow + title, _sectionTitleStyle);

            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && rect.Contains(evt.mousePosition))
            {
                expanded = !expanded;
                evt.Use();
            }

            return expanded;
        }

        public static void DrawGoldDivider(float thickness = 1f)
        {
            Rect r = GUILayoutUtility.GetRect(0f, thickness, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, GoldAccentMuted);
        }
    }
}