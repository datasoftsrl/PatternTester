# Lingue esterne di Pattern Tester

Le lingue dell'interfaccia sono caricate dalla cartella `Languages` accanto all'eseguibile.

## Aggiungere una lingua

Copiare `it.json` o `en.json` in un nuovo file, ad esempio `de.json`, e tradurre i valori.
Il campo `_code` identifica la lingua e `_name` è il nome mostrato nelle Impostazioni.

Esempio:

```json
{
  "_code": "de",
  "_name": "Deutsch",
  "_help": "PatternTester_Guida_de.html",
  "Menu.File": "_Datei"
}
```

Il programma rileva automaticamente i nuovi file `.json` al successivo avvio. Non è necessario ricompilare.

Se `_help` non viene indicato oppure il file HTML non esiste, la Guida utilizza automaticamente la versione italiana come fallback.

Le traduzioni mancanti in un file vengono inoltre completate usando `it.json`.
