#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public static class IOSAudioPostProcess
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        // Tìm file UnityAppController.mm
        string filePath = Path.Combine(pathToBuiltProject, "Classes/UnityAppController.mm");
        if (!File.Exists(filePath))
            filePath = Path.Combine(pathToBuiltProject, "MainApp/main.mm"); // fallback nếu Unity bản mới

        if (!File.Exists(filePath))
        {
            UnityEngine.Debug.LogWarning("Không tìm thấy UnityAppController.mm hoặc main.mm");
            return;
        }

        string fileText = File.ReadAllText(filePath);

        // Thêm import nếu chưa có
        if (!fileText.Contains("#import <AVFoundation/AVFoundation.h>"))
        {
            fileText = fileText.Replace("#import <UnityAppController.h>",
                "#import <UnityAppController.h>\n#import <AVFoundation/AVFoundation.h>");
        }

        // Thêm code vào didFinishLaunchingWithOptions
        string marker = "UnityInitRuntime(argc, argv);"; // Dòng đặc trưng trong file này
        string injectCode = @"
    AVAudioSession *session = [AVAudioSession sharedInstance];
    [session setCategory:AVAudioSessionCategoryPlayback
             withOptions:AVAudioSessionCategoryOptionMixWithOthers
                   error:nil];
    [session setActive:YES error:nil];
";
        if (!fileText.Contains("AVAudioSessionCategoryPlayback"))
        {
            fileText = fileText.Replace(marker, marker + injectCode);
            File.WriteAllText(filePath, fileText);
            UnityEngine.Debug.Log("✅ Đã chèn AVAudioSessionCategoryPlayback vào " + filePath);
        }
        else
        {
            UnityEngine.Debug.Log("⚙️ Code AVAudioSession đã tồn tại, bỏ qua.");
        }
    }
}
#endif
