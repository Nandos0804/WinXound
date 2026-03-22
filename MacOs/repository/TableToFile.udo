/*
TableToFile - Writes a function table to a file with formatting options.

DESCRIPTION
Writes the content of a function table to a file. The indices being written can be selected, the float precision and the number of values per line (up to 30).

SYNTAX
TableToFile   Sfilnam, ift, istart, iend, iprec, ippr

INITIALIZATION
Sfilnam	- filename in double quotes
ifn 	- number of the function table
istart 	- first index which is being written
iend 	- last index which is being written (0 = up to the last index in the table)
iprec 	- precision of floats
ippr 	- number of parameters per line (maximum = 30)

CREDITS
joachim heintz 2008
*/

	opcode TableToFile, 0, Siiiii

Sfilnam, ifn, istart, iend, iprec, ippr   xin

ilen		=	ftlen(ifn) ;length of the table (= last index + 1)
iend		=	(iend > ilen-1 || iend == 0 ? ilen-1 : iend) ;set end to last index if 0 or larger than last index
Shello		sprintf	"Writing index %d to %d of ftable %d to file %s", istart, iend, ifn, Sfilnam ;saying hello
		puts	Shello, 1
	if istart > iend igoto error ;break if the start index is larger than the end index

indx		=	istart
Sformat		sprintf	"%%.%df\t", iprec
istep		=	0
		
newline:
Sdump		=	""
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
		fprints	Sfilnam, Sout
		goto	newline
	endif
	if indx <= iend igoto loop
Sout		strcat	Sdump, "%n"
		fprints	Sfilnam, Sout
		goto	end
		
error: 		prints	"Error! Index istart is larger than index iend.%n"
end:
	endop
 
