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

#include <iostream>

#include "wx-editor.h"
#include "wx-main.h"
#include "wx-textEditor.h"
#include "wx-intellitip.h"
#include "wx-settings.h"
#include "wx-global.h"
#include "wx-findcodestructure.h"
#include "wx-position.h"

#include "../scintilla/Scintilla.h"
#include "../scintilla/SciLexer.h"
#include "../scintilla/ScintillaWidget.h"

#define SSM(sci, m, w, l) scintilla_send_message(sci, m, w, l)

#define FIND_STRUCTURE_INTERVAL 6


wxEditor::wxEditor() 	
{
	FileIsReadOnly = false;
	BookmarksOnLoad = true;
	FileName = "";
	CurrentWordsList = "";
	timerCounter = 0;
	isChanged = false;

	mPosition = new wxPosition();

	this->CreateNewEditor();
	
	findStructure = new wxFindCodeStructure(this, textEditor);
	findStructure->sig_done.connect(
		sigc::mem_fun(*this, &wxEditor::FindCodeStructureWorkCompleted));

	FindCodeStructureWorkCompleted();

	std::cerr << "wxEditor Created" << std::endl;
}
wxEditor::~wxEditor(void)
{	
	/*
	Gtk::VBox*				vbox;
	Gtk::ScrolledWindow*	mScrolledWindow;
	Gtk::ScrolledWindow*	mScrolledWindowlistBoxBookmarks;
	Gtk::VPaned*			VPane;
	*/
	
	delete findStructure;

	//ORC-SCO TOOLBAR
	delete toolbarOrcSco;
	toolbarOrcSco = NULL;
	//TREEVIEW
	delete treeViewStructure;
	treeViewStructure = NULL;
	delete mScrolledWindow;
	mScrolledWindow = NULL;
	//BOOKMARKS
	delete listBoxBookmarks;
	listBoxBookmarks = NULL;
	delete mScrolledWindowlistBoxBookmarks;
	mScrolledWindowlistBoxBookmarks = NULL;
	//VPANE
	delete VPane;
	VPane = NULL;

	//TEXTEDITOR
	delete textEditor;
	textEditor = NULL;
	//INTELLITIP
	delete intelliTip;
	intelliTip = NULL;
	//VBOX
	delete vbox;
	vbox = NULL;

	//WXPOSITION
	delete mPosition;
	mPosition = NULL;

	std::cout << "wxEditor Released" << std::endl;
}

void wxEditor::SetOwner(wxMain* mainOwner)
{
	owner = mainOwner;
}




bool wxEditor::on_editor_button_press_before(GdkEventButton* event)
{
	//std::cerr << "wxEditor BUTTON PRESSED BEFORE!!!" << std::endl;
	
	//If modifiers are Command+Alt we must trap the mouseDown message (this is needed for String selection).
	//We don't send the message to Scintilla because, if we select the string, Scintilla will cancel 
	//our selection when receive the MouseDown signal.
	//So we only notify parent about SCILeftMouseSelectQuotes message and after return without processing
    //the "ScintillaGTK->PressThis" method with the "return true";
	if(event->button == 1 && 
	   event->state == (GDK_CONTROL_MASK | GDK_MOD1_MASK))
	{
		m_signal_mouse_down.emit(this, event);
		return true;
	}
	
	return false;
}

bool wxEditor::on_editor_button_press_after(GdkEventButton* event)
{
	//std::cerr << "wxEditor BUTTON PRESSED AFTER!!!" << std::endl;
	
	//STORE CURSOR POSITION
	mPosition->StoreCursorPos(textEditor->getCaretPosition());
	
	m_signal_mouse_down.emit(this, event);
	return false;
}

bool wxEditor::on_editor_button_release(GdkEventButton* event)
{
	//std::cerr << "wxEditor BUTTON RELEASED!!!" << std::endl;
	return false;
}

bool wxEditor::on_editor_focus_in_event(GdkEventFocus* event)
{
	//wxGLOBAL->DebugPrint("TEXTEDITOR FOCUS IN");
	m_signal_editor_focus(TRUE);
	return false;
}

bool wxEditor::on_editor_focus_out_event(GdkEventFocus* event)
{
	//wxGLOBAL->DebugPrint("TEXTEDITOR FOCUS OUT");
	m_signal_editor_focus(FALSE);
	return false;
}




bool wxEditor::CreateNewEditor()
{

	//Add TextEditor + Intellitip Pane:
	textEditor = new wxTextEditor();
	intelliTip = new wxIntellitip();
	intelliTip->set_can_focus(FALSE);


	///////////////////////////////////
	//OrcSco Toolbar:
	toolbarOrcSco = new Gtk::Toolbar();
	//Toolbar Label title
	Gtk::Label* linkTitle = Gtk::manage(new Gtk::Label("Linked with: "));
	Gtk::ToolItem* toolItemLabelTitle = Gtk::manage(new Gtk::ToolItem());
	toolItemLabelTitle->add(*linkTitle);
	toolItemLabelTitle->set_expand(FALSE);
	toolItemLabelTitle->set_is_important(TRUE);
	//Toolbar Label OrcScoName !!!
	labelOrcScoName = Gtk::manage(new Gtk::Label("", 0.0, 0.5, false)); //TODO: MANAGE OR NOT ???;
	toolItemLabel = Gtk::manage(new Gtk::ToolItem()); //TODO: MANAGE OR NOT ???;
	toolItemLabel->add(*labelOrcScoName);
	toolItemLabel->set_expand(TRUE);
	toolItemLabel->set_is_important(TRUE);
	//Toolbar buttons
	Gtk::ToolButton* toolbuttonShowList = Gtk::manage(new Gtk::ToolButton(Gtk::Stock::INDEX));
	toolbuttonShowList->set_label("Select from list");
	Gtk::ToolButton* toolbuttonBrowseOrcSco = Gtk::manage(new Gtk::ToolButton(Gtk::Stock::OPEN));
	toolbuttonBrowseOrcSco->set_label("Browse Orc/Sco file");
	Gtk::ToolButton* toolbuttonClearOrcSco = Gtk::manage(new Gtk::ToolButton(Gtk::Stock::CLEAR));
	toolbuttonClearOrcSco->set_label("Clear current Link");
	Gtk::SeparatorToolItem* toolSeparator = Gtk::manage(new Gtk::SeparatorToolItem());
	Gtk::ToolButton* toolbuttonSwitchOrcSco = Gtk::manage(new Gtk::ToolButton(Gtk::Stock::COPY));
	toolbuttonSwitchOrcSco->set_label("Open/Switch Orc/Sco");
	//Toolbar Toolbutton Events:
	toolbuttonShowList->signal_clicked().connect(
		sigc::mem_fun(*this, &wxEditor::on_toolbuttonShowList_Clicked));
	toolbuttonBrowseOrcSco->signal_clicked().connect(
		sigc::mem_fun(*this, &wxEditor::on_toolbuttonBrowseOrcSco_Clicked));
	toolbuttonClearOrcSco->signal_clicked().connect(
		sigc::mem_fun(*this, &wxEditor::on_toolbuttonClearOrcSco_Clicked));
	toolbuttonSwitchOrcSco->signal_clicked().connect(
		sigc::mem_fun(*this, &wxEditor::on_toolbuttonSwitchOrcSco_Clicked));

	toolbuttonShowList->set_tooltip_markup("Select from list");
	toolbuttonBrowseOrcSco->set_tooltip_markup("Browse Orc/Sco file");
	toolbuttonClearOrcSco->set_tooltip_markup("Clear current Link");
	toolbuttonSwitchOrcSco->set_tooltip_markup("Open/Switch Orc/Sco");

	toolbarOrcSco->append(*toolItemLabelTitle);
	toolbarOrcSco->append(*toolItemLabel);
	toolbarOrcSco->append(*toolbuttonShowList);
	toolbarOrcSco->append(*toolbuttonBrowseOrcSco);
	toolbarOrcSco->append(*toolbuttonClearOrcSco);
	toolbarOrcSco->append(*toolSeparator);
	toolbarOrcSco->append(*toolbuttonSwitchOrcSco);

	toolbarOrcSco->set_toolbar_style(Gtk::TOOLBAR_ICONS);
	toolbarOrcSco->set_icon_size(Gtk::ICON_SIZE_SMALL_TOOLBAR);
	toolbarOrcSco->show_all();
	///////////////////////////////////

		
	vbox = new Gtk::VBox();
	vbox->pack_start(*toolbarOrcSco, FALSE, FALSE, 0);
	vbox->pack_start(*textEditor, TRUE, TRUE, 0);
	vbox->pack_start(*intelliTip, FALSE, FALSE, 0);
	vbox->set_size_request(200,-1);
	
	
	////this->add2(*editor);
	//this->add2(*vbox);
	//RESIZE, SHRINK
	this->pack2(*vbox, TRUE, FALSE);

	//Add explorer pane:
	//TreeViewStructure
	treeViewStructure = new Gtk::TreeView();
	treeViewStructure->set_can_focus(FALSE);
	mScrolledWindow = new Gtk::ScrolledWindow();
	mScrolledWindow->add(*treeViewStructure);
	//Only show the scrollbars when they are necessary:
	mScrolledWindow->set_policy(Gtk::POLICY_AUTOMATIC, Gtk::POLICY_AUTOMATIC);

	//ListViewText (guint columns_count, bool editable=false, Gtk::SelectionMode mode=Gtk::SELECTION_SINGLE)
	listBoxBookmarks = new Gtk::ListViewText(1, false, Gtk::SELECTION_SINGLE);	
	listBoxBookmarks->set_can_focus(FALSE);
	mScrolledWindowlistBoxBookmarks = new Gtk::ScrolledWindow();
	mScrolledWindowlistBoxBookmarks->add(*listBoxBookmarks);
	//Only show the scrollbars when they are necessary:
	mScrolledWindowlistBoxBookmarks->set_policy(Gtk::POLICY_AUTOMATIC, Gtk::POLICY_AUTOMATIC);
	
	VPane = new Gtk::VPaned();
	VPane->set_size_request(200,-1);
	this->pack1(*VPane, FALSE, FALSE); //OLD:TRUE,FALSE
	this->set_position(200);
	VPane->pack1(*mScrolledWindow, TRUE, FALSE);
	VPane->pack2(*mScrolledWindowlistBoxBookmarks, TRUE, FALSE); //TRUE,FALSE
	treeViewStructure->set_size_request(-1,20);
	listBoxBookmarks->set_size_request(-1,40);
	mScrolledWindowlistBoxBookmarks->set_size_request(-1,40);
	listBoxBookmarks->set_column_title(0,"Bookmarks");

	VPane->show();
	

	g_signal_connect(textEditor->getPrimaryView(), 
	                 SCINTILLA_NOTIFY, 
	                 G_CALLBACK(&wxEditor::on_SCI_NOTIFY),
	                 this);
	g_signal_connect(textEditor->getSecondaryView(), 
	                 SCINTILLA_NOTIFY, 
	                 G_CALLBACK(&wxEditor::on_SCI_NOTIFY),
	                 this);



	
	//menuItemFileInfo->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuItemFileInfo_Clicked));
	treeViewStructure->signal_button_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_treeViewStructure_clicked), false);

	listBoxBookmarks->signal_button_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_listBoxBookmarks_clicked), false);



	//TEXTEDITOR MOUSE DOWN (BUTTON PRESS)
	textEditor->get_textView1_Widget()->signal_button_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_button_press_before), false);
	textEditor->get_textView1_Widget()->signal_button_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_button_press_after), true);
	//textEditor->get_textView1_Widget()->signal_button_release_event().connect(
	//	sigc::mem_fun(*this, &wxEditor::on_editor_button_release));

	textEditor->get_textView2_Widget()->signal_button_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_button_press_before), false);
	textEditor->get_textView2_Widget()->signal_button_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_button_press_after), true);
	//textEditor->get_textView2_Widget()->signal_button_release_event().connect(
	//	sigc::mem_fun(*this, &wxEditor::on_editor_button_release));


	//TEXTEDITOR KEYPRESS EVENT
	textEditor->get_textView1_Widget()->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_key_press_before), false);
	textEditor->get_textView2_Widget()->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_key_press_before), false);

	
	//TEXTEDITOR FOCUS EVENT
	textEditor->get_textView1_Widget()->signal_focus_in_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_focus_in_event));
	textEditor->get_textView1_Widget()->signal_focus_out_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_focus_out_event));
	textEditor->get_textView2_Widget()->signal_focus_in_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_focus_in_event));
	textEditor->get_textView2_Widget()->signal_focus_out_event().connect(
		sigc::mem_fun(*this, &wxEditor::on_editor_focus_out_event));


	show_all_children(); 
	show();
	
	return true;
}


void wxEditor::on_toolbuttonShowList_Clicked()
{
	m_signal_orcsco_show_list.emit();
}
void wxEditor::on_toolbuttonBrowseOrcSco_Clicked()
{
	//Open document
	Gtk::FileChooserDialog dialog("Open File",
	                              Gtk::FILE_CHOOSER_ACTION_OPEN);
	//dialog.set_transient_for(*this);

	//Add response buttons the the dialog:
	dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
	dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

	dialog.set_select_multiple(FALSE);
	dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

	Glib::ustring extension = 
		Glib::str_has_suffix(FileName.lowercase(),".orc") ? "*.sco" : "*.orc";
	
	//Add filters, so that only certain file types can be selected:
	Glib::RefPtr<Gtk::FileFilter> filter_csound_files = Gtk::FileFilter::create();
	filter_csound_files->set_name("CSound files");
	filter_csound_files->add_pattern(extension);
	dialog.add_filter(filter_csound_files);

	Glib::RefPtr<Gtk::FileFilter> filter_any = Gtk::FileFilter::create();
	filter_any->set_name("Any files");
	filter_any->add_pattern("*");
	dialog.add_filter(filter_any);

	//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
	if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
	                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
	{
		dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
	}
	

	int result = dialog.run();

	//Handle the response:
	if(result == Gtk::RESPONSE_OK)
	{
		wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

		//OLD:Single file
		std::string filename = dialog.get_filename();

		this->SetCurrentOrcScoFile(filename);
	}

	this->SetFocus();
}

void wxEditor::on_toolbuttonClearOrcSco_Clicked()
{
	this->SetCurrentOrcScoFile("");
}

void wxEditor::on_toolbuttonSwitchOrcSco_Clicked()
{
	m_signal_switch_orcsco.emit();
}

void wxEditor::ShowOrcScoPanel(bool value)
{
	toolbarOrcSco->set_visible(value);
}


void wxEditor::SetCurrentOrcScoFile(Glib::ustring OrcScoFilename)
{
	labelOrcScoName->set_text(OrcScoFilename);
	//labelOrcScoName->set_markup(OrcScoFilename);
	toolItemLabel->set_tooltip_markup(OrcScoFilename);
}

Glib::ustring wxEditor::GetCurrentOrcScoFile()
{
	return labelOrcScoName->get_text();
}

bool wxEditor::on_editor_key_press_before(GdkEventKey* event)
{
	
	if(textEditor->AutocActive() && 
	   event->keyval == GDK_KEY_Tab)
	{
		textEditor->AutocComplete();
		textEditor->AddText("\t");
		return true;
	}
	else if(textEditor->AutocActive() && 
	        event->keyval == GDK_KEY_Return &&
	        event->state == GDK_SHIFT_MASK)
	{
		//Raise event for Synopsis insertion
		Glib::ustring opcode = textEditor->AutocGetCurrentText();
		if(opcode.size() > 0)
			m_signal_autocompletion_synopsis.emit(this, 
			                                      event, 
			                                      opcode.c_str());
		textEditor->AutocCancel();
		return true;
	}

	return false;
}


//g_timeout_add_seconds ();
//guint               g_timeout_add_seconds               (guint interval,
//                                                         GSourceFunc function,
//                                                         gpointer data);
void wxEditor::StartExplorerTimer()
{
	//Glib::signal_timeout().connect(sigc::ptr_fun(&timeout_handler), 1000);
	//Glib::signal_timeout().connect_seconds(sigc::mem_fun(*this, &wxEditor::TimerSearch_Tick), 1);
	Glib::signal_timeout().connect(sigc::mem_fun(*this, &wxEditor::TimerSearch_Tick), 200);
}

bool wxEditor::TimerSearch_Tick()
{
	//wxGLOBAL->DebugPrint("TIMERSEARCH", "TICK");
	
	timerCounter ++;
	if(timerCounter >= FIND_STRUCTURE_INTERVAL)
	{
		timerCounter = 0;
		if(isChanged == true) // && wxGLOBAL->isSyntaxType(this)) //Skip findStructure for Non-Syntax documents
		{
			wxGLOBAL->DebugPrint("TIMERSEARCH", "FINDSTRUCTURE CALLED");
			//[NSThread detachNewThreadSelector:@selector(findStructure:) toTarget:self withObject:[textEditor getText]];
			isChanged = false;
			findStructure->Start();
		}
	}

	return true;
}


void wxEditor::on_SCI_NOTIFY(GtkWidget *widget, gint wParam, gpointer lParam, gpointer data)
{
	//wxTextEditor* _textEditor = reinterpret_cast<wxTextEditor*>(widget);
	ScintillaObject* _textView = reinterpret_cast<ScintillaObject*>(widget);

	wxEditor* _this = reinterpret_cast<wxEditor*>(data);
	SCNotification *nt = reinterpret_cast<SCNotification*>(lParam);

	gint currentPosition = SSM(_textView, SCI_GETCURRENTPOS, 0, 0);
	bool isCharBefore = false;
	bool isCharNext = false;
	int line = 0;

	//std::cout << nt->nmhdr.idFrom << std::endl; //!!!!!!

	switch (nt->nmhdr.code) //  nmhdr.code)
	{
		case SCN_SAVEPOINTLEFT:
		{
			_this->SavePointLeft();
			//std::cerr << "SCN_SAVEPOINTLEFT" << std::endl;
			break;
		}
		case SCN_SAVEPOINTREACHED:
		{
			_this->SavePointReached();
			//std::cerr << "SCN_SAVEPOINTREACHED" << std::endl;
			break;
		}
		case SCN_MODIFYATTEMPTRO:
		{
			//std::cerr << "SCN_MODIFYATTEMPTRO" << std::endl;
			break;
		}
		case SCN_MARGINCLICK:
			//std::cerr << "SCN_MARGINCLICK" << std::endl;
			line = SSM(_textView, SCI_LINEFROMPOSITION, nt->position, 0);
			//std::cerr << "line:" << line << std::endl;
			SSM(_textView, SCI_TOGGLEFOLD, line, 0);
			SSM(_textView, SCI_GOTOPOS, nt->position, 0); //set the caret position to current
			break;

		case SCN_UPDATEUI:
			//void g_signal_emit_by_name (gpointer instance, const gchar *detailed_signal, ...);
			//std::cerr << "SCN_UPDATEUI" << std::endl;
			//_this->owner->onDocumentUpdateUi(_this);
			_this->UpdateUI(_this);
			break;

 		case SCN_MODIFIED:
		{	
			//NOTIFICATION: SC_MOD_CONTAINER->CONVERT_EOL!!!
			if(nt->modificationType & SC_MOD_CONTAINER)
			{
				if(_textView == _this->textEditor->getPrimaryView())
				{
					//std::cerr << "SC_MOD_CONTAINER" << std::endl;
					_this->on_SCITextEditor_ModContainer(nt->token);
					////_this->owner->onZoomNotification(_this);
					_this->ZoomNotify(_this);
				}
			}

			if(nt->modificationType & (SC_MOD_INSERTTEXT | SC_MOD_DELETETEXT))
			{
				if(_textView == _this->textEditor->getPrimaryView())
				{
					//_this->on_SCITextEditor_TextChanged();
					_this->Modified(_this);
					//std::cerr << "SCN_MODIFIED" << std::endl;
				}
			}
			break;
		}
		case SCN_CHARADDED:
			//std::cerr << "SCN_CHARADDED" << std::endl;
			break;

		case SCN_USERLISTSELECTION:
		{
			//std::cerr << "SCN_USERLISTSELECTION" << std::endl;
			break;
		}
		case SCN_AUTOCSELECTION:
			//std::cerr << "SCN_AUTOCSELECTION" << std::endl;
			//LPARAM: lParam - The start position of the word being completed.
			//TEXT: text - The text of the selection.
			//SSM(_textView, SCI_AUTOCCANCEL, 0, 0);
			//std::cerr << nt->lParam << std::endl;			

			gunichar c;

			if(currentPosition > 0)
			{
				c = SSM(_textView, SCI_GETCHARAT, currentPosition - 1, 0);
				isCharBefore = !g_unichar_isspace(c);
			}
			
			if(currentPosition < SSM(_textView, SCI_GETTEXTLENGTH, 0, 0))
			{
				c = SSM(_textView, SCI_GETCHARAT, currentPosition + 1, 0);
				isCharNext = !g_unichar_isspace(c);
			}


			//std::cerr << "isCharBefore: " << isCharBefore
			//	      << "isCharNext: " << isCharNext << std::endl;
			
			if (isCharBefore == false &&
			    isCharNext == true)
            {
				SSM(_textView, SCI_AUTOCCANCEL, 0, 0);
				SSM(_textView, SCI_ADDTEXT, strlen(nt->text), (sptr_t)nt->text);
            }
			else
			{
			
			}

			break;

		case SCN_AUTOCCANCELLED:
		{
			//std::cerr << "SCN_AUTOCCANCELLED" << std::endl;
			break;
		}
		case SCN_NEEDSHOWN:
		{
			//std::cerr << "SCN_NEEDSHOWN" << std::endl;
			break;
		}
		case SCN_URIDROPPED:
		{
			if(_textView == _this->textEditor->getPrimaryView())
				_this->UriDropped(_this, nt->text, 1);
			else
				_this->UriDropped(_this, nt->text, 2);

			//std::cerr << "SCN_URIDROPPED" << nt->text << std::endl;
			break;
		}
		case SCN_CALLTIPCLICK:
		{
			//std::cerr << "SCN_CALLTIPCLICK" << std::endl;
			break;
		}
		case SCN_ZOOM:
		{
			//std::cerr << "SCN_ZOOM" << std::endl;
			
			////_this->owner->onZoomNotification(_this);
			_this->ZoomNotify(_this);
			
			if(_textView == _this->textEditor->getPrimaryView())
				_this->textEditor->refreshMargins(1);
			else
				_this->textEditor->refreshMargins(2);
			
			break;
		}
			
	}

}


////////////////////////////////////////////////////////////////////////////////
// SCINTILLA EVENTS
////////////////////////////////////////////////////////////////////////////////
void wxEditor::SavePointLeft()
{
	m_signal_save_point_left.emit();
}
void wxEditor::SavePointReached()
{
	m_signal_save_point_reached.emit();
}
void wxEditor::UpdateUI(wxEditor* editor)
{
	m_signal_update_ui.emit(editor);
}
void wxEditor::ZoomNotify(wxEditor* editor)
{
	m_signal_zoom_notify.emit(editor);
}
void wxEditor::Modified(wxEditor* editor)
{
	m_signal_modified.emit(editor);

	//-TIMER for FindStructure operation
	timerCounter = 0;
	isChanged = true;	
}
void wxEditor::UriDropped(wxEditor* editor, Glib::ustring filename, gint view)
{
	m_signal_uri_dropped.emit(editor, filename, view);
}


void wxEditor::on_SCITextEditor_ModContainer(gint token)
{
	//if (TextEditorModContainer != null) TextEditorModContainer(sender, e);

	//After Undo/Redo we must check for real line endings to synchronize
	//Scintilla Eol Mode
	gint eolModeReal = textEditor->GetEolModeReal();
	gint eolModeScintilla = textEditor->getEOLMode();

	//std::cerr << "eolModeReal: " << eolModeReal << " - " <<
	//			 "eolModeScintilla: " << eolModeScintilla << std::endl;

	if (eolModeReal != textEditor->getEOLMode())
	{
		textEditor->setEolMode(eolModeReal);
	}

	if(token == 2)
		m_signal_mod_container.emit(this);
}



////////////////////////////////////////////////////////////////////////////////


const gchar* wxEditor::getFileType()
{

	//Set Highlight language
	//if (textEditor.FileName.ToLower().EndsWith(".py") ||
	//    textEditor.FileName.ToLower().EndsWith(".pyw"))
	if(Glib::str_has_suffix(FileName.lowercase(), ".py") ||
	   Glib::str_has_suffix(FileName.lowercase(), ".pyw"))
	{
		//Python
		return "python";

	}
	//else if (textEditor.FileName.ToLower().EndsWith(".lua"))
	else if (Glib::str_has_suffix(FileName.lowercase(), ".lua"))
	{
		//Lua
		return "lua";
	}
	//else if (textEditor.FileName.ToLower().EndsWith(".csd"))
	else if (Glib::str_has_suffix(FileName.lowercase(), ".csd"))
	{
		//Csound
		return "csound";
	}

	return "none";
	
}

Glib::ustring wxEditor::getCurrentSyntaxLanguage()
{
	return mLanguage.c_str();
}


void wxEditor::SetFocus()
{
	this->textEditor->setFocus();
}


void wxEditor::ConfigureEditorForLua()
{

	try
	{
		mLanguage = "lua";
		CurrentWordsList = "";

		//Set Keywords list
		std::string keywords = 
			"and break do else elseif "
			"end false for function if "
			"in local nil not or repeat return "
			"then true until while ";

		std::string keywords2 = 
			"_VERSION assert collectgarbage dofile "
			"error gcinfo loadfile loadstring "
			"print rawget rawset require tonumber "
			"tostring type unpack ";

		std::string keywords3 = "";
		std::string keywords4 = "";

		//Add Lua 4:
		keywords2.append( 
			"_ALERT _ERRORMESSAGE _INPUT _PROMPT _OUTPUT "
			"_STDERR _STDIN _STDOUT call dostring foreach "
			"foreachi getn globals newtype"
			"sort tinsert tremove ");

		keywords3.append("abs acos asin atan atan2 ceil cos deg exp "
			"floor format frexp gsub ldexp log log10 max "
			"min mod rad random randomseed "
			"sin sqrt strbyte strchar strfind strlen "
			"strlower strrep strsub strupper tan ");

		keywords4.append("openfile closefile readfrom writeto appendto "
			"remove rename flush seek tmpfile tmpname read write "
			"clock date difftime execute exit getenv setlocale time ");

		//Add Lua 5:
		keywords2.append("_G getfenv getmetatable ipairs loadlib next pairs pcall "
			"rawequal setfenv setmetatable xpcall "
			"string table math coroutine io os debug "
			"load module select ");

		keywords3.append("string.byte string.char string.dump string.find "
			"string.len string.lower string.rep string.sub "
			"string.upper string.format string.gfind string.gsub "
			"table.concat table.foreach table.foreachi table.getn "
			"table.sort table.insert table.remove table.setn "
			"math.abs math.acos math.asin math.atan math.atan2 "
			"math.ceil math.cos math.deg math.exp "
			"math.floor math.frexp math.ldexp math.log "
			"math.log10 math.max math.min math.mod "
			"math.pi math.pow math.rad math.random math.randomseed "
			"math.sin math.sqrt math.tan string.gmatch string.match "
			"string.reverse table.maxn math.cosh math.fmod "
			"math.modf math.sinh math.tanh math.huge ");

		keywords4.append("coroutine.create coroutine.resume coroutine.status "
			"coroutine.wrap coroutine.yield io.close io.flush "
			"io.input io.lines io.open io.output io.read io.tmpfile "
			"io.type io.write io.stdin io.stdout io.stderr "
			"os.clock os.date os.difftime os.execute os.exit "
			"os.getenv os.remove os.rename "
			"os.setlocale os.time os.tmpname "
			"coroutine.running package.cpath package.loaded "
			"package.loadlib package.path package.preload "
			"package.seeall io.popen ");


		textEditor->setKeyWords(0, keywords.c_str());
		textEditor->setKeyWords(1, keywords2.c_str());
		textEditor->setKeyWords(2, keywords3.c_str());
		textEditor->setKeyWords(3, keywords4.c_str());

		this->SciEditSetFontsAndStyles();
		//this->SciEditSetFontsAndStyles(textEditor.PrimaryView);
		//this->SciEditSetFontsAndStyles(textEditor.SecondaryView);

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - ConfigureEditorForLua Error");
	}

}

void wxEditor::ConfigureEditorForPython()
{

	try
	{
		mLanguage = "python";
		CurrentWordsList = "";

		//Set Keywords list
		std::string keywords = 
			"and as assert break class continue def del elif "
			"else except exec finally for from global if import "
			"in is lambda None not or pass print raise return "
			"try while with yield";
		textEditor->setKeyWords(0, keywords.c_str());

		this->SciEditSetFontsAndStyles();
		//this.SciEditSetFontsAndStyles(textEditor.PrimaryView);
		//this.SciEditSetFontsAndStyles(textEditor.SecondaryView);

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - ConfigureEditorForPython Error");
	}

}

void wxEditor::ConfigureEditorForPythonMixed(GHashTable* Opcodes)
{

	try
	{
		mLanguage = "python";

		//Set Keywords list
		std::string keywords = "and as assert break class continue def del elif "
			"else except exec finally for from global if import "
			"in is lambda None not or pass print raise return "
			"try while with yield";
		textEditor->setKeyWords(0, keywords.c_str());

		if (Opcodes != NULL)
		{	
			CurrentWordsList = "";
			
			//Set Keywords list
			//Glib::ustring KeyWordList = "";
			std::string KeyWordList = "";
			//foreach (DictionaryEntry de in Opcodes)
			GHashTableIter iter;
			gpointer key, value;
			g_hash_table_iter_init (&iter, Opcodes);
			while (g_hash_table_iter_next (&iter, &key, &value)) 
			{
				/* do something with key and value */
				KeyWordList.append((gchar*)key);
				KeyWordList.append(" ");
			}		
			textEditor->setKeyWords(1, KeyWordList.c_str());
		}

		this->SciEditSetFontsAndStyles();
		//this.SciEditSetFontsAndStyles(textEditor.PrimaryView);
		//this.SciEditSetFontsAndStyles(textEditor.SecondaryView);

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - ConfigureEditorForPythonMixed Error");
	}

}

void wxEditor::ConfigureEditorForCSound(GHashTable* Opcodes, Glib::ustring UdoOpcodesList)
{
	try
	{
		mLanguage = "csound";
		
		if (Opcodes != NULL)
		{
			CurrentWordsList = "";
			
			//Set Keywords list
			//Glib::ustring KeyWordList = "";
			std::string KeyWordList = "";

			//foreach (DictionaryEntry de in Opcodes)
			GHashTableIter iter;
			gpointer key, value;
			g_hash_table_iter_init (&iter, Opcodes);
			while (g_hash_table_iter_next (&iter, &key, &value)) 
			{
				/* do something with key and value */
				//if (de.Key.ToString() != "instr" && de.Key.ToString() != "endin")
				if(!g_str_equal(key, "instr") &&
				   !g_str_equal(key, "endin"))
				{
					KeyWordList.append((gchar*)key);
					KeyWordList.append(" ");
				}
			}


			//CABBAGE
			Glib::ustring CabbageWords = "channel pos caption size value  "
										 "onoffCaption items colour ";
			Glib::ustring CabbageWidgets = "form rslider hslider vslider "
										   "button checkbox combobox groupbox keyboard ";

			KeyWordList.append(CabbageWords);
			CabbageWidgets.append(UdoOpcodesList);
			
			this->textEditor->setKeyWords(0, KeyWordList.c_str());
			this->textEditor->setKeyWords(2, CabbageWidgets.c_str());


			Glib::ustring TagWordList = 
				"<CsVersion> </CsVersion> "
				"<CsoundSynthesizer> </CsoundSynthesizer> "
				"<CsOptions> </CsOptions> "
				"<CsInstruments> </CsInstruments> "
				"<CsScore> </CsScore> "
				"<CsVersion> </CsVersion> "
				"<CsLicence> </CsLicence> "
				"<Cabbage> </Cabbage>";
			
			this->textEditor->setKeyWords(1, TagWordList.c_str());

			this->textEditor->setKeyWords(3, " instr endin ");

			
			CurrentWordsList.append(KeyWordList);
			CurrentWordsList.append("instr endin ");
			CurrentWordsList.append(TagWordList);

	
			std::vector<std::string> wordList;
			gchar** list = g_strsplit(CurrentWordsList.c_str(), " ",0);
			//std::sort (wordList.begin(), wordList.end());
			int length = wxGLOBAL->ArrayLength(list);
			//std::sort(&list[0], &list[length]);

			for(int i=0; i < length; i++)
			{
				wordList.push_back(list[i]);
			}
			std::sort (wordList.begin(), wordList.end());

			CurrentWordsList = "";
			for(int i=0; i < length; i++)
			{
				if(wordList[i] != "")
				{
					CurrentWordsList.append(wordList[i]);
					if(i < (length - 1))
						CurrentWordsList.append(" ");
				}
			}	

			//CurrentWordsList = wxGLOBAL->TrimRight(CurrentWordsList);
			
			g_strfreev (list);

			
		}

		this->textEditor->setWordChars("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_");

		this->SciEditSetFontsAndStyles();

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - ConfigureEditorForCSound Error");
	}
}




void wxEditor::ConfigureEditorForText()
{
	mLanguage = "none";
}

void wxEditor::SciEditSetFontsAndStyles()
{

	if (mLanguage == "none") return;


	//Set Default Font (name and size) to styles 0 -> 32
	textEditor->setTextEditorFont(wxSETTINGS->EditorProperties.DefaultFontName.c_str(),
	                              wxSETTINGS->EditorProperties.DefaultFontSize);



	//mSciEdit.SetZoom(0);
	textEditor->setZoom(0);
	//mSciEdit.SetTabWidth(wxGlobal.Settings.EditorProperties.DefaultTabSize);
	textEditor->setTabIndent(wxSETTINGS->EditorProperties.DefaultTabSize);
	//textEditor->setPasteConvertEndings(true);   //???

	//Set styles from 0 to 33
	//STYLE_NUMBERS_MARGINS = 33;
	
	for (gint i = 0; i < 34; i++)
	{
		//const gchar* temp = wxSETTINGS->StyleGetForeColor(mLanguage, i).c_str();
		textEditor->StyleSetFore(i, wxSETTINGS->StyleGetForeColor(mLanguage, i).c_str());
		textEditor->StyleSetBack(i, wxSETTINGS->StyleGetBackColor(mLanguage, i).c_str());
		textEditor->StyleSetBold(i, wxSETTINGS->StyleGetBold(mLanguage, i));
		textEditor->StyleSetItalic(i, wxSETTINGS->StyleGetItalic(mLanguage, i));
	}


	//INDENT MARGINS
	//Set Indent Margins (same color as number margin)
	//mSciEdit.SetFoldMarginColour(true,wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, 33));
	//mSciEdit.SetFoldMarginHiColour(true,wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, 33));
	textEditor->SetFoldMarginColour(true, wxSETTINGS->StyleGetBackColor(mLanguage, 33).c_str());
	textEditor->SetFoldMarginHiColour(true, wxSETTINGS->StyleGetBackColor(mLanguage, 33).c_str());

	//Indent Symbols colors
	//FORE
	//mSciEdit.MarkerSetFore((int)SciConst.SC_MARKNUM_FOLDER,wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, 33));
	//mSciEdit.MarkerSetFore((int)SciConst.SC_MARKNUM_FOLDEROPEN,wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, 33));
	//mSciEdit.MarkerSetFore((int)SciConst.SC_MARKNUM_FOLDERSUB,wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, 33));
	//mSciEdit.MarkerSetFore((int)SciConst.SC_MARKNUM_FOLDERTAIL,wxGlobal.Settings.EditorProperties.StyleGetBackColor(mLanguage, 33));
	textEditor->MarkerSetFore(SC_MARKNUM_FOLDER, wxSETTINGS->StyleGetBackColor(mLanguage, 33).c_str());
	textEditor->MarkerSetFore(SC_MARKNUM_FOLDEROPEN, wxSETTINGS->StyleGetBackColor(mLanguage, 33).c_str());
	textEditor->MarkerSetFore(SC_MARKNUM_FOLDERSUB, wxSETTINGS->StyleGetBackColor(mLanguage, 33).c_str());
	textEditor->MarkerSetFore(SC_MARKNUM_FOLDERTAIL, wxSETTINGS->StyleGetBackColor(mLanguage, 33).c_str());

	//BACK
	//mSciEdit.MarkerSetBack((int)SciConst.SC_MARKNUM_FOLDER,wxGlobal.Settings.EditorProperties.StyleGetForeColor(mLanguage, 33));
	//mSciEdit.MarkerSetBack((int)SciConst.SC_MARKNUM_FOLDEROPEN,wxGlobal.Settings.EditorProperties.StyleGetForeColor(mLanguage, 33));
	//mSciEdit.MarkerSetBack((int)SciConst.SC_MARKNUM_FOLDERSUB,wxGlobal.Settings.EditorProperties.StyleGetForeColor(mLanguage, 33));
	//mSciEdit.MarkerSetBack((int)SciConst.SC_MARKNUM_FOLDERTAIL,wxGlobal.Settings.EditorProperties.StyleGetForeColor(mLanguage, 33));
	textEditor->MarkerSetBack(SC_MARKNUM_FOLDER, wxSETTINGS->StyleGetForeColor(mLanguage, 33).c_str());
	textEditor->MarkerSetBack(SC_MARKNUM_FOLDEROPEN, wxSETTINGS->StyleGetForeColor(mLanguage, 33).c_str());
	textEditor->MarkerSetBack(SC_MARKNUM_FOLDERSUB, wxSETTINGS->StyleGetForeColor(mLanguage, 33).c_str());
	textEditor->MarkerSetBack(SC_MARKNUM_FOLDERTAIL, wxSETTINGS->StyleGetForeColor(mLanguage, 33).c_str());

	
	//DEFAULT STYLE "34" STYLE_BRACELIGHT
	/*
	 mSciEdit.StyleSetFore(SciConst.STYLE_BRACELIGHT,
	                       mSciEdit.StyleGetFore(32)); //TextForeColor
						   mSciEdit.StyleSetBack(SciConst.STYLE_BRACELIGHT,
						                         wxGlobal.Settings.EditorProperties.StyleGetBackColor(
						                                                                              mLanguage, 256));   //TextSelectionBackColor
																									  */
	textEditor->StyleSetFore(STYLE_BRACELIGHT, 
	                         wxSETTINGS->StyleGetForeColor(mLanguage, 32).c_str());
	textEditor->StyleSetBack(STYLE_BRACELIGHT, 
	                         wxSETTINGS->StyleGetBackColor(mLanguage, 256).c_str());




	//TEXT SELECTION (style 256)
	//mSciEdit.SetSelFore(true,
	//                    wxGlobal.Settings.EditorProperties.StyleGetForeColor(
	//                    mLanguage, 256));
	//To leave syntax highlight colors, rem the following line:
	textEditor->setSelFore(true, wxSETTINGS->StyleGetForeColor(mLanguage, 256).c_str());

	//mSciEdit.SetSelBack(true,
	//                    wxGlobal.Settings.EditorProperties.StyleGetBackColor(
	//                    mLanguage, 256));
	textEditor->setSelBack(true,
	                       wxSETTINGS->StyleGetBackColor(mLanguage, 256).c_str());


	//BOOKMARKS (style 257)
	//mSciEdit.MarkerSetBack(0, wxGlobal.Settings.EditorProperties.StyleGetBackColor(
	//                       mLanguage, 257));
	textEditor->MarkerSetBack(0, wxSETTINGS->StyleGetBackColor(mLanguage, 257).c_str());
	//mSciEdit.MarkerSetAlpha(0, wxGlobal.Settings.EditorProperties.StyleGetAlpha(
	//                       mLanguage, 257));
	textEditor->MarkerSetAlpha(0, wxSETTINGS->StyleGetAlpha(mLanguage, 257));


	//VERTICAL RULER (style 258)
	//mSciEdit.SetEdgeColour(wxGlobal.Settings.EditorProperties.StyleGetForeColor(
	//                       mLanguage, 258));
	textEditor->setEdgeColor(wxSETTINGS->StyleGetForeColor(mLanguage, 258).c_str());


	//CARET LINE MARKER (style 259)
	//mSciEdit.SetCaretLineBack(wxGlobal.Settings.EditorProperties.StyleGetForeColor(
	//                          mLanguage, 259));
	textEditor->setCaretLineBack(wxSETTINGS->StyleGetForeColor(mLanguage, 259).c_str());

	//CARET COLOR (Same as Text Fore Color)
	//mSciEdit.SetCaretFore(mSciEdit.StyleGetFore(32));
	textEditor->setCaretFore(wxSETTINGS->StyleGetForeColor(mLanguage, 32).c_str());

}


void wxEditor::on_SCITextEditor_MouseDown()//(object sender, MouseEventArgs e)
{
	/*
	//Store cursor position
	if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
	{
		mPosition.StoreCursorPos(textEditor.GetCaretPosition());
		//System.Diagnostics.Debug.WriteLine(textEditor.GetCaretPosition());
	}
	if (TextEditorMouseDown != null)
		TextEditorMouseDown(this, e);
	*/
}

void wxEditor::on_SCITextEditor_TextChanged()//(object sender, Int32 position, Int32 length)
{

	/*
	this->owner->onSciModified(this);

	//-TIMER for FindStructure operation
	timerCounter = 0;
	isChanged = true;
	*/
}



/*
private void SCITextEditor_FileDragDrop(object sender, string[] filename)
{
	if (FileDropped != null) FileDropped(this, filename);
}
*/



void wxEditor::setShowExplorer(bool value)
{
	//treeViewStructure->set_visible(value);
	//listBoxBookmarks->set_visible(value);
	get_child1()->set_visible(value);
}
bool wxEditor::getShowExplorer()
{
	return get_child1()->get_visible(); //treeViewStructure->get_visible();
}

bool wxEditor::getShowIntelliTip()
{
	return intelliTip->get_visible();
}
void wxEditor::setShowIntelliTip(bool value)
{
	intelliTip->set_visible(value);
}

void wxEditor::SetIntelliTip(const gchar* Title, const gchar* Params)
{
	intelliTip->ShowTip(Title, Params);
}









void wxEditor::Comment()
{
	try
	{
		Glib::ustring languagecomment = "";

		if (Glib::str_has_suffix(FileName.lowercase(),".csd") ||
		    Glib::str_has_suffix(FileName.lowercase(),".orc") ||
		    Glib::str_has_suffix(FileName.lowercase(),".sco") ||
		    Glib::str_has_suffix(FileName.lowercase(),".udo"))
			languagecomment = ";";
		else if (Glib::str_has_suffix(FileName.lowercase(),".py") ||
		         Glib::str_has_suffix(FileName.lowercase(),".pyw"))
			languagecomment = "#";
		else if (Glib::str_has_suffix(FileName.lowercase(),".lua"))
			languagecomment = "--";
		else 
			return;

		gint mLineStart = textEditor->getLineNumberFromPosition(textEditor->getSelectionStart());
		gint mLineEnd = textEditor->getLineNumberFromPosition(textEditor->getSelectionEnd());


		//Disable comment if the end of the selection (caret)
		//is located at the start of the line
		//if (GetFocusedEditor.GetSelText().Length > 0)
		if(textEditor->getSelectedText().size() > 0)
		{
			if (mLineEnd > mLineStart) //if multiple lines selected
			{
				gint curPosition =
					textEditor->getSelectionEnd() -					 //GetFocusedEditor.GetSelectionEnd() -
					textEditor->getPositionFromLineNumber(mLineEnd); //GetFocusedEditor.PositionFromLine(mLineEnd);

				if (curPosition == 0)
				{
					mLineEnd -= 1;
					if (mLineEnd < mLineStart)
						mLineEnd = mLineStart;
				}
			}
		}


		for (gint mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
		{
			textEditor->InsertText(textEditor->getPositionFromLineNumber(mLine),
			                       languagecomment.c_str());
		}
	}

	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - Comment Error");
	}

}
void wxEditor::UnComment()
{
	try
	{
		Glib::ustring languagecomment = "";
		if (Glib::str_has_suffix(FileName.lowercase(),".csd") ||
		    Glib::str_has_suffix(FileName.lowercase(),".orc") ||
		    Glib::str_has_suffix(FileName.lowercase(),".sco") ||
		    Glib::str_has_suffix(FileName.lowercase(),".udo"))
			languagecomment = ";";
		else if (Glib::str_has_suffix(FileName.lowercase(),".py") ||
		         Glib::str_has_suffix(FileName.lowercase(),".pyw"))
			languagecomment = "#";
		else if (Glib::str_has_suffix(FileName.lowercase(),".lua"))
			languagecomment = "--";
		else 
			return;

		gint mLineStart = textEditor->getLineNumberFromPosition(textEditor->getSelectionStart());
		gint mLineEnd = textEditor->getLineNumberFromPosition(textEditor->getSelectionEnd());

		gint curPos = 0;
		gint curLinePos = 0;
		Glib::ustring curText = "";

		for (gint mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
		{
			curPos = textEditor->getPositionFromLineNumber(mLine);
			curText = textEditor->getTextOfLine(mLine);
			curLinePos = curText.find(languagecomment); //curText.IndexOf(languagecomment);
			if (curLinePos > -1) //string::npos //Glib::ustring::npos
			{
				textEditor->setCaretPosition(curPos + curLinePos);
				//foreach (char c in languagecomment.ToCharArray())
				for(uint i = 0; i < languagecomment.length(); i++)
				{
					textEditor->PerformDelete(); //Equivalent to: 'Canc' key
				}
			}
		}
	}

	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - UnComment Error");
	}
	
}

bool wxEditor::on_treeViewStructure_clicked(GdkEventButton* event)
{
	//wxGLOBAL->DebugPrint("wxEditor", "treeViewStructure Clicked");


	Glib::ustring StringToFind = "";
	Glib::ustring ParentName = "";
	//Glib::ustring KeyValue = "";
	
	Gtk::TreePath path;
	//Gtk::TreeViewColumn col;
	treeViewStructure->get_path_at_pos((int)event->x, (int)event->y, path);
	if(!path)return false;
	
	Gtk::TreeModel::iterator iter = treeViewStructure->get_model()->get_iter(path);



	//Glib::RefPtr<Gtk::TreeSelection> sel = treeViewStructure->get_selection();
	//Gtk::TreeModel::iterator iter = sel->get_selected();;
	if(iter) //If anything is selected
	{
		//Retrieve row text
		Gtk::TreeModel::Row row = *iter;
		//Do something with the row.
		row.get_value(1, StringToFind);

		//wxGLOBAL->DebugPrint(StringToFind.c_str(), 
		//                     ParentName.c_str());
		
		//Retrieve row parent text (if any)
		Gtk::TreeModel::Row rowParent = *row.parent();
		if(rowParent)
		{
			rowParent.get_value(0, ParentName);

			//wxGLOBAL->DebugPrint(StringToFind.c_str(), 
			//                     ParentName.c_str());
		}

	}
	else return false;


	try
	{
		bool isTag = false;

		gint gStart = 0;
		gint gEnd = textEditor->getTextLength();

		//ROOT NODE
		if(StringToFind == "<CsoundSynthesizer>" ||
		   StringToFind == "<CsOptions>" ||
		   StringToFind == "<CsInstruments>" ||
		   StringToFind == "<CsScore>")
		{
			isTag = true;
		}
		else if (ParentName == "<CsScore>")
		{
			//if (e.Node.Text.StartsWith("s"))
			if(Glib::str_has_prefix(StringToFind, "s"))
			{
				//Int32 cycles = Int32.Parse(e.Node.Text.Substring(1));
				gint cycles = atoi(StringToFind.substr(1).c_str());
				gint mStart = 0;
				gint ret = -1;

				//wxGLOBAL->DebugPrint("cycles", wxGLOBAL->IntToString(cycles).c_str());

				for (gint c = 0; c < cycles; c++)
				{
					ret = textEditor->FindText("s",
					                           true,
					                           true,
					                           false,
					                           false,
					                           false,
					                           true,
					                           mStart,
					                           -1);
					if (ret > -1)
					{
						mStart = ret + 2;
					}
					else break;
				}
				if (ret > -1)
				{
					textEditor->GoToLine(textEditor->getLineNumberFromPosition(ret));
					if (ret > -1)
						textEditor->SelectLine(textEditor->getLineNumberFromPosition(ret), true);
					return false;
				}
			}
			else
			{
				//Backward search
				gStart = gEnd;
				gEnd = 0;
			}
		}

		

		//Find and select line
		gint position =
			textEditor->FindText(StringToFind.c_str(), isTag, true, false,
			                    false, false, true, gStart, gEnd);
		if (position > -1)
			textEditor->SelectLine(textEditor->getLineNumberFromPosition(position), 
			                       true);

	}

	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - on_treeViewStructure_clicked Error");
	}

	return false;
}




////////////////////////////////////////////////////////////////////////////////
//BOOKMARKS:
void wxEditor::RefreshListBoxBookmarks()
{
	try
	{
		gint mBookLine = 0;
		gint CurLine = 0;
		gint mIndex = 0;
		Glib::ustring mText = "";
	
		listBoxBookmarks->clear_items();
		//ListBoxBookmarks.BeginUpdate();
		do
		{
			mBookLine = textEditor->MarkerNext(CurLine, 1);
			if (mBookLine == -1) break;

			mIndex += 1;
			mText = textEditor->getTextOfLine(mBookLine);
			//mText = Regex.Replace(mText, @"\s+", " ").Trim();
			//ListBoxBookmarks.Items.Add(String.Format("{0:G}", mIndex) + ". " + mText);
			mText.insert(0, 
			             Glib::ustring::compose("%1. ", wxGLOBAL->IntToString(mIndex)));
			listBoxBookmarks->append(ParseLine(mText));
			CurLine = mBookLine + 1;

		}while (true);
		//ListBoxBookmarks.EndUpdate();
		//ListBoxBookmarks.Refresh();
		
		
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - RefreshListBoxBookmarks Error");
	}
}

bool wxEditor::on_listBoxBookmarks_clicked(GdkEventButton* event)
{

	//wxGLOBAL->DebugPrint("listBoxBookmarks: ", "clicked");
	
	try
	{
		gint CurLine = 0;
		gint BookLine = 0;
		gint mIndex = -1;
		//gint mIndex = listBoxBookmarks->get_selected()[0];
		//if(indexes.size() == 0) return false;

		//This is needed to retrieve the real current selected row
		Gtk::TreePath path;
		listBoxBookmarks->get_path_at_pos((int)event->x, (int)event->y, path);
		if(!path)return false;
		Gtk::TreeModel::iterator iter = listBoxBookmarks->get_model()->get_iter(path);

		//Glib::RefPtr<Gtk::TreeSelection> sel = treeViewStructure->get_selection();
		//Gtk::TreeModel::iterator iter = sel->get_selected();;
		if(iter) //If anything is selected
		{
			//Retrieve row text
			Gtk::TreeModel::Row row = *iter;
			//Do something with the row.
			//Glib::ustring StringToFind = "";
			//row.get_value(0, StringToFind);
			
			//std::cout << listBoxBookmarks->get_model()->get_string(row) << std::endl;
			mIndex = atoi(listBoxBookmarks->get_model()->get_string(row).c_str());
		}
		else return false;

		if (mIndex == -1) return false;

		
		for (gint Ciclo = 0; Ciclo < mIndex + 1; Ciclo++)
		{
			BookLine = textEditor->MarkerNext(CurLine, 1);
			CurLine = BookLine + 1;
		}

		//ListBoxBookmarks.SelectedIndex = -1;
		textEditor->GoToLine(BookLine);
		textEditor->setFocus();

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - on_listBoxBookmarks_clicked Error");
	}

	return false;
}

Glib::ustring wxEditor::ParseLine(Glib::ustring Text)
{
	try
	{
		//Text = Regex.Replace(Text, @"(\s+)", " ");
		Glib::RefPtr<Glib::Regex> my_regex = Glib::Regex::create("(\\s+)");
		Glib::ustring ret = my_regex->replace(Text, 0, " ", Glib::REGEX_MATCH_NOTBOL);
		ret = g_strstrip((gchar*)ret.c_str());

		//wxGLOBAL->DebugPrint("ParseLine", ret.c_str());
		
		return ret;
		
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxEditor - ParseLine Error ");
		return Text;
	}
}

//void InsertRemoveBookmark();
void wxEditor::InsertRemoveBookmark()
{
	//Int32 mLine = GetFocusedEditor.LineFromPosition(GetFocusedEditor.GetCurrentPos());
	//Int32 mLine = this.GetCurrentLineNumber();
	gint mLine = textEditor->getCurrentLineNumber();

	//if (GetFocusedEditor.MarkerGet(mLine) > 0) //Bookmark already exist so we remove it
	if(textEditor->MarkerGet(mLine) > 0)
	{
		//textView1.MarkerDelete(mLine, 0);
		//textView2.MarkerDelete(mLine, 0);
		textEditor->MarkerDelete(mLine);
	}
	else //Bookmark doesn't exist so we add it
	{
		//textView1.MarkerAdd(mLine, 0);
		//textView2.MarkerAdd(mLine, 0);
		textEditor->MarkerAdd(mLine);
	}
	RefreshListBoxBookmarks();
}
//void InsertBookmarkAt(Int32 lineNumber);
void wxEditor::InsertBookmarkAt(gint lineNumber)
{
	//textView1.MarkerAdd(lineNumber, 0);
	//textView2.MarkerAdd(lineNumber, 0);
	textEditor->MarkerAdd(lineNumber);

	///RefreshListBoxBookmarks(); ///???
}
//void RemoveAllBookmarks();
void wxEditor::RemoveAllBookmarks()
{
	//textView1.MarkerDeleteAll(0);
	//textView2.MarkerDeleteAll(0);
	//ListBoxBookmarks.Items.Clear();
	listBoxBookmarks->clear_items();
	textEditor->MarkerDeleteAll(0);
}
//void GoToNextBookmark();
void wxEditor::GoToNextBookmark()
{
	//Int32 mLine = GetFocusedEditor.MarkerNext(this.GetCurrentLineNumber() + 1, 1);
	gint mLine = textEditor->MarkerNext(textEditor->getCurrentLineNumber() + 1, 1);

	if (mLine > -1)
		//this.GoToLine(mLine);
		textEditor->GoToLine(mLine);
}
//void GoToPreviousBookmark();
void wxEditor::GoToPreviousBookmark()
{
	//Int32 mLine = GetFocusedEditor.MarkerPrevious(this.GetCurrentLineNumber() - 1, 1);
	gint mLine = textEditor->MarkerPrevious(textEditor->getCurrentLineNumber() - 1, 1);
	
	if (mLine > -1)
		//this.GoToLine(mLine);
		textEditor->GoToLine(mLine);
}
//bool HasBookmarks();
bool wxEditor::HasBookmarks()
{
	//Int32 mLine = this.textView1.MarkerNext(0, 1); //[self MarkerNext:0 markerMask:1];
	gint mLine = textEditor->MarkerNext(0, 1);

	return (mLine > -1);
}


void wxEditor::RefreshExplorer()
{
	isChanged = true;
	timerCounter = FIND_STRUCTURE_INTERVAL;
	//StartExplorerTimer();
	//FindCodeStructureWorkCompleted();
}

void wxEditor::FindCodeStructureWorkCompleted()//Glib::RefPtr<Gtk::TreeStore> RefTreeModel)
{
	wxGLOBAL->DebugPrint("FindCodeStructureWorkCompleted", "OK");

	//treeViewStructure->set_model(RefTreeModel);	
	treeViewStructure->set_model(findStructure->MyRefTreeModel);

	
	//Create a new coloumn to add correctly the pixbuf images
	Gtk::TreeView::Column* pColumn =
    Gtk::manage( new Gtk::TreeView::Column("Explorer") );

	//Create a new CellRendereText to set font scale!!!
	Gtk::CellRendererText* textCell =
    Gtk::manage( new Gtk::CellRendererText() );

	if(wxSETTINGS->EditorProperties.ExplorerFontSize == 0)
		textCell->property_scale() = 0.9;
	else if(wxSETTINGS->EditorProperties.ExplorerFontSize == 1)
		textCell->property_scale() = 1.0;
	else if(wxSETTINGS->EditorProperties.ExplorerFontSize == 2)
		textCell->property_scale() = 1.1;
	
	
	pColumn->pack_start(findStructure->MyColumns.image, false);
	//pColumn->pack_start(findStructure->MyColumns.key, false); //false = don't expand.
	pColumn->pack_start(*textCell, false);
	//pColumn->add_attribute(*textCell, "text", 0); 
	pColumn->add_attribute(textCell->property_text(), findStructure->MyColumns.key); 
	
	
	//Add the TreeView's view columns:
	treeViewStructure->remove_all_columns();
	//treeViewStructure->append_column("Explorer", findStructure->MyColumns.key);
	treeViewStructure->append_column(*pColumn);
	treeViewStructure->expand_all();
	
}



/*		//TODO:

        public bool BookmarksOnLoad()
        {
            get { return mBookmarksOnLoad; }
            set { mBookmarksOnLoad = value; }
        }


        public PrintDocument GetPrintDocument()
        {
            return textEditor.GetPrintDocument();
        }


*/



void wxEditor::StoreCursorPos()
{
	mPosition->StoreCursorPos(textEditor->getCaretPosition());
}

void wxEditor::StoreCursorPos(gint position)
{
	mPosition->StoreCursorPos(position);
}

void wxEditor::GoToPreviousPos()
{
	gint pos = mPosition->PreviousPos();
	gint curLine = textEditor->getLineNumberFromPosition(pos);

	textEditor->GoToLine(curLine);
	textEditor->setCaretPosition(pos);
	textEditor->setFocus();
}

void wxEditor::GoToNextPos()
{
	gint pos = mPosition->NextPos();
	gint curLine = textEditor->getLineNumberFromPosition(pos);
	
	textEditor->GoToLine(curLine);
	textEditor->setCaretPosition(pos);
	textEditor->setFocus();
}



/*
public void SetFont(Font f)
{
	textEditor.SetTextFont(f);
}

public Font GetFont()
{
	return textEditor.GetTextFont();
}
*/





