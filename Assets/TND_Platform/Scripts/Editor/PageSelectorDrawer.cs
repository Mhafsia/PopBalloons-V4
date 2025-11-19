#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using TNDPlatform.Attributes;
using TNDPlatform.Managers;

namespace TNDPlatform.Editor
{
    /// <summary>
    /// PropertyDrawer pour afficher un dropdown des pages disponibles dans NavigationManager.Pages
    /// </summary>
    [CustomPropertyDrawer(typeof(PageSelectorAttribute))]
    public class PageSelectorDrawer : PropertyDrawer
    {
        private static string[] cachedPageNames;
        private static string[] cachedDisplayNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [PageSelector] with string fields only.");
                return;
            }

            PageSelectorAttribute attr = (PageSelectorAttribute)attribute;

            // Récupérer toutes les pages disponibles (cache pour performance)
            if (cachedPageNames == null)
            {
                LoadAvailablePages(attr.includeEmpty);
            }

            EditorGUI.BeginProperty(position, label, property);

            // Trouver l'index actuel
            int currentIndex = System.Array.IndexOf(cachedPageNames, property.stringValue);
            if (currentIndex < 0) currentIndex = 0; // Défaut: première option (vide ou première page)

            // Afficher le dropdown
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, cachedDisplayNames);

            // Mettre à jour la valeur si changée
            if (newIndex != currentIndex)
            {
                property.stringValue = cachedPageNames[newIndex];
            }

            EditorGUI.EndProperty();
        }

        private static void LoadAvailablePages(bool includeEmpty)
        {
            List<string> pageNames = new List<string>();
            List<string> displayNames = new List<string>();

            if (includeEmpty)
            {
                pageNames.Add("");
                displayNames.Add("(Aucune navigation)");
            }

            // Utiliser la réflexion pour lire toutes les constantes de NavigationManager.Pages
            System.Type pagesType = typeof(NavigationManager.Pages);
            FieldInfo[] fields = pagesType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    string pageName = (string)field.GetValue(null);
                    string displayName = FormatDisplayName(field.Name);

                    pageNames.Add(pageName);
                    displayNames.Add(displayName);
                }
            }

            cachedPageNames = pageNames.ToArray();
            cachedDisplayNames = displayNames.ToArray();
        }

        /// <summary>
        /// Formater le nom d'affichage (ex: FamilyDashboard → 👨‍👩‍👧 Famille - Tableau de bord)
        /// </summary>
        private static string FormatDisplayName(string fieldName)
        {
            // Ajouter des icônes et regrouper par profil
            if (fieldName == "ProfileSelector") return "🏠 Sélecteur de profil";
            if (fieldName == "Settings") return "⚙️ Paramètres";

            // Famille
            if (fieldName.StartsWith("Family"))
            {
                string suffix = fieldName.Substring(6); // Retirer "Family"
                return $"👨‍👩‍👧 Famille - {AddSpacesToCamelCase(suffix)}";
            }

            // Clinicien
            if (fieldName.StartsWith("Clinician"))
            {
                string suffix = fieldName.Substring(9); // Retirer "Clinician"
                return $"🏥 Clinicien - {AddSpacesToCamelCase(suffix)}";
            }

            // Enseignant
            if (fieldName.StartsWith("Teacher"))
            {
                string suffix = fieldName.Substring(7); // Retirer "Teacher"
                return $"🎓 Enseignant - {AddSpacesToCamelCase(suffix)}";
            }

            // Fallback
            return AddSpacesToCamelCase(fieldName);
        }

        /// <summary>
        /// Ajouter des espaces dans les noms CamelCase (ex: "DashBoard" → "Dash Board")
        /// </summary>
        private static string AddSpacesToCamelCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    result.Append(' ');
                }
                result.Append(text[i]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Réinitialiser le cache si les pages changent (utile en dev)
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            cachedPageNames = null;
            cachedDisplayNames = null;
        }
    }
}
#endif
