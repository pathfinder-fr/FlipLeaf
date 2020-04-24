# Readers

Les readers sont les classes responsables de la lecture et de la transformation d'un fichier physique sur le disque en contenu pour le site.

Chaque type de reader est représenté par un interface `IxxxReader`.

# IContentReader : lecteur de contenu

Ces readers sont chargés de gérer les fichiers de contenu, pour les transformer à l'écran.

**HtmlContentReader.** Ce reader sait lire les fichiers statiques HTML.
Il interpréte leur entête YAML et applique la syntaxe liquid.

**MarkdownContentReader.** Ce reader est chargé de lire les fichiers markdown (.md).
Il interpréte leur entête YAML et applique la syntaxe liquid.

# IDataReader : lecteur de données

Ces readers savent lire les fichiers présents dans le dossier `_data` et peuvent contribuer à ajouter des données au site.

Les données du site sont ensuite accessible via la variable liquid `site.data`.

# ILiquidLayoutReader : lecteur de layouts liquid

Ils sont utilisés pour le rendu des fichiers avec la syntaxe liquid.

Cette syntaxe permet d'utiliser des layouts, qui servent à encapsuler le contenu dans un autre contenu partagé et réutilisable.

Ces readers sont chargés de lire le contenu des fichiers présents dans le dossier `_layouts`.