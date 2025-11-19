# 📦 UI Components - Guide de Configuration Unity

Ce guide explique comment configurer les composants UI réutilisables dans Unity.

---

## 🎴 Card Component

### Création d'un Prefab Card

#### 1. Hiérarchie
```
Card (RectTransform + Image + Card.cs + Shadow)
├── Header (Horizontal Layout Group)
│   ├── Icon (TextMeshProUGUI) [emoji]
│   └── Title (TextMeshProUGUI)
├── Content (Vertical Layout Group)
│   └── Body (TextMeshProUGUI)
└── Footer (Horizontal Layout Group)
    └── ActionButton (Button)
        └── ButtonText (TextMeshProUGUI)
```

#### 2. Configuration Card (Root)
- **RectTransform**: Width=300, Height=auto (Content Size Fitter)
- **Image** (Background):
  - Color: White (255, 255, 255)
  - Image Type: Sliced (pour coins arrondis)
  - Sprite: UI/Rounded_Square (créer un sprite 9-sliced avec coins arrondis)
- **Shadow**:
  - Effect Color: (0, 0, 0, 25) - noir semi-transparent
  - Effect Distance: (0, 4)
- **Content Size Fitter**:
  - Vertical Fit: Preferred Size
- **Vertical Layout Group**:
  - Padding: 20 (all sides)
  - Spacing: 15
  - Child Force Expand: Width ✓

#### 3. Configuration Header
- **Horizontal Layout Group**:
  - Spacing: 10
  - Child Alignment: Middle Left
  - Child Force Expand: Width ✓, Height ✗
- **Height**: 40px

**Icon (TextMeshProUGUI)**:
- Font Size: 24
- Alignment: Middle Center
- Width: 40px
- Auto Size: Off
- Text: "📄" (par défaut)

**Title (TextMeshProUGUI)**:
- Font Size: 18
- Font Style: Bold
- Color: #2C3E50 (texte sombre)
- Alignment: Middle Left
- Auto Size: Off

#### 4. Configuration Content
- **Vertical Layout Group**:
  - Spacing: 10
  - Child Force Expand: Width ✓

**Body (TextMeshProUGUI)**:
- Font Size: 14
- Color: #34495E (gris foncé)
- Alignment: Top Left
- Wrapping: Enabled
- Auto Size: Off
- Min Height: 60px

#### 5. Configuration Footer
- **Horizontal Layout Group**:
  - Child Alignment: Middle Right
  - Child Force Expand: Width ✗, Height ✗
- **Height**: 50px

**ActionButton**:
- Width: 120px, Height: 40px
- Normal Color: #4A90E2
- Highlighted: 10% darker
- Pressed: 20% darker
- Transition: Color Tint

**ButtonText**:
- Font Size: 14
- Color: White
- Alignment: Middle Center
- Text: "Voir plus"

#### 6. Script Card References
Assigner dans l'inspecteur:
- Header Section → Header GameObject
- Title Text → Title TextMeshProUGUI
- Icon Text → Icon TextMeshProUGUI
- Content Section → Content GameObject
- Body Text → Body TextMeshProUGUI
- Footer Section → Footer GameObject
- Action Button → ActionButton Button
- Action Button Text → ButtonText TextMeshProUGUI

### Utilisation en Code
```csharp
// Récupérer le composant
Card card = GetComponent<Card>();

// Configuration basique
card.SetTitle("Prochaine Séance");
card.SetIcon("🎮");
card.SetBody("PopBalloons - Motricité\n14:00 - 14:30");
card.SetAction("Lancer", () => {
    Debug.Log("Séance lancée!");
});

// Styles
card.SetStyle(CardStyle.Primary);    // Couleur du profil
card.SetStyle(CardStyle.Success);    // Vert
card.SetStyle(CardStyle.Warning);    // Orange
card.SetShadow(true);                // Avec ombre
```

---

## 📊 ProgressBar Component

### Création d'un Prefab ProgressBar

#### 1. Hiérarchie
```
ProgressBar (RectTransform + ProgressBar.cs)
├── TopRow (Horizontal Layout Group)
│   ├── Label (TextMeshProUGUI)
│   └── Percentage (TextMeshProUGUI)
└── BarContainer (RectTransform + Image background)
    └── FillBar (Image + Fill)
```

#### 2. Configuration ProgressBar (Root)
- **RectTransform**: Width=280, Height=60
- **Vertical Layout Group**:
  - Spacing: 8
  - Child Force Expand: Width ✓

#### 3. Configuration TopRow
- **Height**: 20px
- **Horizontal Layout Group**:
  - Child Force Expand: Width ✓

**Label (TextMeshProUGUI)**:
- Font Size: 14
- Color: #2C3E50
- Alignment: Middle Left
- Text: "Progression"

**Percentage (TextMeshProUGUI)**:
- Font Size: 14
- Font Style: Bold
- Color: #4A90E2
- Alignment: Middle Right
- Text: "75%"

#### 4. Configuration BarContainer
- **Height**: 24px
- **Width**: Stretch (anchor: stretch horizontal)
- **Image** (Background):
  - Color: (200, 200, 200, 77) - gris semi-transparent
  - Sprite: UI/RoundedBar_BG
  - Image Type: Sliced

**FillBar (Image)**:
- **Anchor**: Stretch horizontal, Center vertical
- **Pivot**: (0, 0.5)
- **Position**: (0, 0)
- **Height**: 20px (légèrement moins que container)
- **Image Type**: Filled
- **Fill Method**: Horizontal (Left to Right)
- **Fill Amount**: 0.75 (sera contrôlé par script)
- **Color**: #4A90E2 (bleu)
- **Sprite**: UI/RoundedBar_Fill

#### 5. Script ProgressBar References
- Fill Image → FillBar Image
- Background Image → BarContainer Image
- Label Text → Label TextMeshProUGUI
- Percentage Text → Percentage TextMeshProUGUI
- Show Percentage → ✓ (coché)
- Animate Changes → ✓ (coché)
- Animation Duration → 0.5

### Utilisation en Code
```csharp
ProgressBar progressBar = GetComponent<ProgressBar>();

// Définir valeur (0-1)
progressBar.SetValue(0.75f);

// Définir pourcentage (0-100)
progressBar.SetPercentage(75f);

// Avec/sans animation
progressBar.SetValue(0.5f, animate: true);

// Changer le label
progressBar.SetLabel("Séances complétées");

// Styles
progressBar.SetStyle(ProgressBarStyle.ProfileColor);  // Couleur du profil
progressBar.SetStyle(ProgressBarStyle.Success);       // Vert
progressBar.SetStyle(ProgressBarStyle.Warning);       // Orange
progressBar.SetStyle(ProgressBarStyle.Gradient);      // Dégradé vert→rouge

// Méthodes utiles
progressBar.Increment(0.1f);   // +10%
progressBar.Decrement(0.05f);  // -5%
progressBar.Reset();           // Retour à 0
progressBar.Fill();            // Remplir à 100%
```

---

## 🏅 Badge Component

### Création d'un Prefab Badge

#### 1. Hiérarchie
```
Badge (RectTransform + Badge.cs)
├── Background (Image - fond coloré)
├── Border (Image - cadre doré optionnel)
├── Icon (TextMeshProUGUI ou Image)
├── LockOverlay (Image - cadenas si verrouillé)
├── GlowEffect (Image - effet lumineux)
└── Label (TextMeshProUGUI - sous le badge)
```

#### 2. Configuration Badge (Root)
- **RectTransform**: Width=80, Height=100 (Medium size)
- **Vertical Layout Group**:
  - Spacing: 8
  - Child Alignment: Upper Center

#### 3. Configuration Background
- **Width/Height**: 60x60 (pour Medium)
- **Image**:
  - Sprite: UI/Circle ou UI/Badge_BG
  - Color: #4A90E2 (sera contrôlé par script)
  - Image Type: Simple
  - Preserve Aspect: ✓

#### 4. Configuration Border
- **Width/Height**: 64x64 (4px plus grand que BG)
- **Image**:
  - Sprite: UI/Circle_Border
  - Color: #FFD700 (or)
  - Image Type: Simple
- **Active**: false (par défaut, activé pour badges spéciaux)

#### 5. Configuration Icon (TextMeshProUGUI)
- **Position**: Centré sur Background
- **Font Size**: 28
- **Color**: White
- **Alignment**: Middle Center
- **Text**: "🏆"

**OU Icon (Image)** si sprite:
- **Width/Height**: 40x40
- **Position**: Centré
- **Preserve Aspect**: ✓

#### 6. Configuration LockOverlay
- **Width/Height**: Same as Background (60x60)
- **Position**: Centré sur Background
- **Image**:
  - Sprite: UI/Lock_Icon
  - Color: (0, 0, 0, 150) - noir semi-transparent
  - Image Type: Simple
- **Active**: false (activé si isLocked = true)

#### 7. Configuration GlowEffect
- **Width/Height**: 70x70 (10px plus grand)
- **Position**: Centré sur Background
- **Image**:
  - Sprite: UI/Glow (sprite flou)
  - Color: (255, 215, 0, 100) - or semi-transparent
  - Image Type: Simple
- **Active**: false (activé pour badges spéciaux débloqués)

#### 8. Configuration Label
- **Width**: 80px, **Height**: 30px
- **Font Size**: 12
- **Color**: #2C3E50
- **Alignment**: Upper Center
- **Wrapping**: Enabled
- **Text**: "Badge"

#### 9. Script Badge References
- Background Image → Background Image
- Icon Image → Icon Image (optionnel)
- Icon Text → Icon TextMeshProUGUI (optionnel)
- Label Text → Label TextMeshProUGUI
- Lock Overlay → LockOverlay Image
- Border Image → Border Image
- Glow Effect → GlowEffect GameObject
- Shape → Circle
- Size → Medium
- Is Locked → false
- Is Special → false

### Utilisation en Code
```csharp
Badge badge = GetComponent<Badge>();

// Configuration basique
badge.SetIcon("🏆");
badge.SetLabel("Première Séance");
badge.SetColor(new Color(1f, 0.84f, 0.0f)); // Or

// Verrouiller/déverrouiller
badge.SetLocked(true);   // Badge grisé avec cadenas
badge.Unlock();          // Animation de déverrouillage

// Marquer comme spécial (cadre doré + glow)
badge.SetSpecial(true);

// Presets pour récompenses
badge.SetBronzeReward("🥉", "Débutant");
badge.SetSilverReward("🥈", "Assidu");
badge.SetGoldReward("🥇", "Champion");
badge.SetSpecialReward("⭐", "Légende");

// Tailles
badge.SetSize(BadgeSize.Small);       // 40x40
badge.SetSize(BadgeSize.Medium);      // 60x60
badge.SetSize(BadgeSize.Large);       // 80x80
badge.SetSize(BadgeSize.ExtraLarge);  // 120x120
```

---

## 🔘 StyledButton Component

### Création d'un Prefab StyledButton

#### 1. Hiérarchie
```
StyledButton (RectTransform + Button + StyledButton.cs)
├── Background (Image)
├── Border (Image - optionnel)
└── Content (Horizontal Layout Group)
    ├── Icon (TextMeshProUGUI ou Image)
    └── Label (TextMeshProUGUI)
```

#### 2. Configuration StyledButton (Root)
- **RectTransform**: Width=120, Height=50 (Medium)
- **Button**:
  - Transition: None (contrôlé par script)
  - Navigation: Automatic

#### 3. Configuration Background
- **Anchor**: Stretch (all)
- **Offset**: 0 (all)
- **Image**:
  - Sprite: UI/Button_BG
  - Color: #4A90E2 (sera contrôlé par script)
  - Image Type: Sliced

#### 4. Configuration Border
- **Anchor**: Stretch (all)
- **Offset**: -2 (pour créer une bordure de 2px)
- **Image**:
  - Sprite: UI/Button_Border
  - Color: #4A90E2
  - Image Type: Sliced
- **Active**: false (activé pour variantes Outline/Secondary)

#### 5. Configuration Content
- **Anchor**: Stretch (all)
- **Offset**: 0 (all)
- **Horizontal Layout Group**:
  - Spacing: 8
  - Padding: 15 (left/right), 10 (top/bottom)
  - Child Alignment: Middle Center
  - Child Force Expand: Width ✗, Height ✗

**Icon (TextMeshProUGUI)**:
- **Width**: 24px, **Height**: 24px
- **Font Size**: 20
- **Color**: White
- **Alignment**: Middle Center
- **Text**: "▶"

**Label (TextMeshProUGUI)**:
- **Font Size**: 16
- **Color**: White
- **Alignment**: Middle Center
- **Text**: "Lancer"

#### 6. Script StyledButton References
- Background Image → Background Image
- Border Image → Border Image
- Label Text → Label TextMeshProUGUI
- Icon Text → Icon TextMeshProUGUI
- Icon Image → Icon Image (optionnel)
- Variant → Primary
- Button Size → Medium
- Use Profile Color → ✓ (coché)
- Icon On Right → false

### Utilisation en Code
```csharp
StyledButton button = GetComponent<StyledButton>();

// Configuration basique
button.SetText("Lancer");
button.SetIcon("▶");

// Variantes
button.SetVariant(ButtonVariant.Primary);    // Fond coloré, texte blanc
button.SetVariant(ButtonVariant.Secondary);  // Fond blanc, bordure colorée
button.SetVariant(ButtonVariant.Outline);    // Transparent, bordure colorée
button.SetVariant(ButtonVariant.Ghost);      // Transparent, texte coloré
button.SetVariant(ButtonVariant.Danger);     // Rouge (supprimer, annuler)

// Tailles
button.SetSize(ButtonSize.Small);   // 80x40
button.SetSize(ButtonSize.Medium);  // 120x50
button.SetSize(ButtonSize.Large);   // 160x60

// Couleurs
button.UseProfileColor(true);       // Couleur du profil actif
button.SetCustomColor(Color.blue);  // Couleur personnalisée

// Événements
button.AddClickListener(() => {
    Debug.Log("Bouton cliqué!");
});

// État
button.SetInteractable(true);   // Activé
button.SetInteractable(false);  // Désactivé (grisé)
```

---

## 🎨 Sprites Requis

Pour que ces composants fonctionnent correctement, créez les sprites suivants:

### 1. Card Sprites
- **UI/Rounded_Square**: Carré 100x100px avec coins arrondis (radius 12px), 9-sliced
  - Border: Left=20, Right=20, Top=20, Bottom=20

### 2. ProgressBar Sprites
- **UI/RoundedBar_BG**: Rectangle 100x24px, coins arrondis, 9-sliced
  - Border: Left=12, Right=12, Top=0, Bottom=0
- **UI/RoundedBar_Fill**: Identique mais couleur unie

### 3. Badge Sprites
- **UI/Circle**: Cercle parfait 64x64px blanc
- **UI/Circle_Border**: Anneau (donut) 64x64px
- **UI/Lock_Icon**: Cadenas 32x32px
- **UI/Glow**: Cercle flou 80x80px (effet radial gradient)

### 4. Button Sprites
- **UI/Button_BG**: Rectangle 120x50px, coins arrondis, 9-sliced
  - Border: Left=15, Right=15, Top=15, Bottom=15
- **UI/Button_Border**: Rectangle outline 120x50px, 9-sliced

### Création dans Photoshop/Figma
```
Fichier → Nouveau → 100x100px
Forme Rectangle Arrondi → Rayon 12px
Exporter → PNG (transparent background)
Unity → Import → Texture Type: Sprite (2D and UI)
Sprite Editor → Slice Type: Manual → Border: 20,20,20,20
```

---

## ✅ Checklist de Configuration

### Card Prefab
- [ ] Hiérarchie créée (Card → Header/Content/Footer)
- [ ] Shadow component ajouté
- [ ] Content Size Fitter configuré
- [ ] Layout Groups configurés
- [ ] Sprites 9-sliced assignés
- [ ] Script references assignées
- [ ] Prefab sauvegardé dans `Assets/TND_Platform/Prefabs/UI/`

### ProgressBar Prefab
- [ ] Hiérarchie créée (ProgressBar → TopRow/BarContainer)
- [ ] Fill Image configurée (Filled, Horizontal)
- [ ] Layout Groups configurés
- [ ] Sprites assignés
- [ ] Script references assignées
- [ ] Animation activée
- [ ] Prefab sauvegardé

### Badge Prefab
- [ ] Hiérarchie créée (Badge → BG/Border/Icon/Lock/Glow/Label)
- [ ] Images circulaires configurées
- [ ] Lock/Glow désactivés par défaut
- [ ] Tailles configurées (Small/Medium/Large variants)
- [ ] Script references assignées
- [ ] Prefab sauvegardé

### StyledButton Prefab
- [ ] Hiérarchie créée (Button → BG/Border/Content)
- [ ] Button component configuré (Transition: None)
- [ ] Layout Group configuré
- [ ] Border désactivée par défaut
- [ ] Script references assignées
- [ ] Hover events testés
- [ ] Prefab sauvegardé

---

## 🧪 Tests

### Test Card
```csharp
Card card = Instantiate(cardPrefab).GetComponent<Card>();
card.SetTitle("Test Card");
card.SetIcon("🎮");
card.SetBody("Description de test avec texte assez long pour vérifier le wrapping automatique.");
card.SetAction("Action", () => Debug.Log("Click!"));
card.SetStyle(CardStyle.Primary);
```

### Test ProgressBar
```csharp
ProgressBar bar = Instantiate(progressBarPrefab).GetComponent<ProgressBar>();
bar.SetLabel("Test Progress");
bar.SetValue(0f);
bar.SetValue(0.75f, animate: true); // Animation 0→75%
```

### Test Badge
```csharp
Badge badge = Instantiate(badgePrefab).GetComponent<Badge>();
badge.SetGoldReward("🏆", "Champion");
badge.SetLocked(true);
// Attendre 2s puis:
badge.Unlock(); // Animation de déverrouillage
```

### Test StyledButton
```csharp
StyledButton btn = Instantiate(buttonPrefab).GetComponent<StyledButton>();
btn.SetText("Test Button");
btn.SetIcon("▶");
btn.SetVariant(ButtonVariant.Primary);
btn.AddClickListener(() => Debug.Log("Clicked!"));
```

---

## 🎯 Utilisation dans FamilyDashboard

```csharp
// Dans FamilyDashboard.cs

// Card pour prochaine séance
Card nextSessionCard = Instantiate(cardPrefab, cardsContainer, false).GetComponent<Card>();
nextSessionCard.SetTitle("Prochaine Séance");
nextSessionCard.SetIcon("🎮");
nextSessionCard.SetBody($"{nextSession.gameName}\n{nextSession.scheduledTime:HH:mm}");
nextSessionCard.SetAction("Lancer", OnLaunchNextSession);
nextSessionCard.SetStyle(CardStyle.Primary);

// ProgressBar pour progression hebdomadaire
ProgressBar weeklyProgress = Instantiate(progressBarPrefab, progressContainer, false).GetComponent<ProgressBar>();
weeklyProgress.SetLabel("Séances de la semaine");
weeklyProgress.SetPercentage(weeklyData.CompletionRate);
weeklyProgress.SetStyle(ProgressBarStyle.Gradient);

// Badge pour récompenses
foreach (var reward in DataManager.Instance.Rewards)
{
    Badge badge = Instantiate(badgePrefab, rewardsContainer, false).GetComponent<Badge>();
    badge.SetIcon(reward.Name);
    badge.SetLabel(reward.Name);
    badge.SetLocked(!reward.IsUnlocked);
    
    if (reward.Type == RewardType.Gold)
        badge.SetGoldReward(reward.Name, reward.Name);
}
```

---

## 📝 Notes Importantes

1. **TextMeshPro**: Tous les textes utilisent TextMeshProUGUI (pas Text legacy)
2. **Layout Groups**: Les Content Size Fitters et Layout Groups sont essentiels pour le responsive
3. **Sprites 9-Sliced**: Permettent de redimensionner sans déformation
4. **Profile Colors**: Les composants s'adaptent automatiquement au profil actif
5. **Animations**: Les transitions sont fluides (0.3-0.5s) avec SmoothStep
6. **Accessibility**: Tailles de police lisibles (min 14px), contrastes suffisants

---

Prochaine étape: Créer les prefabs SessionItem et DayIndicator pour le FamilyDashboard!
