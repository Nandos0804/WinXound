/*
BufPlay2 - Plays a stereo sample from two GEN01 (non-normalized) function tables (with poscil3)

DESCRIPTION
Plays a stereo sample from two GEN01 (non-normalized) function tables for the left and right channel (see example). Playing is done with poscil3; the parameters are similar to diskin, plus a factor for amplitude scaling.

SYNTAX
aL, aR BufPlay2 ifnL, ifnR, kspeed, iskip, kvol

INITIALIZATION
ifnL - number of the function table which contains channel 1 of the sample (use GEN01 without normalizing, i.e. using -1 as the number of the gen routine, and 1 for the channel parameter) 
ifnR - number of the function table which contains channel 2 of the sample (use the same f-statement as for ifnL but with 2 for the channel parameter) 
iskip - skiptime (sec) 



PERFORMANCE
kspeed - speed and direction (negative = backwards) of the pointer through the sample, e.g. 1 = normal, 2 = double (=octave higher), -0.5 = half (octave lower) and backwards 
kvol - ampltude scaling factor (1 = original amplitude)

CREDITS
joachim heintz 2008
*/

	opcode 	BufPlay2, aa, iikik
	ifnL, ifnR, kspeed, iskip, kvol	xin
irel	=	ftlen(ifnL) / ftsr(ifnL)
kcps	=	kspeed / irel
iphs	=	iskip / irel
aL	poscil3 	kvol, kcps, ifnL, iphs
aR	poscil3 	kvol, kcps, ifnR, iphs
  	xout	aL, aR
	endop
 
