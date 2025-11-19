using UnityEngine;

namespace TNDPlatform.Attributes
{
    /// <summary>
    /// Attribut pour afficher un dropdown de sélection de page au lieu d'un champ texte.
    /// Utilisé avec NavigationManager.Pages.
    /// </summary>
    public class PageSelectorAttribute : PropertyAttribute
    {
        public bool includeEmpty;

        public PageSelectorAttribute(bool includeEmpty = true)
        {
            this.includeEmpty = includeEmpty;
        }
    }
}
