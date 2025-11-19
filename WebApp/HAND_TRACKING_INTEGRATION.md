# 🤲 Hand Tracking - Intégré à l'Application Web

Le système hand tracking a été **intégré directement dans l'application web principale** sous forme d'onglet.

## 📂 Fichiers Modifiés

### ✅ WebApp/index.html
- **Ajout système d'onglets** : `<div class="tabs-navigation">`
- **Onglet "Jeu"** : Contient tous les contrôles de jeu existants
- **Onglet "Hand Tracking"** : Nouveau contenu pour le hand tracking
  - Boutons de contrôle (Démarrer/Arrêter/Capturer)
  - Boutons d'enregistrement (Démarrer/Exporter CSV)
  - Statistiques (Frames, Timestamp, FPS, Enregistrées)
  - Panneaux pour Main Gauche et Main Droite
  - Affichage des 26 joints par main

### ✅ WebApp/style.css
- **Styles des onglets** (`.tabs-navigation`, `.tab-btn`, `.tab-content`)
- **Styles hand tracking** :
  - `.handtracking-controls` : Contrôles de tracking
  - `.handtracking-stats` : Grid de statistiques
  - `.hands-container` : Grid 2 colonnes pour les mains
  - `.hand-panel` : Panneau pour chaque main
  - `.joint-list` : Liste scrollable des articulations
  - `.recording-indicator` : Indicateur d'enregistrement avec animation

### ✅ WebApp/app.js
- **Fonction `switchTab()`** : Navigation entre onglets
- **Fonctions hand tracking** :
  - `startHandTracking()` : Envoie commande WebSocket
  - `stopHandTracking()` : Arrête le streaming
  - `captureHandFrame()` : Capture une frame unique
  - `startHandRecording()` : Démarre l'enregistrement
  - `stopHandRecording()` : Arrête et exporte en CSV
  - `handleHandTrackingData()` : Traite les données reçues
  - `updateHandDisplay()` : Met à jour l'affichage des mains
  - `exportHandTrackingCSV()` : Exporte les données en CSV

## 🎯 Utilisation

1. **Ouvrir l'application** : `WebApp/index.html`
2. **Se connecter au HoloLens** (comme d'habitude)
3. **Cliquer sur l'onglet "🤲 Hand Tracking"**
4. **Utiliser les contrôles** :
   - `▶️ Démarrer Tracking` : Stream continu
   - `⏸️ Arrêter Tracking` : Stop du stream
   - `📸 Capturer 1 Frame` : Snapshot unique
   - `⏺️ Démarrer Enregistrement` : Enregistrer les données
   - `⏹️ Arrêter & Exporter CSV` : Sauvegarder en fichier

## 📊 Affichage

### Statistiques
- **Frames Reçues** : Nombre total de frames
- **Timestamp** : Temps Unity de la dernière frame
- **FPS** : Fréquence de réception
- **Enregistrées** : Frames sauvegardées pendant l'enregistrement

### Mains
Chaque main affiche :
- ✅ **Status** : Trackée / Non trackée
- 📍 **26 joints** avec :
  - Nom de l'articulation (ex: Palm, IndexTip)
  - Position 3D (x, y, z)
  - Rotation Quaternion (x, y, z, w)

## 📥 Export CSV

Format des données exportées :
```csv
timestamp_ms,hand,joint,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z,rot_w
123456,Left,Palm,0.123,0.456,0.789,0.0,0.0,0.0,1.0
123456,Left,Wrist,0.100,0.400,0.750,0.1,0.2,0.3,0.9
...
```

## 🔧 Configuration Unity

Dans `WebSocketServer` (Inspector) :
```
Hand Tracking
├─ Send Hand Tracking Data: ☐ (Optionnel pour auto-stream)
└─ Hand Tracking Interval: 0.1 (10 Hz par défaut)
```

## 🎨 Interface

L'onglet Hand Tracking a été stylisé pour correspondre au reste de l'application :
- **Mêmes couleurs** : Gradient violet (#667eea → #764ba2)
- **Mêmes boutons** : Action-btn avec icônes
- **Mêmes cards** : Stats avec ombres et border-radius
- **Responsive** : Grid adaptatif pour les mains

## 📚 Documentation Complète

- **HAND_TRACKING_GUIDE.md** : Guide détaillé du hand tracking
- **WEBSOCKET_API.md** : Documentation API WebSocket complète

---

✅ **Le hand tracking est maintenant parfaitement intégré dans l'application principale !**

Aucun fichier séparé nécessaire - tout est dans l'onglet "🤲 Hand Tracking" de `WebApp/index.html`.
