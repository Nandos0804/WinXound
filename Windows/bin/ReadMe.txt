WinXound
========
WinXound is a free and open-source Front-End GUI Editor for CSound 5, CSoundAV, 
CSoundAC, with Python and Lua support, developed by Stefano Bonetti. 
It runs on Microsoft Windows and Apple Mac OsX.
WinXound is optimized to work with the new CSound 5 compiler.


FEATURES:
=========
- Edit CSound, Python and Lua files (csd, orc, sco, py, lua) with Syntax Highlight;
- Run CSound, CSoundAV, CSoundAC, Python and Lua compilers;
- Run external language tools (CSoundGUI or QuteCsound, Idle, or other GUI Editors);
- CSound analysis user friendly GUI;
- Integrated CSound manual help;
- Possibilities to set personal colors for the syntax highlighter; 
- Convert orc/sco to csd or csd to orc/sco; 
- Split code into two windows horizontally or vertically;
- CSound csd explorer (File structure for Tags and Instruments)
- Line numbers; 
- Bookmarks;
- Code repository to store your preferred code; 
And much more ...


SYSTEM REQUIREMENTS:
==================== 
- Operating systems: Microsoft Windows Xp, Vista, Seven (32/64 bit versions); 
  (For Windows Xp you also need the Microsoft Framework .Net version 2.0 or Major.
   You can download it from www.microsoft.com site)
- CSound 5: http://sourceforge.net/projects/csound
  (needed for CSound and LuaJit compilers);
- Not requested but suggested: CSoundAV by Gabriel Maldonado 
  (http://www.csounds.com/maldonado/);
- Requested to work with Python: Python compiler 
  (http://www.python.org/download/)


INSTALLATION AND USAGE:
=======================
- ONLY FOR WINDOWS XP: Download and install the 
  Microsoft Framework .Net version 2.0 or Major;
- Download and install the latest version of CSound 5 located at:
  http://sourceforge.net/projects/csound; 
- Download the WinXound zipped file, 
  decompress it where you want (see the (*)note below),
  double-click on "WinXound_Net" executable;
- In order to use the program correctly, it's very important that you 
  fill all the compiler paths fields (see "Menu File -> Settings"). 

(*)note:
THE WINXOUND FOLDER MUST BE LOCATED IN A PATH WHERE YOU HAVE FULL READ AND WRITE PERMISSION,
for example in your User Personal folder.


WEB SITE AND CONTACTS:
======================
- Web: winxound.codeplex.com
- Email: stefano_bonetti@tin.it (or stefano_bonetti@alice.it)


SOURCE CODE:
============
- The source code is written in C# using Microsoft Visual C# Express 2008.
- The Scintilla library (http://www.scintilla.org/) used inside WinXound is compiled 
  using Microsoft Visual C++ 2008 Express.
They are freeware and you can download them at the Microsoft Visual Studio Web-Site.


CREDITS:
========
- The TextEditor is entirely based on the wonderful SCINTILLA text control by Neil Hodgson
(www.scintilla.org).