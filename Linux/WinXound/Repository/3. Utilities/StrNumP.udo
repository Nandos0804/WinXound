/*
StrNumP - tests whether a string is a numerical string

DESCRIPTION
tests whether a string is a numerical string ("1" or "1.23435" but not "1a"). returns 1 for "yes" and 0 for "no". if "yes", the string can be converted to a number by the opcode strtod

SYNTAX
itest StrNumP String

INITIALIZATION
String - any string
itest - 1 if String is a numerical string, 0 if not

CREDITS
joachim heintz 2010
*/

  opcode StrNumP, i, S
Str		xin	
ip		=		1
ilen		strlen 	Str
 if ilen == 0 then
ip		=		0
		igoto		end 
 endif 
indx		=		0
inpnts		=		0; how many dots have there been
loop:
iascii		strchar	Str, indx
 if iascii < 48 || iascii > 57 then; if not 0-9
  if iascii == 46 && inpnts == 0 then; if first dot
inpnts		=		1
  else 
ip		=		0
  endif 
 endif	
		loop_lt	indx, 1, ilen, loop 
end:		xout		ip
  endop 

 
