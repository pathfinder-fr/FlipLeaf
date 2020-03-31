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
* (TODO) prévisualisation du rendu avant enregistrement
* (TODO) enregistrement temporaire des pages dans le stockage local du navigateur

## Instructions

* Tous les OS supportés par .net core sont gérés : Windows, Linux ou macOS.
* Nécessite le [SDK .net core](https://dotnet.microsoft.com/download) installé.
* Lancez la commande `dotnet run -p FlipLeaf.Web`.
* Ouvrez votre navigateur à la page `http://localhost:5000`.
* L’éditeur s’ouvre pour modifier votre page d’accueil, vous pouvez saisir un contenu au format markdown et cliquez sur le bouton `Save`
* Votre contenu est sauvegardé dans le dossier `FlipLeaf.Web/.content` qui est automatiquement converti comme repository git.

## Tips

* L'éditeur supporte la syntaxe de liens wiki type `[[page]]` et `[[mon titre|page]]`
* Cliquer sur un lien vers une page inexistante vous amène automatiquement sur l'éditeur pour créer la page
* Le commentaire en bas est utilisé comme message pour le commit git
* Vous pouvez créer une page en saisissant simplement son url dans la barre d'adresse
* Vous pouvez parcourir les pages via l'url `http://localhost:5000/_manage`