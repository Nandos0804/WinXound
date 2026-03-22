<Cabbage>
form caption("Subtractive Synth") size(474, 265), colour("black")
image pos(1, 1), size(450, 130), colour("black"), shape("rounded"), outline("white"), line(4)
rslider pos(30, 20), size(90, 90) channel("cf"), min(0), max(20000), caption("Centre Frequency"), colour("white"), midiCtrl(1, 7)
rslider pos(130, 20), size(90, 90) channel("res"), min(0), max(1), caption("Resonance"), colour("white")
rslider pos(230, 20), size(90, 90) channel("lfo_rate"), min(0), max(10), caption("LFO Rate"), colour("white")
rslider pos(330, 20), size(90, 90) channel("lfo_depth"), min(0), max(10000), caption("LFO Depth"), colour("white"), midiCtrl(2, 7)
keyboard pos(1, 140), size(450, 100)
</Cabbage>
<CsoundSynthesizer>
<CsOptions>
-d -n -+rtmidi=null -M0 -b1024 --midi-key-cps=4 --midi-velocity-amp=5 
;-+rtaudio=alsa -odac
</CsOptions>
<CsInstruments>
; Initialize the global variables.
sr = 44100
ksmps = 32
nchnls = 2


massign 0, 1

instr 1
kenv linenr 1, 0.1, 1, 0.01

kcf chnget "cf"
kres chnget "res"
klforate chnget "lfo_rate"
klfodepth chnget "lfo_depth"
 
asig vco p5, p4, 1
klfo lfo klfodepth, klforate, 5
aflt moogladder asig, kcf+klfo, kres
outs aflt*kenv, aflt*kenv
endin



</CsInstruments>
<CsScore>
f1 0 1024 10 1
f0 3600
</CsScore>
</CsoundSynthesizer>
