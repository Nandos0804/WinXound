/*
MatrixPhaseSynth
*/

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