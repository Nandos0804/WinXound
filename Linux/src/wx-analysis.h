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


#ifndef _WX_ANALYSIS_H_
#define _WX_ANALYSIS_H_

class wxMain;
class wxBrowser;
class wxTerminal;

class wxAnalysis
{
public:
	wxAnalysis(wxMain* owner);
	virtual ~wxAnalysis();

	
	void associateWidgets(Glib::RefPtr<Gtk::Builder> builder);
	void on_buttonExit_Clicked();
	void on_buttonStartAnalysis_Clicked();
	bool on_key_press_event(GdkEventKey* event);
	void showWindowAt(gint x, gint y);
	void closeWindow();
	gint getIndex(Glib::ustring utility);
	void SetDefaultValues(Glib::ustring mUtilityName);
	void on_buttonResetAtsa_Clicked();
	void buttonResetCvanal_Clicked();
	void buttonResetHetro_Clicked();
	void buttonResetLpanal_Clicked();
	void buttonResetPvanal_Clicked();
	Glib::ustring ATSA();
	Glib::ustring CVANAL();
	Glib::ustring HETRO();
	Glib::ustring LPANAL();
	Glib::ustring PVANAL();
	void StartCompiler();
	void on_Compiler_Completed(Glib::ustring mErrorLine, Glib::ustring mWaveFile);
	void on_buttonStopBatch_Clicked();
	void on_buttonInput_Clicked();
	void on_buttonOutput_Clicked();
	void on_buttonClear_Clicked();
	void on_buttonReset_Clicked();
	void on_notebook1_switch_page(Gtk::Widget* page, guint page_num);

	Gtk::Window*		analysisWindow;
	Gtk::SpinButton*	spinbuttonLineNumber;
	Gtk::Button*		buttonFindLine;
	Gtk::Button*		buttonExit;
	Gtk::Notebook*		notebook1;
	Gtk::Notebook*		notebook2;
	Gtk::Statusbar*		statusbar1;

	Gtk::VBox*			vboxCompiler;
	Gtk::VBox*			vboxHelp;

	//////////////////////////////

	//Gtk::ComboBoxEntry* comboboxentryInputFile;
	//Gtk::ComboBox m_Combo;
	//Gtk::ComboBoxEntryText*  comboboxentryInputFile;
	Gtk::ComboBoxText*  comboboxentryInputFile;
	Gtk::Entry*			entryOutputPath;
	Gtk::VBox*			vboxInput;

	Gtk::SpinButton*	b_Atsa;
	Gtk::SpinButton*	e_Atsa;
	Gtk::SpinButton*	l_Atsa;
	Gtk::SpinButton*	Max_Atsa;
	Gtk::SpinButton*	d_Atsa;
	Gtk::SpinButton*	c_Atsa;
	Gtk::SpinButton*	T_SMR_Atsa;
	Gtk::SpinButton*	h_Atsa;
	Gtk::SpinButton*	m_Atsa;
	Gtk::SpinButton*	t_Atsa;
	Gtk::SpinButton*	M_SMR_Atsa;
	Gtk::SpinButton*	s_Atsa;
	Gtk::SpinButton*	S_SMR_Atsa;
	Gtk::SpinButton*	g_Atsa;
	Gtk::SpinButton*	P_Peak_Atsa;
	Gtk::SpinButton*	s_Cvanal;
	Gtk::SpinButton*	c_Cvanal;
	Gtk::SpinButton*	b_Cvanal;
	Gtk::SpinButton*	d_Cvanal;
	Gtk::SpinButton*	s_Hetro;
	Gtk::SpinButton*	f_Hetro;
	Gtk::SpinButton*	b_Hetro;
	Gtk::SpinButton*	n_Hetro;
	Gtk::SpinButton*	Max_Hetro;
	Gtk::SpinButton*	d_Hetro;
	Gtk::SpinButton*	c_Hetro;
	Gtk::SpinButton*	h_Hetro;
	Gtk::SpinButton*	min_Hetro;
	Gtk::SpinButton*	l_Hetro;

	Gtk::SpinButton*	s_Lpanal;
	Gtk::SpinButton*	p_Lpanal;
	Gtk::SpinButton*	b_Lpanal;
	Gtk::SpinButton*	Max_Lpanal;
	Gtk::SpinButton*	Min_Lpanal;
	Gtk::SpinButton*	d_Lpanal;
	Gtk::SpinButton*	c_Lpanal;
	Gtk::SpinButton*	h_Lpanal;
	Gtk::CheckButton*	a_Lpanal;
	Gtk::Entry*			Comments_Lpanal;

	Gtk::SpinButton*	s_Pvanal;
	Gtk::SpinButton*	b_Pvanal;
	Gtk::SpinButton*	d_Pvanal;
	Gtk::SpinButton*	c_Pvanal;

	Gtk::Button*		buttonBrowseFile;
	Gtk::Button*		buttonClear;
	Gtk::Button*		buttonBrowsePath;
	Gtk::Button*		buttonReset;
	Gtk::Button*		buttonStopBatch;
	//Gtk::Button*		buttonPause;
	Gtk::Button*		buttonStartAnalysis;

	Gtk::ComboBoxText*  w_Atsa;
	Gtk::ComboBoxText*  F_File_Atsa;
	Gtk::ComboBoxText*  v_Lpanal;
	Gtk::ComboBoxText*  n_Pvanal;
	Gtk::ComboBoxText*  w_Pvanal;
	Gtk::ComboBoxText*  h_Pvanal;
	Gtk::ComboBoxText*  window_Pvanal;

	Gtk::CheckButton*   checkbuttonSDIF;
	Gtk::CheckButton*   checkbuttonPvocEx;
	


	

protected:

private:
	wxMain*				mOwner;
	wxTerminal*			compiler;
	wxBrowser*			help;

	gint				mCurrentIndex;
    bool				mStopBatch;

	void clearStatubarInfo();
	void setStatubarInfo(Glib::ustring text);

};

#endif // _WX_ANALYSIS_H_
