/*
StraySetNum - Inserts a number in an array-string at a certain position

DESCRIPTION
Puts the number istrin (default=0) at the position ielindx (default=-1: at the end) of Stray, and returns the result via the strset number istrout (default=1). Elements in Stray are seperated by the two ascii-coded seperators isepA (default=32: space) and isepB (default=9: tab). if just isepA is given, it is also read as isepB.
Requires the UDO FracNum.

SYNTAX
StraySetNum Stray, inum [, ielindx [, istrout] [, isep1 [, isep2]]]]

INITIALIZATION
Stray - a string as array
inum - the number to be inserted
ielindx - the element position in Stray at which the number is inserted (starting with 0); the default -1 means append at the end of Stray
istrout - a number for the strset opcode, denoting the resulting string (Stray with the insertion of the new element). the default is 1; as strset creates global numbers you should be careful in using this number
isep1 - the first seperator (default=32: space)
isep2 - the second seperator (default=9: tab)

CREDITS
joachim heintz april 2010
*/

  opcode FracNum, i, io
;returns the number of digits in the fractional part (0=integer)
inum, ifracs	xin
ifac		=		10^ifracs
if int(inum*ifac) == inum*ifac then
igoto end
else
ifracs		FracNum	inum, ifracs+1
endif
end:		xout 		ifracs
  endop

  opcode StraySetNum, 0, Sijpjj
;puts the number istrin (default=0) at the position ielindx (default=-1: at the end) of Stray, and returns the result via the strset number istrout (default=1). elements in the string are seperated by the two ascii-coded seperators isepA (default=32: space) and isepB (default=9: tab). if just isepA is given, it is also read as isepB
Stray, inum, ielindx, istrout, isepA, isepB xin
;;DEFINE THE SEPERATORS
isep1		=		(isepA == -1 ? 32 : isepA)
isep2		=		(isepA == -1 && isepB == -1 ? 9 : (isepB == -1 ? isep1 : isepB))
Sep1		sprintf	"%c", isep1
Sep2		sprintf	"%c", isep2
;;INITIALIZE SOME PARAMETERS
ifracs		FracNum	inum
ilen		strlen		Stray
iel		=		0; actual element position
iwarsep	=		1
indx		=		0
;;APPEND inum IF ielindx=-1
 if ielindx == -1 then
Sformat	sprintf	"%%s%%s%%.%df", ifracs
Sres		sprintf	Sformat, Stray, Sep1, inum
		igoto		end	
  endif
;;PREPEND inum IF ielindx=0
 if ielindx == 0 then
Sformat	sprintf	"%%.%df%%s%%s", ifracs
Sres		sprintf	Sformat, inum, Sep1, Stray
		igoto		end	
  endif
loop:
Snext		strsub		Stray, indx, indx+1; next sign
isep1p		strcmp		Snext, Sep1; returns 0 if Snext is sep1
isep2p		strcmp		Snext, Sep2; 0 if Snext is sep2
;;NEXT SIGN IS NOT SEP1 NOR SEP2
if isep1p != 0 && isep2p != 0 then
 if iwarsep == 1 then; first character after a seperator 
  if iel == ielindx then; if searched element index
S1		strsub		Stray, 0, indx; string before Sin
S2		strsub		Stray, indx, -1; string after Sin
Sformat	sprintf	"%%s%%.%df%%s%%s", ifracs
Sres		sprintf	Sformat, S1, inum, Sep1, S2
		igoto		end
  else 			;if not searched element index
iel		=		iel+1; increase it
iwarsep	=		0; log that it's not a seperator 
  endif 
 endif 
;;NEXT SIGN IS SEP1 OR SEP2
else 
iwarsep	=		1
endif
		loop_lt	indx, 1, ilen, loop 
;;APPEND inum IF ielindx IS >= NUMBER OF ELEMENTS
Sformat	sprintf	"%%s%%s%%.%df", ifracs
Sres		sprintf	Sformat, Stray, Sep1, inum
end:		strset		istrout, Sres
  endop 

 
