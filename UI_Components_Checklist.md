# Configuration des Boutons et Éléments UI

Ce document détaille la configuration requise pour chaque type de bouton et élément d'interface dans le projet PopBalloons. Il sert de référence pour s'assurer que les interactions (Touch et AirTap) fonctionnent correctement et que la logique de navigation est cohérente.

---

## 1. Boutons de Mode (Menu Principal)
Ces boutons permettent de choisir entre les modes "Cognitif", "Moteur" et "Jeu Libre".

**GameObjects Cibles :** `Btn_Mode_Cognitive`, `Btn_Mode_Motor`, `Btn_Mode_FreePlay`

### Composants Requis
| Composant | Configuration | Rôle |
| :--- | :--- | :--- |
| **RectTransform** | Position et taille visuelle. | Définit la zone d'affichage. |
| **BoxCollider** | `Is Trigger`: **Coché**<br>`Size`: Doit correspondre au RectTransform (Width/Height) + Profondeur Z (ex: 0.1). | Zone de détection physique pour Touch et AirTap. |
| **NearInteractionTouchable** | `Events to Receive`: **Touch**<br>`Bounds`: Doit correspondre au BoxCollider.<br>`Local Forward`: (0, 0, 1) | Permet l'interaction tactile (poke). |
| **ModeButton** (Script) | `Game Type`: **COGNITIVE**, **MOBILITY** ou **FREEPLAY** (selon le bouton).<br>`Enable Touch Interaction`: **Coché**. | Gère le clic et met à jour le `GameModeSelector`. |
| **CanvasGroup** | `Alpha`: 1 (visible)<br>`Interactable`: Coché<br>`Blocks Raycasts`: Coché | Gère la visibilité globale du bouton. |

### Fonctionnement
1.  L'utilisateur clique (Touch ou AirTap).
2.  `ModeButton.SelectMode()` est appelé.
3.  Le script met à jour `GameModeSelector.Instance.CurrentGameType` avec le type choisi.
4.  Si c'est **FREEPLAY**, il lance le jeu directement.
5.  Si c'est **COGNITIVE** ou **MOBILITY**, il change le `MainPanel` pour afficher le sous-menu correspondant (ex: `Panel_Cognitive`).

---

## 2. Boutons de Niveau (Sous-Menus)
Ces boutons ("1", "2", "3"...) se trouvent dans les panels "Cognitif" ou "Moteur". Ils servent à lancer un niveau spécifique pour le mode *déjà sélectionné*.

**GameObjects Cibles :** `Btn_Level_1`, `Btn_Level_2`, etc. (dans `Panel_Cognitive` et `Panel_Mobility`)

### Problème Courant : "Lance toujours FreePlay"
Si ces boutons lancent FreePlay au lieu du mode choisi, c'est que le `GameModeSelector` a perdu l'information ou que le bouton écrase la valeur.
**Solution :** Ces boutons NE DOIVENT PAS avoir de variable "Game Type" locale qui écraserait la sélection globale. Ils doivent lire le mode actuel depuis le `GameModeSelector`.

### Composants Requis
| Composant | Configuration | Rôle |
| :--- | :--- | :--- |
| **RectTransform** | Position et taille visuelle. | Zone d'affichage. |
| **BoxCollider** | `Is Trigger`: **Coché**<br>`Size`: Identique au RectTransform + Profondeur Z. | Zone de détection. |
| **NearInteractionTouchable** | `Events to Receive`: **Touch**<br>`Bounds`: Identique au Collider. | Interaction tactile. |
| **LoadLevelButton** (Script) | `Enable Touch Interaction`: **Coché**. | Lance le niveau. **Ne doit pas définir le GameType localement**, il doit utiliser celui défini par le `ModeButton` précédent. |
| **LevelSelectionButton** (Optionnel) | Si présent, assurez-vous qu'il ne rentre pas en conflit avec `LoadLevelButton`. Idéalement, `LoadLevelButton` suffit s'il est bien codé pour lire le `GameModeSelector`. | Met à jour le niveau dans le sélecteur (si séparé). |

### Fonctionnement Correct
1.  L'utilisateur a cliqué sur "Cognitif" précédemment -> `GameModeSelector.CurrentGameType` = COGNITIVE.
2.  Le panel Cognitif s'affiche.
3.  L'utilisateur clique sur "Niveau 1".
4.  `LoadLevelButton.Load()` est appelé.
5.  Il lit `GameModeSelector.CurrentGameType` (qui est COGNITIVE).
6.  Il lance `GameManager.NewGame(COGNITIVE, 1)`.

---

## 3. Bouton Retour (Back)
Permet de revenir au menu précédent.

**GameObject Cible :** `Btn_Back`

### Composants Requis
| Composant | Configuration | Rôle |
| :--- | :--- | :--- |
| **RectTransform** | Position et taille. | Zone d'affichage. |
| **BoxCollider** | `Is Trigger`: **Coché**<br>`Size`: Identique au RectTransform + Profondeur Z. | Zone de détection. |
| **NearInteractionTouchable** | `Events to Receive`: **Touch** | Interaction tactile. |
| **TouchableBackButton** (Script) | `Target State`: L'état vers lequel revenir (ex: `MODE_PICK`). | Gère la navigation retour. |
| **CanvasGroup** | **Recommandé**. Permet de désactiver proprement l'interactivité si le bouton doit être caché ou grisé. | Gestion visibilité/interactivité. |

---

## 4. Interaction AirTap (Distance)
Pour que les boutons fonctionnent aussi avec le AirTap (clic à distance avec la main), les scripts doivent implémenter l'interface MRTK appropriée.

**Interface Requise :** `IMixedRealityPointerHandler`

### Vérification dans les Scripts
Tous les scripts de boutons (`ModeButton`, `LoadLevelButton`, `TouchableBackButton`) doivent avoir :
```csharp
public class MonScript : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityTouchHandler
{
    // ...
    public void OnPointerClicked(MixedRealityPointerEventData eventData) 
    {
        // Code pour gérer le clic (identique au Touch)
        LancerAction();
    }
    
    public void OnPointerDown(MixedRealityPointerEventData eventData) { }
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
}
```
*Note : Si `OnPointerClicked` ne fonctionne pas, essayez de mettre la logique dans `OnPointerDown`.*

---

## Résumé des Vérifications à Faire
1.  **ModeButton** : Vérifier qu'il met bien à jour `GameModeSelector` avec le bon type (Cognitive/Mobility).
2.  **LoadLevelButton** : Vérifier qu'il **lit** bien `GameModeSelector` et n'utilise pas une valeur par défaut (comme FreePlay).
3.  **Colliders** : Vérifier que tous les boutons ont un `BoxCollider` (Trigger) de la bonne taille (voir logs `[UI Check]`).
4.  **AirTap** : Vérifier que les scripts implémentent `IMixedRealityPointerHandler`.
