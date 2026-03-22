/*
BasicFM
*/

<Cabbage>
form caption("Simple FM Synth") size(440, 300)
groupbox caption("Simple FM Synth"), pos(10, 10), size(400, 150)
hslider pos(30, 30), channel("index"), size(350, 50), min(0), max(50), caption("Index")
hslider pos(30, 80), channel("mf"), size(350, 50), min(0), max(100), caption("Mod Freq")
keyboard pos(10, 180), size(400, 100)
</Cabbage>
 
