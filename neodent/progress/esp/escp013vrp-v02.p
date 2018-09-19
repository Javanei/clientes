/**************************************************************************
**
**     Programa: ESPD013RP
**
**     Objetivo: Impress�o de Ordem de Produ��o
**
**************************************************************************/

{include/i-prgvrs.i ESCP003RP 2.06.00.004}
{include/i_fnctrad.i}

DEF STREAM slog.

/* The following Visual Style parameters define the initial state of the application's main window. */
&global-define SW-SHOWNORMAL      1 /* Start in a normal size window. */
&global-define SW-SHOWMINIMIZED   2 /* Start minimized. Show an icon at the bottom of the screen. */
&global-define SW-SHOWMAXIMIZED   3 /* Start in a maximized window. */
&global-define SW-SHOWNOACTIVATE  4 /* Start but set the focus back to the calling program. */
&global-define SW-SHOWMINNOACTIVE 7 /* Start minimized and set the focus back to the calling program. */

{esp/escp013vtt.i}

/* DEFini��o e prepara��o dos Par�metros  */

def input parameter raw-param as raw no-undo.
def input parameter table     for tt-raw-digita.

create tt-param.
raw-transfer raw-param to tt-param.

def new global shared var c-seg-usuario as character format "x(12)" no-undo.

def buffer bf-item          for item.
def buffer bf-aloca-reserva for aloca-reserva.

DEFINE STREAM s1.
DEFINE VARIABLE cTmp AS CHARACTER NO-UNDO.

{esp/esvl001.i}

def temp-table tt-ext-texto-cq no-undo like ext-texto-cq.

def temp-table tt-pdf-desenhos no-undo like ttDesenhoItem
   field nr-pdf       as integer
   field nr-sequencia as integer
   field nr-ord-produ as integer
   index tt1 nr-pdf nr-sequencia ascending.

def temp-table ttDesenhosCarregados no-undo
   field deCodigo like desenho-item.de-codigo.

def temp-table tt-pdf no-undo
   field nr-pdf          as integer
   field arquivo         as char
   field qt-paginas      as integer
   field qt-desenhos     as integer
   field desenvolvimento as logical
    FIELD bloqueado      AS LOGICAL
   field qt-pg-ord       as integer
   field qt-pg-insp      as integer
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
   field rowid-comp as rowid
   index tt1 it-codigo op-codigo cod-exame cod-comp ascending
   index tt2 cod-comp ascending.

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

{include/i-rpvar.i} /* Vari�veis para cabe�alho e rodap� */

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

run utp/ut-acomp.p persistent set h-acomp.

function getAcrobatCommandLine returns character () forward.

function PosTextRight returns character (ctexto as character, itamanho as integer) forward.

/* Defini��o da temp-table de erros */
{cdp/cd0666.i}

/*** Definicao de temp-table tt-Editor */
{include/tt-edit.i}
{include/pi-edit.i}

run pi-inicializar in h-acomp (input  Return-value ).

assign c-programa     = "ESCP013"
       c-versao       = "2.06"       
       c-revisao      = "00.006".

FIND FIRST param-global NO-LOCK NO-ERROR.

if AVAILABLE param-global THEN DO:

   FIND FIRST mguni.empresa WHERE empresa.ep-codigo = param-global.empresa-prin NO-LOCK NO-ERROR.

   IF AVAILABLE empresa THEN c-empresa = empresa.nome.

END.

{pdfinclude/pdf_inc.i "THIS-PROCEDURE"}

assign defaultFontSize     = 8
       defaultFontName     = "Courier"
       defaultFontBoldName = "Courier-Bold".

/* Montar tabela tempor�ria com a imagem da impress�o */

OUTPUT STREAM slog TO value(SESSION:TEMP-DIRECTORY + "escp013-" + STRING(TIME) + ".log").
RUN pi-log(INPUT '============== INICIO', INPUT FALSE).
RUN pi-log(INPUT 'Versao: 2011-03-02 13:58', INPUT TRUE).

DO  ON ERROR UNDO, LEAVE
    ON STOP UNDO, LEAVE:
if not tt-param.l-imp-por-item then do:
RUN pi-log(INPUT 'Iniciando impressao por ordem', INPUT FALSE).
   for each ord-prod exclusive-lock use-index ch-emite-op where
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
            ord-prod.dt-inicio    <= tt-param.d-inicio-fim     and
           (ord-prod.emite-ordem and not tt-Param.l-reimpressao or
            not ord-prod.emite-ordem and tt-Param.l-reimpressao):

      run pi-acompanhar in h-acomp (input ord-prod.nr-ord-produ).   

      if ord-prod.it-codigo < tt-param.c-item-ini or
         ord-prod.it-codigo > tt-param.c-item-fim then next.

      IF NOT tt-param.l-somente-desenho THEN DO:
          find first ord-manut where ord-manut.nr-ord-produ = ord-prod.nr-ord-prod no-lock no-error.

          if available ord-manut then next.

          /*N�o considera modulo de Frotas*/
          if ord-prod.origem = "MV" then next.

          find first planejad where planejad.cd-planejado = ord-prod.cd-planejado no-lock no-error.

          if not tt-Param.l-Reimpressao then ord-prod.emite-ordem = no.
      END.

      find first item where item.it-codigo = ord-prod.it-codigo no-lock no-error.    
RUN pi-log(INPUT 'processando ordem: ' + STRING(ord-prod.nr-ord-produ) + ' - item: ' + STRING(ord-prod.it-codigo), INPUT TRUE).

      ASSIGN i-nr-pdf = i-nr-pdf + 1.

      create tt-pdf.
      ASSIGN tt-pdf.nr-pdf     = i-nr-pdf
             tt-pdf.arquivo    = substring(tt-param.arquivo, 1, r-index(tt-param.arquivo, "\")) + "OS" + string(ord-prod.nr-ord-produ) + ".pdf"
             tt-pdf.qt-paginas = 0.

      find first ext-ord-prod where ext-ord-prod.nr-ord-produ = ord-prod.nr-ord-produ no-lock no-error.

      IF NOT tt-param.l-somente-desenho THEN DO:
          if available ext-ord-prod and ext-ord-prod.deposito-desenv then do:
             ASSIGN tt-pdf.desenvolvimento = yes.
          end.
      END.

      IF NOT tt-param.l-somente-desenho THEN DO:
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

      IF NOT tt-param.l-somente-desenho THEN DO:
          for each reservas where reservas.nr-ord-pro = ord-prod.nr-ord-prod no-lock,
             first bf-item  where bf-item.it-codigo   = reservas.it-codigo   no-lock:

             find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                          tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-lock no-error.

             if l-primeira or (available tt-pdf-dados and (tt-pdf-dados.nr-linha + tt-pdf-dados.qt-salto) > 90) then do:

                run pi-cria-dados ("ESTRUTURA",                                        9, 1, "", 0, 0, 0).
                run pi-cria-dados (fill("-", 119),                                   119, 1, "", 0, 0, 0).
                run pi-cria-dados ("COD.",                                             9, 0, "", 0, 0, 0).
                run pi-cria-dados (" ",                                                1, 0, "", 0, 0, 0).
                run pi-cria-dados ("DESCRI��O",                                       57, 0, "", 0, 0, 0).
                run pi-cria-dados ("   QUANT. UM.  OPER.    DEP.               QTDE", 65, 1, "", 0, 0, 0).
                run pi-cria-dados (" ",                                               82, 0, "", 0, 0, 0).
                run pi-cria-dados ("BAIXA    BAIXA    LOTE      ALOCADA",             35, 1, "", 0, 0, 0).
                run pi-cria-dados (fill("-", 119),                                   119, 1, "", 0, 0, 0).

                ASSIGN l-primeira = no.

             end.

             run pi-cria-dados (reservas.it-codigo, 9, 0, "", 0, 0, 0).
             run pi-cria-dados (" ",                1, 0, "", 0, 0, 0).

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

                   run pi-cria-dados (" ",                 9, 0, "", 0, 0, 0).
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

                run pi-cria-dados (" ",                 9, 0, "", 0, 0, 0).
                run pi-cria-dados (" ",                 1, 0, "", 0, 0, 0).
                run pi-cria-dados (tt-editor.conteudo, 30, 1, "", 0, 0, 0).

             end.
          end.

          run pi-soma-linha (1).

          l-primeira = yes.
      END.
      
      IF NOT tt-param.l-somente-desenho THEN DO:
          for each oper-ord where oper-ord.emite-ficha                          and
                                  oper-ord.nr-ord-produ = ord-prod.nr-ord-produ no-lock
                               by oper-ord.op-codigo:    

              run pi-soma-linha (1).

              find last tt-pdf-dados where tt-pdf-dados.nr-pdf    = tt-pdf.nr-pdf     and
                                           tt-pdf-dados.nr-pagina = tt-pdf.qt-paginas no-lock no-error.

              if l-primeira or (available tt-pdf-dados and (tt-pdf-dados.nr-linha + tt-pdf-dados.qt-salto) > 90) then do:

                 run pi-cria-dados (" ",                    50, 0, "", 0, 0, 0).
                 run pi-cria-dados ("ROTEIRO DE PRODUCAO",  19, 1, "", 0, 0, 0).
                 run pi-cria-dados ("  SEQUENCIA ",         35, 0, "", 8, 0, 0).
                 run pi-cria-dados ("DENOMINA��O",          49, 0, "", 8, 0, 0).
                 run pi-cria-dados ("GRUPO M�QUINA ",       20, 1, "", 8, 0, 0).
                 run pi-cria-dados (fill("-", 119),        119, 3, "", 8, 0, 0).

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

                run pi-print-editor (ficha-oper.desc-linha, 119).

             end.

             for each tt-Editor:

                run pi-cria-dados (tt-Editor.Conteudo, 119, 1, "", 8, 0, 0).

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

             do i-cont = i-linha-aux to 6:

                run pi-soma-linha (1).

             end.
          end.

          if tt-param.l-plano-controle then do:
    
             run pi-gera-cabecalho-inspecao (input yes).
    
             run pi-gera-dados-inspecao.
    
          end.
      END.

      /* Busca dos desenhos. */
RUN pi-log(INPUT 'Vai buscar os desenhos', INPUT TRUE).
      RUN esp/esvl002.p (INPUT ord-prod.it-codigo, OUTPUT TABLE ttDesenhoitem).
RUN pi-log(INPUT 'Buscou desenhos. Retorno: ' + RETURN-VALUE, INPUT TRUE).

      IF RETURN-VALUE = "OK":U THEN DO:
         ASSIGN i-nr-sequencia = 0.

         FOR EACH ttDesenhoItem:
            ASSIGN i-nr-sequencia = i-nr-sequencia + 1.
RUN pi-log(INPUT '    Desenho: ' + ttDesenhoItem.deCodigo, INPUT TRUE).

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
RUN pi-log (INPUT 'Acabou a leitura', INPUT TRUE).
end.

if tt-param.l-imp-por-item then do:
   for each item where item.it-codigo >= tt-param.c-item-ini and
                       item.it-codigo <= tt-param.c-item-fim no-lock:
RUN pi-log(INPUT 'Iniciando a impressao por item', INPUT FALSE).

      run pi-acompanhar in h-acomp (input item.it-codigo).

      ASSIGN i-nr-pdf = i-nr-pdf + 1.

      create tt-pdf.
      ASSIGN tt-pdf.nr-pdf     = i-nr-pdf
             tt-pdf.arquivo    = substring(tt-param.arquivo, 1, r-index(tt-param.arquivo, "\")) + "IT" + string(item.it-codigo) + ".pdf"
             tt-pdf.qt-paginas = 0.

      IF NOT tt-param.l-somente-desenho THEN DO:
          run pi-gera-cabecalho-item (input yes).
          run pi-gera-dados-item.
      END.

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

run pi-acompanhar in h-acomp (input "Aguarde, gerando arquivo PDF").   

RUN pi-log (INPUT 'Arquivo destino: ' + tt-param.arquivo, INPUT TRUE).
run pdf_new ("Spdf", tt-param.arquivo).

file-info:file-name = "Code39.ttf".
run pdf_load_font ("Spdf", "Code39", file-info:full-pathname, "code39.afm","").

file-info:file-name = "IDAutomationHC39M.ttf".
run pdf_load_font ("Spdf", "BarcodeFont39", file-info:full-pathname, "BarcodeFont39.afm","").

RUN pi-log (INPUT 'vai carregar a imagem de desenvolvimento: ' + search("desenvolvimento.jpg"), INPUT TRUE).
run pdf_load_image ("Spdf", "Desenvolvimento", search("desenvolvimento.jpg")).
RUN pi-log (INPUT 'vai carregar a imagem de desenvolvimento: ' + search("desenhobloqueado.jpg"), INPUT TRUE).
run pdf_load_image ("Spdf", "Bloqueado", search("desenhobloqueado.jpg")).
RUN pi-log (INPUT 'Carregou as imagens padroes', INPUT TRUE).

if not can-find(first tt-pdf no-lock) then do:
   run pdf_new_page("Spdf").
end.

empty temp-table ttDesenhosCarregados.

for each tt-pdf no-lock:
   run pdf_set_PaperType ("Spdf", "A4").
   run pdf_set_Orientation ("Spdf", "Portrait").
   run pdf_set_LeftMargin ("Spdf", 10).

   if tt-pdf.desenvolvimento then do:
      run pdf_place_image ("Spdf", "Desenvolvimento", 50, 650, 500, 500).
   end.
   ELSE IF tt-pdf.bloqueado THEN DO:
       run pdf_place_image ("Spdf", "Bloqueado", 50, 650, 500, 500).
   END.

   for each tt-pdf-dados where tt-pdf-dados.nr-pdf = tt-pdf.nr-pdf no-lock 
                      break by tt-pdf-dados.nr-pagina
                            by tt-pdf-dados.nr-sequencia:

      if first-of(tt-pdf-dados.nr-pagina) then do:
         run pdf_set_Orientation ("Spdf","Portrait").
         run pdf_new_page ("Spdf").
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
         c-pagina = "P�gina " + string(tt-pdf-dados.nr-pagina) + " de " + string(tt-pdf.qt-paginas).
         c-pagina = fill(" ", 17 - length(c-pagina)) + c-pagina.

         run pdf_text_xy ("Spdf", "Impresso em: " + string(TODAY,"99/99/9999") + " - " + string(TIME,"HH:MM"),  10, 40).
         run pdf_text_xy ("Spdf", c-pagina, 495, 40).
      end.
   end.

   if can-find(first tt-pdf-desenhos where tt-pdf-desenhos.nr-pdf = tt-pdf.nr-pdf no-lock) then do:
RUN pi-log (INPUT 'Desenho encontrado', INPUT TRUE).
      for each tt-pdf-desenhos where tt-pdf-desenhos.nr-pdf = tt-pdf.nr-pdf no-lock:
RUN pi-log (INPUT 'Processando desenho: ' + tt-pdf-desenhos.deCodigo, INPUT TRUE).
         ASSIGN cDeCodigo = REPLACE(tt-pdf-desenhos.deCodigo, ' ':U, '_').
         ASSIGN cItemDir  = tt-pdf-desenhos.nomDirDesenhos + "\" + cDeCodigo.
RUN pi-log (INPUT 'Codigo do desenho: ' + cDeCodigo, INPUT TRUE).
RUN pi-log (INPUT 'Diretorio temporario dos desenhos: ' + cItemDir, INPUT TRUE).

         ASSIGN cDeCodigo1 = cDeCodigo.

         FIND FIRST ttDesenhosCarregados WHERE ttDesenhosCarregados.deCodigo MATCHES cDeCodigo1 + "__*":U NO-ERROR.
         IF NOT AVAILABLE ttDesenhosCarregados THEN DO:
RUN pi-log (INPUT 'Desenho ainda nao carregado', INPUT TRUE).
            OS-CREATE-DIR VALUE(cItemDir).
DEF VAR iTempoGasto AS INTEGER NO-UNDO.
DEF VAR dTempoGasto AS DECIMAL NO-UNDO.

            /* Faz o download do desenho do Vault */
            FILE-INFO:FILE-NAME = "vaultsrv.exe":U.
            IF FILE-INFO:FULL-PATHNAME = ? THEN
                FILE-INFO:FILE-NAME = "esp/vaultsrv.exe":U.
RUN pi-log(INPUT 'Caminho do arquivo vaultsrv.exe: ' + IF FILE-INFO:FULL-PATHNAME <> ? THEN FILE-INFO:FULL-PATHNAME ELSE '?', INPUT TRUE).
            ASSIGN cCmd = FILE-INFO:FULL-PATHNAME + " -d ~"":U + tt-pdf-desenhos.nomDirDesenhos /*SUBSTRING(SESSION:TEMP-DIRECTORY, 1, LENGTH(SESSION:TEMP-DIRECTORY) - 1)*/ /*cItemDir*/ + "~""
                + " -i 10":U
                + " -u ~"":U + tt-pdf-desenhos.nomUsuarioVault + "~""
                + " -p ~"":U + tt-pdf-desenhos.nomSenhaVault + "~""
                + " -h ~"":U + tt-pdf-desenhos.nomServidorVault + "~""
                + " -v ~"":U + tt-pdf-desenhos.nomBaseVault + "~""
                + " -s 2":U
                + " -cv":U
                + " ~"":U + tt-pdf-desenhos.deCodigo + "~"":U
                /*+ " > ":U + cItemDir + "\":U + cDeCodigo + ".log":U*/
                + " > ":U + SESSION:TEMP-DIRECTORY + cDeCodigo + ".log":U
                .
RUN pi-log (INPUT 'Vai buscar o desenho no vault: ' + cCmd, INPUT TRUE).

ASSIGN iTempoGasto = ETIME(YES).
            OS-COMMAND SILENT VALUE(cCmd).
ASSIGN iTempoGasto = ETIME(YES).
ASSIGN dTempoGasto = iTempoGasto / 1000.
RUN pi-log (INPUT 'Tempo gasto no vault/download: ' + STRING(dTempoGasto) + "seg", INPUT TRUE).
            RUN pi-log (INPUT 'Arquivo de retorno: ' + SEARCH(cItemDir + "\":U + cDeCodigo + ".log":U), INPUT TRUE).
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
RUN pi-log (INPUT 'Leu a linha: ' + cTmp, INPUT TRUE).
                        IF ENTRY(1, cTmp, "=") = "CheckedOut" THEN DO:
                            ASSIGN tt-pdf.bloqueado = (ENTRY(2, cTmp, "=") = "True":U).
                        END.
                        ELSE DO:
                            IF ENTRY(2, cTmp, "=":U) = "True" THEN DO:
                                IF cDesenhosImprimir <> "":U THEN
                                    ASSIGN cDesenhosImprimir = cDesenhosImprimir + ",":U.
                                ASSIGN cDesenhosImprimir = cDesenhosImprimir + ENTRY(1, cTmp, "=":U).
                            END.
                        END.
                    END.
RUN pi-log (INPUT 'Desenhos a imprimir: ' + cDesenhosImprimir, INPUT TRUE).
                END.
                INPUT STREAM s1 CLOSE.
                RUN pi-log (INPUT 'URL do desenho: ' + tt-pdf-desenhos.nomUrl, INPUT TRUE).
                IF tt-pdf-desenhos.nomUrl <> ? AND tt-pdf-desenhos.nomUrl <> "":U AND NOT tt-pdf.bloqueado THEN DO:
                    /* Vai processar a imagem */
                    IF tt-pdf-desenhos.nomUrl MATCHES "*.dwf":U THEN DO:

RUN pi-log (INPUT '� uma imagem DWF', INPUT TRUE).
                        /* � uma imagem em autocad */
                        /*FILE-INFO:FILE-NAME = "dwgtoimage.exe".
                        IF FILE-INFO:FULL-PATHNAME = ? THEN
                            FILE-INFO:FILE-NAME = "esp/dwgtoimage.exe".
    RUN pi-log(INPUT 'Caminho do arquivo dwgtoimage.exe: ' + IF FILE-INFO:FULL-PATHNAME <> ? THEN FILE-INFO:FULL-PATHNAME ELSE '?', INPUT TRUE).

                        ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
                            + " -i ~"":U + tt-pdf-desenhos.nomUrl + "~"":U
                            + " -o ~"":U + cItemDir + "~"":U
                            + " -l ~"ALL~"":U
                            + " -t ~"jpeg~"":U
                            + " -q 100":U
                            .*/

                        /*FILE-INFO:FILE-NAME = "CADConverter.exe".
                        IF FILE-INFO:FULL-PATHNAME = ? THEN
                            FILE-INFO:FILE-NAME = "esp/CADConverter.exe".
    RUN pi-log(INPUT 'Caminho do arquivo CADConverter.exe: ' + IF FILE-INFO:FULL-PATHNAME <> ? THEN FILE-INFO:FULL-PATHNAME ELSE '?', INPUT TRUE).

                        ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
                            + " ~"":U + tt-pdf-desenhos.nomUrl + "~"":U
                            + " ~"":U + cItemDir + "~"":U
                            + " -c ~"jpg~"":U
                            + " -jq 100":U
                            + " -cm normal":U
                            + " -cl true":U
                            + " -jcs rgb":U
                            + " -s 3072x2179":U
                            + " -m f":U
                            .*/

                        /*FILE-INFO:FILE-NAME = "AcmeCADConverter.exe".
                        IF FILE-INFO:FULL-PATHNAME = ? THEN
                            FILE-INFO:FILE-NAME = "esp/AcmeCADConverter.exe".
    RUN pi-log(INPUT 'Caminho do arquivo AcmeCADConverter.exe: ' + IF FILE-INFO:FULL-PATHNAME <> ? THEN FILE-INFO:FULL-PATHNAME ELSE '?', INPUT TRUE).

                         ASSIGN cCmd = FILE-INFO:FULL-PATHNAME
                             + " /r" /* Command Line mode */
                             + " /e" /* Auto zoom extend */
                             + " /ls" /* Uses layout paper size if possible */
                             + " /m" /* Includes mask raster file */
                             + " /p 1" /* integer Indicate the raster pixel format, [1 -- 1_bit format, 2 -- gray format, 3 -- 256 colors], default is 3--256 colors */
                             + " /w 2048" /* integer Raster width, default is 800. It also supports the unit flag, such as (420mm = 420 mm, 11in = 11 inch), if you do not specify the unit flag, the unit will be pixels default. */
                             + " /h 1516" /* integer Indicate the raster height, default is 600, the unit flag is the same as the '/w' parameter. */
                             + " /res 300" /* integer the output quality, it is represented with DPI, it means how many pixels per inch. */
                             + " /ad" /* Auto detects and fits the current page size for the converted drawing. */
                             /*+ " /b"*/ /* integer Indicate background color index, [0-black, 1-red, 2-yellow, 3-green, 4-aqua, 5-blue, 6-Magenta, 7-White, 8-Dark Gray], the other colors please refer to the set color dialog. Default is 7-white */
                             + " /j 10" /* integer Indicate the jpeg quality, 1-lower, 10-highest, default is 10 */
                             + " /i" /* When you set this parameter and when you open the file and unable to find the �shx file�, it will no longer prompt the user, instead the name of such file will be written to a report file (if a report file needs to be so generated). */
                             + " /f 2" /* integer Raster file format: 1 -- bitmap format(*.bmp), 2 -- Jpeg format(*.jpg), 3 -- GIF format(*.gif), 4-- PCX format(*.pcx), etc. */
                             + " /key ~"INPO0MC7M0RJOKR6PBAQAL2AH7TAH9DLOJ9NZQTLJINALVYNF5IBLKC8K6OA~""
                             + " /a -2" /*Layout Index is a interger number, the follows is the meanings:
                                                0 : Always converts model space .
                                                1 : Converts the 1st layout.
                                                2 : Converts the 2nd layout, and so on.
                                                -1 :Converts the current layout.
                                                -2 :Converts all layouts.
                                                -3 :Converts all paper space layouts */
                             + " /d ~"":U + cItemDir + "~"":U /* The output folder or PDF filename */
                             + " ~"":U + tt-pdf-desenhos.nomUrl + "~"":U
                             .

    RUN pi-log (INPUT 'Vai converter para JPEG: ' + cCmd, INPUT TRUE).
    ASSIGN iTempoGasto = ETIME(YES).
    ASSIGN dTempoGasto = iTempoGasto / 1000.
    RUN pi-log (INPUT 'Tempo gasto no vault/download: ' + STRING(dTempoGasto) + "seg", INPUT TRUE).
                        OS-COMMAND SILENT VALUE(cCmd).
                        */
ASSIGN iTempoGasto = ETIME(YES).
ASSIGN dTempoGasto = iTempoGasto / 1000.
RUN pi-log (INPUT 'Tempo gasto convertendo desenho ' + tt-pdf-desenhos.nomUrl + " :" + STRING(dTempoGasto) + "seg", INPUT TRUE).

DEF STREAM sJpg.
DEF VAR cJpgFileTmp AS CHAR NO-UNDO.
DEF VAR cFileType AS CHAR NO-UNDO.
ASSIGN iDesenhoImprimir = 0.
INPUT STREAM sJpg FROM OS-DIR(cItemDir).
REPEAT:
    IMPORT STREAM sJpg cTmp cJpgFile cFileType.
RUN pi-log (INPUT 'Arquivo--->: ' + cJpgFile, INPUT TRUE).
    IF cFileType = "F":U AND SUBSTRING(cTmp, LENGTH(cTmp) - 3) = ".jpg" THEN DO:
        ASSIGN iDesenhoImprimir = iDesenhoImprimir + 1.
        IF LOOKUP(STRING(iDesenhoImprimir), cDesenhosImprimir, ",":U) > 0 THEN DO:
            cDeCodigo1 = cDeCodigo + "__" + STRING(iDesenhoImprimir, "9999":U).
RUN pi-log (INPUT 'Vai carregar a imagem: ' + cJpgFile, INPUT TRUE).
            ASSIGN cJpgFileTmp = SESSION:TEMP-DIRECTORY + STRING(TIME) + ".jpg".
            OS-COPY VALUE(cJpgFile) VALUE(cJpgFileTmp).
RUN pi-log (INPUT 'Copiou imagem: ' + cJpgFileTmp, INPUT TRUE).
            ASSIGN cJpgFile = cJpgFileTmp.
            RUN pdf_load_image ("Spdf", cDeCodigo1, cJpgFile).
RUN pi-log (INPUT 'Carregou a imagem: ' + cJpgFile, INPUT TRUE).
            CREATE ttDesenhosCarregados.
            ASSIGN ttDesenhosCarregados.deCodigo = cDeCodigo1.

            ASSIGN  tt-pdf.qt-desenhos           = tt-pdf.qt-desenhos + 1
                    tt-pdf.qt-paginas            = tt-pdf.qt-paginas  + 1.

        END.
    END.
END.
INPUT STREAM sJpg CLOSE.


                        /*ASSIGN cTmp = SUBSTRING(tt-pdf-desenhos.nomUrl, 1, LENGTH(tt-pdf-desenhos.nomUrl) - 4).
                        ASSIGN cJpgFile   = SEARCH(cTmp + ".jpg":U).
RUN pi-log (INPUT 'Arquivo JPEG principal: ' + cJpgFile, INPUT TRUE).
                        IF cJpgFile <> ? THEN DO:
                            ASSIGN iDesenhoImprimir = iDesenhoImprimir + 1.
                            IF LOOKUP(STRING(iDesenhoImprimir), cDesenhosImprimir, ",":U) > 0 OR NUM-ENTRIES(cDesenhosImprimir, ",":U) = 1 THEN DO:
                                cDeCodigo1 = cDeCodigo + "__" + STRING(0, "9999":U).
RUN pi-log (INPUT 'Vai carregar a imagem: ' + cJpgFile, INPUT TRUE).
ASSIGN cJpgFileTmp = SESSION:TEMP-DIRECTORY + STRING(TIME) + ".jpg".
OS-COPY VALUE(cJpgFile) VALUE(cJpgFileTmp).
RUN pi-log (INPUT 'Copiou imagem: ' + cJpgFileTmp, INPUT TRUE).
ASSIGN cJpgFile = cJpgFileTmp.
                               RUN pdf_load_image ("Spdf", cDeCodigo1, cJpgFile).
RUN pi-log (INPUT 'Carregou a imagem: ' + cJpgFile, INPUT TRUE).

                               CREATE ttDesenhosCarregados.
                               ASSIGN ttDesenhosCarregados.deCodigo = cDeCodigo1.
                            END.
                        END.

                        DO iCount = 1 TO 20:
                           ASSIGN cJpgFile   = SEARCH(cTmp + "_Layout":U + STRING(iCount) + ".jpg":U).
                           IF cJpgFile = ? THEN
                               ASSIGN cJpgFile   = SEARCH(cTmp + "-Layout":U + STRING(iCount) + ".jpg":U).
                           ASSIGN cDeCodigo1 = cDeCodigo + "__" + STRING(iCount).
RUN pi-log (INPUT 'Imagem de layout' + STRING(iCount) + ': ' + cJpgFile, INPUT TRUE).

                           IF cJpgFile <> ? THEN DO:
                               ASSIGN iDesenhoImprimir = iDesenhoImprimir + 1.
                               IF LOOKUP(STRING(iDesenhoImprimir), cDesenhosImprimir, ",":U) > 0 THEN DO:
RUN pi-log (INPUT 'Vai carregar a imagem: ' + cJpgFile, INPUT TRUE).
ASSIGN cJpgFileTmp = SESSION:TEMP-DIRECTORY + STRING(TIME) + ".jpg".
OS-COPY VALUE(cJpgFile) VALUE(cJpgFileTmp).
RUN pi-log (INPUT 'Copiou imagem: ' + cJpgFileTmp, INPUT TRUE).
ASSIGN cJpgFile = cJpgFileTmp.
                                 RUN pdf_load_image ("Spdf", cDeCodigo1, cJpgFile).
RUN pi-log (INPUT 'Carregou a imagem: ' + cJpgFile, INPUT TRUE).

                                 CREATE ttDesenhosCarregados.
                                 ASSIGN ttDesenhosCarregados.deCodigo = cDeCodigo1.
                               END.
                           END.
                        END.*/
                    END.
                    ELSE IF tt-pdf-desenhos.nomUrl MATCHES "*.pdf":U THEN DO:
                        /* � uma imagem em PDF */
                        /*FILE-INFO:FILE-NAME = "p2iagent.exe".

                        cCmd = FILE-INFO:FULL-PATHNAME + " --dest=~"" + cItemDir + "~""
                                                       + " --format=1"
                                                       + " --src=~"" + tt-pdf-desenhos.nomUrl + "~"".

                        OS-COMMAND SILENT VALUE(cCmd).*/

                        DO iCount = 1 TO 20:
                           cJpgFile   = SEARCH(cItemDir + "/page" + STRING(iCount, "9999":U) + ".jpg":U).
                           cDeCodigo1 = cDeCodigo + "__" + STRING(iCount, "9999":U).

                           IF cJpgFile = ? THEN LEAVE.
ASSIGN cJpgFileTmp = SESSION:TEMP-DIRECTORY + STRING(TIME) + ".jpg".
OS-COPY VALUE(cJpgFile) VALUE(cJpgFileTmp).
RUN pi-log (INPUT 'Copiou imagem: ' + cJpgFileTmp, INPUT TRUE).
ASSIGN cJpgFile = cJpgFileTmp.

                           run pdf_load_image ("Spdf", cDeCodigo1, cJpgFile).

                           CREATE ttDesenhosCarregados.
                           ASSIGN ttDesenhosCarregados.deCodigo = cDeCodigo1.
                        END.
                    END.
                END.
            END.
            ELSE DO:
                RUN pi-log (INPUT 'Nao achou o arquivo', INPUT TRUE).
            END.

         END.

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
               RUN pdf_text_xy ("Spdf","ORDEM DE PRODU��O: " + STRING(tt-pdf-desenhos.nr-ord-produ), 10, 585).
            end.

            c-pagina = "P�gina " + string((tt-pdf.qt-paginas - tt-pdf.qt-desenhos) + tt-pdf-desenhos.nr-sequencia) + " de " + string(tt-pdf.qt-paginas).
            c-pagina = fill(" ", 17 - length(c-pagina)) + c-pagina.

            run pdf_text_xy ("Spdf", "Impresso em: " + string(TODAY,"99/99/9999") + " - " + string(TIME,"HH:MM"),  10, 4).
               
            if pdf_Orientation("Spdf") = "Portrait" then
               run pdf_text_xy ("Spdf", c-pagina, 495, 4).
            else
               run pdf_text_xy ("Spdf", c-pagina, 740, 4).

            OS-DELETE VALUE(cCmd) RECURSIVE.

         end.
      END.
   end.

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
OUTPUT STREAM slog CLOSE.
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
      run pi-cria-dados ("ORDEM DE PRODU��O",                              17, 1, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados (" ",                                              97, 0, "",                   0, 0, 0).
      run pi-cria-dados ("*" + string(ord-prod.nr-ord-produ) + "*",         0, 1, "BarcodeFont39",     10, 0, 0).
      run pi-cria-dados (" ",                                             119, 1, "",                   0, 0, 0).
      run pi-cria-dados ("ITEM: ",                                          6, 0, "",                   0, 0, 0).
      run pi-cria-dados (ord-prod.it-codigo + " - " + item.desc-item,      83, 2, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados ("U.M.: ",                                          6, 0, "",                   0, 0, 0).
      run pi-cria-dados (item.un,                                          50, 0, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados ("QUANTIDADE PLANEJADA: ",                         22, 0, "",                   0, 0, 0).
      run pi-cria-dados (PosTextRight(string(ord-prod.qt-ordem), 11),       0, 1, defaultFontBoldName, 12, 0, 0).
      run pi-cria-dados (fill("-", 119),                                  119, 1, "",                   0, 0, 0).
   end.
   else do:
      run pi-cria-dados (" ",                                              85, 0, "",                   0, 0, 0).
      run pi-cria-dados ("Num. Ordem: ",                                   12, 3, "",                   0, 0, 0).
      run pi-cria-dados (" ",                                              97, 0, "",                   0, 0, 0).
      run pi-cria-dados ("*" + string(ord-prod.nr-ord-produ) + "*",         0, 2, "BarcodeFont39",     10, 0, 0).
      run pi-cria-dados (fill("-", 119),                                  119, 1, "",                   0, 0, 0).
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

   run pi-cria-dados (c-empresa,                                        85, 0, "",                   0, 0, 0).
   run pi-cria-dados ("Num. Ordem: ",                                   12, 1, "",                   0, 0, 0).
   run pi-cria-dados (c-Programa + " - " + c-versao + "." + c-revisao,  40, 1, "",                   0, 0, 0).
   run pi-cria-dados (" ",                                              40, 0, "",                   0, 0, 0).
   run pi-cria-dados ("PLANO DE CONTROLE DE INSPE��O",                  29, 1, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados (" ",                                              97, 0, "",                   0, 0, 0).
   run pi-cria-dados ("*" + string(ord-prod.nr-ord-produ) + "*",         0, 1, "BarcodeFont39",     10, 0, 0).
   run pi-cria-dados (" ",                                             119, 1, "",                   0, 0, 0).
   run pi-cria-dados ("ITEM: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (ord-prod.it-codigo + " - " + item.desc-item,      83, 2, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("U.M.: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (item.un,                                          50, 0, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("QUANTIDADE PLANEJADA: ",                         22, 0, "",                   0, 0, 0).
   run pi-cria-dados (PosTextRight(string(ord-prod.qt-ordem), 11),       0, 1, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados (fill("-", 119),                                  119, 1, "",                   0, 0, 0).

   if l-imp-revisao then do:
      /*Revis�o Plano de Inspe��o*/
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

      run pi-cria-dados ("Plano de Inspe��o: ", 19, 0, "", 0, 0, 0).
      run pi-cria-dados (c-desc-rev-plano-insp, 33, 0, "", 0, 0, 0).

      /*Inicio Validade*/
      run pi-cria-dados ("Data: ",                                 6, 0, "", 0, 0, 0).
      run pi-cria-dados (string(v-dat-ini-validade,"99/99/9999"), 10, 2, "", 0, 0, 0).

      ASSIGN c-nom-desenho = ""
             c-rev-desenho = ""
             c-dat-desenho = ""
             i-qt-caracter = 0. 

      /*Revis�o de Desenhos*/
      for each desenho-item where desenho-item.it-codigo = ord-prod.it-codigo no-lock:
         for last revisao where revisao.de-codigo = desenho-item.de-codigo no-lock:
            ASSIGN i-qt-caracter = maximum(10, length(revisao.de-codigo)).

            ASSIGN c-nom-desenho = "Desenho: " + revisao.de-codigo                          + fill(" ", i-qt-caracter - length(revisao.de-codigo)) + (if c-nom-desenho = "" then "" else " / " + c-nom-desenho)
                   c-rev-desenho = "Revis�o: " + revisao.rv-codigo                          + fill(" ", i-qt-caracter - length(revisao.rv-codigo)) + (if c-rev-desenho = "" then "" else " / " + c-rev-desenho)
                   c-dat-desenho = "Data:    " + string(revisao.data-revisao, "99/99/9999") + fill(" ", i-qt-caracter - 10)                        + (if c-dat-desenho = "" then "" else " / " + c-dat-desenho).
         end.
      end.

      run pi-cria-dados ("Revis�o de Desenhos: ",  22, 1, defaultFontBoldName, 0, 0, 0).
      run pi-cria-dados (c-nom-desenho,           119, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-rev-desenho,           119, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-dat-desenho,           119, 1, "",                  0, 0, 0).
      run pi-cria-dados (fill("-", 119),            0, 1, "",                  0, 0, 0).
   end.
   
   /*Dados da Opera��o - In�cio*/
   run pi-cria-dados ("C�digo/Nome Opera��o ", 22, 1, "", 0, 0, 0).
   run pi-cria-dados (" ",                      1, 0, "", 8, 0, 0).
   run pi-cria-dados ("Seq/Carac.Controlada",  33, 0, "", 8, 0, 0).
   run pi-cria-dados ("|Meio Controle",        16, 0, "", 8, 0, 0).
   run pi-cria-dados ("Amostragem",            11, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    8, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 1, "", 8, 0, 0).
   run pi-cria-dados ("",                      69, 0, "", 8, 0, 0).
   run pi-cria-dados ("01",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("02",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("03",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("04",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("05",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("06",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("07",                     7, 2, "", 8, 0, 0).
   run pi-cria-dados (fill("�", 119),           0, 1, "", 0, 0, 0).
END PROCEDURE.



procedure pi-gera-dados-inspecao:
   def var i-qtd-linha-max       as integer       no-undo.
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
       run buscar-exames (input        oper-ord.nr-ord-produ,
                          input        oper-ord.it-codigo,
                          input        oper-ord.cod-roteiro,
                          input        oper-ord.op-codigo,
                          output TABLE tt-exames).

       run pi-soma-linha (1).

       run pi-cria-dados (oper-ord.op-codigo,  9, 0, "", 0, 0, 0).
       run pi-cria-dados ("-",                 2, 0, "", 0, 0, 0).
       run pi-cria-dados (oper-ord.descricao, 34, 0, "", 0, 0, 0).

       if not can-find(first tt-exames) then do:
           run pi-cria-dados (" - Opera��o sem Caracter�sticas Controladas", 49, 0, "", 0, 0, 0).
       end.

       run pi-soma-linha (1).
       run pi-cria-dados (fill("�", 119), 119, 1, "", 0, 0, 0).
    
       ASSIGN i-qtd-linha-max = 1.
       
       for each tt-exames by tt-exames.cod-comp:
          assign c-reg   = ""
                 c-reg-2 = "".
              
          find first comp-exame where rowid(comp-exame) = rowid-comp no-lock no-error.

          find first ext-exame where ext-exame.cod-exame = tt-exames.cod-exame no-lock no-error.
          if available ext-exame then do:
             if ext-exame.log-um-ficha  then c-reg   = string(ext-exame.cd-texto).
             if ext-exame.log-lote-fixo then c-reg   = string(ext-exame.qtd-lote-fixo).
          
             if ext-exame.log-um-cada   then 
                assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                       c-reg-2 = string(ext-exame.qtd-um-cada).
    
             if c-reg = "" then do: /*Amostragem*/
                find first item where item.it-codigo = oper-ord.it-codigo no-lock no-error.
                if available item then do:
                   find first nivel-insp where nivel-insp.nivel     = item.nivel                 and 
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
    
          if c-reg-2 = "" then do:
             if v-log-char then
                assign i-qtd-linha = 1.
             else do:
                ASSIGN i-qtd-linha = truncate(decimal(c-reg), 0) + (if (decimal(c-reg) - truncate(decimal(c-reg), 0)) > 0 then 1 else 0).
              
                if i-qtd-linha <= 7 then
                   ASSIGN i-qtd-linha  = 1.
                else
                   ASSIGN i-qtd-linha  = truncate(i-qtd-linha / 7, 0) + (if (i-qtd-linha - truncate(i-qtd-linha / 7, 0)) > 0 then 1 else 0).
             end.
          end.
          else do:
             ASSIGN i-qtd-linha = integer(substring(c-reg,3)).
          
             ASSIGN i-qtd-linha = truncate(ord-prod.qt-ordem / i-qtd-linha, 0) + (if ((ord-prod.qt-ordem / i-qtd-linha) - truncate(ord-prod.qt-ordem / i-qtd-linha, 0)) > 0 then 1 else 0).
          
             if i-qtd-linha <= 7 then
                ASSIGN i-qtd-linha  = 1.
             else
                ASSIGN i-qtd-linha  = truncate(i-qtd-linha / 7, 0) + (if (i-qtd-linha - truncate(i-qtd-linha / 7, 0)) > 0 then 1 else 0).
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
       
          if substring(c-des-freq-med,1,10) = "" then
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
                             string(substring(c-des-freq-med-aux,1,10),"x(10)") + 
                             " " +
                             string(c-reg,"x(06)") + " " +
                             "______ ______ ______ ______ ______ ______ ______"
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
                               "  ______ ______ ______ ______ ______ ______ ______".
    
          assign i-aux = 1.
          do while i-aux <= i-qtd-linha:
             if i-aux = 1 then do:
                run pi-cria-dados (v-linha-1, 130, 1, "", 8, 0, 0).

                if v-linha-1-aux <> "" then do:
                   run pi-soma-linha (1).
                   run pi-cria-dados (v-linha-1-aux, 68, 0, "", 8, 0, 0).

                   if i-qtd-linha > 1 then do:
                      run pi-cria-dados (" ______ ______ ______ ______ ______ ______ ______",49, 1, "", 8, 0, 0).
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
             run pi-cria-dados (" *** N�O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).
          end.

          if c-reg = "Erro2" then do:
             run pi-cria-dados (" *** N�O FOI ENCONTRADO NIVEL DE INSPE�AO *** ", 62, 1, "", 8, 0, 0).
          end.
       end.

       if can-find(first tt-exames) then do:
          run pi-soma-linha (2).

          run pi-cria-dados ("                                                     Operador Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("                                        Respons�vel Libera��o Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("                                                             Data:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("                                                             Hora:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

          run pi-cria-dados (fill("�", 119), 119, 0, "", 0, 0, 0).

       end.

       if last-of (oper-ord.nr-ord-produ) then do:
          run buscar-exames (input        oper-ord.nr-ord-produ,
                             input        oper-ord.it-codigo,
                             input        oper-ord.cod-roteiro,
                             input        0,
                             output TABLE tt-exames).

          if can-find(first tt-exames) then do:
             run pi-soma-linha (1).
             run pi-cria-dados ("Caracter�sticas Controladas do Item", 35, 1, "", 0, 0, 0).
             run pi-cria-dados (fill("�", 119), 119, 1, "", 0, 0, 0).
          end.
    
          ASSIGN i-qtd-linha-max = 1.
       
          for each tt-exames by tt-exames.cod-comp:
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
    
                if c-reg = "" then do: /*Amostragem*/
                   find first item where item.it-codigo = oper-ord.it-codigo no-lock no-error.
                   if available item then do:
                      find first nivel-insp no-lock
                           where nivel-insp.nivel     = item.nivel
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
             
                i-qtd-linha = truncate(ord-prod.qt-ordem / i-qtd-linha, 0) + (if ((ord-prod.qt-ordem / i-qtd-linha) - truncate(ord-prod.qt-ordem / i-qtd-linha, 0)) > 0 then 1 else 0).
             
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
          
             if substring(c-des-freq-med,1,10) = "" then
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
                                string(c-reg,"x(06)") + 
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

               run pi-cria-dados (" *** N�O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).

            end.

            if c-reg = "Erro2" then do:

               run pi-cria-dados (" *** N�O FOI ENCONTRADO NIVEL DE INSPE�AO *** ", 62, 1, "", 8, 0, 0).

            end.
         end.

         if can-find(first tt-exames) then do:

            run pi-soma-linha (2).
       
            run pi-cria-dados ("                                                     Operador Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("                                        Respons�vel Libera��o Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("                                                             Data:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("                                                             Hora:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

            run pi-cria-dados (fill("�", 119), 119, 0, "", 8, 0, 0).

         end.
      end.
   end.

   run pi-soma-linha (2).

   run pi-cria-dados (fill("-", 119), 119, 0, "", 8, 0, 0).

   if can-find(first tt-ext-texto-cq) then do:

      run pi-soma-linha (1).

      run pi-cria-dados ("OBSERVA��O:", 16, 2, "", 0, 0, 0).

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

   run pi-cria-dados (c-empresa,                                        85, 1, "",                   0, 0, 0).
   run pi-cria-dados (c-Programa + " - " + c-versao + "." + c-revisao,  40, 1, "",                   0, 0, 0).
   run pi-cria-dados (" ",                                              40, 0, "",                   0, 0, 0).
   run pi-cria-dados ("PLANO DE CONTROLE DE INSPE��O POR ITEM",         38, 3, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("ITEM: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (item.it-codigo + " - " + item.desc-item,          83, 2, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados ("U.M.: ",                                          6, 0, "",                   0, 0, 0).
   run pi-cria-dados (item.un,                                          50, 1, defaultFontBoldName, 12, 0, 0).
   run pi-cria-dados (fill("-", 119),                                  119, 1, "",                   0, 0, 0).

   if l-imp-revisao then do:

      /*Revis�o Plano de Inspe��o*/
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

      run pi-cria-dados ("Plano de Inspe��o: ", 19, 0, "", 0, 0, 0).
      run pi-cria-dados (c-desc-rev-plano-insp, 33, 0, "", 0, 0, 0).

      /*Inicio Validade*/
      run pi-cria-dados ("Data: ",                                 6, 0, "", 0, 0, 0).
      run pi-cria-dados (string(v-dat-ini-validade,"99/99/9999"), 10, 2, "", 0, 0, 0).

      c-nom-desenho = "".
      c-rev-desenho = "".
      c-dat-desenho = "".
      i-qt-caracter = 0. 

      /*Revis�o de Desenhos*/
      for each desenho-item where desenho-item.it-codigo = item.it-codigo no-lock:

         for last revisao where revisao.de-codigo = desenho-item.de-codigo no-lock:

            i-qt-caracter = maximum(10, length(revisao.de-codigo)).

            c-nom-desenho = "Desenho: " + revisao.de-codigo                          + fill(" ", i-qt-caracter - length(revisao.de-codigo)) + (if c-nom-desenho = "" then "" else " / " + c-nom-desenho).
            c-rev-desenho = "Revis�o: " + revisao.rv-codigo                          + fill(" ", i-qt-caracter - length(revisao.rv-codigo)) + (if c-rev-desenho = "" then "" else " / " + c-rev-desenho).
            c-dat-desenho = "Data:    " + string(revisao.data-revisao, "99/99/9999") + fill(" ", i-qt-caracter - 10)                        + (if c-dat-desenho = "" then "" else " / " + c-dat-desenho).

         end.
      end.

      run pi-cria-dados ("Revis�o de Desenhos: ",  22, 1, defaultFontBoldName, 0, 0, 0).
      run pi-cria-dados (c-nom-desenho,           119, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-rev-desenho,           119, 1, "",                  0, 0, 0).
      run pi-cria-dados (c-dat-desenho,           119, 1, "",                  0, 0, 0).
      run pi-cria-dados (fill("-", 119),            0, 1, "",                  0, 0, 0).

   end.
   
   /*Dados da Opera��o - In�cio*/
   run pi-cria-dados ("C�digo/Nome Opera��o ", 22, 1, "", 0, 0, 0).
   run pi-cria-dados (" ",                      1, 0, "", 8, 0, 0).
   run pi-cria-dados ("Seq/Carac.Controlada",  33, 0, "", 8, 0, 0).
   run pi-cria-dados ("|Meio Controle",        16, 0, "", 8, 0, 0).
   run pi-cria-dados ("Amostragem",            11, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    8, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 0, "", 8, 0, 0).
   run pi-cria-dados ("Reg",                    7, 1, "", 8, 0, 0).
   run pi-cria-dados ("",                      69, 0, "", 8, 0, 0).
   run pi-cria-dados ("01",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("02",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("03",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("04",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("05",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("06",                     7, 0, "", 8, 0, 0).
   run pi-cria-dados ("07",                     7, 2, "", 8, 0, 0).
   run pi-cria-dados (fill("�", 119),           0, 1, "", 0, 0, 0).

END PROCEDURE.

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

   for each operacao where operacao.it-codigo = item.it-codigo no-lock
                  break by operacao.it-codigo
                        by operacao.op-codigo:
               
       run buscar-exames (input        0,
                          input        operacao.it-codigo,
                          input        operacao.cod-roteiro,
                          input        operacao.op-codigo,
                          output TABLE tt-exames).

       run pi-soma-linha (1).

       run pi-cria-dados (operacao.op-codigo,  9, 0, "", 0, 0, 0).
       run pi-cria-dados ("-",                 2, 0, "", 0, 0, 0).
       run pi-cria-dados (operacao.descricao, 34, 0, "", 0, 0, 0).

       if not can-find(first tt-exames) then do:
           run pi-cria-dados (" - Opera��o sem Caracter�sticas Controladas", 49, 0, "", 0, 0, 0).
       end.

       run pi-soma-linha (1).

       run pi-cria-dados (fill("�", 119), 119, 1, "", 0, 0, 0).
    
       i-qtd-linha-max = 1.
       
       for each tt-exames by tt-exames.cod-comp:
    
          assign c-reg   = ""
                 c-reg-2 = "".
              
          find first comp-exame where rowid(comp-exame) = rowid-comp no-lock no-error.

          find first ext-exame where ext-exame.cod-exame = tt-exames.cod-exame no-lock no-error.
            
          if available ext-exame then do:
       
             if ext-exame.log-um-ficha  then c-reg   = string(ext-exame.cd-texto).
             if ext-exame.log-lote-fixo then c-reg   = string(ext-exame.qtd-lote-fixo).
          
             if ext-exame.log-um-cada   then 
                assign c-reg   = "1/" + string(ext-exame.qtd-um-cada)
                       c-reg-2 = string(ext-exame.qtd-um-cada).
    
             if c-reg = "" then do: /*Amostragem*/
          
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

          find first ext-comp-exame where ext-comp-exame.cod-exame = tt-exames.cod-exame and 
                                          ext-comp-exame.cod-comp  = tt-exames.cod-comp  no-lock no-error.
              
          if available ext-comp-exame then
             assign c-des-freq-med = ext-comp-exame.des-freq-med.
          else
             assign c-des-freq-med = "".            
    
          c-cod-comp = string(tt-exames.cod-comp).
       
          if substring(c-des-freq-med,1,10) = "" then
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
                             string(substring(c-des-freq-med-aux,1,10),"x(10)") + 
                             " " +
                             string(c-reg,"x(06)") + " " +
                             "______ ______ ______ ______ ______ ______ ______"
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
                               "  ______ ______ ______ ______ ______ ______ ______".
    
          assign i-aux = 1.
    
          do while i-aux <= i-qtd-linha:
           
             if i-aux = 1 then do:
           
                run pi-cria-dados (v-linha-1, 130, 1, "", 8, 0, 0).

                if v-linha-1-aux <> "" then do:

                   run pi-soma-linha (1).

                   run pi-cria-dados (v-linha-1-aux, 68, 0, "", 8, 0, 0).

                   if i-qtd-linha > 1 then do:

                      run pi-cria-dados (" ______ ______ ______ ______ ______ ______ ______",49, 1, "", 8, 0, 0).

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

             run pi-cria-dados (" *** N�O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).

          end.

          if c-reg = "Erro2" then do:

             run pi-cria-dados (" *** N�O FOI ENCONTRADO NIVEL DE INSPE�AO *** ", 62, 1, "", 8, 0, 0).

          end.
       end.

       if can-find(first tt-exames) then do:

          run pi-soma-linha (2).

          run pi-cria-dados ("                                                     Operador Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("                                        Respons�vel Libera��o Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("                                                             Data:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
          run pi-cria-dados ("                                                             Hora:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

          run pi-cria-dados (fill("�", 119), 119, 0, "", 0, 0, 0).

       end.

       if last-of (operacao.it-codigo) then do:

          run buscar-exames (input        0,
                             input        operacao.it-codigo,
                             input        operacao.cod-roteiro,
                             input        0,
                             output TABLE tt-exames).

          if can-find(first tt-exames) then do:

             run pi-soma-linha (1).
       
             run pi-cria-dados ("Caracter�sticas Controladas do Item", 35, 1, "", 0, 0, 0).

             run pi-cria-dados (fill("�", 119), 119, 1, "", 0, 0, 0).

          end.
    
          i-qtd-linha-max = 1.
       
          for each tt-exames by tt-exames.cod-comp:
       
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
    
                if c-reg = "" then do: /*Amostragem*/
              
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
          
             if substring(c-des-freq-med,1,10) = "" then
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
                                string(c-reg,"x(06)") + 
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

               run pi-cria-dados (" *** N�O FOI ENCONTRADO TAMANHO DE AMOSTRAGEM *** ", 62, 1, "", 8, 0, 0).

            end.

            if c-reg = "Erro2" then do:

               run pi-cria-dados (" *** N�O FOI ENCONTRADO NIVEL DE INSPE�AO *** ", 62, 1, "", 8, 0, 0).

            end.
         end.

         if can-find(first tt-exames) then do:

            run pi-soma-linha (2).
       
            run pi-cria-dados ("                                                     Operador Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("                                        Respons�vel Libera��o Nro:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("                                                             Data:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).
            run pi-cria-dados ("                                                             Hora:   ______ ______ ______ ______ ______ ______ ______", 118, 2, "", 8, 0, 0).

            run pi-cria-dados (fill("�", 119), 119, 0, "", 8, 0, 0).

         end.
      end.
   end.

   run pi-soma-linha (2).

   run pi-cria-dados (fill("-", 119), 119, 0, "", 8, 0, 0).

   if can-find(first tt-ext-texto-cq) then do:

      run pi-soma-linha (1).

      run pi-cria-dados ("OBSERVA��O:", 16, 2, "", 0, 0, 0).

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
   def output parameter table for tt-exames.

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

            end.
         end.
      end.
      else do:

         for each oper-exam where oper-exam.it-codigo = c-it-codigo and 
                                  oper-exam.op-codigo = i-op-codigo no-lock:

            for each comp-exame where comp-exame.cod-exam = oper-exam.cod-exame no-lock:

               create tt-exames.

               tt-exames.it-codigo  = oper-exam.it-codigo.
               tt-exames.op-codigo  = oper-exam.op-codigo.
               tt-exames.cod-exame  = oper-exam.cod-exame.
               tt-exames.cod-comp   = comp-exame.cod-comp.
               tt-exames.rowid-comp = rowid(comp-exame).

            end.
         end.
      end.
   end.
   else do:

      for each it-exame where it-exame.it-codigo = c-it-codigo no-lock:

         for each comp-exame where comp-exame.cod-exam = it-exame.cod-exame no-lock:
      
            create tt-exames.
         
            tt-exames.it-codigo  = it-exame.it-codigo.
            tt-exames.cod-exame  = it-exame.cod-exame.
            tt-exames.cod-comp   = comp-exame.cod-comp.
            tt-exames.rowid-comp = rowid(comp-exame).
      
         end.
      end.
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

PROCEDURE pi-log:
    DEF INPUT PARAM msg AS CHAR NO-UNDO.
    DEF INPUT PARAM lappend AS LOGICAL NO-UNDO.

    /*IF lappend THEN
        OUTPUT STREAM slog TO "c:\temp\escp013.log" APPEND.
    ELSE
        OUTPUT STREAM slog TO "c:\temp\escp013.log".*/

    PUT STREAM slog UNFORMATTED "== " + ISO-DATE(NOW) + " == " + msg SKIP.
END PROCEDURE.