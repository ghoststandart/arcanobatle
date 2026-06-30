using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Makes pressing Play in the editor always start from the MainMenu scene,
/// regardless of which scene is currently open. (Editor-only; builds start from
/// the first scene in Build Settings.)
/// </summary>
[InitializeOnLoad]
public static class PlayFromMenu
{
    static PlayFromMenu()
    {
        var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity");
        if (menu != null)
        {
            EditorSceneManager.playModeStartScene = menu;
        }
    }
}
