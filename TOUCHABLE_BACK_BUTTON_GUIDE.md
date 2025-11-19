# Guide: Rendre les boutons "Retour" touchables

## 🎯 Objectif
Permettre aux joueurs de toucher (poke) ou pointer (ray) les boutons "Retour" pour revenir au menu de sélection de mode.

## 📦 Composant créé: `TouchableBackButton`

### Fonctionnalités
- ✅ Supporte le **near interaction** (poke/touch avec la main)
- ✅ Supporte le **far interaction** (ray/pinch)
- ✅ Renvoie automatiquement vers le panel `MODE_PICK` (menu de sélection)
- ✅ Configurable dans l'Inspector pour cibler d'autres états si besoin

---

## 🔧 Comment l'utiliser

### Pour les boutons RETOUR dans Cognitive, Motricity, FreePlay:

1. **Sélectionner le bouton "Retour"** dans la hiérarchie Unity
   - Exemple: `CognitivePanel/BackButton`
   - Exemple: `MotricityPanel/BackButton`
   - Exemple: `FreePlayPanel/BackButton`

2. **S'assurer qu'il a un collider**
   - Le bouton doit avoir un `BoxCollider` (ou autre collider)
   - ✅ Cocher **Is Trigger**
   - Ajuster la taille pour couvrir toute la zone cliquable

3. **Ajouter le composant `TouchableBackButton`**
   - Dans l'Inspector: `Add Component` → Rechercher `TouchableBackButton`
   - Ou: `Add Component` → `PopBalloons.UI` → `Touchable Back Button`

4. **Configuration (optionnel)**
   - **Target State**: `MODE_PICK` (par défaut) — menu de sélection
   - **Verbose**: cocher pour voir les logs de debug

5. **Tester**
   - Lancer le jeu
   - Aller dans un des panels (Cognitive, Motricity, FreePlay)
   - Toucher le bouton avec la main OU pointer avec le rayon
   - ✅ Devrait revenir au menu de sélection de mode

---

## 🎮 Pour les boutons de niveau (avant de lancer la partie)

Si tu veux que les boutons de sélection de niveau soient aussi touchables:

1. **Sélectionner chaque bouton de niveau**
   - Exemple: `LevelButton1`, `LevelButton2`, etc.

2. **Vérifier qu'il a un collider trigger**
   - BoxCollider avec **Is Trigger** coché

3. **Ajouter `TouchableButton`** (le script que j'avais créé avant)
   - Ce script générique permet de déclencher le `onClick` existant du bouton Unity
   - Ou utiliser `TouchableNavigationProxy` si besoin de navigation custom

---

## 📋 Exemple de configuration complète

### Bouton Retour Cognitif
```
CognitivePanel
  └─ BackButton (GameObject)
       ├─ BoxCollider (Is Trigger ✓)
       ├─ TouchableBackButton
       │    └─ Target State: MODE_PICK
       │    └─ Verbose: false
       └─ (autres composants UI existants)
```

### Bouton Retour Motricité
```
MotricityPanel
  └─ BackButton (GameObject)
       ├─ BoxCollider (Is Trigger ✓)
       ├─ TouchableBackButton
       │    └─ Target State: MODE_PICK
       └─ (autres composants UI existants)
```

### Bouton Retour FreePlay
```
FreePlayPanel
  └─ BackButton (GameObject)
       ├─ BoxCollider (Is Trigger ✓)
       ├─ TouchableBackButton
       │    └─ Target State: MODE_PICK
       └─ (autres composants UI existants)
```

---

## 🔍 Debug

Si ça ne marche pas:

1. **Vérifier les logs Unity Console**
   - Activer `Verbose` dans `TouchableBackButton`
   - Tu devrais voir: `[TouchableBackButton] Retour vers MODE_PICK`

2. **Vérifier le collider**
   - Le `BoxCollider` doit être **Is Trigger = true**
   - La taille doit couvrir toute la zone cliquable
   - Afficher les colliders: Unity Editor → `Gizmos` → Activer les colliders

3. **Vérifier MainPanel.Instance**
   - S'assurer qu'il y a bien un `MainPanel` dans la scène
   - Vérifier qu'il n'y a qu'une seule instance

4. **Tester les deux modes d'interaction**
   - **Near**: approcher la main et toucher directement
   - **Far**: pointer avec le rayon (hand ray) et pincer

---

## 💡 Avantages de cette approche

✅ **Simple**: un seul composant à ajouter  
✅ **Réutilisable**: marche sur tous les boutons retour  
✅ **Configurable**: peut cibler n'importe quel état du MainPanel  
✅ **Compatible**: fonctionne avec les boutons Unity UI existants  
✅ **Pas de duplication**: pas besoin de réécrire la logique de navigation  

---

## 📝 Alternative: Personnaliser le Target State

Si tu veux qu'un bouton retour aille ailleurs que `MODE_PICK`:

1. Sélectionner le bouton dans l'Inspector
2. Changer `Target State`:
   - `PROFILE` → Menu de sélection de profil
   - `MODE_PICK` → Menu de sélection de mode (par défaut)
   - `COGNITIVE` → Panel cognitif
   - `MOBILITY` → Panel motricité
   - `FREEPLAY` → Panel jeu libre

---

## 🎯 Prochaines étapes suggérées

1. ✅ Ajouter `TouchableBackButton` sur tous les boutons "Retour"
2. ⚙️ Ajouter `TouchableButton` sur les boutons de sélection de niveau
3. 🧪 Tester en mode Simulation (Unity Editor) et sur HoloLens
4. 🎨 Optionnel: Ajouter du feedback visuel (scale, couleur) lors du touch

---

Besoin d'aide pour l'intégration ? Dis-moi quel bouton pose problème !
