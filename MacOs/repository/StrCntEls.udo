/*
StrCntEls - returns number of elements in a string

DESCRIPTION
returns the number of elements in String, as the number of fields which are seperated by spaces or tabs

SYNTAX
inmels StrCntEls String

INITIALIZATION
String - a string
inmels - number of fields in String which are seperated by spaces or tabs

CREDITS
joachim heintz 2010
*/

  opcode StrCntEls, i, S
;returns the number of elements in String, as the number of fields which are seperated by spaces or tabs
String		xin
ilen		strlen		String
icount		=		0
iwarleer	=		1
indx		=		0
 if ilen == 0 igoto end ;don't go into the loop if String is empty
loop:
Snext		strsub		String, indx, indx+1; next sign
ispace		strcmp		Snext, " "; returns 0 if Snext is a space 
itab		strcmp		Snext, "	"; 0 if Snext is a tab 
 if ispace == 0 || itab == 0 then; if space or tab
iwarleer	=		1; tell the log so
 else 				; if not 
  if iwarleer == 1 then	; and has been space or tab before
icount		=		icount + 1; increase counter
iwarleer	=		0; and tell you are no space nor tab 
  endif 
 endif	
		loop_lt	indx, 1, ilen, loop 
end: 		xout		icount
  endop 

 
