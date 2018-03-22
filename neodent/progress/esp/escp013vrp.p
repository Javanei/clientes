/**************************************************************************
**
**     Programa: ESPD013RP
**
**     Objetivo: Impress∆o de Ordem de Produá∆o
**
**************************************************************************/

{include/i-prgvrs.i ESCP013VRP 2.06.00.007}
{include/i_fnctrad.i}

DEF STREAM slog.

/* The following Visual Style parameters define the initial state of the application's main window. */
&global-define SW-SHOWNORMAL      1 /* Start in a normal size window. */
&global-define SW-SHOWMINIMIZED   2 /* Start minimized. Show an icon at the bottom of the screen. */
&global-define SW-SHOWMAXIMIZED   3 /* Start in a maximized window. */
&global-define SW-SHOWNOACTIVATE  4 /* Start but set the focus back to the calling program. */
&global-define SW-SHOWMINNOACTIVE 7 /* Start minimized and set the focus back to the calling program. */

{esp/escp013vtt.i}

def buffer b-op for ord-prod.

/* DEFiniá∆o e preparaá∆o dos ParÉmetros  */

def input parameter raw-param as raw no-undo.
def input parameter table     for tt-raw-digita.

DEF VAR c-item LIKE  ITEM.it-codigo.

create tt-param.
raw-transfer raw-param to tt-param.


def new global shared var c-seg-usuario as character format "x(12)" no-undo.

def buffer bf-item          for item.
def buffer bf-aloca-reserva for aloca-reserva.

DEFINE STREAM s1.
DEFINE VARIABLE cTmp AS CHARACTER NO-UNDO.

{esp/esvl001.i}

def temp-table tt-op-temp no-undo like operacao.

def temp-table tt-ext-texto-cq no-undo like ext-texto-cq.

def temp-table tt-pdf-desenhos no-undo like ttDesenhoItem
   field nr-pdf       as integer
   field nr-sequencia as integer
   field nr-ord-produ as integer
   FIELD bloqueado    AS LOGICAL
   index tt1 nr-pdf nr-sequencia ascending.

def temp-table ttDesenhosCarregados no-undo
   field deCodigo like desenho-item.de-codigo.

def temp-table tt-pdf no-undo
   field nr-pdf          as integer
   field arquivo         as char
   field qt-paginas      as integer
   field qt-desenhos     as integer
   field desenvolvimento as logical
   FIELD bloqueado       AS LOGICAL
   field qt-pg-ord       as integer
   field qt-pg-insp      as integer
   field class_ordem     as integer
   index tt1 nr-pdf ascending.

def temp-table tt-pdf-dados no-undo
   field nr-pdf        as integer
   field nr-pagina     as integer
   field nr-linha      as integer
   field nr-sequencia  as integer
   field texto         as char
   field qt-caracteres as integer
   field posicao       as integer
   field qt-salto      as integer
   field fonte         as char
   field tam-fonte     as integer
   field cor-fonte     as integer
   index tt1 nr-pdf nr-pagina nr-sequencia ascending.

def temp-table tt-exames no-undo
   field it-codigo  like oper-ord.it-codigo
   field op-codigo  like oper-ord.op-codigo
   field cod-exame  like oper-exam.cod-exame
   field cod-comp   like comp-exame.cod-comp
   FIELD classifica AS CHAR
   field rowid-comp as rowid
   index tt1 it-codigo op-codigo cod-exame cod-comp ascending
   index tt2 classifica ascending.


/* Para n∆o imprimir o fundo "Desenvolvimento nas p†ginas de inspeá∆o */
DEFINE TEMP-TABLE tt-not-desenvolvimento
    FIELD nr-pagina     AS INTEGER
    INDEX id-ttnd nr-pagina.

DEFINE VARIABLE l-gera-log AS LOGICAL NO-UNDO INIT true.

def var l-primeira   as logical no-undo.
def var i-pagina-aux as integer no-undo.
def var i-linha-aux  as integer no-undo.
def var i-cont       as integer no-undo.
def var cDeCodigo    as char    no-undo.
def var cItemDir     as char    no-undo.
def var cDeCodigo1   as char    no-undo.
def var cCmd         as char    no-undo.
def var iCount       as integer no-undo.
def var cJpgFile     as char    no-undo.
def var ipWidth      as integer no-undo.
def var ipHeight     as integer no-undo.
def var iWidth       as integer no-undo.
def var iHeight      as integer no-undo.
def var l-em-revisao as logical no-undo.


{include/i-rpvar.i} /* Vari†veis para cabeáalho e rodapÇ */

def var l-teste              as logical   no-undo.
def var l-Erro               as logical   no-undo.
def var nomeArquivo          as character no-undo.
def var defaultFontSize      as integer   no-undo.
def var defaultFontName      as character no-undo.
def var defaultFontBoldName  as character no-undo.
def var cTemp                as character no-undo.
def var cProgramName         as character no-undo.
def var cFileName            as character no-undo.
def var iReturnResult        as integer   no-undo.
def var h-acomp              as handle    no-undo.
def var c-pagina             as character no-undo.
def var i-lin-itens          as integer   no-undo.

DEF VAR c-linhas AS CHAR EXTENT 2 NO-UNDO.

def var i-nr-pdf       as integer no-undo.
def var i-nr-pagina    as integer no-undo.
def var i-nr-sequencia as integer no-undo.
DEF VAR iTempoGasto AS INTEGER NO-UNDO.
DEF VAR dTempoGasto AS DECIMAL NO-UNDO.
DEF STREAM sJpg.
DEF VAR cJpgFileTmp AS CHAR NO-UNDO.
DEF VAR cFileType AS CHAR NO-UNDO.

DEF VAR oper-padrao  AS INTEG NO-UNDO.
DEF VAR oper-padrao1 AS INTEG NO-UNDO.
DEF VAR oper-padrao2 AS INTEG NO-UNDO.
DEF VAR oper-padrao3 AS INTEG NO-UNDO.


run utp/ut-acomp.p persistent set h-acomp.

function getAcrobatCommandLine returns character () forward.

function PosTextRight returns character (ctexto as character, itamanho as integer) forward.

/* Definiá∆o da temp-table de erros */
{cdp/cd0666.i}

/*** Definicao de temp-table tt-Editor */
{include/tt-edit.i}
{include/pi-edit.i}

run pi-inicializar in h-acomp (input  Return-value ).

assign c-programa     = "ESCP013"
       c-versao       = "2.06"       
       c-revisao      = "00.007".

FIND FIRST param-global NO-LOCK NO-ERROR.

if AVAILABLE param-global THEN DO:

   FIND FIRST mguni.empresa WHERE empresa.ep-codigo = param-global.empresa-prin NO-LOCK NO-ERROR.

   IF AVAILABLE empresa THEN c-empresa = empresa.nome.

END.

{pdfinclude/pdf_inc.i "THIS-PROCEDURE"}

assign defaultFontSize     = 8
       defaultFontName     = "Courier"
       defaultFontBoldName = "Courier-Bold".

/* Montar tabela tempor†ria com a imagem da impress∆o */

DO  ON ERROR UNDO, LEAVE
    ON STOP UNDO, LEAVE:
    
if not tt-param.l-imp-por-item then do:

   for each ord-prod no-lock use-index ch-emite-op where /* Marcilene  em 09/12/2014 - alterado para no-lock, devido ao erro de LOCK na tabela ORD-PROD */
            ord-prod.emite-ordem   = (not tt-Param.l-reimpressao) and
            ord-prod.cd-planejado >= tt-param.c-planejador-ini and
            ord-prod.cd-planejado <= tt-param.c-planejador-fim and
            ord-prod.nr-ord-produ >= tt-param.i-ordem-ini      and
            ord-prod.nr-ord-produ <= tt-param.i-ordem-fim      and
            ord-prod.nr-linha     >= tt-param.i-linha-ini      and
            ord-prod.nr-linha     <= tt-param.i-linha-fim      and
            ord-prod.estado       < 7                          and
            ord-prod.dt-emissao   >= tt-param.d-emissao-ini    and
            ord-prod.dt-emissao   <= tt-param.d-emissao-fim    and
            ord-prod.dt-inicio    >= tt-param.d-inicio-ini     and
            ord-prod.dt-inicio    <= tt-param.d-inicio-fim
            by ord-prod.nr-ord-prod:

      run pi-acompanhar in h-acomp (input ord-prod.nr-ord-produ).   

      if ord-prod.it-codigo < tt-param.c-item-ini or
         ord-prod.it-codigo > tt-param.c-item-fim then next.

      if ord-prod.cod-roteiro < tt-param.c-roteiro-ini or
         ord-prod.cod-roteiro > tt-param.c-roteiro-fim then next.

      IF NOT tt-param.l-somente-desenho THEN DO:
         find first ord-manut where ord-manut.nr-ord-produ = ord-prod.nr-ord-prod no-lock no-error.

         if available ord-manut then next.

          /*N∆o considera modulo de Frotas*/
         if ord-prod.origem = "MV" then next.

         find first planejad where planejad.cd-planejado = ord-prod.cd-planejado no-lock no-error.

         if not tt-Param.l-Reimpressao then do:
            find first b-op exclusive-lock   /* Marcilene em 09/12/2014 - Leitura no buffer para n∆o segurar a tabela ORD-PROD */
                 where b-op.nr-ord-prod = ord-prod.nr-ord-prod no-error.
                  
            if avail  b-op then
               assign b-op.emite-ordem = no.
                
            release b-op.   
          
            /*ord-prod.emite-ordem = 
            no.*/
         end.
      END.

      find first item where item.it-codigo = ord-prod.it-codigo no-lock no-error.    
      
      ASSIGN i-nr-pdf = i-nr-pdf + 1.

      create tt-pdf.
      ASSIGN tt-pdf.nr-pdf     = i-nr-pdf
             tt-pdf.arquivo    = substring(tt-param.arquivo, 1, r-index(tt-param.arquivo, "\")) + "OS" + string(ord-prod.nr-ord-produ) + ".pdf"
             tt-pdf.qt-paginas = 0.

      find first ext-ord-prod where ext-ord-prod.nr-ord-produ = ord-prod.nr-ord-produ no-lock no-error.

      IF NOT tt-param.l-somente-desenho THEN DO:
      
         if available ext-ord-prod then do:

            tt-pdf.class_ordem = ext-ord-prod.class_ordem.
          
            if ext-ord-prod.class_ordem = 1 then tt-pdf.desenvolvimento = yes.
             
         end.
      END.

      IF NOT tt-param.l-somente-desenho and
         not tt-param.l-so-controle     THEN DO:
       
            
           
          run pi-gera-cabecalho-ordem (input yes).
          run pi-cria-dados ("Observacao: ", 12, 0, "", 0, 0, 0).
          run pi-print-editor (ord-prod.narrativa, 80).

          if not tt-param.l-narrativa or not can-find(first tt-Editor) then do:
             run pi-cria-dados (" ", 116, 1, "", 0, 0, 0).
          end.

          if tt-param.l-narrativa then do:
             ASSIGN l-teste = no.

             for each tt-Editor:
                if l-teste then run pi-cria-dados (" ", 12, 0, "", 0, 0, 0).
                run pi-cria-dados (tt-editor.conteudo, 104, 1, defaultFontBoldName, 0, 0, 0).
                ASSIGN l-teste = yes.
             end.
          end.

          run pi-soma-linha (1).

          run pi-cria-dados (PosTextRight("DATAS", 18),                    40, 1, "",                  0, 0, 0).
          run pi-cria-dados (fill("-", 32),                               105, 1, "",                  0, 0, 0).

          run pi-cria-dados ("ABERTURA   LIBERACAO  ENTREGA", 32, 1, "", 0, 0, 0).
          run pi-cria-dados (string(ord-prod.dt-inicio),      11, 0, "", 0, 0, 0).
          run pi-cria-dados (string(ord-prod.dt-emissao),     11, 0, "", 0, 0, 0).
          run pi-cria-dados (string(ord-prod.dt-termino),     10, 1, "", 0, 0, 0).
          run pi-cria-dados (" ",                             55, 0, "", 0, 0, 0).
      END.

      ASSIGN l-primeira = yes.

      IF NOT tt-param.l-somente-desenho and
         not tt-param.l-so-controle     THEN DO:

          for each reservas where reservas.nr-ord-pro = ord-prod.nr-ord-prod no-lock,
             first bf-item  where bf-item.it-codigo   = reservas.it-codigo   no-lock:

             find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                          tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-lock no-error.

             if l-primeira or (available tt-pdf-dados and (tt-pdf-dados.nr-linha + tt-pdf-dados.qt-salto) > 90) then do:

                run pi-cria-dados ("ESTRUTURA",                                        9, 1, "", 0, 0, 0).
                run pi-cria-dados (fill("-", 122),                                   122, 1, "", 0, 0, 0).
                run pi-cria-dados ("COD.",                                            12, 0, "", 0, 0, 0).
                run pi-cria-dados (" ",                                                1, 0, "", 0, 0, 0).
                run pi-cria-dados ("DESCRIÄ«O",                                       57, 0, "", 0, 0, 0).
                run pi-cria-dados ("   QUANT. UM.  OPER.    DEP.               QTDE", 65, 1, "", 0, 0, 0).
                run pi-cria-dados (" ",                                               82, 0, "", 0, 0, 0).
                run pi-cria-dados ("BAIXA    BAIXA    LOTE      ALOCADA",             35, 1, "", 0, 0, 0).
                run pi-cria-dados (fill("-", 122),                                   122, 1, "", 0, 0, 0).

                ASSIGN l-primeira = no.

             end.

             run pi-cria-dados (reservas.it-codigo, 12, 0, "", 0, 0, 0).
             run pi-cria-dados (" ",                 1, 0, "", 0, 0, 0).

             empty temp-table tt-editor.

             run pi-print-editor (bf-item.desc-item, 57).

             find first tt-editor no-error.

             if not available tt-editor then

                run pi-cria-dados (" ", 57, 0, "", 0, 0, 0).

             else do:

                run pi-cria-dados (tt-editor.conteudo, 57, 0, "", 0, 0, 0).

                delete tt-editor.

             end.

             run pi-cria-dados (" ",                                          1, 0, "", 0, 0, 0).
             run pi-cria-dados (PosTextRight(string(reservas.quant-orig), 8), 8, 0, "", 0, 0, 0).
             run pi-cria-dados (" ",                                          1, 0, "", 0, 0, 0).
             run pi-cria-dados (bf-item.un,                                   5, 0, "", 0, 0, 0).
             run pi-cria-dados (reservas.op-codigo,                           4, 0, "", 0, 0, 0).
             run pi-cria-dados (" ",                                          5, 0, "", 0, 0, 0).

             for each aloca-reserva where aloca-reserva.nr-ord-pro = ord-prod.nr-ord-prod and
                                          aloca-reserva.it-codigo  = reservas.it-codigo   no-lock:

                run pi-cria-dados (string(aloca-reserva.cod-depos),   9, 0, "", 0, 0, 0).
                run pi-cria-dados (string(aloca-reserva.lote),       11, 0, "", 0, 0, 0).
                run pi-cria-dados (string(aloca-reserva.quant-aloc),  9, 0, "", 0, 0, 0).

                find next tt-editor no-error.

                if not available tt-editor then

                   run pi-cria-dados (" ", 78, 0, "", 0, 0, 0).

                else do:

                   run pi-soma-linha (1).

                   run pi-cria-dados (" ",                12, 0, "", 0, 0, 0).
                   run pi-cria-dados (" ",                 1, 0, "", 0, 0, 0).
                   run pi-cria-dados (tt-editor.conteudo, 57, 0, "", 0, 0, 0).
                   run pi-cria-dados (" ",                12, 0, "", 0, 0, 0).

                   delete tt-editor.

                end.

                find first bf-aloca-reserva where bf-aloca-reserva.nr-ord-prod = aloca-reserva.nr-ord-prod and
                                                  bf-aloca-reserva.it-codigo   = aloca-reserva.it-codigo   and
                                                  rowid(bf-aloca-reserva)      > rowid(aloca-reserva)      no-lock no-error.

                if available bf-aloca-reserva then run pi-soma-linha (1).

             end.

             run pi-soma-linha (1).

             for each tt-editor:

                run pi-cria-dados (" ",                12, 0, "", 0, 0, 0).
                run pi-cria-dados (" ",                 1, 0, "", 0, 0, 0).
                run pi-cria-dados (tt-editor.conteudo, 30, 1, "", 0, 0, 0).

             end.
          end.

          run pi-soma-linha (1).

          l-primeira = yes.
      END.
      
      IF NOT tt-param.l-somente-desenho THEN DO:

         if not tt-param.l-so-controle then do:

            for each oper-ord where oper-ord.emite-ficha                          and
                                    oper-ord.nr-ord-produ = ord-prod.nr-ord-produ no-lock
                                 by oper-ord.op-codigo:

               if oper-ord.cod-roteiro < tt-param.c-roteiro-ini or
                  oper-ord.cod-roteiro > tt-param.c-roteiro-fim then next.

                FIND ext-grup-maquina NO-LOCK
                    WHERE ext-grup-maquina.gm-codigo = oper-ord.gm-codigo
                    NO-ERROR.

                run pi-soma-linha (1).

                find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                             tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-lock no-error.

                if l-primeira or (available tt-pdf-dados and (tt-pdf-dados.nr-linha + tt-pdf-dados.qt-salto) > 90) then do:

                   run pi-cria-dados (" ",                    50, 0, "", 0, 0, 0).
                   run pi-cria-dados ("ROTEIRO DE PRODUCAO",  19, 1, "", 0, 0, 0).
                   run pi-cria-dados ("  SEQUENCIA ",         35, 0, "", 8, 0, 0).
                   run pi-cria-dados ("DENOMINAÄ«O",          49, 0, "", 8, 0, 0).
                   run pi-cria-dados ("GRUPO MµQUINA ",       20, 1, "", 8, 0, 0).
                   run pi-cria-dados (fill("-", 122),        122, 3, "", 8, 0, 0).

                   l-primeira = no.

                end.

                cTemp = "*" + string(oper-ord.op-codigo,'999') + "*".

                run pi-cria-dados (" ",   1, 0, "Code39", 20, 0, 0).
                run pi-cria-dados (cTemp, 0, 0, "Code39", 20, 0, 0).

                run pi-cria-dados (" ",                                  1, 0, "", 8, 0,  0).
                run pi-cria-dados (string(oper-ord.op-codigo, '>>>9'),   4, 0, "", 8, 0,  0).
                run pi-cria-dados (substring(oper-ord.descricao, 1, 49), 0, 0, "", 8, 0, 36).
                run pi-cria-dados (string(oper-ord.gm-codigo, 'X(9)'),   0, 1, "", 8, 0, 85).

                i-linha-aux = 1.

               /*
               empty temp-table tt-Editor.

               find first ficha-oper of oper-ord no-lock no-error.

               if available ficha-oper then do:

                  run pi-print-editor (ficha-oper.desc-linha, 122).

               end.

               for each tt-Editor:

                  run pi-cria-dados (tt-Editor.Conteudo, 122, 1, "", 8, 0, 0).

                  i-linha-aux = i-linha-aux + 1.

               end.
               */

               if can-find(first op-ferram where op-ferram.num-id-operacao = oper-ord.num-id-operacao no-lock) then do:

                  ASSIGN l-primeira = yes.

                  for each op-ferram where op-ferram.num-id-operacao = oper-ord.num-id-operacao no-lock:

                     if l-primeira or (available tt-pdf-dados and (tt-pdf-dados.nr-linha + tt-pdf-dados.qt-salto) > 90) then do:

                        run pi-cria-dados (" ",             8, 0, "", 0, 0, 0).
                        run pi-cria-dados ("Ferramentas:", 12, 1, "", 0, 0, 0).

                        ASSIGN l-primeira = no.

                        ASSIGN i-linha-aux = i-linha-aux + 1.

                     end.

                     run pi-cria-dados (" ",                  12, 0, "", 0, 0, 0).
                     run pi-cria-dados (op-ferram.ferramenta, 16, 0, "", 0, 0, 0).
                     run pi-cria-dados (" - ",                 3, 0, "", 0, 0, 0).

                     find first ferr-prod where ferr-prod.cod-ferr-prod = op-ferram.ferramenta no-lock no-error.

                     if not available ferr-prod then

                        run pi-soma-linha.

                     else do:

                        run pi-cria-dados (ferr-prod.des-ferr-prod, 40, 1, "", 0, 0, 0).

                     end.

                     ASSIGN i-linha-aux = i-linha-aux + 1.

                  end.

                  run pi-cria-dados (" ", 1, 1, "", 0, 0, 0).

                  ASSIGN i-linha-aux = i-linha-aux + 1.

               end.

               /* GRID */

               IF AVAIL ext-grup-maquina AND ext-grup-maquina.log-imprime-grid = TRUE THEN DO:

                  find first operacao where operacao.num-id-operacao = oper-ord.num-id-operacao no-lock no-error.

                   RUN pi-cria-dados ("+----------------------------------------------------+-------------------------------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|                                D I A R I O       D E      B O R D O                      | MAQ:                      |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+----------------------------------------------------+-------------------------------------+---------------------------+", 122, 1, "", 0, 0, 0).

                   if avail operacao then do:
                      RUN pi-cria-dados ("| DISPOSITIVO:                          | TMP MAQ UN: " + string(operacao.tempo-maquin,">>9.99") +  "    TMP MAQ TOT: " + 
                                            string((operacao.tempo-maquin * ord-prod.qt-ordem),">>>>9.99") +  "      | MES                       |", 122, 1, "", 0, 0, 0).
                   end.
                   else
                      RUN pi-cria-dados ("| DISPOSITIVO:                          | TMP MAQ UN: 0" +  "    TMP MAQ TOT: 0"  +  "      | MES                       |", 122, 1, "", 0, 0, 0).

                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|  DATA  | INICIO |  FIM   |   REFUGO   | QUANTIDADE | RETRABALHO | REFUGO   | COLABORADOR | OBSERVACAO                |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        | PREPARAÄ«O | PRODUZIDA  |            | OPERAÄ«O |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("|        |        |        |            |            |            |          |             |                           |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("| VERIFICAÄ«O FINAL DO LOTE PARA EVITAR PEÄAS DIFERENTES:                      COLABORADOR:                            |", 122, 1, "", 0, 0, 0).
                   RUN pi-cria-dados ("+--------+--------+--------+------------+------------+------------+----------+-------------+---------------------------+", 122, 1, "", 0, 0, 0).

                   ASSIGN i-linha-aux = 1.

               END.

               do i-cont = i-linha-aux to 6:

                  run pi-soma-linha (1).

               end.
            end.
         end.

         if tt-param.l-plano-controle or tt-param.l-so-controle then do:

            run pi-consiste-revisao (input  ord-prod.nr-ord-produ,
                                     input  ord-prod.it-codigo,
                                     output l-em-revisao).

            if not l-em-revisao then do:
               
               run pi-gera-cabecalho-inspecao (input yes).

               run pi-gera-dados-inspecao.

            end.
         end.
      end.
             
      if not tt-param.l-so-controle then do:

         /* Busca dos desenhos. */
         RUN esp/esvl002.p (INPUT ord-prod.it-codigo, OUTPUT TABLE ttDesenhoitem).

         IF RETURN-VALUE = "OK":U THEN DO:
            ASSIGN i-nr-sequencia = 0.

            FOR EACH ttDesenhoItem:
               ASSIGN i-nr-sequencia = i-nr-sequencia + 1.

               create tt-pdf-desenhos.
               buffer-copy ttDesenhoItem to tt-pdf-desenhos.
               ASSIGN  tt-pdf-desenhos.nr-pdf       = tt-pdf.nr-pdf
                       tt-pdf-desenhos.nr-sequencia = i-nr-sequencia
                       tt-pdf-desenhos.nr-ord-produ = ord-prod.nr-ord-produ
                       /*tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
                       tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1*/.

            END.
         END.
      end.
      
      /* Cria hist-ord-prod */
      IF tt-Param.l-reimpressao THEN DO:
          RUN pi-cria-hist-ord-prod.
      END.

   end.
end.

if tt-param.l-imp-por-item then do:

   for each item where item.it-codigo >= tt-param.c-item-ini and
                       item.it-codigo <= tt-param.c-item-fim no-lock:

      run pi-acompanhar in h-acomp (input item.it-codigo).

      ASSIGN i-nr-pdf = i-nr-pdf + 1.

      create tt-pdf.
      ASSIGN tt-pdf.nr-pdf     = i-nr-pdf
             tt-pdf.arquivo    = substring(tt-param.arquivo, 1, r-index(tt-param.arquivo, "\")) + "IT" + string(item.it-codigo) + ".pdf"
             tt-pdf.qt-paginas = 0.

      run pi-consiste-revisao (input  0,
                               input  item.it-codigo,
                               output l-em-revisao).

      if /*not l-em-revisao               and*/
         not tt-param.l-somente-desenho then do:

          
         ASSIGN c-item =  ITEM.it-codigo.

         run pi-gera-cabecalho-item (input yes).

         run pi-gera-dados-item.

      end.

      if not tt-param.l-so-controle then do:

         /* Busca dos desenhos. */
         RUN esp/esvl002.p (INPUT item.it-codigo, OUTPUT TABLE ttDesenhoitem).

         IF RETURN-VALUE = "OK":U THEN DO:

            ASSIGN i-nr-sequencia = 0.

            FOR EACH ttDesenhoItem:

               ASSIGN i-nr-sequencia = i-nr-sequencia + 1.

               create tt-pdf-desenhos.
               buffer-copy ttDesenhoItem to tt-pdf-desenhos.
               ASSIGN  tt-pdf-desenhos.nr-pdf       = tt-pdf.nr-pdf
                       tt-pdf-desenhos.nr-sequencia = i-nr-sequencia
                       /*tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
                       tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1*/.
            END.
         END.
      end.
   end.
end.

run pi-acompanhar in h-acomp (input "Aguarde, gerando arquivo PDF").   

run pdf_new ("Spdf", tt-param.arquivo).

file-info:file-name = "Code39.ttf".
run pdf_load_font ("Spdf", "Code39", file-info:full-pathname, "code39.afm","").

file-info:file-name = "IDAutomationHC39M.ttf".
run pdf_load_font ("Spdf", "BarcodeFont39", file-info:full-pathname, "BarcodeFont39.afm","").

run pdf_load_image ("Spdf", "Desenvolvimento", search("desenvolvimento.jpg")).
run pdf_load_image ("Spdf", "Bloqueado", search("desenhobloqueado.jpg")).

if search("Neoshape.jpg") <> ? then do:

   run pdf_load_image ("Spdf", "Neoshape", search("Neoshape.jpg")).

end.

if search("Neodent_Digital.jpg") <> ? then do:

   run pdf_load_image ("Spdf", "Neodent_Digital", search("Neodent_Digital.jpg")).

end.

if search("Neoguide.jpg") <> ? then do:

   run pdf_load_image ("Spdf", "Neoguide", search("Neoguide.jpg")).

end.

if search("USA.jpg") <> ? then do:

   run pdf_load_image ("Spdf", "USA", search("USA.jpg")).

end.

if search("Reprocesso.jpg") <> ? then do:

   run pdf_load_image ("Spdf", "Reprocesso", search("Reprocesso.jpg")).

end.

if not can-find(first tt-pdf no-lock) then do:
   run pdf_new_page("Spdf").
end.

empty temp-table ttDesenhosCarregados.

for each tt-pdf no-lock:

   run pdf_set_PaperType ("Spdf", "A4").
   run pdf_set_Orientation ("Spdf", "Portrait").
   run pdf_set_LeftMargin ("Spdf", 10).

   ASSIGN i-nr-sequencia = tt-pdf.qt-paginas - tt-pdf.qt-desenhos.

   if tt-pdf.desenvolvimento then do:
       /*ASSIGN  tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
               tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1.*/
   END.
   ELSE DO:
       RUN pi-load-images.
   END.

   for each tt-pdf-dados where tt-pdf-dados.nr-pdf = tt-pdf.nr-pdf no-lock 
                      break by tt-pdf-dados.nr-pagina
                            by tt-pdf-dados.nr-sequencia:

      if first-of(tt-pdf-dados.nr-pagina) then do:
      
         run pdf_new_page ("Spdf").

         if tt-pdf.desenvolvimento or
            tt-pdf.class_ordem > 1 then do:
         
            /* Para n∆o imprimir o fundo "Desenvolvimento nas p†ginas de inspeá∆o */
             IF NOT CAN-FIND(FIRST tt-not-desenvolvimento
                             WHERE tt-not-desenvolvimento.nr-pagina = tt-pdf-dados.nr-pagina) THEN DO:

                if tt-pdf.class_ordem = 1 then ASSIGN cDeCodigo = "Desenvolvimento".
                /*if tt-pdf.class_ordem = 2 then ASSIGN cDeCodigo = "Neoshape".*/

                if tt-pdf.class_ordem = 2 then do:
                
                   if search("Neodent_Digital.jpg") <> ? then
                      ASSIGN cDeCodigo = "Neodent_Digital".
                   else
                      ASSIGN cDeCodigo = "Neoshape".
                   
                end.

                /*if tt-pdf.class_ordem = 3 then ASSIGN cDeCodigo = "Neoguide".*/
                if tt-pdf.class_ordem = 3 then ASSIGN cDeCodigo = "USA".
                if tt-pdf.class_ordem = 4 then ASSIGN cDeCodigo = "Reprocesso".

                 ASSIGN ipWidth  = pdf_PageHeight("Spdf").
                 ASSIGN ipHeight = pdf_PageHeight("Spdf").
                 ASSIGN iWidth   = pdf_ImageDim  ("Spdf", cDeCodigo, "WIDTH").
                 ASSIGN iHeight  = pdf_ImageDim  ("Spdf", cDeCodigo, "HEIGHT").

                 IF iWidth <= iHeight THEN
                    RUN pdf_set_Orientation ("Spdf","Portrait").
                 ELSE DO:
                    RUN pdf_set_Orientation ("Spdf","Landscape").
                    ASSIGN iWidth  = pdf_ImageDim ("Spdf", cDeCodigo, "WIDTH").
                    ASSIGN iHeight = pdf_ImageDim ("Spdf", cDeCodigo, "HEIGHT").
                 END.

                 run pdf_set_font ("Spdf", defaultFontName, defaultFontSize).
                 run pdf_place_image ("Spdf",
                                      cDeCodigo,
                                      1,                      /* Coluna da esquerda para direita */
                                      pdf_PageHeight("Spdf"), /* Linha de baixo para cima */
                                      pdf_PageWidth("Spdf"),  /* Largura */
                                      pdf_PageHeight("Spdf")) /* Altura */.

                 run pdf_set_Orientation ("Spdf","Portrait").
             END.

         END.
      end.

      run pi-imprime-texto (input tt-pdf-dados.texto,
                            input tt-pdf-dados.qt-caracteres,
                            input tt-pdf-dados.posicao,
                            input tt-pdf-dados.fonte,
                            input tt-pdf-dados.tam-fonte,
                            input tt-pdf-dados.cor-fonte).

      repeat i-cont = 1 to tt-pdf-dados.qt-salto:
         run pdf_skip ("Spdf").
      end.

      if last-of(tt-pdf-dados.nr-pagina) then do:
         c-pagina = "P†gina " + string(tt-pdf-dados.nr-pagina) + " de " + string(tt-pdf.qt-paginas).
         c-pagina = fill(" ", 17 - length(c-pagina)) + c-pagina.

         run pdf_text_xy ("Spdf", "Impresso em: " + string(TODAY,"99/99/9999") + " - " + string(TIME,"HH:MM"),  10, 40).
         run pdf_text_xy ("Spdf", c-pagina, 495, 40).
      end.
   end.

   /*if tt-pdf.desenvolvimento then do:
       /*
       ASSIGN cDeCodigo = "Desenvolvimento".
       ASSIGN ipWidth  = pdf_PageHeight("Spdf").
       ASSIGN ipHeight = pdf_PageHeight("Spdf").
       ASSIGN iWidth   = pdf_ImageDim  ("Spdf", cDeCodigo, "WIDTH").
       ASSIGN iHeight  = pdf_ImageDim  ("Spdf", cDeCodigo, "HEIGHT").

       IF iWidth <= iHeight THEN
          RUN pdf_set_Orientation ("Spdf","Portrait").
       ELSE DO:
          RUN pdf_set_Orientation ("Spdf","Landscape").
          ASSIGN iWidth  = pdf_ImageDim ("Spdf", cDeCodigo, "WIDTH").
          ASSIGN iHeight = pdf_ImageDim ("Spdf", cDeCodigo, "HEIGHT").
       END.

       run pdf_new_page("Spdf").
       run pdf_set_font ("Spdf", defaultFontName, defaultFontSize).
       run pdf_place_image ("Spdf",
                            cDeCodigo,
                            1,                      /* Coluna da esquerda para direita */
                            pdf_PageHeight("Spdf"), /* Linha de baixo para cima */
                            pdf_PageWidth("Spdf"),  /* Largura */
                            pdf_PageHeight("Spdf")) /* Altura */.
       */
       run pdf_new_page("Spdf").
   END.
   ELSE IF tt-pdf.bloqueado THEN DO:
       /*Comentado porque por enquanto n∆o deve mostrar nada.
         Quando pedirem para mostrar, Ç s¢ descomentar!!!
       ASSIGN cDeCodigo = "Bloqueado".
       ASSIGN ipWidth  = pdf_PageHeight("Spdf").
       ASSIGN ipHeight = pdf_PageHeight("Spdf").
       ASSIGN iWidth   = pdf_ImageDim  ("Spdf", cDeCodigo, "WIDTH").
       ASSIGN iHeight  = pdf_ImageDim  ("Spdf", cDeCodigo, "HEIGHT").

       IF iWidth <= iHeight THEN
          RUN pdf_set_Orientation ("Spdf","Portrait").
       ELSE DO:
          RUN pdf_set_Orientation ("Spdf","Landscape").
          ASSIGN iWidth  = pdf_ImageDim ("Spdf", cDeCodigo, "WIDTH").
          ASSIGN iHeight = pdf_ImageDim ("Spdf", cDeCodigo, "HEIGHT").
       END.

       run pdf_new_page("Spdf").
       run pdf_set_font ("Spdf", defaultFontName, defaultFontSize).
       run pdf_place_image ("Spdf",
                            cDeCodigo,
                            1,                      /* Coluna da esquerda para direita */
                            pdf_PageHeight("Spdf"), /* Linha de baixo para cima */
                            pdf_PageWidth("Spdf"),  /* Largura */
                            pdf_PageHeight("Spdf")) /* Altura */.
       */
   END.
   ELSE*/ DO:

       if can-find(first tt-pdf-desenhos where tt-pdf-desenhos.nr-pdf = tt-pdf.nr-pdf no-lock) then do:

          for each tt-pdf-desenhos where tt-pdf-desenhos.nr-pdf = tt-pdf.nr-pdf no-lock:

             if tt-pdf-desenhos.bloqueado then next.

             ASSIGN cDeCodigo = REPLACE(tt-pdf-desenhos.deCodigo, ' ':U, '_').
             ASSIGN cDeCodigo1 = cDeCodigo.

             FOR EACH ttDesenhosCarregados WHERE ttDesenhosCarregados.deCodigo MATCHES cDeCodigo1 + "__*":U:

                ASSIGN cDeCodigo = ttDesenhosCarregados.deCodigo.
                ASSIGN ipWidth  = pdf_PageHeight("Spdf").
                ASSIGN ipHeight = pdf_PageHeight("Spdf").
                ASSIGN iWidth   = pdf_ImageDim  ("Spdf", cDeCodigo, "WIDTH").
                ASSIGN iHeight  = pdf_ImageDim  ("Spdf", cDeCodigo, "HEIGHT").

                IF iWidth <= iHeight THEN
                   RUN pdf_set_Orientation ("Spdf","Portrait").
                ELSE DO:
                   RUN pdf_set_Orientation ("Spdf","Landscape").
                   ASSIGN iWidth  = pdf_ImageDim ("Spdf", cDeCodigo, "WIDTH").
                   ASSIGN iHeight = pdf_ImageDim ("Spdf", cDeCodigo, "HEIGHT").
                END.

                run pdf_new_page("Spdf").
                run pdf_set_font ("Spdf", defaultFontName, defaultFontSize).
                run pdf_place_image ("Spdf",
                                     cDeCodigo,
                                     1,                      /* Coluna da esquerda para direita */
                                     pdf_PageHeight("Spdf"), /* Linha de baixo para cima */
                                     pdf_PageWidth("Spdf"),  /* Largura */
                                     pdf_PageHeight("Spdf")) /* Altura */.

                if tt-pdf-desenhos.nr-ord-produ > 0 then do:
                   RUN pdf_text_xy ("Spdf","ORDEM DE PRODUÄ«O: " + STRING(tt-pdf-desenhos.nr-ord-produ), 10, 585).
                end.

                ASSIGN i-nr-sequencia = i-nr-sequencia + 1.
                c-pagina = "P†gina " + string(i-nr-sequencia) + " de " + string(tt-pdf.qt-paginas).
                c-pagina = fill(" ", 17 - length(c-pagina)) + c-pagina.

                run pdf_text_xy ("Spdf", "Impresso em: " + string(TODAY,"99/99/9999") + " - " + string(TIME,"HH:MM"),  10, 4).

                if pdf_Orientation("Spdf") = "Portrait" then
                   run pdf_text_xy ("Spdf", c-pagina, 495, 4).
                else
                   run pdf_text_xy ("Spdf", c-pagina, 740, 4).

                /*OS-DELETE VALUE(cCmd) RECURSIVE.*/
             end.
          END.
       end.
   END.

   if (tt-pdf.qt-paginas modulo 2) <> 0 then do:
      run pdf_new_page("Spdf").
   end.

   /*run pdf_close("Spdf").*/

   /*run WinExec (input getAcrobatCommandLine() + chr(32) + tt-pdf.arquivo, input 1, output iReturnResult).*/

end.

run pdf_close("Spdf").

run WinExec (input getAcrobatCommandLine() + chr(32) + tt-param.arquivo, input 1, output iReturnResult).

run pi-finalizar in h-acomp.
END.
return "OK".





procedure pi-soma-linha:

   def input parameter i-qt-linhas as integer no-undo.

   find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-error.

   if available tt-pdf-dados then do:

      tt-pdf-dados.qt-salto = tt-pdf-dados.qt-salto + i-qt-linhas.

   end.

end procedure.



procedure pi-cria-dados:
   def input parameter c-texto     as char    no-undo.
   def input parameter i-tamanho   as integer no-undo.
   def input parameter i-linhas    as integer no-undo.
   def input parameter c-fonte     as char    no-undo.
   def input parameter i-tam-fonte as integer no-undo.
   def input parameter i-cor-fonte as integer no-undo.
   def input parameter i-posicao   as integer no-undo.

   def var i-nr-sequencia as integer no-undo.
   def var i-nr-linha     as integer no-undo.

   find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-lock no-error.

   if available tt-pdf-dados then
      assign i-nr-sequencia = tt-pdf-dados.nr-sequencia + 1
             i-nr-linha     = tt-pdf-dados.nr-linha     + tt-pdf-dados.qt-salto.
   else
      assign i-nr-sequencia = 1
             i-nr-linha     = 1.

 if i-nr-linha > 90 then do:
      if tt-pdf.qt-pg-insp = 0 then
         run pi-gera-cabecalho-ordem (input no).
      else do:
         if tt-param.l-imp-por-item then

            run pi-gera-cabecalho-item (input no).
         else
            run pi-gera-cabecalho-inspecao (input no).
      end.

      find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                   tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-lock no-error.

      if available tt-pdf-dados then
         assign i-nr-sequencia = tt-pdf-dados.nr-sequencia + 1
                i-nr-linha     = tt-pdf-dados.nr-linha     + tt-pdf-dados.qt-salto.
      else
         assign i-nr-sequencia = 1
                i-nr-linha     = 1.

   end.
      
   create tt-pdf-dados.
   ASSIGN tt-pdf-dados.nr-pdf        = tt-pdf.nr-pdf
          tt-pdf-dados.nr-pagina     = tt-pdf.qt-paginas
          tt-pdf-dados.nr-linha      = i-nr-linha
          tt-pdf-dados.nr-sequencia  = i-nr-sequencia
          tt-pdf-dados.texto         = c-texto
          tt-pdf-dados.qt-caracteres = i-tamanho
          tt-pdf-dados.posicao       = i-posicao
          tt-pdf-dados.qt-salto      = i-linhas
          tt-pdf-dados.fonte         = c-fonte
          tt-pdf-dados.tam-fonte     = i-tam-fonte
          tt-pdf-dados.cor-fonte     = i-cor-fonte.
end procedure.




procedure pi-gera-cabecalho-ordem:
   def input parameter l-imp-tudo as logical no-undo.

   ASSIGN tt-pdf.qt-paginas = tt-pdf.qt-paginas + 1
          tt-pdf.qt-pg-ord  = tt-pdf.qt-pg-ord  + 1.

   if l-imp-tudo then do:
      run pi-cria-dados (c-empresa,                                        85, 0, "",                   0, 0, 0).
      run pi-cria-dados ("Num. Ordem: ",                                   12, 1, "",                   0, 0, 0).
      run pi-cria-dados (c-Programa + " - " + c-versao + "." + c-revisao,  48, 1, "",                   0, 0, 0).
      run pi-cria-dados (" ",                                              48, 0, "",                   0, 0, 0).
      run pi-cria-dados ("ORDEM DE PRODUÄ«O",                              17, 1, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados (" ",                                              97, 0, "",                   0, 0, 0).
      run pi-cria-dados ("*" + string(ord-prod.nr-ord-produ) + "*",         0, 1, "BarcodeFont39",     10, 0, 0).
      run pi-cria-dados (" ",                                             122, 1, "",                   0, 0, 0).
      run pi-cria-dados ("ITEM: ",                                          6, 0, "",                   0, 0, 0).
      run pi-cria-dados (ord-prod.it-codigo + " - " + item.desc-item,      83, 2, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados ("U.M.: ",                                          6, 0, "",                   0, 0, 0).
      run pi-cria-dados (item.un,                                          20, 0, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados ("LOTE: ",                                          7, 0, "",                   0, 0, 0).
      run pi-cria-dados (ord-prod.lote,                                    23, 0, defaultFontBoldName, 12, 0, 0).      
      run pi-cria-dados ("QUANTIDADE PLANEJADA: ",                         22, 0, "",                   0, 0, 0).
      run pi-cria-dados (PosTextRight(string(ord-prod.qt-ordem), 11),       0, 1, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados (fill("-", 122),                                  122, 1, "",                   0, 0, 0).
   end.
   else do:
      run pi-cria-dados (" ",                                              85, 0, "",                   0, 0, 0).
      run pi-cria-dados ("Num. Ordem: ",                                   12, 3, "",                   0, 0, 0).
      run pi-cria-dados (" ",                                              97, 0, "",                   0, 0, 0).
      run pi-cria-dados ("*" + string(ord-prod.nr-ord-produ) + "*",         0, 2, "BarcodeFont39",     10, 0, 0).
      run pi-cria-dados (fill("-", 122),                                  122, 1, "",                   0, 0, 0).
   end.
end procedure.

PROCEDURE pi-gera-cabecalho-inspecao:
   def input parameter l-imp-revisao as logical no-undo.

   def var c-desc-rev-plano-insp as char          no-undo.
   def var v-dat-ini-validade    as date          no-undo.
   def var c-nom-desenho         as char          no-undo.
   def var c-rev-desenho         as char          no-undo.
   def var c-dat-desenho         as char          no-undo.
   def var i-qt-caracter         as integer       no-undo.

   ASSIGN tt-pdf.qt-paginas = tt-pdf.qt-paginas + 1
          tt-pdf.qt-pg-insp = tt-pdf.qt-pg-insp + 1.

   /* Para n∆o imprimir o fundo "Desenvolvimento nas p†ginas de inspeá∆o */
   CREATE tt-not-desenvolvimento.
   ASSIGN nr-pagina = tt-pdf.qt-paginas.

   run pi-cria-dados (c-empresa,                                        85, 0, "",                   0, 0, 0).
   run pi-cria-dados ("Num. Ordem: ",                                   12, 1, "",                   0, 0, 0).
   run pi-cria-dados (c-Programa + " - " + c-versao + "." + c-revisao,  40, 1, "",                   0, 0, 0).
   run pi-cria-dados (" ",                                              40, 0, "",                   0, 0, 0).
   run pi-cria-dados ("PLANO DE CONTROLE DE INSPEÄ«O",                  29, 1, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados (" ",                                              97, 0, "",                   0, 0, 0).
   run pi-cria-dados ("*" + string(ord-prod.nr-ord-produ) + "*",         0, 1, "BarcodeFont39",     10, 0, 0).
   run pi-cria-dados (" ",                                             122, 1, "",                   0, 0, 0).
   run pi-cria-dados ("ITEM: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (ord-prod.it-codigo + " - " + item.desc-item,      83, 2, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("U.M.: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (item.un,                                          50, 0, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("QUANTIDADE PLANEJADA: ",                         22, 0, "",                   0, 0, 0).
   run pi-cria-dados (PosTextRight(string(ord-prod.qt-ordem), 11),       0, 1, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados (fill("-", 122),                                  122, 1, "",                   0, 0, 0).

   if l-imp-revisao then do:
      /*Revis∆o Plano de Inspeá∆o*/
      find last texto where texto.it-codigo = ord-prod.it-codigo no-lock no-error.
      if available texto then do:
          find first tipo-texto where tipo-texto.tipo = texto.tipo no-lock no-error.

          if available tipo-texto then
              c-desc-rev-plano-insp = tipo-texto.descricao.
          else
              c-desc-rev-plano-insp = "".

          find first ext-texto where ext-texto.it-codigo = texto.it-codigo and 
                                     ext-texto.tipo      = texto.tipo      no-lock no-error.
          if available ext-texto then do:
              assign v-dat-ini-validade = ext-texto.dat-ini-validade.
          end.
          else 
              assign v-dat-ini-validade = today.
      end.
      else
          assign c-desc-rev-plano-insp = "".

      run pi-cria-dados ("Plano de Inspeá∆o: ", 19, 0, "", 0, 0, 0).
      run pi-cria-dados ((IF NOT tt-pdf.desenvolvimento THEN c-desc-rev-plano-insp ELSE ""), 33, 0, "", 0, 0, 0).

      /*Inicio Validade*/
      run pi-cria-dados ("Data: ",                                 6, 0, "", 0, 0, 0).
      run pi-cria-dados ((IF NOT tt-pdf.desenvolvimento THEN string(v-dat-ini-validade,"99/99/9999") ELSE "          "), 10, 2, "", 0, 0, 0).

      ASSIGN i-qt-caracter = 0. 

      /*Revis∆o de Desenhos*/
      for each desenho-item where desenho-item.it-codigo = ord-prod.it-codigo     no-lock,
          last revisao      where revisao.de-codigo      = desenho-item.de-codigo no-lock:

          ASSIGN i-qt-caracter = maximum(10, length(revisao.de-codigo)).

      end.

      IF i-qt-caracter = 0 THEN

          ASSIGN c-nom-desenho = ""
                 c-rev-desenho = ""
                 c-dat-desenho = "". 

      ELSE DO:

          ASSIGN c-nom-desenho = ""
                 c-rev-desenho = ""
                 c-dat-desenho = "". 

          /*Revis∆o de Desenhos*/
          for each desenho-item where desenho-item.it-codigo = ord-prod.it-codigo     no-lock,
              last revisao      where revisao.de-codigo      = desenho-item.de-codigo no-lock:

              ASSIGN c-nom-desenho = revisao.de-codigo                                                                                                         + fill(" ", i-qt-caracter - length(revisao.de-codigo)) + (if c-nom-desenho = "" then "" else " / " + c-nom-desenho)
                     c-rev-desenho = (IF NOT tt-pdf.desenvolvimento THEN revisao.rv-codigo                          ELSE FILL(" ", LENGTH(revisao.rv-codigo))) + fill(" ", i-qt-caracter - length(revisao.rv-codigo)) + (if c-rev-desenho = "" then "" else " / " + c-rev-desenho)
                     c-dat-desenho = (IF NOT tt-pdf.desenvolvimento THEN STRING(revisao.data-revisao, "99/99/9999") ELSE "          ")                         + fill(" ", i-qt-caracter - 10)                        + (if c-dat-desenho = "" then "" else " / " + c-dat-desenho).

          end.

          ASSIGN c-nom-desenho = "Desenho: " + c-nom-desenho
                 c-rev-desenho = "Revis∆o: " + c-rev-desenho
                 c-dat-desenho = "Data:    " + c-dat-desenho.

      END.

      run pi-cria-dados ("Revis∆o de Desenhos: ",  22, 1, defaultFontBoldName, 0, 0, 0).

      IF NOT tt-pdf.desenvolvimento THEN DO:

          run pi-cria-dados (c-nom-desenho,       122, 1, "",                  0, 0, 0).

      END.

      run pi-cria-dados (c-rev-desenho,           122, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-dat-desenho,           122, 1, "",                  0, 0, 0).
      run pi-cria-dados (fill("-", 122),            0, 1, "",                  0, 0, 0).

   end.
   
   /*Dados da Operaá∆o - In°cio*/
   run pi-cria-dados ("C¢digo/Nome Operaá∆o ", 22, 1, "", 0, 0, 0).
   run pi-cria-dados (" ",                      1, 0, "", 8, 0, 0).
   run pi-cria-dados ("Seq/Carac.Controlada",  33, 0, "", 8, 0, 0).
   run pi-cria-dados ("|Meio Controle",        16, 0, "", 8, 0, 0).
   run pi-cria-dados ("Amostra",                8, 0, "", 8, 0, 0).   
   run pi-cria-dados ("Freq",                   8, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 1, "", 8, 0, 0).
   run pi-cria-dados ("",                      74, 0, "", 8, 0, 0).
   run pi-cria-dados ("01",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("02",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("03",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("04",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("05",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("06",                     7, 2, "", 8, 0, 0).
   run pi-cria-dados (fill("Ó", 122),           0, 1, "", 0, 0, 0).
END PROCEDURE.



procedure pi-gera-dados-inspecao:

   def var i-qtd-linha-max       as integer       no-undo.
   def var i-qtd-cada            as integer       no-undo.   
   def var c-freq                as char          no-undo.
   def var c-reg                 as char          no-undo.
   def var c-reg-2               as char          no-undo.
   def var v-log-char            as logical       no-undo.
   def var v-cont                as integer       no-undo.
   def var v-cod-caracter        as char          no-undo.
   def var i-qtd-linha           as integer       no-undo.
   def var c-des-freq-med        as char          no-undo.
   def var c-cod-comp            as char          no-undo.
   def var c-des-freq-med-aux    as char          no-undo.
   def var c-descricao           as char extent 2 no-undo.
   def var c-meio-controle       as char extent 2 no-undo.
   def var v-linha-1             as char          no-undo.
   def var v-linha-1-aux         as char          no-undo.
   def var v-linha-aux           as char          no-undo.
   def var i-aux                 as integer       no-undo.

   for each oper-ord where oper-ord.nr-ord-produ = ord-prod.nr-ord-produ no-lock
                  break by(oper-ord.nr-ord-produ):
                  
      if oper-ord.cod-roteiro < tt-param.c-roteiro-ini or
         oper-ord.cod-roteiro > tt-param.c-roteiro-fim then next.

       FIND ext-grup-maquina NO-LOCK
           WHERE ext-grup-maquina.gm-codigo = oper-ord.gm-codigo  NO-ERROR.
           
       run buscar-exames (input        oper-ord.nr-ord-produ,
                          input        oper-ord.it-codigo,
                          input        oper-ord.cod-roteiro,
                          input        oper-ord.op-codigo,
                          INPUT        ord-prod.qt-ordem,
                          output TABLE tt-exames).

       run pi-soma-linha (1).

       run pi-cria-dados (oper-ord.op-codigo,  9, 0, "", 0, 0, 0).
       run pi-cria-dados ("-",                 2, 0, "", 0, 0, 0).
       run pi-cria-dados (oper-ord.descricao, 34, 0, "", 0, 0, 0).

       if not can-find(first tt-exames) then do:
           run pi-cria-dados (" - Operaá∆o sem Caracter°sticas Controladas neste Documento", 60, 0, "", 0, 0, 0).
       end.

       run pi-soma-linha (1).
       run pi-cria-dados (fill("Ó", 122), 122, 1, "", 0, 0, 0).
    
       ASSIGN i-qtd-linha-max = 1.
       
       for each tt-exames by tt-exames.classifica:

          assign c-reg   = ""
                 c-reg-2 = "".
              
          find first comp-exame where rowid(comp-exame) = tt-exames.rowid-comp no-lock no-error.

          find first ext-exame where ext-exame.cod-exame = tt-exames.cod-exame no-lock no-error.          
          
          if available ext-exame then do:
             if ext-exame.log-um-ficha  then c-reg   = string(ext-exame.cd-texto).
             if ext-exame.log-lote-fixo then c-reg   = string(ext-exame.qtd-lote-fixo).             
          
             if ext-exame.log-um-cada   then 
                assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                       c-reg-2 = string(ext-exame.qtd-um-cada).
    
             find first exame where exame.cod-exame = comp-exame.cod-exame no-lock no-error.                
    
             if c-reg = "" or (avail exame and exame.amostragem) then do: /*Amostragem*/
             
                find first bf-item where bf-item.it-codigo = oper-ord.it-codigo no-lock no-error.
                
                find first esp-nivel-insp no-lock
                     where esp-nivel-insp.cod-exame = comp-exame.cod-exame
                       and esp-nivel-insp.cod-comp  = comp-exame.cod-comp no-error.
                
                if avail esp-nivel-insp then do:
                   find first nivel-insp where nivel-insp.nivel     = esp-nivel-insp.nivel              and 
                                               nivel-insp.tam-lote >= integer(ord-prod.qt-ordem) no-lock no-error.
                          
                   if available nivel-insp then do:
                      find first amostra where amostra.tipo-plano  = esp-nivel-insp.tipo-plano          and 
                                               amostra.cod-amostra = nivel-insp.cod-amostra no-lock no-error.
                      if available amostra then
                         assign c-reg   = "1/" + string(round((ord-prod.qt-ordem / amostra.tam-amostra),0))
                                c-reg-2 = string(amostra.tam-amostra).                       
                      else
                         assign c-reg = "Erro1".
                   end.
                   else
                      assign c-reg = "Erro2".
                end.
                else do:
                    if available bf-item then do:
                       find first nivel-insp where nivel-insp.nivel     = bf-item.nivel                 and 
                                                   nivel-insp.tam-lote >= integer(ord-prod.qt-ordem) no-lock no-error.
                             
                       if available nivel-insp then do:
                          find first amostra where amostra.tipo-plano  = 2                      and 
                                                   amostra.cod-amostra = nivel-insp.cod-amostra no-lock no-error.
                          if available amostra then
                             assign c-reg   = "1/" + string(round((ord-prod.qt-ordem / amostra.tam-amostra),0))
                                    c-reg-2 = string(amostra.tam-amostra).
                          
                          else
                             assign c-reg = "Erro1".
                       end.
                       else
                          assign c-reg = "Erro2".
                    end.
                end.
             end.
          end.
       
          find first ext-texto-cq where ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.
          if available ext-texto-cq then do:
             find first tt-ext-texto-cq where tt-ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.
             if not avail tt-ext-texto-cq then do:
                create tt-ext-texto-cq.
                ASSIGN tt-ext-texto-cq.cd-texto  = ext-texto-cq.cd-texto
                       tt-ext-texto-cq.descricao = ext-texto-cq.descricao.
             end.
          end.
    
          ASSIGN v-log-char = no.
       
          do v-cont = 1 to length(c-reg):
             ASSIGN v-cod-caracter = substr(c-reg, v-cont, 1).
           
             if index('01234567890':U, v-cod-caracter) = 0 then do:
                ASSIGN v-log-char = yes.
             end.
          end.  
          
          assign c-freq = c-reg.        
          
          if avail ext-exame and ext-exame.cd-texto <> "" then
             c-reg   = ext-exame.cd-texto.
          else
             c-reg = "".
             
          if c-reg-2 = "" then do:
             assign i-qtd-linha = 1.
          end.
          else do:
             if exame.amostragem then
                if index(c-reg-2, "/") > 0 then 
                   assign i-qtd-cada = integer(substring(c-reg-2,3)).
                else
                   assign i-qtd-cada = integer(c-reg-2). 
             else
                if index(c-freq, "/") > 0 then 
                   assign i-qtd-cada = ord-prod.qt-ordem / integer(substring(c-freq,3)).
                else
                   assign i-qtd-cada = ord-prod.qt-ordem / integer(c-freq).                
         
             if i-qtd-cada <= 6 then
                ASSIGN i-qtd-linha  = 1.
             else
                ASSIGN i-qtd-linha  = truncate(i-qtd-cada / 6, 0).  /*truncate(i-qtd-linha / 7, 0) + (if (i-qtd-linha - truncate(i-qtd-linha / 7, 0)) > 0 then 1 else 0).*/
                  
             if i-qtd-linha * 6 < i-qtd-cada then
                assign i-qtd-linha = i-qtd-linha + 1.

          end.             
          
          if i-qtd-linha > i-qtd-linha-max then
             assign i-qtd-linha-max = i-qtd-linha. 

          find first ext-comp-exame where ext-comp-exame.cod-exame = tt-exames.cod-exame and 
                                          ext-comp-exame.cod-comp  = tt-exames.cod-comp  no-lock no-error.
                                          
          if available ext-comp-exame then
             assign c-des-freq-med = ext-comp-exame.des-freq-med.
          else
             assign c-des-freq-med = "".            
    
          ASSIGN c-cod-comp = string(tt-exames.cod-comp).
          
          if substring(c-des-freq-med,1,10) = ""  or (avail exame and exame.amostragem) then
             assign c-des-freq-med-aux = c-reg-2.
          else
             assign c-des-freq-med-aux = substring(c-des-freq-med,1,10).

          RUN quebrar-texto (INPUT comp-exame.descricao,
                             INPUT 25,
                             OUTPUT c-descricao).
    
          RUN quebrar-texto (INPUT comp-exame.inst-equip,
                             INPUT 15,
                             OUTPUT c-meio-controle).
    
          assign v-linha-1 = " " + 
                             string(c-cod-comp,"x(06)") + 
                             " " + 
                             string(c-descricao[1],"x(25)") + 
                             " |" + 
                             string(c-meio-controle[1],"x(15)") + 
                             " " + 
                             string(substring(c-des-freq-med-aux,1,10),"x(10)") +   /*  Marcio   string(substring(c-des-freq-med-aux,1,7),"x(7)") +    */
                             " __" +
                             /*
                             Solicitado em 19/01/2015 pelo Rodrigo Freitas, para que fique em coment†rio atÇ entrar em vigor a nova SM
                             string(c-freq,"x(06)") + " " +                           
                             */
                             string("______", "x(06)") + " " +
                             string(c-reg,"x(03)") + 
                             " ______ ______ ______ ______ ______ ______".
    
          if c-descricao[2]                  <> "" or 
             c-meio-controle[2]              <> "" or 
             substring(c-des-freq-med,11,10) <> "" then
    
             assign v-linha-1-aux = "        " +
                                    string(c-descricao[2],"x(25)") + 
                                    " |" + 
                                    string(c-meio-controle[2],"x(15)") + 
                                    " " + 
                                    string(substring(c-des-freq-med,11,10),"x(10)")
                                   .
          else
             assign v-linha-1-aux = "".
    
          assign v-linha-aux = "                                                                   " +
                               "        ______ ______ ______ ______ ______ ______".
    
          assign i-aux = 1.
          
          do while i-aux <= i-qtd-linha:
             if i-aux = 1 then do:
                run pi-cria-dados (v-linha-1, 130, 1, "", 8, 0, 0).

                if v-linha-1-aux <> "" then do:
                   run pi-soma-linha (1).
                   run pi-cria-dados (v-linha-1-aux, 68, 0, "", 8, 0, 0).

                   if i-qtd-linha > 1 then do:
                      run pi-cria-dados ("       ______ ______ ______ ______ ______ ______",48, 1, "", 8, 0, 0).
                      assign i-aux = i-aux + 1.
                   end.

                   run pi-soma-linha (1).
                end.
    
                run pi-soma-linha (1).
             end.
             else do:
                run pi-cria-dados (v-linha-aux, 130, 2, "", 8, 0, 0).
             end.

             assign i-aux = i-aux + 1.
          end.
    
          if c-reg = "Erro1" then do:
             run pi-cria-dados (" *** N«O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).
          end.

          if c-reg = "Erro2" then do:
             run pi-cria-dados (" *** N«O FOI ENCONTRADO NIVEL DE INSPEÄAO *** ", 62, 1, "", 8, 0, 0).
          end.
       end.

       if can-find(first tt-exames) then do:
           
          FIND FIRST operacao WHERE operacao.op-codigo = oper-ord.op-codigo NO-LOCK NO-ERROR.
      
          ASSIGN oper-padrao = operacao.nr-oper-pad. 

          IF oper-ord.num-id-operacao = 0  AND oper-ord.descricao = 'USINAGEM - TORNO CNC' THEN ASSIGN oper-padrao = 1.
         
          run pi-soma-linha (2).
        
          IF oper-padrao = 1  THEN DO:

             run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                             Responsavel liberacao Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

          END. 
          ELSE DO:

             run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                             Responsavel liberacao Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

          END.


          run pi-soma-linha (2).

          run pi-cria-dados ("Produá∆o:  Qte. Aprovado:_______________          Qte. Rejeito Comum:_______________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("Operaá∆o: Isento de material de outro lote     ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Plano de controle: Apontamento e preenchimento ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Produto: Identificaá∆o e armazenamento         ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("Qualidade:", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Atributo:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Vari†vel:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("Outros:                          Qte. Rejeito Comum:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("( )Aprovado ( )Reprovado ( )N/A     Nr. RO:__________     Data:__________     Matr°cula:__________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (1).

          run pi-cria-dados (fill("Ó", 122), 122, 0, "", 0, 0, 0).

       end.

       if last-of (oper-ord.nr-ord-produ) then do:

          run buscar-exames (input        oper-ord.nr-ord-produ,
                             input        oper-ord.it-codigo,
                             input        oper-ord.cod-roteiro,
                             input        0,
                             INPUT        ord-prod.qt-ordem,
                             output TABLE tt-exames).

          if can-find(first tt-exames) then do:

             run pi-soma-linha (1).
             run pi-cria-dados ("Caracter°sticas Controladas do Item", 35, 1, "", 0, 0, 0).
             run pi-cria-dados (fill("Ó", 122), 122, 1, "", 0, 0, 0).

          end.
    
          ASSIGN i-qtd-linha-max = 1.
       
          for each tt-exames by tt-exames.classifica:
          
             find first comp-exame where rowid(comp-exame) = tt-exames.rowid-comp no-lock no-error.
                    
             assign c-reg   = ""
                    c-reg-2 = "".

             find first ext-exame where ext-exame.cod-exame = comp-exame.cod-exame no-lock no-error.
             
             if available ext-exame then do:
                if ext-exame.log-um-ficha  then c-reg = string(ext-exame.cd-texto).
                if ext-exame.log-lote-fixo then c-reg = string(ext-exame.qtd-lote-fixo).
              
                if ext-exame.log-um-cada then 
                   assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                          c-reg-2 = string(ext-exame.qtd-um-cada).
                          
                find first exame where exame.cod-exame = comp-exame.cod-exame no-lock no-error.                  
    
                if c-reg = "" or (avail exame and exame.amostragem) then do: /*Amostragem*/
                
                   find first bf-item where bf-item.it-codigo = oper-ord.it-codigo no-lock no-error.
                   
                   find first esp-nivel-insp no-lock
                        where esp-nivel-insp.cod-exame = comp-exame.cod-exame
                          and esp-nivel-insp.cod-comp  = comp-exame.cod-comp no-error.
                
                   if avail esp-nivel-insp then do:
                      find first nivel-insp where nivel-insp.nivel     = esp-nivel-insp.nivel              and 
                                                  nivel-insp.tam-lote >= integer(ord-prod.qt-ordem) no-lock no-error.
                              
                      if available nivel-insp then do:
                         find first amostra where amostra.tipo-plano  = esp-nivel-insp.tipo-plano                      and 
                                                  amostra.cod-amostra = nivel-insp.cod-amostra no-lock no-error.
                         if available amostra then
                            assign c-reg   = "1/" + string(round((ord-prod.qt-ordem / amostra.tam-amostra),0))
                                   c-reg-2 = string(amostra.tam-amostra).                       
                         else
                            assign c-reg = "Erro1".
                      end.
                      else
                         assign c-reg = "Erro2".
                   end.
                   else do:
                   
                      if available bf-item then do:
                         find first nivel-insp no-lock
                              where nivel-insp.nivel     = bf-item.nivel
                                and nivel-insp.tam-lote >= integer(ord-prod.qt-ordem) no-error.
                         if available nivel-insp then do:
                            find first amostra where amostra.tipo-plano  = 2                      and 
                                                     amostra.cod-amostra = nivel-insp.cod-amostra no-lock no-error.
                                                        
                            if available amostra then
                               assign c-reg   = "1/" + string(round((ord-prod.qt-ordem / amostra.tam-amostra),0))
                                      c-reg-2 = string(amostra.tam-amostra).
                            else
                               assign c-reg = "Erro1".
                         end.
                         else
                           assign c-reg = "Erro2".
                       end.
                   end.
                end.
             end.

             find first ext-texto-cq where ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.
             if available ext-texto-cq then do:
                find first tt-ext-texto-cq where tt-ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.
                if not available tt-ext-texto-cq then do:
                   create tt-ext-texto-cq.
                   ASSIGN tt-ext-texto-cq.cd-texto  = ext-texto-cq.cd-texto
                          tt-ext-texto-cq.descricao = ext-texto-cq.descricao.
                end.
             end.
       
             ASSIGN v-log-char = no.
             do v-cont = 1 to length(c-reg):
                ASSIGN v-cod-caracter = substr(c-reg, v-cont, 1).
              
                if index('01234567890':U, v-cod-caracter) = 0 then do:
                   ASSIGN v-log-char = yes.
                end.
             end.
             
             if avail ext-exame and ext-exame.cd-texto <> "" then
                c-reg   = ext-exame.cd-texto.
             else
                c-reg = "".
                
             if c-reg-2 = "" then do:
                assign i-qtd-linha = 1.
             end.
             else do:
                if index(c-reg-2, "/") > 0 then 
                   assign i-qtd-linha = ord-prod.qt-ordem / integer(substring(c-reg-2,3)).
                else
                   assign i-qtd-linha = integer(c-reg-2).   
             
                ASSIGN i-qtd-linha = truncate(ord-prod.qt-ordem / i-qtd-linha, 0) + (if ((ord-prod.qt-ordem / i-qtd-linha) - truncate(ord-prod.qt-ordem / i-qtd-linha, 0)) > 0 then 1 else 0).             
          
             end.
    
             if i-qtd-linha > i-qtd-linha-max then
                assign i-qtd-linha-max = i-qtd-linha.
    
             find first ext-comp-exame where ext-comp-exame.cod-exame = comp-exame.cod-exame and 
                                             ext-comp-exame.cod-comp  = comp-exame.cod-comp  no-lock no-error.
                 
             if available ext-comp-exame then
                assign c-des-freq-med = ext-comp-exame.des-freq-med.
             else
                assign c-des-freq-med = "".            
        
             c-cod-comp = string(comp-exame.cod-comp).
          
             if substring(c-des-freq-med,1,10) = "" or (avail exame and exame.amostragem) /*or (avail ext-exame and ext-exame.log-um-cada)*/ then
                assign c-des-freq-med-aux = c-reg-2.
             else
                assign c-des-freq-med-aux = substring(c-des-freq-med,1,10).
                
             if index(c-des-freq-med, "/") > 0 then
                assign i-qtd-linha = 1.   
              
             RUN quebrar-texto (INPUT comp-exame.descricao,
                                INPUT 25,
                                OUTPUT c-descricao).

             RUN quebrar-texto (INPUT comp-exame.inst-equip,
                                INPUT 15,
                                OUTPUT c-meio-controle).
        
             assign v-linha-1 = " " + string(c-cod-comp,"x(06)") + 
                                " " + 
                                string(c-descricao[1],"x(25)") + 
                                " |" + 
                                string(c-meio-controle[1],"x(15)") + 
                                " " + 
                                string(substring(c-des-freq-med-aux,1,10),"x(10)") + 
                                " " +
                                string(c-reg,"x(07)") +                                 
                                "______ ______ ______ ______ ______ ______ ______"
                                .
    
             if c-descricao[2]                  <> "" or 
                c-meio-controle[2]              <> "" or 
                substring(c-des-freq-med,11,10) <> "" then
        
                assign v-linha-1-aux =  "        " +
                                        string(c-descricao[2],"x(25)") + 
                                        " |" + 
                                        string(c-meio-controle[2],"x(15)") + 
                                        " " + 
                                        string(substring(c-des-freq-med,11,10),"x(10)")
                                      .
             else
                assign v-linha-1-aux = "".
        
             assign v-linha-aux = "                                                                   " +
                                  "  ______ ______ ______ ______ ______ ______ ______".
        
             assign i-aux = 1.
        
             do while i-aux <= i-qtd-linha:
              
                if i-aux = 1 then do:
              
                   run pi-cria-dados (v-linha-1, 130, 1, "", 8, 0, 0).

                   if v-linha-1-aux <> "" then do:

                      run pi-soma-linha (1).

                      run pi-cria-dados (v-linha-1-aux, 68, 0, "", 8, 0, 0).

                      if i-qtd-linha > 1 then do:

                         run pi-cria-dados (" ______ ______ ______ ______ ______ ______ ______", 49, 1, "", 8, 0, 0).

                         assign i-aux = i-aux + 1.

                      end.

                      run pi-soma-linha (1).

                   end.

                   run pi-soma-linha (1).
        
                end.
                else do:

                   run pi-cria-dados (v-linha-aux, 130, 2, "", 8, 0, 0).

               end.

               assign i-aux = i-aux + 1.

            end.
    
            if c-reg = "Erro1" then do:

               run pi-cria-dados (" *** N«O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).

            end.

            if c-reg = "Erro2" then do:

               run pi-cria-dados (" *** N«O FOI ENCONTRADO NIVEL DE INSPEÄAO *** ", 62, 1, "", 8, 0, 0).

            end.
         end.

         if can-find(first tt-exames) then do:

            FIND FIRST operacao NO-LOCK WHERE operacao.op-codigo = oper-ord.op-codigo NO-ERROR.
      
            ASSIGN oper-padrao1 = operacao.nr-oper-pad. 

            IF oper-ord.num-id-operacao = 0  AND oper-ord.descricao = 'USINAGEM - TORNO CNC' THEN ASSIGN oper-padrao1 = 1.
               
            RUN pi-soma-linha (2).
       
            IF oper-padrao1 = 1  THEN DO:

               run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               RUN pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                             Responsavel liberacao Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               RUN pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               RUN pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               RUN pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

            END. 
            ELSE DO:
            
               run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                             Responsavel liberacao Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
               run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          
            END.

            run pi-soma-linha (2).

            run pi-cria-dados ("Produá∆o:  Qte. Aprovado:_______________          Qte. Rejeito Comum:_______________", 118, 2, "", 8, 0, 0).

            run pi-soma-linha (2).

            run pi-cria-dados ("Operaá∆o: Isento de material de outro lote     ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("Plano de controle: Apontamento e preenchimento ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("Produto: Identificaá∆o e armazenamento         ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

            run pi-soma-linha (2).

            run pi-cria-dados ("Qualidade:", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("Atributo:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("Vari†vel:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

            run pi-soma-linha (2).

            run pi-cria-dados ("Outros:                          Qte. Rejeito Comum:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

            run pi-soma-linha (2).

            run pi-cria-dados ("( )Aprovado ( )Reprovado ( )N/A     Nr. RO:__________     Data:__________     Matr°cula:__________", 118, 2, "", 8, 0, 0).

            run pi-soma-linha (1).

            run pi-cria-dados (fill("Ó", 122), 122, 0, "", 8, 0, 0).

         end.
      end.
   end.

   run pi-soma-linha (2).

   run pi-cria-dados (fill("-", 122), 122, 0, "", 8, 0, 0).

   if can-find(first tt-ext-texto-cq) then do:

      run pi-soma-linha (1).

      run pi-cria-dados ("OBSERVAÄ«O:", 16, 2, "", 0, 0, 0).

      for each tt-ext-texto-cq where tt-ext-texto-cq.cd-texto <> "":

         run pi-cria-dados (" - ", 3, 0, "", 0, 0, 0).

         run pi-cria-dados (tt-ext-texto-cq.cd-texto, 2, 1, "", 0, 0, 0).

         empty temp-table tt-editor.
        
         run pi-print-editor (tt-ext-texto-cq.descricao, 90).
        
         for each tt-editor:

            run pi-cria-dados ("   ", 3, 0, "", 0, 0, 0).

            run pi-cria-dados (tt-editor.conteudo, 90, 1, "", 0, 0, 0).

         end.

         run pi-soma-linha (1).
        
      end.

      empty temp-table tt-ext-texto-cq.

   end.
end procedure.

PROCEDURE pi-gera-cabecalho-item:

   def input parameter l-imp-revisao as logical no-undo.

   def var c-desc-rev-plano-insp as char    no-undo.
   def var v-dat-ini-validade    as date    no-undo.
   def var c-nom-desenho         as char    no-undo.
   def var c-rev-desenho         as char    no-undo.
   def var c-dat-desenho         as char    no-undo.
   def var i-qt-caracter         as integer no-undo.

   tt-pdf.qt-paginas = tt-pdf.qt-paginas + 1.
   tt-pdf.qt-pg-insp = tt-pdf.qt-pg-insp + 1.

   /* Para n∆o imprimir o fundo "Desenvolvimento nas p†ginas de inspeá∆o */
   CREATE tt-not-desenvolvimento.
   ASSIGN nr-pagina = tt-pdf.qt-paginas.

  

   run pi-cria-dados (c-empresa,                                        85, 1, "",                   0, 0, 0).
   run pi-cria-dados (c-Programa + " - " + c-versao + "." + c-revisao,  40, 1, "",                   0, 0, 0).
   run pi-cria-dados (" ",                                              40, 0, "",                   0, 0, 0).
   run pi-cria-dados ("PLANO DE CONTROLE DE INSPEÄ«O POR ITEM",         38, 3, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("ITEM: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (item.it-codigo + " - " + item.desc-item,          83, 2, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("U.M.: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (item.un,                                          50, 1, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados (fill("-", 122),                                  122, 1, "",                   0, 0, 0).

   if l-imp-revisao then do:

      /*Revis∆o Plano de Inspeá∆o*/
      find last texto where texto.it-codigo = item.it-codigo no-lock no-error.

      if available texto then do:

          find first tipo-texto where tipo-texto.tipo = texto.tipo no-lock no-error.

          if available tipo-texto then
              c-desc-rev-plano-insp = tipo-texto.descricao.
          else
              c-desc-rev-plano-insp = "".

          find first ext-texto where ext-texto.it-codigo = texto.it-codigo and 
                                     ext-texto.tipo      = texto.tipo      no-lock no-error.

          if available ext-texto then do:
              assign v-dat-ini-validade = ext-texto.dat-ini-validade.
          end.
          else 
              assign v-dat-ini-validade = today.

      end.
      else
          assign c-desc-rev-plano-insp = "".

      run pi-cria-dados ("Plano de Inspeá∆o: ", 19, 0, "", 0, 0, 0).
      run pi-cria-dados ((IF NOT tt-pdf.desenvolvimento THEN c-desc-rev-plano-insp ELSE ""), 33, 0, "", 0, 0, 0).

      /*Inicio Validade*/
      run pi-cria-dados ("Data: ",                                 6, 0, "", 0, 0, 0).
      run pi-cria-dados ((IF NOT tt-pdf.desenvolvimento THEN string(v-dat-ini-validade,"99/99/9999") ELSE "          "), 10, 2, "", 0, 0, 0).

      ASSIGN i-qt-caracter = 0. 

      /*Revis∆o de Desenhos*/
      for each desenho-item where desenho-item.it-codigo = item.it-codigo         no-lock,
          last revisao      where revisao.de-codigo      = desenho-item.de-codigo no-lock:

          ASSIGN i-qt-caracter = maximum(10, length(revisao.de-codigo)).

      end.

      IF i-qt-caracter = 0 THEN

          ASSIGN c-nom-desenho = ""
                 c-rev-desenho = ""
                 c-dat-desenho = "". 

      ELSE DO:

          ASSIGN c-nom-desenho = ""
                 c-rev-desenho = ""
                 c-dat-desenho = "". 

          /*Revis∆o de Desenhos*/
          for each desenho-item where desenho-item.it-codigo = item.it-codigo         no-lock,
              last revisao      where revisao.de-codigo      = desenho-item.de-codigo no-lock:

              ASSIGN c-nom-desenho = revisao.de-codigo                                                                                                         + fill(" ", i-qt-caracter - length(revisao.de-codigo)) + (if c-nom-desenho = "" then "" else " / " + c-nom-desenho)
                     c-rev-desenho = (IF NOT tt-pdf.desenvolvimento THEN revisao.rv-codigo                          ELSE FILL(" ", LENGTH(revisao.rv-codigo))) + fill(" ", i-qt-caracter - length(revisao.rv-codigo)) + (if c-rev-desenho = "" then "" else " / " + c-rev-desenho)
                     c-dat-desenho = (IF NOT tt-pdf.desenvolvimento THEN STRING(revisao.data-revisao, "99/99/9999") ELSE "          ")                         + fill(" ", i-qt-caracter - 10)                        + (if c-dat-desenho = "" then "" else " / " + c-dat-desenho).

          end.

          ASSIGN c-nom-desenho = "Desenho: " + c-nom-desenho
                 c-rev-desenho = "Revis∆o: " + c-rev-desenho
                 c-dat-desenho = "Data:    " + c-dat-desenho.

      END.

      run pi-cria-dados ("Revis∆o de Desenhos: ",  22, 1, defaultFontBoldName, 0, 0, 0).
      run pi-cria-dados (c-nom-desenho,           122, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-rev-desenho,           122, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-dat-desenho,           122, 1, "",                  0, 0, 0).
      run pi-cria-dados (fill("-", 122),            0, 1, "",                  0, 0, 0).

   end.
   
   /*Dados da Operaá∆o - In°cio*/
   
   run pi-cria-dados ("C¢digo/Nome Operaá∆o ", 22, 1, "", 0, 0, 0).
   run pi-cria-dados (" ",                      1, 0, "", 8, 0, 0).
   run pi-cria-dados ("Seq/Carac.Controlada",  33, 0, "", 8, 0, 0).
   run pi-cria-dados ("|Meio Controle",        16, 0, "", 8, 0, 0).
   run pi-cria-dados ("Amostra",                8, 0, "", 8, 0, 0).   
   run pi-cria-dados ("Freq",                   8, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 1, "", 8, 0, 0).
   run pi-cria-dados ("",                      74, 0, "", 8, 0, 0).
   run pi-cria-dados ("01",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("02",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("03",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("04",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("05",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("06",                     7, 2, "", 8, 0, 0).
   run pi-cria-dados (fill("Ó", 122),           0, 1, "", 0, 0, 0).


END PROCEDURE.



procedure pi-consiste-revisao:
    
   def input parameter  i-nr-ord-produ like ord-prod.nr-ord-produ no-undo.
   def input parameter  c-it-codigo    like ord-prod.it-codigo    no-undo.
   def output parameter l-em-revisao   as logical                 no-undo.

   if i-nr-ord-produ = 0 then do:

      for each operacao where operacao.it-codigo = c-it-codigo no-lock:

         find first cst_ext_operacao where cst_ext_operacao.it-codigo   = operacao.it-codigo  /* and
                                           cst_ext_operacao.cod-roteiro = operacao.cod-roteiro and
                                           cst_ext_operacao.op-codigo   = operacao.op-codigo */  no-lock no-error.

         if available cst_ext_operacao and cst_ext_operacao.log_revisao then do:

            l-em-revisao = yes.

            leave.

         end.
      end.
   end.
   else do:

      for each oper-ord where oper-ord.nr-ord-produ = i-nr-ord-produ no-lock:

         find first cst_ext_operacao where cst_ext_operacao.it-codigo   = oper-ord.it-codigo /*  and
                                           cst_ext_operacao.cod-roteiro = oper-ord.cod-roteiro and
                                           cst_ext_operacao.op-codigo   = oper-ord.op-codigo */  no-lock no-error.

         if available cst_ext_operacao and cst_ext_operacao.log_revisao then do:

            l-em-revisao = yes.

            leave.

         end.
      end.
   end.

end procedure.

procedure pi-gera-dados-item:

   def var i-qtd-linha-max    as integer       no-undo.
   def var c-reg              as char          no-undo.
   def var c-reg-2            as char          no-undo.
   def var v-log-char         as logical       no-undo.
   def var v-cont             as integer       no-undo.
   def var v-cod-caracter     as char          no-undo.
   def var i-qtd-linha        as integer       no-undo.
   def var c-des-freq-med     as char          no-undo.
   def var c-cod-comp         as char          no-undo.
   def var c-des-freq-med-aux as char          no-undo.
   def var c-descricao        as char extent 2 no-undo.
   def var c-meio-controle    as char extent 2 no-undo.
   def var v-linha-1          as char          no-undo.
   def var v-linha-1-aux      as char          no-undo.
   def var v-linha-aux        as char          no-undo.
   def var i-aux              as integer       no-undo.
   def var c-freq             as char          no-undo.
   def var i-qtd-cada         as integer       no-undo.   

   empty temp-table tt-op-temp.

   for each operacao where operacao.it-codigo    = item.it-codigo         and
                           operacao.cod-roteiro >= tt-param.c-roteiro-ini and
                           operacao.cod-roteiro <= tt-param.c-roteiro-fim no-lock:

      if operacao.data-inicio  <= today and
         operacao.data-termino >= today then do:

         create tt-op-temp.

         buffer-copy operacao to tt-op-temp.

      end.
   end.

   if not can-find(first tt-op-temp no-lock) then do:

      for each rot-item where rot-item.it-codigo    = item.it-codigo         and
                              rot-item.cod-roteiro >= tt-param.c-roteiro-ini and
                              rot-item.cod-roteiro <= tt-param.c-roteiro-fim no-lock:

         if rot-item.data-inicio  <= today and
            rot-item.data-termino >= today then do:

            for each operacao where operacao.it-codigo   = ""                   and
                                    operacao.cod-roteiro = rot-item.cod-roteiro no-lock:

               if operacao.data-inicio  <= today and
                  operacao.data-termino >= today then do:

                  create tt-op-temp.

                  buffer-copy operacao to tt-op-temp.

               end.
            end.
         end.
      end.
   end.

   for each tt-op-temp                                                           no-lock,
      first operacao where operacao.num-id-operacao = tt-op-temp.num-id-operacao no-lock
                  break by tt-op-temp.it-codigo
                        by tt-op-temp.op-codigo:

       if operacao.data-inicio  > today or
          operacao.data-termino < today then next.
               
       if operacao.cod-roteiro < tt-param.c-roteiro-ini or
          operacao.cod-roteiro > tt-param.c-roteiro-fim then next.

       run buscar-exames (input        0,
                          input        item.it-codigo,
                          input        operacao.cod-roteiro,
                          input        operacao.op-codigo,
                          INPUT        tt-param.i-qte,
                          output TABLE tt-exames).

       run pi-soma-linha (1).

       run pi-cria-dados (operacao.op-codigo,  9, 0, "", 0, 0, 0).
       run pi-cria-dados ("-",                 2, 0, "", 0, 0, 0).
       run pi-cria-dados (operacao.descricao, 34, 0, "", 0, 0, 0).

       if not can-find(first tt-exames) then do:
           run pi-cria-dados (" - Operaá∆o sem Caracter°sticas Controladas", 49, 0, "", 0, 0, 0).
       end.

       run pi-soma-linha (1).

       run pi-cria-dados (fill("Ó", 122), 122, 1, "", 0, 0, 0).
    
       i-qtd-linha-max = 1.
                     
       for each tt-exames by tt-exames.classifica:
    
           assign c-reg   = ""
                  c-reg-2 = "".              

           find first comp-exame where rowid(comp-exame) = tt-exames.rowid-comp no-lock no-error.

           find first ext-exame where ext-exame.cod-exame = tt-exames.cod-exame no-lock no-error.
          
           if available ext-exame then do:          

              if ext-exame.log-um-ficha  then c-reg   = string(ext-exame.cd-texto).
              if ext-exame.log-lote-fixo then c-reg   = string(ext-exame.qtd-lote-fixo).

              if ext-exame.log-um-cada   then 
                 assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                        c-reg-2 = string(ext-exame.qtd-um-cada).

              find first exame where exame.cod-exame = comp-exame.cod-exame no-lock no-error.                     
    
              if c-reg = "" or (avail exame and exame.amostrage) then do: /*Amostragem*/    

                 find first esp-nivel-insp no-lock  
                      where esp-nivel-insp.cod-exame = comp-exame.cod-exame
                        and esp-nivel-insp.cod-comp  = comp-exame.cod-comp no-error.

                 if avail esp-nivel-insp then do:
                    find first nivel-insp where nivel-insp.nivel     = esp-nivel-insp.nivel              and 
                                                nivel-insp.tam-lote >= tt-param.i-qte no-lock no-error.

                    if available nivel-insp and tt-param.i-qte <> 0 then do:
                       find first amostra where amostra.tipo-plano   = esp-nivel-insp.tipo-plano          and 
                                                amostra.cod-amostra  = nivel-insp.cod-amostra no-lock no-error.
                       if available amostra then
                          assign c-reg   = "1/" + string(round((tt-param.i-qte / amostra.tam-amostra),0))
                                 c-reg-2 = string(amostra.tam-amostra).                       
                       else
                          assign c-reg = "Erro1".
                    end.
                    else
                       assign c-reg = " ".
                 end.
                 else do:
                    find first nivel-insp where nivel-insp.nivel     = item.nivel and 
                                                nivel-insp.tam-lote >= 1          no-lock no-error.

                    if available nivel-insp then do:                  
                       find first amostra where amostra.tipo-plano  = 2                      and 
                                                amostra.cod-amostra = nivel-insp.cod-amostra no-lock no-error.

                       if available amostra then
                          assign c-reg   = "1/" + string(amostra.tam-amostra)
                                 c-reg-2 = string(amostra.tam-amostra).

                       else
                          assign c-reg = "Erro1".
                    end.
                    else
                       assign c-reg = "Erro2".

                 end.    
              end.
           end.
         
           find first ext-texto-cq where ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.

           if available ext-texto-cq then do:

                find first tt-ext-texto-cq where tt-ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.

                if not avail tt-ext-texto-cq then do:

                   create tt-ext-texto-cq.

                   tt-ext-texto-cq.cd-texto  = ext-texto-cq.cd-texto.
                   tt-ext-texto-cq.descricao = ext-texto-cq.descricao.

                end.
           end.

           v-log-char = no.

           do v-cont = 1 to length(c-reg):

              v-cod-caracter = substr(c-reg, v-cont, 1).

              if index('01234567890':U, v-cod-caracter) = 0 then do:

                 v-log-char = yes.

              end.
           end.
    
           assign c-freq = c-reg.        
          
           if avail ext-exame and ext-exame.cd-texto <> "" then
              c-reg   = ext-exame.cd-texto.
           else
              c-reg = "".             

           if c-reg-2 = "" then do:
              assign i-qtd-linha = 1.
           end.
           else do:
              if exame.amostragem then
                 if index(c-reg-2, "/") > 0 then 
                    assign i-qtd-cada = integer(substring(c-reg-2,3)).
                 else
                    assign i-qtd-cada = integer(c-reg-2). 
              else
                 if index(c-freq, "/") > 0 then 
                    assign i-qtd-cada = tt-param.i-qte / integer(substring(c-freq,3)).
                 else
                    assign i-qtd-cada = tt-param.i-qte / integer(c-freq).                

               if i-qtd-cada <= 6 then
                  ASSIGN i-qtd-linha  = 1.
               else
                  ASSIGN i-qtd-linha  = truncate(i-qtd-cada / 6, 0).  /*truncate(i-qtd-linha / 7, 0) + (if (i-qtd-linha - truncate(i-qtd-linha / 7, 0)) > 0 then 1 else 0).*/

               if i-qtd-linha * 6 < i-qtd-cada then
                  assign i-qtd-linha = i-qtd-linha + 1.
           end.             

           find first ext-comp-exame where ext-comp-exame.cod-exame = tt-exames.cod-exame and 
                                           ext-comp-exame.cod-comp  = tt-exames.cod-comp  no-lock no-error.

           if available ext-comp-exame then
              assign c-des-freq-med = ext-comp-exame.des-freq-med.
           else
              assign c-des-freq-med = "".            

           c-cod-comp = string(tt-exames.cod-comp).

           if substring(c-des-freq-med,1,10) = "" or (avail exame and exame.amostragem) /*or (avail ext-exame and ext-exame.log-um-cada)*/ then
              assign c-des-freq-med-aux = c-reg-2. /*amostra.tam-amostra*/
           else
              assign c-des-freq-med-aux = substring(c-des-freq-med,1,10).

           RUN quebrar-texto (INPUT comp-exame.descricao,
                              INPUT 25,
                              OUTPUT c-descricao).

           RUN quebrar-texto (INPUT comp-exame.inst-equip,
                              INPUT 15,
                              OUTPUT c-meio-controle).  
  
           assign v-linha-1 = " " + 
                              string(c-cod-comp,"x(06)") + 
                              " " + 
                              string(c-descricao[1],"x(25)") + 
                              " |" + 
                              string(c-meio-controle[1],"x(15)") + 
                              " " + 
                              string(substring(c-des-freq-med-aux,1,10),"x(10)") +    /* MArcio   string(substring(c-des-freq-med-aux,1,7),"x(7)") + )  */
                              " __" +
                              /*
                              Solicitado em 19/01/2015 pelo Rodrigo Freitas, para que fique em coment†rio atÇ entrar em vigor a nova SM
                              string(c-freq,"x(06)") + " " +                           
                              */
                              string("______", "x(06)") + " " +
                              string(c-reg,"x(03)")  +
                              "______ ______ ______ ______ ______ ______".

                           .

           if c-descricao[2]                  <> "" or 
              c-meio-controle[2]              <> "" or 
              substring(c-des-freq-med,11,10) <> "" then

              assign v-linha-1-aux = "        " +
                                     string(c-descricao[2],"x(25)") + 
                                     " |" + 
                                     string(c-meio-controle[2],"x(15)") + 
                                     " " + 
                                     string(substring(c-des-freq-med,11,10),"x(10)")
                                    .
           else
              assign v-linha-1-aux = "".

           assign v-linha-aux = "                                                                   " +
                                "       ______ ______ ______ ______ ______ ______".

           assign i-aux = 1.

           do while i-aux <= i-qtd-linha:

              if i-aux = 1 then do:

                 run pi-cria-dados (v-linha-1, 130, 1, "", 8, 0, 0).

                 if v-linha-1-aux <> "" then do:

                    run pi-soma-linha (1).

                    run pi-cria-dados (v-linha-1-aux, 68, 0, "", 8, 0, 0).

                    if i-qtd-linha > 1 then do:

                       run pi-cria-dados ("      ______ ______ ______ ______ ______ ______",48, 1, "", 8, 0, 0).

                       assign i-aux = i-aux + 1.

                    end.

                    run pi-soma-linha (1).

                 end.

                 run pi-soma-linha (1).

              end.
              else do:

                 run pi-cria-dados (v-linha-aux, 130, 2, "", 8, 0, 0).

              end.

              assign i-aux = i-aux + 1.

           end.

           if c-reg = "Erro1" then do:

              run pi-cria-dados (" *** N«O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).

           end.

           if c-reg = "Erro2" then do:

              run pi-cria-dados (" *** N«O FOI ENCONTRADO NIVEL DE INSPEÄAO *** ", 62, 1, "", 8, 0, 0).

           end.
       end.

       if can-find(first tt-exames) then do:

          IF AVAIL operacao AND operacao.nr-oper-pad > 0 THEN

             ASSIGN oper-padrao2 = operacao.nr-oper-pad. 

          ELSE DO:

             ASSIGN oper-padrao2 = 2.

          END.

          IF oper-padrao2 = 1  THEN DO:

             run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                             Responsavel liberacao Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                          ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

          END. 
          ELSE DO:

             run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                             Responsavel liberacao Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

          END.

          run pi-soma-linha (2).

          run pi-cria-dados ("Produá∆o:  Qte. Aprovado:_______________          Qte. Rejeito Comum:_______________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("Operaá∆o: Isento de material de outro lote     ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Plano de controle: Apontamento e preenchimento ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Produto: Identificaá∆o e armazenamento         ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("Qualidade:", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Atributo:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("Vari†vel:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("Outros:                          Qte. Rejeito Comum:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (2).

          run pi-cria-dados ("( )Aprovado ( )Reprovado ( )N/A     Nr. RO:__________     Data:__________     Matr°cula:__________", 118, 2, "", 8, 0, 0).

          run pi-soma-linha (1).

          run pi-cria-dados (fill("Ó", 122), 122, 0, "", 0, 0, 0).

       end.

       if last-of (tt-op-temp.it-codigo) then do:

          run buscar-exames (input        0,
                             input        item.it-codigo,
                             input        operacao.cod-roteiro,
                             input        0,
                             INPUT        tt-param.i-qte,
                             output TABLE tt-exames).

          if can-find(first tt-exames) then do:

             run pi-soma-linha (1).

             run pi-cria-dados ("Caracter°sticas Controladas do Item", 35, 1, "", 0, 0, 0).

             run pi-cria-dados (fill("Ó", 122), 122, 1, "", 0, 0, 0).

          end.

          i-qtd-linha-max = 1.

          for each tt-exames by tt-exames.classifica:

             find first comp-exame where rowid(comp-exame) = tt-exames.rowid-comp no-lock no-error.

             assign c-reg   = ""
                    c-reg-2 = "".

             find first ext-exame where ext-exame.cod-exame = comp-exame.cod-exame no-lock no-error.
           
             if available ext-exame then do:

                if ext-exame.log-um-ficha  then c-reg = string(ext-exame.cd-texto).
                if ext-exame.log-lote-fixo then c-reg = string(ext-exame.qtd-lote-fixo).

                if ext-exame.log-um-cada then 
                   assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                          c-reg-2 = string(ext-exame.qtd-um-cada).

                find first exame where exame.cod-exame = comp-exame.cod-exame no-lock no-error.   

                if c-reg = "" or (avail exame and exame.amostrage) then do: /*Amostragem*/

                   find first esp-nivel-insp no-lock  
                        where esp-nivel-insp.cod-exame = comp-exame.cod-exame
                          and esp-nivel-insp.cod-comp  = comp-exame.cod-comp no-error.

                   if avail esp-nivel-insp then do:
                      find first nivel-insp where nivel-insp.nivel     = esp-nivel-insp.nivel              and 
                                                  nivel-insp.tam-lote >= tt-param.i-qte no-lock no-error.

                      if available nivel-insp and tt-param.i-qte <> 0 then do:
                         find first amostra where amostra.tipo-plano   = esp-nivel-insp.tipo-plano          and 
                                                  amostra.cod-amostra  = nivel-insp.cod-amostra no-lock no-error.
                         if available amostra then
                            assign c-reg   = "1/" + string(round((tt-param.i-qte / amostra.tam-amostra),0))
                                   c-reg-2 = string(amostra.tam-amostra).                       
                         else
                            assign c-reg = "Erro1".
                      end.
                      else
                         assign c-reg = " ".
                   end.
                end.
             end.
       
             find first ext-texto-cq where ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.

             if available ext-texto-cq then do:

                find first tt-ext-texto-cq where tt-ext-texto-cq.cd-texto = string(c-reg) no-lock no-error.

                if not available tt-ext-texto-cq then do:

                   create tt-ext-texto-cq.

                   tt-ext-texto-cq.cd-texto  = ext-texto-cq.cd-texto.
                   tt-ext-texto-cq.descricao = ext-texto-cq.descricao.

                end.
             end.

             v-log-char = no.

             do v-cont = 1 to length(c-reg):

                v-cod-caracter = substr(c-reg, v-cont, 1).

                if index('01234567890':U, v-cod-caracter) = 0 then do:

                   v-log-char = yes.

                end.
             end.
    
             if c-reg-2 = "" then do:

                if v-log-char then

                   assign i-qtd-linha = 1.

                else do:

                   i-qtd-linha = truncate(decimal(c-reg), 0) + (if (decimal(c-reg) - truncate(decimal(c-reg), 0)) > 0 then 1 else 0).

                   if i-qtd-linha <= 7 then
                      i-qtd-linha  = 1.
                   else
                      i-qtd-linha  = truncate(i-qtd-linha / 7, 0) + (if (i-qtd-linha - truncate(i-qtd-linha / 7, 0)) > 0 then 1 else 0).

                end.
             end.
             else do:

                i-qtd-linha = integer(substring(c-reg,3)).

                if i-qtd-linha <= 7 then
                   i-qtd-linha  = 1.
                else
                   i-qtd-linha  = truncate(i-qtd-linha / 7, 0) + (if (i-qtd-linha - truncate(i-qtd-linha / 7, 0)) > 0 then 1 else 0).

             end.

             if i-qtd-linha > i-qtd-linha-max then
                assign i-qtd-linha-max = i-qtd-linha.

             find first ext-comp-exame where ext-comp-exame.cod-exame = comp-exame.cod-exame and 
                                             ext-comp-exame.cod-comp  = comp-exame.cod-comp  no-lock no-error.

             if available ext-comp-exame then
                assign c-des-freq-med = ext-comp-exame.des-freq-med.
             else
                assign c-des-freq-med = "".            

             c-cod-comp = string(comp-exame.cod-comp).
       
             if substring(c-des-freq-med,1,10) = "" or (avail exame and exame.amostragem) or (avail ext-exame and ext-exame.log-um-cada) then
                assign c-des-freq-med-aux = c-reg-2.
             else
                assign c-des-freq-med-aux = substring(c-des-freq-med,1,10).

             RUN quebrar-texto (INPUT comp-exame.descricao,
                                INPUT 25,
                                OUTPUT c-descricao).

             RUN quebrar-texto (INPUT comp-exame.inst-equip,
                                INPUT 15,
                                OUTPUT c-meio-controle).

             assign v-linha-1 = " " + string(c-cod-comp,"x(06)") + 
                                " " + 
                                string(c-descricao[1],"x(25)") + 
                                " |" + 
                                string(c-meio-controle[1],"x(15)") + 
                                " " + 
                                string(substring(c-des-freq-med-aux,1,10),"x(10)") + 
                                " " +
                                string(c-reg,"x(07)") + 
                                " " +
                                "______ ______ ______ ______ ______ ______ ______"
                                .

             if c-descricao[2]                  <> "" or 
                c-meio-controle[2]              <> "" or 
                substring(c-des-freq-med,11,10) <> "" then

                assign v-linha-1-aux =  "        " +
                                        string(c-descricao[2],"x(25)") + 
                                        " |" + 
                                        string(c-meio-controle[2],"x(15)") + 
                                        " " + 
                                        string(substring(c-des-freq-med,11,10),"x(10)")
                                      .
             else
                assign v-linha-1-aux = "".

             assign v-linha-aux = "                                                                   " +
                                  "   ______ ______ ______ ______ ______ ______".

             assign i-aux = 1.
       
             do while i-aux <= i-qtd-linha:
          
                 if i-aux = 1 then do:

                    run pi-cria-dados (v-linha-1, 130, 1, "", 8, 0, 0).

                    if v-linha-1-aux <> "" then do:

                       run pi-soma-linha (1).

                       run pi-cria-dados (v-linha-1-aux, 68, 0, "", 8, 0, 0).

                       if i-qtd-linha > 1 then do:

                          run pi-cria-dados ("  ______ ______ ______ ______ ______ ______", 45, 1, "", 8, 0, 0).

                          assign i-aux = i-aux + 1.

                       end.

                       run pi-soma-linha (1).

                    end.

                    run pi-soma-linha (1).

                 end.
                 else do:

                    run pi-cria-dados (v-linha-aux, 130, 2, "", 8, 0, 0).

                 end.

                 assign i-aux = i-aux + 1.

             end.
    
             if c-reg = "Erro1" then do:

                run pi-cria-dados (" *** N«O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).

             end.

             if c-reg = "Erro2" then do:

                run pi-cria-dados (" *** N«O FOI ENCONTRADO NIVEL DE INSPEÄAO *** ", 62, 1, "", 8, 0, 0).

             end.
          end.

          if can-find(first tt-exames) then do:

             run pi-soma-linha (2).

             run pi-cria-dados ("                                                          Operador Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                             Respons†vel Liberaá∆o Nro:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Data:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("                                                                  Hora:   ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

             run pi-soma-linha (2).

             run pi-cria-dados ("Produá∆o:  Qte. Aprovado:_______________          Qte. Rejeito Comum:_______________", 118, 2, "", 8, 0, 0).

             run pi-soma-linha (2).

             run pi-cria-dados ("Operaá∆o: Isento de material de outro lote     ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("Plano de controle: Apontamento e preenchimento ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("Produto: Identificaá∆o e armazenamento         ( )OK ( )NOK        Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

             run pi-soma-linha (2).

             run pi-cria-dados ("Qualidade:", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("Atributo:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).
             run pi-cria-dados ("Vari†vel:  Qte. Amostra:__________     Qte. Rejeito:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

             run pi-soma-linha (2).

             run pi-cria-dados ("Outros:                          Qte. Rejeito Comum:__________     Observaá∆o:___________________________________", 118, 2, "", 8, 0, 0).

             run pi-soma-linha (2).

             run pi-cria-dados ("( )Aprovado ( )Reprovado ( )N/A     Nr. RO:__________     Data:__________     Matr°cula:__________", 118, 2, "", 8, 0, 0).

             run pi-soma-linha (1).

             run pi-cria-dados (fill("Ó", 122), 122, 0, "", 8, 0, 0).

          end.
       end.
   end.

   run pi-soma-linha (2).

   run pi-cria-dados (fill("-", 122), 122, 0, "", 8, 0, 0).

   if can-find(first tt-ext-texto-cq) then do:

      run pi-soma-linha (1).

      run pi-cria-dados ("OBSERVAÄ«O:", 16, 2, "", 0, 0, 0).

      for each tt-ext-texto-cq where tt-ext-texto-cq.cd-texto <> "":

         run pi-cria-dados (" - ", 3, 0, "", 0, 0, 0).

         run pi-cria-dados (tt-ext-texto-cq.cd-texto, 2, 1, "", 0, 0, 0).

         empty temp-table tt-editor.
        
         run pi-print-editor (tt-ext-texto-cq.descricao, 90).
        
         for each tt-editor:

            run pi-cria-dados ("   ", 3, 0, "", 0, 0, 0).

            run pi-cria-dados (tt-editor.conteudo, 90, 1, "", 0, 0, 0).

         end.

         run pi-soma-linha (1).
        
      end.

      empty temp-table tt-ext-texto-cq.

   end.

end procedure.


procedure buscar-exames:

   def input  parameter i-nr-ord-produ like oper-ord.nr-ord-produ no-undo.
   def input  parameter c-it-codigo    like oper-ord.it-codigo    no-undo.
   def input  parameter c-cod-roteiro  like oper-ord.cod-roteiro  no-undo.
   def input  parameter i-op-codigo    like oper-ord.op-codigo    no-undo.
   def input  parameter de-qt-item     like ord-prod.qt-ordem     no-undo.
   def output parameter table for tt-exames.

   DEF VAR c-it-aux AS CHAR NO-UNDO.

   c-it-aux = c-it-codigo.

   EMPTY TEMP-TABLE tt-exames.
   
   if i-op-codigo <> 0 then do:

      if not tt-param.l-imp-por-item and
         can-find(first esp-oper-exam where esp-oper-exam.nr-ord-produ = i-nr-ord-produ and
                                            esp-oper-exam.it-codigo    = c-it-codigo    and
                                            esp-oper-exam.cod-roteiro  = c-cod-roteiro  and
                                            esp-oper-exam.op-codigo    = i-op-codigo    no-lock) then do:

         for each esp-oper-exam where esp-oper-exam.nr-ord-produ = i-nr-ord-produ and
                                      esp-oper-exam.it-codigo    = c-it-codigo    and
                                      esp-oper-exam.cod-roteiro  = c-cod-roteiro  and
                                      esp-oper-exam.op-codigo    = i-op-codigo    no-lock:

            for each comp-exame where comp-exame.cod-exam = esp-oper-exam.cod-exame no-lock:

               create tt-exames.

               tt-exames.it-codigo  = esp-oper-exam.it-codigo.
               tt-exames.op-codigo  = esp-oper-exam.op-codigo.
               tt-exames.cod-exame  = esp-oper-exam.cod-exame.
               tt-exames.cod-comp   = comp-exame.cod-comp.
               tt-exames.rowid-comp = rowid(comp-exame).

               IF NOT tt-param.l-ord-amostra THEN

                  ASSIGN tt-exames.classifica = string(comp-exame.cod-comp, "9999999999").

               ELSE DO:

                  RUN pi-busca-amostragem (INPUT  c-it-aux,
                                           INPUT  de-qt-item,
                                           OUTPUT tt-exames.classifica).

               END.
            end.
         end.
      end.
      else do:

         if c-cod-roteiro <> "" and
            not can-find(first oper-exam where oper-exam.it-codigo   = c-it-codigo   and 
                                               oper-exam.cod-roteiro = c-cod-roteiro and
                                               oper-exam.op-codigo   = i-op-codigo   no-lock) then do:

            c-it-codigo = "".

         end.

         for each oper-exam where oper-exam.it-codigo   = c-it-codigo   and
                                  oper-exam.cod-roteiro = c-cod-roteiro and
                                  oper-exam.op-codigo   = i-op-codigo   no-lock:

            for each comp-exame where comp-exame.cod-exam = oper-exam.cod-exame no-lock:

               create tt-exames.

               tt-exames.it-codigo  = oper-exam.it-codigo.
               tt-exames.op-codigo  = oper-exam.op-codigo.
               tt-exames.cod-exame  = oper-exam.cod-exame.
               tt-exames.cod-comp   = comp-exame.cod-comp.
               tt-exames.rowid-comp = rowid(comp-exame).

               IF NOT tt-param.l-ord-amostra THEN

                  ASSIGN tt-exames.classifica = string(comp-exame.cod-comp, "9999999999").

               ELSE DO:

                  RUN pi-busca-amostragem (INPUT  c-it-aux,
                                           INPUT  de-qt-item,
                                           OUTPUT tt-exames.classifica).

               END.
            end.
         end.
      end.
   end.
   else do:

      for each it-exame where it-exame.it-codigo = c-it-codigo no-lock:

         if not tt-param.l-imp-por-item and
            can-find(first esp-oper-exam where esp-oper-exam.nr-ord-produ = i-nr-ord-produ          and
                                               esp-oper-exam.it-codigo    = c-it-codigo             and
                                               esp-oper-exam.cod-roteiro  = c-cod-roteiro           and
                                               esp-oper-exam.op-codigo    = esp-oper-exam.op-codigo and
                                               esp-oper-exam.cod-exame    = it-exame.cod-exame      no-lock) then next.

         if can-find(first oper-exam where oper-exam.it-codigo   = c-it-codigo         and 
                                           oper-exam.cod-roteiro = c-cod-roteiro       and
                                           oper-exam.op-codigo   = oper-exam.op-codigo and
                                           oper-exam.cod-exame   = it-exame.cod-exame  no-lock) then next.

         for each comp-exame where comp-exame.cod-exam = it-exame.cod-exame no-lock:
      
            create tt-exames.
         
            tt-exames.it-codigo  = it-exame.it-codigo.
            tt-exames.cod-exame  = it-exame.cod-exame.
            tt-exames.cod-comp   = comp-exame.cod-comp.
            tt-exames.rowid-comp = rowid(comp-exame).

            IF NOT tt-param.l-ord-amostra THEN

               ASSIGN tt-exames.classifica = string(comp-exame.cod-comp, "9999999999").

            ELSE DO:

               RUN pi-busca-amostragem (INPUT  c-it-aux,
                                        INPUT  de-qt-item,
                                        OUTPUT tt-exames.classifica).

            END.
         end.
      end.
   end.     

end procedure.



procedure pi-busca-amostragem:

   DEF INPUT  PARAMETER c-it-codigo        AS CHAR    NO-UNDO.
   DEF INPUT  PARAMETER de-qt-item         AS DECIMAL NO-UNDO.
   DEF OUTPUT PARAMETER c-des-freq-med-aux AS CHAR    NO-UNDO.

   DEF VAR c-reg          AS CHAR NO-UNDO.
   DEF VAR c-reg-2        AS CHAR NO-UNDO.
   DEF VAR c-des-freq-med AS CHAR NO-UNDO.

   assign c-reg   = ""
          c-reg-2 = "".

   find first exame     where exame.cod-exame     = comp-exame.cod-exame no-lock no-error.
   find first ext-exame where ext-exame.cod-exame = comp-exame.cod-exame no-lock no-error.

   if available exame     and
      available ext-exame then do:

      if ext-exame.log-um-ficha  then c-reg = string(ext-exame.cd-texto).
      if ext-exame.log-lote-fixo then c-reg = string(ext-exame.qtd-lote-fixo).

      if ext-exame.log-um-cada then 
         assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                c-reg-2 = string(ext-exame.qtd-um-cada).

      if c-reg = "" or exame.amostragem then do: /*Amostragem*/

         find first bf-item where bf-item.it-codigo = c-it-codigo no-lock no-error.

         find first esp-nivel-insp where esp-nivel-insp.cod-exame = comp-exame.cod-exame and 
                                         esp-nivel-insp.cod-comp  = comp-exame.cod-comp  no-lock no-error.

         if avail esp-nivel-insp then do:

            find first nivel-insp where nivel-insp.nivel     = esp-nivel-insp.nivel and 
                                        nivel-insp.tam-lote >= integer(de-qt-item)  no-lock no-error.

            if available nivel-insp then do:

               find first amostra where amostra.tipo-plano  = esp-nivel-insp.tipo-plano and 
                                        amostra.cod-amostra = nivel-insp.cod-amostra    no-lock no-error.

               if available amostra then do:

                  assign c-reg-2 = string(amostra.tam-amostra).                       

               end.
            end.
         end.
         else do:

            if available bf-item then do:

               find first nivel-insp where nivel-insp.nivel     = bf-item.nivel       and 
                                           nivel-insp.tam-lote >= integer(de-qt-item) no-lock no-error.

               if available nivel-insp then do:

                  find first amostra where amostra.tipo-plano  = 2                      and 
                                           amostra.cod-amostra = nivel-insp.cod-amostra no-lock no-error.

                  if available amostra then do:

                     assign c-reg-2 = string(amostra.tam-amostra).

                  end.
               end.
             end.
         end.
      end.

      find first ext-comp-exame where ext-comp-exame.cod-exame = comp-exame.cod-exame and 
                                      ext-comp-exame.cod-comp  = comp-exame.cod-comp  no-lock no-error.

      if available ext-comp-exame then
         assign c-des-freq-med = ext-comp-exame.des-freq-med.
      else
         assign c-des-freq-med = "".            

      if substring(c-des-freq-med,1,10) = "" or exame.amostragem  then
         assign c-des-freq-med-aux = c-reg-2.
      else
         assign c-des-freq-med-aux = substring(c-des-freq-med,1,10).

   end.

end procedure.



PROCEDURE quebrar-texto:

    DEF INPUT  PARAMETER c-texto   AS CHAR          NO-UNDO.
    DEF INPUT  PARAMETER i-tamanho AS INTEGER       NO-UNDO.
    DEF OUTPUT PARAMETER c-linhas  AS CHAR EXTENT 2 NO-UNDO.

    DEF VAR i-cont1   AS INTEGER NO-UNDO.
    DEF VAR i-cont2   AS INTEGER NO-UNDO.
    DEF VAR c-letra   AS CHAR    NO-UNDO.
    DEF VAR c-palavra AS CHAR    NO-UNDO.

    c-texto   = TRIM(c-texto).
    c-letra   = "".
    c-palavra = "".

    REPEAT i-cont1 = 1 TO LENGTH(c-texto):

       c-letra = SUBSTRING(c-texto, i-cont1, 1).

       c-palavra = c-palavra + c-letra.

       IF c-letra = " "             OR 
          i-cont1 = length(c-texto) THEN DO:

           REPEAT i-cont2 = 1 TO 2:

               IF i-cont2 = 2 THEN DO:
                   
                   c-linhas[i-cont2] = c-linhas[i-cont2] + c-palavra.

               END.
               ELSE DO:

                   IF  c-linhas[i-cont2 + 1] = "" AND
                       (length(c-linhas[i-cont2]) + LENGTH(c-palavra)) < i-tamanho THEN DO:

                       c-linhas[i-cont2] = c-linhas[i-cont2] + c-palavra.

                       LEAVE.

                   END.
               END.
           END.

           c-palavra = "".
           
       END.
    END.

END PROCEDURE.



procedure pi-imprime-texto:

   def input parameter c-texto         as char    no-undo.
   def input parameter i-qt-caracteres as integer no-undo.
   def input parameter i-posicao   as integer no-undo.
   def input parameter c-fonte         as char    no-undo.
   def input parameter i-tam-fonte     as integer no-undo.
   def input parameter i-cor-fonte     as integer no-undo.

   if i-qt-caracteres = 0  then i-qt-caracteres = length(c-texto).
   if c-fonte         = "" then c-fonte         = defaultFontName.
   if i-tam-fonte     = 0  then i-tam-fonte     = defaultFontSize.

   if length(c-texto) > i-qt-caracteres then

      assign c-texto = substring(c-texto, 1, i-qt-caracteres).
   
   else if length(c-texto) < i-qt-caracteres then do:

      assign c-texto = c-texto + fill(" ", i-qt-caracteres - length(c-texto)).

   end.

   run pdf_set_font ("Spdf", c-fonte, i-tam-fonte).

   case i-cor-fonte:
      when 0 then run pdf_text_color ("Spdf", 0.0, 0.0, 0.0). /* Preto */
      when 1 then run pdf_text_color ("Spdf", 1.0, 0.0, 0.0). /* Vermelho */
      when 2 then run pdf_text_color ("Spdf", 0.0, 1.0, 0.0). /* Green */
      when 3 then run pdf_text_color ("Spdf", 0.0, 0.0, 1.0). /* Azul */
   end case.

   if i-posicao <> 0 then
      run pdf_text_at ("Spdf", c-texto, i-posicao).
   else
      run pdf_text ("Spdf", c-texto).

   run pdf_text_color ("Spdf", 0.0, 0.0, 0.0).

   run pdf_set_font ("Spdf", defaultFontName, defaultFontSize).

end.



function PosTextRight returns character (ctexto as character, itamanho as integer):

   if length(ctexto) > itamanho then do:
      assign ctexto = substring(ctexto, 1, itamanho).
   end.
   else if length(ctexto) < itamanho then do:
      assign ctexto = fill(" ", itamanho - length(ctexto)) + ctexto.
   end.

   return ctexto.

end.



function getAcrobatCommandLine returns character ().

   def var cCmdLine as character.

   load "SOFTWARE" base-key "HKEY_LOCAL_MACHINE".

   use  "SOFTWARE".

   get-key-value section "Microsoft\Windows\CurrentVersion\App Paths\AcroRd32.exe"
                 key     default
                 value   cCmdLine.

   unload "SOFTWARE".

   return cCmdLine.

end function.



procedure WinExec external "KERNEL32.DLL":
   def input  param ProgramName as character.
   def input  param VisualStyle as long.
   def return param StatusCode  as long.
end procedure.

PROCEDURE pi-load-images:
   if can-find(first tt-pdf-desenhos where tt-pdf-desenhos.nr-pdf = tt-pdf.nr-pdf no-lock) then do:
      for each tt-pdf-desenhos where tt-pdf-desenhos.nr-pdf = tt-pdf.nr-pdf no-lock:
         ASSIGN cDeCodigo = REPLACE(tt-pdf-desenhos.deCodigo, ' ':U, '_').
         
         ASSIGN cItemDir  = /*tt-pdf-desenhos.nomDirDesenhos*/ session:temp-dir + "desenhos\" + cDeCodigo.         

         ASSIGN cDeCodigo1 = cDeCodigo.

         FIND FIRST ttDesenhosCarregados WHERE ttDesenhosCarregados.deCodigo MATCHES cDeCodigo1 + "__*":U NO-ERROR.
         IF NOT AVAILABLE ttDesenhosCarregados THEN DO:
            OS-CREATE-DIR VALUE(cItemDir).

            /* Faz o download do desenho do Vault */
            FILE-INFO:FILE-NAME = "vaultsrv.exe":U.
            IF FILE-INFO:FULL-PATHNAME = ? THEN
                FILE-INFO:FILE-NAME = "esp/vaultsrv.exe":U.
            
            assign cCmd = "u:".
            
            OS-COMMAND SILENT VALUE(cCmd).
            
            assign cCmd = "mkdir desenhos".
            
            OS-COMMAND SILENT VALUE(cCmd).

            ASSIGN cCmd = FILE-INFO:FULL-PATHNAME + " -d ~"":U + session:temp-dir + "desenhos" /*tt-pdf-desenhos.nomDirDesenhos*/ /*SUBSTRING(SESSION:TEMP-DIRECTORY, 1, LENGTH(SESSION:TEMP-DIRECTORY) - 1)*/ /*cItemDir*/ + "~""
                + " -i 10":U
                + " -u ~"":U + tt-pdf-desenhos.nomUsuarioVault + "~""
                + " -p ~"":U + tt-pdf-desenhos.nomSenhaVault + "~""
                + " -h ~"":U + tt-pdf-desenhos.nomServidorVault + "~""
                + " -v ~"":U + tt-pdf-desenhos.nomBaseVault + "~""
                + " -ecmlogin ~"":U + tt-pdf-desenhos.ecmLogin + "~""
                + " -ecmuser ~"":U + tt-pdf-desenhos.ecmMatricula + "~""
                + " -ecmpassword ~"":U + tt-pdf-desenhos.ecmPassword + "~""
                + " -ecmurl ~"":U + tt-pdf-desenhos.ecmURL + "~""
                + " -ecmcompany ~"":U + STRING(tt-pdf-desenhos.ecmCompany) + "~""
                + " -ecmrootfolder ~"":U + STRING(tt-pdf-desenhos.ecmRootFolder) + "~""
                + " -s 2":U
                /*+ " -cv":U*/
                + IF NOT l-gera-log THEN " -nolog" ELSE "":U
                + " ~"":U + tt-pdf-desenhos.deCodigo + "~"":U
                + " > ":U + SESSION:TEMP-DIRECTORY + cDeCodigo + ".log":U
                .

            ASSIGN iTempoGasto = ETIME(YES).
            
            OS-COMMAND SILENT VALUE(cCmd).
            
            ASSIGN iTempoGasto = ETIME(YES).
            ASSIGN dTempoGasto = iTempoGasto / 1000.

            IF SEARCH(cItemDir + "\":U + cDeCodigo + ".log":U) <> ? THEN DO:
                INPUT STREAM s1 FROM VALUE(cItemDir + "\":U + cDeCodigo + ".log":U).

                DEF VAR cDesenhosImprimir AS CHAR NO-UNDO.
                DEF VAR iDesenhoImprimir AS INT NO-UNDO.
                ASSIGN cDesenhosImprimir = "":U
                       iDesenhoImprimir = 0.
                IMPORT STREAM s1 cTmp.
                IF cTmp BEGINS "File=":U THEN DO:
                    ASSIGN tt-pdf-desenhos.nomUrl = cItemDir + "\":U + ENTRY(2, cTmp, "=":U).
                    REPEAT:
                        IMPORT STREAM s1 cTmp.
                        IF ENTRY(1, cTmp, "=") = "CheckedOut" THEN DO:
                            ASSIGN tt-pdf.bloqueado = (ENTRY(2, cTmp, "=") = "True":U).

                            ASSIGN tt-pdf-desenhos.bloqueado = (ENTRY(2, cTmp, "=") = "True":U).

                            /*TODO: Descomentar quando liberar a impressao do desenho de bloqueado
                            ASSIGN  tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
                                    tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1.*/
                        END.
                        ELSE DO:
                            IF tt-pdf.desenvolvimento THEN DO:
                                /* Quando for uma OP de desenvolvimento, somente os desenhos de desenvolvimento poder∆o ser impressos. */
                                IF ENTRY(2, cTmp, "=":U) = "DES" THEN DO:
                                    IF cDesenhosImprimir <> "":U THEN
                                        ASSIGN cDesenhosImprimir = cDesenhosImprimir + ",":U.
                                    ASSIGN cDesenhosImprimir = cDesenhosImprimir + ENTRY(1, cTmp, "=":U).
                                END.
                            END.
                            ELSE DO:
                                IF tt-param.l-anvisa THEN DO:
                                    IF ENTRY(2, cTmp, "=":U) = "ANVISA" THEN DO:
                                        IF cDesenhosImprimir <> "":U THEN
                                            ASSIGN cDesenhosImprimir = cDesenhosImprimir + ",":U.
                                        ASSIGN cDesenhosImprimir = cDesenhosImprimir + ENTRY(1, cTmp, "=":U).
                                    END.
                                END.
                                IF tt-param.l-fda THEN DO:
                                    IF ENTRY(2, cTmp, "=":U) = "FDA" THEN DO:
                                        IF cDesenhosImprimir <> "":U THEN
                                            ASSIGN cDesenhosImprimir = cDesenhosImprimir + ",":U.
                                        ASSIGN cDesenhosImprimir = cDesenhosImprimir + ENTRY(1, cTmp, "=":U).
                                    END.
                                END.
                                IF NOT tt-param.l-anvisa AND NOT tt-param.l-fda THEN DO:
                                    IF ENTRY(2, cTmp, "=":U) = "OP" OR ENTRY(2, cTmp, "=":U) = "PS" OR ENTRY(2, cTmp, "=":U) = "True" THEN DO:
                                        IF cDesenhosImprimir <> "":U THEN
                                            ASSIGN cDesenhosImprimir = cDesenhosImprimir + ",":U.
                                        ASSIGN cDesenhosImprimir = cDesenhosImprimir + ENTRY(1, cTmp, "=":U).
                                    END.
                                END.
                            END.
                        END.
                    END.
                END.
                INPUT STREAM s1 CLOSE.
                IF tt-pdf-desenhos.nomUrl <> ? AND tt-pdf-desenhos.nomUrl <> "":U AND NOT tt-pdf-desenhos.bloqueado THEN DO:

                   ASSIGN iTempoGasto = ETIME(YES).
                   ASSIGN dTempoGasto = iTempoGasto / 1000.

                   ASSIGN iDesenhoImprimir = 0.
                   INPUT STREAM sJpg FROM OS-DIR(cItemDir).
                   REPEAT:
                       IMPORT STREAM sJpg cTmp cJpgFile cFileType.
                       IF cFileType = "F":U AND SUBSTRING(cTmp, LENGTH(cTmp) - 3) = ".jpg" THEN DO:
                           ASSIGN iDesenhoImprimir = iDesenhoImprimir + 1.
                           IF LOOKUP(STRING(iDesenhoImprimir), cDesenhosImprimir, ",":U) > 0 THEN DO:
                               cDeCodigo1 = cDeCodigo + "__" + STRING(iDesenhoImprimir, "9999":U).
                               RUN pdf_load_image ("Spdf", cDeCodigo1, cJpgFile).
                               CREATE ttDesenhosCarregados.
                               ASSIGN ttDesenhosCarregados.deCodigo = cDeCodigo1.
                   
                               ASSIGN  tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
                                       tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1.
                           END.
                       END.
                   END.
                   INPUT STREAM sJpg CLOSE.
                END.
            END.
         END.
         ELSE DO:
             FOR EACH ttDesenhosCarregados WHERE ttDesenhosCarregados.deCodigo MATCHES cDeCodigo1 + "__*":U:
                 ASSIGN  tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
                         tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1.
             END.
         END.
      END.
   end.
END PROCEDURE.

PROCEDURE pi-cria-hist-ord-prod:

    DEF VAR i-seq AS INT NO-UNDO.

    FIND LAST hist-ord-prod NO-LOCK
        WHERE hist-ord-prod.nr-ord-produ = ord-prod.nr-ord-produ 
        USE-INDEX hstop_id NO-ERROR.
    IF AVAIL hist-ord-prod THEN DO:
        ASSIGN i-seq = hist-ord-prod.seq-hist + 1.
    END.
    ELSE
        ASSIGN i-seq = 1.

    CREATE hist-ord-prod.
    ASSIGN hist-ord-prod.nr-ord-prod = ord-prod.nr-ord-produ
           hist-ord-prod.seq-hist    = i-seq
           hist-ord-prod.dt-inicio   = ord-prod.dt-inicio
           hist-ord-prod.usuar-hist  = c-seg-usuario
           hist-ord-prod.dat-hist    = TODAY
           hist-ord-prod.hra-hist    = STRING (TIME, "HH:MM:SS")
           hist-ord-prod.motivo      = TRIM (tt-Param.c-motivo).

END.
