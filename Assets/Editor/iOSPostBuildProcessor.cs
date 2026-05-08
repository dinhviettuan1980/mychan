using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Text.RegularExpressions;

public class iOSPostBuildProcessor
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string pbxPath = PBXProject.GetPBXProjectPath(buildPath);

        // ── 1. Remove stale ARM thumb flags ───────────────────────────────────
        string content = File.ReadAllText(pbxPath);
        content = Regex.Replace(content, @"\s*GCC_THUMB_SUPPORT\s*=\s*\w+;", "");
        content = Regex.Replace(content, @"\s*""-mno-thumb"",?", "");
        File.WriteAllText(pbxPath, content);

        // ── 2. Xcode build settings ────────────────────────────────────────────
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(pbxPath);

        string mainTarget      = proj.GetUnityMainTargetGuid();
        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

        proj.SetBuildProperty(mainTarget,      "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(frameworkTarget, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(mainTarget,      "DEAD_CODE_STRIPPING", "YES");

        proj.WriteToFile(pbxPath);

        // ── 3. Privacy manifest ────────────────────────────────────────────────
        AddPrivacyManifest(buildPath, pbxPath);

        // ── 4. Info.plist tweaks ───────────────────────────────────────────────
        PatchInfoPlist(buildPath);

        Debug.Log("[PostBuild] iOS post-build complete (Unity 2022).");
    }

    static void PatchInfoPlist(string buildPath)
    {
        string plistPath = Path.Combine(buildPath, "Info.plist");
        if (!File.Exists(plistPath)) return;

        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetBoolean("UIRequiresFullScreen", true);
        plist.WriteToFile(plistPath);
    }

    static void AddPrivacyManifest(string buildPath, string pbxPath)
    {
        string src = Path.Combine(Application.dataPath, "Plugins/iOS/PrivacyInfo.xcprivacy");
        if (!File.Exists(src))
        {
            Debug.LogWarning("[PostBuild] PrivacyInfo.xcprivacy not found at Assets/Plugins/iOS/.");
            return;
        }

        string destDir  = Path.Combine(buildPath, "UnityFramework");
        string destFile = Path.Combine(destDir, "PrivacyInfo.xcprivacy");
        Directory.CreateDirectory(destDir);
        File.Copy(src, destFile, true);

        PBXProject proj = new PBXProject();
        proj.ReadFromFile(pbxPath);

        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();
        string fileGuid = proj.AddFile(
            "UnityFramework/PrivacyInfo.xcprivacy",
            "UnityFramework/PrivacyInfo.xcprivacy",
            PBXSourceTree.Source);
        proj.AddFileToBuild(frameworkTarget, fileGuid);

        proj.WriteToFile(pbxPath);
        Debug.Log("[PostBuild] Added PrivacyInfo.xcprivacy to UnityFramework.");
    }
}
