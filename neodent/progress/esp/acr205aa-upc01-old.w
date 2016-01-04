&ANALYZE-SUSPEND _VERSION-NUMBER AB_v10r12 GUI ADM2
&ANALYZE-RESUME
/* Connected Databases 
*/
&Scoped-define WINDOW-NAME CURRENT-WINDOW
&Scoped-define FRAME-NAME gDialog
{adecomm/appserv.i}
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _DEFINITIONS gDialog 
/*------------------------------------------------------------------------

  File: 

  Description: from cntnrdlg.w - ADM2 SmartDialog Template

  Input Parameters:
      <none>

  Output Parameters:
      <none>

  Author: 

  Created: 
------------------------------------------------------------------------*/
/*          This .W file was created with the Progress AppBuilder.      */
/*----------------------------------------------------------------------*/

/* Create an unnamed pool to store all the widgets created 
     by this procedure. This is a good default which assures
     that this procedure's triggers and internal procedures 
     will execute in this procedure's storage, and that proper
     cleanup will occur on deletion of the procedure. */

CREATE WIDGET-POOL.

/* ***************************  Definitions  ************************** */

/* Parameters Definitions ---                                           */

/* Local Variable Definitions ---                                       */

{src/adm2/widgetprto.i}

&GLOBAL-DEFINE ECM_LOGIN      ECM_LOGIN
&GLOBAL-DEFINE ECM_SENHA      ECM_SENHA
&GLOBAL-DEFINE ECM_MATRICULA  ECM_MATRICULA
&GLOBAL-DEFINE ECM_URL        ECM_URL
&GLOBAL-DEFINE ECM_EMPRESA    ECM_EMPRESA
&GLOBAL-DEFINE ECM_ROOTFOLDERNAME ECM_ROOTFOLDERNAME
&GLOBAL-DEFINE ECM_ROOTFOLDERID ECM_ROOTFOLDERID
&GLOBAL-DEFINE ECM_PROXYSERVER ECM_PROXYSERVER
&GLOBAL-DEFINE ECM_PROXYPORT  ECM_PROXYPORT
&GLOBAL-DEFINE ECM_PROXYUSER  ECM_PROXYUSER
&GLOBAL-DEFINE ECM_PROXYPASS  ECM_PROXYPASS
&GLOBAL-DEFINE ECM_GROUPUPLOAD  ECM_GROUPUPLOAD
&GLOBAL-DEFINE ECM_GROUPREMOVE  ECM_GROUPREMOVE

DEFINE INPUT PARAMETER p_rec_cliente AS RECID NO-UNDO.

def var c-versao-prg as char initial " 1.00.00.005":U no-undo.

{include/i_dbinst.i}
{include/i_dbtype.i}
{include/i_fcldef.i}

DEF STREAM s1.

def new global shared var v_cod_usuar_corren
    as character
    format "x(12)":U
    label "Usuario Corrente"
    column-label "Usuario Corrente"
    no-undo.

DEF VAR ecm_login AS CHAR NO-UNDO.
DEF VAR ecm_pass AS CHAR NO-UNDO.
DEF VAR ecm_user AS CHAR NO-UNDO.
DEF VAR ecm_url AS CHAR NO-UNDO.
DEF VAR ecm_company AS INTEGER NO-UNDO INIT 1.
DEF VAR ecm_rootfolder AS CHAR NO-UNDO.
DEF VAR ecm_rootfolderid AS INT NO-UNDO INIT 0.
DEF VAR ecm_proxyserver AS CHAR NO-UNDO INIT "noproxy":U.
DEF VAR ecm_proxyport AS INT NO-UNDO INIT 0.
DEF VAR ecm_proxyuser AS CHAR NO-UNDO INIT "":U.
DEF VAR ecm_proxypass AS CHAR NO-UNDO INIT "":U.
DEF VAR ecm_canupload AS LOGICAL NO-UNDO INIT FALSE.
DEF VAR ecm_canremove AS LOGICAL NO-UNDO INIT FALSE.

DEF VAR cCmd AS CHAR NO-UNDO.
DEF VAR cOutputFile AS CHAR NO-UNDO.
DEF VAR cLine AS CHAR NO-UNDO.
DEF VAR cProxyParam AS CHAR NO-UNDO INIT "".

DEF TEMP-TABLE tt-docs NO-UNDO
    FIELD id AS INTEGER LABEL "ID"
    FIELD VERSION AS INTEGER LABEL "Version"
    FIELD TYPE AS INTEGER LABEL "Type"
    FIELD NAME AS CHAR LABEL "Arquivo" FORMAT "x(60)"
    .

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-PREPROCESSOR-BLOCK 

/* ********************  Preprocessor Definitions  ******************** */

&Scoped-define PROCEDURE-TYPE SmartDialog
&Scoped-define DB-AWARE no

&Scoped-define ADM-CONTAINER DIALOG-BOX

&Scoped-define ADM-SUPPORTED-LINKS Data-Target,Data-Source,Page-Target,Update-Source,Update-Target

/* Name of designated FRAME-NAME and/or first browse and/or first query */
&Scoped-define FRAME-NAME gDialog
&Scoped-define BROWSE-NAME br_docs

/* Internal Tables (found by Frame, Query & Browse Queries)             */
&Scoped-define INTERNAL-TABLES tt-docs

/* Definitions for BROWSE br_docs                                       */
&Scoped-define FIELDS-IN-QUERY-br_docs tt-docs.NAME   
&Scoped-define ENABLED-FIELDS-IN-QUERY-br_docs   
&Scoped-define SELF-NAME br_docs
&Scoped-define QUERY-STRING-br_docs FOR EACH tt-docs
&Scoped-define OPEN-QUERY-br_docs OPEN QUERY {&SELF-NAME} FOR EACH tt-docs.
&Scoped-define TABLES-IN-QUERY-br_docs tt-docs
&Scoped-define FIRST-TABLE-IN-QUERY-br_docs tt-docs


/* Definitions for DIALOG-BOX gDialog                                   */
&Scoped-define OPEN-BROWSERS-IN-QUERY-gDialog ~
    ~{&OPEN-QUERY-br_docs}

/* Standard List Definitions                                            */
&Scoped-Define ENABLED-OBJECTS RECT-1 br_docs Btn_OK btUpload btRemove 
&Scoped-Define DISPLAYED-OBJECTS vCodCliente vNomCliente 

/* Custom List Definitions                                              */
/* List-1,List-2,List-3,List-4,List-5,List-6                            */

/* _UIB-PREPROCESSOR-BLOCK-END */
&ANALYZE-RESUME



/* ***********************  Control Definitions  ********************** */

/* Define a dialog box                                                  */

/* Definitions of the field level widgets                               */
DEFINE BUTTON Btn_OK AUTO-GO 
     LABEL "OK" 
     SIZE 15 BY 1.13.

DEFINE BUTTON btRemove 
     LABEL "Exclui documento" 
     SIZE 17.86 BY 1.13.

DEFINE BUTTON btUpload 
     LABEL "Novo documento" 
     SIZE 20 BY 1.13.

DEFINE VARIABLE vCodCliente AS CHARACTER FORMAT "X(16)":U INITIAL "0001" 
     LABEL "Cliente" 
     VIEW-AS FILL-IN 
     SIZE 14 BY 1 NO-UNDO.

DEFINE VARIABLE vNomCliente AS CHARACTER FORMAT "X(40)":U INITIAL "Cliente 0001" 
     VIEW-AS FILL-IN 
     SIZE 52 BY 1 NO-UNDO.

DEFINE RECTANGLE RECT-1
     EDGE-PIXELS 2 GRAPHIC-EDGE  NO-FILL   
     SIZE 76.72 BY 1.67.

/* Query definitions                                                    */
&ANALYZE-SUSPEND
DEFINE QUERY br_docs FOR 
      tt-docs SCROLLING.
&ANALYZE-RESUME

/* Browse definitions                                                   */
DEFINE BROWSE br_docs
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _DISPLAY-FIELDS br_docs gDialog _FREEFORM
  QUERY br_docs DISPLAY
      tt-docs.NAME
/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME
    WITH NO-ROW-MARKERS SEPARATORS SIZE 76 BY 6.75 FIT-LAST-COLUMN TOOLTIP "Documentos".


/* ************************  Frame Definitions  *********************** */

DEFINE FRAME gDialog
     vCodCliente AT ROW 1.33 COL 8 COLON-ALIGNED WIDGET-ID 2
     vNomCliente AT ROW 1.33 COL 22.72 COLON-ALIGNED NO-LABEL WIDGET-ID 4
     br_docs AT ROW 3.25 COL 2 WIDGET-ID 200
     Btn_OK AT ROW 10.5 COL 3
     btUpload AT ROW 10.5 COL 20 WIDGET-ID 8
     btRemove AT ROW 10.5 COL 41.14 WIDGET-ID 10
     RECT-1 AT ROW 1.08 COL 1.29 WIDGET-ID 6
     SPACE(0.70) SKIP(9.41)
    WITH VIEW-AS DIALOG-BOX KEEP-TAB-ORDER 
         SIDE-LABELS NO-UNDERLINE THREE-D  SCROLLABLE 
         TITLE "Documentos do Cliente"
         DEFAULT-BUTTON Btn_OK WIDGET-ID 100.


/* *********************** Procedure Settings ************************ */

&ANALYZE-SUSPEND _PROCEDURE-SETTINGS
/* Settings for THIS-PROCEDURE
   Type: SmartDialog
   Allow: Basic,Browse,DB-Fields,Query,Smart
   Container Links: Data-Target,Data-Source,Page-Target,Update-Source,Update-Target
   Other Settings: COMPILE APPSERVER
 */
&ANALYZE-RESUME _END-PROCEDURE-SETTINGS

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _INCLUDED-LIB gDialog 
/* ************************* Included-Libraries *********************** */

{src/adm2/containr.i}

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME




/* ***********  Runtime Attributes and AppBuilder Settings  *********** */

&ANALYZE-SUSPEND _RUN-TIME-ATTRIBUTES
/* SETTINGS FOR DIALOG-BOX gDialog
   FRAME-NAME                                                           */
/* BROWSE-TAB br_docs vNomCliente gDialog */
ASSIGN 
       FRAME gDialog:SCROLLABLE       = FALSE
       FRAME gDialog:HIDDEN           = TRUE.

/* SETTINGS FOR FILL-IN vCodCliente IN FRAME gDialog
   NO-ENABLE                                                            */
/* SETTINGS FOR FILL-IN vNomCliente IN FRAME gDialog
   NO-ENABLE                                                            */
/* _RUN-TIME-ATTRIBUTES-END */
&ANALYZE-RESUME


/* Setting information for Queries and Browse Widgets fields            */

&ANALYZE-SUSPEND _QUERY-BLOCK BROWSE br_docs
/* Query rebuild information for BROWSE br_docs
     _START_FREEFORM
OPEN QUERY {&SELF-NAME} FOR EACH tt-docs
     _END_FREEFORM
     _Query            is OPENED
*/  /* BROWSE br_docs */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _QUERY-BLOCK DIALOG-BOX gDialog
/* Query rebuild information for DIALOG-BOX gDialog
     _Options          = "SHARE-LOCK"
     _Query            is NOT OPENED
*/  /* DIALOG-BOX gDialog */
&ANALYZE-RESUME

 



/* ************************  Control Triggers  ************************ */

&Scoped-define SELF-NAME gDialog
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL gDialog gDialog
ON ENTRY OF FRAME gDialog /* Documentos do Cliente */
DO:
    ASSIGN btUpload:SENSITIVE = ecm_canupload.
    ASSIGN btRemove:SENSITIVE = ecm_canremove.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL gDialog gDialog
ON WINDOW-CLOSE OF FRAME gDialog /* Documentos do Cliente */
DO:  
  /* Add Trigger to equate WINDOW-CLOSE to END-ERROR. */
  APPLY "END-ERROR":U TO SELF.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define BROWSE-NAME br_docs
&Scoped-define SELF-NAME br_docs
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL br_docs gDialog
ON MOUSE-SELECT-DBLCLICK OF br_docs IN FRAME gDialog
DO:
    DEF VAR cURL AS CHAR NO-UNDO.

    IF AVAIL tt-docs AND (ecm_canremove OR ecm_canupload) THEN DO:
        /*
        ASSIGN cURL = "START " + ecm_url + "/documentviewer?WDNrDocto=" + STRING(tt-docs.id) + "&WDNrVersao=" + STRING(tt-docs.version).
        OS-COMMAND NO-WAIT VALUE(cURL).
        */
        FILE-INFO:FILE-NAME = "ecmfiles.exe":U.
        IF FILE-INFO:FULL-PATHNAME = ? THEN
            FILE-INFO:FILE-NAME = "esp/ecmfiles.exe":U.
        ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
                + " -ecmlogin ~"":U + ecm_login + "~""
                + " -ecmuser ~"":U + ecm_user + "~""
                + " -ecmpassword ~"":U + ecm_pass + "~""
                + " -ecmurl ~"":U + ecm_url + "~""
                + " -ecmcompany ~"":U + string(ecm_company) + "~""
                + " -action ~"view~""
                + " -ecmDocumentId " + STRING(tt-docs.id)
                + cProxyParam
                + " -ecmRootFolder " + STRING(ecm_rootfolderid)
                + " -ecmFolder ~"" + (IF ecm_rootfolderid = 0 THEN ecm_rootfolder + "/" + vCodCliente ELSE vCodCliente) + "~""
                + " > " + SESSION:TEMP-DIRECTORY + vCodCliente + ".view.log"
            .

        OS-COMMAND SILENT VALUE(cCmd).
    END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btRemove
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btRemove gDialog
ON CHOOSE OF btRemove IN FRAME gDialog /* Exclui documento */
DO:
    DEF VAR lConfirma AS LOGICAL NO-UNDO INIT FALSE.

    IF ecm_canremove THEN DO:
        ASSIGN cOutputFile = SESSION:TEMP-DIRECTORY + vCodCliente + ".txt".

        IF AVAIL tt-docs THEN DO:
            MESSAGE "Confirma exclusÆo?"
                VIEW-AS ALERT-BOX QUESTION
                BUTTON YES-NO
                UPDATE lConfirma
                .
            IF lConfirma THEN DO ON ERROR UNDO, LEAVE:
                SESSION:SET-WAIT-STATE("GENERAL":U).
                FILE-INFO:FILE-NAME = "ecmfiles.exe":U.
                IF FILE-INFO:FULL-PATHNAME = ? THEN
                    FILE-INFO:FILE-NAME = "esp/ecmfiles.exe":U.
                ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
                        + " -ecmlogin ~"":U + ecm_login + "~""
                        + " -ecmuser ~"":U + ecm_user + "~""
                        + " -ecmpassword ~"":U + ecm_pass + "~""
                        + " -ecmurl ~"":U + ecm_url + "~""
                        + " -ecmcompany ~"":U + string(ecm_company) + "~""
                        + " -action ~"delete~""
                        + " -outputFile ~"" + cOutputFile + "~""
                        + cProxyParam
                        + " -ecmDocumentId " + STRING(tt-docs.id)
                        + " -ecmRootFolder " + STRING(ecm_rootfolderid)
                        + " -ecmFolder ~"" + (IF ecm_rootfolderid = 0 THEN ecm_rootfolder + "/" + vCodCliente ELSE vCodCliente) + "~""
                        + " > " + SESSION:TEMP-DIRECTORY + vCodCliente + ".remove.log"
                    .

                OS-COMMAND SILENT VALUE(cCmd).

                RUN pi-load-docs.

                {&OPEN-QUERY-br_docs}
            END.
            SESSION:SET-WAIT-STATE("").
        END.
    END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&Scoped-define SELF-NAME btUpload
&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CONTROL btUpload gDialog
ON CHOOSE OF btUpload IN FRAME gDialog /* Novo documento */
DO:
    DEF VAR cFile AS CHAR NO-UNDO.
    DEF VAR lOk AS LOGICAL NO-UNDO INIT FALSE.
    ASSIGN cOutputFile = SESSION:TEMP-DIRECTORY + vCodCliente + ".txt".

    IF ecm_canupload THEN DO:
        SYSTEM-DIALOG GET-FILE cFile 
            MUST-EXIST
            TITLE "Escolha o arquivo"
            UPDATE lOk
            USE-FILENAME.

        IF lOk THEN DO ON ERROR UNDO, LEAVE:
            SESSION:SET-WAIT-STATE("GENERAL":U).
            FILE-INFO:FILE-NAME = "ecmfiles.exe":U.
            IF FILE-INFO:FULL-PATHNAME = ? THEN
                FILE-INFO:FILE-NAME = "esp/ecmfiles.exe":U.
            ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
                    + " -ecmlogin ~"":U + ecm_login + "~""
                    + " -ecmuser ~"":U + ecm_user + "~""
                    + " -ecmpassword ~"":U + ecm_pass + "~""
                    + " -ecmurl ~"":U + ecm_url + "~""
                    + " -ecmcompany ~"":U + string(ecm_company) + "~""
                    + " -action ~"upload~""
                    + " -inputFile ~"" + cFile + "~""
                    + " -outputFile ~"" + cOutputFile + "~""
                    + cProxyParam
                    + " -ecmRootFolder " + STRING(ecm_rootfolderid)
                    + " -ecmFolder ~"" + (IF ecm_rootfolderid = 0 THEN ecm_rootfolder + "/" + vCodCliente ELSE vCodCliente) + "~""
                    + " > " + SESSION:TEMP-DIRECTORY + vCodCliente + ".upload.log"
                .

            OS-COMMAND SILENT VALUE(cCmd).

            RUN pi-load-docs.

            {&OPEN-QUERY-br_docs}
        END.
        SESSION:SET-WAIT-STATE("").
    END.
END.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


&UNDEFINE SELF-NAME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _CUSTOM _MAIN-BLOCK gDialog 


/* ***************************  Main Block  *************************** */

FIND emsuni.cliente NO-LOCK
    WHERE RECID(emsuni.cliente) = p_rec_cliente
    NO-ERROR.
ASSIGN vCodCliente = STRING(emsuni.cliente.cdn_cliente)
       vNomCliente = cliente.nom_pessoa.

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
    ASSIGN ecm_login = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_LOGIN}"
           ext_param_geral.val_parametro = "adm"
           ecm_login = "adm".
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
    ASSIGN ecm_pass = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_SENHA}"
           ext_param_geral.val_parametro = "adm"
           ecm_pass = "adm".
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
    ASSIGN ecm_user = ext_param_geral.val_parametro.
END.
ELSE DO:
    ASSIGN ecm_user = ecm_login.
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
    ASSIGN ecm_url = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_URL}"
           ext_param_geral.val_parametro = "http://localhost:8080/webdesk"
           ecm_url = "http://localhost:8080/webdesk".
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
    ASSIGN ecm_company = INTEGER(ext_param_geral.val_parametro).
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_EMPRESA}"
           ext_param_geral.val_parametro = "1"
           ecm_company = 1.

END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERNAME}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERNAME}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERNAME}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    ASSIGN ecm_rootfolder = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERNAME}"
           ext_param_geral.val_parametro = "CLIENTES"
           ecm_rootfolder = "CLIENTES".
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERID}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERID}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERID}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    ASSIGN ecm_rootfolderid = INTEGER(ext_param_geral.val_parametro).
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_ROOTFOLDERID}"
           ext_param_geral.val_parametro = "0"
           ecm_rootfolderid = 0.
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_PROXYSERVER}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYSERVER}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYSERVER}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    ASSIGN ecm_proxyserver = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_PROXYSERVER}"
           ext_param_geral.val_parametro = "noproxy":U
           ecm_proxyserver = "noproxy":U.
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_PROXYPORT}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYPORT}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYPORT}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    ASSIGN ecm_proxyport = INTEGER(ext_param_geral.val_parametro).
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_PROXYPORT}"
           ext_param_geral.val_parametro = "0"
           ecm_proxyport = INTEGER(ext_param_geral.val_parametro).
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_PROXYUSER}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYUSER}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYUSER}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    ASSIGN ecm_proxyuser = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_PROXYUSER}"
           ext_param_geral.val_parametro = ""
           ecm_proxyuser = ext_param_geral.val_parametro.
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_PROXYPASS}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYPASS}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_PROXYPASS}"
        NO-ERROR.
IF AVAIL ext_param_geral THEN DO:
    ASSIGN ecm_proxypass = ext_param_geral.val_parametro.
END.
ELSE DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_PROXYPASS}"
           ext_param_geral.val_parametro = ""
           ecm_proxypass = ext_param_geral.val_parametro.
END.

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_GROUPUPLOAD}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_GROUPUPLOAD}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_GROUPUPLOAD}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_GROUPUPLOAD}"
           ext_param_geral.val_parametro = "EAD".
END.
ASSIGN ecm_canupload = CAN-FIND(FIRST emsbas.usuar_grp_usuar
                                WHERE emsbas.usuar_grp_usuar.cod_grp_usuar = ext_param_geral.val_parametro
                                  AND emsbas.usuar_grp_usuar.cod_usuario = v_cod_usuar_corren).

FIND ext_param_geral NO-LOCK
    WHERE ext_param_geral.cod_usuario = v_cod_usuar_corren
      AND ext_param_geral.cod_parametro = "{&ECM_GROUPREMOVE}"
    NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = ""
          AND ext_param_geral.cod_parametro = "{&ECM_GROUPREMOVE}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN
    FIND ext_param_geral NO-LOCK
        WHERE ext_param_geral.cod_usuario = "*"
          AND ext_param_geral.cod_parametro = "{&ECM_GROUPREMOVE}"
        NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = "":U
           ext_param_geral.cod_parametro = "{&ECM_GROUPREMOVE}"
           ext_param_geral.val_parametro = "ERM".
END.
ASSIGN ecm_canremove = CAN-FIND(FIRST emsbas.usuar_grp_usuar
                                WHERE emsbas.usuar_grp_usuar.cod_grp_usuar = ext_param_geral.val_parametro
                                  AND emsbas.usuar_grp_usuar.cod_usuario = v_cod_usuar_corren).

IF ecm_proxyserver <> ? AND ecm_proxyserver <> "":U THEN DO:
    ASSIGN cProxyParam = " -ecmProxyServer ~"":U + ecm_proxyserver + "~"":U.
    IF ecm_proxyuser <> ? AND ecm_proxyuser <> "":U THEN DO:
        ASSIGN cProxyParam = cProxyParam + " -ecmProxyUser ~"":U + ecm_proxyuser + "~"":U.
    END.
    IF ecm_proxypass <> ? AND ecm_proxypass <> "":U THEN DO:
        ASSIGN cProxyParam = cProxyParam + " -ecmProxyPass ~"":U + ecm_proxypass + "~"":U.
    END.
    IF ecm_proxyport > 0 THEN DO:
        ASSIGN cProxyParam = cProxyParam + " -ecmProxyPort ":U + STRING(ecm_proxyport).
    END.
END.

RUN pi-load-docs.

{src/adm2/dialogmn.i}

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME


/* **********************  Internal Procedures  *********************** */

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE adm-create-objects gDialog  _ADM-CREATE-OBJECTS
PROCEDURE adm-create-objects :
/*------------------------------------------------------------------------------
  Purpose:     Create handles for all SmartObjects used in this procedure.
               After SmartObjects are initialized, then SmartLinks are added.
  Parameters:  <none>
------------------------------------------------------------------------------*/

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE disable_UI gDialog  _DEFAULT-DISABLE
PROCEDURE disable_UI :
/*------------------------------------------------------------------------------
  Purpose:     DISABLE the User Interface
  Parameters:  <none>
  Notes:       Here we clean-up the user-interface by deleting
               dynamic widgets we have created and/or hide 
               frames.  This procedure is usually called when
               we are ready to "clean-up" after running.
------------------------------------------------------------------------------*/
  /* Hide all frames. */
  HIDE FRAME gDialog.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE enable_UI gDialog  _DEFAULT-ENABLE
PROCEDURE enable_UI :
/*------------------------------------------------------------------------------
  Purpose:     ENABLE the User Interface
  Parameters:  <none>
  Notes:       Here we display/view/enable the widgets in the
               user-interface.  In addition, OPEN all queries
               associated with each FRAME and BROWSE.
               These statements here are based on the "Other 
               Settings" section of the widget Property Sheets.
------------------------------------------------------------------------------*/
  DISPLAY vCodCliente vNomCliente 
      WITH FRAME gDialog.
  ENABLE RECT-1 br_docs Btn_OK btUpload btRemove 
      WITH FRAME gDialog.
  VIEW FRAME gDialog.
  {&OPEN-BROWSERS-IN-QUERY-gDialog}
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE pi-load-docs gDialog 
PROCEDURE pi-load-docs :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/

    ASSIGN cOutputFile = SESSION:TEMP-DIRECTORY + vCodCliente + ".txt".
    
    FILE-INFO:FILE-NAME = cOutputFile.
    IF FILE-INFO:FULL-PATHNAME <> ? THEN DO:
        OS-DELETE VALUE(FILE-INFO:FULL-PATHNAME).
    END.
    
    FILE-INFO:FILE-NAME = "ecmfiles.exe":U.
    IF FILE-INFO:FULL-PATHNAME = ? THEN
        FILE-INFO:FILE-NAME = "esp/ecmfiles.exe":U.
    ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
            + " -ecmlogin ~"":U + ecm_login + "~""
            + " -ecmuser ~"":U + ecm_user + "~""
            + " -ecmpassword ~"":U + ecm_pass + "~""
            + " -ecmurl ~"":U + ecm_url + "~""
            + " -ecmcompany ~"":U + string(ecm_company) + "~""
            + " -action ~"listFiles~""
            + " -outputFile ~"" + cOutputFile + "~""
            + cProxyParam
            + " -ecmRootFolder " + STRING(ecm_rootfolderid)
            + " -ecmFolder ~"" + (IF ecm_rootfolderid = 0 THEN ecm_rootfolder + "/" + vCodCliente ELSE vCodCliente) + "~""
            + " > " + SESSION:TEMP-DIRECTORY + vCodCliente + ".load.log"
            .
    
    OS-COMMAND SILENT VALUE(cCmd).
    
    RUN pi-read-docs-info.
END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

&ANALYZE-SUSPEND _UIB-CODE-BLOCK _PROCEDURE pi-read-docs-info gDialog 
PROCEDURE pi-read-docs-info :
/*------------------------------------------------------------------------------
  Purpose:     
  Parameters:  <none>
  Notes:       
------------------------------------------------------------------------------*/
    ASSIGN cOutputFile = SESSION:TEMP-DIRECTORY + vCodCliente + ".txt".
    
    EMPTY TEMP-TABLE tt-docs.

    FILE-INFO:FILE-NAME = cOutputFile.
    IF FILE-INFO:FULL-PATHNAME <> ? THEN DO:
        /* Importa */
        INPUT STREAM s1 FROM VALUE(FILE-INFO:FULL-PATHNAME) CONVERT SOURCE "ibm850".
        IMPORT STREAM s1 UNFORMATTED cLine.
        REPEAT:
            CREATE tt-docs.
            IMPORT STREAM s1 DELIMITER "|" tt-docs.id tt-docs.VERSION tt-docs.TYPE tt-docs.NAME.
        END.
        INPUT CLOSE.
    END.

END PROCEDURE.

/* _UIB-CODE-BLOCK-END */
&ANALYZE-RESUME

