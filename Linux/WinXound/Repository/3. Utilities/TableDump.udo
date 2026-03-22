/*
TableDump - Prints a function table with formatting options.

DESCRIPTION
Prints the content of a function table. The indices being printed can be selected, the float precision and the number of values per line (up to 30).

SYNTAX
TableDump   ifn, istart, iend, iprec, ippr

INITIALIZATION
ifn 		- number of the function table
istart 	- first index which is being print
iend 	- last index which is being print (0 = up to the last index in the table)
iprec 	- precision of floats in the printout
ippr 	- number of parameters per row (maximum = 30)

CREDITS
joachim heintz 2008
*/

	opcode TableDump, 0, iiiii

ifn, istart, iend, iprec, ippr   xin

ilen		=	ftlen(ifn) ;length of the table (= last index + 1)
iend		=	(iend > ilen-1 || iend == 0 ? ilen-1 : iend)    ;set end to last index if 0 or larger than last index
		prints	"Printing index %d to %d of ftable %d:%n", istart, iend, ifn
	if istart > iend igoto error   ;break if the start index is larger than the end index

indx		=	istart
Sformat		sprintf	"%%.%df\t", iprec
istep		=	0
		
newline:
	if indx > iend igoto end
Sdump		sprintf	"Index %d ff:\t", istep*ippr+istart
istep		=	istep + 1
icount		=	0

loop:
ival		tab_i	indx, ifn
Snew		sprintf	Sformat, ival
Sdump		strcat	Sdump, Snew
indx		=	indx + 1
icount		=	icount + 1
imod		=	icount % ippr
	if imod == 0 then
Sout		strcat	Sdump, "%n"
		prints	Sout
		igoto	newline
	endif
	if indx <= iend igoto loop
Sout		strcat	Sdump, "%n"
		prints	Sout
		igoto	end
		
error: 		prints	"Error! Index istart is larger than index iend.%n"
end:
	endop

 
