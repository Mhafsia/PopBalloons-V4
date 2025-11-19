# Configuration Unity pour le Streaming MJPEG

## Étapes d'installation

### 1. Ajouter le composant MJPEGStreamServer

1. **Ouvrir Unity**
2. **Ouvrir la scène principale** (celle avec le WebSocketServer)
3. **Sélectionner le GameObject** qui contient le composant `WebSocketServer`
4. **Ajouter le composant** `MJPEGStreamServer` :
   - Click "Add Component"
   - Chercher "MJPEG"
   - Sélectionner "MJPEG Stream Server"

### 2. Configurer les paramètres (Inspector)

**Paramètres recommandés pour démarrer :**

```
✅ Auto Start: true
📡 Port: 8081
🎥 Target FPS: 15
📐 Stream Width: 640
📐 Stream Height: 480
🖼️ JPEG Quality: 75
```

### 3. Configurer le pare-feu Windows

**Ouvrir PowerShell en Administrateur** et exécuter :

```powershell
New-NetFirewallRule -DisplayName "MJPEG Stream PopBalloons" -Direction Inbound -LocalPort 8081 -Protocol TCP -Action Allow
```

### 4. Tester dans Unity Editor

1. **Play** la scène
2. Vérifier dans la **Console Unity** :
   ```
   MJPEG Stream Server started on port 8081
   Stream URL: http://localhost:8081/stream/
   ```
3. **Ouvrir le navigateur** et aller à `http://localhost:8081/stream/`
4. Vous devriez voir le flux vidéo de la caméra Unity

### 5. Tester avec l'application web

1. **Ouvrir** `WebApp/index.html` dans le navigateur
2. **Connecter** au WebSocket (devrait être automatique)
3. **Cliquer** sur "📷 Vue du Joueur"
4. Le stream vidéo devrait apparaître

## Vérifications

### ✅ Le streaming fonctionne si :

- Console Unity affiche : `MJPEG Stream Server started on port 8081`
- `http://localhost:8081/stream/` affiche la vidéo dans le navigateur
- Console navigateur affiche : `✅ Stream vidéo connecté`
- Vous voyez l'image se mettre à jour en temps réel

### ❌ Problèmes courants :

**"Port already in use"**
- Un autre programme utilise le port 8081
- Solution : Changer le port dans l'Inspector (ex: 8082)
- OU fermer l'autre application

**"Connection refused" dans le navigateur**
- Le pare-feu bloque le port
- Solution : Exécuter la commande PowerShell ci-dessus
- OU désactiver temporairement le pare-feu pour tester

**"Main camera not found"**
- Aucune caméra avec le tag "MainCamera"
- Solution : Vérifier qu'une caméra a le tag "MainCamera"

**Stream très saccadé**
- Réseau trop lent ou CPU surchargé
- Solution : Réduire le FPS (15 → 10) ou la résolution (640x480 → 480x360)

## Optimisations selon l'usage

### Pour réseau local rapide (WiFi 5GHz / Ethernet)
```
Target FPS: 20-25
Stream Width: 800
Stream Height: 600
JPEG Quality: 85
```

### Pour réseau WiFi 2.4GHz standard
```
Target FPS: 12-15
Stream Width: 640
Stream Height: 480
JPEG Quality: 75
```

### Pour HoloLens (WiFi + économie batterie)
```
Target FPS: 10
Stream Width: 480
Stream Height: 360
JPEG Quality: 65
```

## Architecture

```
GameObject (Unity Scene)
├── WebSocketServer (déjà présent)
│   └── Port: 8080 (commandes)
└── MJPEGStreamServer (NOUVEAU !)
    └── Port: 8081 (streaming vidéo)
```

Les deux serveurs fonctionnent en parallèle :
- **WebSocket** (8080) : Commandes, stats, profils
- **MJPEG** (8081) : Streaming vidéo continu

## Build pour HoloLens

Pas de configuration spéciale nécessaire ! Le MJPEGStreamServer fonctionne aussi sur HoloLens.

**Note importante :** Sur HoloLens, utiliser l'adresse IP du casque (pas localhost) :
- Trouver l'IP dans les paramètres réseau HoloLens
- Exemple : `http://192.168.1.50:8081/stream/`

## Logs utiles

**Unity Console :**
```
MJPEG Stream Server started on port 8081
Stream URL: http://192.168.1.100:8081/stream/
Client connected to MJPEG stream. Active clients: 1
Client disconnected. Active clients: 0
```

**Navigateur Console (F12) :**
```
📷 Stream vidéo activé: http://192.168.1.100:8081/stream/
✅ Stream vidéo connecté
```

## Support

Pour plus de détails techniques, voir `MJPEG_STREAM_GUIDE.md`.
