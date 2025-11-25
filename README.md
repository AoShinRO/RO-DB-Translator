# RO-DB-Translator
The RO-DB-Translator is a Windows desktop utility that translates Ragnarok Online database files from rAthena YML format into different languages using the DivinePride API.

# Project Purpose

The application solves the problem of manually translating game database entries for Ragnarok Online private servers. Server administrators need to convert item names and monster names in item_db.yml and mob_db.yml files from one language to another (Portuguese Brazilian, English US). Rather than manually translating hundreds or thousands of entries, this tool automates the process by querying the DivinePride API, which maintains an authoritative database of official Ragnarok Online game data in multiple languages.

The tool reads YML database files, extracts entity IDs, queries the DivinePride API for translated names based on the selected target language, and writes new YML files with translated names to an output directory.

# Wiki

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/AoShinRO/RO-DB-Translator)
