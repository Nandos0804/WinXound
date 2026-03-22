<Cabbage>
form caption("Simple FM Synth") size(440, 300)
groupbox caption("Simple FM Synth"), pos(10, 10), size(400, 150)
hslider pos(30, 30), channel("index"), size(350, 50), min(0), max(50), caption("Index")
hslider pos(30, 80), channel("mf"), size(350, 50), min(0), max(100), caption("Mod Freq")
keyboard pos(10, 180), size(400, 100)
</Cabbage>
<CsoundSynthesizer>
<CsOptions>
-d -n -+rtmidi=null -M0 --midi-key-cps=4 --midi-velocity-amp=5
</CsOptions>
<CsInstruments>
; Initialize the global variables.
sr = 44100
ksmps = 32
nchnls = 2

massign 0, 1

instr 1
kindx chnget "index"
kmf chnget "mf"
kamp expsegr 0.01, 0.1, 10000, 10, 0.01, 1, 1 
amod oscili kindx*kmf, kmf, 1 
acar oscili kamp, p4+amod, 1 
outs acar, acar
endin



</CsInstruments>
<CsScore>
f1 0 1024 10 1
f0 3600

  </CsScore>
</CsoundSynthesizer>
