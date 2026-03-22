/*
BufPlay1 - Plays a mono sample from a GEN01 (non-normalized) function table (with poscil3)

DESCRIPTION
Plays a mono sample from a GEN01 (non-normalized) function table (with poscil3). The input parameters are similar to diskin, plus a factor for amplitude scaling.

SYNTAX
aout BufPlay1 ifn, kspeed, iskip, kvol

INITIALIZATION
ifn - number of the function table which contains the sample (please use GEN01 without normalizing, i.e. using -1 as the number of the gen routine)
iskip - skiptime (sec)



PERFORMANCE
kspeed - speed and direction (negative = backwards) of the pointer through the sample, e.g. 1 = normal, 2 = double (=octave higher), -0.5 = half (octave lower) and backwards
kvol - ampltude scaling factor (1 = original amplitude)

CREDITS
joachim heintz 2008
*/

opcode 	BufPlay1, a, ikik
	ifn, kspeed, iskip, kvol	xin
irel	=	ftlen(ifn) / ftsr(ifn)
kcps	=	kspeed / irel
iphs	=	iskip / irel
asig	poscil3 	kvol, kcps, ifn, iphs
  	xout	asig
	endop
 
