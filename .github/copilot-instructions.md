# Plateforme TND - Interface Web Next.js

## ✅ Projet Créé avec Succès

Le projet Next.js avec TypeScript et Tailwind CSS est maintenant opérationnel!

### 📂 Structure du Projet

```
tnd-platform-web/
├── src/
│   ├── app/
│   │   ├── layout.tsx       # Layout principal
│   │   ├── page.tsx         # Page d'accueil avec sélecteur de profil
│   │   └── globals.css      # Styles Tailwind
│   ├── components/
│   │   ├── ProfileSelector.tsx       # Sélecteur Famille/Clinicien/Enseignant
│   │   ├── dashboards/
│   │   │   └── FamilyDashboard.tsx  # Dashboard famille complet
│   │   └── ui/
│   │       ├── Card.tsx             # Composant carte réutilisable
│   │       ├── ProgressBar.tsx      # Barre de progression avec évolution
│   │       ├── Badge.tsx            # Badge de récompense
│   │       ├── SessionItem.tsx      # Item de séance
│   │       └── DayIndicator.tsx     # Indicateur de jour (calendrier)
│   └── lib/
│       └── mockData.ts              # Données de test (Marie, 8 ans)
├── package.json
├── tsconfig.json
├── tailwind.config.js               # Config Tailwind avec couleurs TND
└── next.config.mjs
```

### 🚀 Lancer l'Application

Le serveur est déjà lancé sur **http://localhost:3000**

Si besoin de relancer :
```bash
cd "tnd-platform-web"
npm run dev
```

### 🎨 Fonctionnalités Disponibles

#### ✅ Sélecteur de Profil
- 3 profils : Famille (👨‍👩‍👧), Clinicien (🏥), Enseignant (🎓)
- Cards cliquables avec animations

#### ✅ Dashboard Famille
- **Programme du jour** : 3 séances (1 complétée, 2 en attente)
- **Progression hebdomadaire** : 3/15 séances (20%), calendrier L-M-M-J-V-S-D
- **Aptitudes** : 5 compétences avec progression et évolution
  - Motricité : 80% (+5%)
  - Attention : 60% (+12%)
  - Coordination : 70% (stable)
  - Mémoire : 55% (-3%)
  - Logique : 75% (+8%)
- **Récompenses** : 4 badges (1 débloqué)
- **Actions rapides** : Lancer PopBalloons, Voir statistiques

#### ✅ Composants UI Réutilisables
- **Card** : Variants (default, primary, accent)
- **ProgressBar** : Variants (default, success, warning, gradient) avec évolution
- **Badge** : Types (bronze, silver, gold, special) + état locked/unlocked
- **SessionItem** : États (completed ✓, pending ○, in-progress ⟳)
- **DayIndicator** : Jours de la semaine avec complétion

### 🎨 Palette de Couleurs Tailwind

Configurée dans `tailwind.config.js` :

```javascript
colors: {
  family: {
    primary: '#4A90E2',    // Bleu
    secondary: '#7ED321',  // Vert
    accent: '#F5A623',     // Orange
  },
  clinician: {
    primary: '#2C5F8D',    // Bleu foncé
    secondary: '#17A2B8',  // Turquoise
    accent: '#6F42C1',     // Violet
  },
  teacher: {
    primary: '#28A745',    // Vert
    secondary: '#FFC107',  // Jaune
    accent: '#17A2B8',     // Bleu ciel
  },
}
```

### 📊 Données de Test

Patient : **Marie, 8 ans**

Séances aujourd'hui :
- ✓ 09:00 - PopBalloons Motricité (450 pts)
- ○ 14:00 - PopBalloons Cognitive
- ○ 17:00 - Exercices d'attention

### 🔧 Scripts NPM

- `npm run dev` : Serveur de développement (port 3000)
- `npm run build` : Build de production
- `npm start` : Serveur de production
- `npm run lint` : Vérification ESLint

### 📝 Prochaines Étapes

- [ ] Dashboard Clinicien
- [ ] Dashboard Enseignant
- [ ] API REST pour connexion Unity/HoloLens
- [ ] WebSocket pour communication temps réel
- [ ] Authentification (JWT)
- [ ] Base de données (PostgreSQL/MongoDB)
- [ ] Graphiques interactifs (Chart.js/Recharts)
- [ ] Déploiement (Vercel/Netlify)

<!--
## Execution Guidelines
PROGRESS TRACKING:
- If any tools are available to manage the above todo list, use it to track progress through this checklist.
- After completing each step, mark it complete and add a summary.
- Read current todo list status before starting each new step.

COMMUNICATION RULES:
- Avoid verbose explanations or printing full command outputs.
- If a step is skipped, state that briefly (e.g. "No extensions needed").
- Do not explain project structure unless asked.
- Keep explanations concise and focused.

DEVELOPMENT RULES:
- Use '.' as the working directory unless user specifies otherwise.
- Avoid adding media or external links unless explicitly requested.
- Use placeholders only with a note that they should be replaced.
- Use VS Code API tool only for VS Code extension projects.
- Once the project is created, it is already opened in Visual Studio Code—do not suggest commands to open this project in Visual Studio again.
- If the project setup information has additional rules, follow them strictly.

FOLDER CREATION RULES:
- Always use the current directory as the project root.
- If you are running any terminal commands, use the '.' argument to ensure that the current working directory is used ALWAYS.
- Do not create a new folder unless the user explicitly requests it besides a .vscode folder for a tasks.json file.
- If any of the scaffolding commands mention that the folder name is not correct, let the user know to create a new folder with the correct name and then reopen it again in vscode.

EXTENSION INSTALLATION RULES:
- Only install extension specified by the get_project_setup_info tool. DO NOT INSTALL any other extensions.

PROJECT CONTENT RULES:
- If the user has not specified project details, assume they want a "Hello World" project as a starting point.
- Avoid adding links of any type (URLs, files, folders, etc.) or integrations that are not explicitly required.
- Avoid generating images, videos, or any other media files unless explicitly requested.
- If you need to use any media assets as placeholders, let the user know that these are placeholders and should be replaced with the actual assets later.
- Ensure all generated components serve a clear purpose within the user's requested workflow.
- If a feature is assumed but not confirmed, prompt the user for clarification before including it.
- If you are working on a VS Code extension, use the VS Code API tool with a query to find relevant VS Code API references and samples related to that query.

TASK COMPLETION RULES:
- Your task is complete when:
  - Project is successfully scaffolded and compiled without errors
  - copilot-instructions.md file in the .github directory exists in the project
  - README.md file exists and is up to date
  - User is provided with clear instructions to debug/launch the project

Before starting a new task in the above plan, update progress in the plan.
-->
