# FlipLeaf

FlipLeaf est un projet hybride entre un CMS, un générateur de sites statiques et un wiki.

Principales fonctionnalités :

* Stockage des données sur fichiers
* Utilisation de git pour gérer un historique et un audit des opérations sur fichiers
* Rendu des fichiers [markdown](https://fr.wikipedia.org/wiki/Markdown) sous forme de pages via [markdig](https://github.com/lunet-io/markdig)
* Entête de page format YAML
* Syntaxe [liquid](https://shopify.github.io/liquid/) sur les pages
* Site asp.net core pour le rendu et la gestion des pages
* (TODO) templates de formulaire de gestion des pages
* (TODO) prévisualisation du rendu avant enregistrement
* (TODO) enregistrement temporaire des pages dans le stockage local du navigateur

## Instructions de démarrage

* Tous les OS supportés par .net core sont gérés : Windows, Linux ou macOS.
* Nécessite le [SDK .NET Core](https://dotnet.microsoft.com/download) installé.
  La commande `dotnet` doit être disponible depuis l'invite de commande.
* Pour démarrer le site, lancez la commande `dotnet run -p FlipLeaf.Web` depuis le dossier du projet.
* Ouvrez votre navigateur à la page `http://localhost:5000`.
* L’éditeur s’ouvre pour modifier votre page d’accueil, vous pouvez saisir un contenu au format markdown et l’enregistrer.
* Votre contenu est sauvegardé dans le dossier `FlipLeaf.Web/.content` qui est automatiquement converti comme repository git.

## Tips

* L’éditeur supporte la syntaxe de liens wiki type `[[page]]` et `[[mon titre|page]]`
* Cliquer sur un lien vers une page inexistante vous amène automatiquement sur l’éditeur pour créer la page
* Le commentaire en bas est utilisé comme message pour le commit git
* Vous pouvez créer une page en saisissant simplement son url dans la barre d’adresse
* Vous pouvez parcourir les pages via l’url `http://localhost:5000/_manage`
* Le titre de la page se définit via l’entête `title`
* Le document par défaut d’un dossier est `index.md` (ou `index.html`)
* Vous pouvez directement créer des pages HTML, elles sont prioritaires sur les pages markdown

## Syntaxe liquid et entête Yaml

Vous pouvez indiquer des variables dans l’entête du fichier markdown ou html.

Une entête YAML peut être insérée en début de fichier.
Elle est délimitée par une ligne de `---` avant et après l’entête.

```md
---
title: Mon titre
category: Page
---
# Contenu
```

Dans cet exemple, l’entête YAML définit une variable `title` et une variable `category`.
(Notez que la variable `title` est spéciale, elle est reprise comme titre de la page).

Pour utiliser le contenu d’une variable dans une page, vous pouvez utiliser la syntaxe liquid `{{ title }}`.

## Configuration

La configuration du site se fait via le fichier `FlipLeaf.Web/appsettings.json`.

Pour modifier les valeurs par défaut, recopiez les valeurs ci-dessous dans votre fichier:

```json
{
    "FlipLeaf": {
        // location where the content is stored.
        // can be relative to the site directory or an absolute path
        // directory will be created if necessary
        "SourcePath": ".content",

        // base location of the website
        // used to transform relative links
        // beware to keep the end '/' 
        "BaseUrl": "http://localhost:5000/"
    }
}

```

