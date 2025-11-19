# Guide du Streaming Vidéo MJPEG

## Vue d'ensemble

Le système utilise **MJPEG (Motion JPEG)** pour streamer la vue du joueur en temps réel vers le navigateur web. MJPEG est un format de streaming vidéo qui envoie une série continue d'images JPEG via HTTP.

## Avantages du MJPEG

✅ **Vrai flux vidéo continu** (pas de "sauts" comme avec les captures d'écran)
✅ **Faible latence** (~200-300ms)
✅ **Simple à implémenter** (HTTP standard, pas besoin de plugins)
✅ **Compatible tous navigateurs** (Chrome, Firefox, Safari, Edge)
✅ **Fonctionne avec Unity Editor ET HoloLens**
✅ **Performance optimisée** (capture uniquement si des clients sont connectés)

## Architecture

```
Unity (HoloLens/Editor)              Navigateur Web
┌─────────────────────┐              ┌──────────────────┐
│ MJPEGStreamServer   │              │  WebApp          │
│                     │              │                  │
│ ┌─────────────────┐ │              │ ┌──────────────┐ │
│ │ Camera Capture  │ │              │ │ <img> tag    │ │
│ │ (15 FPS)        │ │              │ │              │ │
│ └────────┬────────┘ │              │ └──────▲───────┘ │
│          │          │              │        │         │
│ ┌────────▼────────┐ │   HTTP GET   │ ┌──────┴───────┐ │
│ │ JPEG Encoding   │ │◄─────────────┤ │ toggleCamera │ │
│ │ (Quality 75%)   │ │              │ │ Feed()       │ │
│ └────────┬────────┘ │              │ └──────────────┘ │
│          │          │              │                  │
│ ┌────────▼────────┐ │ MJPEG Stream │                  │
│ │ HTTP Server     │─┼──────────────┼─────────────────►│
│ │ (Port 8081)     │ │              │                  │
│ └─────────────────┘ │              │                  │
└─────────────────────┘              └──────────────────┘
        │                                     │
        │         WebSocket (Port 8080)       │
        └─────────────────────────────────────┘
             (Commands & Stream URL)
```

## Composants

### 1. MJPEGStreamServer.cs (Unity)

Serveur HTTP qui gère le streaming MJPEG.

**Paramètres configurables (Unity Inspector):**
- `port` : Port HTTP (défaut: 8081)
- `targetFPS` : Images par seconde (défaut: 15 FPS)
- `streamWidth` : Largeur de la capture (défaut: 640px)
- `streamHeight` : Hauteur de la capture (défaut: 480px)
- `jpegQuality` : Qualité JPEG 0-100 (défaut: 75)

**Fonctionnalités clés:**
- Capture continue de la caméra Unity
- Encodage JPEG en temps réel
- Streaming HTTP multipart/x-mixed-replace
- Gestion multi-clients
- Optimisation : capture uniquement si clients connectés

### 2. WebSocketServer.cs (Unity)

Modifié pour envoyer l'URL du stream MJPEG.

**Commande WebSocket:**
```json
{
  "type": "toggleCameraFeed"
}
```

**Réponse:**
```json
{
  "type": "cameraFeedURL",
  "data": {
    "url": "http://192.168.1.100:8081/stream/"
  }
}
```

### 3. WebApp (HTML/JavaScript)

**Nouveau comportement:**
1. Clic sur "📷 Vue du Joueur"
2. Envoi commande WebSocket `toggleCameraFeed`
3. Réception de l'URL du stream
4. Affichage dans `<img src="http://IP:8081/stream/">`
5. Le navigateur se connecte directement au serveur MJPEG

## Configuration Unity

### Étape 1 : Ajouter le MJPEGStreamServer

1. Ouvrir la scène principale
2. Sélectionner le GameObject avec `WebSocketServer`
3. Ajouter le composant `MJPEGStreamServer` (Add Component → Scripts → MJPEGStreamServer)

### Étape 2 : Configurer les paramètres

Dans l'Inspector du `MJPEGStreamServer`:

**Pour réseau local rapide (WiFi 5GHz / Ethernet):**
- Target FPS: 20-30
- Stream Width: 800
- Stream Height: 600
- JPEG Quality: 85

**Pour réseau lent (WiFi 2.4GHz):**
- Target FPS: 10-15
- Stream Width: 640
- Stream Height: 480
- JPEG Quality: 70

**Pour HoloLens (batterie & WiFi limités):**
- Target FPS: 10-12
- Stream Width: 480
- Stream Height: 360
- JPEG Quality: 65

### Étape 3 : Configurer le pare-feu (Windows)

Le port 8081 doit être ouvert pour les connexions entrantes.

**Powershell (Admin):**
```powershell
New-NetFirewallRule -DisplayName "MJPEG Stream" -Direction Inbound -LocalPort 8081 -Protocol TCP -Action Allow
```

## Utilisation

### Depuis le navigateur web

1. Connecter au WebSocket (automatique au chargement)
2. Cliquer sur "📷 Vue du Joueur"
3. Le stream vidéo apparaît automatiquement
4. Pour arrêter : recharger la page ou fermer le navigateur

### Accès direct au stream

Vous pouvez aussi accéder directement au stream MJPEG :
```
http://<IP_DE_LORDINATEUR>:8081/stream/
```

Par exemple :
- `http://192.168.1.100:8081/stream/`
- `http://localhost:8081/stream/` (depuis le même PC)

## Performances

### Bande passante estimée

| Résolution | FPS | Qualité | Débit (~) |
|-----------|-----|---------|-----------|
| 480x360   | 10  | 65%     | ~300 KB/s |
| 640x480   | 15  | 75%     | ~600 KB/s |
| 800x600   | 20  | 85%     | ~1.2 MB/s |
| 1280x720  | 30  | 90%     | ~3.0 MB/s |

### Impact CPU Unity

- Capture RenderTexture : ~2-5% CPU
- Encodage JPEG : ~3-8% CPU
- HTTP Streaming : ~1-2% CPU
- **Total : ~6-15% CPU** (dépend de la résolution/FPS)

### Optimisations automatiques

- ✅ Capture désactivée si aucun client connecté
- ✅ Réutilisation des buffers mémoire
- ✅ Nettoyage automatique des ressources
- ✅ Threading pour l'HTTP (pas de blocage Unity)

## Troubleshooting

### Le stream ne se connecte pas

1. **Vérifier que MJPEGStreamServer est actif**
   - Console Unity doit afficher : `MJPEG Stream Server started on port 8081`

2. **Tester l'accès direct**
   - Ouvrir `http://localhost:8081/stream/` dans le navigateur
   - Si ça marche : problème de réseau/pare-feu
   - Si ça ne marche pas : problème de serveur Unity

3. **Vérifier le pare-feu**
   ```powershell
   # Lister les règles de pare-feu
   Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*8081*"}
   ```

4. **Vérifier l'adresse IP**
   ```powershell
   ipconfig
   ```
   Utiliser l'adresse IPv4 de votre carte réseau active

### Le stream est saccadé

1. **Réduire le FPS** : 15 → 10 FPS
2. **Réduire la résolution** : 640x480 → 480x360
3. **Réduire la qualité JPEG** : 75 → 65
4. **Vérifier la qualité WiFi**

### Le stream a trop de latence

1. **Augmenter le FPS** : 15 → 20-25 FPS
2. **Réduire la qualité JPEG** pour compenser la bande passante
3. **Utiliser une connexion Ethernet** si possible

### Erreur "Port already in use"

Un autre programme utilise le port 8081.

**Solution 1 : Changer le port**
- Dans Unity Inspector : Port = 8082 (ou autre)

**Solution 2 : Trouver et fermer l'application**
```powershell
# Trouver quel programme utilise le port 8081
netstat -ano | findstr :8081
# Tuer le processus (remplacer PID par le numéro affiché)
taskkill /PID <PID> /F
```

## Sécurité

### Réseau local uniquement

Par défaut, le serveur écoute sur `http://*:8081/` ce qui signifie toutes les interfaces réseau.

**Pour limiter à localhost uniquement** (si vous testez localement) :
Modifier dans `MJPEGStreamServer.cs` ligne 50 :
```csharp
httpListener.Prefixes.Add($"http://localhost:{port}/stream/");
```

### Pas d'authentification

⚠️ Le stream MJPEG n'a **aucune authentification** par défaut.
N'importe qui sur le réseau peut voir le stream s'il connaît l'URL.

**Pour production** : Ajouter un token d'authentification dans l'URL ou les headers HTTP.

## Comparaison avec l'ancienne méthode

| Caractéristique | Screenshots (ancien) | MJPEG Stream (nouveau) |
|----------------|---------------------|----------------------|
| Fluidité | ❌ Saccadé (2 FPS) | ✅ Fluide (15+ FPS) |
| Latence | ⚠️ ~500ms | ✅ ~200ms |
| Bande passante | ✅ Faible (~100 KB/s) | ⚠️ Moyenne (~600 KB/s) |
| Complexité | ✅ Simple | ⚠️ Moyenne |
| Multi-clients | ⚠️ Tous reçoivent les images | ✅ Chacun se connecte indépendamment |
| Impact CPU | ✅ Faible (5%) | ⚠️ Moyen (10-15%) |

## Conclusion

Le streaming MJPEG offre une **expérience vidéo beaucoup plus fluide** que les captures d'écran périodiques, au prix d'une utilisation réseau et CPU légèrement supérieure. C'est le meilleur compromis pour un système de monitoring en temps réel.

Pour des besoins encore plus performants (très faible latence, haute résolution), envisager WebRTC mais cela nécessite des bibliothèques tierces et une complexité beaucoup plus élevée.
