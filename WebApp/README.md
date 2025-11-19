# 🎈 PopBalloons Web Controller

Application web companion pour contrôler le jeu PopBalloons sur HoloLens depuis n'importe quel appareil (téléphone, tablette, ordinateur).

## 🚀 Démarrage rapide

### 1. Ouvrir l'application web

Il suffit d'ouvrir `index.html` dans un navigateur web :
- **Sur PC/Mac** : Double-cliquez sur `index.html`
- **Sur téléphone/tablette** : Naviguez vers le fichier et ouvrez-le

Ou utilisez un serveur web local :
```bash
# Avec Python 3
python -m http.server 8000

# Avec Node.js (npx http-server)
npx http-server -p 8000

# Puis ouvrez http://localhost:8000 dans votre navigateur
```

### 2. Configurer la connexion

1. Sur le HoloLens, le jeu doit être lancé avec le serveur WebSocket activé
2. Notez l'adresse IP du HoloLens (affichée dans le jeu)
3. Dans l'app web, entrez l'adresse IP et cliquez sur "Se connecter"

## 📱 Fonctionnalités

### Contrôles de jeu
- **Lancer les modes** : Cognitif, Moteur, FreePlay
- **Sélectionner les niveaux** : 1 à 5 pour les modes Cognitif et Moteur
- **Retour au menu** : Revenir à l'écran principal
- **Quitter le jeu** : Arrêter la session en cours

### Statistiques en direct
- **Score** : Score actuel du joueur
- **Ballons éclatés** : Nombre de ballons éclatés
- **Temps** : Durée de la session

### Console
- Affiche les messages de connexion
- Logs des actions effectuées
- Retours du HoloLens

## 🔧 Configuration Unity (HoloLens)

Pour que l'app web fonctionne, il faut ajouter un serveur WebSocket dans Unity.

### Étape 1 : Installer le package WebSocket

Le code Unity pour le serveur WebSocket sera fourni séparément.

### Étape 2 : Vérifier la connexion réseau

- Le HoloLens et l'appareil avec l'app web doivent être sur le **même réseau WiFi**
- Notez l'adresse IP du HoloLens (Paramètres > Réseau)

## 📡 Protocole de communication

### Messages envoyés par l'app web

```json
{
  "type": "startGame",
  "data": {
    "gameType": "COGNITIVE|MOBILITY|FREEPLAY",
    "level": 1
  }
}
```

```json
{
  "type": "goHome",
  "data": {}
}
```

```json
{
  "type": "quitGame",
  "data": {}
}
```

### Messages reçus du HoloLens

```json
{
  "type": "stats",
  "data": {
    "score": 100,
    "balloons": 10,
    "time": 120
  }
}
```

```json
{
  "type": "gameState",
  "data": {
    "state": "PLAY|HOME|SETUP"
  }
}
```

```json
{
  "type": "response",
  "data": {
    "message": "Game started"
  }
}
```

## 🎨 Personnalisation

### Modifier les couleurs
Éditez `style.css` pour changer le thème de l'application.

### Ajouter des fonctionnalités
Ajoutez de nouveaux boutons dans `index.html` et les fonctions correspondantes dans `app.js`.

## 🐛 Dépannage

### L'app ne se connecte pas
- Vérifiez que le HoloLens et votre appareil sont sur le même réseau WiFi
- Vérifiez l'adresse IP du HoloLens
- Assurez-vous que le serveur WebSocket est lancé dans Unity

### Les stats ne s'affichent pas
- Vérifiez que le jeu envoie bien les messages de stats
- Regardez la console (F12) pour voir les erreurs JavaScript

### Les boutons ne fonctionnent pas
- Vérifiez la connexion WebSocket (voyant vert = connecté)
- Regardez la console de l'app pour voir les messages envoyés

## 📄 Licence

Même licence que le projet PopBalloons principal.
