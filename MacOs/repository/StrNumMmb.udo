/*
StrNumMmb - returns the position (as index of element) of the first occurence of inum in Snumstring, or -1 if no element has been found

DESCRIPTION
returns the position (as index of element) of the first occurence of inum in Snumstring, or -1 if no element has been found. the numbers in Snumstring are seperated be isep (the ascii code of a sign, default=44: comma)

SYNTAX
ipos StrNumMmb Snumstring, inum, isep

INITIALIZATION
Snumstring - string of numbers
inum - number which is asked to be in Snumstring
isep - ascii code of the seperator of elements in Snumstring (default=44: comma)
ipos - element index (0 = first) of the first occurrence of inum in Snumstring, or -1 if inum has not been found

CREDITS
joachim heintz 2010
*/

  opcode StrNumMmb, i, Sio
;returns the position (as index of element) of the first occurence of inum in Snumstring, or -1 if no element has been found. the numbers in Snumstring are seperated be isep (the ascii code of a sign, default=44: comma)
Snumstring, inum, isep	xin 
isep	 	=		(isep == 0 ? 44 : isep)
Sep		sprintf	"%c", isep
Swithsep	strcat		Snumstring, Sep; add one seperator at the end 
istartsel	=		0; startindex as in strindex for next element
iposel		=		0; position of the next element
iyep		=		0; was a member found or not 
loop:
Srest		strsub 	Swithsep, istartsel; rest of the string
irestlen	strlen		Srest ; its length 
 if irestlen == 0 igoto end ;break if there is nothing more to do
inextsep	strindex 	Srest, Sep
Snextnum	strsub		Srest, 0, inextsep
inextnum	strtod		Snextnum ;get next number
 if inextnum == inum then ;if it's a member
iyep		=		1 ;tell it
		igoto		end ;and break
 else 				;if not
iposel		=		iposel + 1 ;update element counter
istartsel	=		istartsel + inextsep + 1 ;and startindex
		igoto		loop ;and go back to loop 
 endif 
end:		
ipos		=		(iyep == 0 ? -1 : iposel)
		xout		ipos
  endop 

 
