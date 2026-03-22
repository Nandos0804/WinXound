/*
StraySetEl - Inserts an element in an array-string at a certain position

DESCRIPTION
Inserts the string decoded by the strset number istrin (default=0) at the position ielindx (default=-1: at the end) of Stray, and returns the result again as a strset number istrout (default=1). Elements in the string are seperated by the two ascii-coded seperators isepA (default=32: space) and isepB (default=9: tab). If just isepA is given, it is also read as isepB.

SYNTAX
StraySetEl Stray [, istrin [, ielindx [, istrout] [, isep1 [, isep2]]]]

INITIALIZATION
Stray - a string as array 
istrin - a number for the strset opcode, denoting the string to be inserted in Stray (default=0)
ielindx - the element position in Stray at which the new element is inserted (starting with 0); the default -1 means append at the end of Stray
istrout - a number for the strset opcode, denoting the resulting string (Stray with the insertion of the new element). the default is 1; as strset creates global numbers you should be careful in using this number
isep1 - the first seperator (default=32: space)
isep2 - the second seperator (default=9: tab) 

CREDITS
joachim heintz april 2010
*/

  opcode StraySetEl, 0, Sojpjj
;puts the string decoded by the strset number istrin (default=0) at the position ielindx (default=-1: at the end) of Stray, and returns the result again as a strset number istrout (default=1). elements in the string are seperated by the two ascii-coded seperators isepA (default=32: space) and isepB (default=9: tab). if just isepA is given, it is also read as isepB
Stray, istrin, ielindx, istrout, isepA, isepB xin
;;DEFINE THE SEPERATORS
isep1		=		(isepA == -1 ? 32 : isepA)
isep2		=		(isepA == -1 && isepB == -1 ? 9 : (isepB == -1 ? isep1 : isepB))
Sep1		sprintf	"%c", isep1
Sep2		sprintf	"%c", isep2
;;INITIALIZE SOME PARAMETERS
Sin		strget		istrin
ilen		strlen		Stray
iel		=		0; actual element position
iwarsep	=		1
indx		=		0
;;APPEND Sin IF ielindx=-1
 if ielindx == -1 then
Sres		sprintf	"%s%s%s", Stray, Sep1, Sin
		igoto		end	
  endif
;;PREPEND Sin IF ielindx=0
 if ielindx == 0 then
Sres		sprintf	"%s%s%s", Sin, Sep1, Stray
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
Sres		sprintf	"%s%s%s%s", S1, Sin, Sep1, S2
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
;;APPEND Sin IF ielindx is >= number of elements
Sres		sprintf	"%s%s%s", Stray, Sep1, Sin
end:		strset		istrout, Sres
  endop 

 
