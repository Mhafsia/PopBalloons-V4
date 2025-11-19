# Configuration FreePlay Mode - Guide Complet

## Fichiers créés

### Scripts UI/Panel
1. **FreePlayPanel.cs** - Gestion des états UI pour FreePlay
2. **FreePlayPanelElement.cs** - Élément UI réactif aux changements d'état
3. **FreePlayStats.cs** - Affichage des statistiques (temps, ballons, score)
4. **QuitFreePlayButton.cs** - Bouton pour quitter FreePlay

### Logique de jeu (déjà créée)
- **GameCreator.ContinuousFreePlay()** - Coroutine de spawn infini
- **GameCreator.freePlaySpawnInterval** - Intervalle entre spawns
- **GameCreator.freePlayMaxSimultaneous** - Nombre max de ballons simultanés

---

## Configuration dans Unity Editor

### 1) Créer le Panel FreePlay dans la scène

#### A) Dupliquer un panel existant
1. Ouvre ta scène principale (celle avec le menu)
2. Dans la Hiérarchie, trouve `MotricityPanel` ou `CognitivePanel`
3. Duplique-le (Ctrl+D) et renomme-le **`FreePlayPanel`**

#### B) Configurer le Panel
1. Sélectionne `FreePlayPanel`
2. **Remplace** le component `MotricityPanel` ou `CognitivePanel` par **`FreePlayPanel`**
   - Remove Component (ancien)
   - Add Component → FreePlayPanel
3. Le panel doit avoir 3 sous-panels enfants (GameObjects) :
   - **SETUP** - affiché au menu principal
   - **INGAME** - affiché pendant le jeu
   - **ENDGAME** - affiché à la fin (optionnel pour FreePlay)

#### C) Configurer les sous-panels
Pour chaque GameObject enfant (SETUP, INGAME, ENDGAME) :
1. Ajoute le component **`FreePlayPanelElement`**
2. Dans l'Inspector, configure :
   - **States To Display** : sélectionne l'état correspondant
     - SETUP pour le sous-panel SETUP
     - INGAME pour le sous-panel INGAME
     - ENDGAME pour le sous-panel ENDGAME

---

### 2) Ajouter le bouton FreePlay au menu

#### Méthode simple (déjà expliquée)
1. Duplique un bouton Cognitive ou Motor
2. Renomme-le **`FreePlayButton`**
3. Change le texte visible en "**FreePlay**"
4. Dans le component `LoadLevelButton` :
   - Type = **FREEPLAY**
   - Level Number = **0**
5. Dans le Button OnClick :
   - Cible : le bouton lui-même
   - Fonction : `LoadLevelButton.Load()`

---

### 3) Créer l'interface In-Game pour FreePlay

#### A) Affichage des statistiques
1. Dans le sous-panel **INGAME** de `FreePlayPanel`, crée un **TextMeshPro** (UI → Text - TextMeshPro)
2. Renomme-le `StatsDisplay`
3. Positionne-le en haut de l'écran
4. Ajoute le component **`FreePlayStats`**
5. Configure dans l'Inspector :
   - Stats Text : glisse-dépose le TMP lui-même
   - Show Time : ✓
   - Show Balloons Popped : ✓
   - Show Score : ✓

#### B) Bouton Quitter
1. Dans le sous-panel **INGAME**, crée un **Button** (UI → Button)
2. Renomme-le `QuitButton`
3. Change le texte en "**Quitter**" ou "**Menu**"
4. Positionne-le (coin en bas à gauche ou en haut à droite)
5. Ajoute le component **`QuitFreePlayButton`**
6. Dans le Button OnClick :
   - Cible : le bouton lui-même
   - Fonction : `QuitFreePlayButton.QuitFreePlay()`

---

### 4) Configurer les paramètres de spawn FreePlay

1. Trouve le GameObject qui a le component **`GameCreator`** (souvent un Manager dans la scène)
2. Dans l'Inspector, cherche la section **FreePlay settings**
3. Configure :
   - **Free Play Spawn Interval** : 1.5 (secondes entre chaque spawn)
   - **Free Play Max Simultaneous** : 10 (ballons max en même temps, 0 = illimité)

---

### 5) Vérifications finales

#### Checklist :
- [ ] `FreePlayPanel` existe dans la scène avec les 3 sous-panels (SETUP, INGAME, ENDGAME)
- [ ] Chaque sous-panel a le component `FreePlayPanelElement` avec le bon état
- [ ] Le bouton FreePlay est configuré avec `LoadLevelButton` (Type=FREEPLAY, Level=0)
- [ ] Le sous-panel INGAME contient :
  - [ ] Un TextMeshPro avec `FreePlayStats`
  - [ ] Un Button avec `QuitFreePlayButton`
- [ ] `GameCreator` a les paramètres FreePlay configurés

#### Test :
1. Lance Play Mode
2. Clique le bouton FreePlay
3. Vérifie que :
   - Le panel INGAME s'affiche
   - Les stats se mettent à jour (temps, ballons, score)
   - Des ballons spawns automatiquement toutes les ~1.5s
   - Le bouton Quitter ramène au menu principal

---

## Structure hiérarchique recommandée

```
Canvas
├─ FreePlayPanel (Component: FreePlayPanel)
│  ├─ SETUP (Component: FreePlayPanelElement, States: SETUP)
│  │  └─ (vide ou message "Prêt à jouer")
│  │
│  ├─ INGAME (Component: FreePlayPanelElement, States: INGAME)
│  │  ├─ StatsDisplay (TMP + FreePlayStats)
│  │  └─ QuitButton (Button + QuitFreePlayButton)
│  │
│  └─ ENDGAME (Component: FreePlayPanelElement, States: ENDGAME)
│     └─ (optionnel : message "Session terminée")
│
├─ MainPanel
│  └─ ModePick
│     ├─ CognitiveButton
│     ├─ MotorButton
│     └─ FreePlayButton (Component: LoadLevelButton)
│
└─ (autres panels...)
```

---

## Résumé des composants créés

| Fichier | Fonction | Où l'utiliser |
|---------|----------|---------------|
| `FreePlayPanel.cs` | Gère les états du panel FreePlay | GameObject `FreePlayPanel` |
| `FreePlayPanelElement.cs` | Élément UI réactif | Sous-panels (SETUP, INGAME, ENDGAME) |
| `FreePlayStats.cs` | Affiche stats en temps réel | TextMeshPro dans INGAME |
| `QuitFreePlayButton.cs` | Bouton pour quitter | Button dans INGAME |

---

## Personnalisation (optionnelle)

### Ajouter un compteur de ballons bonus
Modifie `FreePlayStats.cs` pour tracker les bonus séparément :
```csharp
// Ajouter dans la classe
private int bonusBalloons;
```

### Modifier l'intervalle de spawn dynamiquement
Dans `GameCreator.ContinuousFreePlay()`, remplace :
```csharp
yield return new WaitForSeconds(freePlaySpawnInterval);
```
Par :
```csharp
float interval = Mathf.Lerp(freePlaySpawnInterval, 0.5f, Time.time / 60f); // accélère avec le temps
yield return new WaitForSeconds(interval);
```

### Ajouter un score cible
Ajoute un champ `public int freePlayTargetScore` dans `GameCreator` et arrête la coroutine quand atteint.

---

## Dépannage

### Le bouton FreePlay ne fait rien
- Vérifie que `LoadLevelButton.Type = FREEPLAY`
- Vérifie que OnClick appelle bien `LoadLevelButton.Load()`
- Regarde la Console pour des warnings

### Les stats ne s'affichent pas
- Vérifie que `FreePlayStats` a bien la référence au TMP
- Vérifie que le GameObject avec `FreePlayStats` est actif pendant INGAME

### Les ballons ne spawns pas
- Vérifie que `GameCreator.balloonsPrefabs` est rempli dans l'Inspector
- Regarde la Console pour des erreurs de spawn
- Vérifie que `freePlayMaxSimultaneous` n'est pas à 0 et qu'il reste de la place

### Le panel INGAME ne s'affiche pas
- Vérifie que `FreePlayPanel` et `FreePlayPanelElement` sont bien configurés
- Vérifie que les états (SETUP, INGAME, ENDGAME) correspondent entre le Panel et les Elements

---

## Support

Si tu rencontres un problème :
1. Vérifie la Console Unity pour des erreurs/warnings
2. Assure-toi que tous les components sont bien assignés dans l'Inspector
3. Teste en mode Play et regarde les logs
