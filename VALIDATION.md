# Validation du projet

Date : 12 septembre 2026.

## Contrôles réalisés ici

- Analyse des 8 fichiers C# avec le parseur Tree-sitter C# : aucune erreur de syntaxe.
- `node --check` sur le JavaScript du template Web : réussi.
- Compilation syntaxique des outils Python : réussie.
- Simulation indépendante du guidage : **36 parcours de trois tours terminés**, soit 3 circuits × 3 difficultés × 4 adversaires.
- Temps simulés : **32,58 à 67,28 secondes** selon circuit et difficulté. Ce ne sont ni des benchmarks ni des temps validés dans Unity.
- Une première simulation a montré un point de passage manqué dans le deuxième circuit. Le guidage a été corrigé pour viser le prochain point à valider. La deuxième série a terminé les 36 parcours.
- Vérification de la structure du ZIP, du JSON des packages et des métadonnées Unity.
- Génération et ouverture du plan des trois circuits (`Circuits.png`), d'après les mêmes coordonnées et la même interpolation que le projet.

La simulation reprend accélération, direction, glissement latéral et validation successive des points. Elle ne simule pas les collisions entre voitures, les bonus, le turbo, le rendu, l'interface, ni les contraintes du navigateur. Ses résultats portent sur la continuité du parcours de base de l'IA.

Le script facultatif `Tools/validate_tracks.py` nécessite Python, NumPy et Matplotlib. Il réécrit le rapport JSON et le plan des circuits. Il n'est pas requis pour ouvrir, jouer ou exporter avec Unity.

## À exécuter dans Unity avant publication

L'éditeur Unity n'est pas disponible dans l'environnement de création. Il reste donc à effectuer les vérifications suivantes :

1. Importer le projet avec Unity 6.0 et laisser les packages se résoudre. Vérifier l'absence d'erreur dans la Console.
2. Ouvrir la scène via le menu Pocket GP et démarrer Play. Vérifier les trois circuits, les cinq couleurs et les trois difficultés.
3. Terminer une course ; vérifier tours, classement, record et fonctionnement de Rejouer.
4. Terminer les trois manches du championnat ; vérifier l'addition des points et l'écran du trophée.
5. Tester une sortie de route, le retour R, les contacts entre voitures et un parcours à contresens. Les tours ne doivent se valider qu'après les points successifs.
6. Tester freinage, dérapage, turbo vide/rechargé et collecte des jetons.
7. Tester pause/reprise, perte de focus, retour au menu et redimensionnement portrait/paysage.
8. Vérifier les volumes, le son test et la restitution audio après un clic utilisateur dans un navigateur.
9. Exporter avec Pocket GP > 3 - Construire pour le Web.
10. Servir l'export via HTTP/HTTPS puis jouer sur un navigateur desktop et sur les téléphones ciblés. Tester plusieurs appuis tactiles simultanés, l'affichage, les chargements et le mode Éco.

## Statut de livraison

**Sources complètes et procédure d'export préparée ; compilation Unity et validation sur navigateur/appareils à effectuer. Aucun build Web compilé n'est inclus.**
