# FlipLeaf

FlipLeaf est un projet hybride entre un CMS, un générateur de sites statiques et un wiki.

Principales fonctionnalités :

* Stockage des données sur fichiers
* Utilisation de git pour gérer un historique et un audit des opérations sur fichiers
* Rendu des fichiers markdown sous forme de pages
* Syntaxe liquid sur les pages
* Entête de page format YAML
* Site asp.net core pour le rendu et la gestion des pages
* (TODO) templates de formulaire de gestion des pages

## Instructions

* Tous les OS supportés par .net core sont gérés : Windows, Linux ou macOS.
* Nécessite le [SDK .net core](https://dotnet.microsoft.com/download) installé.
* Lancez la commande `dotnet run -p FlipLeaf.Web`.
* Ouvrez votre navigateur à la page `http://localhost:5000`.
* L’éditeur s’ouvre pour modifier votre page d’accueil, vous pouvez saisir un contenu au format markdown et cliquez sur le bouton `Save`
* Votre contenu est sauvegardé dans le dossier `FlipLeaf.Web/.content` qui est automatiquement géré comme repository git.