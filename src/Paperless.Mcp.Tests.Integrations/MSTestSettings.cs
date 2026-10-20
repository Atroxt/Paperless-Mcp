// Diese Tests teilen sich EINE echte Paperless-Instanz und veraendern deren Datenbestand.
// Parallel ausgefuehrt treten sie sich gegenseitig auf die Fuesse: ein Test loescht einen
// Tag, waehrend ein anderer ihn gerade einem Dokument zuweist -- Paperless antwortet dann
// mit 500, und die Fehlerzahl schwankt von Lauf zu Lauf.
//
// Die Unit-Tests duerfen weiter parallel laufen, die haben keine geteilte Aussenwelt.
[assembly: DoNotParallelize]
