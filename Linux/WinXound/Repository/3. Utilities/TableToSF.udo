/*
TableToSF - writes the content of a table to a soundfile

DESCRIPTION
writes the content of a table to a soundfile, with optional start and end point

SYNTAX
TableToSF ift, Soutname, ktrig [,iformat [,istart [,iend]]]

INITIALIZATION
ift - function table to write
Soutname - output file name in double quotes
iformat - output file format according to the fout manual page. if not specified or -1, the file is written with a wav header and 24 bit
istart - start in seconds in the function table to write (default=0)
iend - last point to write in the function table in seconds (default=-1: until the end)

PERFORMANCE
ktrig - if 1, the file is being written in one control-cycle. Make sure the trigger is 1 just for one k-cycle; otherwise the writing operation will be repeated again and again in each control cycle

CREDITS
joachim heintz july 2010
*/

 opcode TableToSF, 0, iSkjoj
ift, Soutname, ktrig, iformat, istart, iend xin
istrtsmps =         istart*sr
iendsmps  =         (iend == -1 ? ftlen(ift) : iend*sr)
 if iformat == -1 then
iformat   =         18
 endif
 if ktrig == 1 then
kcnt      init      istrtsmps
loop:
kcnt      =         kcnt+ksmps
andx      interp    kcnt-1
asig      tab       andx, giAudio
          fout      Soutname, iformat, asig
 if kcnt <= iendsmps-ksmps kgoto loop
 endif 
 endop
 
