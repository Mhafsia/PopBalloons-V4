# 👨‍👩‍👧 Configuration Dashboard Famille

## 📋 Vue d'ensemble

Le Dashboard Famille est l'écran principal pour les parents et la famille. Il affiche:
- Message de bienvenue personnalisé
- Programme des séances du jour
- Progression hebdomadaire
- Accès rapide aux jeux, progrès et récompenses

---

## 🏗️ Structure UI dans Unity

### Hiérarchie Complète

```
Canvas
└── FamilyDashboard (+ CanvasGroup + BasePage + FamilyDashboard)
    ├── Background (Image)
    ├── TopBar
    │   ├── Logo (Image)
    │   ├── WelcomeText (TextMeshPro)
    │   ├── SettingsButton (Button)
    │   └── HelpButton (Button)
    ├── MainContent
    │   ├── LeftColumn
    │   │   └── AvatarCard
    │   │       ├── AvatarImage
    │   │       ├── LevelText
    │   │       └── StarsDisplay
    │   ├── CenterColumn
    │   │   ├── ProgramCard
    │   │   │   ├── CardTitle (TextMeshPro "📅 PROGRAMME DU JOUR")
    │   │   │   ├── SessionsScrollView
    │   │   │   │   └── SessionsContainer (Vertical Layout Group)
    │   │   │   │       └── [SessionItems - instanciés dynamiquement]
    │   │   │   └── LaunchButton (Button)
    │   │   │       └── ButtonText (TextMeshPro)
    │   │   └── ProgressCard
    │   │       ├── CardTitle (TextMeshPro "📊 PROGRESSION")
    │   │       ├── WeeklyCalendar (Horizontal Layout Group)
    │   │       │   └── [DayIndicators - instanciés dynamiquement]
    │   │       ├── StatsText (TextMeshPro)
    │   │       └── ProgressBar
    │   │           ├── Background (Image)
    │   │           └── Fill (Image)
    │   └── RightColumn (3 boutons d'accès rapide)
    │       ├── GamesButton
    │       │   ├── Icon (Image - 🎮)
    │       │   └── Text (TextMeshPro "JEUX")
    │       ├── ProgressButton
    │       │   ├── Icon (Image - 📈)
    │       │   └── Text (TextMeshPro "PROGRÈS")
    │       └── RewardsButton
    │           ├── Icon (Image - 🏆)
    │           └── Text (TextMeshPro "RÉCOMPENSES")
```

---

## 🎨 Configuration Détaillée

### 1. FamilyDashboard (Root)

**Components:**
- Canvas Group (alpha = 0 au départ)
- BasePage
  - Page Name: `FamilyDashboard`
  - Requires Profile: ✓
  - Allowed Profiles: [Family]
  - Animate In: ✓
  - Animate Out: ✓
  - Animation Duration: 0.3

- FamilyDashboard (Script)
  - [Assigner toutes les références via Inspector]

**RectTransform:**
- Anchor: Stretch (full screen)
- Offset: 0, 0, 0, 0

---

### 2. Background

**Image Component:**
- Color: #F8F9FA (gris très clair)
- Raycast Target: ✓

**RectTransform:**
- Anchor: Stretch
- Offset: 0, 0, 0, 0

---

### 3. TopBar

**RectTransform:**
- Anchor Preset: Top Stretch
- Height: 100
- Pivot: (0.5, 1)

**Horizontal Layout Group:**
- Padding: Left 40, Right 40, Top 20
- Spacing: 20
- Child Alignment: Middle Left
- Child Force Expand: Width ✓, Height ☐

#### 3.1 Logo
- Width: 60, Height: 60
- Image: [Logo de la plateforme]

#### 3.2 WelcomeText
**TextMeshPro:**
- Text: "Bienvenue, Marie ! 👋" (sera mis à jour dynamiquement)
- Font Size: 32
- Font Style: Bold
- Color: #2C3E50
- Auto Size: Min 24, Max 32
- Alignment: Middle Left
- Layout Element: Flexible Width = 1

#### 3.3 SettingsButton & HelpButton
**Button:**
- Width: 50, Height: 50
- Colors:
  - Normal: #E8F4F8
  - Highlighted: #D0E8F2
  - Pressed: #B8D8E8

**Icon (TextMeshPro):**
- Text: "⚙️" (Settings) ou "❓" (Help)
- Font Size: 28
- Alignment: Center Middle

---

### 4. MainContent

**RectTransform:**
- Anchor: Stretch
- Top: -100 (sous TopBar)
- Bottom: 40
- Left: 40
- Right: 40

**Horizontal Layout Group:**
- Spacing: 30
- Child Alignment: Upper Left
- Child Force Expand: Width ☐, Height ✓

---

### 5. ProgramCard (dans CenterColumn)

**RectTransform:**
- Min Width: 600
- Height: Flexible

**Image (Card Background):**
- Color: #FFFFFF
- Shadow: Offset (0, 4), Distance 8, Color rgba(0,0,0,0.1)

**Vertical Layout Group:**
- Padding: 24 all around
- Spacing: 16

#### 5.1 CardTitle
**TextMeshPro:**
- Text: "📅 PROGRAMME DU JOUR"
- Font Size: 24
- Font Style: Bold
- Color: #2C3E50

#### 5.2 SessionsScrollView
**Scroll Rect:**
- Vertical: ✓
- Horizontal: ☐
- Movement Type: Clamped
- Scrollbar: Vertical (auto-hide)

**Content (SessionsContainer):**
- Vertical Layout Group
  - Spacing: 12
  - Child Force Expand: Width ✓, Height ☐
- Content Size Fitter
  - Vertical Fit: Preferred Size

#### 5.3 LaunchButton
**Button:**
- Height: 60
- Colors:
  - Normal: #28A745
  - Highlighted: #218838
  - Pressed: #1E7E34
  - Disabled: #6C757D

**ButtonText (TextMeshPro):**
- Text: "Lancer la séance suivante"
- Font Size: 20
- Color: #FFFFFF
- Alignment: Center Middle

---

### 6. Prefabs Nécessaires

#### 6.1 SessionItem Prefab

**Structure:**
```
SessionItem (120px height)
├── StatusIcon (TextMeshPro) - 30x30 - Left
├── Title (TextMeshPro) - Flex Width
├── TypeTag (Image + Text)
└── Time (TextMeshPro) - Right
```

**Configuration:**
```
SessionItem
├── RectTransform: Height 120
├── Image: Color #FFFFFF, Shadow
├── Horizontal Layout Group
│   ├── Padding: 16 all
│   ├── Spacing: 12
│   └── Child Alignment: Middle Left
```

**StatusIcon:**
- Size: 30x30
- Font Size: 24
- Layout Element: Min Width 30, Min Height 30, Flexible ☐

**Title:**
- Font Size: 18
- Font Style: SemiBold
- Color: #2C3E50
- Layout Element: Flexible Width = 1

**TypeTag:**
- Image: Rounded corners, Color variable
- Padding: 8x4
- Text: Font Size 14, Color #FFFFFF

**Time:**
- Font Size: 16
- Color: #7F8C8D
- Alignment: Middle Right
- Layout Element: Min Width 100

---

#### 6.2 DayIndicator Prefab

**Structure:**
```
DayIndicator (80px width, 100px height)
├── Background (Image)
├── DayLabel (TextMeshPro "L", "M", etc.)
└── StatusIcon (TextMeshPro "✓" ou "•")
```

**Configuration:**
- Background: Color #F8F9FA, Rounded corners
- DayLabel: Font Size 18, Top, Center
- StatusIcon: Font Size 32, Center

---

### 7. ProgressCard

**RectTransform:**
- Height: 200

**Vertical Layout Group:**
- Padding: 24
- Spacing: 16

#### 7.1 WeeklyCalendar
**Horizontal Layout Group:**
- Spacing: 8
- Child Alignment: Middle Center
- Child Force Expand: Width ✓, Height ☐

#### 7.2 StatsText
**TextMeshPro:**
- Font Size: 16
- Color: #2C3E50
- Alignment: Center

#### 7.3 ProgressBar
**Structure:**
```
ProgressBar
├── Background (Image - #E0E0E0)
└── Fill (Image - #28A745)
```

**Background:**
- Height: 12
- Rounded corners

**Fill:**
- Image Type: Filled
- Fill Method: Horizontal
- Fill Amount: 0.0 à 1.0 (contrôlé par script)
- Color: #28A745

---

### 8. Quick Action Buttons

**Dimensions:**
- Width: 200
- Height: 250

**Structure commune:**
```
ButtonCard
├── Background (Image)
├── Icon (Image ou TextMeshPro)
├── Title (TextMeshPro)
└── Arrow (TextMeshPro ">")
```

**Styles:**
- GamesButton: Background #4A90E2, Icon 🎮
- ProgressButton: Background #7ED321, Icon 📈
- RewardsButton: Background #F5A623, Icon 🏆

---

## 🔗 Assignation des Références

Dans l'Inspector de `FamilyDashboard` :

### Top Bar
- Welcome Text: [TopBar/WelcomeText]
- Settings Button: [TopBar/SettingsButton]
- Help Button: [TopBar/HelpButton]

### Today's Program
- Sessions Container: [MainContent/CenterColumn/ProgramCard/SessionsScrollView/Viewport/SessionsContainer]
- Session Item Prefab: [Créer et assigner le prefab SessionItem]
- Launch Next Session Button: [MainContent/CenterColumn/ProgramCard/LaunchButton]
- Next Session Button Text: [MainContent/CenterColumn/ProgramCard/LaunchButton/ButtonText]

### Weekly Progress
- Weekly Stats Text: [MainContent/CenterColumn/ProgressCard/StatsText]
- Weekly Calendar Container: [MainContent/CenterColumn/ProgressCard/WeeklyCalendar]
- Day Indicator Prefab: [Créer et assigner le prefab DayIndicator]
- Progress Fill Bar: [MainContent/CenterColumn/ProgressCard/ProgressBar/Fill]

### Quick Actions
- Games Button: [MainContent/RightColumn/GamesButton]
- Progress Button: [MainContent/RightColumn/ProgressButton]
- Rewards Button: [MainContent/RightColumn/RewardsButton]

### Visual Feedback
- Background Gradient: [Background]

---

## 🎮 Setup des Managers

**N'oublie pas de créer :**

1. **GameObject `DataManager`** dans la scène
   - Add Component → DataManager (script)
   - Il va générer automatiquement les données fictives au démarrage

2. Vérifier que **ProfileManager** et **NavigationManager** sont présents

---

## ✅ Checklist de Test

- [ ] Le Dashboard s'affiche quand on sélectionne "Famille"
- [ ] Le message de bienvenue affiche "Bienvenue, Marie ! 👋"
- [ ] 3 séances s'affichent dans le programme du jour
- [ ] La première séance est marquée ✓ (Terminée)
- [ ] Le calendrier hebdomadaire affiche L, M, M, J, V, S, D
- [ ] Les 3 premiers jours (L, M, M) ont une ✓
- [ ] Le bouton "Lancer..." affiche "Lancer PopBalloons"
- [ ] Cliquer sur "Lancer PopBalloons" démarre le jeu
- [ ] Les 3 boutons d'accès rapide sont cliquables
- [ ] La barre de progression est remplie à ~20% (3/15 séances)

---

## 🐛 Troubleshooting

### "SessionItems ne s'affichent pas"
**Solution:**
1. Vérifier que `sessionsContainer` est bien assigné
2. Vérifier que `sessionItemPrefab` existe et est assigné
3. Check Console pour les logs "📅 X séances affichées"

### "Bouton Lancer désactivé"
**Solution:**
1. Vérifier que DataManager génère bien les données fictives
2. Check Console pour "🎲 Génération des données fictives..."
3. Vérifier qu'au moins une séance a le statut `Pending`

### "NullReferenceException"
**Solution:**
Toujours vérifier que TOUTES les références sont assignées dans l'Inspector

---

**Prêt pour les tests ! 🚀**
