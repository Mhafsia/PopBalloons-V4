# Structure des Données JSON - PopBalloons

## 📄 Vue d'ensemble

Les données de jeu sont sauvegardées au format JSON via `DataManager.cs`. Ce document décrit la structure complète des fichiers JSON générés.

---

## 🗂️ Structure Racine : `DatasCollection`

```json
{
  "datasList": [
    {
      "username": "string",
      "dateTime": "string (ISO 8601)",
      "levelDatas": { /* Voir LevelDatas */ }
    }
  ]
}
```

### Propriétés
- **`datasList`** : Tableau de sessions de jeu (une entrée par niveau joué)
  - **`username`** : Nom du joueur/profil
  - **`dateTime`** : Date et heure de début (format `yyyy-MM-ddTHH:mm:ss.fff`)
  - **`levelDatas`** : Données détaillées du niveau

---

## 📊 `LevelDatas` - Données d'un Niveau

```json
{
  "mode": "MOBILITY | COGNITIVE | FREEPLAY",
  "name": "string",
  "score": 0,
  "cognitiveDatas": {
    "correctBalloons": 0,
    "wrongBalloons": 0
  },
  "Waves": [ /* Voir Waves */ ],
  "userDatas": [ /* Voir UserDatas */ ],
  "listBalloonDatas": [ /* Voir BalloonDatas */ ],
  "listGazeItemDatas": [ /* Voir GazeItemDatas */ ],
  "listGazeDatas": [ /* Voir GazeDatas */ ]
}
```

### Propriétés
| Champ | Type | Description |
|-------|------|-------------|
| `mode` | string | Type de jeu : `"MOBILITY"`, `"COGNITIVE"`, `"FREEPLAY"` |
| `name` | string | Nom/identifiant du niveau |
| `score` | int | Score total obtenu |
| `cognitiveDatas` | CognitiveDatas | Données spécifiques au mode cognitif (**null** en mode MOBILITY) |
| `Waves` | Waves[] | Configuration des vagues de ballons |
| `userDatas` | UserDatas[] | Données de tracking utilisateur (position tête, fréquence cardiaque) |
| `listBalloonDatas` | BalloonDatas[] | Données de chaque ballon (spawn, destruction, score) |
| `listGazeItemDatas` | GazeItemDatas[] | Données de regard sur objets spécifiques |
| `listGazeDatas` | GazeDatas[] | Données brutes de eye-tracking |

---

## 🎯 `CognitiveDatas` - Données Mode Cognitif

```json
{
  "correctBalloons": 0,
  "wrongBalloons": 0
}
```

### Propriétés
- **`correctBalloons`** : Nombre de ballons corrects éclatés
- **`wrongBalloons`** : Nombre de ballons incorrects éclatés

> ⚠️ **Note** : Ce champ est `null` pour le mode MOBILITY et FREEPLAY

---

## 🌊 `Waves` - Configuration des Vagues

```json
{
  "intendedColor": "Red",
  "nbOption": 3,
  "balloonsOptions": [
    {
      "color": "Red",
      "id": 0,
      "balloonPosition": {
        "x": 0.5,
        "y": 1.2,
        "z": 2.0
      }
    }
  ]
}
```

### Propriétés
| Champ | Type | Description |
|-------|------|-------------|
| `intendedColor` | string | Couleur attendue (pour mode cognitif) |
| `nbOption` | int | Nombre de ballons dans cette vague |
| `balloonsOptions` | Options[] | Liste des ballons avec position et couleur |

### `Options` (Ballon dans une vague)
- **`color`** : Couleur du ballon (`"Red"`, `"Blue"`, `"Green"`, `"Yellow"`, `"Purple"`)
- **`id`** : Identifiant unique du ballon
- **`balloonPosition`** : Position Vector3 initiale

---

## 👤 `UserDatas` - Données Utilisateur (Tracking)

```json
{
  "headPos": {
    "x": 0.0,
    "y": 1.6,
    "z": 0.0
  },
  "headRotationY": 45.0,
  "BPM": 72.5,
  "timeStamp": "2025-11-16T14:30:05.123"
}
```

### Propriétés
| Champ | Type | Description |
|-------|------|-------------|
| `headPos` | Vector3 | Position de la tête (x, y, z) |
| `headRotationY` | float | Rotation Y de la tête (en degrés) |
| `BPM` | float | Battements par minute (fréquence cardiaque) |
| `timeStamp` | string | Moment de l'enregistrement (ISO 8601) |

---

## 🎈 `BalloonDatas` - Données d'un Ballon

```json
{
  "timeOfSpawn": "2025-11-16T14:30:01.000",
  "timeOfDestroy": "2025-11-16T14:30:03.500",
  "lifeTime": 2.5,
  "balloonPointGain": 35.0,
  "balloonWasDestroyByUser": true,
  "balloonTimout": false,
  "poppedColor": "Red",
  "intendedColor": "Red",
  "distance": 0.3,
  "balloonInitialPosition": {
    "x": 0.5,
    "y": 1.2,
    "z": 2.0
  }
}
```

### Propriétés
| Champ | Type | Description |
|-------|------|-------------|
| `timeOfSpawn` | string | Moment d'apparition (ISO 8601) |
| `timeOfDestroy` | string | Moment de destruction (ISO 8601) |
| `lifeTime` | float | Durée de vie en secondes |
| `balloonPointGain` | float | Points gagnés/perdus |
| `balloonWasDestroyByUser` | bool | `true` si éclaté par l'utilisateur |
| `balloonTimout` | bool | `true` si timeout (pas éclaté) |
| `poppedColor` | string | Couleur du ballon éclaté |
| `intendedColor` | string | Couleur attendue (mode cognitif) |
| `distance` | float | Distance parcourue depuis le spawn |
| `balloonInitialPosition` | Vector3 | Position initiale (x, y, z) |

---

## 👁️ `GazeItemDatas` - Regard sur Objets

```json
{
  "objectType": "Balloon",
  "timeOfLook": "2025-11-16T14:30:02.000",
  "duration": 0.8,
  "targetName": "RedBalloon_01"
}
```

### Propriétés
| Champ | Type | Description |
|-------|------|-------------|
| `objectType` | string | Type d'objet regardé (`"Balloon"`, etc.) |
| `timeOfLook` | string | Début du regard (ISO 8601) |
| `duration` | float | Durée du regard en secondes |
| `targetName` | string | Nom de l'objet ciblé |

---

## 👀 `GazeDatas` - Eye-Tracking Brut

```json
{
  "targetName": "RedBalloon_01",
  "timeStamp": "2025-11-16T14:30:02.100",
  "targetIsValid": true,
  "isCalibrationValid": true,
  "origin": {
    "x": 0.0,
    "y": 1.6,
    "z": 0.0
  },
  "direction": {
    "x": 0.24,
    "y": -0.19,
    "z": 0.95
  },
  "eyeGazeTarget": {
    "x": 0.5,
    "y": 1.2,
    "z": 2.0
  }
}
```

### Propriétés
| Champ | Type | Description |
|-------|------|-------------|
| `targetName` | string | Nom de l'objet ciblé |
| `timeStamp` | string | Moment de l'enregistrement (ISO 8601) |
| `targetIsValid` | bool | Cible valide détectée |
| `isCalibrationValid` | bool | Calibration eye-tracker valide |
| `origin` | Vector3 | Point d'origine du rayon (position œil) |
| `direction` | Vector3 | Direction du rayon (vecteur normalisé) |
| `eyeGazeTarget` | Vector3 | Point de collision du regard |

---

## 📝 Exemple Complet

```json
{
  "datasList": [
    {
      "username": "Marie",
      "dateTime": "2025-11-16T14:30:00.000",
      "levelDatas": {
        "mode": "COGNITIVE",
        "name": "Niveau_1",
        "score": 450,
        "cognitiveDatas": {
          "correctBalloons": 8,
          "wrongBalloons": 2
        },
        "Waves": [
          {
            "intendedColor": "Red",
            "nbOption": 3,
            "balloonsOptions": [
              {
                "color": "Red",
                "id": 0,
                "balloonPosition": { "x": 0.5, "y": 1.2, "z": 2.0 }
              },
              {
                "color": "Blue",
                "id": 1,
                "balloonPosition": { "x": -0.5, "y": 1.2, "z": 2.0 }
              },
              {
                "color": "Green",
                "id": 2,
                "balloonPosition": { "x": 0.0, "y": 1.8, "z": 2.0 }
              }
            ]
          }
        ],
        "userDatas": [
          {
            "headPos": { "x": 0.0, "y": 1.6, "z": 0.0 },
            "headRotationY": 45.0,
            "BPM": 72.5,
            "timeStamp": "2025-11-16T14:30:05.123"
          },
          {
            "headPos": { "x": 0.1, "y": 1.58, "z": 0.05 },
            "headRotationY": 48.0,
            "BPM": 74.0,
            "timeStamp": "2025-11-16T14:30:06.123"
          }
        ],
        "listBalloonDatas": [
          {
            "timeOfSpawn": "2025-11-16T14:30:01.000",
            "timeOfDestroy": "2025-11-16T14:30:03.500",
            "lifeTime": 2.5,
            "balloonPointGain": 35.0,
            "balloonWasDestroyByUser": true,
            "balloonTimout": false,
            "poppedColor": "Red",
            "intendedColor": "Red",
            "distance": 0.3,
            "balloonInitialPosition": { "x": 0.5, "y": 1.2, "z": 2.0 }
          },
          {
            "timeOfSpawn": "2025-11-16T14:30:04.000",
            "timeOfDestroy": "2025-11-16T14:30:05.200",
            "lifeTime": 1.2,
            "balloonPointGain": -5.0,
            "balloonWasDestroyByUser": true,
            "balloonTimout": false,
            "poppedColor": "Blue",
            "intendedColor": "Red",
            "distance": 0.15,
            "balloonInitialPosition": { "x": -0.5, "y": 1.2, "z": 2.0 }
          }
        ],
        "listGazeItemDatas": [
          {
            "objectType": "Balloon",
            "timeOfLook": "2025-11-16T14:30:02.000",
            "duration": 0.8,
            "targetName": "RedBalloon_01"
          },
          {
            "objectType": "Balloon",
            "timeOfLook": "2025-11-16T14:30:04.500",
            "duration": 0.3,
            "targetName": "BlueBalloon_02"
          }
        ],
        "listGazeDatas": [
          {
            "targetName": "RedBalloon_01",
            "timeStamp": "2025-11-16T14:30:02.100",
            "targetIsValid": true,
            "isCalibrationValid": true,
            "origin": { "x": 0.0, "y": 1.6, "z": 0.0 },
            "direction": { "x": 0.24, "y": -0.19, "z": 0.95 },
            "eyeGazeTarget": { "x": 0.5, "y": 1.2, "z": 2.0 }
          },
          {
            "targetName": "BlueBalloon_02",
            "timeStamp": "2025-11-16T14:30:04.600",
            "targetIsValid": true,
            "isCalibrationValid": true,
            "origin": { "x": 0.1, "y": 1.58, "z": 0.05 },
            "direction": { "x": -0.28, "y": -0.18, "z": 0.94 },
            "eyeGazeTarget": { "x": -0.5, "y": 1.2, "z": 2.0 }
          }
        ]
      }
    },
    {
      "username": "Marie",
      "dateTime": "2025-11-16T14:35:00.000",
      "levelDatas": {
        "mode": "MOBILITY",
        "name": "Niveau_2",
        "score": 520,
        "cognitiveDatas": null,
        "Waves": [],
        "userDatas": [],
        "listBalloonDatas": [],
        "listGazeItemDatas": [],
        "listGazeDatas": []
      }
    }
  ]
}
```

---

## 📌 Notes Importantes

### Modes de Jeu
| Mode | Description | Sauvegarde |
|------|-------------|-----------|
| `MOBILITY` | Motricité | ✅ Oui |
| `COGNITIVE` | Cognitif (sélection couleur) | ✅ Oui |
| `FREEPLAY` | Jeu libre | ❌ **Non** |

> ⚠️ **Important** : Le mode FREEPLAY **ne sauvegarde pas de données** (voir `DataManager.HandleLevelEnd()`)

### Format de Dates (ISO 8601)
Tous les champs temporels utilisent le format : `yyyy-MM-ddTHH:mm:ss.fff`

Exemple : `2025-11-16T14:30:05.123`

### Couleurs de Ballons
Valeurs possibles :
- `"Red"` (Rouge)
- `"Blue"` (Bleu)
- `"Green"` (Vert)
- `"Yellow"` (Jaune)
- `"Purple"` (Violet)

### Vector3 (Position/Direction)
Format Unity : 
```json
{
  "x": 0.0,
  "y": 0.0,
  "z": 0.0
}
```

### Fréquence d'Enregistrement
- **`UserDatas`** : Enregistré périodiquement pendant le jeu
- **`BalloonDatas`** : Un enregistrement par ballon (spawn + destroy)
- **`GazeDatas`** : Haute fréquence (eye-tracking continu)
- **`GazeItemDatas`** : Enregistré quand regard fixe un objet

---

## 🔧 Implémentation Technique

### Fichier Source
`Assets/Actimage.PopBalloons/Scripts/Data/DataManager.cs`

### Méthode de Sauvegarde
```csharp
// Windows UWP
JsonUtility.ToJson(datasCollection, true)

// Autres plateformes
JsonUtility.ToJson(datasCollection, true)
```

### Classes C# Correspondantes
```csharp
[Serializable] public class DatasCollection
[Serializable] public class Datas
[Serializable] public class LevelDatas
[Serializable] public class CognitiveDatas
[Serializable] public class Waves
[Serializable] public class Options
[Serializable] public class UserDatas
[Serializable] public class BalloonDatas
[Serializable] public class GazeItemDatas
[Serializable] public class GazeDatas
```

---

## 📂 Emplacement du Fichier

### Windows UWP (HoloLens)
```
LocalState/PopBalloonsData.json
```

### Autres Plateformes
```
Application.persistentDataPath/PopBalloonsData.json
```

---

## 🔄 Cycle de Vie des Données

1. **Début de niveau** : `HandleLevelStart()` initialise un nouvel objet `Datas`
2. **Pendant le jeu** :
   - `AddUsersDatas()` : Tracking utilisateur continu
   - `AddBalloonsDatas()` : Chaque ballon éclaté
   - `AddWavesData()` : Configuration des vagues
   - Gaze tracking enregistré automatiquement
3. **Fin de niveau** : `HandleLevelEnd()` sauvegarde tout dans le JSON
4. **Sauvegarde** : `SaveDatas()` écrit le fichier sur disque

---

## 📊 Utilisation des Données

### Analyses Possibles
- **Performance** : Score, temps de réaction, précision
- **Engagement** : Durée de regard, focus sur ballons
- **Motricité** : Distance parcourue, mouvements de tête
- **Progression** : Comparaison entre sessions
- **Santé** : Monitoring BPM pendant l'effort

### Export
Les données JSON peuvent être exportées pour analyse dans :
- Python (pandas, matplotlib)
- R (data analysis)
- Excel/CSV (conversion nécessaire)
- Power BI / Tableau (visualisation)

---

## 🛠️ Validation du JSON

### Schema JSON (optionnel)
Un schéma JSON peut être créé pour validation automatique des données.

### Outils de Validation
- [JSONLint](https://jsonlint.com/)
- [JSON Schema Validator](https://www.jsonschemavalidator.net/)

---

**Version** : 1.0  
**Dernière mise à jour** : 18 novembre 2025  
**Projet** : PopBalloons v3
