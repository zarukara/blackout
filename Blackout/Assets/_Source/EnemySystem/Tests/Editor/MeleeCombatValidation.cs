#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EnemySystem.Tests.Editor
{
    [InitializeOnLoad]
    public static class MeleeCombatValidation
    {
        private const string RunningKey = "BlackHost.MeleeValidation";
        static MeleeCombatValidation() => EditorApplication.playModeStateChanged += OnPlayMode;

        [MenuItem("BlackHost/Validation/Run Melee Combat Smoke Tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(RunningKey, true);
            EditorApplication.isPlaying = true;
        }

        // Explicit utility, never runs on import. Keeps the existing controller/clips/GUIDs.
        [MenuItem("BlackHost/Validation/Apply Melee Attack Timing")]
        public static void ApplyAnimationTiming()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/_Presentation/Animations/Enemies/MeleePolice/PoliceMelee Animator.controller");
            foreach (var transition in controller.layers[0].stateMachine.anyStateTransitions)
                if (transition.destinationState != null && transition.destinationState.name == "Attack")
                {
                    transition.duration = 0.05f;
                    transition.hasFixedDuration = true;
                    EditorUtility.SetDirty(transition);
                }
            foreach (var child in controller.layers[0].stateMachine.states)
                if (child.state.name == "Attack" && child.state.motion is AnimationClip clip)
                {
                    // Default gameplay cycle is .35 + .15 + .5 = 1 second.
                    child.state.speed = clip.length;
                    EditorUtility.SetDirty(child.state);
                    Debug.Log("Melee Attack clip length=" + clip.length + "; state speed=" + child.state.speed);
                }
            AssetDatabase.SaveAssets();
        }

        public static void PrepareAndRun()
        {
            ApplyAnimationTiming();
            Run();
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(RunningKey, false)) return;
            MeleeCombatSmokeRunner.Completed = (success, report) =>
            {
                SessionState.SetBool(RunningKey, false);
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/MeleeCombatValidation.txt", (success ? "PASS\n" : "FAIL\n") + report);
                if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
                else
                {
                    Debug.Log(report);
                    EditorApplication.isPlaying = false;
                }
            };
            new GameObject("Melee combat validation runner").AddComponent<MeleeCombatSmokeRunner>();
        }
    }
}
#endif
