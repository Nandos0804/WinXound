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



#ifndef _WX_TERMINAL_H_
#define _WX_TERMINAL_H_

class wxTerminal //: public Gtk::Box 
{
public:
	wxTerminal(bool isCompiler);
	virtual ~wxTerminal();

	Gtk::Widget* getTerminalWidget();
	void Compile(Glib::ustring compilerName,
	             Glib::ustring parameters,
	             Glib::ustring filename1,
	             Glib::ustring filename2);
	void Compile(Glib::ustring compilerName,
	             Glib::ustring parameters,
	             Glib::ustring filename1,
	             Glib::ustring filename2,
	             bool ClearBuffer);
	void ConfigureTerminalForCompiler();
	void CompilerCompleted();
	void PauseCompiler();
	void StopCompiler();
	void RestartTerminalPage();
	void SetFocus();
	void SetCompilerFont(Glib::ustring name, gint size);
	void ForceKill();
	void SaveOutput();
	bool HasFocus();
	void ClearCompilerText();
	void AppendCompilerText(Glib::ustring text);

	bool ProcessActive;
	bool mIsCompiler;

	static void on_RestartTerminalPage(GtkWidget *widget, gpointer data);
	static void on_child_exited(GtkWidget *widget, gpointer data);
	static void on_buttonStop_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonPause_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonPanic_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonSave_clicked(GtkWidget *widget, gpointer data);


	//signal accessor:
	//string mErrorLine, string mWaveFile
	typedef sigc::signal<void, Glib::ustring, Glib::ustring> type_signal_compiler_completed;
	type_signal_compiler_completed signal_compiler_completed(){return m_compiler_completed;};	

protected:

	type_signal_compiler_completed  m_compiler_completed;

	
	
	bool CreateNewTerminal();
	bool CreateNewCompiler();
	
	int StringLength(gchar** array);
	//bool TimerCheck_Tick();


	static void  on_check_resize (GtkWidget      *widget,
	                              GtkRequisition *requisition,
	                              gpointer        user_data) ;
	void  check_resize();
	Glib::ustring FindError(Glib::ustring text);
	Glib::ustring FindSounds(Glib::ustring text);
	

	GtkWidget*		frame;
	pid_t			pid;
	Gtk::Widget*	wid;
	GtkWidget*		vte;
	GtkWidget*		buttonStop;
	GtkWidget*		buttonPause;
	GtkWidget*		buttonPanic;
	GtkWidget*		buttonSave;
	bool			paused;
	GtkWidget*		scrollbar;

	Glib::ustring   currentFilename;
	Glib::ustring   currentOutputPath;


private:

};

#endif // _WX_TERMINAL_H_
