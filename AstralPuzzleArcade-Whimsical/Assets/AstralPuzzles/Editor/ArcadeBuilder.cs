using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AstralPuzzles.Editor {
public static class ArcadeBuilder {
    [MenuItem("Tools/Astral Puzzles/Create Standalone Arcade")]
    public static void Create() {
        Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        new GameObject("Astral Puzzle Arcade").AddComponent<PuzzleArcade>();
        var camera=new GameObject("Camera").AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.035f,.045f,.085f);
        camera.transform.position=new Vector3(0,0,-10);
        Debug.Log("Standalone arcade created. Save the scene, then press Play. Add the saved scene to your Build Profile to export a game.");
    }
}
}
