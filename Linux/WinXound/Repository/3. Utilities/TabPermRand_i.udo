/*
TabPermRand_i - Permutes the values of a function table randomly

DESCRIPTION
Permutes the values of iTabin randomly and writes the result (optionally with printing) to iTabout

SYNTAX
TabPermRand_i iTabin, iTabout, iprint, iprecision

INITIALIZATION
iTabin: function table to be permuted
iTabout: function table in which the result is written
iprint: 1=result is printed out
iprecision: float precision

CREDITS
joachim heintz 2009
*/

opcode TabPermRand_i, 0, iiii
iTabin, iTabout, iprint, iprecision	xin
itablen	=		ftlen(iTabin)
icopy		ftgentmp	0, 0, -itablen, -2, 0 
		tableicopy	icopy, iTabin
ileng		init		itablen
irndlm		init		ileng
indxerg	init		0
Sformat	sprintf	"%%.%df\t", iprecision
loop:
irand		random	0, irndlm - .0001
index		=	int(irand)
ival		tab_i	index, icopy
		tabw_i	ival, indxerg, iTabout
	korrektur:
if (index + 1 == ileng) igoto korrigiert
indxneu	=	index
indxalt	=	indxneu+1
ivalalt	tab_i	indxalt, icopy
		tabw_i	ivalalt, indxneu, icopy
index	=	index + 1
	igoto	korrektur
	korrigiert:
ileng	=	ileng - 1
irndlm	=	irndlm - 1
indxerg	=	indxerg + 1
if iprint == 1 then
		prints		Sformat, ival
endif
if (irndlm > 0) igoto loop
		prints		"%n"
endop
 
