/*
StrGetNum - returns the number for ielindex in Snumstring

DESCRIPTION
returns the number for ielindex in Snumstring. if ielindex is out of range, an error occurs

SYNTAX
inum StrGetNum Snumstring, ielindx

INITIALIZATION
Snumstring - string containing just numbers as elements, seperated by spaces or tabs
ielindx - index of searched element, starting with 0


CREDITS
joachim heintz 2010
*/

  opcode StrGetNum, i, Si
;returns the number for ielindex in Snumstring. if ielindex is out of range, an error occurs
Snumstring, ielindx		xin
ilen		strlen		Snumstring
istartsel	=		-1; startindex for searched element
iendsel		=		-1; endindex for searched element
iel		=		0; actual number of element while searching
iwarleer	=		1
indx		=		0
 if ilen == 0 igoto end ;don't go into the loop if String is empty
loop:
Snext		strsub		Snumstring, indx, indx+1; next sign
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
end: 		
Snum		strsub		Snumstring, istartsel, iendsel
inum		strtod		Snum
		xout		inum
  endop 

 
