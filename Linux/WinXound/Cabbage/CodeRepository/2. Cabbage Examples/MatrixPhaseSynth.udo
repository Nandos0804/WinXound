<Cabbage>
form caption("Matrix Phase Modulation Synth") size(690, 700), colour("grey")
image pos(2, 6), size(445, 410), file("1x1Matrix.png")
groupbox caption("Index One"), pos(10, 10), size(430, 130), colour("yellow")
rslider pos(30, 30),  size(90, 90) channel("gkatt2"), min(0), max(5), caption("Attack"), colour("white"), value(0.01)
rslider pos(130, 30), size(90, 90) channel("gkdec2"), min(0), max(16), caption("Decay"), colour("white"), value(3.034) 
rslider pos(230, 30), size(90, 90) channel("gkslev2"), min(0), max(1), caption("Sustain"), colour("white"), value(0.0) 
rslider pos(330, 30), size(90, 90) channel("gkrel2"), min(0), max(1), caption("Release"), colour("white"), value(0.1) 

groupbox caption("Index Two"), pos(10, 145), size(430, 130), colour("yellow")                                   
rslider pos(30, 165), size(90, 90) channel("gkatt1"), min(0), max(5), caption("Attack"), colour("white"), value(0.01)
rslider pos(130, 165), size(90, 90) channel("gkdec1"), min(0), max(16), caption("Decay"), colour("white"), value(2.41)
rslider pos(230, 165), size(90, 90) channel("gkslev1"), min(0), max(1), caption("Sustain"), colour("white")value(0)
rslider pos(330, 165), size(90, 90) channel("gkrel1"), min(0), max(1), caption("Release"), colour("white") value(0.1)


groupbox caption("Index Three"), pos(10, 280), size(430, 130), colour("yellow")
rslider pos(30, 300), size(90, 90) channel("gkatt0"), min(0), max(5), caption("Attack"), colour("white"), value(0.01) 	
rslider pos(130, 300), size(90, 90) channel("gkdec0"), min(0), max(16), caption("Decay"), colour("white"), value(6) 	
rslider pos(230, 300), size(90, 90) channel("gkslev0"), min(0), max(1), caption("Sustain"), colour("white"), value(0)	
rslider pos(330, 300), size(90, 90) channel("gksrel0"), min(0), max(1), caption("Release"), colour("white"), value(0.1) 	

groupbox caption("Filter"), pos(0, 415), size(447, 122), colour("white")
groupbox pos(17, 425), size(415, 43), colour("yellow")
groupbox pos(17, 455), size(415, 43), colour("yellow")
groupbox pos(17, 485), size(415, 43), colour("yellow")
label caption("Cut-Off"), pos(40, 425), size(100, 50), colour("white")
hslider pos(100, 425), size(330, 50), min(0), max(20000), colour("white"), channel("gkcutoff"), value(5737)
label caption("Resonance"), pos(20, 455), size(100, 50), colour("white")                               
hslider pos(100, 455), size(330, 50), min(0), max(1), colour("white"), channel("gkreson"), value(0.252)
label caption("Chorus"), pos(40, 485), size(100, 50), colour("white")
hslider pos(100, 485), size(330, 50), min(0), max(1), colour("white"), channel("gkchorus"), value(0.732)

image pos(2, 540) size(445, 40), file("credits.png")

groupbox caption("Room"), pos(450, 385), size(220, 194), colour("yellow")
rslider pos(465, 402), size(90, 80) channel("gkpan"), colour("black"), min(0), value(0.5), max(1), caption("Pan"), colour("black")
rslider pos(565, 402), size(90, 80) channel("gkreverb"), colour("black"), min(0), max(1), value(0.7), caption("Room Size"), colour("black")
rslider pos(465, 486), size(90, 80) channel("gkfco"), colour("black"), min(0), max(12000), value(8535), caption("FCO"), colour("black")
rslider pos(565, 486), size(90, 80) channel("gkwetdry"), colour("black"), min(0), max(1), value(0.5), caption("Wet/Dry"), colour("black")

image pos(460, 15), size(196, 180) file("3x3Matrix.png")
groupbox caption("Modulation Matrix"), pos(450, 0), size(220, 207), colour("yellow")
rslider pos(487, 40), size(40, 40), colour("white"), min(0), max(4), channel("gkindex2_2")  value(0)
rslider pos(549, 40), size(40, 40), colour("white"), min(0), max(4), channel("gkindex2_1")  value(2)
rslider pos(607, 40), size(40, 40), colour("white"), min(0), max(4), channel("gkindex2_0")  value(1.5)                                                                                             
rslider pos(487, 93), size(40, 40), colour("white"), min(0), max(4),  channel("gkindex1_2") value(0)
rslider pos(549, 93), size(40, 40), colour("white"), min(0), max(4),  channel("gkindex1_1") value(0)
rslider pos(607, 93), size(40, 40), colour("white"), min(0), max(4),  channel("gkindex1_0") value(3.1)                                                                                             
rslider pos(487, 147), size(40, 40), colour("white"), min(0), max(4), channel("gkindex0_2") value(0)
rslider pos(549, 147), size(40, 40), colour("white"), min(0), max(4), channel("gkindex0_1") value(0)
rslider pos(607, 147), size(40, 40), colour("white"), min(0), max(4), channel("gkindex0_0") value(1.7)

groupbox caption("Oscil Direct Mix"), pos(450, 210), size(220,90), colour("black")
label pos(460, 225) size(100, 30), caption("3"), colour("white")
hslider pos(480, 230), size(180, 20), colour("yellow"), min(0), max(2), channel("gkmix0"), value(0.5)
label pos(460, 245) size(100, 30), caption("2"), colour("white")
hslider pos(480, 250), size(180, 20), colour("yellow"), min(0), max(2), channel("gkmix1"), value(0.5)
label pos(460, 265) size(100, 30), caption("1"), colour("white")
hslider pos(480, 270), size(180, 20), colour("yellow"), min(0), max(2), channel("gkmix2"), value(2)

groupbox caption("Madulator ratios"), pos(450, 300), size(220,80), colour("black")
label pos(460, 320) size(100, 30), caption("1"), colour("white")
hslider pos(480, 325), size(180, 20), colour("yellow"), min(0), max(5),  value(1.999), channel("gkmodratio")
label pos(460, 340) size(100, 30), caption("2"), colour("white")       
hslider pos(480, 345), size(180, 20), colour("yellow"), min(0), max(5), value(5.002), channel("gkmodratio2")



keyboard pos(1, 585), size(665, 100)

</Cabbage>
; inspired by Iain McCurdy's rt collection
; Aaron Krister Johnson, 2011
; adapted for Cabbage by ROry Walsh, 2011
<CsoundSynthesizer>
<CsOptions>
;YOU MAY NEED TO CHANGE THE DEVICE NUMBER AFTER THE MIDI INPUT FLAG (-M)
-d -n -+rtmidi=null -M0 -b1024 --midi-velocity-amp=5 --midi-key-cps=4 
</CsOptions>
<CsInstruments>
sr     = 44100
ksmps  = 32
nchnls = 2
0dbfs = 1

gaL	init 0
gaR	init 0


	instr 1		;FM (PM) INSTRUMENT

kstatus, kchan, kdata1, kdata2 midiin
k1 changed kstatus
k2 changed kchan
k3 changed kdata1
k4 changed kdata2
if((k1==1)||(k2==1)||(k3==1)||(k4==1)) then
	;printks "Status:%d Value:%d ChanNo:%d CtrlNo:%d\n", 0, kstatus, kdata2, kchan, kdata1
endif
if(kstatus!=144) then
;create global variables that read from the 
;different named software buses
gkmodratio2 chnget "gkmodratio2"
gkmodratio chnget "gkmodratio" 
gkindex2_2 chnget "gkindex2_2"
gkindex2_1 chnget "gkindex2_1"
gkindex2_0 chnget "gkindex2_0"
gkindex1_2 chnget "gkindex1_2"
gkindex1_1 chnget "gkindex1_1"
gkindex1_0 chnget "gkindex1_0"
gkindex0_2 chnget "gkindex0_2"
gkindex0_1 chnget "gkindex0_1"
gkindex0_0 chnget "gkindex0_0"

gkmix0 chnget "gkmix0"
gkmix1 chnget "gkmix1"
gkmix2 chnget "gkmix2"
       
gkatt2 chnget "gkatt2"
gkdec2 chnget "gkdec2"
gkslev2 chnget "gkslev2"
gkrel2 chnget "gkrel2"
;;     
gkatt1 chnget "gkatt1"
gkdec1 chnget "gkdec1"
gkslev1 chnget "gkslev1"
gkrel1 chnget "gkrel1"
;;
gkatt0 chnget "gkatt0"
gkdec0 chnget "gkdec0"
gkslev0 chnget "gkslev0"
gkrel0 chnget "gkrel0"
;;
gkcutoff chnget "gkcutoff"
gkreson chnget "gkreson"
gkchorus chnget "gkchorus"
;;
gkpan chnget "gkpan"
gkreverb chnget "gkreverb"
gkfco chnget "gkfco"
gkwetdry chnget "gkwetdry"

;Make sure all controls are sending a signal
printk2 gkmodratio
printk2 gkmodratio
printk2 gkindex2_2
printk2 gkindex2_1
printk2 gkindex2_0
printk2 gkindex1_2
printk2 gkindex1_1
printk2 gkindex1_0
printk2 gkindex0_2
printk2 gkindex0_1
printk2 gkindex0_0

printk2 gkmix0
printk2 gkmix1
printk2 gkmix2

printk2 gkatt2
printk2 gkdec2
printk2 gkslev2
printk2 gkrel2
;;
printk2 gkatt1
printk2 gkdec1
printk2 gkslev1
printk2 gkrel1 
;;
printk2 gkatt0
printk2 gkdec0
printk2 gkslev0
printk2 gkrel0
;;
printk2 gkcutoff 
printk2 gkreson 
printk2 gkchorus 
;;
printk2 gkpan
printk2 gkreverb
printk2 gkfco
printk2 gkwetdry



icps		= p4 			;READ NOTE INFORMATION FROM LIVE MIDI INPUT (CREATE A VARIABLE 'icps' BASED ON RECEIVED NOTE INFORMATION)
iamp		= p5			;READ VELOCITY INFORMATION FROM LIVE MIDI INPUT (CREATE A VARIABLE 'iamp' BASED ON RECEIVED VELOCITY INFORMATION)
i1div2pi	= 0.1592
ipanR		= i(gkpan)
ipanL		= 1-ipanR
;;; initialize feedback
a0LFeedback init 0
a0RFeedback init 0
a1Feedback init 0
a2Feedback init 0
a1 init 0
a2 init 0
a0L init 0
;;; cross modulation index matrix:
kpeak = iamp * i1div2pi
kpeak0_0 = kpeak * gkindex0_0
kpeak0_1 = kpeak * gkindex0_1
kpeak0_2 = kpeak * gkindex0_2
kpeak1_0 = kpeak * gkindex1_0
kpeak1_1 = kpeak * gkindex1_1
kpeak1_2 = kpeak * gkindex1_2
kpeak2_0 = kpeak * gkindex2_0
kpeak2_1 = kpeak * gkindex2_1
kpeak2_2 = kpeak * gkindex2_2
;;; ENVELOPES
kenv2 	mxadsr 	i(gkatt2), i(gkdec2), i(gkslev2)+0.0001, i(gkrel2)	    ;LINE SEGMENT ENVELOPE WITH MIDI RELEASE MECHANISM
kenv1 	mxadsr 	i(gkatt1), i(gkdec1), i(gkslev1)+0.0001, i(gkrel1)          ;LINE SEGMENT ENVELOPE WITH MIDI RELEASE MECHANISM
kenv0 	mxadsr 	i(gkatt0), i(gkdec0), i(gkslev0)+0.0001, i(gkrel0)	
;; STEREO "CHORUS" ENRICHMENT USING JITTER
kjitL		jitter  i(gkchorus)*3, .3, 6
kjitR		jitter  i(gkchorus)*3, .3, 6
;;; MODULATORS
;;;  ares table a, ifn [, ixmode (1 = normalized 0 to 1)] [, ixoffset] [, iwrap] ;;
a2	phasor  icps * gkmodratio2
a2      table   a2 + (a2Feedback * kpeak2_2) + (a1 * kpeak1_2) + (a0L * kpeak0_2), 1, 1, 0, 1
a2 =	a2 * kenv2
a1	phasor  icps * gkmodratio
a1	table   a1 + (a1Feedback*kpeak1_1) + (a2 * kpeak2_1) + (a0L * kpeak0_1), 1, 1, 0, 1
a1 =	a1 * kenv1
a0L	phasor  icps + kjitL
a0R	phasor	icps + kjitR
a0L	table	a0L + (a0LFeedback * kpeak0_0) + (a1 * kpeak1_0) + (a2 * kpeak2_0), 1, 1, 0, 1
a0R	table	a0R + (a0LFeedback * kpeak0_0) + (a1 * kpeak1_0) + (a2 * kpeak2_0), 1, 1, 0, 1
a0L =   a0L * kenv0
a0R =   a0R * kenv0
aSigL	= ((a0L * gkmix0) + (a1 * gkmix1) + (a2 * gkmix2)) * (iamp) ;;; .5 b/c I've found it can get loud...
aSigR	= ((a0R * gkmix0) + (a1 * gkmix1) + (a2 * gkmix2)) * (iamp) ;;; so I wanna give some headroom
;; update feedback
if kenv0 < 0.05 then
  a0LFeedback = 0
else
  a0LFeedback = a0L
endif
if kenv1 < 0.05 then
  a1Feedback = 0
else
  a1Feedback = a1
endif
if kenv2 < 0.05 then
  a2Feedback = 0
else
  a2Feedback = a2
endif
;;
aFilterL	bqrez  aSigL, gkcutoff, gkreson
aFilterR	bqrez  aSigR, gkcutoff, gkreson
aFilterL	balance aFilterL, aSigL
aFilterR	balance aFilterR, aSigR
aL		= aFilterL * .6 * ipanL
aR		= aFilterR * .6 * ipanR
		denorm aL, aR
gaL		= gaL + aL
gaR		= gaR + aR
	outs	aL, aR
endif 
endin



instr 99
aLrev, aRrev	reverbsc gaL, gaR, gkreverb, gkfco, sr, .5, 1
outs aLrev*gkwetdry, aRrev*gkwetdry
gaL = 0
gaR = 0
endin
			

</CsInstruments>
<CsScore>
i99 0 9000       ;;; schhhtarttt the reverb!!! 
f 1 0 65537 10 1 ;;; sine wave, baby!!!
f 0 10000        ;;; dummy score event plays for a long time
</CsScore>
</CsoundSynthesizer>