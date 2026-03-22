;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; This is a simple csound file to test your audio output   ;
; from WinXound. In order to work correctly with WinXound  ;
; it's very important that you fill all the required       ;
; compiler paths fields into the WinXound Settings         ;
; window with correct values (WinXound -> Preferences).    ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

<CsoundSynthesizer>

<CsOptions>
-W -odevaudio
</CsOptions>

<CsInstruments>
sr     = 44100
kr     = 4410
ksmps  = 10
nchnls = 1

	instr 1 ;Simple sine at 440Hz
k1	linen 10000, 0.1, p3, 0.1
a1	oscili k1,440,1
	out a1
	endin
</CsInstruments>

<CsScore>
f1 0 4096 10 1
i1 0 3
</CsScore>

</CsoundSynthesizer>