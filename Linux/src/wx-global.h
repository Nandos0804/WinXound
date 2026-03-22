/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * winxound_gtkmm
 * Copyright (C) Stefano Bonetti 2010 <stefano_bonetti@tin.it>
 * 
 * winxound_gtkmm is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * winxound_gtkmm is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <gtkmm.h>

#ifndef _WX_GLOBAL_H_
#define _WX_GLOBAL_H_



#define wxGLOBAL wxGlobal::getInstance()
#define TITLE "WinXound 3.4.0"
#define VERSION "Version 3.4.0"

#define CABBAGE_NAME "cabbage"

//#define UI_FILE PACKAGE_DATA_DIR"/winxound/ui/winxound_gtkmm.ui"
//#define RELEASE

	#define WINXOUND_ICON "/usr/local/share/winxound/src/winxound_48.png"
	#define UI_FILE "/usr/local/share/winxound/src/winxound_gtkmm.ui"
	#define UI_ANALYSIS_FILE "/usr/local/share/winxound/src/winxound_analysis.ui"
	#define UI_FINDANDREPLACE_FILE "/usr/local/share/winxound/src/winxound_findandreplace.ui"
	#define UI_FINDLINE_FILE "/usr/local/share/winxound/src/winxound_findline.ui"
	#define UI_SETTINGS_FILE "/usr/local/share/winxound/src/winxound_settings.ui"


class wxEditor;


class wxGlobal
{
public:
    static wxGlobal* getInstance( void);

	virtual ~wxGlobal();
	
    //Place Public (non-static) class methods here
	bool isSyntaxType(wxEditor* editor);
	Glib::ustring BoolToString(bool b);
	bool StringToBool(Glib::ustring value);
	Gdk::Point StringToPoint(Glib::ustring value);
	Glib::ustring PointToString(Gdk::Point p);
	Glib::ustring IntToString(int value);
	gdouble StringToDouble(Glib::ustring text);
	Glib::ustring DoubleToString(gdouble value);
	guint StringToHex(Glib::ustring str);
	Glib::ustring HexToString(Gdk::Color color);
	void DebugPrint(const gchar* message);
	void DebugPrint(const gchar* message, const gchar* description);
	Glib::ustring getSettingsPath();
	Glib::ustring getRepositoryPath();
	Glib::ustring getHelpPath();
	Glib::ustring getHomePath();
	Glib::ustring getIconsPath();
	void CheckWinXoundDirectories();
	int ShowMessageBox(const char* text, const char* title, Gtk::ButtonsType type);
	int ShowMessageBox(Gtk::Window* ownerWindow, const char* text, const char* title, Gtk::ButtonsType type);
	Glib::ustring getExePath();
	GHashTable* LoadOpcodes();
	bool FileIsReadable(const gchar* filename);
	bool FileIsWritable(const gchar* filename);
	bool FileIsExecutable(const gchar* filename);
	int ArrayLength(gchar** array);
	Glib::ustring getSrcPath();
	Glib::ustring Trim(Glib::ustring text);
	Glib::ustring TrimLeft(Glib::ustring s);
	Glib::ustring TrimRight(Glib::ustring s);
	Glib::ustring RemoveDoubleSpaces(Glib::ustring s);
	Glib::ustring getCabbagePath();
	Glib::ustring getCabbageRepositoryPath();
	Glib::ustring getFileExtension(Glib::ustring filename);
	Glib::ustring getFileNameWithoutExtension(Glib::ustring filename);
	

private:
    static wxGlobal* instance;
    wxGlobal(void);
};


#endif // _WX_GLOBAL_H_
