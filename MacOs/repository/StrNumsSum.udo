/*
StrNumsSum - calculates the sum of the numbers in Snumstring

DESCRIPTION
calculates the sum of the numbers in Snumstring. if ihop=1, each element is used; if ihop=2 each second element, etc (default=1). istartel (default=0) defines the first element, iendel (default=-1: until end) the last element to be summed.
requires StrGetEl

SYNTAX
isum StrNumsSum Snumstring, ihop, istartel, iendel

INITIALIZATION
Snumstring - string containing just numbers
ihop - hop size of calculation the sum of the numbers
istartel - at which element to start (default = 0)
iendel - at which element to stop (default = -1 = end of string)

CREDITS
joachim heintz 2010
*/

  opcode StrGetEl, ii, Si
;returns the startindex and the endindex (= the first space after the element) for ielindex in String. if startindex returns -1, the element has not been found
String, ielindx		xin
ilen		strlen		String
istartsel	=		-1; startindex for searched element
iendsel		=		-1; endindex for searched element
iel		=		0; actual number of element while searching
iwarleer	=		1
indx		=		0
 if ilen == 0 igoto end ;don't go into the loop if String is empty
loop:
Snext		strsub		String, indx, indx+1; next sign
ispace		strcmp		Snext, " "; returns 0 if Snext is a space 
itab		strcmp		Snext, "	"; 0 if Snext is a tab 
;;NEXT SIGN IS NOT A SPACE NOR A TAB
if ispace != 0 && itab != 0 then
 if iwarleer == 1 then; first character after a space 
  if iel == ielindx then; if searched element index
istartsel	=		indx; set it
iwarleer	=		0
  else 			;if not searched element index
iel		=		iel+1; increase it
iwarleer	=		0; log that it's no space 
  endif 
 endif 
;;NEXT SIGN IS A SPACE OR TAB
else 
 if istartsel > -1 then; if this is first space after searched element
iendsel		=		indx; set iendsel
		igoto		end ;break
 else	
iwarleer	=		1
 endif 
endif
		loop_lt	indx, 1, ilen, loop 
end: 		xout		istartsel, iendsel
  endop 

  opcode StrNumsSum, i, Spoj
;calculates the sum of the numbers in Snumstring. if ihop=1, each element is used; if ihop=2 each second element, etc (default=1). istartel (default=0) defines the first element, iendel (default=-1: until end) the last element to be summed
Snumstring, ihop, istartel, iendel xin 
isum		=		0
iel		=		istartel
loop:
indx1, indx2	StrGetEl	Snumstring, iel
 if indx1 == -1 then; if there is no more element
		igoto 		end; return
 elseif iendel > -1 && iendel < iel then
		igoto		end
 endif 
Snum		strsub		Snumstring, indx1, indx2
inum		strtod		Snum; element as number
isum		=		isum + inum
iel		=		iel + ihop
		igoto		loop 
end:		xout 		isum
  endop 

 
