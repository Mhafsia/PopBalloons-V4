# 📅 SessionItem & DayIndicator - Guide de Configuration

Configuration détaillée des deux composants spécifiques au FamilyDashboard.

---

## 📋 SessionItem Component

Item de liste pour afficher une séance programmée avec son statut.

### Hiérarchie
```
SessionItem (RectTransform + Image + Button + SessionItem.cs)
├── StatusIndicator (Image - barre colorée à gauche)
├── TimeSection (Vertical Layout)
│   ├── TimeText (TextMeshProUGUI) - "14:00"
│   └── StatusIcon (TextMeshProUGUI) - "✓" ou "○"
├── Content (Vertical Layout)
│   ├── GameName (TextMeshProUGUI) - "PopBalloons"
│   └── ScoreText (TextMeshProUGUI) - "450 pts"
└── Arrow (TextMeshProUGUI) - "→"
```

### Configuration SessionItem (Root)
- **RectTransform**: Width=280px, Height=80px
- **Image** (Background):
  - Color: White (sera modifié par script)
  - Sprite: UI/RoundedRect
  - Image Type: Sliced
- **Button**:
  - Transition: Color Tint
  - Normal: Transparent
  - Highlighted: (0, 0, 0, 25) - gris clair
  - Pressed: (0, 0, 0, 50)
  - Disabled: (200, 200, 200, 128)
- **Horizontal Layout Group**:
  - Padding: 0 (left), 15 (right/top/bottom)
  - Spacing: 12
  - Child Alignment: Middle Left
  - Child Force Expand: Height ✓

### Configuration StatusIndicator
- **Width**: 4px (barre mince à gauche)
- **Height**: Stretch (toute la hauteur)
- **Anchor**: Left stretch
- **Offset Left**: 0, **Right**: 4
- **Image**:
  - Color: #7ED321 (vert, sera modifié par script)
  - Image Type: Simple
  - Raycast Target: ✗ (pas cliquable)

### Configuration TimeSection
- **Width**: 60px, **Height**: Stretch
- **Vertical Layout Group**:
  - Spacing: 4
  - Child Alignment: Middle Center
  - Child Force Expand: Width ✓

**TimeText**:
- **Font Size**: 16
- **Font Style**: Bold
- **Color**: #2C3E50 (texte sombre)
- **Alignment**: Middle Center
- **Text**: "14:00"

**StatusIcon**:
- **Font Size**: 18
- **Color**: #7ED321 (sera modifié par script)
- **Alignment**: Middle Center
- **Text**: "✓"

### Configuration Content
- **Width**: Flexible (Layout Element: Flexible Width = 1)
- **Height**: Stretch
- **Vertical Layout Group**:
  - Spacing: 4
  - Child Alignment: Middle Left
  - Child Force Expand: Width ✓

**GameName**:
- **Font Size**: 16
- **Font Style**: Bold
- **Color**: #2C3E50
- **Alignment**: Middle Left
- **Text**: "PopBalloons - Motricité"

**ScoreText**:
- **Font Size**: 12
- **Color**: #7F8C8D (gris moyen)
- **Alignment**: Middle Left
- **Text**: "450 pts"
- **Active**: false (visible seulement si complété)

### Configuration Arrow
- **Width**: 24px, **Height**: 24px
- **Font Size**: 18
- **Color**: (0, 0, 0, 50) - gris très clair
- **Alignment**: Middle Center
- **Text**: "→"

### Script SessionItem References
Assigner dans l'inspecteur:
- Time Text → TimeText TextMeshProUGUI
- Game Name Text → GameName TextMeshProUGUI
- Status Icon Text → StatusIcon TextMeshProUGUI
- Score Text → ScoreText TextMeshProUGUI
- Background Image → Background Image
- Status Indicator → StatusIndicator Image
- Item Button → Button component
- Completed Color → #7ED321 (vert)
- Pending Color → #F5A623 (orange)
- In Progress Color → #4A90E2 (bleu)
- Skipped Color → #B0B0B0 (gris)

### Utilisation en Code
```csharp
// Instanciation
SessionItem item = Instantiate(sessionItemPrefab, container, false).GetComponent<SessionItem>();

// Configuration
item.Setup(sessionData, (session) => {
    Debug.Log($"Clic sur: {session.GameName}");
    // Lancer la séance, afficher détails, etc.
});

// Marquage comme complété
item.MarkAsCompleted(score: 450, duration: 18.5f);

// Rafraîchissement
item.Refresh();
```

### États Visuels

**Complété** (✓):
- StatusIndicator: Vert #7ED321
- Background: Vert très clair (alpha 10%)
- TimeText: Heure de complétion
- StatusIcon: ✓ vert
- ScoreText: Visible avec points

**En cours** (⟳):
- StatusIndicator: Bleu #4A90E2
- Background: Bleu très clair (alpha 10%)
- StatusIcon: ⟳ bleu
- ScoreText: "En cours..."

**Pending** (○):
- StatusIndicator: Orange #F5A623
- Background: Blanc
- StatusIcon: ○ orange
- ScoreText: Masqué

**Skipped** (✕):
- StatusIndicator: Gris #B0B0B0
- Background: Gris clair
- StatusIcon: ✕ gris
- ScoreText: Masqué

---

## 📆 DayIndicator Component

Indicateur de jour pour le calendrier hebdomadaire (L M M J V S D).

### Hiérarchie
```
DayIndicator (RectTransform + DayIndicator.cs)
├── BackgroundCircle (Image - cercle de fond)
├── DayLetter (TextMeshProUGUI) - "L"
└── StatusDot (Image - point vert si complété)
```

### Configuration DayIndicator (Root)
- **RectTransform**: Width=40px, Height=60px
- **Vertical Layout Group**:
  - Spacing: 4
  - Child Alignment: Upper Center
  - Child Force Expand: Width ✗, Height ✗

### Configuration BackgroundCircle
- **Width/Height**: 40x40px (cercle)
- **Image**:
  - Sprite: UI/Circle
  - Color: #F5F5F5 (gris très clair, sera modifié par script)
  - Image Type: Simple
  - Preserve Aspect: ✓
  - Raycast Target: ✗

### Configuration DayLetter
- **Width**: 40px, **Height**: 40px
- **Position**: Centré sur BackgroundCircle (Z-index au-dessus)
- **Font Size**: 16
- **Font Style**: Bold
- **Color**: #2C3E50 (sera modifié par script)
- **Alignment**: Middle Center
- **Text**: "L"

### Configuration StatusDot
- **Width/Height**: 8x8px (petit point)
- **Position**: Sous le cercle
- **Image**:
  - Sprite: UI/Circle
  - Color: #7ED321 (vert)
  - Image Type: Simple
  - Preserve Aspect: ✓
  - Raycast Target: ✗
- **Active**: false (visible seulement si complété)

### Script DayIndicator References
- Day Letter Text → DayLetter TextMeshProUGUI
- Status Dot Image → StatusDot Image
- Background Circle → BackgroundCircle Image
- Completed Color → #7ED321 (vert)
- Today Color → #4A90E2 (bleu)
- Future Color → #CCCCCC (gris clair)
- Missed Color → #DC3545 (rouge)

### Utilisation en Code
```csharp
// Instanciation
DayIndicator indicator = Instantiate(dayIndicatorPrefab, calendarContainer, false).GetComponent<DayIndicator>();

// Configuration avec DayOfWeek
indicator.Setup(
    day: DayOfWeek.Monday,
    completed: true,
    today: false,
    past: true
);

// Configuration avec DateTime
indicator.Setup(date: DateTime.Today, completed: false);

// Marquage comme complété avec animation
indicator.SetCompleted(true);
indicator.AnimateCompletion();

// Méthodes statiques utiles
string letter = DayIndicator.GetDayLetterStatic(DayOfWeek.Monday); // "L"
string name = DayIndicator.GetDayNameFr(DayOfWeek.Monday); // "Lundi"
```

### États Visuels

**Aujourd'hui** (isToday = true):
- BackgroundCircle: Bleu #4A90E2 (plein)
- DayLetter: Blanc, Bold
- StatusDot: Masqué (ou vert si complété)

**Complété** (isCompleted = true):
- BackgroundCircle: Vert très clair (alpha 20%)
- DayLetter: Texte sombre #2C3E50, Bold
- StatusDot: Visible, Vert #7ED321

**Manqué** (isPast = true, isCompleted = false):
- BackgroundCircle: Rouge très clair (alpha 10%)
- DayLetter: Gris #808080
- StatusDot: Masqué

**Futur** (isPast = false, isCompleted = false):
- BackgroundCircle: Gris très clair #F5F5F5
- DayLetter: Gris #808080
- StatusDot: Masqué

---

## 🎯 Intégration dans FamilyDashboard

### Configuration des Containers

**Sessions Container** (pour SessionItem):
```
SessionsContainer (Vertical Layout Group)
├── Padding: 10 (all)
├── Spacing: 12
├── Child Force Expand: Width ✓, Height ✗
├── Child Control Size: Height ✓
```

**Weekly Calendar Container** (pour DayIndicator):
```
WeeklyCalendar (Horizontal Layout Group)
├── Padding: 5 (all)
├── Spacing: 8
├── Child Alignment: Middle Center
├── Child Force Expand: Width ✗, Height ✗
```

### Code FamilyDashboard

**Mise à jour des séances d'aujourd'hui**:
```csharp
private void UpdateTodaysSessions()
{
    // Nettoyer les items existants
    foreach (Transform child in sessionsContainer)
    {
        Destroy(child.gameObject);
    }

    // Récupérer les séances du jour
    List<SessionData> todaySessions = DataManager.Instance.TodaysSessions;

    // Créer un item pour chaque séance
    foreach (SessionData session in todaySessions)
    {
        SessionItem item = Instantiate(sessionItemPrefab, sessionsContainer, false)
            .GetComponent<SessionItem>();

        item.Setup(session, OnSessionClicked);
    }

    // Message si aucune séance
    if (todaySessions.Count == 0)
    {
        TextMeshProUGUI emptyText = new GameObject("EmptyText").AddComponent<TextMeshProUGUI>();
        emptyText.transform.SetParent(sessionsContainer, false);
        emptyText.text = "Aucune séance programmée aujourd'hui";
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyText.fontSize = 14;
        emptyText.color = new Color(0.5f, 0.5f, 0.5f);
    }
}

private void OnSessionClicked(SessionData session)
{
    Debug.Log($"Séance cliquée: {session.GameName}");
    
    // Afficher détails, lancer séance, etc.
    if (session.Status == SessionStatus.Pending)
    {
        // Proposer de lancer
        StartSession(session);
    }
    else if (session.Status == SessionStatus.Completed)
    {
        // Afficher résultats
        ShowSessionResults(session);
    }
}
```

**Mise à jour du calendrier hebdomadaire**:
```csharp
private void UpdateWeeklyCalendar()
{
    // Nettoyer
    foreach (Transform child in weeklyCalendarContainer)
    {
        Destroy(child.gameObject);
    }

    // Récupérer la progression hebdomadaire
    WeeklyProgress weeklyData = DataManager.Instance.GetWeeklyProgress();
    
    // Obtenir le début de la semaine (lundi)
    DateTime startOfWeek = weeklyData.WeekStart;

    // Créer 7 indicateurs (L à D)
    for (int i = 0; i < 7; i++)
    {
        DateTime day = startOfWeek.AddDays(i);
        bool isCompleted = weeklyData.DailyCompletion[i];

        DayIndicator indicator = Instantiate(dayIndicatorPrefab, weeklyCalendarContainer, false)
            .GetComponent<DayIndicator>();

        indicator.Setup(day, isCompleted);
    }
}
```

---

## ✅ Checklist de Configuration

### SessionItem Prefab
- [ ] Hiérarchie créée (StatusIndicator/TimeSection/Content/Arrow)
- [ ] Layout Groups configurés
- [ ] Button component ajouté au root
- [ ] StatusIndicator positionné à gauche (4px width)
- [ ] Sprites assignés (RoundedRect pour background)
- [ ] Script references assignées (8 champs)
- [ ] Couleurs configurées (Completed/Pending/InProgress/Skipped)
- [ ] ScoreText désactivé par défaut
- [ ] Prefab sauvegardé dans `Assets/TND_Platform/Prefabs/UI/Components/`

### DayIndicator Prefab
- [ ] Hiérarchie créée (BackgroundCircle/DayLetter/StatusDot)
- [ ] Layout Group configuré (vertical, upper center)
- [ ] Cercle de fond avec sprite Circle
- [ ] DayLetter centré sur le cercle
- [ ] StatusDot positionné sous le cercle
- [ ] StatusDot désactivé par défaut
- [ ] Script references assignées (3 champs)
- [ ] Couleurs configurées (Completed/Today/Future/Missed)
- [ ] Prefab sauvegardé

### FamilyDashboard References
- [ ] SessionItem Prefab assigné
- [ ] DayIndicator Prefab assigné
- [ ] SessionsContainer référencé
- [ ] WeeklyCalendarContainer référencé
- [ ] UpdateTodaysSessions() appelé dans RefreshUI()
- [ ] UpdateWeeklyCalendar() appelé dans RefreshUI()

---

## 🧪 Tests

### Test SessionItem
```csharp
// Créer une session test
SessionData testSession = new SessionData
{
    SessionId = "test_001",
    GameName = "PopBalloons - Motricité",
    ScheduledTime = DateTime.Now.AddHours(2),
    Status = SessionStatus.Pending
};

// Instancier l'item
SessionItem item = Instantiate(sessionItemPrefab).GetComponent<SessionItem>();
item.Setup(testSession, (s) => Debug.Log($"Clicked: {s.GameName}"));

// Test complétion
yield return new WaitForSeconds(2);
item.MarkAsCompleted(score: 450, duration: 18.5f);
```

### Test DayIndicator
```csharp
// Créer un indicateur pour aujourd'hui
DayIndicator today = Instantiate(dayIndicatorPrefab).GetComponent<DayIndicator>();
today.Setup(DateTime.Today, completed: false);

// Créer un indicateur complété
DayIndicator completed = Instantiate(dayIndicatorPrefab).GetComponent<DayIndicator>();
completed.Setup(DateTime.Today.AddDays(-1), completed: true);

// Test animation
yield return new WaitForSeconds(2);
today.AnimateCompletion();
```

### Test Calendrier Complet
```csharp
// Générer une semaine
DateTime monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
bool[] completions = { true, true, true, false, false, false, false };

for (int i = 0; i < 7; i++)
{
    DayIndicator day = Instantiate(dayIndicatorPrefab, calendarContainer, false)
        .GetComponent<DayIndicator>();
    
    day.Setup(monday.AddDays(i), completions[i]);
}
```

---

## 📐 Dimensions Recommandées

### SessionItem
- **Width**: 280-320px (selon container)
- **Height**: 80px (fixe)
- **StatusIndicator**: 4px width
- **TimeSection**: 60px width
- **Arrow**: 24px

### DayIndicator
- **Root**: 40x60px
- **BackgroundCircle**: 40x40px
- **DayLetter**: Centré, 16pt Bold
- **StatusDot**: 8x8px

### Spacing
- **SessionsContainer spacing**: 12px entre items
- **WeeklyCalendar spacing**: 8px entre jours
- **Padding**: 10-15px autour des containers

---

## 🎨 Palette de Couleurs

### SessionItem
```csharp
Completed:   #7ED321 (vert)
InProgress:  #4A90E2 (bleu)
Pending:     #F5A623 (orange)
Skipped:     #B0B0B0 (gris)
Background:  #FFFFFF (blanc)
Text:        #2C3E50 (sombre)
```

### DayIndicator
```csharp
Completed:   #7ED321 (vert)
Today:       #4A90E2 (bleu)
Future:      #CCCCCC (gris clair)
Missed:      #DC3545 (rouge)
Text:        #2C3E50 (sombre)
TextFuture:  #808080 (gris moyen)
```

---

Avec SessionItem et DayIndicator, le FamilyDashboard est maintenant complet! 🎉

Prochaine étape: Tester l'ensemble dans Unity ou créer les autres pages Family (Games, Progress, Rewards).

