# Post-Apocalyptic Warfare : The Battle for Survival Begins

**Mini Projet Jeu 2D — Cours de jeux vidéo (Unity 6000.2)**  
**Cégep Marie-Victorin**

- **Nom :** Taha Yassine El Maaroufi
- **Numéro d'étudiant (DA) :** 2368229
- **Courriel :** 2368229@cegepmv.ca
- **Lien GitHub :** [https://github.com/Yassine3232/Mini-Project-Taha-Yassine-El-Maaroufi-](https://github.com/Yassine3232/Mini-Project-Taha-Yassine-El-Maaroufi-)

---

## 1. Histoire et but du jeu

L'histoire se passe pendant la Seconde Guerre mondiale. On joue Peter McCain, un espion américain de l'OSS.

Après une invasion de zombies, Peter est coincé dans l'asile de **Verrückt** en Allemagne. Il doit éliminer les zombies pour s'enfuir et aller vers le laboratoire secret caché dans les marécages de **Shi No Numa** en Mandchourie. Le but est de survivre aux vagues de zombies et de nettoyer chaque zone pour passer au niveau suivant.

---

## 2. Contrôles du jeu

- **Bouger à gauche et à droite :** Touches `A` / `D` ou flèches du clavier
- **Tirer :** Touche `Espace`

---

## 3. Déroulement du jeu

- Le joueur commence avec 100 points de vie (PV).
- Les zombies attaquent le joueur quand ils s'approchent de lui.
- Le joueur doit tirer sur les zombies pour les éliminer.
- **Niveau 1 (Verrückt) :** Il faut tuer 10 zombies pour faire apparaître le bouton du niveau suivant.
- **Niveau 2 (Shi No Numa) :** Les zombies sont plus gros, plus rapides et ont plus de vie.
- **Game Over :** Si la vie du joueur tombe à 0, l'écran de fin de partie s'affiche avec un bouton pour recommencer.

---

## 4. Les scripts du jeu

- `PlayerMovement.cs` : Permet au joueur de marcher de gauche à droite et de se tourner du bon côté.
- `PlayerAttack.cs` : Permet au joueur de tirer des balles avec la touche Espace.
- `Projectile.cs` : Fait avancer la balle et applique les dégâts aux zombies touchés.
- `EnemyPatrol.cs` : Fait patrouiller les zombies de gauche à droite avec leur animation de marche.
- `Enemychaseattack.cs` : Fait courir le zombie vers le joueur quand il le voit et l'attaque au corps à corps.
- `ZombieHealth.cs` : Gère les points de vie des zombies et les fait clignoter en rouge quand ils prennent des dégâts.
- `ZombieManager.cs` : Fait apparaître les zombies, affiche la barre de vie et le nombre de zombies restants, et débloque la suite.
- `SimpleCameraFollow.cs` : Fait en sorte que la caméra suive le joueur sans sortir de la carte.
- `ParallaxManager.cs` : Ajoute un effet de profondeur en déplaçant les arrière-plans à différentes vitesses.
- `GameOverController.cs` : Gère le bouton pour relancer le niveau après une défaite.

---

## 5. Ressources utilisées

- **Logiciel :** Unity 6000.2
- **Sprites et animations :** Packs gratuits trouvés sur CraftPix :
  - [Free Soldier Sprite Sheets](https://craftpix.net/freebies/free-soldier-sprite-sheets-pixel-art/)
  - [Soldier Zombie Character Sprite Sheets](https://craftpix.net/product/soldier-zombie-character-sprite-sheets-pixel-art/)
  - [Free Urban Zombie Sprite Sheet Pack](https://craftpix.net/freebies/free-urban-zombie-sprite-sheet-pixel-art-pack/)
  - [Free Zombie Sprite Sheet Pack](https://craftpix.net/freebies/free-zombie-sprite-sheet-pack-pixel-art/)
- **Utilisation de l'IA (Déclaration d'aide) :**
  J'ai utilisé un outil d'IA pour m'aider à débloquer et ajuster certaines mécaniques précises du jeu :
  - Corriger la direction des tirs de balle pour que le joueur tire bien vers le côté où il regarde.
  - Corriger la logique d'attaque des zombies au corps à corps quand ils s'approchent trop près du joueur (éviter qu'ils cessent d'attaquer ou se bloquent).
  - Bloquer la caméra sur les bords de la carte pour éviter d'afficher le vide en dehors du niveau.
  - Compléter la logique du script `ZombieManager` à qui il manquait plusieurs fonctionnalités (gestion des vagues et passage des paramètres comme la taille et les points de vie).
  
  Tout le reste du projet et de l'intégration a été réalisé par moi-même.
