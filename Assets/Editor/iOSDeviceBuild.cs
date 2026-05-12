using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

public class iOSDeviceBuild
{
    [MenuItem("Build/Build iOS for Device")]
    public static void Build()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = "V88TR24Z7K";

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes            = scenes,
            locationPathName  = "IOS",
            target            = BuildTarget.iOS,
            options           = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log("iOS build succeeded: " + report.summary.outputPath);
        else
            Debug.LogError("iOS build FAILED");
    }
}
