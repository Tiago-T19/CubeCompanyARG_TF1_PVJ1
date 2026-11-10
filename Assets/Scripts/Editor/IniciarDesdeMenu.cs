
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class IniciarDesdeMenu
{
    static IniciarDesdeMenu()
    {
        EditorApplication.delayCall += Configurar;
    }

    static void Configurar()
    {
        string[] guids = AssetDatabase.FindAssets("MainMenu t:Scene");
        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (ruta.EndsWith("/MainMenu.unity"))
            {
                EditorSceneManager.playModeStartScene =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(ruta);
                return;
            }
        }
        UnityEngine.Debug.LogWarning("No se encontró la escena MainMenu.unity");
    }
}
#endif