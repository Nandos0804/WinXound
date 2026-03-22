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

class wxEditor;
class wxMain;
class wxTextEditor;
class wxIntellitip;
class wxFindCodeStructure;
class wxPosition;

//#include "wx-main.h"
//#include "wx-textEditor.h"
//#include "wx-intellitip.h"

//#include "../scintilla/Scintilla.h"
//#include "../scintilla/SciLexer.h"
//#include "../scintilla/ScintillaWidget.h"


#ifndef _WX_EDITOR_H_
#define _WX_EDITOR_H_



class wxEditor: public Gtk::HPaned 
{
public:
	wxEditor();
	virtual ~wxEditor();

	wxTextEditor*			textEditor;
	Gtk::TreeView*			treeViewStructure;
	Gtk::ListViewText*		listBoxBookmarks;
	Gtk::Toolbar*			toolbarOrcSco;
	wxIntellitip*			intelliTip;
	Glib::ustring			FileName;
	Glib::ustring			CurrentWordsList;
	
	bool					FileIsReadOnly;
	bool					BookmarksOnLoad;

	void SetOwner(wxMain* mainOwner);
	bool CreateNewEditor();
	const gchar* getFileType();
	void StartExplorerTimer();
	void SetFocus();
	Glib::ustring getCurrentSyntaxLanguage();
	void ConfigureEditorForLua();
	void ConfigureEditorForPython();
	void ConfigureEditorForPythonMixed(GHashTable* Opcodes);
	void ConfigureEditorForCSound(GHashTable* Opcodes, Glib::ustring UdoOpcodesList);
	void ConfigureEditorForText();
	void SciEditSetFontsAndStyles();
	bool getShowExplorer();
	void setShowExplorer(bool value);
	bool getShowIntelliTip();
	void setShowIntelliTip(bool value);
	void SetIntelliTip(const gchar* Title, const gchar* Params);
	bool TimerSearch_Tick();
	void FindCodeStructureWorkCompleted();//Glib::RefPtr<Gtk::TreeStore> RefTreeModel);
	void Comment();
	void UnComment();
	void InsertRemoveBookmark();
	void InsertBookmarkAt(gint lineNumber);
	void RemoveAllBookmarks();
	void GoToNextBookmark();
	void GoToPreviousBookmark();
	bool HasBookmarks();
	void RefreshListBoxBookmarks();
	void RefreshExplorer();

	void StoreCursorPos();
	void StoreCursorPos(gint position);
	void GoToPreviousPos();
	void GoToNextPos();

	void SetCurrentOrcScoFile(Glib::ustring OrcScoFilename);
	Glib::ustring GetCurrentOrcScoFile();
	void ShowOrcScoPanel(bool value);


	

	//SIGNALS
	//signal accessor:
	//string mErrorLine, string mWaveFile
	typedef sigc::signal<void>		type_signal_save_point_left;
	type_signal_save_point_left		signal_save_point_left(){return m_signal_save_point_left;};
	
	typedef sigc::signal<void>		type_signal_save_point_reached;
	type_signal_save_point_reached	signal_save_point_reached(){return m_signal_save_point_reached;};

	typedef sigc::signal<void, wxEditor*>	type_signal_update_ui;
	type_signal_update_ui		    signal_update_ui(){return m_signal_update_ui;};

	typedef sigc::signal<void, wxEditor*>	type_signal_zoom_notify;
	type_signal_zoom_notify		    signal_zoom_notify(){return m_signal_zoom_notify;};

	typedef sigc::signal<void, wxEditor*>	type_signal_modified;
	type_signal_modified			signal_modified(){return m_signal_modified;};

	typedef sigc::signal<void, wxEditor*, Glib::ustring, gint>	type_signal_uri_dropped;
	type_signal_uri_dropped			signal_uri_dropped(){return m_signal_uri_dropped;};

	typedef sigc::signal<void, wxEditor*, GdkEventButton*>	type_signal_mouse_down;
	type_signal_mouse_down			signal_mouse_down(){return m_signal_mouse_down;};

	typedef sigc::signal<void, wxEditor*, GdkEventKey*, const gchar*>	type_signal_autocompletion_synopsis;
	type_signal_autocompletion_synopsis		signal_autocompletion_synopsis(){return m_signal_autocompletion_synopsis;};

	typedef sigc::signal<void, wxEditor*>	type_signal_mod_container;
	type_signal_mod_container		signal_mod_container(){return m_signal_mod_container;};

	typedef sigc::signal<void, bool>		type_signal_editor_focus;
	type_signal_editor_focus		signal_editor_focus(){return m_signal_editor_focus;};

	typedef sigc::signal<void>		type_signal_orcsco_show_list;
	type_signal_orcsco_show_list	signal_orcsco_show_list(){return m_signal_orcsco_show_list;};

	typedef sigc::signal<void>		type_signal_switch_orcsco;
	type_signal_switch_orcsco		signal_switch_orcsco(){return m_signal_switch_orcsco;};
	
	
protected:	

	Gtk::VBox*				vbox;
	Gtk::ScrolledWindow*	mScrolledWindow;
	Gtk::ScrolledWindow*	mScrolledWindowlistBoxBookmarks;
	Gtk::VPaned*			VPane;
	Gtk::Label*				labelOrcScoName;
	Gtk::ToolItem*			toolItemLabel;
	
	void SavePointLeft();
	void SavePointReached();
	void UpdateUI(wxEditor* editor);
	void ZoomNotify(wxEditor* editor);
	void Modified(wxEditor* editor);
	void UriDropped(wxEditor* editor, Glib::ustring filename, gint view);
	bool on_editor_focus_in_event(GdkEventFocus* event);
	bool on_editor_focus_out_event(GdkEventFocus* event);
	type_signal_save_point_left			m_signal_save_point_left;
	type_signal_save_point_reached		m_signal_save_point_reached;
	type_signal_update_ui				m_signal_update_ui;
	type_signal_zoom_notify				m_signal_zoom_notify;
	type_signal_modified				m_signal_modified;
	type_signal_uri_dropped				m_signal_uri_dropped;
	type_signal_mouse_down				m_signal_mouse_down;
	type_signal_autocompletion_synopsis	m_signal_autocompletion_synopsis;
	type_signal_mod_container			m_signal_mod_container;
	type_signal_editor_focus			m_signal_editor_focus;
	type_signal_orcsco_show_list		m_signal_orcsco_show_list;
	type_signal_switch_orcsco			m_signal_switch_orcsco;

private:

	//Scintilla notifications:
    static void on_SCI_NOTIFY(GtkWidget *widget, gint wParam, gpointer lParam, gpointer data);
	void on_SCITextEditor_MouseDown();
	void on_SCITextEditor_TextChanged();
	void on_SCITextEditor_ModContainer(gint token);
	bool on_treeViewStructure_clicked(GdkEventButton* event);
	bool on_listBoxBookmarks_clicked(GdkEventButton* event);

	bool on_editor_button_press_before(GdkEventButton* event);
	bool on_editor_button_press_after(GdkEventButton* event);
	bool on_editor_button_release(GdkEventButton* event);
	bool on_editor_key_press_before(GdkEventKey* event);

	void on_toolbuttonShowList_Clicked();
	void on_toolbuttonBrowseOrcSco_Clicked();
	void on_toolbuttonClearOrcSco_Clicked();
	void on_toolbuttonSwitchOrcSco_Clicked();
	
	Glib::ustring ParseLine(Glib::ustring Text);
	
	wxMain*					owner;
	std::string				mLanguage;
	gint					timerCounter;
	bool					isChanged;
	wxFindCodeStructure*	findStructure;
	wxPosition*				mPosition;
	//GTimer*				TimerSearch;
	//GHashTable* Opcodes;

};

#endif // _WX_EDITOR_H_
