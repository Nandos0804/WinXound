/*
TableDumpSimp - prints the content of a table in a simple way

DESCRIPTION
prints the content of a table in a simple way. you may have to set the flag -+max_str_len=10000 for avoiding buffer overflow

SYNTAX
TableDumpSimp ifn, iprec, ippr

INITIALIZATION
ifn - function table number
iprec - float precision while printing (default = 3)
ippr - parameters per row (default = 10, maximum = 32)

CREDITS
joachim heintz 2010
*/

  opcode TableDumpSimp, 0, ijo
;prints the content of a table in a simple way
ifn, iprec, ippr   xin; function table, float precision while printing (default = 3), parameters per row (default = 10, maximum = 32)
iprec		=		(iprec == -1 ? 3 : iprec)
ippr		=		(ippr == 0 ? 10 : ippr)
iend		=		ftlen(ifn)
indx		=		0
Sformat	sprintf	"%%.%df\t", iprec
Sdump		=		""
loop:
ival		tab_i		indx, ifn
Snew		sprintf	Sformat, ival
Sdump		strcat		Sdump, Snew
indx		=		indx + 1
imod		=		indx % ippr
	if imod == 0 then
		puts		Sdump, 1
Sdump		=		""
	endif
	if indx < iend igoto loop
		puts		Sdump, 1
  endop

 
