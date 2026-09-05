using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace LiangTools.Editor
{
    public class LiangToolsWindow : EditorWindow
    {
        private string _version;

        [MenuItem("Tools/Liang Tools/About")]
        private static void Open()
        {
            var window = GetWindow<LiangToolsWindow>(true, LiangToolsInfo.DisplayName);
            window.minSize = new Vector2(320f, 120f);
        }

        private void OnEnable()
        {
            var info = PackageInfo.FindForAssembly(typeof(LiangToolsWindow).Assembly);
            _version = info != null ? info.version : "unknown";
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(LiangToolsInfo.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Package", LiangToolsInfo.PackageName);
            EditorGUILayout.LabelField("Version", _version);
        }
    }
}
