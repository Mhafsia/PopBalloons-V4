# Guide d'utilisation - Interface Chercheur avec WebSocket

## 🔬 Vue d'ensemble

L'interface Chercheur permet de visualiser en temps réel les données du jeu PopBalloons qui s'exécute sur HoloLens 2, grâce à une connexion WebSocket.

## 🔌 Configuration de la connexion

### 1. Démarrer le jeu Unity sur HoloLens

Le serveur WebSocket démarre automatiquement avec le jeu PopBalloons sur le port **8080**.

Dans Unity, le composant `WebSocketServer` a ces paramètres :
- **Port** : 8080
- **Auto Start** : true (activé par défaut)
- **Send Stats Updates** : true
- **Stats Update Interval** : 1 seconde

### 2. Trouver l'adresse IP du HoloLens

#### Méthode 1 : Via les logs Unity
Regardez les logs Unity/FileLogger au démarrage :
```
✅ WEBSOCKET SERVER STARTED SUCCESSFULLY!
📡 Port: 8080
🌐 Local IPv4: 192.168.1.100
🔗 Connect from web app using: ws://192.168.1.100:8080
```

#### Méthode 2 : Via le Device Portal
1. Ouvrez le Device Portal du HoloLens
2. Allez dans **Networking** → **Wi-Fi**
3. Notez l'adresse IPv4

#### Méthode 3 : Via les paramètres HoloLens
1. Faites le geste "Start"
2. Allez dans **Paramètres** → **Réseau & Internet** → **Wi-Fi**
3. Cliquez sur le réseau connecté
4. Notez l'adresse IP

### 3. Se connecter depuis l'interface web

1. Ouvrez l'interface web : http://localhost:3000
2. Sélectionnez le profil **Chercheur** 🔬
3. Dans le champ "WebSocket URL", entrez :
   ```
   ws://[IP_DU_HOLOLENS]:8080
   ```
   Exemple : `ws://192.168.1.100:8080`

4. Cliquez sur **▶️ Se connecter au HoloLens**

5. Si la connexion réussit :
   - Le bouton devient rouge "⏹ Déconnecter"
   - Un badge vert "✅ Connecté" apparaît
   - Le statut patient passe à "En session"

## 📊 Données transmises en temps réel

### Stats de jeu (toutes les 1 seconde)

Le WebSocket envoie automatiquement :

```json
{
  "type": "stats",
  "data": {
    "score": 450,
    "balloons": 12,
    "time": 65.3
  }
}
```

**Affichage dans l'interface :**
- **Durée** : Temps de jeu formaté (MM:SS)
- **Ballons** : Nombre de ballons éclatés
- **Score** : Score actuel
- **Moy/min** : Ballons par minute
- **Graphique** : Historique des 20 dernières secondes

### Données de Hand Tracking (10 Hz - optionnel)

Si activé dans Unity (`sendHandTrackingData = true`) :

```json
{
  "type": "handTracking",
  "data": {
    "timestamp": 65.3,
    "timestampMs": 65300,
    "leftHand": {
      "isTracked": true,
      "joints": {
        "Palm": {
          "position": {"x": 0.1, "y": 0.5, "z": 0.3},
          "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}
        },
        // ... autres articulations
      }
    },
    "rightHand": { /* ... */ }
  }
}
```

## 🔧 Dépannage

### ❌ "Erreur de connexion WebSocket"

**Causes possibles :**
1. L'IP du HoloLens est incorrecte
2. Le HoloLens n'est pas sur le même réseau
3. Le jeu n'est pas démarré sur HoloLens
4. Le firewall bloque le port 8080

**Solutions :**
```bash
# Tester la connexion depuis PowerShell :
Test-NetConnection -ComputerName 192.168.1.100 -Port 8080

# Vérifier que le serveur WebSocket écoute :
# Sur HoloLens, consultez les logs Unity
```

### ⚠️ Connexion instable

**Solution :**
- Vérifiez la qualité du signal Wi-Fi
- Réduisez la distance entre HoloLens et le routeur
- Essayez de réduire `statsUpdateInterval` dans Unity

### 📱 Tester sans HoloLens

Pour tester l'interface sans HoloLens, vous pouvez :

1. **Lancer Unity en mode Play sur PC**
   - Ouvrez le projet dans Unity
   - Cliquez sur Play ▶️
   - Le WebSocket se lancera automatiquement
   - Vous verrez dans la Console : "🎮 Running in Unity Editor - WebSocket enabled!"

2. **Connectez-vous à localhost**
   - Dans l'interface web, entrez : `ws://localhost:8080`
   - Cliquez sur "Se connecter au HoloLens"
   - Les stats du jeu en mode Play apparaîtront en temps réel !

3. **Tester sur le réseau local**
   - Trouvez votre IP locale : `ipconfig` (Windows) ou `ifconfig` (Mac/Linux)
   - Connectez-vous depuis un autre appareil : `ws://[VOTRE_IP]:8080`

## 🎯 Utilisation avancée

### Modifier l'intervalle de mise à jour

Dans Unity, `WebSocketServer.cs` :

```csharp
[SerializeField] private float statsUpdateInterval = 1f; // 1 seconde
```

Valeurs recommandées :
- **0.5s** : Mise à jour rapide (plus de trafic réseau)
- **1.0s** : Équilibre (par défaut)
- **2.0s** : Économie de batterie

### Activer le Hand Tracking

```csharp
[SerializeField] private bool sendHandTrackingData = true;
[SerializeField] private float handTrackingInterval = 0.1f; // 10 Hz
```

### Envoyer des commandes au HoloLens

Le WebSocket peut aussi **recevoir** des commandes (déjà implémenté) :

```javascript
// Depuis l'interface web
websocket.send(JSON.stringify({
  type: 'setFreePlaySettings',
  data: {
    numberOfBalloons: 50,
    gameDuration: 120,
    difficultyLevel: 2
  }
}));
```

## 📋 Checklist de connexion

- [ ] HoloLens allumé et connecté au Wi-Fi
- [ ] Jeu PopBalloons lancé sur HoloLens
- [ ] Adresse IP du HoloLens notée
- [ ] Interface web ouverte (localhost:3000)
- [ ] Profil Chercheur sélectionné
- [ ] URL WebSocket saisie (ws://IP:8080)
- [ ] Bouton "Se connecter" cliqué
- [ ] Badge "✅ Connecté" affiché
- [ ] Données qui s'affichent en temps réel

## 🎮 Workflow complet

1. **Démarrer le jeu** sur HoloLens
2. **Noter l'IP** dans les logs (ex: 192.168.1.100)
3. **Ouvrir l'interface web** : http://localhost:3000
4. **Sélectionner "Chercheur"** 🔬
5. **Entrer l'URL** : ws://192.168.1.100:8080
6. **Se connecter** : cliquer sur ▶️
7. **Observer** les données en temps réel
8. **Déconnecter** : cliquer sur ⏹

## 🔗 Références

- Code Unity : `Assets/Actimage.PopBalloons/Scripts/Network/WebSocketServer.cs`
- Code Web : `tnd-platform-web/lib/useWebSocket.ts`
- Dashboard : `tnd-platform-web/components/dashboards/ResearcherDashboard.tsx`
