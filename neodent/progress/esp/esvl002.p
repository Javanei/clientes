/**
 * Busca o caminho do arquivo do desenho
 */

{utp/ut-glob.i}
{esp/esvl001.i}

DEFINE INPUT  PARAMETER pItCodigo       AS CHAR  NO-UNDO.
DEFINE OUTPUT PARAMETER TABLE FOR ttDesenhoItem.

RUN esp/esvl001.p (INPUT pItCodigo, OUTPUT TABLE ttDesenhoItem).
IF RETURN-VALUE <> "OK":U THEN
    RETURN RETURN-VALUE.
FIND LAST ttDesenhoItem NO-ERROR.
IF NOT AVAIL ttDesenhoItem THEN DO:
    RETURN "Item nÆo possui desenho".
END.

/* Busca o nome do servidor Vault */
FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&SERVIDOR_VAULT}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&SERVIDOR_VAULT}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&SERVIDOR_VAULT}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.nomServidorVault = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com o nome do servidor Vault".
END.

/* Busca o nome da base Vault */
FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&BANCO_VAULT}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&BANCO_VAULT}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&BANCO_VAULT}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.nomBaseVault = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com o nome da base Vault".
END.

/* Busca o nome do usuario Vault */
FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&USUARIO_VAULT}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&USUARIO_VAULT}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&USUARIO_VAULT}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.nomUsuarioVault = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com o nome do usu rio Vault".
END.

/* Busca a senha do usuario Vault */
FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&SENHA_VAULT}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&SENHA_VAULT}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&SENHA_VAULT}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.nomSenhaVault = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com a senha do usu rio Vault".
END.

/* Busca o diret¢rio onde os desenhos estÆo pr‚-convertidos */
FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&DIR_DESENHOS}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&DIR_DESENHOS}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&DIR_DESENHOS}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.nomDirDesenhos = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com a pasta dos desenhos pr‚-convertidos".
END.

/* ECM */
FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_LOGIN}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_LOGIN}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_LOGIN}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmLogin = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com o nome do usu rio ECM".
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_SENHA}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_SENHA}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_SENHA}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmPassword = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com a senha do usu rio ECM".
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_MATRICULA}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_MATRICULA}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_MATRICULA}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmMatricula = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmMatricula = ttDesenhoItem.ecmLogin.
    END.
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_URL}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_URL}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_URL}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmURL = ext_param_geral.val_parametro.
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com a URL do ECM".
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_EMPRESA}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_EMPRESA}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_EMPRESA}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmCompany = INTEGER(ext_param_geral.val_parametro).
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com a Empresa do ECM".
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDER}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDER}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDER}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    FOR EACH ttDesenhoItem:
        ASSIGN ttDesenhoItem.ecmRootFolder = INTEGER(ext_param_geral.val_parametro).
    END.
END.
ELSE DO:
    RETURN "NÆo encontrado parƒmetro com a pasta raiz do ECM".
END.


/* -------------------- */

FIND FIRST ttDesenhoItem NO-ERROR.
IF AVAIL ttDesenhoItem THEN
    RETURN "OK":U.
ELSE
    RETURN "NÆo encontrado o desenho do item".
