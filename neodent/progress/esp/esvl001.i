&GLOBAL-DEFINE SERVIDOR_VAULT SERVIDOR_VAULT
&GLOBAL-DEFINE BANCO_VAULT    BANCO_VAULT
&GLOBAL-DEFINE USUARIO_VAULT  USUARIO_VAULT
&GLOBAL-DEFINE SENHA_VAULT    SENHA_VAULT
&GLOBAL-DEFINE DIR_DESENHOS   DIR_DESENHOS
&GLOBAL-DEFINE ECM_LOGIN      ECM_LOGIN
&GLOBAL-DEFINE ECM_SENHA      ECM_SENHA
&GLOBAL-DEFINE ECM_MATRICULA  ECM_MATRICULA
&GLOBAL-DEFINE ECM_URL        ECM_URL
&GLOBAL-DEFINE ECM_EMPRESA    ECM_EMPRESA
&GLOBAL-DEFINE ECM_ROOTFOLDER ECM_ROOTFOLDER

/**
 * Define a temp-table usada para buscar os desenhos do item.
 */
DEFINE TEMP-TABLE ttDesenhoItem
    FIELD itCodigo          LIKE desenho-item.it-codigo
    FIELD deCodigo          LIKE desenho-item.de-codigo
    FIELD nomUrl            AS CHARACTER
    FIELD nomArquivo        AS CHARACTER
    FIELD rowDesenhoItem    AS ROWID
    FIELD nomServidorVault  AS CHARACTER
    FIELD nomBaseVault      AS CHARACTER
    FIELD nomUsuarioVault   AS CHARACTER
    FIELD nomSenhaVault     AS CHARACTER
    FIELD nomDirDesenhos    AS CHARACTER
    FIELD ecmLogin          AS CHARACTER
    FIELD ecmPassword       AS CHARACTER
    FIELD ecmMatricula      AS CHARACTER
    FIELD ecmURL            AS CHARACTER
    FIELD ecmCompany        AS INTEGER
    FIELD ecmRootFolder     AS INTEGER
    .
