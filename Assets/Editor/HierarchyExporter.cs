// v1.0 - Export de la hiérarchie de la scène active vers un fichier texte
// À placer dans un dossier "Editor"

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;

public static class HierarchyExporter
{
    [MenuItem("Tools/Export Hierarchy to Text")]
    public static void ExportHierarchy()
    {
        // Scène active
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Scene: " + scene.name);

        // Parcourt toutes les racines
        foreach (GameObject root in roots)
        {
            DumpTransform(root.transform, 0, sb);
        }

        // Copie dans le presse-papiers pour collage direct dans un fichier texte ou un doc
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("Hierarchy copied to clipboard.");

        // Propose aussi d'enregistrer dans un fichier .txt
        string path = EditorUtility.SaveFilePanel(
            "Save hierarchy as text",
            "",
            scene.name + "_hierarchy.txt",
            "txt"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            Debug.Log("Hierarchy exported to: " + path);
        }
    }

    private static void DumpTransform(Transform t, int depth, StringBuilder sb)
    {
        // Indentation avec 2 espaces par niveau
        sb.AppendLine(new string(' ', depth * 2) + "- " + t.name);

        for (int i = 0; i < t.childCount; i++)
        {
            DumpTransform(t.GetChild(i), depth + 1, sb);
        }
    }
}
