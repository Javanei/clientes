def temp-table tt-param no-undo
   field destino          as integer
   field arquivo          as character format "x(35)"
   field usuario          as character format "x(12)"
   field data-exec        as date
   field hora-exec        as integer
   field classifica       as integer
   field desc-classifica  as character format "x(40)"
   field modelo-rtf       as character format "x(35)"
   field l-habilitaRtf    as log
   field c-planejador-ini as character format 'x(12)'
   field c-planejador-fim as character format 'x(12)'
   field i-ordem-ini      as integer   format '>>>,>>>,>>9'
   field i-ordem-fim      as integer   format '>>>,>>>,>>9'
   field i-linha-ini      as integer   format '>>9'
   field i-linha-fim      as integer   format '>>9'
   field d-inicio-ini     as date      format '99/99/9999'
   field d-inicio-fim     as date      format '99/99/9999'
   field d-emissao-ini    as date      format '99/99/9999'
   field d-emissao-fim    as date      format '99/99/9999'
   field l-narrativa      as logical
   field l-reimpressao    as logical
   field l-plano-controle as logical
   field l-imp-por-item   as logical
   field c-item-ini       as char
   field c-item-fim       as char
   FIELD l-somente-desenho AS LOGICAL INIT FALSE
   FIELD l-anvisa         AS LOGICAL INIT FALSE
   FIELD l-fda            AS LOGICAL INIT FALSE.


def temp-table tt-raw-digita
    field raw-digita       as raw.

define temp-table tt-digita no-undo
    field ordem            as integer   format ">>>>9"
    field exemplo          as character format "x(30)"
    index id ordem.
