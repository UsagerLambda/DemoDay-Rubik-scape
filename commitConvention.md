# 📝 Convention de Commit Rubik'scape

## 🏗️ Structure Générale des Commits
```
<type>(<scope>): <description concise>

[corps du commit optionnel]

[pied de page optionnel]
```

## 📋 Types de Commits

### Principaux Types
- `feat`: Nouvelle fonctionnalité
- `fix`: Correction de bug
- `docs`: Modifications de documentation
- `style`: Corrections de formatage
- `refactor`: Refactorisation du code
- `test`: Ajout ou modification de tests
- `chore`: Tâches de maintenance
- `perf`: Amélioration de performance
- `build`: Modifications des fichiers de build
- `ci`: Changements dans la configuration CI

## 🎯 Scopes Spécifiques à Rubik'scape
- `cube`: Mécanique de rotation du cube
- `tiles`: Système de tuiles
- `xr`: Interactions réalité augmentée
- `ui`: Interface utilisateur
- `sdk`: Intégration Meta Quest SDK
- `path`: Algorithme de génération de chemins
- `interaction`: Gestion des interactions

## 📌 Exemples Concrets

### Nouveautés
```
feat(cube): implémentation de la rotation de base du cube
```

### Corrections
```
fix(xr): correction du tracking des mains lors des rotations
```

### Améliorations
```
refactor(tiles): optimisation de l'algorithme de génération de chemins
```

### Documentation
```
docs(readme): mise à jour des instructions d'installation
```

## 🚨 Règles Supplémentaires
- Utilisez l'impératif présent
- Première lettre en minuscule
- Pas de point final
- Limiter à 72 caractères maximum
- Langue: Français recommandé

## 🔍 Exemple Complet
```
feat(xr): ajout du système de validation de parcours

- Implémentation de l'algorithme de vérification
- Gestion des connexions de tuiles
- Feedback visuel pour les chemins invalides

Resolves #42
```

## 🤝 Workflow Recommandé
1. Créer une branche par feature
2. Commits atomiques et clairs
3. Pull Request avec description détaillée
4. Revue de code systématique

*Version 1.0 - Décembre 2024*
