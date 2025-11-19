# 🚀 Guide d'optimisation PopBalloons pour HoloLens

## 📊 État actuel
- **RAM utilisée** : ~364 MB
- **FPS** : ~33 fps (cible : 60 fps)

## 🎯 Optimisations recommandées

### 1️⃣ **Textures et Matériaux**
```
✅ Réduire la résolution des textures :
- Ballons : 512x512 → 256x256
- UI : 1024x1024 → 512x512
- Compression : ASTC 6x6 (pour HoloLens)

Dans Unity :
- Sélectionner les textures
- Inspector > Max Size : 512 ou 256
- Compression : ASTC
- Générer les mipmaps
```

### 2️⃣ **Modèles 3D**
```
✅ Optimiser les ballons :
- Réduire le nombre de polygones
- Utiliser Level of Detail (LOD)
- Combiner les meshes statiques

Ballons actuels : probablement 500-1000 triangles
Cible : 200-300 triangles par ballon
```

### 3️⃣ **MJPEG Stream**
```csharp
// Dans MJPEGStreamServer.cs, réduire la qualité quand personne ne regarde :
[SerializeField] private int jpegQuality = 75; // → 50-60
[SerializeField] private int streamWidth = 640; // → 480
[SerializeField] private int streamHeight = 480; // → 360
[SerializeField] private int targetFPS = 15; // → 10

// Ou désactiver complètement si non utilisé
public void StopStreaming() {
    activeStreams.Clear();
}
```

### 4️⃣ **Object Pooling pour les Ballons**
Au lieu de créer/détruire constamment :
```csharp
// Créer un pool de ballons réutilisables
private Queue<BalloonBehaviour> balloonPool = new Queue<BalloonBehaviour>();

public BalloonBehaviour GetBalloon() {
    if (balloonPool.Count > 0) {
        var balloon = balloonPool.Dequeue();
        balloon.gameObject.SetActive(true);
        return balloon;
    }
    return Instantiate(balloonPrefab);
}

public void ReturnBalloon(BalloonBehaviour balloon) {
    balloon.gameObject.SetActive(false);
    balloonPool.Enqueue(balloon);
}
```

### 5️⃣ **Désactiver le Profiler en production**
```csharp
// Dans la configuration MRTK ou au démarrage :
#if !UNITY_EDITOR
    if (CoreServices.DiagnosticsSystem != null) {
        CoreServices.DiagnosticsSystem.ShowProfiler = false;
        CoreServices.DiagnosticsSystem.ShowDiagnostics = false;
    }
#endif
```

### 6️⃣ **Optimiser le WebSocket**
```csharp
// Réduire la fréquence des stats updates
[SerializeField] private float statsUpdateInterval = 1f; // → 2f ou 3f

// Ne broadcaster les profils que quand ils changent (déjà fait ✅)
```

### 7️⃣ **Audio**
```
✅ Compresser les sons :
- Format : Vorbis (au lieu de PCM)
- Quality : 70%
- Load Type : Compressed in Memory
```

### 8️⃣ **Garbage Collection**
```csharp
// Dans les boucles fréquentes (FreePlay, etc.)
// Éviter les allocations :

// ❌ Mauvais
foreach (var balloon in balloons.ToList()) { }

// ✅ Bon
for (int i = balloons.Count - 1; i >= 0; i--) {
    var balloon = balloons[i];
}

// Utiliser StringBuilder au lieu de string concatenation
```

## 🎮 **Paramètres Unity Build pour HoloLens**

```
Build Settings :
- Scripting Backend : IL2CPP
- API Compatibility Level : .NET Standard 2.0
- Stripping Level : Medium ou High
- Enable Exceptions : None

Quality Settings :
- Pixel Light Count : 1
- Texture Quality : Half Res
- Shadow Quality : Disable
- Anti Aliasing : Disabled (HoloLens fait déjà du MSAA)
```

## 📈 **Mesurer l'impact**

1. **Profiler Unity** (Ctrl+7) :
   - CPU Usage
   - Memory (Texture Memory, Mesh Memory)
   - Rendering (Draw Calls, Batches)

2. **HoloLens Device Portal** :
   - Performance > System Performance
   - Surveiller RAM et CPU en temps réel

## 🎯 **Objectifs**
- **RAM** : < 200 MB (gain de ~160 MB)
- **FPS** : 60 fps stable
- **Draw Calls** : < 100
- **Batches** : < 50

## ⚡ **Quick Wins (à faire en premier)**

1. ✅ Désactiver le Profiler visuel
2. ✅ Réduire qualité MJPEG (75→50)
3. ✅ Compression textures (ASTC)
4. ✅ Réduire statsUpdateInterval (1s→3s)
5. ✅ Désactiver logs Debug en production
