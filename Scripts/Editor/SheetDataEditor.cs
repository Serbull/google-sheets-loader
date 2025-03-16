using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace GoogleSheets
{
    [CustomEditor(typeof(SheetData), true)]
    public class SheetDataEditor : Editor
    {
        private SerializedProperty _sheetId;
        private SerializedProperty _pageId;
        private SerializedProperty _lastFetchTime;
        private SerializedProperty _lastFetchSuccessful;

        protected virtual void OnEnable()
        {
            _sheetId = serializedObject.FindProperty("_sheetId");
            _pageId = serializedObject.FindProperty("_pageId");
            _lastFetchTime = serializedObject.FindProperty("_lastFetchTime");
            _lastFetchSuccessful = serializedObject.FindProperty("_lastFetchSuccessful");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var sheetData = (SheetData)target;

            GUIStyle boxStyle = new(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            EditorGUILayout.BeginVertical(boxStyle);
            var textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Google Sheet Settings", textStyle);

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(_sheetId);

            if (!string.IsNullOrWhiteSpace(_sheetId.stringValue))
            {
                EditorGUILayout.PropertyField(_pageId);

                GUILayout.Space(10);
                float totalWidth = EditorGUIUtility.currentViewWidth - 45;
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Open", GUILayout.Width(totalWidth * 0.2f)))
                {
                    var url = CVSLoader.GetUrl(_sheetId.stringValue, _pageId.stringValue);
                    Application.OpenURL(url);
                }
                if (GUILayout.Button("Download table", GUILayout.Width(totalWidth * 0.8f)))
                {
                    sheetData.DownloadTable();
                }

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrWhiteSpace(_lastFetchTime.stringValue))
                {
                    GUILayout.Space(5);
                    GUIStyle style = new();
                    style.normal.textColor = _lastFetchSuccessful.boolValue ? Color.green : Color.red;
                    style.alignment = TextAnchor.MiddleCenter;
                    var time = System.DateTime.ParseExact(_lastFetchTime.stringValue, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                    System.TimeSpan ts = System.DateTime.UtcNow - time;
                    string text;
                    if (ts.TotalSeconds < 10) text = "just now";
                    else if (ts.TotalSeconds < 60) text = $"{ts.TotalSeconds:0} seconds ago";
                    else if (ts.TotalMinutes < 60) text = $"{ts.TotalMinutes:0} minutes ago";
                    else if (ts.TotalHours < 24) text = $"{ts.TotalHours:0} hours ago";
                    else text = time.Date.ToString();
                    GUILayout.Label($"Last fetch {text}", style);
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            DrawPropertiesExcluding(serializedObject, "m_Script", "_sheetId", "_pageId", "_lastFetchTime", "_lastFetchSuccessful");

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(sheetData);
            }
        }
    }
}
