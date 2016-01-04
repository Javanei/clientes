
FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'SERVIDOR_VAULT'
    NO-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'SERVIDOR_VAULT'
           ext_param_geral.val_parametro = 'vault.neodent.com.br'.
END.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'BANCO_VAULT'
    NO-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'BANCO_VAULT'
           ext_param_geral.val_parametro = 'neodent'.
END.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'USUARIO_VAULT'
    NO-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'USUARIO_VAULT'
           ext_param_geral.val_parametro = 'cbueno'.
END.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'SENHA_VAULT'
    NO-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'SENHA_VAULT'
           ext_param_geral.val_parametro = 'neodent1011'.
END.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'DIR_DESENHOS'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'DIR_DESENHOS'.
END.
ASSIGN ext_param_geral.val_parametro = 'n:\desenhos'.

/* ECM */

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'ECM_LOGIN'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'ECM_LOGIN'.
END.
ASSIGN ext_param_geral.val_parametro = 'adm'.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'ECM_SENHA'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'ECM_SENHA'.
END.
ASSIGN ext_param_geral.val_parametro = 'adm'.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'ECM_MATRICULA'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'ECM_MATRICULA'.
END.
ASSIGN ext_param_geral.val_parametro = 'adm'.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'ECM_URL'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'ECM_URL'.
END.
ASSIGN ext_param_geral.val_parametro = 'http://localhost:8080/webdesk'.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'ECM_EMPRESA'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'ECM_EMPRESA'.
END.
ASSIGN ext_param_geral.val_parametro = '1'.

FIND FIRST ext_param_geral
    WHERE ext_param_geral.cod_usuario = ""
      AND ext_param_geral.cod_parametro = 'ECM_ROOTFOLDER'
    EXCLUSIVE-LOCK NO-ERROR.
IF NOT AVAIL ext_param_geral THEN DO:
    CREATE ext_param_geral.
    ASSIGN ext_param_geral.cod_usuario = ''
           ext_param_geral.cod_parametro = 'ECM_ROOTFOLDER'.
END.
ASSIGN ext_param_geral.val_parametro = '1'.
