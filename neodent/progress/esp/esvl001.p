/**
 * Busca os desenhos do item
 */

{esp/esvl001.i}

DEFINE INPUT  PARAMETER pItCodigo       AS CHAR  NO-UNDO.
DEFINE OUTPUT PARAMETER TABLE FOR ttDesenhoItem.

FIND ITEM NO-LOCK
    WHERE ITEM.it-codigo = pItCodigo
    NO-ERROR.
IF NOT AVAIL ITEM THEN DO:
    RETURN "Item " + pItCodigo + " n∆o existe".
END.

EMPTY TEMP-TABLE ttDesenhoItem.
FOR EACH desenho-item NO-LOCK
    WHERE desenho-item.it-codigo = pItCodigo:
    CREATE ttDesenhoItem.
    ASSIGN  ttDesenhoItem.itCodigo = desenho-item.it-codigo
            ttDesenhoItem.deCodigo = desenho-item.de-codigo
            ttDesenhoItem.rowDesenhoItem = ROWID(desenho-item).
END.
IF CAN-FIND(FIRST ttDesenhoItem) THEN DO:
    RETURN "OK":U.
END.
RETURN "Item " + pItCodigo + " n∆o possui nenhum desenho associado".
