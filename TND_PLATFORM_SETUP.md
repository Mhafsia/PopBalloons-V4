# 🎨 Guide de Configuration UI - Plateforme TND

## 📋 Étape 1 : Setup des Managers

### 1.1 Créer les GameObjects Managers

Dans la hiérarchie Unity :

```
Hierarchy
├── [Managers] (Empty GameObject)
│   ├── ProfileManager
│   └── NavigationManager
```

**Instructions :**
1. Clic droit dans Hierarchy → Create Empty → Renommer `[Managers]`
2. Clic droit sur `[Managers]` → Create Empty → Renommer `ProfileManager`
3. Add Component → `ProfileManager` (script)
4. Clic droit sur `[Managers]` → Create Empty → Renommer `NavigationManager`
5. Add Component → `NavigationManager` (script)

### 1.2 Configurer ProfileManager

Dans l'Inspector du GameObject `ProfileManager` :

```
ProfileManager (Script)
├── Default Profile: None
├── Mock Data Settings
│   ├── Use Mock Data: ✓
│   ├── Mock Patient Name: "Marie"
│   └── Mock Patient Age: 8
```

### 1.3 Configurer NavigationManager

Dans l'Inspector du GameObject `NavigationManager` :

```
NavigationManager (Script)
├── Default Page: "ProfileSelector"
├── Enable Transitions: ✓
└── Transition Duration: 0.3
```

---

## 📱 Étape 2 : Créer l'Écran de Sélection de Profil

### 2.1 Structure UI Canvas

```
Canvas (Screen Space - Overlay)
├── ProfileSelectorPage (GameObject + BasePage script)
│   ├── Background (Image - couleur #F8F9FA)
│   ├── ContentPanel (CanvasGroup)
│   │   ├── Logo (Image)
│   │   ├── Title (TextMeshPro)
│   │   │   Text: "Plateforme TND"
│   │   │   Font Size: 48
│   │   ├── ProfileDropdown (TMP_Dropdown)
│   │   │   └── Template
│   │   │       └── Item
│   │   │           └── Item Label
│   │   ├── Description (TextMeshPro)
│   │   │   Text: "Choisissez votre profil pour commencer"
│   │   │   Font Size: 18
│   │   │   Alignment: Center
│   │   └── ValidateButton (Button)
│   │       └── Text: "Valider"
```

### 2.2 Configuration Détaillée

#### ProfileSelectorPage GameObject
- Add Component: `Canvas Group`
- Add Component: `BasePage`
  - Page Name: `ProfileSelector`
  - Requires Profile: ☐ (décoché)
  - Animate In: ✓
  - Animate Out: ✓
  - Animation Duration: 0.3

- Add Component: `ProfileSelector`
  - Profile Dropdown: [Assigner ProfileDropdown]
  - Validate Button: [Assigner ValidateButton]
  - Description Text: [Assigner Description TextMeshPro]
  - Background Panel: [Assigner Background Image]

#### Background (Image)
- Anchor: Stretch (full screen)
- Color: #F8F9FA
- Raycast Target: ✓

#### ContentPanel
- Anchor: Center
- Width: 800
- Height: 600
- Pivot: (0.5, 0.5)

#### Logo (Image)
- Position Y: 200
- Width: 200
- Height: 200
- Color: #4A90E2

#### Title (TextMeshPro)
- Position Y: 100
- Font Size: 48
- Color: #2C3E50
- Alignment: Center Middle
- Auto Size: Off

#### ProfileDropdown (TMP_Dropdown)
- Position Y: 0
- Width: 600
- Height: 60
- Font Size: 24
- Template Height: 200

**Style du Dropdown :**
```
Dropdown
├── Colors
│   ├── Normal: #FFFFFF
│   ├── Highlighted: #E8F4F8
│   ├── Pressed: #D0E8F2
│   └── Disabled: #F0F0F0
└── Navigation: Automatic
```

#### Description (TextMeshPro)
- Position Y: -80
- Width: 600
- Font Size: 18
- Color: #7F8C8D
- Alignment: Center Middle
- Wrapping: Enabled

#### ValidateButton (Button)
- Position Y: -160
- Width: 300
- Height: 60

**Style du Bouton :**
```
Button
├── Normal Color: #28A745
├── Highlighted: #218838
├── Pressed: #1E7E34
├── Disabled: #6C757D
└── Text
    ├── Font Size: 24
    ├── Color: #FFFFFF
    └── Text: "Valider"
```

---

## 🎨 Étape 3 : Créer les Dashboards (Structure de base)

### 3.1 Dashboard Famille

```
Canvas
└── FamilyDashboard (GameObject + BasePage)
    ├── Page Name: "FamilyDashboard"
    ├── Requires Profile: ✓
    ├── Allowed Profiles: [Family]
    └── TopBar
        ├── Logo
        ├── WelcomeText: "Bienvenue, Marie ! 👋"
        ├── SettingsButton
        └── HelpButton
```

### 3.2 Dashboard Clinicien

```
Canvas
└── ClinicianDashboard (GameObject + BasePage)
    ├── Page Name: "ClinicianDashboard"
    ├── Requires Profile: ✓
    ├── Allowed Profiles: [Clinician]
    └── TopBar
        ├── Logo
        ├── PatientSelector (Dropdown)
        ├── PatientsButton
        └── ReportsButton
```

### 3.3 Dashboard Enseignant

```
Canvas
└── TeacherDashboard (GameObject + BasePage)
    ├── Page Name: "TeacherDashboard"
    ├── Requires Profile: ✓
    ├── Allowed Profiles: [Teacher]
    └── TopBar
        ├── Logo
        ├── StudentSelector (Dropdown)
        ├── StudentsButton
        └── ResourcesButton
```

---

## 🔧 Étape 4 : Configuration des Couleurs par Profil

### Palette Famille
```css
Primaire:   #4A90E2 (Bleu doux)
Secondaire: #7ED321 (Vert encourageant)
Accent:     #F5A623 (Orange chaleureux)
Fond:       #F8F9FA (Gris très clair)
Texte:      #2C3E50 (Gris foncé)
```

### Palette Clinicien
```css
Primaire:   #2C5F8D (Bleu médical)
Secondaire: #17A2B8 (Turquoise)
Accent:     #6F42C1 (Violet analytique)
Fond:       #FFFFFF (Blanc)
Texte:      #212529 (Noir)
```

### Palette Enseignant
```css
Primaire:   #28A745 (Vert éducation)
Secondaire: #FFC107 (Jaune soleil)
Accent:     #17A2B8 (Bleu ciel)
Fond:       #FFFEF7 (Crème doux)
Texte:      #2C3E50 (Gris foncé)
```

---

## 🎮 Étape 5 : Test de l'Écran de Sélection

### Checklist de Test

- [ ] Lancer Unity en mode Play
- [ ] L'écran ProfileSelector s'affiche au démarrage
- [ ] Le dropdown contient 4 options :
  - [ ] "Sélectionnez votre profil..."
  - [ ] "👨‍👩‍👧 Famille"
  - [ ] "🏥 Clinicien"
  - [ ] "🎓 Enseignant"
- [ ] Le bouton "Valider" est désactivé par défaut
- [ ] Sélectionner "Famille" :
  - [ ] La description change
  - [ ] La couleur de fond devient bleue (#4A90E2)
  - [ ] Le bouton "Valider" devient actif
- [ ] Cliquer sur "Valider" :
  - [ ] Log dans la Console : "✅ Validation du profil: Family"
  - [ ] Navigation vers FamilyDashboard

### Logs Attendus dans la Console

```
🎯 ProfileManager initialized
🧭 NavigationManager initialized
✅ ProfileDropdown initialisé avec 3 profils
📋 Profil sélectionné dans dropdown: Family
✅ Validation du profil: Family
👨‍👩‍👧 Interface Famille activée pour Marie
🧭 Navigation: ProfileSelector → FamilyDashboard
```

---

## 🐛 Troubleshooting

### Problème : "NullReferenceException sur ProfileDropdown"
**Solution :** Vérifier que le ProfileDropdown est bien assigné dans l'Inspector du ProfileSelector

### Problème : "La page ne s'affiche pas après validation"
**Solution :** 
1. Vérifier que le GameObject FamilyDashboard existe
2. Vérifier que le BasePage a le bon Page Name: "FamilyDashboard"
3. Vérifier que Allowed Profiles contient "Family"

### Problème : "Pas d'animation de transition"
**Solution :**
1. Vérifier que NavigationManager a Enable Transitions = ✓
2. Vérifier que BasePage a Animate In/Out = ✓
3. Vérifier que le GameObject a bien un CanvasGroup

---

## 📚 Ressources

### Scripts Créés
- `ProfileManager.cs` - Gestion des profils
- `NavigationManager.cs` - Gestion de la navigation
- `ProfileSelector.cs` - UI de sélection
- `BasePage.cs` - Classe de base pour toutes les pages

### Prochaines Étapes
1. ✅ Écran de sélection de profil
2. ⏳ Dashboard Famille (en cours)
3. ⏳ Dashboard Clinicien
4. ⏳ Dashboard Enseignant
5. ⏳ Composants UI réutilisables (Cards, Progress bars, etc.)

---

**Prêt pour la Phase 2 : Interface Famille ! 🚀**
