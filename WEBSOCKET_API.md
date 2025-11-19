# 📡 WebSocket API - PopBalloons HoloLens

Documentation complète de toutes les commandes WebSocket disponibles.

## 🔌 Connexion

**URL:** `ws://<IP_HOLOLENS>:8080`  
**Exemple:** `ws://192.168.1.100:8080`

```javascript
const ws = new WebSocket('ws://192.168.1.100:8080');

ws.onopen = () => {
    console.log('✅ Connecté au HoloLens');
};

ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    console.log('Message reçu:', message);
};
```

---

## 🎮 Commandes de Jeu

### 1. Démarrer une partie

**Commande:**
```json
{
  "type": "startGame",
  "data": {
    "gameType": "COGNITIVE",
    "level": 1,
    "freePlaySettings": {
      "spawnInterval": 1.5,
      "maxSimultaneous": 10
    }
  }
}
```

**Paramètres:**
- `gameType`: `"COGNITIVE"`, `"MOBILITY"`, ou `"FREEPLAY"`
- `level`: Numéro du niveau (1-5 pour Cognitive/Mobility)
- `freePlaySettings`: (Optionnel, uniquement pour FreePlay)
  - `spawnInterval`: Intervalle entre les ballons (secondes)
  - `maxSimultaneous`: Nombre max de ballons simultanés

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Started COGNITIVE level 1"
  }
}
```

---

### 2. Retour au menu principal

**Commande:**
```json
{ "type": "goHome" }
```

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Returned to home menu"
  }
}
```

---

### 3. Quitter la partie en cours

**Commande:**
```json
{ "type": "quitGame" }
```

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Game stopped"
  }
}
```

---

## 👤 Commandes de Profil

### 4. Obtenir le profil actuel

**Commande:**
```json
{ "type": "getProfile" }
```

**Réponse:**
```json
{
  "type": "profile",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "username": "JohnDoe",
    "avatar": {
      "colorOption": 0,
      "eyeOption": 1,
      "accessoryOption": 2
    },
    "levels": [
      {
        "name": "Cognitive_Level_1",
        "score": 1250
      },
      {
        "name": "Mobility_Level_1",
        "score": 980
      }
    ]
  }
}
```

---

### 5. Obtenir tous les profils

**Commande:**
```json
{ "type": "getProfiles" }
```

**Réponse:**
```json
{
  "type": "profilesList",
  "data": {
    "profiles": [
      {
        "id": "profile-id-1",
        "username": "Player1",
        "avatar": { "colorOption": 0, "eyeOption": 1, "accessoryOption": 2 }
      },
      {
        "id": "profile-id-2",
        "username": "Player2",
        "avatar": { "colorOption": 1, "eyeOption": 0, "accessoryOption": 1 }
      }
    ]
  }
}
```

---

### 6. Sélectionner un profil

**Commande:**
```json
{
  "type": "selectProfile",
  "data": {
    "profileId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  }
}
```

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Profile changed to JohnDoe"
  }
}
```

---

### 7. Créer un profil de debug

**Commande:**
```json
{ "type": "createDebugProfile" }
```

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Debug profile created with 10 levels"
  }
}
```

---

## 📹 Commandes Caméra

### 8. Obtenir l'URL du flux MJPEG

**Commande:**
```json
{ "type": "toggleCameraFeed" }
```

**Réponse:**
```json
{
  "type": "cameraFeedURL",
  "data": {
    "url": "http://192.168.1.100:8081/stream"
  }
}
```

**Utilisation du flux:**
```html
<img src="http://192.168.1.100:8081/stream" alt="HoloLens Camera">
```

---

## 🤲 Commandes Hand Tracking

### 9. Démarrer le streaming de hand tracking

**Commande:**
```json
{ "type": "startHandTracking" }
```

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Hand tracking started"
  }
}
```

**Puis vous recevrez automatiquement des messages:**
```json
{
  "type": "handTracking",
  "data": {
    "timestamp": 123.456,
    "timestampMs": 123456,
    "leftHand": {
      "handedness": "Left",
      "isTracked": true,
      "joints": [
        {
          "jointName": "Palm",
          "position": { "x": 0.123, "y": 0.456, "z": 0.789 },
          "rotation": { "x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0 }
        }
        // ... 25 autres joints
      ]
    },
    "rightHand": { /* ... */ }
  }
}
```

---

### 10. Arrêter le streaming de hand tracking

**Commande:**
```json
{ "type": "stopHandTracking" }
```

**Réponse:**
```json
{
  "type": "response",
  "data": {
    "message": "Hand tracking stopped"
  }
}
```

---

### 11. Capturer une frame unique de hand tracking

**Commande:**
```json
{ "type": "getHandTrackingFrame" }
```

**Réponse:** Une frame complète (même format que `startHandTracking`)

---

## 📊 Messages Automatiques

### Stats de jeu (pendant une partie)

**Fréquence:** Toutes les 1 seconde (par défaut)

```json
{
  "type": "stats",
  "data": {
    "score": 1250,
    "balloons": 42,
    "time": 120.5
  }
}
```

---

### Mise à jour de profil

**Trigger:** Quand un profil est modifié (scores, changement de profil)

```json
{
  "type": "profile",
  "data": {
    "id": "...",
    "username": "...",
    "avatar": { /* ... */ },
    "levels": [ /* ... */ ]
  }
}
```

---

### Hand Tracking (quand actif)

**Fréquence:** Configurable (0.1s par défaut = 10 Hz)

```json
{
  "type": "handTracking",
  "data": {
    "timestamp": 123.456,
    "timestampMs": 123456,
    "leftHand": { /* ... */ },
    "rightHand": { /* ... */ }
  }
}
```

---

## ❌ Messages d'Erreur

```json
{
  "type": "error",
  "data": {
    "message": "Description de l'erreur"
  }
}
```

**Erreurs courantes:**
- `"Unknown command: xxx"` - Commande non reconnue
- `"Profile not found"` - ID de profil invalide
- `"Error: xxx"` - Erreur générique

---

## 📝 Exemples Complets

### Exemple 1: Lancer une partie Cognitive

```javascript
const ws = new WebSocket('ws://192.168.1.100:8080');

ws.onopen = () => {
    // Démarrer le niveau 1 Cognitive
    ws.send(JSON.stringify({
        type: "startGame",
        data: {
            gameType: "COGNITIVE",
            level: 1
        }
    }));
};

ws.onmessage = (event) => {
    const msg = JSON.parse(event.data);
    
    if (msg.type === 'stats') {
        console.log(`Score: ${msg.data.score}, Balloons: ${msg.data.balloons}`);
    }
};
```

---

### Exemple 2: Gérer les profils

```javascript
// Obtenir tous les profils
ws.send(JSON.stringify({ type: "getProfiles" }));

// Sélectionner un profil spécifique
ws.send(JSON.stringify({
    type: "selectProfile",
    data: {
        profileId: "abc123..."
    }
}));

// Écouter les changements de profil
ws.onmessage = (event) => {
    const msg = JSON.parse(event.data);
    
    if (msg.type === 'profile') {
        console.log(`Profil actuel: ${msg.data.username}`);
        console.log(`Scores:`, msg.data.levels);
    }
};
```

---

### Exemple 3: FreePlay avec paramètres personnalisés

```javascript
ws.send(JSON.stringify({
    type: "startGame",
    data: {
        gameType: "FREEPLAY",
        level: 0,
        freePlaySettings: {
            spawnInterval: 2.0,    // Ballons toutes les 2 secondes
            maxSimultaneous: 15     // Max 15 ballons en même temps
        }
    }
}));
```

---

### Exemple 4: Enregistrer les données de hand tracking

```javascript
let recordedFrames = [];

// Démarrer le tracking
ws.send(JSON.stringify({ type: "startHandTracking" }));

// Collecter les données
ws.onmessage = (event) => {
    const msg = JSON.parse(event.data);
    
    if (msg.type === 'handTracking') {
        recordedFrames.push(msg.data);
        console.log(`Frames enregistrées: ${recordedFrames.length}`);
    }
};

// Arrêter après 10 secondes
setTimeout(() => {
    ws.send(JSON.stringify({ type: "stopHandTracking" }));
    
    // Exporter en CSV ou JSON
    console.log('Données collectées:', recordedFrames);
}, 10000);
```

---

## 🛠️ Configuration Unity

### WebSocketServer (Inspector)

```
Server Configuration
├─ Port: 8080
├─ Auto Start: ✓
├─ Send Stats Updates: ✓
└─ Stats Update Interval: 1.0

Hand Tracking
├─ Send Hand Tracking Data: ☐
└─ Hand Tracking Interval: 0.1
```

**Recommandations:**
- **Stats Update Interval:** 1s pour usage normal, 0.5s pour temps réel
- **Hand Tracking Interval:** 
  - 0.033s (30 Hz) pour analyse précise
  - 0.1s (10 Hz) pour usage standard
  - 0.2s (5 Hz) pour économie de bande passante

---

## 📚 Ressources

- **HAND_TRACKING_GUIDE.md** - Guide complet du hand tracking
- **hand-tracking-viewer.html** - Interface de visualisation
- **MJPEG_STREAM_GUIDE.md** - Guide du streaming vidéo
- **FREEPLAY_SETUP_GUIDE.md** - Configuration FreePlay

---

## 🔐 Sécurité

⚠️ **Note:** Le serveur WebSocket n'utilise pas d'authentification. Assurez-vous que le HoloLens est sur un réseau sécurisé.

---

## 🐛 Support

Pour toute question ou problème:
1. Vérifiez les logs Unity Console
2. Testez la connexion avec `hand-tracking-viewer.html`
3. Consultez les guides de documentation

---

✅ **API WebSocket complète et prête à l'emploi !**
