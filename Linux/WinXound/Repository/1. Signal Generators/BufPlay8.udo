/*
BufPlay8 - Plays an eight channel sample from eight GEN01 (non-normalized) function tables (with poscil3)

DESCRIPTION
Plays an eight channel sample from eight GEN01 (non-normalized) function tables (see example). Playing is done with poscil3; the parameters are similar to diskin, plus a factor for amplitude scaling.

SYNTAX
a1, a2, a3, a4, a5, a6, a7, a8 BufPlay8 ifn1, ifn2, ifn3, ifn4, ifn5, ifn6, ifn7, ifn8, kspeed, iskip, kvol

INITIALIZATION
ifn1 - number of the function table which contains channel 1 of the sample (use GEN01 without normalizing, i.e. using -1 as the number of the gen routine, and 1 for the channel parameter) 
ifn2 - number of the function table which contains channel 2 of the sample (use the same f-statement as for ifn1 but with 2 for the channel parameter) 
ifn3... ifn8 - the same with 3 ... 8 for the channel parameters
iskip - skiptime (sec) 

PERFORMANCE
kspeed - speed and direction (negative = backwards) of the pointer through the sample, e.g. 1 = normal, 2 = double (=octave higher), -0.5 = half (octave lower) and backwards 
kvol - ampltude scaling factor (1 = original amplitude)

CREDITS
joachim heintz 2008
*/

	opcode 	BufPlay8, aaaaaaaa, iiiiiiiikik
	ifn1, ifn2, ifn3, ifn4, ifn5, ifn6, ifn7, ifn8, kspeed, iskip, kvol	xin
irel	=	ftlen(ifn1) / ftsr(ifn1)
kcps	=	kspeed / irel
iphs	=	iskip / irel
a1	poscil3 	kvol, kcps, ifn1, iphs
a2	poscil3 	kvol, kcps, ifn2, iphs
a3	poscil3 	kvol, kcps, ifn3, iphs
a4	poscil3 	kvol, kcps, ifn4, iphs
a5	poscil3 	kvol, kcps, ifn5, iphs
a6	poscil3 	kvol, kcps, ifn6, iphs
a7	poscil3 	kvol, kcps, ifn7, iphs
a8	poscil3 	kvol, kcps, ifn8, iphs
  	xout	a1, a2, a3, a4, a5, a6, a7, a8
	endop
 
