#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG
using UnityEngine;

namespace LiangTools.Debugging
{
    /// <summary>
    /// Runtime settings for the overlay, stored as an asset under a <c>Resources</c>
    /// folder so the value reaches a build on a device.
    ///
    /// A scripting define cannot carry a <see cref="KeyCode"/>, and editor
    /// <c>PlayerPrefs</c> and anything in <c>ProjectSettings/</c> are not readable from
    /// a player build. The asset is only created once you change a setting, so a project
    /// that leaves the defaults alone gets no extra file.
    /// </summary>
    public sealed class LiangDebugSettings : ScriptableObject
    {
        public const string ResourceName = "LiangToolsDebugSettings";

        public const KeyCode DefaultOpenKey = KeyCode.M;

        [SerializeField]
        [Tooltip("Key that opens and closes the overlay. None disables it.")]
        private KeyCode openKey = DefaultOpenKey;

        public KeyCode OpenKey
        {
            get => openKey;
            set => openKey = value;
        }

        public static LiangDebugSettings Load()
        {
            return Resources.Load<LiangDebugSettings>(ResourceName);
        }
    }
}
#endif
