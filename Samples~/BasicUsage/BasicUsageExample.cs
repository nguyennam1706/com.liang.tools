using LiangTools;
using UnityEngine;

public class BasicUsageExample : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"{LiangToolsInfo.DisplayName} ({LiangToolsInfo.PackageName}) is available.");
    }
}
