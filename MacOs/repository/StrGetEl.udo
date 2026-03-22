/*
StrGetEl - gets the element of a string by indexing (like tab_i/tablei for function tables)

DESCRIPTION
gets the element of a string by indexing. elements are a group of characters which are seperated by another group by spaces or tabs. as at the moment (march 2010) no strings can be returned by a UDO, the startindex and the endindex are returned. if startindex returns -1, the element has not been found

SYNTAX
istartsel, iendsel StrGetEl String, ielindx

INITIALIZATION
String - input string
ielindx - index of selected element (0 = first element)
istartsel - character index in String at which the searched element begins, or -1 if it hasn't been found
iendsel - character index in String which is the first after the element, or -1 for the end of String

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

 
