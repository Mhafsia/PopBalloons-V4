# Configuration Unity pour l'App Compagnon

## Installation

### 1. Ajouter le serveur WebSocket à votre scène

1. Dans Unity, ouvrez votre scène principale
2. Créez un nouveau GameObject vide (clic droit dans Hierarchy > Create Empty)
3. Nommez-le **"WebSocketServer"**
4. Ajoutez le script `WebSocketServer.cs` au GameObject
5. Dans l'Inspector, configurez :
   - **Port**: 8080 (par défaut)
   - **Auto Start**: ✅ (coché)
   - **Send Stats Updates**: ✅ (coché)
   - **Stats Update Interval**: 1.0 (secondes)

### 2. Vérifier les scripts

Assurez-vous que ces scripts sont bien présents dans votre projet :
- `Assets/Actimage.PopBalloons/Scripts/Network/WebSocketServer.cs`
- `Assets/Actimage.PopBalloons/Scripts/Utilities/UnityMainThreadDispatcher.cs`

### 3. Configuration réseau sur HoloLens

#### Pour tester en local (ordinateur):
1. L'app web utilisera `localhost:8080`
2. Aucune configuration supplémentaire nécessaire

#### Pour tester sur HoloLens:
1. Assurez-vous que le HoloLens et votre appareil sont sur le **même réseau WiFi**
2. Trouvez l'adresse IP du HoloLens :
   - Ouvrez les **Settings** sur HoloLens
   - Allez dans **Network & Internet** > **WiFi** > **Hardware Properties**
   - Notez l'**IPv4 address** (ex: 192.168.1.100)
3. Dans l'app web, entrez cette adresse IP

### 4. Capabilities requises (UWP/HoloLens)

Dans Unity, allez dans **Edit > Project Settings > Player > Publishing Settings**

Cochez les capabilities suivantes :
- ✅ **InternetClient**
- ✅ **InternetClientServer**
- ✅ **PrivateNetworkClientServer**

Ces permissions permettent au HoloLens d'accepter les connexions réseau.

## Utilisation

### Démarrage automatique
Le serveur démarre automatiquement au lancement du jeu si **Auto Start** est activé.

### Vérification du démarrage
Dans la console Unity, vous devriez voir :
```
WebSocket Server started on port 8080
Connect from web app using: ws://[VOTRE_IP]:8080
```

### Connexion depuis l'app web
1. Ouvrez `WebApp/index.html` dans un navigateur
2. Entrez l'adresse IP affichée dans la console
3. Cliquez sur "Se connecter"
4. Le statut devrait passer à "Connecté" (vert)

## Commandes disponibles

L'app web peut envoyer ces commandes au HoloLens :

### Démarrer un jeu
```json
{
  "type": "startGame",
  "data": {
    "gameType": "COGNITIVE" | "MOBILITY" | "FREEPLAY",
    "level": 1-5
  }
}
```

### Retourner au menu
```json
{
  "type": "goHome"
}
```

### Quitter le jeu en cours
```json
{
  "type": "quitGame"
}
```

## Statistiques en direct

Le serveur envoie automatiquement les stats toutes les secondes pendant une partie :
```json
{
  "type": "stats",
  "data": {
    "score": 150,
    "balloons": 12,
    "time": 45.3
  }
}
```

## Dépannage

### Le serveur ne démarre pas
- Vérifiez que le port 8080 n'est pas déjà utilisé
- Vérifiez les capabilities UWP (voir section 4)
- Regardez les erreurs dans la console Unity

### Impossible de se connecter depuis l'app web
- Vérifiez que HoloLens et l'appareil sont sur le même WiFi
- Vérifiez l'adresse IP du HoloLens
- Désactivez temporairement le pare-feu Windows sur le HoloLens
- Essayez de pinger le HoloLens depuis votre appareil

### Les commandes ne fonctionnent pas
- Vérifiez que `GameManager.Instance` existe dans votre scène
- Vérifiez la console Unity pour les erreurs
- Assurez-vous que le `UnityMainThreadDispatcher` est créé

### Les stats ne s'affichent pas
- Vérifiez que **Send Stats Updates** est activé
- Vérifiez que `ScoreManager.instance` existe
- Les stats ne sont envoyées que pendant une partie active

## Architecture technique

### Thread Safety
- Le serveur écoute les connexions sur un thread séparé
- Toutes les commandes Unity sont exécutées sur le thread principal via `UnityMainThreadDispatcher`
- Cela évite les erreurs "Can only be called from the main thread"

### Performance
- Le serveur utilise des threads légers (background threads)
- Impact minimal sur les performances du jeu
- Les stats sont envoyées à intervalle configurable (par défaut 1 seconde)

### Sécurité
- Le serveur accepte les connexions de tous les appareils sur le réseau local
- Pour un usage en production, considérez d'ajouter une authentification

## Prochaines étapes possibles

- [ ] Ajouter authentification par mot de passe
- [ ] Supporter plusieurs connexions simultanées
- [ ] Ajouter des commandes supplémentaires (pause, redémarrer, etc.)
- [ ] Afficher la liste des appareils connectés
- [ ] Enregistrer les sessions de jeu
- [ ] Support HTTPS/WSS pour connexions sécurisées
