# Guide: Afficher la vue du joueur sur le site web

## 📸 Option recommandée: Screenshots périodiques

### Avantages
- ✅ Simple à implémenter
- ✅ Fonctionne en éditeur ET sur HoloLens
- ✅ Pas besoin de serveur externe
- ✅ Utilise WebSocket existant

### Comment ça marche
1. Unity prend un screenshot de la caméra principale toutes les X secondes
2. Convertit l'image en Base64
3. Envoie via WebSocket au navigateur
4. Le navigateur affiche dans un `<img>`

### Code à ajouter

#### 1. Dans WebSocketServer.cs (Unity)
```csharp
// Ajouter ces variables en haut de la classe
private bool sendCameraFeed = false;
private float cameraFeedInterval = 0.5f; // 2 FPS
private float lastCameraFeedTime = 0f;

// Dans Update()
if (sendCameraFeed && isRunning && Time.time - lastCameraFeedTime > cameraFeedInterval)
{
    SendCameraFeed();
    lastCameraFeedTime = Time.time;
}

// Nouvelle méthode
private void SendCameraFeed()
{
    StartCoroutine(CaptureCamera());
}

private IEnumerator CaptureCamera()
{
    yield return new WaitForEndOfFrame();
    
    Camera cam = Camera.main;
    if (cam == null) yield break;
    
    RenderTexture rt = new RenderTexture(640, 480, 24);
    cam.targetTexture = rt;
    Texture2D screenshot = new Texture2D(640, 480, TextureFormat.RGB24, false);
    cam.Render();
    RenderTexture.active = rt;
    screenshot.ReadPixels(new Rect(0, 0, 640, 480), 0, 0);
    cam.targetTexture = null;
    RenderTexture.active = null;
    Destroy(rt);
    
    byte[] bytes = screenshot.EncodeToJPG(75);
    string base64 = System.Convert.ToBase64String(bytes);
    
    string json = $"{{\"type\":\"cameraFeed\",\"data\":{{\"image\":\"data:image/jpeg;base64,{base64}\"}}}}";
    BroadcastRawJson(json);
    
    Destroy(screenshot);
}

// Ajouter case dans ProcessMessage switch
case "toggleCameraFeed":
    sendCameraFeed = !sendCameraFeed;
    Debug.Log($"Camera feed: {sendCameraFeed}");
    SendMessageToClient(client, new ResponseMessage
    {
        type = "response",
        data = new ResponseData { message = $"Camera feed {(sendCameraFeed ? "enabled" : "disabled")}" }
    });
    break;
```

#### 2. Dans index.html
```html
<!-- Ajouter dans controls-section -->
<div class="camera-feed-section" style="margin-top: 20px;">
    <button onclick="toggleCameraFeed()" class="action-btn" style="background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%);">
        📷 Vue du Joueur
    </button>
    <div id="camera-feed" style="display: none; margin-top: 15px; text-align: center;">
        <img id="camera-image" style="max-width: 100%; border-radius: 10px; box-shadow: 0 5px 15px rgba(0,0,0,0.3);" />
    </div>
</div>
```

#### 3. Dans app.js
```javascript
let cameraFeedActive = false;

// Dans handleMessage switch
case 'cameraFeed':
    updateCameraFeed(message.data.image);
    break;

// Nouvelle fonction
function toggleCameraFeed() {
    if (ws && ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({ type: 'toggleCameraFeed' }));
        cameraFeedActive = !cameraFeedActive;
        
        const feedDiv = document.getElementById('camera-feed');
        feedDiv.style.display = cameraFeedActive ? 'block' : 'none';
        
        log(cameraFeedActive ? '📷 Vue joueur activée' : '📷 Vue joueur désactivée');
    }
}

function updateCameraFeed(imageData) {
    const img = document.getElementById('camera-image');
    img.src = imageData;
}
```

## 🚀 Veux-tu que j'implémente cette fonctionnalité?

Dis-moi si tu veux que je l'ajoute! C'est environ 10 minutes d'implémentation. 

### Paramètres personnalisables:
- **Résolution**: 640x480 (bon compromis) ou 1280x720 (meilleure qualité mais plus lourd)
- **FPS**: 0.5s = 2 FPS (suffit pour monitoring) ou 0.2s = 5 FPS (plus fluide)
- **Qualité JPEG**: 75% (bon compromis) ou 90% (meilleure qualité mais plus lourd)
