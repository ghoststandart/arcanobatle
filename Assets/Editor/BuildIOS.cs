using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildIOS
{
    public static void BuildForSimulator()
    {
        var outputPath = GetArg("-outputPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Builds/iOS");
        var bundleId = GetArg("-bundleId") ?? "com.defaultcompany.arcanobatle";

        Directory.CreateDirectory(outputPath);

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleId);
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);

        // Portrait-only for iOS.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildIOS] No enabled scenes in EditorBuildSettings.");
            EditorApplication.Exit(1);
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            targetGroup = BuildTargetGroup.iOS,
            options = BuildOptions.None,
        };

        Debug.Log($"[BuildIOS] Building to {outputPath} with bundleId {bundleId}");
        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[BuildIOS] Failed: {report.summary.result} ({report.summary.totalErrors} errors)");
            EditorApplication.Exit(1);
            return;
        }

        // When SimulatorSDK is selected on Apple Silicon, Unity sometimes drops the x86_64-only
        // variants of UnityRuntime.framework and baselib.a into the exported Xcode project, which
        // fails to link for arm64. Replace them with the universal (sim-x64arm64) variants so the
        // build works on both Intel and Apple Silicon simulators.
        SwapSimulatorFrameworksToUniversal(outputPath);

        Debug.Log($"[BuildIOS] Success: {report.summary.totalSize} bytes in {report.summary.totalTime}");
        EditorApplication.Exit(0);
    }

    private static void SwapSimulatorFrameworksToUniversal(string outputPath)
    {
        string editorRoot = Path.GetDirectoryName(Path.GetDirectoryName(EditorApplication.applicationPath));
        string trampoline = Path.Combine(editorRoot, "PlaybackEngines/iOSSupport/Trampoline");
        string universalRuntime = Path.Combine(trampoline, "Frameworks/UnityRuntime-sim-x64arm64/UnityRuntime.framework");
        string universalBaselib = Path.Combine(trampoline, "Libraries/baselib-sim-x64arm64.a");

        if (Directory.Exists(universalRuntime))
        {
            string dest = Path.Combine(outputPath, "Frameworks/UnityRuntime.framework");
            if (Directory.Exists(dest))
            {
                Directory.Delete(dest, true);
            }
            CopyDirectory(universalRuntime, dest);
            Debug.Log($"[BuildIOS] Replaced UnityRuntime.framework with universal simulator variant.");
        }
        else
        {
            Debug.LogWarning($"[BuildIOS] Universal UnityRuntime not found at {universalRuntime} — simulator build may fail to link on Apple Silicon.");
        }

        if (File.Exists(universalBaselib))
        {
            string dest = Path.Combine(outputPath, "Libraries/baselib.a");
            File.Copy(universalBaselib, dest, true);
            Debug.Log($"[BuildIOS] Replaced baselib.a with universal simulator variant.");
        }
        else
        {
            Debug.LogWarning($"[BuildIOS] Universal baselib not found at {universalBaselib} — simulator build may fail to link on Apple Silicon.");
        }
    }

    private static void CopyDirectory(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var file in Directory.GetFiles(src))
        {
            File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), true);
        }
        foreach (var dir in Directory.GetDirectories(src))
        {
            CopyDirectory(dir, Path.Combine(dst, Path.GetFileName(dir)));
        }
    }

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
