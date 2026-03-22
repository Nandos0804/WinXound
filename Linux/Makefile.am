## Process this file with automake to produce Makefile.in
## Created by Anjuta

SUBDIRS = ./scintilla/gtk ./src

#all: standalone

# Make standalone
standalone:
	@echo
	@echo [make standalone]: operation started...
	-cp ./src/winxound ./WinXound/winxound
	-cp ./src/*.ui ./WinXound/src
	-cp ./src/*.png ./WinXound/src
	-cp ./src/opcodes.txt ./WinXound/src/opcodes.txt
	@echo [make standalone]: operation completed.
	@echo
	@echo To run WinXound type: ./WinXound/winxound
	@echo or go to the WinXound folder and double click on winxound.
	@echo


install:
	@echo
	@echo Installation not supported - Please use:
	@echo 'make standalone'
	@echo When finished copy the WinXound folder inside a path
	@echo where you have full Read and Write permission, for example
	@echo in your Home folder, and double click the winxound executable file.
	@echo

uninstall: ;
uninstall-local: ;

## Remove doc directory on uninstall
#uninstall-local:
#	-rm -r $(winxound_gtkmmdocdir)
