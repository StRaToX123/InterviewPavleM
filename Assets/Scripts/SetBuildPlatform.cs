
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public class SetBuildPlatform
{
    // Set your desired default platform here, such as Android or iOS
    private const BuildTarget defaultTarget = BuildTarget.Android;

    static SetBuildPlatform()
    {
        // Check if the current platform matches the desired default
        if (EditorUserBuildSettings.activeBuildTarget != defaultTarget)
        {
            Debug.Log($"Switching build platform to {defaultTarget}");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, defaultTarget);
        }
    }
}


