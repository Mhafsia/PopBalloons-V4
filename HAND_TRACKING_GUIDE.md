# 🤲 Guide Hand Tracking - PopBalloons HoloLens

Ce guide explique comment utiliser le système de capture et transmission des données de hand tracking pour l'analyse des mouvements des mains.

## 📋 Vue d'ensemble

Le système capture les données suivantes pour **chaque main** (gauche et droite) :

- ✅ **Position 3D (x, y, z)** de chaque articulation (joint) en mètres
- ✅ **Rotation (quaternion)** de chaque articulation
- ✅ **Timestamp** de capture (en secondes et millisecondes)
- ✅ **Identification main gauche vs droite**
- ✅ **État de tracking** (main visible ou non)

## 🦴 Articulations Capturées

MRTK suit **26 joints par main** :

### Main complète
- `Palm` - Paume de la main
- `Wrist` - Poignet

### Pouce (5 joints)
- `ThumbMetacarpalJoint` - Métacarpien
- `ThumbProximalJoint` - Interphalangienne proximale
- `ThumbDistalJoint` - Interphalangienne distale
- `ThumbTip` - Bout du pouce

### Index (5 joints)
- `IndexMetacarpal` - Métacarpien
- `IndexKnuckle` - Jointure (articulation métacarpophalangienne)
- `IndexMiddleJoint` - Articulation centrale
- `IndexDistalJoint` - Articulation distale
- `IndexTip` - Bout de l'index

### Majeur (5 joints)
- `MiddleMetacarpal`
- `MiddleKnuckle`
- `MiddleMiddleJoint`
- `MiddleDistalJoint`
- `MiddleTip`

### Annulaire (5 joints)
- `RingMetacarpal`
- `RingKnuckle`
- `RingMiddleJoint`
- `RingDistalJoint`
- `RingTip`

### Auriculaire (5 joints)
- `PinkyMetacarpal`
- `PinkyKnuckle`
- `PinkyMiddleJoint`
- `PinkyDistalJoint`
- `PinkyTip`

## 🎮 Configuration Unity

### 1. Configuration du WebSocketServer

Dans Unity, sélectionnez le GameObject avec `WebSocketServer` :

```
Inspector > WebSocket Server
├─ Server Configuration
│  ├─ Port: 8080
│  ├─ Auto Start: ✓
│  └─ Send Stats Updates: ✓
│
└─ Hand Tracking
   ├─ Send Hand Tracking Data: ☐ (Activez pour stream automatique)
   └─ Hand Tracking Interval: 0.1 (10 captures/seconde)
```

### 2. Réglages de fréquence

**Intervalles recommandés :**
- **Analyse en temps réel** : `0.033s` (30 Hz)
- **Analyse standard** : `0.1s` (10 Hz) ⭐ Par défaut
- **Économie de bande passante** : `0.2s` (5 Hz)
- **Enregistrement léger** : `0.5s` (2 Hz)

## 📡 Commandes WebSocket

### Démarrer le streaming continu
```javascript
ws.send(JSON.stringify({ type: "startHandTracking" }));
```
**Réponse :** `{ type: "response", data: { message: "Hand tracking started" } }`

### Arrêter le streaming
```javascript
ws.send(JSON.stringify({ type: "stopHandTracking" }));
```
**Réponse :** `{ type: "response", data: { message: "Hand tracking stopped" } }`

### Capturer une frame unique
```javascript
ws.send(JSON.stringify({ type: "getHandTrackingFrame" }));
```
**Réponse :** Une frame complète (voir format ci-dessous)

## 📦 Format des Données

### Structure JSON reçue

```json
{
  "type": "handTracking",
  "data": {
    "timestamp": 123.456,        // Temps Unity (secondes depuis démarrage)
    "timestampMs": 123456,       // Millisecondes
    "leftHand": {
      "handedness": "Left",
      "isTracked": true,
      "joints": [
        {
          "jointName": "Palm",
          "position": { "x": 0.123, "y": 0.456, "z": 0.789 },
          "rotation": { "x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0 }
        },
        {
          "jointName": "Wrist",
          "position": { "x": 0.100, "y": 0.400, "z": 0.750 },
          "rotation": { "x": 0.1, "y": 0.2, "z": 0.3, "w": 0.9 }
        },
        // ... 24 autres joints
      ]
    },
    "rightHand": {
      "handedness": "Right",
      "isTracked": true,
      "joints": [ /* ... */ ]
    }
  }
}
```

### Si une main n'est pas visible

```json
{
  "type": "handTracking",
  "data": {
    "timestamp": 123.456,
    "timestampMs": 123456,
    "leftHand": null,           // Main gauche non trackée
    "rightHand": { /* ... */ }   // Main droite visible
  }
}
```

## 💻 Exemple d'utilisation JavaScript

### Connexion et réception

```javascript
const ws = new WebSocket('ws://192.168.1.100:8080');

ws.onopen = () => {
    console.log('✅ Connecté au HoloLens');
    
    // Démarrer le hand tracking
    ws.send(JSON.stringify({ type: "startHandTracking" }));
};

ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    
    if (message.type === 'handTracking') {
        processHandTrackingData(message.data);
    }
};

function processHandTrackingData(data) {
    console.log(`Frame @ ${data.timestamp}s`);
    
    // Main gauche
    if (data.leftHand && data.leftHand.isTracked) {
        const palmPos = data.leftHand.joints.find(j => j.jointName === 'Palm').position;
        console.log(`Left palm: (${palmPos.x}, ${palmPos.y}, ${palmPos.z})`);
    }
    
    // Main droite
    if (data.rightHand && data.rightHand.isTracked) {
        const indexTip = data.rightHand.joints.find(j => j.jointName === 'IndexTip').position;
        console.log(`Right index tip: (${indexTip.x}, ${indexTip.y}, ${indexTip.z})`);
    }
}
```

### Calcul de vitesse/accélération

```javascript
class HandTracker {
    constructor() {
        this.previousFrame = null;
    }
    
    processFrame(frame) {
        if (this.previousFrame) {
            const deltaTime = frame.timestamp - this.previousFrame.timestamp;
            
            // Calculer la vitesse de la paume gauche
            if (frame.leftHand && this.previousFrame.leftHand) {
                const velocity = this.calculateVelocity(
                    this.getPalmPosition(frame.leftHand),
                    this.getPalmPosition(this.previousFrame.leftHand),
                    deltaTime
                );
                
                console.log(`Left palm velocity: ${velocity.toFixed(3)} m/s`);
            }
        }
        
        this.previousFrame = frame;
    }
    
    getPalmPosition(hand) {
        const palm = hand.joints.find(j => j.jointName === 'Palm');
        return palm.position;
    }
    
    calculateVelocity(pos1, pos2, deltaTime) {
        const dx = pos1.x - pos2.x;
        const dy = pos1.y - pos2.y;
        const dz = pos1.z - pos2.z;
        const distance = Math.sqrt(dx*dx + dy*dy + dz*dz);
        return distance / deltaTime;
    }
}

// Utilisation
const tracker = new HandTracker();
ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    if (message.type === 'handTracking') {
        tracker.processFrame(message.data);
    }
};
```

### Enregistrement dans un fichier CSV

```javascript
class HandDataRecorder {
    constructor() {
        this.records = [];
    }
    
    recordFrame(frame) {
        if (frame.leftHand) {
            frame.leftHand.joints.forEach(joint => {
                this.records.push({
                    timestamp: frame.timestampMs,
                    hand: 'Left',
                    joint: joint.jointName,
                    x: joint.position.x,
                    y: joint.position.y,
                    z: joint.position.z,
                    qx: joint.rotation.x,
                    qy: joint.rotation.y,
                    qz: joint.rotation.z,
                    qw: joint.rotation.w
                });
            });
        }
        
        // Même chose pour rightHand...
    }
    
    exportToCSV() {
        const headers = 'timestamp,hand,joint,x,y,z,qx,qy,qz,qw\n';
        const rows = this.records.map(r => 
            `${r.timestamp},${r.hand},${r.joint},${r.x},${r.y},${r.z},${r.qx},${r.qy},${r.qz},${r.qw}`
        ).join('\n');
        
        const csv = headers + rows;
        
        // Télécharger le fichier
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `hand_tracking_${Date.now()}.csv`;
        a.click();
    }
}

// Utilisation
const recorder = new HandDataRecorder();

// Enregistrer pendant 10 secondes
ws.send(JSON.stringify({ type: "startHandTracking" }));

setTimeout(() => {
    ws.send(JSON.stringify({ type: "stopHandTracking" }));
    recorder.exportToCSV();
    console.log('✅ Enregistrement terminé et exporté');
}, 10000);
```

## 📊 Analyses Possibles

Avec ces données, vous pouvez calculer :

### Métriques de performance
- **Vitesse des mains** : Distance parcourue / temps
- **Accélération** : Variation de vitesse / temps
- **Amplitude de mouvement** : Distance min-max pour chaque joint
- **Précision** : Écart-type des positions lors de maintien statique

### Gestes et interactions
- **Pincement** : Distance entre pouce et index
- **Ouverture main** : Distance entre joints des doigts
- **Direction pointée** : Vecteur depuis paume vers index
- **Tremblements** : Variations haute fréquence de position

### Analyse temporelle
- **Temps de réaction** : Délai entre stimulus et mouvement
- **Durée de geste** : Temps entre début et fin d'action
- **Fréquence de mouvement** : Nombre d'actions par minute

## 🔧 Dépannage

### Aucune donnée reçue
1. Vérifiez que `Send Hand Tracking Data` est activé dans Unity
2. Envoyez la commande `startHandTracking`
3. Assurez-vous que les mains sont visibles par le HoloLens

### Main non trackée (null)
- Assurez-vous que la main est dans le champ de vision du HoloLens
- Évitez les occlusions (main cachée derrière un objet)
- Vérifiez l'éclairage de la pièce

### Trop de données / Lag
- Augmentez `Hand Tracking Interval` (ex: 0.2s au lieu de 0.1s)
- Utilisez `getHandTrackingFrame` au lieu du streaming continu
- Filtrez les joints non nécessaires côté client

## 📈 Performance

### Taille des données
- **Frame complète (2 mains, 26 joints chacune)** : ~8-10 KB
- **À 10 Hz** : ~80-100 KB/s
- **À 30 Hz** : ~240-300 KB/s

### Recommandations
- Utilisez WiFi 5 GHz pour de meilleures performances
- Limitez à 10-15 Hz sauf besoin spécifique
- Arrêtez le streaming quand non nécessaire

## 🎯 Cas d'usage

### Thérapie / Rééducation
```javascript
// Mesurer l'amplitude de mouvement du poignet
function measureWristFlexion(hand) {
    const wrist = hand.joints.find(j => j.jointName === 'Wrist');
    const palm = hand.joints.find(j => j.jointName === 'Palm');
    
    // Calculer l'angle entre les deux rotations
    const angle = calculateAngleBetweenQuaternions(wrist.rotation, palm.rotation);
    return angle;
}
```

### Analyse sportive
```javascript
// Détecter un geste de lancer
function detectThrowingMotion(frames) {
    // Analyser la vitesse de la main sur plusieurs frames
    const velocities = frames.map(f => calculateHandVelocity(f.rightHand));
    const maxVelocity = Math.max(...velocities);
    
    if (maxVelocity > 5.0) { // m/s
        console.log('🎯 Lancer détecté !');
    }
}
```

### Gaming / Interaction
```javascript
// Détecter un geste de pincement (grab)
function detectPinch(hand) {
    const thumb = hand.joints.find(j => j.jointName === 'ThumbTip');
    const index = hand.joints.find(j => j.jointName === 'IndexTip');
    
    const distance = calculateDistance(thumb.position, index.position);
    return distance < 0.02; // 2cm
}
```

## 📝 Notes Importantes

1. **Coordonnées** : Les positions sont en mètres dans l'espace mondial Unity
2. **Rotation** : Les quaternions sont normalisés (w² + x² + y² + z² = 1)
3. **Timestamp** : Utilisez `timestampMs` pour synchroniser avec autres sources
4. **Performance** : Le hand tracking MRTK est optimisé mais consomme du CPU
5. **Précision** : HoloLens 2 offre une précision de ~1-2mm pour le hand tracking

## 🔗 Ressources

- [MRTK Hand Tracking Documentation](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/features/input/hand-tracking)
- [HoloLens 2 Hand Tracking Specs](https://learn.microsoft.com/en-us/hololens/hololens2-hardware)
- [Quaternions Explained](https://www.3dgep.com/understanding-quaternions/)

---

✅ **Système prêt à l'emploi !** Activez simplement le hand tracking dans Unity et envoyez `startHandTracking` depuis votre application web.
