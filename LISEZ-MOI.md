# Pocket Grand Prix

Jeu de course miniature 3D pour Unity 6, conçu pour le Web sur ordinateur et téléphone.
Version source 1.0 — 12 septembre 2026.

## Ouvrir et jouer

1. Décompresser le ZIP en conservant le dossier **PocketGrandPrix**.
2. Dans Unity Hub, ajouter ce dossier comme projet existant.
3. Utiliser **Unity 6.0 LTS** avec le module **Web Build Support**. La référence de projet est 6000.0.23f1 ; une révision plus récente de Unity 6.0 peut mettre le projet à niveau. Le téléchargement de l'éditeur et la résolution du package officiel Unity UI nécessitent Internet au premier lancement.
4. Attendre la fin de l'importation et de la compilation. Le projet applique sa configuration initiale et ouvre la scène.
5. Cliquer sur **Play**. Si nécessaire : **Pocket GP > 2 - Ouvrir la scène**.

La scène initiale est volontairement minimale : le code construit voitures, routes, décors, éclairage, interface et audio au lancement. C'est normal de ne pas voir le circuit en mode édition. Tout le contenu visuel est procédural ; aucun modèle ou asset payant n'est requis. Les tracés se modifient dans GPTrack.cs et les décors dans GPArt.cs.

## Export Web

1. Installer le module **Web Build Support** correspondant exactement à votre éditeur dans Unity Hub.
2. Quitter le mode Play.
3. Choisir **Pocket GP > 3 - Construire pour le Web**.
4. L'export est créé dans **Builds/Web/**. La commande signale une erreur si le module Web manque ou si la compilation échoue.
5. Publier **tout le contenu** de ce dossier, en conservant son arborescence, sur un hébergement statique HTTPS. La page d'entrée est `index.html`.

Le template inclut un écran de chargement, une erreur lisible et un bouton plein écran lorsque le navigateur le permet. L'export non compressé simplifie l'hébergement : aucun en-tête gzip/Brotli particulier n'est demandé, au prix d'un téléchargement plus volumineux. Le serveur doit servir `.wasm` avec le type `application/wasm`. Ne pas ouvrir `index.html` par double-clic en `file://`.

Pour tester localement après l'export, avec Python 3 installé :

```sh
python Tools/serve_web.py
```

Puis ouvrir **http://localhost:8080**. Pour tester un téléphone sur le même réseau, ouvrir `http://ADRESSE-IP-DU-PC:8080` et autoriser le serveur local dans le pare-feu si nécessaire. Cela teste le jeu, mais certaines fonctions Web telles que le plein écran peuvent varier selon le navigateur.

Export automatisé, depuis la racine du projet (adapter le chemin de l'éditeur) :

```sh
Unity -batchmode -quit -projectPath . -executeMethod PocketGP.Editor.GPBuild.BuildWeb -logFile build.log
```

Un éditeur installé, activé et muni du module Web est requis. Une exportation Unity produit des fichiers HTML, JavaScript, données et WebAssembly : le projet n'est pas un jeu HTML autonome en un seul fichier.

## Contenu jouable

- Trois circuits : **Petit-déjeuner express**, **Bureau en folie**, **Jardin des champions**.
- Une voiture contrôlée par le joueur et quatre adversaires IA.
- Course rapide ou championnat des trois circuits.
- Trois tours par course, classement et validation des points de passage dans l'ordre.
- Trois difficultés influençant la vitesse des adversaires ; cinq couleurs de voiture.
- Accélération automatique, freinage, dérapage et réserve de turbo.
- Petit turbo après un dérapage tenu plus de 0,7 seconde.
- Jetons : 100 points et recharge de turbo. Plaques vertes : accélération temporaire.
- Ralentissement hors piste et retour automatique après une sortie importante.
- Pause, recommencer, résultats, records locaux séparés par circuit/difficulté.
- Championnat : 10 / 7 / 5 / 3 / 1 points selon la place ; trophée selon le total du joueur.
- Musique originale synthétisée, moteur et effets ; volumes et son test.
- Interface française et commandes tactiles ; affichage adaptatif portrait/paysage.

Le championnat attribue un trophée à votre total personnel, sans classement cumulé des adversaires. Les records sont enregistrés dans le stockage local du navigateur : pas de classement mondial, pas de compte, pas de multijoueur. Les décors sont visuels ; la limite principale est le ralentissement hors piste. Les collisions entre voitures utilisent un modèle arcade simplifié.

## Commandes

| Action | Ordinateur | Téléphone |
|---|---|---|
| Accélérer | Automatique | Automatique |
| Tourner | Flèches gauche/droite, Q/D ou A/D | Boutons ‹ et › |
| Freiner | Flèche bas ou S | FREIN |
| Déraper | Espace | DRIFT maintenu |
| Turbo | Maj | TURBO maintenu |
| Retour au dernier point validé | R | Automatique après une sortie importante |
| Pause | P ou Échap | PAUSE |

La perte de focus met la course en pause. Le mode Éco coupe les ombres et l'anticrénelage. Le rendu Web limite la densité de pixels à 1,5 pour éviter une résolution interne excessive sur téléphone. Paysage conseillé pour davantage de visibilité.

## Architecture

| Fichier | Rôle |
|---|---|
| `GPRace.cs` | Initialisation, états, course, caméra, bonus, sauvegarde |
| `GPCar.cs` | Conduite arcade, IA, tours, dérapage, traces |
| `GPTrack.cs` | Tracés Catmull-Rom, route, points de contrôle, regroupement des décors |
| `GPArt.cs` | Voitures et objets 3D, matériaux réutilisés |
| `GPUi.cs` | Menus, HUD, saisie tactile et réglages |
| `GPAudio.cs` | Musique et effets générés en mémoire |
| `Editor/GPBuild.cs` | Configuration initiale et export Web |
| `Resources/PocketLit.shader` | Shader du pipeline de rendu intégré |
| `Assets/WebGLTemplates/PocketGP/index.html` | Page de chargement Web |

Utiliser le **pipeline de rendu intégré** de Unity. Ne pas convertir ce projet en URP/HDRP sans adapter le shader. Les déplacements sont intégrés à pas fixe ; le jeu n'utilise pas WheelCollider. Les matériaux sont mutualisés et les décors statiques sont fusionnés par matériau. Les objets de bonus sont réutilisés pendant la course.

## Vérifications effectuées et limites

L'environnement de création ne disposait pas de l'éditeur Unity. **Le projet n'a donc pas été compilé ou exécuté dans Unity et aucun export Web compilé n'est fourni.** Le dossier livré contient les sources et les outils d'export, pas un build validé à mettre directement en ligne.

Vérifications effectuées : analyse syntaxique des huit fichiers C#, contrôle syntaxique du JavaScript du template, structure de l'archive et simulation indépendante du guidage IA et de la validation des tours. Voir `VALIDATION.md` pour les résultats exacts. Une analyse syntaxique ne vérifie pas les références aux API Unity ni le rendu.

Avant publication, effectuer les essais listés dans `VALIDATION.md` sur l'éditeur choisi, puis dans les navigateurs visés. Les performances mobiles, le rendu et le confort des commandes doivent encore être vérifiés sur appareils réels. Aucun nombre d'images par seconde n'est garanti.

## Identité et droits

Pocket Grand Prix est une création originale inspirée du genre des courses miniatures. Le projet n'inclut pas les graphismes, marques, voitures sous licence, musiques ou niveaux de Micro Machines. Les formes et sons sont générés par le code fourni. Vous pouvez modifier ce contenu pour votre jeu ; l'utilisation et la distribution du moteur et des packages Unity restent soumises à leurs conditions respectives.

Documentation Unity :
- [Compatibilité navigateurs](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-browsercompatibility.html)
- [Structure d'un export Web](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-building.html)
