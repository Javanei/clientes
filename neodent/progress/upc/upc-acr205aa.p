/*****************************************************************************
**
**     Programa: upc_bas_cliente_fin
**
**
*****************************************************************************/

&GLOBAL-DEFINE ECM_GROUPUPLOAD  ECM_GROUPUPLOAD
&GLOBAL-DEFINE ECM_GROUPREMOVE  ECM_GROUPREMOVE

def input param p-ind-event      as char          no-undo.
def input param p-ind-object     as char          no-undo.
def input param p-wgh-object     as handle        no-undo.
def input param p-wgh-frame      as widget-handle no-undo.
def input param p-cod-table      as char          no-undo.
def input param p-row-table      as recid         no-undo.


/*TODO: {include/i_fclpreproc.i}*/ /* Include que define o processador do Facelift ativado ou nÆo. */

&IF "{&aplica_facelift}" = "YES" &THEN
	{include/i_fcldef.i}
&endif

def new global shared var v_cod_usuar_corren
    as character
    format "x(12)":U
    label "Usuario Corrente"
    column-label "Usuario Corrente"
    no-undo.

DEF NEW GLOBAL SHARED VAR h-upc-acr205aa-ecm_canupload AS LOGICAL NO-UNDO INIT FALSE.
DEF NEW GLOBAL SHARED VAR h-upc-acr205aa-ecm_canremove AS LOGICAL NO-UNDO INIT FALSE.

def new global shared var h-upc-acr205aa-label-status as widget-handle no-undo.
def new global shared var h-upc-acr205aa-cod-status as widget-handle no-undo.
def new global shared var h-upc-acr205aa-des-status as widget-handle no-undo.
def new global shared var v-upc-acr205aa_rec_cliente_doc_ecm
    as recid
    format ">>>>>>9":U
    no-undo.

DEFINE NEW GLOBAL SHARED variable h-upc-acr205aa-documentos-ecm AS WIDGET-HANDLE  NO-UNDO.


if p-ind-event = 'INITIALIZE' then DO:
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
    ASSIGN h-upc-acr205aa-ecm_canupload = CAN-FIND(FIRST emsbas.usuar_grp_usuar
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
    ASSIGN h-upc-acr205aa-ecm_canremove = CAN-FIND(FIRST emsbas.usuar_grp_usuar
                                    WHERE emsbas.usuar_grp_usuar.cod_grp_usuar = ext_param_geral.val_parametro
                                      AND emsbas.usuar_grp_usuar.cod_usuario = v_cod_usuar_corren).

    /*
    assign p-wgh-frame:window:width = p-wgh-frame:window:width + i-alargar
           p-wgh-frame:width = p-wgh-frame:width + i-alargar.
    */       

    run pi-processa-childs (p-wgh-frame:first-child).
end.


/*


message 'p-ind-event: ' p-ind-event skip
        'p-ind-object: ' p-ind-object skip
        'p-wgh-object: ' p-wgh-object skip
        'p-wgh-frame: ' p-wgh-frame skip
        'p-cod-table: ' p-cod-table skip
        'p-row-table: ' string(p-row-table) skip
        view-as alert-box.


*/

 if p-ind-event = 'DISPLAY' then do:
    ASSIGN v-upc-acr205aa_rec_cliente_doc_ecm = p-row-table.
    find emsuni.cliente no-lock
        where recid(emsuni.cliente) = p-row-table
        no-error.

    IF AVAIL emsuni.cliente THEN DO:

        /*TODO: 
        find ext-emitente-neo no-lock
        where ext-emitente-neo.cod-emitente = emsuni.cliente.cdn_cliente
        no-error.

        IF AVAIL ext-emitente-neo THEN DO:

            IF ext-emitente-neo.cod-status <> 0 THEN DO:

                FIND FIRST esp-status NO-LOCK
                     WHERE esp-status.cod-status = ext-emitente-neo.cod-status NO-ERROR.

                ASSIGN h-upc-acr205aa-cod-status:SCREEN-VALUE = string(ext-emitente-neo.cod-status)
                       h-upc-acr205aa-des-status:SCREEN-VALUE = esp-status.des-status.

            END.
            ELSE  ASSIGN h-upc-acr205aa-cod-status:SCREEN-VALUE = "0"
                         h-upc-acr205aa-des-status:SCREEN-VALUE = "".



        END.
        ELSE ASSIGN h-upc-acr205aa-cod-status:SCREEN-VALUE = "0"
                    h-upc-acr205aa-des-status:SCREEN-VALUE = "".
        */
    END.
    ELSE ASSIGN h-upc-acr205aa-cod-status:SCREEN-VALUE = "0"
                h-upc-acr205aa-des-status:SCREEN-VALUE = "".
end.

ELSE IF p-ind-event = "showdocs":U THEN DO:
    IF v-upc-acr205aa_rec_cliente_doc_ecm <> ? THEN DO:
        FIND cliente NO-LOCK
            WHERE RECID(emsuni.cliente) = v-upc-acr205aa_rec_cliente_doc_ecm
            NO-ERROR.
        IF AVAIL emsuni.cliente THEN DO:
            RUN esp/acr205aa-upc01.w (INPUT v-upc-acr205aa_rec_cliente_doc_ecm).
        END.
    END.
END.




procedure pi-processa-childs:

    def input param p-wh-ref as widget-handle no-undo.
    def var wh-ref as widget-handle no-undo.
    
    assign wh-ref = p-wh-ref:first-child.
    
    do while valid-handle(wh-ref):

        if wh-ref:type = 'FIELD-GROUP' then do:
            run pi-processa-childs (input wh-ref).
        end.
        else do:
            IF wh-ref:NAME = "des_grp_clien" THEN DO:

                ASSIGN wh-ref:WIDTH = 20.00.
                create text h-upc-acr205aa-label-status
                assign frame        = wh-ref:FRAME
                       format       = "x(8)"
                       screen-value = "Status:"
                       row          = wh-ref:ROW
                       col          = wh-ref:COL + 21
                       fgcolor      = 1
                       height       = 0.88
                       FONT         = wh-ref:FONT
                       SENSITIVE    = NO
                       visible      = yes.       

                create fill-in h-upc-acr205aa-cod-status
                    assign frame              = wh-ref:FRAME
                           NAME               = 'upc-cod-status'
                           DATA-TYPE          = 'INTEGER'
                           
                           side-label-handle  = h-upc-acr205aa-label-status:HANDLE

                           format             = '>>>>>>9'
                           height             = 0.88
                           WIDTH              = 06.00
                           BGCOLOR      = wh-ref:BGCOLOR

                           row                = wh-ref:ROW
                           col                = wh-ref:col + 28

                           label              = "Status:"
                           SENSITIVE          = NO
                           visible            = true.

                create fill-in h-upc-acr205aa-des-status
                    assign frame              = wh-ref:FRAME
                           NAME               = 'upc-des-status'
                           DATA-TYPE          = 'CHARACTER'
                           
                           

                           format             = 'X(20)'
                           height             = 0.88
                           WIDTH              = 20.00
                           BGCOLOR            = wh-ref:BGCOLOR

                           row                = wh-ref:ROW
                           col                = wh-ref:col + 34

                           
                           SENSITIVE          = NO
                           visible            = true.


            END.
            ELSE IF wh-ref:TYPE = "BUTTON":U
                AND wh-ref:NAME = "bt_historico_padrao":U THEN DO:
                IF h-upc-acr205aa-ecm_canupload OR h-upc-acr205aa-ecm_canremove THEN DO:
                    CREATE BUTTON h-upc-acr205aa-documentos-ecm
                        ASSIGN
                            FRAME       = p-wgh-frame
                            COL         = wh-ref:COL + wh-ref:WIDTH + 0.5
                            ROW         = wh-ref:ROW
                            WIDTH       = wh-ref:WIDTH
                            HEIGHT      = wh-ref:HEIGHT
                            VISIBLE     = TRUE
                            SENSITIVE   = TRUE
                            LABEL       = "Documentos"
                            HELP        = "Documentos"
                            TRIGGERS:
                                ON CHOOSE PERSISTENT RUN upc/upc-acr205aa.p (
                                    INPUT "showdocs",
                                    INPUT p-ind-object,
                                    INPUT h-upc-acr205aa-documentos-ecm,
                                    INPUT p-wgh-frame,
                                    INPUT p-cod-table,
                                    INPUT p-row-table).
                            END TRIGGERS
                            .
                    h-upc-acr205aa-documentos-ecm:LOAD-IMAGE("image/im-attac.bmp").
                    h-upc-acr205aa-documentos-ecm:LOAD-IMAGE-INSENSITIVE("image/ii-attac.bmp").
                END.
            END.
        end.
        assign wh-ref = wh-ref:next-sibling.
    end.
end.
