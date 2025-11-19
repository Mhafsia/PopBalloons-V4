# Plateforme TND - Interface Web

Application web Next.js pour la plateforme de suivi des troubles neurodéveloppementaux (TND).

## 🎯 Fonctionnalités

- **Sélecteur de profil** : Famille, Clinicien, Enseignant
- **Dashboard Famille** : Programme, progression, aptitudes, récompenses
- **Composants UI réutilisables** : Card, ProgressBar, Badge, SessionItem, DayIndicator
- **Design responsive** : Adapté desktop, tablette, mobile
- **Tailwind CSS** : Styles modernes et personnalisables

## 🚀 Démarrage rapide

### Installation des dépendances

```bash
npm install
```

### Lancement du serveur de développement

```bash
npm run dev
```

Ouvrez [http://localhost:3000](http://localhost:3000) dans votre navigateur.

## 📂 Structure du projet

```
tnd-platform-web/
├── src/
│   ├── app/
│   │   ├── layout.tsx       # Layout principal
│   │   ├── page.tsx         # Page d'accueil
│   │   └── globals.css      # Styles globaux
│   ├── components/
│   │   ├── ProfileSelector.tsx
│   │   ├── dashboards/
│   │   │   └── FamilyDashboard.tsx
│   │   └── ui/
│   │       ├── Card.tsx
│   │       ├── ProgressBar.tsx
│   │       ├── Badge.tsx
│   │       ├── SessionItem.tsx
│   │       └── DayIndicator.tsx
│   └── lib/
│       └── mockData.ts      # Données de test
├── tailwind.config.js       # Configuration Tailwind
└── next.config.mjs          # Configuration Next.js
```

## 🎨 Palette de couleurs

### Famille (Bleu)
- Primary: `#4A90E2`
- Secondary: `#7ED321`
- Accent: `#F5A623`

### Clinicien (Bleu foncé)
- Primary: `#2C5F8D`
- Secondary: `#17A2B8`
- Accent: `#6F42C1`

### Enseignant (Vert)
- Primary: `#28A745`
- Secondary: `#FFC107`
- Accent: `#17A2B8`

## 📊 Composants UI

### Card
Carte réutilisable avec titre, icône et contenu.

```tsx
<Card title="Titre" icon="🎯" variant="primary">
  Contenu
</Card>
```

### ProgressBar
Barre de progression avec évolution.

```tsx
<ProgressBar 
  label="Motricité" 
  value={0.8} 
  variant="gradient"
  showEvolution
  evolution={5}
/>
```

### Badge
Badge de récompense.

```tsx
<Badge 
  icon="🏆" 
  label="Champion" 
  type="gold" 
  locked={false}
/>
```

## 🔧 Scripts disponibles

- `npm run dev` : Serveur de développement
- `npm run build` : Build de production
- `npm start` : Serveur de production
- `npm run lint` : Vérification du code

## 🌐 Prochaines étapes

- [ ] Dashboard Clinicien
- [ ] Dashboard Enseignant
- [ ] API REST pour connexion Unity
- [ ] WebSocket pour communication temps réel
- [ ] Authentification
- [ ] Base de données
- [ ] Graphiques interactifs

## 📝 Notes

Cette interface web remplace l'interface Unity pour une meilleure accessibilité.  
Les données sont actuellement mockées pour la démo.

## 🤝 Contribution

Développé pour la plateforme TND - Troubles Neurodéveloppementaux.
