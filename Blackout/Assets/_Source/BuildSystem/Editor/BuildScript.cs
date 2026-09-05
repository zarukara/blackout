using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildSystem
{
    public static class BuildScript
    {
        private const string WindowsBuildPath = "Builds/Windows/BlackHost.exe";
        private const string AndroidBuildPath = "Builds/Android/BlackHost.apk";

        [MenuItem("Build/Build Windows")]
        public static void BuildWindows()
        {
            BuildProject(
                WindowsBuildPath,
                BuildTarget.StandaloneWindows64
            );
        }

        [MenuItem("Build/Build Android")]
        public static void BuildAndroid()
        {
            EditorUserBuildSettings.buildAppBundle = false;

            BuildProject(
                AndroidBuildPath,
                BuildTarget.Android
            );
        }

        [MenuItem("Build/Build All")]
        public static void BuildAll()
        {
            Debug.Log("Build All started.");

            bool windowsBuildSucceeded = BuildProject(
                WindowsBuildPath,
                BuildTarget.StandaloneWindows64
            );

            if (!windowsBuildSucceeded)
            {
                Debug.LogError("Windows build failed. Build All stopped.");
                return;
            }

            EditorUserBuildSettings.buildAppBundle = false;

            bool androidBuildSucceeded = BuildProject(
                AndroidBuildPath,
                BuildTarget.Android
            );

            if (!androidBuildSucceeded)
            {
                Debug.LogError("Android build failed. Build All stopped.");
                return;
            }

            Debug.Log("Build All completed successfully.");
        }

        private static bool BuildProject(string buildPath, BuildTarget target)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("Build failed: no enabled scenes found.");
                return false;
            }

            string directory = Path.GetDirectoryName(buildPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = target,
                options = BuildOptions.None
            };

            Debug.Log($"{target} build started.");

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"{target} build succeeded: {buildPath}");
                return true;
            }

            Debug.LogError(
                $"{target} build failed. Errors: {report.summary.totalErrors}"
            );

            return false;
        }
    }
}