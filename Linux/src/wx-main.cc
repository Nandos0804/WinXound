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
#include <fstream>
#include <algorithm>

#include "wx-main.h"
#include "wx-textEditor.h"
#include "wx-editor.h"
#include "wx-settings.h"
#include "wx-global.h"
#include "wx-terminal.h"
#include "wx-browser.h"
#include "wx-about.h"
#include "wx-importexport.h"
#include "wx-findandreplace.h"
#include "wx-findline.h"
#include "wx-analysis.h"
#include "wx-csoundrepository.h"
#include "wx-repository.h"
#include "wx-codeformatter.h"
#include "wx-print.h"
//#include "wx-pipe.h"
//#include "wx-cabbage.h"
//#include "wx-cabbagerepository.h"

//#include <libvtemm/terminal.h>
//#include <vte/vte.h> 
//#include <webkit/webkit.h> 



#define UNTITLED_CSD "Untitled.csd"
#define UNTITLED_ORC "Untitled.orc"
#define UNTITLED_SCO "Untitled.sco"
#define UNTITLED_PY "Untitled.py"
#define UNTITLED_LUA "Untitled.lua"
#define UNTITLED_CABBAGE "Untitled.cabbage"

#define IMPORTED "Imported: "
#define WINXOUND_UNTITLED "WinXound_Untitled"

#define NOTEBOOK_COMPILER_SIZE 30	////80-160 //140
#define TAB_POPUP_RATIO 1.7


//using Glib::ustring;



//CONSTRUCTOR
wxMain::wxMain(int argc, char *argv[])
{
	//INIT THREADS: g_thread_init(NULL);	
	Glib::thread_init();


	//DEBUG:
	//std::cout << UI_FILE << std::endl;
	//std::cout << "LOCALE_DIR: " << PACKAGE_LOCALE_DIR << std::endl;
	//std::cout << "DATA_DIR: " << PACKAGE_DATA_DIR << std::endl;
	//std::cout << "GET_HOME_DIR: " << Glib::get_home_dir() << std::endl;
	//std::cout << "G_USER_DIRECTORY_PUBLIC_SHARE: " << 
	//	Glib::get_user_special_dir(G_USER_DIRECTORY_PUBLIC_SHARE) << std::endl;

	/*
	std::cout << "User_CONFIG_Dir: " << Glib::get_user_config_dir () << std::endl;
	std::cout << "User_DATA_Dir: " << Glib::get_user_data_dir () << std::endl;
	std::cout << "PACKAGE_LOCALE_DIR: " << PACKAGE_LOCALE_DIR << std::endl;
	std::cout << "PACKAGE_SRC_DIR: " << PACKAGE_SRC_DIR << std::endl;
	std::cout << "PACKAGE_DATA_DIR: " << PACKAGE_DATA_DIR << std::endl;
	std::cout << "PIXMAP_PATH: " << PIXMAP_PATH << std::endl;
	std::cout << "SYSCONF_PATH: " << SYSCONF_PATH << std::endl;
	*/

	
	/*
	// EXAMPLE OF A TRY CATCH BLOCK IN C++
	try
	{
		throw 1;
	}
	//catch(const Glib::Error& err) //FOR GLIB
	//catch(const Glib::Error& ex) //FOR GLIB
	catch (...) //CATCH ALL!!!
	//catch (std::exception const& ex)
	{
		std::cout << "CRASH" << std::endl;
	}
	*/


	
	//Set some menus to NULL !!!
	AdditionalFlagsSubMenu = NULL;
	AdditionalFlagsSubMenu2 = NULL;

	
	
	////////////////////////////////////////////////////////////////////////////
	//Check if WinXound executable is correctly located in a 
	//Full Read-Write Access folder
	try
	{
		//Glib::ustring settingsPath = wxGLOBAL->getSettingsPath();
		Glib::ustring tPath = wxGLOBAL->getHelpPath();
		tPath.append("/dummy.txt");

		//std::cerr << wxGLOBAL->FileIsWritable(settingsPath.c_str()) << std::endl;
		
		if(!wxGLOBAL->FileIsWritable(tPath.c_str()))
		{
			mWinXoundIsReadOnly = true;
			wxGLOBAL->ShowMessageBox(this,
			                         "It seems that you have saved WinXound inside a \n"
			                         "path with a restricted access for your account!\n"
			                         "Please locate WinXound folder into a path where you have\n"
			                         "full Read and Write permissions, \n"
			                         "for example in your User folder.\n\n"
			                         "WinXound will now work in Read-Only mode (but it may be unstable).\n",
			                         "WinXound Folder Unauthorized Access", //TITLE
			                         Gtk::BUTTONS_OK);
		}

	}
	//catch (std::exception const& ex)
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain constructor: CheckReadWrite Error");
		//mWinXoundIsReadOnly = true;
	}
	////////////////////////////////////////////////////////////////////////////


	////////////////////////////////////////////////////////////////////////////
	//Load the Glade file and instantiate its widgets:
	Glib::RefPtr<Gtk::Builder> builder;
	try
	{
		//builder = Gtk::Builder::create_from_file(UI_FILE);
		builder = Gtk::Builder::create_from_file(
			Glib::ustring::compose("%1/winxound_gtkmm.ui",wxGLOBAL->getSrcPath()));
	}
	catch (const Glib::FileError & ex)
	{
		std::cerr << "wxMain - Constructor - Create Builder Error - Critical\n"
			      << ex.what() << std::endl;
		return;
	}
	catch (...)
	{
		std::cerr << "wxMain - Constructor - Create Builder Error - Critical" << std::endl;
		return;
	}

	//Locate and Associate wxMainWindow
	oldHeight = 0;
	builder->get_widget("window", wxMainWindow);
	wxMainWindow->hide();
	wxMainWindow->set_title(TITLE);
	//wxMainWindow->move(0, 0);
	//wxMainWindow->resize(900,600);
	//wxMainWindow->signal_configure_event().connect(
	//	sigc::mem_fun(*this, &wxMain::on_wxMainWindow_configure_event),false);
	wxMainWindow->signal_delete_event().connect(
		sigc::mem_fun(*this, &wxMain::on_wxMainWindow_delete_event),false);

	wxMainWindow->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxMain::on_key_press_event),false);
	
	/*
	wxMainWindow->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxMain::on_newdialog_keypress));
	wxMainWindow->signal_key_release_event().connect(
		sigc::mem_fun(*this, &wxMain::on_newdialog_keypress));
	*/


	//DRAG AND DROP
	//wxMain::on_DragDataReceived
	std::list<Gtk::TargetEntry> listTargets;
	//text/uri-list
	listTargets.push_back( Gtk::TargetEntry("text/uri-list") );

	wxMainWindow->drag_dest_set(listTargets);
	wxMainWindow->signal_drag_data_received().connect(
		sigc::mem_fun(*this, &wxMain::on_DragDataReceived),false);
	
	CreateWinXoundIcon();


	//Locate and Associate various interface objects
	builder->get_widget("notebookCode", notebookCode);
	builder->get_widget("notebookCompiler", notebookCompiler);
	builder->get_widget("vpaned1",vpanedMain);
	builder->get_widget("toolbar1", toolbar);
	builder->get_widget("statusbarInfo", statusbarInfo);

	builder->get_widget("menuitemLineNumbers", menuitemLineNumbers);
	builder->get_widget("menuitemExplorer", menuitemExplorer);
	builder->get_widget("menuitemOnlineHelp", menuitemOnlineHelp);
	builder->get_widget("menuitemToolbar", menuitemToolbar);
	builder->get_widget("menuitemUseWinXoundFlags", menuitemUseWinXoundFlags);
	

	//Notebooks settings
	notebookCode->set_scrollable(TRUE);
	notebookCode->set_can_focus(FALSE);
	notebookCode->popup_disable();
	notebookCode->signal_switch_page().connect(
		sigc::mem_fun(*this, &wxMain::on_notebookEditor_switch_page));

	notebookCompiler->set_scrollable(TRUE);
	notebookCompiler->set_size_request(-1, NOTEBOOK_COMPILER_SIZE);
	

	//wxMainWindow->set_events(Gdk::BUTTON_PRESS_MASK);
	//wxMainWindow->signal_button_press_event().connect(
	//	sigc::mem_fun(*this, &wxMain::on_notebookEditor_button_press));

	notebookCompiler->signal_size_allocate().connect(
	    sigc::mem_fun(*this, &wxMain::on_notebookCompiler_resize)); 
	
	notebookCompiler->signal_switch_page().connect(
		sigc::mem_fun(*this, &wxMain::on_notebookCompiler_switch_page));
	

	//Connect quit signal
	Gtk::Main::signal_quit().connect(sigc::mem_fun(this, &wxMain::on_main_quit_signal));

	this->setMenuSignals(builder);
	////////////////////////////////////////////////////////////////////////////


	
	////////////////////////////////////////////////////////////////////////////
	//LOAD SETTINGS AND INITIALIZE THE VARIABLES (ALSO ENVIRONMENT VARIABLES)
	//wxGLOBAL->ShowMessageBox(wxGLOBAL->getSettingsPath().c_str(), "SETTINGS", Gtk::BUTTONS_OK);
	wxSETTINGS->LoadSettings(false);	
	this->on_menuitemScreenPositionRESET_Clicked(); //set main window position

	Opcodes = wxGLOBAL->LoadOpcodes();
	compiler = new wxTerminal(true);
	terminal = new wxTerminal(false);
	HelpBrowser = new wxBrowser(true);
	HelpBrowser->signal_csound_file_clicked().connect(
		sigc::mem_fun(*this, &wxMain::on_HelpBrowser_csound_file_clicked));
	AboutWindow = new wxAbout();
	FindAndReplace = new wxFindAndReplace(this);
	FindLine = new wxFindLine(this);
	Analysis = new wxAnalysis(this);
	mCSoundRepository = new wxCSoundRepository();

	mRepository = new wxRepository();
	mRepository->ConfigureEditor(Opcodes);
	//mCabbageRepository = new wxCabbageRepository();
	//mCabbageRepository->ConfigureEditor(Opcodes);
	
	mFormatter = new wxCodeFormatter();
	//namedPipe = new wxPipe();
	//cabbageUtilities = new wxCabbage();
	//cabbageUtilities->signal_message_received().connect(
	//	sigc::mem_fun(*this, &wxMain::on_cabbage_message_received)); 

	//Compiler window
	compilerWindow = new Gtk::Window();
	compilerWindow->signal_hide().connect(
		sigc::mem_fun(*this, &wxMain::on_compilerWindow_closed)); 
	//compilerWindow->signal_hide().connect(
	//	sigc::mem_fun(*this, &wxMain::on_compilerWindow_closing), false); 
	compilerWindow->signal_expose_event().connect(
		sigc::mem_fun(*this, &wxMain::on_compilerWindow_expose_event)); //OK!!!

	compilerWindow->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxMain::on_compilerWindow_key_press_event), false);
	Glib::ustring iconfile = Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath());
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
		compilerWindow->set_icon_from_file(iconfile);

	
	isDragAndDrop = false;

	//Associate compiler completed signal
	compiler->signal_compiler_completed().connect(
		sigc::mem_fun(*this, &wxMain::on_compiler_completed));

	
	CreatePopupMenu();
	
	////////////////////////////////////////////////////////////////////////////
	//CREATE NOTEBOOK COMPILER TABS
	CreateNotebookCompilerTabs();


	////////////////////////////////////////////////////////////////////////////
	//VARIOUS
    //Set menuitemUseWinXoundFlags
	menuitemUseWinXoundFlags->set_active(wxSETTINGS->General.UseWinXoundFlags);

	//Show MainWindow to the user
	//wxMainWindow->show();
	
	//Set Menu and Toolbar states (enabled or disabled)
	CheckMenuConditions();


	/* TODO: ???
	//OPCODES DATABASE LOAD!!!
	//Load opcodes (from "opcodes.txt" file) into the Opcodes_Hashtable
	//and at the same time fill the CSoundOpcodesRepository TreeView
	//Opcodes = wxGlobal.LoadOpcodes(); //OLD Method
	//wxCSoundOpcodesRepository1.SetOpcodes(Opcodes); //OLD Method
	Opcodes = wxCSoundOpcodesRepository1.FillTreeViewAndReturnOpcodes();
	this.Refresh();
	*/

	
	//CSound Default Compiler Flags CHECK !!!
	if (wxSETTINGS->General.CSoundDefaultFlags == "")
	{
		wxSETTINGS->General.CSoundDefaultFlags =
			"-B4096 --displays --asciidisplay";
	}


	
	//menuitemRecentFiles->set_submenu(*new Gtk::RecentChooserMenu);
	//Fill FileMenu with the recent files list
	AddRecentFilesToMenu(NULL);

	//Fill CSound Additional Flags menu
	UpdateAdditionalFlagsMenu();
		


	//Set Compiler font
	try
	{
		compiler->SetCompilerFont(wxSETTINGS->General.CompilerFontName,
		                          wxSETTINGS->General.CompilerFontSize);
	}
	catch(...)
	{
		wxSETTINGS->General.CompilerFontName = "Monospace";
		wxSETTINGS->General.CompilerFontSize = 10;
		compiler->SetCompilerFont(wxSETTINGS->General.CompilerFontName,
		                          wxSETTINGS->General.CompilerFontSize);
		wxGLOBAL->DebugPrint("wxMain constructor: SetCompilerFont Error");
	}



/*	//CABBAGE TOOLS
	//Check for Cabbage path and for Cabbage updates on Internet
	if(!Glib::file_test(wxSETTINGS->Directory.CabbagePath, Glib::FILE_TEST_EXISTS))
	{
		wxSETTINGS->Directory.CabbagePath = "";
		
		Glib::ustring cabbageExePath = 
			Glib::ustring::compose("%1/CabbageLinux/%2",wxGLOBAL->getCabbagePath(), CABBAGE_NAME);

		//std::cout << "CabbageEXE_Path:" << cabbageExePath << std::endl;
		
		if(Glib::file_test(cabbageExePath, Glib::FILE_TEST_EXISTS))
		{
			wxSETTINGS->Directory.CabbagePath = cabbageExePath;
		}
	}

	if(wxSETTINGS->General.CheckForCabbageUpdates)
		cabbageUtilities->CheckForCabbageUpdatesOnInternet(true);*/


	
	
	//Check for First Start 
	if (wxSETTINGS->General.FirstStart)
	{
		//Set window position (centered on screen)
		Glib::RefPtr< Gdk::Screen > scr = wxMainWindow->get_screen();
		gint width = wxSETTINGS->General.WindowSize.get_x() / 2;
		gint height = wxSETTINGS->General.WindowSize.get_y() / 2;

		Gdk::Point p = Gdk::Point(scr->get_width() / 2 - width, 
		                          scr->get_height() / 2 - height);
		wxSETTINGS->General.WindowPosition = p;
		wxMainWindow->move(p.get_x(), p.get_y()); 

		FirstStart();
	}
	


	//Show MainWindow to the user
	wxMainWindow->show();
	vpanedMain->set_position(wxMainWindow->get_height() + NOTEBOOK_COMPILER_SIZE); //80-160
	

	//Set notebookCompiler popup default height
	oldHeight = wxMainWindow->get_height() / TAB_POPUP_RATIO;

	
	//Set initial path for File Dialogs
	if(wxSETTINGS->Directory.LastUsedPath == "")
		wxSETTINGS->Directory.LastUsedPath = wxGLOBAL->getHomePath();



	
	
	///////////////////////////////////
	//Finally check for Files to open
	//Check for command line arguments 
	bool fileLoaded = false;
	try
	{
		//string[] args = System.Environment.GetCommandLineArgs();
		if (argv != NULL)
		{
			gint length = wxGLOBAL->ArrayLength(argv);
			if (length > 1)
			{
				for (gint i = 1; i < length; i++)
				{
					//if (!string.IsNullOrEmpty(args[i]))
					//std::cout << argv[i] << std::endl;
					if(strcmp(argv[i], "") != 0 &&
					   Glib::file_test(argv[i], Glib::FILE_TEST_EXISTS))
					{
						AddNewEditor(argv[i]);
						fileLoaded = true;
					}
				}
				
				if(fileLoaded) return;
			}
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain constructor: CommandLine arguments Error");
	}


	
	
	//Prepare a template document if a StartupAction is defined
	if (wxSETTINGS->General.StartupAction == 1)
		AddNewEditor(UNTITLED_CSD);
	else if (wxSETTINGS->General.StartupAction == 2)
		AddNewEditor(UNTITLED_PY);
	else if (wxSETTINGS->General.StartupAction == 3)
		AddNewEditor(UNTITLED_LUA);
	else if (wxSETTINGS->General.StartupAction == 4)
		OpenLastSessionFiles();
	

}

//DESTRUCTOR
wxMain::~wxMain()
{
	//delete Opcodes; //???
	//delete mCompilerEditor;
	
	delete compiler;
	delete terminal;
	delete HelpBrowser;
	delete AboutWindow;
	delete FindAndReplace;
	delete FindLine;
	
	delete mCSoundRepository;
	delete mRepository;
	delete mFormatter;
	delete Analysis;
	
	delete wxMainWindow;
	delete compilerWindow;

	g_hash_table_destroy(Opcodes);

	delete wxSETTINGS;
	delete wxGLOBAL;

	//delete namedPipe;
	//delete cabbageUtilities;

	
	//RELEASE ALL SCINTILLA RESOURCES
	try { scintilla_release_resources(); }
	catch(...) 
	{
		std::cout << "wxMain: scintilla_release_resources() - Error" << std::endl;
	}

	//CLEANUP
	Glib::Error::register_cleanup(); 
    Glib::wrap_register_cleanup();    

	std::cout << "wxMain Released" << std::endl;
}



void wxMain::on_menuRecentFiles_Clicked(Glib::ustring file)
{
	//std::cerr << file << std::endl;
	
	if(Glib::file_test(file.c_str(), Glib::FILE_TEST_EXISTS))
		AddNewEditor(file.c_str());
}

void wxMain::AddRecentFilesToMenu(const gchar* filename)
{
	if(filename != NULL)
	{
		wxSETTINGS->RecentFilesInsert(filename);
		delete RecentFilesSubMenu;
	}

	//GTK DELETE SIGNALS EVENTS -> PROBABLY RELEASED ON: delete RecentFilesSubMenu;
	
	try
	{
		RecentFilesSubMenu = new Gtk::Menu();
		
		for (uint i = 0; i < wxSETTINGS->RecentFiles.size(); i++)
		{
			/*
			Glib::ustring name = "_";
			name.append(wxGLOBAL->IntToString(i+1));
			name.append(" ");
			name.append(wxSETTINGS->RecentFiles[i]);
			
			Gtk::MenuItem* item = new Gtk::MenuItem(name);
			item->set_use_underline(TRUE);
			RecentFilesSubMenu->append(*item);
			item->signal_activate().connect(
				sigc::bind<Glib::ustring>(
				sigc::mem_fun(*this, &wxMain::on_menuRecentFiles_Clicked), name.substr(3)));
			*/

			Glib::ustring name = wxSETTINGS->RecentFiles[i];
			
			Gtk::MenuItem* item = Gtk::manage(new Gtk::MenuItem(name)); //TODO: CHECK MANAGE!!!
			item->set_use_underline(FALSE);
			RecentFilesSubMenu->append(*item);
			item->signal_activate().connect(
				sigc::bind<Glib::ustring>(
				sigc::mem_fun(*this, &wxMain::on_menuRecentFiles_Clicked), name));
		}
		
		if (wxSETTINGS->RecentFiles.size() > 0)
		{
			RecentFilesSubMenu->show_all();
			menuitemRecentFiles->set_sensitive(TRUE);
			menuitemRecentFiles->set_submenu(*RecentFilesSubMenu);
		}
		else 
			menuitemRecentFiles->set_sensitive(FALSE);
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain constructor: RecentFiles Error");
	}
}





void wxMain::on_notebookEditor_switch_page(GtkNotebookPage* page, guint page_num)
{
	//std::cout << "SWITCH PAGE" << std::endl;
	if(ActiveEditor() == NULL) return;

	//Glib::ustring temp = TITLE;
	//temp.append(" - ");
	//temp.append(ActiveEditor()->FileName);
	//wxMainWindow->set_title(temp);

	onZoomNotification(ActiveEditor());
	CheckMenuConditions();

	ActiveEditor()->SetFocus();	
}

void wxMain::on_notebookCompiler_switch_page(GtkNotebookPage* page, guint page_num)
{
	if(isDragAndDrop)
	{
		notebookCompiler->set_current_page(oldSelectedTab);
		return;
	}
	std::cout << "notebookCompiler SWITCH PAGE" << std::endl;
	oldSelectedTab = page_num;
}



//////////////////////////////////
//SET MENU SIGNALS
void wxMain::setMenuSignals(Glib::RefPtr<Gtk::Builder> builder)
{
	//TOOLBAR
	//Gtk::ToolButton* toolbuttonNew;
	builder->get_widget("toolbuttonNew",toolbuttonNew);
	toolbuttonNew->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemNew_Clicked));

	//Gtk::ToolButton* toolbuttonOpen;
	builder->get_widget("toolbuttonOpen",toolbuttonOpen);
	toolbuttonOpen->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemOpen_Clicked));

	//Gtk::ToolButton* toolbuttonSave;
	builder->get_widget("toolbuttonSave",toolbuttonSave);
	toolbuttonSave->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSave_Clicked));

	//Gtk::ToolButton* toolbuttonSaveAs;
	builder->get_widget("toolbuttonSaveAs",toolbuttonSaveAs);
	toolbuttonSaveAs->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSaveAs_Clicked));
	
	//Gtk::ToolButton* toolbuttonCloseDocument;
	builder->get_widget("toolbuttonCloseDocument",toolbuttonCloseDocument);
	toolbuttonCloseDocument->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemClose_Clicked));

	//Gtk::ToolButton* toolbuttonUndo;
	builder->get_widget("toolbuttonUndo",toolbuttonUndo);
	toolbuttonUndo->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemUndo_Clicked));

	//Gtk::ToolButton* toolbuttonRedo;
	builder->get_widget("toolbuttonRedo",toolbuttonRedo);
	toolbuttonRedo->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemRedo_Clicked));

	//Gtk::ToolButton* toolbuttonResetZoom;
	builder->get_widget("toolbuttonResetZoom",toolbuttonResetZoom);
	toolbuttonResetZoom->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonResetZoom_Clicked));

	//Gtk::ToolButton* toolbuttonZoomOut;
	builder->get_widget("toolbuttonZoomOut",toolbuttonZoomOut);
	toolbuttonZoomOut->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonZoomOut_Clicked));

	//Gtk::ToolButton* toolbuttonZoomIn;
	builder->get_widget("toolbuttonZoomIn",toolbuttonZoomIn);
	toolbuttonZoomIn->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonZoomIn_Clicked));


	//Gtk::ToolButton* toolbuttonFind;
	builder->get_widget("toolbuttonFind",toolbuttonFind);
	toolbuttonFind->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFindAndReplace_Clicked));

	//Gtk::ToolButton* toolbuttonFindLine;
	builder->get_widget("toolbuttonFindLine",toolbuttonFindLine);
	toolbuttonFindLine->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFindLineNumber_Clicked));

	//Gtk::ToolButton* toolbuttonCompile;
	builder->get_widget("toolbuttonCompile",toolbuttonCompile);
	toolbuttonCompile->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCompile_Clicked));

	//Gtk::ToolButton* toolbuttonCompileExternalGUI;
	builder->get_widget("toolbuttonCompileExternalGUI",toolbuttonCompileExternalGUI);
	toolbuttonCompileExternalGUI->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemRunExternalGUI_Clicked));

	/*builder->get_widget("toolbuttonUpdateCabbageInstrument",toolbuttonUpdateCabbageInstrument);
	toolbuttonUpdateCabbageInstrument->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageUpdate_Clicked));
	//Set Cabbage icon
	Glib::ustring iconname = Glib::ustring::compose("%1/cabbage_16.png", wxGLOBAL->getIconsPath());
	if(Glib::file_test(iconname, Glib::FILE_TEST_EXISTS))
	{
		Gtk::Image* imageCSound = new Gtk::Image(iconname);
		imageCSound->show();
		toolbuttonUpdateCabbageInstrument->set_icon_widget(*imageCSound);
	}*/
	

	                                                  	
	//Gtk::ToolButton* toolbuttonCSoundUtilities;
	builder->get_widget("toolbuttonCSoundUtilities",toolbuttonCSoundUtilities);
	toolbuttonCSoundUtilities->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemAnalysis_Clicked));

	//Gtk::ToolButton* toolbuttonCSoundHelp;
	builder->get_widget("toolbuttonCSoundHelp",toolbuttonCSoundHelp);
	toolbuttonCSoundHelp->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCSoundHelp_Clicked));

	//Gtk::ToolButton* toolbuttonCSoundOpcodes;
	builder->get_widget("toolbuttonCSoundOpcodes",toolbuttonCSoundOpcodes);
	toolbuttonCSoundOpcodes->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonCSoundOpcodes_Clicked));

	//Gtk::ToolButton* toolbuttonCSoundFlags;
	builder->get_widget("toolbuttonCSoundFlags",toolbuttonCSoundFlags);
	toolbuttonCSoundFlags->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonCSoundFlags_Clicked));

	//Gtk::ToolButton* toolbuttonCompilerWindow;
	builder->get_widget("toolbuttonCompilerWindow",toolbuttonCompilerWindow);
	toolbuttonCompilerWindow->signal_clicked().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonCompilerWindow_Clicked));
	

	//MENU FILE
	//Gtk::MenuItem* menuitemNew;
	builder->get_widget("menuitemNew", menuitemNew);
	menuitemNew->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemNew_Clicked));

	//Gtk::MenuItem* menuitemOpen;
	builder->get_widget("menuitemOpen", menuitemOpen);
	menuitemOpen->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemOpen_Clicked));

	//Gtk::MenuItem* menuitemSave;
	builder->get_widget("menuitemSave", menuitemSave);
	menuitemSave->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSave_Clicked));

	//Gtk::MenuItem* menuitemSaveAs;
	builder->get_widget("menuitemSaveAs", menuitemSaveAs);
	menuitemSaveAs->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSaveAs_Clicked));

	//Gtk::MenuItem* menuitemSaveAll;
	builder->get_widget("menuitemSaveAll", menuitemSaveAll);
	menuitemSaveAll->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSaveAll_Clicked));

	//Gtk::MenuItem* menuitemClose;
	builder->get_widget("menuitemClose", menuitemClose);
	menuitemClose->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemClose_Clicked));

	//Gtk::MenuItem* menuitemCloseAll;
	builder->get_widget("menuitemCloseAll", menuitemCloseAll);
	menuitemCloseAll->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCloseAll_Clicked));
	
	//Gtk::MenuItem* menuitemFileInfo;
	builder->get_widget("menuitemFileInfo", menuitemFileInfo);
	menuitemFileInfo->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFileInfo_Clicked));


	//Gtk::MenuItem* menuitemImportOrcScoToNewCSD;
	builder->get_widget("menuitemImportOrcScoToNewCSD", menuitemImportOrcScoToNewCSD);
	menuitemImportOrcScoToNewCSD->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemImportOrcScoToNewCSD_Clicked));

	
	//Gtk::MenuItem* menuitemImportOrcSco;
	builder->get_widget("menuitemImportOrcSco", menuitemImportOrcSco);
	menuitemImportOrcSco->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemImportOrcSco_Clicked));

	//Gtk::MenuItem* menuitemImportOrc;
	builder->get_widget("menuitemImportOrc", menuitemImportOrc);
	menuitemImportOrc->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemImportOrc_Clicked));

	//Gtk::MenuItem* menuitemImportSco;
	builder->get_widget("menuitemImportSco", menuitemImportSco);
	menuitemImportSco->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemImportSco_Clicked));

	//Gtk::MenuItem* menuitemExport;
	builder->get_widget("menuitemExport", menuitemExport);
	
	//Gtk::MenuItem* menuitemExportOrcSco;
	builder->get_widget("menuitemExportOrcSco", menuitemExportOrcSco);
	menuitemExportOrcSco->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemExportOrcSco_Clicked));

	//Gtk::MenuItem* menuitemExportOrc;
	builder->get_widget("menuitemExportOrc", menuitemExportOrc);
	menuitemExportOrc->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemExportOrc_Clicked));

	//Gtk::MenuItem* menuitemExportSco;
	builder->get_widget("menuitemExportSco", menuitemExportSco);
	menuitemExportSco->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemExportSco_Clicked));

	//Gtk::MenuItem* menuitemPrint;
	builder->get_widget("menuitemPrint", menuitemPrint);
	menuitemPrint->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemPrint_Clicked));

	//Gtk::MenuItem* menuitemPrintPreview;
	builder->get_widget("menuitemPrintPreview", menuitemPrintPreview);
	menuitemPrintPreview->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemPrintPreview_Clicked));

	//Gtk::MenuItem* menuitemSettings;
	builder->get_widget("menuitemSettings", menuitemSettings);
	menuitemSettings->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSettings_Clicked));
	wxSETTINGS->signal_settings_save().connect(sigc::mem_fun(*this, &wxMain::on_settings_save_Clicked));

	
	//RECENT FILES 
	builder->get_widget("menuitemRecentFiles", menuitemRecentFiles);
	
	
	//Gtk::MenuItem* menuitemExit;
	builder->get_widget("menuitemExit", menuitemExit);
	menuitemExit->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemExit_Clicked));



	//MENU EDIT
	//Gtk::MenuItem* menuitemUndo;
	builder->get_widget("menuitemUndo", menuitemUndo);
	menuitemUndo->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemUndo_Clicked));

	//Gtk::MenuItem* menuitemRedo;
	builder->get_widget("menuitemRedo", menuitemRedo);
	menuitemRedo->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemRedo_Clicked));

	//Gtk::MenuItem* menuitemCut;
	builder->get_widget("menuitemCut", menuitemCut);
	menuitemCut->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCut_Clicked));

	//Gtk::MenuItem* menuitemCopy;
	builder->get_widget("menuitemCopy", menuitemCopy);
	menuitemCopy->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCopy_Clicked));

	//Gtk::MenuItem* menuitemPaste;
	builder->get_widget("menuitemPaste", menuitemPaste);
	menuitemPaste->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemPaste_Clicked));

	//Gtk::MenuItem* menuitemDelete;
	builder->get_widget("menuitemDelete", menuitemDelete);
	menuitemDelete->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemDelete_Clicked));

	//Gtk::MenuItem* menuitemSelectAll;
	builder->get_widget("menuitemSelectAll", menuitemSelectAll);
	menuitemSelectAll->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSelectAll_Clicked));

	//Gtk::MenuItem* menuitemFind;
	builder->get_widget("menuitemFind", menuitemFind);
	
	//Gtk::MenuItem* menuitemFindAndReplace;
	builder->get_widget("menuitemFindAndReplace", menuitemFindAndReplace);
	menuitemFindAndReplace->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFindAndReplace_Clicked));

	//Gtk::MenuItem* menuitemFindNext;
	builder->get_widget("menuitemFindNext", menuitemFindNext);
	menuitemFindNext->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFindNext_Clicked));

	//Gtk::MenuItem* menuitemFindPrevious;
	builder->get_widget("menuitemFindPrevious", menuitemFindPrevious);
	menuitemFindPrevious->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFindPrevious_Clicked));

	//Gtk::MenuItem* menuitemJumpToCaret;
	builder->get_widget("menuitemJumpToCaret", menuitemJumpToCaret);
	menuitemJumpToCaret->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemJumpToCaret_Clicked));

	//Gtk::MenuItem* menuitemFindLineNumber;
	builder->get_widget("menuitemFindLineNumber", menuitemFindLineNumber);
	menuitemFindLineNumber->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFindLineNumber_Clicked));

	//Gtk::MenuItem* menuitemComments;
	builder->get_widget("menuitemComments", menuitemComments);
	
	//Gtk::MenuItem* menuitemCommentLine;
	builder->get_widget("menuitemCommentLine", menuitemCommentLine);
	menuitemCommentLine->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCommentLine_Clicked));

	//Gtk::MenuItem* menuitemRemoveCommentLine;
	builder->get_widget("menuitemRemoveCommentLine", menuitemRemoveCommentLine);
	menuitemRemoveCommentLine->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemRemoveCommentLine_Clicked));

	//Gtk::MenuItem* menuitemBookmarks;
	builder->get_widget("menuitemBookmarks", menuitemBookmarks);
	
	//Gtk::MenuItem* menuitemInsertRemoveBookmark;
	builder->get_widget("menuitemInsertRemoveBookmark", menuitemInsertRemoveBookmark);
	menuitemInsertRemoveBookmark->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemInsertRemoveBookmark_Clicked));

	//Gtk::MenuItem* menuitemRemoveAllBookmarks;
	builder->get_widget("menuitemRemoveAllBookmarks", menuitemRemoveAllBookmarks);
	menuitemRemoveAllBookmarks->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemRemoveAllBookmarks_Clicked));

	//Gtk::MenuItem* menuitemGoToNextBookmark;
	builder->get_widget("menuitemGoToNextBookmark", menuitemGoToNextBookmark);
	menuitemGoToNextBookmark->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemGoToNextBookmark_Clicked));

	//Gtk::MenuItem* menuitemGoToPreviousBookmark;
	builder->get_widget("menuitemGoToPreviousBookmark", menuitemGoToPreviousBookmark);
	menuitemGoToPreviousBookmark->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemGoToPreviousBookmark_Clicked));

	//Gtk::MenuItem* menuitemFormatCode;
	builder->get_widget("menuitemFormatCode", menuitemFormatCode);
	menuitemFormatCode->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFormatCode_Clicked));

	//Gtk::MenuItem* menuitemFormatCodeAll;
	builder->get_widget("menuitemFormatCodeAll", menuitemFormatCodeAll);
	menuitemFormatCodeAll->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFormatCodeAll_Clicked));

	
	//Gtk::MenuItem* menuitemFormatCodeOptions;
	builder->get_widget("menuitemFormatCodeOptions", menuitemFormatCodeOptions);
	menuitemFormatCodeOptions->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFormatCodeOptions_Clicked));

	//Gtk::MenuItem* menuitemCodeRepositoryShowWindow;
	builder->get_widget("menuitemCodeRepositoryShowWindow", menuitemCodeRepositoryShowWindow);
	menuitemCodeRepositoryShowWindow->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCodeRepositoryShowWindow_Clicked));

	//Gtk::MenuItem* menuitemCodeRepositoryStoreSelectedText;
	builder->get_widget("menuitemCodeRepositoryStoreSelectedText", menuitemCodeRepositoryStoreSelectedText);
	menuitemCodeRepositoryStoreSelectedText->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCodeRepositoryStoreSelectedText_Clicked));

	//Gtk::MenuItem* menuitemListOpcodes;
	builder->get_widget("menuitemListOpcodes", menuitemListOpcodes);
	menuitemListOpcodes->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemListOpcodes_Clicked));

	//Gtk::MenuItem* menuitemCSoundOpcodesRepository;
	builder->get_widget("menuitemCSoundOpcodesRepository", menuitemCSoundOpcodesRepository);
	menuitemCSoundOpcodesRepository->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCSoundOpcodesRepository_Clicked));

	
	//FOLDING
	//Gtk::MenuItem* menuitemFolding;
	builder->get_widget("menuitemFolding", menuitemFolding);

	builder->get_widget("menuitemFoldSingle", menuitemFoldSingle);
	menuitemFoldSingle->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFoldSingle_Clicked));
	builder->get_widget("menuitemFoldAll", menuitemFoldAll);
	menuitemFoldAll->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFoldAll_Clicked));
	builder->get_widget("menuitemUnFoldAll", menuitemUnFoldAll);
	menuitemUnFoldAll->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemUnFoldAll_Clicked));
	
	
	//Gtk::MenuItem* menuitemLineEndings;
	builder->get_widget("menuitemLineEndings", menuitemLineEndings);
	menuitemLineEndings->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemLineEndings_drop_down));
	
	//Gtk::MenuItem* menuitemLineEndingsCRLF;
	builder->get_widget("menuitemLineEndingsCRLF", menuitemLineEndingsCRLF);
	menuitemLineEndingsCRLF->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemLineEndingsCRLF_Clicked));

	//Gtk::MenuItem* menuitemLineEndingsCR;
	builder->get_widget("menuitemLineEndingsCR", menuitemLineEndingsCR);
	menuitemLineEndingsCR->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemLineEndingsCR_Clicked));

	//Gtk::MenuItem* menuitemLineEndingsLF;
	builder->get_widget("menuitemLineEndingsLF", menuitemLineEndingsLF);
	menuitemLineEndingsLF->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemLineEndingsLF_Clicked));

	//Gtk::MenuItem* menuitemResetTextZoom;
	builder->get_widget("menuitemResetTextZoom", menuitemResetTextZoom);
	menuitemResetTextZoom->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonResetZoom_Clicked));

	

	//MENU VIEW
	//builder->get_widget("menuitemLineNumbers", menuitemLineNumbers);
	menuitemLineNumbers->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemLineNumbers_Clicked));

	//builder->get_widget("menuitemExplorer", menuitemExplorer);
	menuitemExplorer->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemExplorer_Clicked));

	//builder->get_widget("menuitemOnlineHelp", menuitemOnlineHelp);
	menuitemOnlineHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemOnlineHelp_Clicked));

	//builder->get_widget("menuitemToolbar", menuitemToolbar);
	menuitemToolbar->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemToolbar_Clicked));

	//Gtk::MenuItem* menuitemShowAllTools;
	builder->get_widget("menuitemShowAllTools", menuitemShowAllTools);
	menuitemShowAllTools->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemShowAllTools_Clicked));
	
	//Gtk::MenuItem* menuitemHideAllTools;
	builder->get_widget("menuitemHideAllTools", menuitemHideAllTools);
	menuitemHideAllTools->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemHideAllTools_Clicked));
	
	//Gtk::MenuItem* menuitemFullCode;
	builder->get_widget("menuitemFullCode", menuitemFullCode);
	menuitemFullCode->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFullCode_Clicked));
	
	//Gtk::MenuItem* menuitemSplitHorizontal;
	builder->get_widget("menuitemSplitHorizontal", menuitemSplitHorizontal);
	menuitemSplitHorizontal->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSplitHorizontal_Clicked));
	
	//Gtk::MenuItem* menuitemSplitHorizontalOrcSco;
	builder->get_widget("menuitemSplitHorizontalOrcSco", menuitemSplitHorizontalOrcSco);
	menuitemSplitHorizontalOrcSco->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSplitHorizontalOrcSco_Clicked));
	
	//Gtk::MenuItem* menuitemSplitVertical;
	builder->get_widget("menuitemSplitVertical", menuitemSplitVertical);
	menuitemSplitVertical->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSplitVertical_Clicked));
	
	//Gtk::MenuItem* menuitemSplitVerticalOrcSco;
	builder->get_widget("menuitemSplitVerticalOrcSco", menuitemSplitVerticalOrcSco);
	menuitemSplitVerticalOrcSco->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSplitVerticalOrcSco_Clicked));
	
	//Gtk::MenuItem* menuitemShowCode;
	builder->get_widget("menuitemShowCode", menuitemShowCode);
	menuitemShowCode->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemShowCode_Clicked));
	
	//Gtk::MenuItem* menuitemShowCompiler;
	builder->get_widget("menuitemShowCompiler", menuitemShowCompiler);
	menuitemShowCompiler->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemShowCompiler_Clicked));
	
	//Gtk::MenuItem* menuitemShowHelp;
	builder->get_widget("menuitemShowHelp", menuitemShowHelp);
	menuitemShowHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemShowHelp_Clicked));
	
	//Gtk::MenuItem* menuitemShowHideWhiteSpaces;
	builder->get_widget("menuitemShowHideWhiteSpaces", menuitemShowHideWhiteSpaces);
	menuitemShowHideWhiteSpaces->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemShowHideWhiteSpaces_Clicked));
	
	//Gtk::MenuItem* menuitemShowHideEOLS;
	builder->get_widget("menuitemShowHideEOLS", menuitemShowHideEOLS);
	menuitemShowHideEOLS->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemShowHideEOLS_Clicked));
	
	//Gtk::MenuItem* menuitemNavigateBackward;
	builder->get_widget("menuitemNavigateBackward", menuitemNavigateBackward);
	menuitemNavigateBackward->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemNavigateBackward_Clicked));
	
	//Gtk::MenuItem* menuitemNavigateForward;
	builder->get_widget("menuitemNavigateForward", menuitemNavigateForward);
	menuitemNavigateForward->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemNavigateForward_Clicked));
	
	//Gtk::MenuItem* menuitemSetCaretOnPrimaryView;
	builder->get_widget("menuitemSetCaretOnPrimaryView", menuitemSetCaretOnPrimaryView);
	menuitemSetCaretOnPrimaryView->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSetCaretOnPrimaryView_Clicked));
	
	//Gtk::MenuItem* menuitemSetCaretOnSecondaryView;
	builder->get_widget("menuitemSetCaretOnSecondaryView", menuitemSetCaretOnSecondaryView);
	menuitemSetCaretOnSecondaryView->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemSetCaretOnSecondaryView_Clicked));
	
	//Gtk::MenuItem* menuitemScreenPositionUP;
	builder->get_widget("menuitemScreenPositionUP", menuitemScreenPositionUP);
	menuitemScreenPositionUP->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemScreenPositionUP_Clicked));
	
	//Gtk::MenuItem* menuitemScreenPositionDOWN;
	builder->get_widget("menuitemScreenPositionDOWN", menuitemScreenPositionDOWN);
	menuitemScreenPositionDOWN->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemScreenPositionDOWN_Clicked));
	
	//Gtk::MenuItem* menuitemScreenPositionLEFT;
	builder->get_widget("menuitemScreenPositionLEFT", menuitemScreenPositionLEFT);
	menuitemScreenPositionLEFT->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemScreenPositionLEFT_Clicked));
	
	//Gtk::MenuItem* menuitemScreenPositionRIGHT;
	builder->get_widget("menuitemScreenPositionRIGHT", menuitemScreenPositionRIGHT);
	menuitemScreenPositionRIGHT->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemScreenPositionRIGHT_Clicked));
	
	//Gtk::MenuItem* menuitemScreenPositionRESET;
	builder->get_widget("menuitemScreenPositionRESET", menuitemScreenPositionRESET);
	menuitemScreenPositionRESET->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemScreenPositionRESET_Clicked));

	//Gtk::MenuItem* menuitemCompilerWindow;
	builder->get_widget("menuitemCompilerWindow", menuitemCompilerWindow);
	menuitemCompilerWindow->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonCompilerWindow_Clicked));
	

	

	//MENU TOOLS
	//Gtk::MenuItem* menuitemCompile;
	builder->get_widget("menuitemCompile", menuitemCompile);
	menuitemCompile->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCompile_Clicked));

	builder->get_widget("menuitemCompileWithAdditionalFlags", menuitemCompileWithAdditionalFlags);
	//menuitemCompileWithAdditionalFlags->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCompileWithAdditionalFlags_Clicked));

	//Gtk::MenuItem* menuitemCompileExternal;
	//builder->get_widget("menuitemCompileExternal", menuitemCompileExternal);
	//menuitemCompileExternal->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCompileExternal_Clicked));

	//Gtk::MenuItem* menuitemUseWinXoundFlags;
	//builder->get_widget("menuitemUseWinXoundFlags", menuitemUseWinXoundFlags);
	menuitemUseWinXoundFlags->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemUseWinXoundFlags_Clicked));

	//Gtk::MenuItem* menuitemRunExternalGUI;
	builder->get_widget("menuitemRunExternalGUI", menuitemRunExternalGUI);
	menuitemRunExternalGUI->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemRunExternalGUI_Clicked));

	/*//CABBAGE
	builder->get_widget("menuitemCabbage", menuitemCabbage);	
	builder->get_widget("menuitemCabbageUpdate", menuitemCabbageUpdate);
	menuitemCabbageUpdate->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageUpdate_Clicked));
	builder->get_widget("menuitemCabbageExportVSTI", menuitemCabbageExportVSTI);
	menuitemCabbageExportVSTI->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageExportVSTI_Clicked));
	builder->get_widget("menuitemCabbageExportVST", menuitemCabbageExportVST);
	menuitemCabbageExportVST->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageExportVST_Clicked));
	builder->get_widget("menuitemCabbageExportAU", menuitemCabbageExportAU);
	menuitemCabbageExportAU->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageExportAU_Clicked));
	builder->get_widget("menuitemCabbageLookForInternetUpdates", menuitemCabbageLookForInternetUpdates);
	menuitemCabbageLookForInternetUpdates->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageLookForInternetUpdates_Clicked));
	builder->get_widget("menuitemGoToCabbageWebsite", menuitemGoToCabbageWebsite);
	menuitemGoToCabbageWebsite->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemGoToCabbageWebsite_Clicked));
*/
	
	//Gtk::MenuItem* menuitemAnalysis;
	builder->get_widget("menuitemAnalysis", menuitemAnalysis);
	menuitemAnalysis->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemAnalysis_Clicked));

	//Gtk::MenuItem* menuitemMediaPlayer;
	builder->get_widget("menuitemMediaPlayer", menuitemMediaPlayer);
	menuitemMediaPlayer->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemMediaPlayer_Clicked));

	//Gtk::MenuItem* menuitemExternalWaveEditor;
	builder->get_widget("menuitemExternalWaveEditor", menuitemExternalWaveEditor);
	menuitemExternalWaveEditor->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemExternalWaveEditor_Clicked));

	//Gtk::MenuItem* menuitemCalculator;
	builder->get_widget("menuitemCalculator", menuitemCalculator);
	menuitemCalculator->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCalculator_Clicked));

	Gtk::MenuItem* menuitemTerminal;
	builder->get_widget("menuitemTerminal", menuitemTerminal);
	menuitemTerminal->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemTerminal_Clicked));

	//Gtk::MenuItem* menuitemWinXoundTest;
	builder->get_widget("menuitemWinXoundTest", menuitemWinXoundTest);
	menuitemWinXoundTest->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemWinXoundTest_Clicked));

	
	
	//MENU HELP
	//Gtk::MenuItem* menuitemWinXoundHelp;
	builder->get_widget("menuitemWinXoundHelp", menuitemWinXoundHelp);
	menuitemWinXoundHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemWinXoundHelp_Clicked));

	//Gtk::MenuItem* menuitemOpcodeHelp;
	builder->get_widget("menuitemOpcodeHelp", menuitemOpcodeHelp);
	menuitemOpcodeHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemOpcodeHelp_Clicked));

	//Gtk::MenuItem* menuitemOpcodesIndexHelp;
	builder->get_widget("menuitemOpcodesIndexHelp", menuitemOpcodesIndexHelp);
	menuitemOpcodesIndexHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemOpcodesIndexHelp_Clicked));

	//Gtk::MenuItem* menuitemCSoundHelp;
	builder->get_widget("menuitemCSoundHelp", menuitemCSoundHelp);
	menuitemCSoundHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCSoundHelp_Clicked));

	//Gtk::MenuItem* menuitemCSoundFlagsHelp;
	builder->get_widget("menuitemCSoundFlagsHelp", menuitemCSoundFlagsHelp);
	menuitemCSoundFlagsHelp->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_toolbuttonCSoundFlags_Clicked));

	//Gtk::MenuItem* menuitemCSoundTutorial;
	builder->get_widget("menuitemCSoundTutorial", menuitemCSoundTutorial);
	menuitemCSoundTutorial->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCSoundTutorial_Clicked));

	//Gtk::MenuItem* menuitemFlossManual;
	builder->get_widget("menuitemFlossManual", menuitemFlossManual);
	menuitemFlossManual->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemFlossManual_Clicked));

	/*//Gtk::MenuItem* menuitemCabbageManual;
	builder->get_widget("menuitemCabbageManual", menuitemCabbageManual);
	menuitemCabbageManual->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageManual_Clicked));
	//Gtk::MenuItem* menuitemCabbageManualInternet;
	builder->get_widget("menuitemCabbageManualInternet", menuitemCabbageManualInternet);
	menuitemCabbageManualInternet->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemCabbageManualInternet_Clicked));
	*/
	
	//Gtk::MenuItem* menuitemUdoDatabase;
	builder->get_widget("menuitemUdoDatabase", menuitemUdoDatabase);
	menuitemUdoDatabase->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemUdoDatabase_Clicked));

	//Gtk::MenuItem* menuitemAbout;
	builder->get_widget("menuitemAbout", menuitemAbout);
	menuitemAbout->signal_activate().connect(sigc::mem_fun(*this, &wxMain::on_menuitemAbout_Clicked));

	
	
}


///////////////////////////////
//NEW
void wxMain::on_menuitemNew_Clicked()
{
	
	////AddNewEditor(UNTITLED_CSD);

	Gtk::Dialog dialog("WinXound - New File", TRUE, FALSE);

	//CSOUND CSD
	Glib::ustring iconname = wxGLOBAL->getIconsPath();
	iconname.append("/csound_48.png");
	
	Gtk::Button csoundCsd("CSound _CSD", TRUE);
	Gtk::Image *imageCSoundCsd = new Gtk::Image(iconname);
	csoundCsd.set_image(*imageCSoundCsd);
	csoundCsd.set_image_position(Gtk::POS_TOP);
	imageCSoundCsd->show();

	//CSOUND ORC
	iconname = wxGLOBAL->getIconsPath();
	iconname.append("/orchestra_48.png");
	
	Gtk::Button csoundOrc("CSound _ORC", TRUE);
	Gtk::Image *imageCSoundOrc = new Gtk::Image(iconname);
	csoundOrc.set_image(*imageCSoundOrc);
	csoundOrc.set_image_position(Gtk::POS_TOP);
	imageCSoundOrc->show();

	//CSOUND SCO
	iconname = wxGLOBAL->getIconsPath();
	iconname.append("/score_48.png");
	
	Gtk::Button csoundSco("CSound _SCO", TRUE);
	Gtk::Image *imageCSoundSco = new Gtk::Image(iconname);
	csoundSco.set_image(*imageCSoundSco);
	csoundSco.set_image_position(Gtk::POS_TOP);
	imageCSoundSco->show();

	//PYTHON
	iconname = wxGLOBAL->getIconsPath();
	iconname.append("/python_48.png");
	
	Gtk::Button python("_Python", TRUE);
	Gtk::Image *imagePython= new Gtk::Image(iconname);
	python.set_image(*imagePython);
	python.set_image_position(Gtk::POS_TOP);
	imagePython->show();

	//LUA
	iconname = wxGLOBAL->getIconsPath();
	iconname.append("/lua_48.png");
	
	Gtk::Button lua("_Lua", TRUE);
	Gtk::Image *imageLua= new Gtk::Image(iconname);
	lua.set_image(*imageLua);
	lua.set_image_position(Gtk::POS_TOP);
	imageLua->show();

	//CABBAGE
	iconname = wxGLOBAL->getIconsPath();
	iconname.append("/cabbage_48.png");
	
	Gtk::Button cabbage("Ca_bbage", TRUE);
	Gtk::Image *imageCabbage= new Gtk::Image(iconname);
	cabbage.set_image(*imageCabbage);
	cabbage.set_image_position(Gtk::POS_TOP);
	imageCabbage->show();



	dialog.add_action_widget(csoundCsd, 0);
	dialog.add_action_widget(csoundOrc, 1);
	dialog.add_action_widget(csoundSco, 2);
	dialog.add_action_widget(python, 3);
	dialog.add_action_widget(lua, 4);
	dialog.add_action_widget(cabbage, 5);
	dialog.show_all_children();

	
	//dialog.signal_key_press_event().connect(
	//	sigc::mem_fun(*this, &wxMain::on_newdialog_keypress));
	//dialog.add_button("_CSound", 0);
	//dialog.add_button("_Python", 1);
	//dialog.add_button("_Lua", 2);
	//dialog.add_mnemonic(GDK_C, csound);

	
	dialog.set_resizable(FALSE);
	dialog.set_icon_from_file(Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath()));
	dialog.show_all();


	
	//SET DIALOG POSITION (CENTER PARENT)
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
	dialog.move(x - dialog.get_width() / 2,
	            y - dialog.get_height() / 2);


	int result = dialog.run();

	if(result == 0)
		AddNewEditor(UNTITLED_CSD);
	else if(result == 1)
		AddNewEditor(UNTITLED_ORC);
	else if(result == 2)
		AddNewEditor(UNTITLED_SCO);
	else if(result == 3)
		AddNewEditor(UNTITLED_PY);
	else if(result == 4)
		AddNewEditor(UNTITLED_LUA);
	else if(result == 5)
		AddNewEditor(UNTITLED_CABBAGE);

	delete imageCSoundCsd;
	delete imageCSoundOrc;
	delete imageCSoundSco;
	delete imagePython;
	delete imageLua;
	delete imageCabbage;

}

bool wxMain::on_newdialog_keypress(GdkEventKey* event)
{
	//Gtk::Dialog* dialog = reinterpret_cast<Gtk::Dialog*>(event->window);
	//std::cout << event->keyval << std::endl;

	//guint modifiers = gtk_accelerator_get_default_mod_mask ();

	if (event->keyval == GDK_Control_L ||
	    event->keyval == GDK_Control_R)
    {
		if(event->type == GDK_KEY_PRESS)
			std::cout << "CONTROL PRESSED" << std::endl;
		else if(event->type==GDK_KEY_RELEASE)
			std::cout << "CONTROL RELEASED" << std::endl;
	}

	return false;
}


///////////////////////////////
//OPEN
void wxMain::on_menuitemOpen_Clicked()
{
	Gtk::FileChooserDialog dialog("Open File",
	                              Gtk::FILE_CHOOSER_ACTION_OPEN);
	dialog.set_transient_for(*this);

	//Add response buttons the the dialog:
	dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
	dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

	dialog.set_select_multiple(TRUE);
	dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

	//Add filters, so that only certain file types can be selected:
	Gtk::FileFilter filter_supported_files;
	filter_supported_files.set_name("Supported files");
	//filter_supported_files.add_mime_type("text/plain");
	filter_supported_files.add_pattern("*.csd");
	filter_supported_files.add_pattern("*.orc");
	filter_supported_files.add_pattern("*.sco");
	filter_supported_files.add_pattern("*.py");
	filter_supported_files.add_pattern("*.pyw");
	filter_supported_files.add_pattern("*.lua");
	dialog.add_filter(filter_supported_files);

	Gtk::FileFilter filter_csound_files;
	filter_csound_files.set_name("CSound files");
	filter_csound_files.add_pattern("*.csd");
	filter_csound_files.add_pattern("*.orc");
	filter_csound_files.add_pattern("*.sco");
	dialog.add_filter(filter_csound_files);

	Gtk::FileFilter filter_python_files;
	filter_python_files.set_name("Python files");
	filter_python_files.add_pattern("*.py");
	filter_python_files.add_pattern("*.pyw");
	dialog.add_filter(filter_python_files);

	Gtk::FileFilter filter_lua_files;
	filter_lua_files.set_name("Lua files");
	filter_lua_files.add_pattern("*.lua");
	dialog.add_filter(filter_lua_files);

	Gtk::FileFilter filter_any;
	filter_any.set_name("Any files");
	filter_any.add_pattern("*");
	dialog.add_filter(filter_any);

	//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
	if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
	                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
	{
		dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
	}

	
	int result = dialog.run();
	dialog.hide();

	//Handle the response:
	switch(result)
	{
		case(Gtk::RESPONSE_OK):
		{
			wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

			//OLD:Single file
			//std::string filename = dialog.get_filename();
			//AddNewEditor(filename.c_str());

			//NEW:Multiple files
			std::vector<std::string> files = dialog.get_filenames();
			//for (uint i = 0; i < dialog.get_filenames().size(); i++)
			for (uint i = 0; i < files.size(); i++)
			{
				AddNewEditor(files[i].c_str());
			}

			break;
		}
		case(Gtk::RESPONSE_CANCEL):
		{
			//std::cout << "Cancel clicked." << std::endl;
			break;
		}
	}


}

//////////////////////////////
//SAVE
void wxMain::on_menuitemSave_Clicked()
{
	if (ActiveEditor() == NULL) return;

	//g_str_equal(ActiveEditor()->FileName, UNTITLED_PY)
	if (ActiveEditor()->FileName == UNTITLED_CSD ||
	    ActiveEditor()->FileName == UNTITLED_PY ||
	    ActiveEditor()->FileName == UNTITLED_LUA ||
	    ActiveEditor()->FileName == UNTITLED_ORC ||
	    ActiveEditor()->FileName == UNTITLED_SCO ||
	    ActiveEditor()->FileName == UNTITLED_CABBAGE ||
	    !Glib::file_test(ActiveEditor()->FileName, Glib::FILE_TEST_EXISTS)) //IMPORTED FILES
	{
		//MenuFileSaveAs.PerformClick();
		this->on_menuitemSaveAs_Clicked();
		return;
	}

	try
	{
		//ActiveEditor.SaveFile(ActiveEditor.FileName);
		ActiveEditor()->textEditor->SaveFile(ActiveEditor()->FileName.c_str());
		//ActiveEditor.textEditor.SetSavePoint();
		ActiveEditor()->textEditor->setSavePoint();
		//ActiveEditor.FileIsReadOnly = false;
		ActiveEditor()->FileIsReadOnly = false;
	}
	catch (const Glib::FileError & ex) //TODO: CHECK SAVE FILE UAE !!!
		//catch (System.UnauthorizedAccessException uae)
	{
		if(ex.code() == Glib::FileError::ACCESS_DENIED)
		{
			ShowUaeMessageOnSave(ex.what().c_str());
			ActiveEditor()->FileIsReadOnly = true;
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemSave_Clicked Error");
	}
}



/////////////////////////////
//SAVE AS
void wxMain::on_menuitemSaveAs_Clicked()
{
	if (ActiveEditor() == NULL) return;

	Glib::ustring oldType = ActiveEditor()->getFileType();

	try
	{

		Gtk::FileChooserDialog dialog("Save File",
		                              Gtk::FILE_CHOOSER_ACTION_SAVE);
		dialog.set_transient_for(*this);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		//Add response buttons the the dialog:
		dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
		dialog.add_button(Gtk::Stock::SAVE, Gtk::RESPONSE_OK);

		
		//Set dialog filename		
		if(!Glib::file_test(ActiveEditor()->FileName, Glib::FILE_TEST_EXISTS)) //Untitled file
		{
			dialog.set_current_name(Glib::path_get_basename(ActiveEditor()->FileName.lowercase()));
		}
		else
			dialog.set_current_name(Glib::path_get_basename(ActiveEditor()->FileName));

		
		//Add filters
		Gtk::FileFilter filter_supported_files;
		filter_supported_files.set_name("Supported files");
		filter_supported_files.add_pattern("*.csd");
		filter_supported_files.add_pattern("*.orc");
		filter_supported_files.add_pattern("*.sco");
		filter_supported_files.add_pattern("*.py");
		filter_supported_files.add_pattern("*.pyw");
		filter_supported_files.add_pattern("*.lua");
		filter_supported_files.add_pattern("*.txt");
		dialog.add_filter(filter_supported_files);
		
		Gtk::FileFilter filter_any;
		filter_any.set_name("Any files");
		filter_any.add_pattern("*");
		dialog.add_filter(filter_any);

		
		//Ask for overwrite
		dialog.set_do_overwrite_confirmation(TRUE);

		//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
		if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
		                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
		{
			dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
		}

		

		int result = dialog.run();

		if (result != Gtk::RESPONSE_OK)
		{
			ActiveEditor()->textEditor->setFocus();
			return;
		}
		
		try
		{
			//ActiveEditor.SaveFile(ActiveEditor.FileName);
			std::string filename = dialog.get_filename();
			wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();
			
			ActiveEditor()->FileName = (gchar*)filename.c_str();
			ActiveEditor()->textEditor->SaveFile(ActiveEditor()->FileName.c_str());
			//ActiveEditor.textEditor.SetSavePoint();
			ActiveEditor()->textEditor->setSavePoint();
			//ActiveEditor.FileIsReadOnly = false;
			ActiveEditor()->FileIsReadOnly = false;
		}
		catch (const Glib::FileError& ex)
			//catch (System.UnauthorizedAccessException uae)
		{
			if(ex.code() == Glib::FileError::ACCESS_DENIED)
			{
				ShowUaeMessageOnSave(ex.what().c_str());
				ActiveEditor()->FileIsReadOnly = true;
			}
			return;
		}
		catch(...)
		{
			//wxGlobal.wxMessageError(ex.Message,
			//"Form Main - MenuFileSave_Click - Saving error");
			wxGLOBAL->DebugPrint("wxMain - on_menuitemSaveAs_Clicked Error");
			return;
		}


		//this.Text = wxGlobal.TITLE + " - " + ActiveEditor.FileName;
		Glib::ustring temp = TITLE;
		temp.append(" - ");
		temp.append(ActiveEditor()->FileName);
		wxMainWindow->set_title(temp);
		//wxMainWindow->set_title(ActiveEditor()->FileName);
		//ActiveEditor.Parent.Text =
		//	Path.GetFileName(ActiveEditor.FileName);
		notebookCode->set_tab_label_text(*ActiveEditor(), 
		          						 Glib::path_get_basename(ActiveEditor()->FileName));

		AddRecentFilesToMenu(ActiveEditor()->FileName.c_str());


		//Reset highlight (if necessary)
		if (oldType != ActiveEditor()->getFileType())
			SetHighlightLanguage(ActiveEditor(), true);

		//ActiveEditor.SetFocus();
		ActiveEditor()->textEditor->setFocus();
	}

	//catch (System.UnauthorizedAccessException uae)
	catch (const Glib::FileError & ex)
	{
		ShowUaeMessageOnSave(ex.what().c_str());
		ActiveEditor()->FileIsReadOnly = true;
		return;
	}
	
	catch(...)
	{
		//wxGlobal.wxMessageError(ex.Message,
		//"Form Main - MenuFileSaveAs_Click - Saving error");
		wxGLOBAL->DebugPrint("wxMain - on_menuitemSaveAs_Clicked Error");
	}
}

void wxMain::on_menuitemSaveAll_Clicked()
{
	try
	{
		//if (wxTabCode.TabPages.Count > 0)
		if(notebookCode->get_n_pages() > 0)
		{
			//Int32 tabIndex = wxTabCode.SelectedIndex;
			//foreach (TabPage tp in wxTabCode.TabPages)
			//{
				//System.Diagnostics.Debug.WriteLine(tp.Name);
				//wxTabCode.SelectedTab = tp;
				//MenuFileSave_Click(this, null);
			//}
			//wxTabCode.SelectedIndex = tabIndex;

			gint oldIndex = notebookCode->get_current_page();
			for(int i = 0; i < notebookCode->get_n_pages(); i++)
			{				notebookCode->set_current_page(i);
				this->on_menuitemSave_Clicked();
			}
			notebookCode->set_current_page(oldIndex);
		}
	}
	catch(...)
	{
		//wxGlobal.wxMessageError(ex.Message,
		//"Form Main - MenuFileSaveAll_Click");
		wxGLOBAL->DebugPrint("wxMain - on_menuitemSaveAll_Clicked Error");
	}
}

void wxMain::on_menuitemClose_Clicked()
{
	this->RemoveEditor(this->ActiveEditor());
}

void wxMain::on_menuitemCloseAll_Clicked()
{
	this->RemoveAllEditors();
}

void wxMain::on_menuitemFileInfo_Clicked()
{
	if (ActiveEditor() == NULL) return;

	Glib::ustring permissions = 
		(ActiveEditor()->FileIsReadOnly ? "Read Only" : "Read/Write");



	wxGLOBAL->ShowMessageBox(this,
	                         Glib::ustring::compose("FILENAME: %1\n" 
	                                                "FULL PATH: %2\n\n"
	                                                //"FILE TYPE:\t%3\n" + Path.GetExtension(ActiveEditor.FileName) + newline +
	                                                "FILE PERMISSIONS: %3\n"
	                                                "LINE ENDINGS: %4\n"
	                                                "ENCODING: Unicode (UTF-8)\n"
	                                                "TOTAL LINES: %5\n"
	                                                "TOTAL CHARS: %6\n",
	                                                g_path_get_basename(ActiveEditor()->FileName.c_str()),
	                                                ActiveEditor()->FileName,
	                                                permissions,
	                                                ActiveEditor()->textEditor->GetEolModeReport(),
	                                                ActiveEditor()->textEditor->getLinesCount(),
	                                                ActiveEditor()->textEditor->getTextLength()).c_str(),
	                         "WinXound - File Info",
	                         Gtk::BUTTONS_OK);
}



void wxMain::on_menuitemImportOrcScoToNewCSD_Clicked()
{
	//IMPORT AND CREATE A NEW CSD FILE
	try
	{
		Gtk::FileChooserDialog dialog("Open Orc/Sco Files",
		                              Gtk::FILE_CHOOSER_ACTION_OPEN);
		dialog.set_transient_for(*this);

		//Add response buttons the the dialog:
		dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
		dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

		//dialog.set_select_multiple(TRUE);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		//Add filters, so that only certain file types can be selected:
		Gtk::FileFilter filter_supported_files;
		filter_supported_files.set_name("Orc/Sco files");
		filter_supported_files.add_pattern("*.orc");
		filter_supported_files.add_pattern("*.sco");		
		dialog.add_filter(filter_supported_files);

		//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
		if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
		                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
		{
			dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
		}

		int result = dialog.run();

		//Handle the response:
		if(result != Gtk::RESPONSE_OK) return;

		wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

		//THIRD FIELD: IS_IMPORT_OPERATION = TRUE
		AddNewEditor(dialog.get_filename().c_str(), "", TRUE); 
	
	}
	catch(...)
	{
		//wxGlobal.wxMessageError(ex.Message,
		//"Form Main - MenuFileSaveAll_Click");
		wxGLOBAL->DebugPrint("wxMain - on_menuitemImportOrcScoToNewCSD_Clicked Error");
	}

}
void wxMain::on_menuitemImportOrcSco_Clicked()
{
	//IMPORT ORC/SCO INTO THE CURRENT/EXISTING CSD FILE

	if(ActiveEditor() == NULL) return;
	
	try
	{
		Gtk::FileChooserDialog dialog("Open Orc/Sco Files",
		                              Gtk::FILE_CHOOSER_ACTION_OPEN);
		dialog.set_transient_for(*this);

		//Add response buttons the the dialog:
		dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
		dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

		//dialog.set_select_multiple(TRUE);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		//Add filters, so that only certain file types can be selected:
		Gtk::FileFilter filter_supported_files;
		filter_supported_files.set_name("Orc/Sco files");
		filter_supported_files.add_pattern("*.orc");
		filter_supported_files.add_pattern("*.sco");		
		dialog.add_filter(filter_supported_files);

		//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
		if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
		                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
		{
			dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
		}

		int result = dialog.run();

		//Handle the response:
		if(result != Gtk::RESPONSE_OK) return;

		wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

		//Import Orc/Sco into the existing csd file
		wxImportExport* mImport = new wxImportExport();
		if(Glib::str_has_suffix(dialog.get_filename(), ".orc"))
		{
			//Import ORC
			mImport->ImportORC(ActiveEditor()->textEditor, dialog.get_filename().c_str());
			//Import SCO
			//We have to change the extension name -> SCO
			Glib::ustring scoExtension = wxGLOBAL->getFileNameWithoutExtension(dialog.get_filename());
			scoExtension.append(".sco");
			mImport->ImportSCO(ActiveEditor()->textEditor, scoExtension.c_str());
		}
		else
		{
			//Import SCO
			mImport->ImportSCO(ActiveEditor()->textEditor, dialog.get_filename().c_str());
			//Import ORC
			//We have to change the extension name -> ORC
			Glib::ustring orcExtension = wxGLOBAL->getFileNameWithoutExtension(dialog.get_filename());
			orcExtension.append(".orc");
			mImport->ImportORC(ActiveEditor()->textEditor, orcExtension.c_str());
		}
		delete mImport;

		
	}
	catch(...)
	{
		//wxGlobal.wxMessageError(ex.Message,
		//"Form Main - MenuFileSaveAll_Click");
		wxGLOBAL->DebugPrint("wxMain - on_menuitemImportOrcSco_Clicked Error");
	}
}

void wxMain::on_menuitemImportOrc_Clicked()
{
	//IMPORT ORC INTO THE CURRENT CSD FILE

	if(ActiveEditor() == NULL) return;
		
	try
	{
		Gtk::FileChooserDialog dialog("Open Orc File",
		                              Gtk::FILE_CHOOSER_ACTION_OPEN);
		dialog.set_transient_for(*this);

		//Add response buttons the the dialog:
		dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
		dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

		//dialog.set_select_multiple(TRUE);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		//Add filters, so that only certain file types can be selected:
		Gtk::FileFilter filter_supported_files;
		filter_supported_files.set_name("Orc files");
		filter_supported_files.add_pattern("*.orc");
		dialog.add_filter(filter_supported_files);

		//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
		if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
		                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
		{
			dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
		}

		int result = dialog.run();

		//Handle the response:
		if(result != Gtk::RESPONSE_OK) return;

		wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

		//Import ORC
		wxImportExport* mImport = new wxImportExport();
		mImport->ImportORC(ActiveEditor()->textEditor, dialog.get_filename().c_str());
		delete mImport;
		
	}
	catch(...)
	{
		//wxGlobal.wxMessageError(ex.Message,
		//"Form Main - MenuFileSaveAll_Click");
		wxGLOBAL->DebugPrint("wxMain - on_menuitemImportOrc_Clicked Error");
	}



}

void wxMain::on_menuitemImportSco_Clicked()
{
	//IMPORT SCO INTO THE CURRENT CSD FILE

	if(ActiveEditor() == NULL) return;
	
	try
	{
		Gtk::FileChooserDialog dialog("Open Sco File",
		                              Gtk::FILE_CHOOSER_ACTION_OPEN);
		dialog.set_transient_for(*this);

		//Add response buttons the the dialog:
		dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
		dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

		//dialog.set_select_multiple(TRUE);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		//Add filters, so that only certain file types can be selected:
		Gtk::FileFilter filter_supported_files;
		filter_supported_files.set_name("Sco files");
		filter_supported_files.add_pattern("*.sco");
		dialog.add_filter(filter_supported_files);

		//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
		if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
		                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
		{
			dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
		}

		int result = dialog.run();

		//Handle the response:
		if(result != Gtk::RESPONSE_OK) return;

		wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

		//Import SCO
		wxImportExport* mImport = new wxImportExport();
		mImport->ImportSCO(ActiveEditor()->textEditor, dialog.get_filename().c_str());
		delete mImport;

	}
	catch(...)
	{
		//wxGlobal.wxMessageError(ex.Message,
		//"Form Main - MenuFileSaveAll_Click");
		wxGLOBAL->DebugPrint("wxMain - on_menuitemImportSco_Clicked Error");
	}
}

void wxMain::on_menuitemExportOrcSco_Clicked()
{
	wxImportExport* mExport = new wxImportExport();
	mExport->ExportOrcSco(ActiveEditor()->textEditor, ActiveEditor()->FileName.c_str());
	delete mExport;
}

void wxMain::on_menuitemExportOrc_Clicked()
{
	wxImportExport* mExport = new wxImportExport();
	mExport->ExportORC(ActiveEditor()->textEditor, ActiveEditor()->FileName.c_str());
	delete mExport;
}

void wxMain::on_menuitemExportSco_Clicked()
{
	wxImportExport* mExport = new wxImportExport();
	mExport->ExportSCO(ActiveEditor()->textEditor, ActiveEditor()->FileName.c_str());
	delete mExport;
}

void wxMain::on_menuitemPrint_Clicked()
{
	if(ActiveEditor() == NULL) return;
		wxPrint* print = new wxPrint();
	print->PrintText(ActiveEditor()->textEditor->getText(), false, ActiveEditor()->FileName);
	delete print;
}

void wxMain::on_menuitemPrintPreview_Clicked()
{
	if(ActiveEditor() == NULL) return;
	wxPrint* print = new wxPrint();
	print->PrintText(ActiveEditor()->textEditor->getText(), true, ActiveEditor()->FileName);
	delete print;
}

void wxMain::on_menuitemSettings_Clicked()
{
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
		
	wxSETTINGS->showWindowAt(x,y);
}

void wxMain::on_settings_save_Clicked()
{
	//std::cerr << "wxMain::on_settings_save_Clicked" << std::endl;

	//Apply settings to the current visible widgets

	if (notebookCode->get_n_pages() > 0)
	{
		wxEditor* tempEditor = NULL;
		//foreach (TabPage tp in wxTabCode.TabPages)
		for(int index = 0; index < notebookCode->get_n_pages(); index++)
		{
			tempEditor = (wxEditor*)notebookCode->get_nth_page(index);

			tempEditor->textEditor->setTextEditorFont(
				wxSETTINGS->EditorProperties.DefaultFontName.c_str(),
				wxSETTINGS->EditorProperties.DefaultFontSize);
			
			tempEditor->textEditor->setTabIndent(
				wxSETTINGS->EditorProperties.DefaultTabSize);
			
			tempEditor->textEditor->setShowMatchingBracket(
				wxSETTINGS->EditorProperties.ShowMatchingBracket);
			
			tempEditor->textEditor->setShowVerticalRuler(
				wxSETTINGS->EditorProperties.ShowVerticalRuler);
			
			tempEditor->textEditor->setMarkCaretLine(
				wxSETTINGS->EditorProperties.MarkCaretLine);

			tempEditor->textEditor->ShowFoldLine(
				wxSETTINGS->EditorProperties.ShowFoldLine);

			//if (tempEditor.FileName.ToLower().EndsWith(".py") ||
			//    tempEditor.FileName.ToLower().EndsWith(".pyw"))
			if(Glib::str_has_suffix(tempEditor->FileName.lowercase(), ".py") ||
			   Glib::str_has_suffix(tempEditor->FileName.lowercase(), ".pyw"))
			{
				//Python syntax
				if (wxSETTINGS->EditorProperties.UseMixedPython)
				{
					tempEditor->textEditor->setHighlight("winxoundpython");
					tempEditor->ConfigureEditorForPythonMixed(NULL);
				}
				else
				{
					tempEditor->textEditor->setHighlight("python");
					tempEditor->ConfigureEditorForPython();
				}
				//Refresh syntax
				tempEditor->textEditor->RefreshSyntax();

			}
			//else if (tempEditor.FileName.ToLower().EndsWith(".lua"))
			else if(Glib::str_has_suffix(tempEditor->FileName.lowercase(), ".lua"))
			{
				//Lua Syntax
				tempEditor->ConfigureEditorForLua();
			}
			//else if (tempEditor.FileName.ToLower().EndsWith(".csd"))
			else if(Glib::str_has_suffix(tempEditor->FileName.lowercase(), ".csd"))
			{
				//CSoundSyntax
				tempEditor->ConfigureEditorForCSound(NULL, "");
			}
			else
				tempEditor->ConfigureEditorForText();

			tempEditor->RefreshExplorer();
		}


	}

	//Set environment variables !!!!! ALSO FOR OPCODEDIR
	wxSETTINGS->SetEnvironmentVariables();

	//Set Addition Flags items
	UpdateAdditionalFlagsMenu();

	//Set Compiler font
	try
	{
		compiler->SetCompilerFont(wxSETTINGS->General.CompilerFontName,
		                          wxSETTINGS->General.CompilerFontSize);
	}
	catch(...)
	{
		wxSETTINGS->General.CompilerFontName = "Monospace";
		wxSETTINGS->General.CompilerFontSize = 10;
		compiler->SetCompilerFont(wxSETTINGS->General.CompilerFontName,
		                          wxSETTINGS->General.CompilerFontSize);
		wxGLOBAL->DebugPrint("wxMain - wxMain constructor SetCompilerFont Error");
	}


    if (ActiveEditor() != NULL) ActiveEditor()->SetFocus();
	
}

bool wxMain::on_wxMainWindow_delete_event(GdkEventAny* event)
{

	//Save LastSessionFiles
	wxSETTINGS->LastSessionFilesClear();
	for(int index = notebookCode->get_n_pages() - 1; index > -1; index--)
	{
		wxEditor* temp = (wxEditor*)notebookCode->get_nth_page(index);
		wxSETTINGS->LastSessionFilesInsert(temp->FileName.c_str());
	}

	
	
	//Check document modifications 
	if (RemoveAllEditors() == false)
	{
		return true; //TRUE: STOP CLOSE EVENT
	}

	//wxGlobal.Settings.General.UseWinXoundFlags = 
	//	MenuToolsUseDefaultFlags.Checked;
	wxSETTINGS->General.UseWinXoundFlags = 
		menuitemUseWinXoundFlags->get_active();

	//Save current window position
	gint x, y;
    wxMainWindow->get_position(x, y); 
	//std::cerr << "x: " << x << "y: " << y << std::endl;
	wxSETTINGS->General.WindowPosition.set_x(x); //OriginX;
	wxSETTINGS->General.WindowPosition.set_y(y); //OriginY
	wxSETTINGS->General.WindowSize.set_x(wxMainWindow->get_width());
	wxSETTINGS->General.WindowSize.set_y(wxMainWindow->get_height());
	/*
	//Save current compiler window position
	compilerWindow->get_position(x, y);
	wxSETTINGS->General.CompilerWindowPosition.set_x(x); //OriginX;
	wxSETTINGS->General.CompilerWindowPosition.set_y(y); //OriginY
	wxSETTINGS->General.CompilerWindowSize.set_x(compilerWindow->get_width());
	wxSETTINGS->General.CompilerWindowSize.set_y(compilerWindow->get_height());
	*/
	
	wxSETTINGS->SaveSettings();	
	std::cout << "wxMain: Settings Saved!" << std::endl;



	//Check for temporary files and delete them:
	Glib::ustring settingsDirectory = wxGLOBAL->getSettingsPath();
	Glib::Dir dir_final(settingsDirectory);

	for(Glib::Dir::iterator p = dir_final.begin(); p != dir_final.end(); ++p)
	{
		Glib::ustring temp = *p;

		if(temp.find(WINXOUND_UNTITLED) != Glib::ustring::npos)                   
		{
			//Remove temporary file
			temp.insert(0, "/");
			temp.insert(0, settingsDirectory);

			//std::cout << "File to remove:" << temp << std::endl;
			std::remove(temp.c_str());
		}
	}
	

	return false;
}

void wxMain::on_menuitemExit_Clicked()
{
	if(on_wxMainWindow_delete_event(NULL) == false)
		Gtk::Main::quit();

	//Gtk::Main::quit();
}

//bool thisclass::mymethod() { return false; }
bool wxMain::on_main_quit_signal()
{
	std::cerr << "wxMain::on_main_quit_signal" << std::endl;
	return false;
}


////////////////////////////////////////////////////////////////////////////////
//MENU EDIT
////////////////////////////////////////////////////////////////////////////////
void wxMain::on_menuitemUndo_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformUndo();
}

void wxMain::on_menuitemRedo_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformRedo();
}

void wxMain::on_menuitemCut_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformCut();
}

void wxMain::on_menuitemCopy_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformCopy();
}

void wxMain::on_menuitemPaste_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformPaste();
}

void wxMain::on_menuitemDelete_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformDelete();
}

void wxMain::on_menuitemSelectAll_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->PerformSelectAll();
}






void wxMain::on_menuitemFindAndReplace_Clicked()
{
	if(ActiveEditor() == NULL) return;
	
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width();
	y += wxMainWindow->get_height();
	FindAndReplace->showWindowAt(x, y);

	if(ActiveEditor()->textEditor->getSelectedText().size() > 0)
	{
		FindAndReplace->entryFindText->set_text(ActiveEditor()->textEditor->getSelectedText());
	}
	
}

void wxMain::on_menuitemFindNext_Clicked()
{
	if(ActiveEditor() == NULL) return;
	
	if(ActiveEditor()->textEditor->getSelectedText().size() > 0)
	{
		FindAndReplace->entryFindText->set_text(ActiveEditor()->textEditor->getSelectedText());
		FindNext(FindAndReplace->entryFindText->get_text().c_str(),
		         FindAndReplace->checkbuttonMatchWholeWord->get_active(),
		         FindAndReplace->checkbuttonMatchCase->get_active());
	}
	else if (strlen(FindAndReplace->entryFindText->get_text().c_str()) > 0)
	{
		FindNext(FindAndReplace->entryFindText->get_text().c_str(),
		         FindAndReplace->checkbuttonMatchWholeWord->get_active(),
		         FindAndReplace->checkbuttonMatchCase->get_active());
	}
}

bool wxMain::FindNext(const gchar* text, 
                      bool MatchWholeWord,
                      bool MatchCase)
{
	if(ActiveEditor() == NULL) return false;
	gint ret = ActiveEditor()->textEditor->FindText(text, MatchWholeWord, MatchCase,
	                                                false, true, true, false);
	return (ret > -1);
}

void wxMain::on_menuitemFindPrevious_Clicked()
{
	if(ActiveEditor() == NULL) return;

	if(ActiveEditor()->textEditor->getSelectedText().size() > 0)
	{
		FindAndReplace->entryFindText->set_text(ActiveEditor()->textEditor->getSelectedText());
		FindPrevious(FindAndReplace->entryFindText->get_text().c_str(),
		             FindAndReplace->checkbuttonMatchWholeWord->get_active(),
		             FindAndReplace->checkbuttonMatchCase->get_active());
	}
	else if (strlen(FindAndReplace->entryFindText->get_text().c_str()) > 0)
	{
		FindPrevious(FindAndReplace->entryFindText->get_text().c_str(),
		             FindAndReplace->checkbuttonMatchWholeWord->get_active(),
		             FindAndReplace->checkbuttonMatchCase->get_active());
	}
}
bool wxMain::FindPrevious(const gchar* text, 
                          bool MatchWholeWord,
                          bool MatchCase)
{
	if(ActiveEditor() == NULL) return false;
	gint ret = ActiveEditor()->textEditor->FindText(text, MatchWholeWord, MatchCase,
	                                                true, true, true, false);
	return (ret > -1);
}

void wxMain::Replace(const gchar* StringToFind,
                     const gchar* ReplaceString,
                     bool MatchWholeWord,
                     bool MatchCase,
                     bool FromCaretPosition,
                     bool FCPUp, bool ReplaceAll)
{
	if (ActiveEditor() == NULL) return;
	
	if (ReplaceAll)
	{
		gint ret = ActiveEditor()->textEditor->ReplaceAllText(StringToFind, ReplaceString, MatchWholeWord,
		                                           MatchCase, FromCaretPosition, FCPUp);

		if(ret > 0)
			FindAndReplace->labelInfo->set_text(
				Glib::ustring::compose(" %1 occurence(s) replaced", ret));
		else
			FindAndReplace->labelInfo->set_text(" Text not found");
	}
	else
	{
		ActiveEditor()->textEditor->ReplaceText(ReplaceString);
	}
}



void wxMain::on_menuitemJumpToCaret_Clicked()
{
	if (ActiveEditor() == NULL) return;
    ActiveEditor()->textEditor->ScrollCaret();
}

void wxMain::on_menuitemFindLineNumber_Clicked()
{
	if (ActiveEditor() == NULL) return;
	
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;

	FindLine->spinbuttonLineNumber->set_range(1, ActiveEditor()->textEditor->getLinesCount());	
	FindLine->showWindowAt(x, y);
}

void wxMain::GoToLineNumber(gint linenumber)
{
	if (ActiveEditor() == NULL) return;

	ActiveEditor()->textEditor->GoToLine(linenumber - 1);
	ActiveEditor()->textEditor->SelectLine(linenumber - 1);
	ActiveEditor()->SetFocus();
}

void wxMain::on_menuitemCommentLine_Clicked()
{
	if (ActiveEditor() == NULL) return;
    ActiveEditor()->Comment();
}

void wxMain::on_menuitemRemoveCommentLine_Clicked()
{
	if (ActiveEditor() == NULL) return;
    ActiveEditor()->UnComment();
}

void wxMain::on_menuitemInsertRemoveBookmark_Clicked()
{
	if (ActiveEditor() == NULL) return;
	ActiveEditor()->InsertRemoveBookmark();
}

void wxMain::on_menuitemRemoveAllBookmarks_Clicked()
{
	if (ActiveEditor() == NULL) return;
	ActiveEditor()->RemoveAllBookmarks();
}

void wxMain::on_menuitemGoToNextBookmark_Clicked()
{
	if (ActiveEditor() == NULL) return;
	ActiveEditor()->GoToNextBookmark();
}

void wxMain::on_menuitemGoToPreviousBookmark_Clicked()
{
	if (ActiveEditor() == NULL) return;
	ActiveEditor()->GoToPreviousBookmark();	
}

void wxMain::on_menuitemFormatCode_Clicked()
{
	this->FormatCode(FALSE);
}

void wxMain::on_menuitemFormatCodeAll_Clicked()
{
	this->FormatCode(TRUE);
}

void wxMain::FormatCode(bool allLines)
{
	if (ActiveEditor() == NULL) return;

	try
	{
		//Only csd file formatting is supported
		if(Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".csd") == false) 
			return;

		gint currentFirstVisibleLine =
			ActiveEditor()->textEditor->getFirstVisibleLine();

		gint currentLine = 
			ActiveEditor()->textEditor->getCurrentLineNumber();



		//Check for Bookmarks
		//List<Int32> bookmarks = new List<Int32>();
		std::vector<int> bookmarks;
		if (ActiveEditor()->textEditor->hasBookmarks())
		{
			gint mCurLine = 0;
			gint mBookLine = 0;
			do
			{
				mBookLine = ActiveEditor()->textEditor->MarkerNext(mCurLine, 1);
				if (mBookLine == -1) break;

				//bookmarks.Add(mBookLine);
				bookmarks.push_back(mBookLine);
				mCurLine = mBookLine + 1;
			}
			while (true);

			ActiveEditor()->textEditor->RemoveAllBookmarks();
		}


		//FORMAT THE CODE
		//If no selected text: format all document
		//if (string.IsNullOrEmpty(ActiveEditor.textEditor.GetSelectedText()))
		if(allLines)
		{
			mFormatter->FormatCode(ActiveEditor()->textEditor, Opcodes);
		}
		else if(ActiveEditor()->textEditor->getSelectedText().size() <= 0)
		{
			mFormatter->FormatCode(
			    ActiveEditor()->textEditor,
			    Opcodes,
				ActiveEditor()->textEditor->getLineNumberFromPosition(
					ActiveEditor()->textEditor->getSelectionStart()),
				ActiveEditor()->textEditor->getLineNumberFromPosition(
					ActiveEditor()->textEditor->getSelectionEnd()));
		}
		//If selected text: format only selection
		else
		{
			mFormatter->FormatCode(
			    ActiveEditor()->textEditor,
			    Opcodes,
				ActiveEditor()->textEditor->getLineNumberFromPosition(
					ActiveEditor()->textEditor->getSelectionStart()),
				ActiveEditor()->textEditor->getLineNumberFromPosition(
					ActiveEditor()->textEditor->getSelectionEnd()) + 1);
		}


		//Restore bookmarks
		if (bookmarks.size() > 0)
		{
			//foreach (Int32 i in bookmarks)
			for (uint i = 0; i < bookmarks.size(); i++)
			{
				ActiveEditor()->textEditor->InsertBookmarkAt(bookmarks[i]);
			}

			ActiveEditor()->RefreshListBoxBookmarks();
		}

		ActiveEditor()->textEditor->setFirstVisibleLine(currentFirstVisibleLine);
		ActiveEditor()->textEditor->setCaretPosition(
			//ActiveEditor()->textEditor->getPositionFromLineNumber(currentLine));
		    ActiveEditor()->textEditor->getLineEndPosition(currentLine));                                        

		ActiveEditor()->SetFocus();


	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemFormatCode_Clicked Error", "ERROR");
	}
}

void wxMain::on_menuitemFormatCodeOptions_Clicked()
{
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
	mFormatter->showWindowAt(x, y);
}

void wxMain::on_menuitemCodeRepositoryShowWindow_Clicked()
{
	isDragAndDrop = false;
	//vpanedMain->set_position(wxMainWindow->get_height() / TAB_POPUP_RATIO);
	vpanedMain->set_position(wxMainWindow->get_height() - oldHeight);
	notebookCompiler->set_current_page(4);
	notebookCompiler->grab_focus();

	if(compilerWindow->is_visible())
		compilerWindow->present();
}

void wxMain::on_menuitemCodeRepositoryStoreSelectedText_Clicked()
{
	if(ActiveEditor() == NULL) return;
	if(ActiveEditor()->textEditor->getSelectedText().size() > 0)
	{
		Glib::ustring temp = ActiveEditor()->textEditor->getSelectedText();
		mRepository->InsertText(temp);
	}
}

void wxMain::on_menuitemListOpcodes_Clicked()
{
	if(ActiveEditor() == NULL) return;
	if(ActiveEditor()->CurrentWordsList == "") return;

	Glib::ustring curWord = ActiveEditor()->textEditor->getCurrentWordForAutoc();
	//std::cout << curWord << std::endl;

	ActiveEditor()->textEditor->AutocSetFillups(" ");
	//ActiveEditor()->textEditor->AutocStops(" ");

	/*
	if(g_hash_table_lookup(Opcodes, curWord.c_str()) == NULL)	
	else*/
	{

		ActiveEditor()->textEditor->AutocShow(curWord.size(),
		                                      ActiveEditor()->CurrentWordsList.c_str());  
	}
}

void wxMain::on_menuitemCSoundOpcodesRepository_Clicked()
{
	isDragAndDrop = false;
	//vpanedMain->set_position(wxMainWindow->get_height() / TAB_POPUP_RATIO);
	vpanedMain->set_position(wxMainWindow->get_height() - oldHeight);
	notebookCompiler->set_current_page(3);
	notebookCompiler->grab_focus();

	if(compilerWindow->is_visible())
		compilerWindow->present();
}

void wxMain::on_menuitemFoldSingle_Clicked()
{
	if (ActiveEditor() == NULL) return;

	//int line = ActiveEditor.textEditor.GetCurrentLineNumber();
	gint line = ActiveEditor()->textEditor->getCurrentLineNumber();
	//int level = ActiveEditor.textEditor.GetFocusedEditor.GetFoldLevel(line);
	gint level = ActiveEditor()->textEditor->getFoldLevel(line);

	//if (Convert.ToBoolean(level & SciConst.SC_FOLDLEVELHEADERFLAG))
	if(level & SC_FOLDLEVELHEADERFLAG)
	{
		//ActiveEditor.textEditor.GetFocusedEditor.ToggleFold(line);
		ActiveEditor()->textEditor->ToggleFold(line);
	}
	//else if (ActiveEditor.textEditor.GetFocusedEditor.GetFoldParent(line) != -1)
	else if (ActiveEditor()->textEditor->getFoldParent(line) != -1)
	{
		//line = ActiveEditor.textEditor.GetFocusedEditor.GetFoldParent(line);
		line = ActiveEditor()->textEditor->getFoldParent(line);
		//ActiveEditor.textEditor.GetFocusedEditor.ToggleFold(line);
		ActiveEditor()->textEditor->ToggleFold(line);

		//ActiveEditor.textEditor.SetCaretPosition(ActiveEditor.textEditor.GetLineEndPosition(line));
		//ActiveEditor.textEditor.SetCaretPosition(ActiveEditor.textEditor.GetPositionFromLineNumber(line));
		ActiveEditor()->textEditor->setCaretPosition(
			ActiveEditor()->textEditor->getPositionFromLineNumber(line));
	}
}
void wxMain::on_menuitemFoldAll_Clicked()
{
	if (ActiveEditor() == NULL) return;

	//TextView* tempView = ActiveEditor.textEditor.GetFocusedEditor;

	//The "colourize" refresh is needed to get the correct foldlevel state.
	//Otherwise the "for" cycle doesn't work correctly.
	//tempView.Colourise(0, -1);
	ActiveEditor()->textEditor->Colourise(0, -1);

	//for (Int32 i = 0; i < ActiveEditor.textEditor.GetLines(); i++)
	for (gint i = 0; i < ActiveEditor()->textEditor->getLinesCount(); i++)
	{
		//int level = tempView.GetFoldLevel(i);
		gint level = ActiveEditor()->textEditor->getFoldLevel(i);
			
		//if (Convert.ToBoolean(level & SciConst.SC_FOLDLEVELHEADERFLAG))
		if(level & SC_FOLDLEVELHEADERFLAG)
		{
			//if (tempView.GetFoldExpanded(i))
			if(ActiveEditor()->textEditor->getFoldExpanded(i))
			{
				//tempView.ToggleFold(i);
				ActiveEditor()->textEditor->ToggleFold(i);
			}
		}
	}
}
void wxMain::on_menuitemUnFoldAll_Clicked()
{

	if (ActiveEditor() == NULL) return;

	//TextView tempView = ActiveEditor.textEditor.GetFocusedEditor;

	//The "colourize" refresh is needed to get the correct foldlevel state.
	//Otherwise the "for" cycle doesn't work correctly.
	//tempView.Colourise(0, -1);
	ActiveEditor()->textEditor->Colourise(0, -1);

	//for (Int32 i = 0; i < ActiveEditor.textEditor.GetLines(); i++)
	for (gint i = 0; i < ActiveEditor()->textEditor->getLinesCount(); i++)
	{
		//if (!tempView.GetFoldExpanded(i))
		if(!ActiveEditor()->textEditor->getFoldExpanded(i))
		{
			//tempView.ToggleFold(i);
			ActiveEditor()->textEditor->ToggleFold(i);
		}
	}
}


void wxMain::on_menuitemLineEndingsCRLF_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->ConvertEOL(SC_EOL_CRLF);
	ActiveEditor()->textEditor->setEolMode(SC_EOL_CRLF);
	onZoomNotification(ActiveEditor());
	onSavePointLeft();
}

void wxMain::on_menuitemLineEndingsCR_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->ConvertEOL(SC_EOL_CR);
	ActiveEditor()->textEditor->setEolMode(SC_EOL_CR);
	onZoomNotification(ActiveEditor());
	onSavePointLeft();
}

void wxMain::on_menuitemLineEndingsLF_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->ConvertEOL(SC_EOL_LF);
	ActiveEditor()->textEditor->setEolMode(SC_EOL_LF);
	onZoomNotification(ActiveEditor());
	onSavePointLeft();
}


////////////////////////////////////////////////////////////////////////////////
//MENU VIEW
////////////////////////////////////////////////////////////////////////////////
void wxMain::on_menuitemLineNumbers_Clicked()
{
	try
	{
		wxSETTINGS->EditorProperties.ShowLineNumbers = menuitemLineNumbers->get_active();
			//!wxSETTINGS->EditorProperties.ShowLineNumbers;
		
		if(notebookCode->get_n_pages() < 1) return;

		wxEditor* temp = NULL;
		for(int index = 0; index < notebookCode->get_n_pages(); index++)
		{
			temp = (wxEditor*)notebookCode->get_nth_page(index);
			temp->textEditor->setShowLineNumbers(wxSETTINGS->EditorProperties.ShowLineNumbers);
		}

	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemLineNumbers_Clicked Error");
	}
}

void wxMain::on_menuitemExplorer_Clicked()
{
	try
	{
		wxSETTINGS->EditorProperties.ShowExplorer = menuitemExplorer->get_active();
			//!wxSETTINGS->EditorProperties.ShowExplorer;

		if(notebookCode->get_n_pages() < 1) return;

		wxEditor* temp = NULL;
		for(int index = 0; index < notebookCode->get_n_pages(); index++)
		{
			temp = (wxEditor*)notebookCode->get_nth_page(index);
			temp->setShowExplorer(wxSETTINGS->EditorProperties.ShowExplorer);
		}
		
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemExplorer_Clicked Error");
	}	
}

void wxMain::on_menuitemOnlineHelp_Clicked()
{
	try
	{
		wxSETTINGS->EditorProperties.ShowIntelliTip = menuitemOnlineHelp->get_active();
			//!wxSETTINGS->EditorProperties.ShowIntelliTip;

		if(notebookCode->get_n_pages() < 1) return;

		wxEditor* temp = NULL;
		for(int index = 0; index < notebookCode->get_n_pages(); index++)
		{
			temp = (wxEditor*)notebookCode->get_nth_page(index);
			temp->setShowIntelliTip(wxSETTINGS->EditorProperties.ShowIntelliTip);
		}
		
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemOnlineHelp_Clicked Error");
	}
}

void wxMain::on_menuitemToolbar_Clicked()
{
	try
	{
		wxSETTINGS->General.ShowToolbar = menuitemToolbar->get_active();
			//!wxSETTINGS->General.ShowToolbar;

		toolbar->set_visible(wxSETTINGS->General.ShowToolbar);
		
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemToolbar_Clicked Error");
	}
}

void wxMain::on_menuitemShowAllTools_Clicked()
{
	try
	{
		wxSETTINGS->General.ShowToolbar = true;
		wxSETTINGS->EditorProperties.ShowLineNumbers = true;
		wxSETTINGS->EditorProperties.ShowExplorer = true;
		wxSETTINGS->EditorProperties.ShowIntelliTip = true;

		menuitemToolbar->set_active(true);
		menuitemLineNumbers->set_active(true);
		menuitemExplorer->set_active(true);
		menuitemOnlineHelp->set_active(true);

		toolbar->set_visible(true);
		
		if(notebookCode->get_n_pages() < 1) return;

		wxEditor* temp = NULL;
		for(int index = 0; index < notebookCode->get_n_pages(); index++)
		{
			temp = (wxEditor*)notebookCode->get_nth_page(index);
			temp->setShowIntelliTip(true);
			temp->setShowExplorer(true);
			temp->textEditor->setShowLineNumbers(true);
		}

	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemShowAllTools_Clicked Error");
	}
}

void wxMain::on_menuitemHideAllTools_Clicked()
{
	try
	{
		wxSETTINGS->General.ShowToolbar = false;
		wxSETTINGS->EditorProperties.ShowLineNumbers = false;
		wxSETTINGS->EditorProperties.ShowExplorer = false;
		wxSETTINGS->EditorProperties.ShowIntelliTip = false;

		menuitemToolbar->set_active(false);
		menuitemLineNumbers->set_active(false);
		menuitemExplorer->set_active(false);
		menuitemOnlineHelp->set_active(false);

		toolbar->set_visible(false);
		
		if(notebookCode->get_n_pages() < 1) return;

		wxEditor* temp = NULL;
		for(int index = 0; index < notebookCode->get_n_pages(); index++)
		{
			temp = (wxEditor*)notebookCode->get_nth_page(index);
			temp->setShowIntelliTip(false);
			temp->setShowExplorer(false);
			temp->textEditor->setShowLineNumbers(false);
		}

	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemHideAllTools_Clicked Error");
	}
}

void wxMain::on_menuitemFullCode_Clicked()
{
	try
	{
		if (ActiveEditor() != NULL)
		{
			ActiveEditor()->textEditor->setFocusOnPrimaryView();
			if(ActiveEditor()->textEditor->getIsSplitted() == true)
			{
				ActiveEditor()->textEditor->RemoveSplit();
			}
			ActiveEditor()->textEditor->setFocusOnPrimaryView();
		}
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemFullCode_Clicked Error");
	}
}

void wxMain::on_menuitemSplitHorizontal_Clicked()
{
	try
	{
		if (ActiveEditor() != NULL)
		{
			//ShowFullCode();
			//if(ActiveEditor()->textEditor->getIsSplitted() == true) on_menuitemFullCode_Clicked(); 

			ActiveEditor()->textEditor->Split();
			
			if(ActiveEditor()->textEditor->getIsSplitted() == true)
			{
				gint firstVisibleLine = ActiveEditor()->textEditor->getFirstVisibleLineAtView(1);
				ActiveEditor()->textEditor->setFirstVisibleLineAtView(firstVisibleLine, 2);
			}
			
			ActiveEditor()->textEditor->setFocusOnPrimaryView();
		}
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemSplitHorizontal_Clicked Error");
	}
}

void wxMain::on_menuitemSplitHorizontalOrcSco_Clicked()
{
	if(ActiveEditor() == NULL) return;
	
	ActiveEditor()->textEditor->Split();
	ShowOrcSco();
}

void wxMain::on_menuitemSplitVertical_Clicked()
{
	try
	{
		if (ActiveEditor() != NULL)
		{
			//ShowFullCode();
			//if(ActiveEditor()->textEditor->getIsSplitted() == true) on_menuitemFullCode_Clicked(); 
			ActiveEditor()->textEditor->SplitVertical();
			
			if(ActiveEditor()->textEditor->getIsSplitted() == true)
			{
				gint firstVisibleLine = ActiveEditor()->textEditor->getFirstVisibleLineAtView(1);
				ActiveEditor()->textEditor->setFirstVisibleLineAtView(firstVisibleLine, 2);
			}
			
			ActiveEditor()->textEditor->setFocusOnPrimaryView();
		}
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemSplitVertical_Clicked Error");
	}
}

void wxMain::on_menuitemSplitVerticalOrcSco_Clicked()
{
	if(ActiveEditor() == NULL) return;
	
	ActiveEditor()->textEditor->SplitVertical();
	ShowOrcSco();
}

void wxMain::ShowOrcSco()
{
	try
	{
		if (ActiveEditor() == NULL) return;
		if (!ActiveEditor()->textEditor->getIsSplitted() == true) return;

		gint mFindPos = -1;
		Glib::ustring StringToFind = "";

		//Find <CsInstruments> 
		StringToFind = "<CsInstruments>";
		mFindPos = ActiveEditor()->textEditor->FindText(StringToFind.c_str(), true, true, false,
		                                                false, false, true,
		                                                0, ActiveEditor()->textEditor->getTextLength());
														//???-1
		if (mFindPos > -1)
		{
			ActiveEditor()->textEditor->setFocusOnPrimaryView();
			//ActiveEditor()->textEditor->setCaretPosition(mFindPos);
			gint curLine = ActiveEditor()->textEditor->getLineNumberFromPosition(mFindPos);
			ActiveEditor()->textEditor->setFirstVisibleLineAtView(curLine, 1);
		}

		//Find <CsScore> 
		StringToFind = "<CsScore>";
		mFindPos = ActiveEditor()->textEditor->FindText(StringToFind.c_str(), true, true, false,
		                                                false, false, true,
		                                                0, ActiveEditor()->textEditor->getTextLength());

		if (mFindPos > -1)
		{
			ActiveEditor()->textEditor->setFocusOnSecondaryView();
			//ActiveEditor()->textEditor->setCaretPosition(mFindPos);
			int curLine = ActiveEditor()->textEditor->getLineNumberFromPosition(mFindPos);
			ActiveEditor()->textEditor->setFirstVisibleLineAtView(curLine, 2);
		}

		ActiveEditor()->textEditor->setFocusOnPrimaryView();
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - ShowOrcSco Error");
	}
}


void wxMain::on_popup_menu_position(int& x, int& y, bool& push_in)
{
	x=0; y=0; push_in=false;
	
	wxMainWindow->get_position(x,y);
	
	gint width = wxMainWindow->get_width();
	gint height = wxMainWindow->get_height();
	//x += width / 3;
	//y += height / 3;
	x += width / 2 - AdditionalFlagsSubMenu->get_width() / 2;
	y += height / 2 - AdditionalFlagsSubMenu->get_height() / 2;

}

bool wxMain::on_key_press_event(GdkEventKey* event)
{
	//wxGLOBAL->DebugPrint("KEY", "PRESSED");

	//Show the Additional Flags List
	if (event->keyval == GDK_l && event->state == GDK_MOD1_MASK) //GDK_CONTROL_MASK)
    {
		//std::cout << "pressed" << std::endl;

		//gtk_signal_emit_by_name : "popup-menu" "show-menu"
		//g_signal_emit_by_name(GTK_OBJECT(toolbuttonCompile->gobj()), "show-menu", NULL);
		//gboolean return_val = FALSE;
		//g_signal_emit_by_name(GTK_OBJECT(toolbuttonCompile->gobj()), "popup-menu", &return_val);
		//g_signal_emit_by_name(GTK_OBJECT(menuitemCompileWithAdditionalFlags->gobj()), "clicked", NULL);

		if(ActiveEditor() != NULL)
		{
			//Two times call to get the right AdditionalFlagsSubMenu size
			AdditionalFlagsSubMenu->popup(sigc::mem_fun(this, &wxMain::on_popup_menu_position),
			                              0,
			                              event->time); //gtk_get_current_event_time()); 
			AdditionalFlagsSubMenu->popup(sigc::mem_fun(this, &wxMain::on_popup_menu_position),
			                              0,
			                              event->time); 
		}

		return false;
	}
	
	
	
	if (event->keyval == GDK_Escape) //Escape key sequence
	{
		//1. Hide autocompletion window
		if(ActiveEditor() != NULL)
		{	
			if(ActiveEditor()->textEditor->AutocActive())
			{
				ActiveEditor()->textEditor->AutocCancel();
				ActiveEditor()->SetFocus();
				return false;
			}
		}
		
		//2. Stop compiler (if active)
		if(compiler->ProcessActive)
		{
			compiler->StopCompiler();
			if(ActiveEditor() != NULL) ActiveEditor()->SetFocus();
			return false;
		}
	
		//3. Minimize notebookCompiler 
		on_menuitemShowCode_Clicked();
	}
	
	return false;
}

void wxMain::on_menuitemShowCode_Clicked()
{
	if(ActiveEditor() != NULL)
	{
		ActiveEditor()->SetFocus();
	}
	
	vpanedMain->set_position(wxMainWindow->get_height() + NOTEBOOK_COMPILER_SIZE);
}

void wxMain::on_menuitemShowCompiler_Clicked()
{
	isDragAndDrop = false;
	/*
	 if (tabControlBuild.Height <= tabControlBuild.MinimumSize.Height)
	 {
		 if (oldHeight > tabControlBuild.MinimumSize.Height)
		 {
			 tabControlBuild.Size = new Size(0, oldHeight);
		 }
	 else tabControlBuild.Size = new Size(0, this.ClientSize.Height / 2);
	 }
	*/

	//vpanedMain->set_position(wxMainWindow->get_height() / TAB_POPUP_RATIO);
	vpanedMain->set_position(wxMainWindow->get_height() - oldHeight);
	notebookCompiler->set_current_page(0);
	notebookCompiler->grab_focus();

	if(compilerWindow->is_visible())
		compilerWindow->present();
}

void wxMain::on_menuitemShowHelp_Clicked()
{
	isDragAndDrop = false;
	//vpanedMain->set_position(wxMainWindow->get_height() / TAB_POPUP_RATIO);
	vpanedMain->set_position(wxMainWindow->get_height() - oldHeight);
	notebookCompiler->set_current_page(1);
	notebookCompiler->grab_focus();

	if(compilerWindow->is_visible())
		compilerWindow->present();
}

void wxMain::on_menuitemShowHideWhiteSpaces_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->setShowSpaces(
		!ActiveEditor()->textEditor->getShowSpaces());                                         
}

void wxMain::on_menuitemShowHideEOLS_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->setShowEOLMarker(
		!ActiveEditor()->textEditor->getShowEOLMarker());
}


void wxMain::on_menuitemNavigateBackward_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->GoToPreviousPos();
}

void wxMain::on_menuitemNavigateForward_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->GoToNextPos();
}

void wxMain::on_menuitemSetCaretOnPrimaryView_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->setFocusOnPrimaryView();
}
void wxMain::on_menuitemSetCaretOnSecondaryView_Clicked()
{
	if(ActiveEditor() == NULL) return;
	ActiveEditor()->textEditor->setFocusOnSecondaryView();
}


void wxMain::on_menuitemScreenPositionUP_Clicked()
{
	gint width = wxMainWindow->get_screen()->get_width(); //Gdk::screen_width();
	gint height = wxMainWindow->get_screen()->get_height() / 2; //Gdk::screen_height() / 2;
	wxMainWindow->move(0, 0);
	wxMainWindow->resize(width, height);
}
void wxMain::on_menuitemScreenPositionDOWN_Clicked()
{
	gint width = wxMainWindow->get_screen()->get_width();
	gint height = wxMainWindow->get_screen()->get_height() / 2;
	wxMainWindow->move(0, height);
	wxMainWindow->resize(width, height);
}
void wxMain::on_menuitemScreenPositionLEFT_Clicked()
{
	gint width = wxMainWindow->get_screen()->get_width() / 2;
	gint height = wxMainWindow->get_screen()->get_height();
	wxMainWindow->move(0, 0);
	wxMainWindow->resize(width, height);
}
void wxMain::on_menuitemScreenPositionRIGHT_Clicked()
{
	gint width = wxMainWindow->get_screen()->get_width() / 2;
	gint height = wxMainWindow->get_screen()->get_height();
	wxMainWindow->move(width, 0);
	wxMainWindow->resize(width, height);
}
void wxMain::on_menuitemScreenPositionRESET_Clicked()
{
	//wxMainWindow->set_position(Gtk::WIN_POS_CENTER_ALWAYS);
	wxMainWindow->move(wxSETTINGS->General.WindowPosition.get_x(), 
	             	   wxSETTINGS->General.WindowPosition.get_y());
	wxMainWindow->resize(wxSETTINGS->General.WindowSize.get_x(), 
	                     wxSETTINGS->General.WindowSize.get_y());
}
	




////////////////////////////////////////////////////////////////////////////////
//MENU TOOLS
////////////////////////////////////////////////////////////////////////////////
void wxMain::on_menuitemCompile_Clicked()
{
	Compile();
}

void wxMain::on_menuitemCompileWithAdditionalFlags_Clicked(Glib::ustring value)
{
	std::cout << "on_menuitemCompileWithAdditionalFlags_Clicked: " << value << std::endl;

	try
	{
		//Example: [Realtime output]: -odac
		//string temp = e.ClickedItem.Text.TrimStart("[".ToCharArray());
		//string options = temp.Split("]".ToCharArray())[1];

		size_t index = value.find("]:");
		
		if(index == Glib::ustring::npos) //DIVIDER "]:" NOT FOUND
		{
			wxGLOBAL->ShowMessageBox("Please specify a valid command ([Description]: Flags)"
			                         "in your WinXound Settings (menu File->Settings->Compiler tab).",
			                         "Additional Flags Invalid syntax",
			                         Gtk::BUTTONS_OK);
			return;
		}
		
		Glib::ustring options = value.substr(index + 2);
                                    
		Compile(options);
	}
	catch (...)
	{
		wxGLOBAL->ShowMessageBox("Please specify a valid command ([Description]: Flags)"
		                         "in your WinXound Settings (menu File->Settings->Compiler tab).",
		                         "Additional Flags Invalid syntax",
		                         Gtk::BUTTONS_OK);
	}
}


void wxMain::UpdateAdditionalFlagsMenu()
{
	//Set the AdditionalFlags menu's


	if(AdditionalFlagsSubMenu != NULL)
		delete AdditionalFlagsSubMenu;

	if(AdditionalFlagsSubMenu2 != NULL)
		delete AdditionalFlagsSubMenu2;

	AdditionalFlagsSubMenu = NULL;
	AdditionalFlagsSubMenu2 = NULL;

	
	try
	{
		AdditionalFlagsSubMenu = new Gtk::Menu();
		AdditionalFlagsSubMenu2 = new Gtk::Menu();

		
		//CSoundAdditionalFlags:
		gchar** flags = g_strsplit(wxSETTINGS->General.CSoundAdditionalFlags.c_str(), "\n", 0);
		int length = wxGLOBAL->ArrayLength(flags);
		for(int i=0; i < length; i++)
		{
			Glib::ustring name  = flags[i];

			//AdditionalFlagsSubMenu
			Gtk::MenuItem* item = Gtk::manage(new Gtk::MenuItem(name));
			item->set_use_underline(FALSE);
			AdditionalFlagsSubMenu->append(*item);
			item->signal_activate().connect(
				sigc::bind<Glib::ustring>(
				sigc::mem_fun(*this, &wxMain::on_menuitemCompileWithAdditionalFlags_Clicked), name)); 

			//AdditionalFlagsSubMenu2
			Gtk::MenuItem* item2 = Gtk::manage(new Gtk::MenuItem(name));
			item2->set_use_underline(FALSE);
			AdditionalFlagsSubMenu2->append(*item2);
			item2->signal_activate().connect(
				sigc::bind<Glib::ustring>(
				sigc::mem_fun(*this, &wxMain::on_menuitemCompileWithAdditionalFlags_Clicked), name)); 
			
		}	
		g_strfreev(flags);


		AdditionalFlagsSubMenu->show_all();
		AdditionalFlagsSubMenu2->show_all();
		
		menuitemCompileWithAdditionalFlags->set_submenu(*AdditionalFlagsSubMenu);
		toolbuttonCompile->set_menu(*AdditionalFlagsSubMenu2);
		

	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain::UpdateAdditionalFlagsMenu Error");
	}			 



}



//TODO: REMOVE !!!
void wxMain::on_menuitemCompileExternal_Clicked()
{

}



//**************************************************
//* executes a command in the background
//* returns TRUE if command was executed  (not the result of the command though..)
bool wxMain::executeCommand(const gchar* path, const gchar* cmd, const gchar* args)
{
	gchar     cmd_line[256];
	gchar   **argv;
	gint      argp;
	bool      rc = FALSE;

	if (cmd == NULL)
		return FALSE;

	if (cmd[0] == '\0')
		return FALSE;

	if (path != NULL)
		gint z = chdir (path);

	snprintf (cmd_line, sizeof (cmd_line), "%s %s", cmd, args);

	rc = g_shell_parse_argv (cmd_line, &argp, &argv, NULL);
	if (!rc)
	{
		g_strfreev (argv);
		return rc;
	}

	rc = g_spawn_async (path, argv, NULL,
	                    GSpawnFlags(G_SPAWN_STDOUT_TO_DEV_NULL | G_SPAWN_SEARCH_PATH),
	                    NULL, NULL, NULL, NULL);

	g_strfreev (argv);

	return rc;
}




void wxMain::on_menuitemUseWinXoundFlags_Clicked()
{
	wxSETTINGS->General.UseWinXoundFlags = menuitemUseWinXoundFlags->get_active();

	if (wxSETTINGS->General.UseWinXoundFlags == true)
	{
		//tabControlBuild.TabPages[0].Text = "Compiler [WinXound flags]";
		//notebookCompiler->set_tab_label_text(*compiler->getTerminalWidget(), 
		//                                     "Compiler [WinXound flags]");
		compilerLabel->set_text("Compiler [WinXound flags]");
	}
	else
	{
		//tabControlBuild.TabPages[0].Text = "Compiler [CsOptions flags]";
		//notebookCompiler->set_tab_label_text(*compiler->getTerminalWidget(), 
		//                                     "Compiler [CsOptions flags]");
		compilerLabel->set_text("Compiler [CsOptions flags]");
	}
	
}

void wxMain::on_menuitemRunExternalGUI_Clicked()
{

	if (ActiveEditor() == NULL) return;

	
	Glib::ustring filename = "\"";
	filename.append(ActiveEditor()->FileName);
	filename.append("\"");

	
	//Save file before to compile
	SaveBeforeCompile();

	//if (ActiveEditor.FileName == "[Untitled]" ||
	//    !File.Exists(ActiveEditor.FileName))
	if (ActiveEditor()->FileName == UNTITLED_CSD ||
	    ActiveEditor()->FileName == UNTITLED_PY ||
	    ActiveEditor()->FileName == UNTITLED_LUA ||
		!Glib::file_test(ActiveEditor()->FileName, Glib::FILE_TEST_EXISTS))
	{
		wxGLOBAL->ShowMessageBox(this,
		                         "Please specify a valid filename before to compile.",
		                         "Compiler Error - Invalid filename",
		                         Gtk::BUTTONS_OK);
		return;
	}


	try
	{
		//Compile with CSOUND EXTERNAL (QuteCSound)
		//if (ActiveEditor.FileName.ToLower().EndsWith(".csd"))
		if (Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".csd"))
		{
			//if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.Winsound) ||
			//    !File.Exists(wxGlobal.Settings.Directory.Winsound))
			if (wxSETTINGS->Directory.Winsound == "" ||
			    !Glib::file_test(wxSETTINGS->Directory.Winsound, Glib::FILE_TEST_EXISTS))
			{
				wxGLOBAL->ShowMessageBox(this,
				                         "Cannot find CSound External GUI Editor!\n"
				                         "Please select a valid path in File->Settings->Directories->CSound External GUI Editor.",
				                         "Compiler Error",
				                         Gtk::BUTTONS_OK);

				return;
			}

			//mCompiler.WinsoundCompiler(ActiveEditor.FileName,
			//           wxGlobal.Settings.Directory.Winsound);
			//this->executeCommand("/usr/bin/", "qutecsound", filename.c_str());
			this->executeCommand(NULL,   //OLD: "/usr/bin/"
			                     wxSETTINGS->Directory.Winsound.c_str(), 
			                     filename.c_str());

		}

		//Compile with PYTHON External (IDLE)
		//else if (ActiveEditor.FileName.ToLower().EndsWith(".py") ||
		//         ActiveEditor.FileName.ToLower().EndsWith(".pyw"))
		else if (Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".py") ||
		         Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".pyw"))
		{

			//if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.PythonIdlePath) ||
			//    !File.Exists(wxGlobal.Settings.Directory.PythonIdlePath))
			if (wxSETTINGS->Directory.PythonIdlePath == "" ||
			    !Glib::file_test(wxSETTINGS->Directory.PythonIdlePath, Glib::FILE_TEST_EXISTS))
			{

				wxGLOBAL->ShowMessageBox(this,
				                         "Cannot find Python Idle (or other GUI) Editor!\n"
				                         "Please select a valid path in File->Settings->Directories->Python Idle.",
				                         "Compiler Error",
				                         Gtk::BUTTONS_OK);

				return;
			}

			//System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.PythonIdlePath,
			//                                 "\"" + ActiveEditor.FileName + "\"");
			this->executeCommand(NULL, 
			                     wxSETTINGS->Directory.PythonIdlePath.c_str(), 
			                     filename.c_str());
		}

		//Compile with LUA External
		//else if (ActiveEditor.FileName.ToLower().EndsWith(".lua"))
		else if (Glib::str_has_suffix(ActiveEditor()->FileName.lowercase().c_str(), ".lua"))
		{
			//if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.LuaGuiPath) ||
			//    !File.Exists(wxGlobal.Settings.Directory.LuaGuiPath))
			if (wxSETTINGS->Directory.LuaGuiPath == "" ||
			    !Glib::file_test(wxSETTINGS->Directory.LuaGuiPath, Glib::FILE_TEST_EXISTS))
			{

				wxGLOBAL->ShowMessageBox(this,
				                         "Cannot find Lua External GUI Editor!\n"
				                         "Please select a valid path in File->Settings->Directories->Lua External GUI.",
				                         "Compiler Error",
				                         Gtk::BUTTONS_OK);

				return;
			}

			//System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.LuaGuiPath,
			//                                 "\"" + ActiveEditor.FileName + "\"");
			this->executeCommand(NULL, 
			                     wxSETTINGS->Directory.LuaGuiPath.c_str(), 
			                     filename.c_str());

		}

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("on_menuitemRunExternalGUI_Clicked", "ERROR");
	}

	
}

void wxMain::on_menuitemAnalysis_Clicked()
{
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
	Analysis->showWindowAt(x, y);
}

void wxMain::on_menuitemMediaPlayer_Clicked()
{
	/* TODO: MAYBE!!! LASTWAVEFILE !!!
	try
	{
		if (File.Exists(wxGlobal.LastWaveFile))
		{
			System.Diagnostics.Process.Start("wmplayer", "\"" + wxGlobal.LastWaveFile + "\"");
		}
		else
		{
			System.Diagnostics.Process.Start("wmplayer");
		}
	}
	*/

	try
	{
		if(Glib::file_test(wxSETTINGS->Directory.WavePlayer, Glib::FILE_TEST_EXISTS))
		{
			/*
			this->executeCommand(
				Glib::path_get_dirname(wxSETTINGS->Directory.WavePlayer).c_str(), 
			    Glib::path_get_basename(wxSETTINGS->Directory.WavePlayer).c_str(), 
			    "");
			*/
			this->executeCommand(NULL, 
			                     wxSETTINGS->Directory.WavePlayer.c_str(), 
			                     "");
			
		}
		else
		{
			wxGLOBAL->ShowMessageBox(this,
			                         "Wave Player Path not found!\n"
			                         "Please select a valid path in File->Settings->Directories->Wave Player field",
			                         "WinXound error!",
			                         Gtk::BUTTONS_OK);
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemMediaPlayer_Clicked Error");
	}		                         
}

void wxMain::on_menuitemExternalWaveEditor_Clicked()
{    
	try
	{
		if(Glib::file_test(wxSETTINGS->Directory.WaveEditor, Glib::FILE_TEST_EXISTS))
		{
			/*
			this->executeCommand(
			                     Glib::path_get_dirname(wxSETTINGS->Directory.WaveEditor).c_str(), 
			                     Glib::path_get_basename(wxSETTINGS->Directory.WaveEditor).c_str(), 
			                     "");
			*/
			this->executeCommand(NULL, 
			                     wxSETTINGS->Directory.WaveEditor.c_str(), 
			                     "");

		}
		else
		{
			wxGLOBAL->ShowMessageBox(this,
			                         "Wave Editor Path not found!\n"
			                         "Please select a valid path in File->Settings->Directories->Wave Editor field",
			                         "WinXound error!",
			                         Gtk::BUTTONS_OK);
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemExternalWaveEditor_Clicked Error");
	}	
}

void wxMain::on_menuitemCalculator_Clicked()
{
	//system("gcalctool&"); ///bin/sh -c gcalctool&
	try
	{
		if(Glib::file_test(wxSETTINGS->Directory.Calculator, Glib::FILE_TEST_EXISTS))
		{
			/*
			this->executeCommand(
				Glib::path_get_dirname(wxSETTINGS->Directory.Calculator).c_str(), 
			    Glib::path_get_basename(wxSETTINGS->Directory.Calculator).c_str(), 
			    "");
			*/
			this->executeCommand(NULL, 
			                     wxSETTINGS->Directory.Calculator.c_str(), 
			                     "");			
			
		}
		else
		{
			wxGLOBAL->ShowMessageBox(this,
			                         "Calculator Path not found!\n"
			                         "Please select a valid path in File->Settings->Directories->Calculator field",
			                         "WinXound error!",
			                         Gtk::BUTTONS_OK);;
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemCalculator_Clicked Error");
	}
	
}

void wxMain::on_menuitemTerminal_Clicked()
{
	isDragAndDrop = false;
	//vpanedMain->set_position(wxMainWindow->get_height() / TAB_POPUP_RATIO);
	vpanedMain->set_position(wxMainWindow->get_height() - oldHeight);
	notebookCompiler->set_current_page(2);
	notebookCompiler->grab_focus();
	terminal->SetFocus();

	if(compilerWindow->is_visible())
		compilerWindow->present();

}

void wxMain::on_menuitemWinXoundTest_Clicked()
{
	try
	{
		Glib::ustring temp = wxGLOBAL->getHelpPath();
		temp.append("/test.csd");

		AddNewEditor(temp.c_str());

		if (ActiveEditor() != NULL)
			ActiveEditor()->SetFocus();
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - menuitemWinXoundTest Error");
	}
}



////////////////////////////////////////////////////////////////////////////////
//MENU HELP
////////////////////////////////////////////////////////////////////////////////
void wxMain::on_menuitemWinXoundHelp_Clicked()
{
	Glib::ustring filename = wxGLOBAL->getHelpPath();
	filename.append("/winxound_help.html");

	if(Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
		HelpBrowser->LoadUri(g_filename_to_uri(filename.c_str(), NULL, NULL));

	on_menuitemShowHelp_Clicked();;
}

void wxMain::on_menuitemOpcodesIndexHelp_Clicked()
{
	if (wxSETTINGS->Directory.CSoundHelpHTML != "")
	{
		//HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
		//                     "\\PartReference.html");
		Glib::ustring mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
		mFile.append("/PartReference.html");
		
		if(Glib::file_test(mFile, Glib::FILE_TEST_EXISTS))
			HelpBrowser->LoadUri(g_filename_to_uri(mFile.c_str(), 
		                                   		   NULL, NULL));

		//MenuViewShowHelp_Click(null, null);
		on_menuitemShowHelp_Clicked();
	}
}

void wxMain::on_menuitemOpcodeHelp_Clicked()
{
	/*
	Glib::ustring curWord = editor->textEditor->getWordAt(editor->textEditor->getCaretPosition());

	//if (Opcodes.Contains(gCurWord))
	//if (gCurWord.length() > 1) // && Opcodes.Contains(gCurWord))
	if (curWord.length() > 0 && 
	    g_hash_table_lookup(Opcodes, curWord.c_str()) != NULL);
	*/

	if(ActiveEditor() == NULL)
	{
		if (wxSETTINGS->Directory.CSoundHelpHTML != "")
		{
			//HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
			//                     "\\PartReference.html");
			Glib::ustring temp = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
			temp.append("/PartReference.html");

			if(Glib::file_test(temp, Glib::FILE_TEST_EXISTS))
				HelpBrowser->LoadUri(g_filename_to_uri(temp.c_str(), 
			                                   		   NULL, NULL));

			//MenuViewShowHelp_Click(null, null);
			on_menuitemShowHelp_Clicked();
		}
		return;
	}
	
	try
	{
		Glib::ustring mFile = "";
		Glib::ustring curWord = 
			ActiveEditor()->textEditor->getWordAt(ActiveEditor()->textEditor->getCaretPosition());
		
		//if (Opcodes.Contains(gCurWord))
		if(Opcodes == NULL) return;
		
		if (curWord.length() > 0 && 
			g_hash_table_lookup(Opcodes, curWord.c_str()) != NULL)
		{

			//wxGLOBAL->DebugPrint("wxMain - on_menuitemOpcodeHelp_Clicked Error",
		    //             		 "wxMain - on_menuitemOpcodeHelp_Clicked Error");
			
			if (wxSETTINGS->Directory.CSoundHelpHTML != "")
			{
				//string mFile =
				//	Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//	"\\" + gCurWord + ".html";
				mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
				mFile.append(Glib::ustring::compose("/%1.html", curWord)); 

				//Some words manual switch
				//0dbfs
				if (curWord == "0dbfs")
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/Zerodbfs.html");
				}

				//tb family
				//if (curWord.StartsWith("tb") &&
				//    char.IsDigit(curWord, 2))
				if(Glib::str_has_prefix(curWord, "tb") &&
				   std::isdigit(*curWord.substr(1,2).c_str()))
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/tb.html");
				}
				//PyAssign family
				if (Glib::str_has_prefix(curWord, "pyassign") ||
				    Glib::str_has_prefix(curWord, "pylassign"))
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/pyassign.html");
				}
				//PyCall family
				if (Glib::str_has_prefix(curWord, "pycall") ||
				    Glib::str_has_prefix(curWord, "pylcall"))
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/pycall.html");
				}
				//PyEval family
				if (Glib::str_has_prefix(curWord, "pyeval") ||
				    Glib::str_has_prefix(curWord, "pyleval"))
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/pyeval.html");
				}
				//PyExec family
				if (Glib::str_has_prefix(curWord, "pyexec") ||
				    Glib::str_has_prefix(curWord, "pylexec"))
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/pyexec.html");
				}
				//Pyrun family
				if (Glib::str_has_prefix(curWord, "pyrun") ||
				    Glib::str_has_prefix(curWord, "pylrun"))
				{
					mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
					mFile.append("/pyrun.html");
				}



				//Finally try to load the html file
				if(Glib::file_test(mFile, Glib::FILE_TEST_EXISTS))
				{
					//HelpBrowser.Navigate(mFile);
					HelpBrowser->LoadUri(g_filename_to_uri(mFile.c_str(), 
					                     NULL, NULL));
					
					//MenuViewShowHelp_Click(null, null);
					on_menuitemShowHelp_Clicked();
				}
				else
				{
					//HelpBrowser.Navigate(Application.StartupPath + "\\Help\\error.html");
					Glib::ustring error = wxGLOBAL->getHelpPath();
					error.append("/error.html");

					if(Glib::file_test(error, Glib::FILE_TEST_EXISTS))
						HelpBrowser->LoadUri(g_filename_to_uri(error.c_str(), 
							                 NULL, NULL));

					//MenuViewShowHelp_Click(null, null);
					on_menuitemShowHelp_Clicked();
				}
			}
		}
		else
		{
			if (wxSETTINGS->Directory.CSoundHelpHTML != "")
			{
				//HelpBrowser.Navigate(Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//                     "\\PartReference.html");
				mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
				mFile.append("/PartReference.html");

				if(Glib::file_test(mFile, Glib::FILE_TEST_EXISTS))
					HelpBrowser->LoadUri(g_filename_to_uri(mFile.c_str(), 
							                 NULL, NULL));

				//MenuViewShowHelp_Click(null, null);
				on_menuitemShowHelp_Clicked();
			}
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemOpcodeHelp_Clicked Error");
	}
	
}

void wxMain::on_menuitemCSoundHelp_Clicked()
{
	HelpBrowser->goHome();
	on_menuitemShowHelp_Clicked();
}

void wxMain::on_menuitemCSoundTutorial_Clicked()
{
	try
	{	
		gtk_show_uri (NULL,
		              "http://www.csounds.com/tutorials",
		              gtk_get_current_event_time(),
		              NULL);
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemCSoundTutorial_Clicked Error");
	}
}

void wxMain::on_menuitemFlossManual_Clicked()
{
	//OLD LINK: "http://en.flossmanuals.net/bin/view/Csound/WebHome",
	try
	{	
		gtk_show_uri (NULL,
		              "http://www.flossmanuals.net/csound/index",
		              gtk_get_current_event_time(),
		              NULL);
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemFlossManual_Clicked Error");
	}
}


void wxMain::on_menuitemCabbageManual_Clicked()
{	
/*	try
	{			
		Glib::ustring docDir = 
			Glib::path_get_dirname(wxSETTINGS->Directory.CabbagePath);
		docDir.append("/docs/cabbage.html");

		//std::cout << "Cabbage Manual path: " << docDir << std::endl;
		
		if(Glib::file_test(docDir, Glib::FILE_TEST_EXISTS))
		{
			HelpBrowser->LoadUri(g_filename_to_uri(docDir.c_str(), NULL, NULL));
			on_menuitemShowHelp_Clicked();
		}
		else
		{
			wxGLOBAL->ShowMessageBox("Cabbage Manual not found!\n"
			                         "Please select a valid path in File->Settings->Directories->Cabbage executable path field.",
			                         "WinXound - Cabbage Manual",
			                         Gtk::BUTTONS_OK);
		}
	}
	
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemCabbageManual_Clicked Error");
	}*/
}

void wxMain::on_menuitemCabbageManualInternet_Clicked()
{
/*	//LINK: http://code.google.com/p/cabbage/wiki/Documentation
	try
	{	
		gtk_show_uri (NULL,
		              "http://code.google.com/p/cabbage/wiki/Documentation",
		              gtk_get_current_event_time(),
		              NULL);
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemCabbageManualInternet_Clicked Error");
	}*/
}

void wxMain::on_menuitemUdoDatabase_Clicked()
{
	//LINK: http://www.csounds.com/udo/
	try
	{	
		gtk_show_uri (NULL,
		              "http://www.csounds.com/udo/",
		              gtk_get_current_event_time(),
		              NULL);
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemUdoDatabase_Clicked Error");
	}
}

void wxMain::on_menuitemAbout_Clicked()
{
	AboutWindow->set_transient_for(*this);
	AboutWindow->set_modal(TRUE);
	
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
	AboutWindow->showWindowAt(x, y);			  
}

void wxMain::on_toolbuttonCSoundOpcodes_Clicked()
{
	if (wxSETTINGS->Directory.CSoundHelpHTML != "")
	{
		Glib::ustring mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
		mFile.append("/PartReference.html");

		if(Glib::file_test(mFile, Glib::FILE_TEST_EXISTS))
			HelpBrowser->LoadUri(g_filename_to_uri(mFile.c_str(), NULL, NULL));

		on_menuitemShowHelp_Clicked();
	}
}

void wxMain::on_toolbuttonCSoundFlags_Clicked()
{
	if (wxSETTINGS->Directory.CSoundHelpHTML != "")
	{
		Glib::ustring mFile = Glib::path_get_dirname(wxSETTINGS->Directory.CSoundHelpHTML);
		mFile.append("/CommandFlags.html");

		if(Glib::file_test(mFile, Glib::FILE_TEST_EXISTS))
			HelpBrowser->LoadUri(g_filename_to_uri(mFile.c_str(), NULL, NULL));

		on_menuitemShowHelp_Clicked();
	}
}

void wxMain::on_toolbuttonZoomIn_Clicked()
{
	if(ActiveEditor() == NULL) return;

	/*
	zoom ++;
	if(zoom > 20) zoom = 20;
	ActiveEditor()->textEditor->setZoomForView1(zoom);
	ActiveEditor()->textEditor->setZoomForView2(zoom);
	*/

	gint zoom = ActiveEditor()->textEditor->getZoom();
	if(zoom < 20)
		ActiveEditor()->textEditor->setZoom(zoom + 1);

}

void wxMain::on_toolbuttonZoomOut_Clicked()
{
	if(ActiveEditor() == NULL) return;

	/*
	zoom --;
	if(zoom < -10) zoom = -10;
	ActiveEditor()->textEditor->setZoomForView1(zoom);
	ActiveEditor()->textEditor->setZoomForView2(zoom);
	*/

	gint zoom = ActiveEditor()->textEditor->getZoom();
	if(zoom > -10) 
		ActiveEditor()->textEditor->setZoom(zoom - 1);

}

void wxMain::on_toolbuttonResetZoom_Clicked()
{
	if(ActiveEditor() == NULL) return;

	ActiveEditor()->textEditor->setZoomForView1(0);
	ActiveEditor()->textEditor->setZoomForView2(0);

	
}

void wxMain::on_toolbuttonCompilerWindow_Clicked()
{
	if(compilerWindow->is_visible())
	{
		//Re-Attach to the main window
		compilerWindow->hide();
		return;
	}

	//else

	//Gtk::Statusbar* sBar = manage(new Gtk::Statusbar());
	//sBar->push("PIPPO",0);
	//compilerWindow->add(*sBar); 
	//sBar->show();

	
	//Detach from main window
	notebookCompiler->reparent(*compilerWindow);
	notebookCompiler->set_size_request(-1, NOTEBOOK_COMPILER_SIZE);
	compilerWindow->set_modal(FALSE);
	compilerWindow->set_title("WinXound Output/Tools");

	//Set size
	gint w = wxSETTINGS->General.CompilerWindowSize.get_x();
    gint h = wxSETTINGS->General.CompilerWindowSize.get_y();
	compilerWindow->set_size_request (600, 200); //SET MINIMUM SIZE
	compilerWindow->resize(w,h);

	compilerWindow->show_all();

	compilerWindow->set_decorated (true);
	
	//Set position
	gint x = wxMainWindow->get_screen()->get_width() - w;
	gint y = wxMainWindow->get_screen()->get_height() - h;
	if (wxSETTINGS->General.CompilerWindowPosition.get_x() < wxMainWindow->get_screen()->get_width() &&
	    wxSETTINGS->General.CompilerWindowPosition.get_y() < wxMainWindow->get_screen()->get_height() &&
        wxSETTINGS->General.CompilerWindowPosition.get_x() > -1 &&
        wxSETTINGS->General.CompilerWindowPosition.get_y() > -1)
	{
		x = wxSETTINGS->General.CompilerWindowPosition.get_x();
		y = wxSETTINGS->General.CompilerWindowPosition.get_y();
	}
	compilerWindow->move(x, y);
	
	compilerWindow->present();
}

void wxMain::on_compilerWindow_closing()
{

}

bool wxMain::on_compilerWindow_expose_event(GdkEventExpose* e)
{
	//wxGLOBAL->DebugPrint("COMPILER_WINDOW_EXPOSE");
	
	gint x, y;
	compilerWindow->get_position(x, y);
	//wxGLOBAL->DebugPrint(wxGLOBAL->IntToString(x).c_str(),wxGLOBAL->IntToString(y).c_str());
	
	wxSETTINGS->General.CompilerWindowPosition.set_x(x); //OriginX;
	wxSETTINGS->General.CompilerWindowPosition.set_y(y); //OriginY
	wxSETTINGS->General.CompilerWindowSize.set_x(compilerWindow->get_width());
	wxSETTINGS->General.CompilerWindowSize.set_y(compilerWindow->get_height());
	
	return false;
}

void wxMain::on_compilerWindow_closed()
{
	wxGLOBAL->DebugPrint("COMPILER_WINDOW_CLOSED");

	//1.This doesn't work properly:
	//notebookCompiler->reparent(*vpanedMain);
	//notebookCompiler->set_size_request(-1, NOTEBOOK_COMPILER_SIZE);
	//vpanedMain->set_position(wxMainWindow->get_height() + NOTEBOOK_COMPILER_SIZE); //80-160

	
	//2.This works properly:
	//Set notebookCompiler popup default height
	oldHeight = wxMainWindow->get_height() / TAB_POPUP_RATIO;
	
	compilerWindow->remove();
	vpanedMain->pack2(*notebookCompiler, FALSE, FALSE);
	notebookCompiler->set_size_request(-1, NOTEBOOK_COMPILER_SIZE);
	vpanedMain->set_position(wxMainWindow->get_height() + NOTEBOOK_COMPILER_SIZE); //80-160

	//Reset the focus to the main window
	wxMainWindow->present();
}

bool wxMain::on_compilerWindow_key_press_event(GdkEventKey* event)
{
	//We must replicate the notebookCompiler shortcuts
	
	if (event->keyval == GDK_Escape) //Escape key sequence
	{
		if(compiler->ProcessActive)
		{
			compiler->StopCompiler();
			return false;
		}

		//Reset the focus to the main window
		wxMainWindow->present();
	}
	else if(event->keyval == GDK_9 && event->state == GDK_CONTROL_MASK)
	{
		on_menuitemShowCompiler_Clicked();
	}
	else if(event->keyval == GDK_0 && event->state == GDK_CONTROL_MASK)
	{
		on_menuitemShowHelp_Clicked();
	}
	else if(event->keyval == GDK_8 && event->state == GDK_CONTROL_MASK)
	{
		//Reset the focus to the main window and show the code
		wxMainWindow->present();
		on_menuitemShowCode_Clicked();
	}
	else if(event->keyval == GDK_j && event->state == GDK_CONTROL_MASK)
	{
		on_menuitemCSoundOpcodesRepository_Clicked();
	}
	else if(event->keyval == GDK_i && event->state == GDK_CONTROL_MASK)
	{
		on_menuitemCodeRepositoryShowWindow_Clicked();
	}
	else if(event->keyval == GDK_F7)
	{
		on_menuitemTerminal_Clicked();
		return true;
	}
	else if(event->keyval == GDK_0 && event->state == (GDK_CONTROL_MASK | GDK_MOD1_MASK))
	{
		on_toolbuttonCompilerWindow_Clicked();
	}
	
	return false;
}





////////////////////////////////////
//RETURN THE CURRENT ACTIVE DOCUMENT
wxEditor* wxMain::ActiveEditor()
{
	gint index = notebookCode->get_current_page();
	return (wxEditor*)notebookCode->get_nth_page(index);
}















//////////////////
//ADD NEW DOCUMENT
//void wxMain::AddNewEditor(const Glib::ustring& filename)
void wxMain::AddNewEditor(Glib::ustring filename)
{
	AddNewEditor(filename, "", false);
}
void wxMain::AddNewEditor(Glib::ustring filename, Glib::ustring linkFile)
{
	AddNewEditor(filename, linkFile, false);
}
void wxMain::AddNewEditor(Glib::ustring filename, Glib::ustring linkFile, bool importOperation)
{
	bool IsNewUntitledFile = false;

	if (filename == UNTITLED_CSD ||
	    filename == UNTITLED_ORC ||
	    filename == UNTITLED_SCO ||
	    filename == UNTITLED_PY ||
	    filename == UNTITLED_LUA ||
	    filename == UNTITLED_CABBAGE)
	{
		IsNewUntitledFile = true;
	}


	//1.
	//Check if the loaded file is already open in another tab
	try
	{
		if (IsNewUntitledFile == false)
		{
			//wxEditor tempEditor = null;
			wxEditor* tempEditor = NULL;
			
			//if (wxTabCode.TabPages.Count > 0)
			if(notebookCode->get_n_pages() > 0)
			{
				//foreach (TabPage tp in wxTabCode.TabPages)
				for(int i = 0; i < notebookCode->get_n_pages(); i++)
				{
					//tempEditor = (wxEditor)tp.Controls[0];
					tempEditor = (wxEditor*)notebookCode->get_nth_page(i);
					
					if (tempEditor->FileName == filename)
					{
						//MessageBox.Show("File already open! Please close it and retry.",
						//                "WinXound Info",
						//                MessageBoxButtons.OK, MessageBoxIcon.Information);
						notebookCode->set_current_page(i);
						return;
					}
				}
			}
		}
	}
	catch(...)
	{
		//std::cerr << "wx-main Error: " << ex.what() << std::endl;
		wxGLOBAL->DebugPrint("wxMain::AddNewEditor Error - "
		                     "Check if the loaded file is already open in another tab");
	}

	
	//2.
	//Check Find&Replace window status
	if (FindAndReplace != NULL)
	{
		FindAndReplace->closeWindow();
	}


	//3.
	//Create new Tab and wxEditor control
	//TabPage tab = new TabPage(filename);
	//wxEditor textEditor = new wxEditor();
	wxEditor* editor = new wxEditor();
	editor->SetOwner(this);
	gint index = notebookCode->append_page(*editor);
	notebookCode->set_current_page(index);
	notebookCode->set_tab_reorderable(*editor, true);
	notebookCode->set_tab_label_text(*editor, "");


	if (editor == NULL)
	{
		wxGLOBAL->ShowMessageBox(this,
		                         "FormMain->AddNewEditor: Unable to create editor!",
		                         "WinXound - Critical Error",
		                         Gtk::BUTTONS_OK);
		return;
	}


	//4. NOT NEEDED FOR LINUX
	//Set textEditor dockstyle
	//textEditor.Dock = System.Windows.Forms.DockStyle.Fill;

	
	//5.
	//Check File IO Permissions
	try
	{
		if (IsNewUntitledFile == false &&
		    Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
		{
			if(!wxGLOBAL->FileIsWritable(filename.c_str()))
			{
				editor->FileIsReadOnly = true;

				if (wxSETTINGS->General.ShowReadOnlyFileMessage)
				{
					ShowUaeMessageOnLoad(NULL);
				}
			}
		}
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain AddNewEditor FilePermission Error");
	}
	



	//6.
	//ADD EDITOR SIGNALS (SCINTILLA EVENTS)
	editor->signal_save_point_left().connect(sigc::mem_fun(*this, &wxMain::onSavePointLeft));
	editor->signal_save_point_reached().connect(sigc::mem_fun(*this, &wxMain::onSavePointReached));
	editor->signal_update_ui().connect(sigc::mem_fun(*this, &wxMain::onDocumentUpdateUi));
	editor->signal_zoom_notify().connect(sigc::mem_fun(*this, &wxMain::onZoomNotification));
	editor->signal_modified().connect(sigc::mem_fun(*this, &wxMain::onSciModified));
	editor->signal_uri_dropped().connect(sigc::mem_fun(*this, &wxMain::onUriDropped));
	editor->signal_mouse_down().connect(sigc::mem_fun(*this, &wxMain::onMouseDown));
	editor->signal_autocompletion_synopsis().connect(sigc::mem_fun(*this, &wxMain::onSignalAutocompletionSynopsis));
	editor->signal_mod_container().connect(sigc::mem_fun(*this, &wxMain::onSciModContainer));
	editor->signal_editor_focus().connect(sigc::mem_fun(*this, &wxMain::onEditorFocus));
	editor->signal_switch_orcsco().connect(sigc::mem_fun(*this, &wxMain::onEditorOrcScoSwitchRequest));
	editor->signal_orcsco_show_list().connect(sigc::mem_fun(*this, &wxMain::onEditorOrcScoShowList));

	
	//7.
	//Set TextEditor Preferences
	editor->textEditor->setShowSpaces(false); //inserted in MenuEditView
	editor->textEditor->setShowEOLMarker(false); //inserted in MenuEditView
	editor->textEditor->setAllowCaretBeyondEOL(false); //maybe true! or insert in Settings
	editor->textEditor->setShowMatchingBracket(wxSETTINGS->EditorProperties.ShowMatchingBracket);
	editor->textEditor->setShowVerticalRuler(wxSETTINGS->EditorProperties.ShowVerticalRuler);
	editor->textEditor->setMarkCaretLine(wxSETTINGS->EditorProperties.MarkCaretLine);
	editor->textEditor->setShowLineNumbers(wxSETTINGS->EditorProperties.ShowLineNumbers);
	editor->textEditor->setTabIndent(wxSETTINGS->EditorProperties.DefaultTabSize);
	editor->setShowExplorer(wxSETTINGS->EditorProperties.ShowExplorer);
	editor->setShowIntelliTip(wxSETTINGS->EditorProperties.ShowIntelliTip);
	editor->textEditor->setTextEditorFont(wxSETTINGS->EditorProperties.DefaultFontName.c_str(),
	                                      wxSETTINGS->EditorProperties.DefaultFontSize);
	editor->textEditor->ShowFoldLine(wxSETTINGS->EditorProperties.ShowFoldLine);
	editor->FileName = filename;
	editor->ShowOrcScoPanel(false);


	//8.
	//Load file(s) or create a new one
	//NEW FILES
	if (filename == UNTITLED_CSD)
	{
		//New CSound Document 
		editor->textEditor->setText(wxSETTINGS->Templates.CSound.c_str());
	}
	else if (filename == UNTITLED_ORC || 
	         filename == UNTITLED_SCO)
	{
		//New Orc/Sco empty document
	}
	else if (filename == UNTITLED_PY)
	{
		//New Python empty document
		editor->textEditor->setText(wxSETTINGS->Templates.Python.c_str());
	}
	else if (filename == UNTITLED_LUA)
	{
		//New Lua empty document
		editor->textEditor->setText(wxSETTINGS->Templates.Lua.c_str());
	}
	else if (filename == UNTITLED_CABBAGE)
	{
		//New Cabbage document
		editor->textEditor->setText(wxSETTINGS->Templates.Cabbage.c_str());

		//Replace .cabbage to .csd
		filename = UNTITLED_CSD;
		editor->FileName = filename;
	}
	//IMPORT FILES
	else if ((Glib::str_has_suffix(editor->FileName.lowercase(), ".orc") ||
	         Glib::str_has_suffix(editor->FileName.lowercase(), ".sco")) &&
	         IsNewUntitledFile == false)
	{
		/*
		//OLD VERSION: IMPORT DIRECTLY
		//Import Orc and Sco
		wxImportExport* mImport = new wxImportExport();

		editor->textEditor->setText(mImport->ImportORCSCO(filename.c_str()).c_str());
		Glib::ustring newFilename = editor->FileName.substr(0, editor->FileName.size() - 4);
		newFilename.append(".csd");
		editor->FileName = IMPORTED;
		editor->FileName.append(newFilename);
		IsNewUntitledFile = true;

		delete mImport;
		*/

		//NEW VERSION FOR OSC/SCO
		bool OpenSeparately = false;
		if (wxSETTINGS->General.OrcScoImport == 2)        //Open separately
		{
			OpenSeparately = true;
		}
		else if (wxSETTINGS->General.OrcScoImport == 1 || //Import to CSD
		         wxSETTINGS->General.OrcScoImport == 0)   //Ask always
		{
			bool OkToImport = true;

			if (wxSETTINGS->General.OrcScoImport == 0 &&
			    linkFile.size() <= 0 &&
			    importOperation == false) //Ask always
			{
				gint ret = wxGLOBAL->ShowMessageBox(
					"Would you like to convert your Orc/Sco file to a new Csd document?\n"
				    "To change the default Orc/Sco import action\n"
				    "please look at:\n"
				    "'File->Settings->General->Orc/Sco Import field'",
		            "WinXound - Orc/Sco Import",
		            Gtk::BUTTONS_YES_NO);

				if(ret != Gtk::RESPONSE_YES) OkToImport = false;

			}

			if (OkToImport && linkFile.size() <= 0)
				OpenSeparately = false;
			else
				OpenSeparately = true;
		}

		if (OpenSeparately == true &&
		    importOperation == false) //Open separately
		{
			//Show the OrcSco Panel in the editor and load the file
			editor->ShowOrcScoPanel(true);
			editor->textEditor->LoadFile(filename.c_str());
			if (linkFile.size() <= 0)
				LinkOrcScoFiles(editor);
			else
				editor->SetCurrentOrcScoFile(linkFile);
		}
		else
		{
			//Import Orc and Sco
			wxImportExport* mImport = new wxImportExport();

			editor->textEditor->setText(mImport->ImportORCSCO(filename.c_str()).c_str());
			Glib::ustring newFilename = editor->FileName.substr(0, editor->FileName.size() - 4);
			newFilename.append(".csd");
			editor->FileName = IMPORTED;
			editor->FileName.append(newFilename);
			IsNewUntitledFile = true;

			delete mImport;		
		}

	}
	//LOAD OTHER FILES
	else
	{
		//All other Files 
		editor->textEditor->LoadFile(filename.c_str());
	}



	//9. If this is a new untitled file we must set and check the default line endings
	if (IsNewUntitledFile)
	{
		//NEW FILES
		//Convert (templates) and Set line endings to CRLF (Windows default)
		editor->textEditor->ConvertEOL(SC_EOL_LF);
		editor->textEditor->setEolMode(SC_EOL_LF);
	}
	//Check Line Endings for loaded file (to get Eols and to check Eols coherence)
	else
	{
		//LOADED FILES
		CheckLineEndings(editor);
	}

	

	//10.
	if (IsNewUntitledFile == false)
	{
		notebookCode->set_tab_label_text(*editor, Glib::path_get_basename (filename));
		AddRecentFilesToMenu(filename.c_str());

		//Try to load bookmarks
		LoadBookmarks(editor);
	}
	else
	{
		notebookCode->set_tab_label_text(*editor, 
		                                 Glib::path_get_basename(editor->FileName.c_str()));
	}


	//10.5
	//Set Cabbage/Instr Indentation Margin
	//TODO ???: editor->textEditor->ShowIndentMargin(true);

	
	//11.
	//Set Scintilla Save Point and Empty the Undo Buffer
	editor->textEditor->setSavePoint();
	editor->textEditor->emptyUndoBuffer();
	


	//12.
	//Check for ReadOnly files (in order to add a notification to Tab Title)
	if (editor->FileIsReadOnly)
	{
		Glib::ustring tempTab = notebookCode->get_tab_label_text(*editor);
		tempTab.append(" (Read Only)");
		notebookCode->set_tab_label_text(*editor, tempTab.c_str());
	}



	//13.
	//Set Default Highlight
	SetHighlightLanguage(editor, false);
	//editor.RefreshTextEditor(); //???


	//14.
	//Refresh the StatusBar
	onZoomNotification(editor);


	//15. Final checks
	//Set Title, Undo and Redo conditions and others Menu conditions
	CheckMenuConditions();

	//Set focus on scintilla
	editor->textEditor->setFocus();

	//Start Explorer Structure Timer
	if(wxGLOBAL->isSyntaxType(editor))
		editor->StartExplorerTimer();

}


/////////////////////////////////////////////////////////////////
//ORC-SCO FILES STUFFS:
/////////////////////////////////////////////////////////////////
void wxMain::LinkOrcScoFiles(wxEditor* editor)
{
	Glib::ustring file = wxGLOBAL->getFileNameWithoutExtension(editor->FileName);
	std::vector<std::string> temp = LookForOrcScoFiles(editor);

	if(temp.empty()) return;

	//std::cout << "LinkOrcScoFiles called - File to look: " << file << std::endl;
	
	for (uint i = 0; i < temp.size(); i++)
	{
		//std::cout << "Temp value: " << temp[i] << std::endl;
		if(temp[i].find(file) != std::string::npos)
		{
			editor->SetCurrentOrcScoFile(wxGLOBAL->Trim(temp[i]));
			break;
		}
	}

	temp.clear();
}

void wxMain::onEditorOrcScoSwitchRequest()
{
	if(ActiveEditor() == NULL) return;

	Glib::ustring filename = ActiveEditor()->GetCurrentOrcScoFile();
	if(Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
		AddNewEditor(filename, ActiveEditor()->FileName);
}

void wxMain::onEditorOrcScoShowList()
{
	if(ActiveEditor() == NULL) return;

	//DIALOG
	Gtk::Dialog* dialog = new Gtk::Dialog("WinXound - Orc/Sco Links", TRUE, FALSE);
	dialog->set_size_request(600,400);
	
	//LISTBOX
	Gtk::ListViewText*		listBoxFiles;
	Gtk::ScrolledWindow*	mScrolledWindowlistBoxFiles;
	
	listBoxFiles = Gtk::manage(new Gtk::ListViewText(1, false, Gtk::SELECTION_SINGLE));	
	listBoxFiles->set_can_focus(TRUE);
	mScrolledWindowlistBoxFiles = Gtk::manage(new Gtk::ScrolledWindow());
	mScrolledWindowlistBoxFiles->add(*listBoxFiles);
	//Only show the scrollbars when they are necessary:
	mScrolledWindowlistBoxFiles->set_policy(Gtk::POLICY_AUTOMATIC, Gtk::POLICY_AUTOMATIC);
	listBoxFiles->set_column_title(0,"Files:");
	listBoxFiles->clear_items();


	//Fill ListBox:
	std::vector<std::string> temp = LookForOrcScoFiles(ActiveEditor());
	if(temp.empty()) return;
	int selIndex = 0;
	for (uint i = 0; i < temp.size(); i++)
	{
		listBoxFiles->append_text(temp[i]);
		if(temp[i] == ActiveEditor()->GetCurrentOrcScoFile())
			selIndex = i;
	}
	
	if(selIndex > 0)
	{
		//SELECT THE CURRENT FILE INDEX
		Glib::RefPtr<Gtk::TreeSelection> sel = listBoxFiles->get_selection();
		Gtk::TreePath* path = new Gtk::TreePath(wxGLOBAL->IntToString(selIndex));
		sel->select(*path);
		delete path;
	}
	
	temp.clear();
	
	//pack_start (Widget& child, bool expand, bool fill, guint padding=0)
	dialog->get_vbox()->pack_start(*mScrolledWindowlistBoxFiles, TRUE, TRUE, 0);
	dialog->add_button("OK", 1);
	dialog->add_button("CANCEL", 2);
	dialog->show_all_children();

	dialog->set_resizable(FALSE);
	dialog->set_icon_from_file(Glib::ustring::compose("%1/winxound_48.png",
	                                                  wxGLOBAL->getIconsPath()));

	dialog->show_all();


	
	//SET DIALOG POSITION (CENTER PARENT)
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
	dialog->move(x - dialog->get_width() / 2,
	             y - dialog->get_height() / 2);


	int result = dialog->run();

	if(result == 1)
	{
		if(!listBoxFiles->get_selected().empty())
		{
			gint index = listBoxFiles->get_selected()[0];  
			//std::cout << "listBoxFiles selected text: " << listBoxFiles->get_text(index,0) << std::endl;
			ActiveEditor()->SetCurrentOrcScoFile(listBoxFiles->get_text(index,0));
		}
	}

	dialog->hide();

	
}

std::vector<std::string> wxMain::LookForOrcScoFiles(wxEditor* editor)
{
	//List<string> OrcScoFilesList = new List<string>();
	std::vector<std::string> OrcScoFilesList;
	
	if (editor == NULL) return OrcScoFilesList;

	//if (!editor.FileName.ToLower().EndsWith(".orc") &&
	//    !editor.FileName.ToLower().EndsWith(".sco"))
	//	return new string[] { "" };
	if(!Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(),".orc") &&
	   !Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(),".sco"))
		return OrcScoFilesList;

	
	Glib::ustring mPredicate =
		Glib::str_has_suffix(editor->FileName.lowercase(),".orc") ? "*.sco" : "*.orc";

	//string mPath = Path.GetDirectoryName(editor.FileName);
	Glib::ustring mPath = Glib::path_get_dirname(editor->FileName);



	//if (!string.IsNullOrEmpty(editor.GetCurrentOrcScoFile()))
	//	OrcScoFilesList.Add(editor.GetCurrentOrcScoFile());
	if(editor->GetCurrentOrcScoFile().size() > 0)
		OrcScoFilesList.push_back(editor->GetCurrentOrcScoFile());


	//Look in the current file directory
	Glib::Dir cur_dir(mPath);
	Glib::ustring extension = mPredicate.substr(1);
	for(Glib::Dir::iterator p = cur_dir.begin(); p != cur_dir.end(); ++p)
	{
		Glib::ustring temp = *p;
		
		if(Glib::str_has_suffix(temp.lowercase(), extension))
		{
			Glib::ustring pathToInsert = mPath;
			pathToInsert.append("/");
			pathToInsert.append(temp);
			if(pathToInsert != editor->GetCurrentOrcScoFile())
			{
				OrcScoFilesList.push_back(pathToInsert);
			}
		}
	}



	//Look also in the current opened files
	if(notebookCode->get_n_pages() > 0)
	{
		wxEditor* tempEditor = NULL;

		for(int i = 0; i < notebookCode->get_n_pages(); i++)
		{
			tempEditor = (wxEditor*)notebookCode->get_nth_page(i);
			if (Glib::str_has_suffix(tempEditor->FileName.lowercase(), extension))
			{
				//if(!OrcScoFilesList.Contains(temp.FileName))
				if(std::find(OrcScoFilesList.begin(), 
				             OrcScoFilesList.end(), 
				             tempEditor->FileName) == OrcScoFilesList.end()) //ITEM NOT FOUND
					OrcScoFilesList.push_back(tempEditor->FileName);
			}
		}
	}
	

	//Sort the list
	std::sort (OrcScoFilesList.begin() + 1, OrcScoFilesList.end());

	return OrcScoFilesList;
	
}




/////////////////////////////////////////////////////////////////
//CheckLineEndings: check for line endings and for eols coherence
void wxMain::CheckLineEndings(wxEditor* editor)
{

	if (editor == NULL) return;

	//Check Line Endings
	//SET EOL MODE: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2)

	bool FileIsNotConsistent = false;

	//string s = editor.textEditor.GetText().Replace("\r\n", "");
	Glib::RefPtr<Glib::Regex> my_regex = Glib::Regex::create("\r\n");
	Glib::ustring s = my_regex->replace(editor->textEditor->getText(), 
	                                    0, 
	                                    "", 
	                                    Glib::REGEX_MATCH_NOTBOL);

	
	gint crlfOccurrences = (editor->textEditor->getTextLength() - s.size()) / 2;
	gint crOccurrences = 0;
	gint lfOccurrences = 0;

	//Check if CRLF correspond to TextEditor lines - 1
	//If not check also for LF
	if (crlfOccurrences != editor->textEditor->getLinesCount() - 1)
	{
		//Check if LF correspond to TextEditor lines - 1
		//If not check also for CR
		my_regex = Glib::Regex::create("\n");
		lfOccurrences = s.size() - //s.Replace("\n", "").Length;
						my_regex->replace(s.c_str(), 
									      0, 
									      "", 
									      Glib::REGEX_MATCH_NOTBOL).size();
		
		if (lfOccurrences != editor->textEditor->getLinesCount() - 1)
		{
			//Check for CR
			my_regex = Glib::Regex::create("\r");
			crOccurrences = s.size() - //s.Replace("\r", "").Length;
							my_regex->replace(s.c_str(), 
									      0, 
									      "", 
									      Glib::REGEX_MATCH_NOTBOL).size();
		}
	}


	//string report =
	//    "CRLF:\t" + crlfOccurrences + newline +
	//    "CR:\t" + crOccurrences + newline +
	//    "LF:\t" + lfOccurrences;
	Glib::ustring report =
		Glib::ustring::compose("CRLF:\t%1\n"
		                       "CR:\t%2\n"
		                       "LF:\t%3\n",
		                       crlfOccurrences,
		                       crOccurrences,
		                       lfOccurrences);
	//wxGLOBAL->DebugPrint("EOLS", report.c_str());
		                       


	/*
	string report =
		"Filename: " + Path.GetFileName(editor.FileName) + newline +
		(crlfOccurrences > 0 ? "CRLF:\t" + crlfOccurrences + newline : "") +
		(crOccurrences > 0 ? "CR:\t" + crOccurrences + newline : "") +
		(lfOccurrences > 0 ? "LF:\t" + lfOccurrences : "");
	System.Diagnostics.Debug.WriteLine(report);
	*/


	//CRLF
	//if (editor.textEditor.GetText().Contains("\r\n"))
	if (crlfOccurrences > crOccurrences &&
	    crlfOccurrences > lfOccurrences)
	{
		//Set EOL mode to CRLF
		editor->textEditor->setEolMode(SC_EOL_CRLF);

		//Check for CRLF coherence
		//string crlf = editor.textEditor.GetText().Replace("\r\n", "");
		//if (crlf.Contains("\r") || crlf.Contains("\n"))
		if (crOccurrences > 0 || lfOccurrences > 0)
		{
			//Incoherent EOLS!!! Convert all to CRLF
			//DialogResult ret = ShowEolAlertMessage("CRLF", report);

			//if (ret == DialogResult.Yes)
			editor->textEditor->ConvertEOL(SC_EOL_CRLF);
		}
	}

	//CR
	//else if (editor.textEditor.GetText().Contains("\r"))
	else if (crOccurrences > crlfOccurrences &&
	         crOccurrences > lfOccurrences)
	{
		//Set EOL mode to CR
		editor->textEditor->setEolMode(SC_EOL_CR);

		//Check for CR coherence
		//string cr = editor.textEditor.GetText().Replace("\r", "");
		//if (cr.Contains("\r\n") || cr.Contains("\n"))
		if (crlfOccurrences > 0 || lfOccurrences > 0)
		{
			//Incoherent EOLS!!! Convert all to CR
			//DialogResult ret = ShowEolAlertMessage("CR", report);

			//if (ret == DialogResult.Yes)
			editor->textEditor->ConvertEOL(SC_EOL_CR);
		}
	}

	//LF
	//else if (editor.textEditor.GetText().Contains("\n"))
	else if (lfOccurrences > crlfOccurrences &&
	         lfOccurrences > crOccurrences)
	{
		//Set EOL mode to LF
		editor->textEditor->setEolMode(SC_EOL_LF);

		//Check for LF coherence
		//string lf = editor.textEditor.GetText().Replace("\n", "");
		//if (lf.Contains("\r\n") || lf.Contains("\r"))
		if (crlfOccurrences > 0 || crOccurrences > 0)
		{
			//Incoherent EOLS!!! Convert all to LF
			//DialogResult ret = ShowEolAlertMessage("LF", report);

			//if (ret == DialogResult.Yes)
			editor->textEditor->ConvertEOL(SC_EOL_LF);
		}
	}

	else
	{
		//Convert and Set default LF mode
		editor->textEditor->ConvertEOL(SC_EOL_LF);
		editor->textEditor->setEolMode(SC_EOL_LF);
	}

	if (FileIsNotConsistent)
	{
		//Save the file after conversion ???
	}


}

void wxMain::ShowEolAlertMessage(const gchar* eol, const gchar* report)
{
	Glib::ustring text = "The line endings in the file are not consistent!\n\n";
	text.append(report);
	text.append("\n\n");
	text.append("WinXound will normalize all line endings to ");
	text.append(eol);
	text.append(".");
	
	wxGLOBAL->ShowMessageBox(this,
	                         text.c_str(),
	                         "WinXound Line Endings Information",
	                         Gtk::BUTTONS_OK);

}






bool wxMain::RemoveEditor(wxEditor* editor)
{
	if (editor != NULL)
	{
		//std::cout << "RemoveDocument clicked." << std::endl;
		
		bool savebookmarks = true;

		try
		{
			//Check text changes
			if(editor->textEditor->isTextChanged() == true &&
			   editor->FileIsReadOnly == false) //skip ReadOnly files
			{
				/*
				DialogResult ret = MessageBox.Show(
				                                   "Document '" + editor.FileName +
				                                   "' has changed!" + newline +
				                                   "Do you want to save it?",
				                                   "WinXound",
				                                   MessageBoxButtons.YesNoCancel,
				                                   MessageBoxIcon.Information);
												   */
				Glib::ustring message = "Document '";
				message	+= editor->FileName;
				message	+= "' has changed!\nDo you want to save it?";
				
				Gtk::MessageDialog dialog(*this, 
				                          message,
				                          false /* use_markup */, 
				                          Gtk::MESSAGE_QUESTION,
				                          Gtk::BUTTONS_NONE);
				dialog.add_button("YES", Gtk::RESPONSE_YES);
				dialog.add_button("NO", Gtk::RESPONSE_NO);
				dialog.add_button("Cancel", Gtk::RESPONSE_CANCEL);
				
				//dialog.set_secondary_text("And this is the secondary text that explains things.");

				int result = dialog.run();

				//Handle the response:
				switch(result)
				{
					case(Gtk::RESPONSE_YES):
					{
						//MenuFileSave.PerformClick();
						on_menuitemSave_Clicked();
						//If the user has cancelled the SaveFileDialog we must return false
						//So we check IsTextChanged() to see if it's not saved
						if (editor->textEditor->isTextChanged() == true &&
						    editor->FileIsReadOnly == false)
							return false;							
						//std::cout << "YES clicked." << std::endl;
						break;
					}
					case(Gtk::RESPONSE_NO):
					{
						savebookmarks = false;
						//std::cout << "NO clicked." << std::endl;
						break;
					}
					case(Gtk::RESPONSE_CANCEL):
					{
						editor->textEditor->setFocus();
						//std::cout << "CANCEL clicked." << std::endl;
						return false;
					}
				}

			}
		}
		catch(...)
		{
			wxGLOBAL->DebugPrint("wxMain - RemoveEditor Error - Check Save method");
		}


		try
		{

			//Save Bookmarks
			if (savebookmarks &&
			    wxSETTINGS->EditorProperties.SaveBookmarks &&
			    !FileIsUntitled(editor))
			{
				SaveBookmarks(editor);
			}


			//Close editor tab and release related resources
			//if (wxTabCode.TabPages.Count > 0)
			if(notebookCode->get_n_pages() > 0)
			{
				//Control tabToRemove = editor.Parent;
				//Int32 tabIndex = wxTabCode.Controls.IndexOf(tabToRemove) - 1;
				//if (tabIndex < 0) tabIndex = 0;
				gint tabIndex = notebookCode->page_num(*editor);

				//Remove tab and release all resources
				//wxTabCode.Controls.Remove(tabToRemove);
				//tabToRemove.Dispose();
				//delete editor;
				notebookCode->remove_page(tabIndex);
				delete editor;
				editor = NULL;



				//Select previous tab
				//wxTabCode.SelectedIndex = tabIndex;
				tabIndex--;
				if (tabIndex < 0) tabIndex = 0;
				if(notebookCode->get_n_pages() > 0)
				{
					notebookCode->set_current_page(tabIndex);
					wxEditor* tempEditor = (wxEditor*)notebookCode->get_nth_page(tabIndex);
					wxMainWindow->set_title(tempEditor->FileName);
				}
			}

		}
		catch(...)
		{
			//System.Diagnostics.Debug.WriteLine(
			//"FormMain.RemoveEditor - Close Tab method: " + ex.Message);
			wxGLOBAL->DebugPrint("wxMain - RemoveEditor Error - Close Tab method");
		}
		

	}


	
	//if (wxTabCode.TabPages.Count == 0)
	if(notebookCode->get_n_pages() == 0)
	{
		//this.Text = wxGlobal.TITLE;
		wxMainWindow->set_title("WinXound 3.3.0 (Beta 3)");
		CheckMenuConditions();
		ClearStatubarInfo();
		FindAndReplace->closeWindow();
		//wxMainWindow->activate_focus(); Segmentation_FAULT
	}

	return true;
}

bool wxMain::RemoveAllEditors()
{
	bool IsClosed = false;
	//foreach (TabPage tp in wxTabCode.TabPages)
	//for(int index = 0; index < notebookCode->get_n_pages(); index++)
	for(int index = notebookCode->get_n_pages() - 1; index > -1; index--)
	{
		//wxTabCode.SelectedTab = tp; //To prevent bug on close!!!
		notebookCode->set_current_page(index);
		//IsClosed = RemoveEditor((wxEditor)tp.Controls[0]);
		IsClosed = RemoveEditor((wxEditor*)notebookCode->get_nth_page(index));
		if (IsClosed == false) return false;
	}

	return true;
}


void wxMain::ClearStatubarInfo()
{
	statusbarInfo->pop(1);
	statusbarInfo->push("",1);
}



////////////////////////////////////////////////////////////////////////////////
//SCINTILLA MESSAGES
void wxMain::onSavePointLeft()
{
	//ToolStripSave.Enabled = true;
	//MenuFileSave.Enabled = true;
	toolbuttonSave->set_sensitive(TRUE);
	menuitemSave->set_sensitive(TRUE);
}

void wxMain::onSavePointReached()
{
	//ToolStripSave.Enabled = false;
	//MenuFileSave.Enabled = false;
	toolbuttonSave->set_sensitive(FALSE);
	menuitemSave->set_sensitive(FALSE);
}
void wxMain::onZoomNotification(wxEditor* editor)
{
	Glib::ustring text;

	Glib::ustring fileRights = 
		(editor->FileIsReadOnly ? "Read Only" : "Read/Write");
	
	Glib::ustring lineEndings = "";
	if(editor->textEditor->getEOLMode() == SC_EOL_CRLF)
		lineEndings = "CRLF";
	else if(editor->textEditor->getEOLMode() == SC_EOL_CR)
		lineEndings = "CR";
	else
		lineEndings = "LF";

	//[[textEditor getPrimaryView] getGeneralProperty: SCI_STYLEGETSIZE parameter: STYLE_DEFAULT];
	gint fontSize = wxSETTINGS->EditorProperties.DefaultFontSize;
	gint zoom1 = (fontSize + editor->textEditor->getZoomForView1()) * 100 / fontSize;
	gint zoom2 = (fontSize + editor->textEditor->getZoomForView2()) * 100 / fontSize;

	//gint zoom1 = editor->textEditor->getZoomForView1();
	//gint zoom2 = editor->textEditor->getZoomForView2();
	
	text = Glib::ustring::compose(" File Permissions: %1      Line Endings: %2      "
	                              "Text Zoom: %3%% / %4%%",
	                              fileRights, lineEndings, zoom1, zoom2);
	statusbarInfo->pop(1);
	statusbarInfo->push(text,1);
}
void wxMain::onDocumentUpdateUi(wxEditor* editor)
{
	//std::cerr << "wxMain::onDocumentUpdateUi" << std::endl;
	
	if (editor->textEditor->getTextLength() <= 0) return;
	if (editor->textEditor->getCaretPosition() <= 0) return;
	if (!wxGLOBAL->isSyntaxType(editor)) return;


	if (wxSETTINGS->EditorProperties.ShowMatchingBracket)
		CheckForBracket(editor);
	

	try
	{

		//Retrieve the current word and search inside Opcode Database 
		//gCurWord = tEditor.textEditor.GetWordAt(tEditor.textEditor.GetCaretPosition());
		Glib::ustring curWord = editor->textEditor->getWordAt(editor->textEditor->getCaretPosition());

		//if (Opcodes.Contains(gCurWord))
		//if (gCurWord.length() > 1) // && Opcodes.Contains(gCurWord))
		if(Opcodes != NULL)
		{
			if (curWord.length() > 1 &&    //we look only for 2 or major word length
			    g_hash_table_lookup(Opcodes, curWord.c_str()) != NULL)
			{
				//string[] split = Opcodes[gCurWord].ToString().Split("|".ToCharArray());
				//tEditor.SetIntelliTip("[" + gCurWord + "] - " + split[1], split[2]);

				Glib::ustring value = (gchar*)g_hash_table_lookup(Opcodes, curWord.c_str());
				gchar** valueArray = g_strsplit(value.c_str(), "|", 0);

				curWord.insert(0, "[");
				curWord.append("] - ");
				curWord.append(valueArray[1]);

				editor->SetIntelliTip(curWord.c_str(), valueArray[2]);

				g_strfreev(valueArray);
			}
		}

	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain::onDocumentUpdateUi Error");
	}

}

void wxMain::CheckForBracket(wxEditor* tEditor)
{
	try
	{
		gint caretpos = tEditor->textEditor->getCaretPosition();

		//if (tEditor.textEditor.GetCharAt(caretpos - 1) == ')')
		if(tEditor->textEditor->getCharAt(caretpos -1) == ")")
		{
			gint pos1 = tEditor->textEditor->BraceMatch(caretpos - 1, 0);
			if (pos1 < 0)
			{
				tEditor->textEditor->BraceHiglight(-1, -1);
				return;
			}

			tEditor->textEditor->BraceHiglight(pos1, caretpos - 1);
		}
		else if (tEditor->textEditor->getCharAt(caretpos) == "(")
		{
			gint pos1 = tEditor->textEditor->BraceMatch(caretpos, 0);
			if (pos1 < 0) return;

			tEditor->textEditor->BraceHiglight(pos1, caretpos);
		}
		else
		{
			tEditor->textEditor->BraceHiglight(-1, -1);
		}
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain::CheckForBracket Error");
	}
	
}

void wxMain::onSciModified(wxEditor* editor)
{
	CheckUndoRedo();
}

void wxMain::onSciModContainer(wxEditor* editor)
{
	wxGLOBAL->DebugPrint("wxMain::onSciModContainer");
	//CheckUndoRedo();
	Glib::signal_timeout().connect(sigc::mem_fun(*this, &wxMain::TimerMod_Tick), 100);
}

bool wxMain::TimerMod_Tick()
{
	wxGLOBAL->DebugPrint("wxMain::TimerMod_Tick");
	CheckUndoRedo();
	//One-time timer
	return false;
}

void wxMain::onSignalAutocompletionSynopsis(wxEditor* editor,
                                            GdkEventKey* event,
                                            const gchar* opcode)
{
	//wxGLOBAL->DebugPrint("wxMain::signal_autocompletion_synopsis", opcode);

	Glib::ustring curWord = opcode;
	Glib::ustring synopsis = "";

	if(Opcodes != NULL)
	{

		if (g_hash_table_lookup(Opcodes, curWord.c_str()) != NULL)
		{
			Glib::ustring value = (gchar*)g_hash_table_lookup(Opcodes, curWord.c_str());
			gchar** valueArray = g_strsplit(value.c_str(), "|", 0);

			curWord.insert(0, "[");
			curWord.append("] - ");
			curWord.append(valueArray[1]);

			editor->SetIntelliTip(curWord.c_str(), valueArray[2]);

			//Synopsis: 2
			synopsis = valueArray[2];
			g_strfreev(valueArray);

			//editor->textEditor->AddText( valueArray[2]);
			gint pos = editor->textEditor->getCaretPosition();
			if (pos < 0) pos = 0;

			gint start = pos;
			gint end = pos;

			gint wordStart = editor->textEditor->getWordStart(pos);
			gint wordEnd = editor->textEditor->getWordEnd(pos);

			if (wordStart < start)
			{
				start -= (start - wordStart);
			}
			if (pos < wordEnd)
			{
				end += (wordEnd - pos);
			}

			editor->textEditor->BeginUndoAction();
			{
				if (start != pos)
				{
					editor->textEditor->ReplaceTarget(start, end - start, "");
				}
				editor->textEditor->AddText(synopsis.c_str());

			}
			editor->textEditor->EndUndoAction();			

			
		}
	}

}

void wxMain::onUriDropped(wxEditor* editor, Glib::ustring filename, gint view)
{
	//wxGLOBAL->DebugPrint("onUriDropped", filename.c_str());

	int x, y; 
	Gdk::ModifierType modifiers;

	if(view == 1)
		editor->textEditor->get_textView1_Widget()->get_window()->get_pointer(x, y, modifiers);
	else if(view == 2)
		editor->textEditor->get_textView2_Widget()->get_window()->get_pointer(x, y, modifiers);
	else return;


	gchar** files = g_strsplit(filename.c_str(), "\n", 0);
	if(files == NULL) return;
	
	int length = wxGLOBAL->ArrayLength(files);
	Glib::ustring path = "";

	if(modifiers & GDK_CONTROL_MASK)
	{
		if(strlen(files[0]) > 0)
		{
			path = wxGLOBAL->Trim(Glib::filename_from_uri(files[0]));
			Glib::ustring name = 
				Glib::ustring::compose("\"%1\"", path);

			//editor->textEditor->AddText(name.c_str());
			gint position = editor->textEditor->getPositionFromPoint(x, y, view);
			editor->textEditor->InsertText(position, name.c_str());
			editor->SetFocus();
		}
		
	}
	else
	{
		for(int i=0; i < length; i++)
		{
			//path = wxGLOBAL->Trim(Glib::filename_from_uri(files[i]));
			//this corrects the filename_from_uri bug on 32bit platforms (drag and drop):
			//we must check the size of files (if empty WinXound crashes)
			if(strlen(files[i]) > 0)
				path = wxGLOBAL->Trim(Glib::filename_from_uri(files[i]));

			//wxGLOBAL->DebugPrint("onUriDropped", path.c_str());
				
			if(Glib::file_test(path, Glib::FILE_TEST_EXISTS))
			{
				AddNewEditor(path.c_str());
			}
		}	

	}

	if(files != NULL)
		g_strfreev(files);
	
}

void wxMain::onMouseDown(wxEditor* editor, GdkEventButton* event)
{
	//Left = 1, Right = 3
	//std::cout << event->state << std::endl; 

	

	//SELECT QUOTED STRING
	//if (e.Button == MouseButtons.Left && ModifierKeys == (Keys.Control | Keys.Alt))
	if(event->button == 1 && 
	   event->state == (GDK_CONTROL_MASK | GDK_MOD1_MASK))
	{
		//std::cout << "SELECT QUOTED STRING" << std::endl;
		try
		{

			gint curPos = editor->textEditor->getPositionFromPoint(event->x, event->y);
			editor->textEditor->setCaretPosition(curPos);

			//Style Strings
			if ((editor->textEditor->GetStyleAt(curPos) == 6 && editor->getCurrentSyntaxLanguage() == "csound") ||
			    (editor->textEditor->GetStyleAt(curPos) == 6 && editor->getCurrentSyntaxLanguage() == "lua") ||
			    (editor->textEditor->GetStyleAt(curPos) == 3 && editor->getCurrentSyntaxLanguage() == "python") ||
			    (editor->textEditor->GetStyleAt(curPos) == 4 && editor->getCurrentSyntaxLanguage() == "python"))
			{
				Gdk::Point quotesPos = editor->textEditor->getQuotesPosition(curPos);
				if (quotesPos.get_x() > 0 && quotesPos.get_y() > 0)
				{
					editor->textEditor->setSelection(quotesPos.get_x() - 1, quotesPos.get_y() + 1);
				}
			}
		}
		catch (...)
		{
			wxGLOBAL->DebugPrint("wxMain::onMouseDown Error", "SELECT QUOTED STRING");
		}

		return;
	}



	//HYPERLINKS
	//if (e.Button == MouseButtons.Left && ModifierKeys == Keys.Control)
	if(event->button == 1 && 
	   event->state & GDK_MOD4_MASK)
	   //CONTROL MASK INTERFERE WITH THE SCINTILLA RECTANGULAR SELECTION:
	   //REMEMBER-> ON LINUX SCINTILLA DOES RECTANGULAR SELECTION WITH CTRL KEY
	   //ALT IS USED TO MOVE THE WINDOW - SO WE MUST USE CMD (WINDOWS) KEY
	   //event->state & GDK_CONTROL_MASK)
	{
		//std::cout << "HYPERLINKS" << std::endl;
		on_contextmenu_file_Clicked();
		return;
	}




	//CONTEXT_MENU
	if(event->button == 3)
	{
		/*
		Gtk::Menu		PopupMenu;
		Gtk::MenuItem   PopupGoToDefinition;
		Gtk::MenuItem   PopupGoToReference;
		Gtk::MenuItem   PopupOpcodeHelp;
		Gtk::MenuItem   PopupOpenFile;
		Gtk::MenuItem   PopupComment;
		Gtk::MenuItem   PopupBookmarks;
		*/
		
		//std::cout << "Right Mouse Clicked" << std::endl;

		//REMEMBER: The caret is moved in ScintillaGTK->PressThis (LOOK AT: MODDED FOR WINXOUND)!!!
		//wxEditor tEditor = (wxEditor)sender;
		//Int32 mCurPos = tEditor.textEditor.GetCaretPosition();

		Glib::ustring curWord = 
			ActiveEditor()->textEditor->getWordAt(ActiveEditor()->textEditor->getCaretPosition());
		gint curPos = ActiveEditor()->textEditor->getCaretPosition();

		try
		{

			//CUT-COPY-PASTE
			//bool enabled = (strlen(ActiveEditor()->textEditor->getSelectedText()) > 0);
			//[itemCut setEnabled:enabled];
			//[itemCopy setEnabled:enabled];
			//[itemPaste setEnabled:[textEditor getCanPaste]];

			PopupOpcodeHelp.set_label("Opcodes Help");
			PopupGoToDefinition.set_label("Go to definition of ...");
			PopupGoToReference.set_label("Go to reference of ...");

			//OPCODES DEFINITIONS
			//if (Opcodes.Contains(gCurWord))
			if (g_hash_table_lookup(Opcodes, curWord.c_str()) != NULL)
			{
				//mContextMenu.Items[13].Text = gCurWord + " -> Help";
				PopupOpcodeHelp.set_label(
					Glib::ustring::compose("%1 -> Help", curWord));

			}
			//else if (!string.IsNullOrEmpty(gCurWord))
			else if (curWord != "")
			{
				PopupGoToDefinition.set_label(
					Glib::ustring::compose("Go to definition of %1", curWord));
				PopupGoToReference.set_label(
					Glib::ustring::compose("Go to reference of %1", curWord));
			}

			else
			{
				PopupOpcodeHelp.set_label("Opcodes Help");
				PopupGoToDefinition.set_label("Go to definition of ...");
				PopupGoToReference.set_label("Go to reference of ...");
			}


			//HYPERLINKS
			if ((editor->textEditor->GetStyleAt(curPos) == 6 && editor->getCurrentSyntaxLanguage() == "csound") ||
			    (editor->textEditor->GetStyleAt(curPos) == 6 && editor->getCurrentSyntaxLanguage() == "lua") ||
			    (editor->textEditor->GetStyleAt(curPos) == 3 && editor->getCurrentSyntaxLanguage() == "python") ||
			    (editor->textEditor->GetStyleAt(curPos) == 4 && editor->getCurrentSyntaxLanguage() == "python"))
			{
				//mContextMenu.Items[11].Enabled = true;
				PopupOpenFile.set_sensitive(TRUE);
			}
			else
			{
				//mContextMenu.Items[11].Enabled = false;
				PopupOpenFile.set_sensitive(FALSE);
			}

			//Finally show the context menu
			PopupMenu.popup(event->button, event->time);
		}

		catch (...)
		{
			wxGLOBAL->DebugPrint("wxMain::onMouseDown Error", "Context Menu Section");
		}
	}
}


void wxMain::onEditorFocus(bool isEntered)
{
	//std::cout << "onEditorFocus: " << isEntered << std::endl;
	if(ActiveEditor() == NULL) return;

	//std::cout << "onEditorFocus: " << ActiveEditor()->textEditor->getFocus() << std::endl;

	if(isEntered)
		CheckUndoRedo();
	else
	{
		menuitemUndo->set_sensitive(FALSE);
		menuitemRedo->set_sensitive(FALSE);
	}
	menuitemCut->set_sensitive(isEntered);
	menuitemCopy->set_sensitive(isEntered);
	menuitemPaste->set_sensitive(isEntered);
	menuitemDelete->set_sensitive(isEntered);
	menuitemSelectAll->set_sensitive(isEntered);

}



////////////////////////////////////////////////////////////////////////////////


void wxMain::on_contextmenu_file_Clicked()
{
	//HYPERLINKS
	try
	{
		if (ActiveEditor() == NULL) return;

		wxEditor* tEditor = ActiveEditor();
		gint curPos = tEditor->textEditor->getCaretPosition();

		//Style Strings
		if ((tEditor->textEditor->GetStyleAt(curPos) == 6 && tEditor->getCurrentSyntaxLanguage() == "csound") ||
		    (tEditor->textEditor->GetStyleAt(curPos) == 6 && tEditor->getCurrentSyntaxLanguage() == "lua") ||
		    (tEditor->textEditor->GetStyleAt(curPos) == 3 && tEditor->getCurrentSyntaxLanguage() == "python") ||
		    (tEditor->textEditor->GetStyleAt(curPos) == 4 && tEditor->getCurrentSyntaxLanguage() == "python"))
		{
			Glib::ustring mString = tEditor->textEditor->getTextInQuotes(curPos);

			//if (string.IsNullOrEmpty(mString)) return;
			if(mString == "") return;

			OpenHyperLinks(mString.c_str());
		}
	}

	
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain on_contextmenu_file_Clicked Error", "");
	}
	

	return;
}

void wxMain::on_contextmenu_file_as_text_Clicked()
{

	Gtk::FileChooserDialog dialog("Open File",
	                              Gtk::FILE_CHOOSER_ACTION_OPEN);
	dialog.set_transient_for(*this);

	//Add response buttons the the dialog:
	dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
	dialog.add_button(Gtk::Stock::OPEN, Gtk::RESPONSE_OK);

	dialog.set_select_multiple(FALSE);
	dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

	//Add filters, so that only certain file types can be selected:
	Gtk::FileFilter filter_any;
	filter_any.set_name("Any files");
	filter_any.add_pattern("*");
	dialog.add_filter(filter_any);

	//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
	if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
	                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
	{
		dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
	}

	
	int result = dialog.run();
	dialog.hide();

	//Handle the response:
	if (result != Gtk::RESPONSE_OK) return;
	{
		wxSETTINGS->Directory.LastUsedPath = dialog.get_current_folder();

		std::string filename = dialog.get_filename();
		filename.insert(0, "\"");
		filename.append("\"");
		if(ActiveEditor() != NULL)
		{
			//gint position = ActiveEditor()->textEditor->getCaretPosition();

			if(ActiveEditor()->textEditor->getSelectedText().size() > 0)
				ActiveEditor()->textEditor->ReplaceText(filename.c_str());
			else
				ActiveEditor()->textEditor->AddText(filename.c_str());
		}
		
	}
	
}



void wxMain::OpenHyperLinks(const gchar* mString)
{
	if (ActiveEditor() == NULL) return;

	Glib::ustring filename = "";
	
	try
	{
		//Search for current file directory
		//if (File.Exists(Path.GetDirectoryName(ActiveEditor.FileName) + "\\" + mString))
		if(Glib::file_test(Glib::ustring::compose("%1/%2", 
		                                          Glib::path_get_dirname(ActiveEditor()->FileName),
		                                          mString),
		                   Glib::FILE_TEST_EXISTS))
		{
			//mString = Path.GetDirectoryName(ActiveEditor.FileName) + "\\" + mString;
			filename = Glib::ustring::compose("%1/%2", 
			                                 Glib::path_get_dirname(ActiveEditor()->FileName),
			                                 mString);
		}
		
		//Search for full path
		//else if (File.Exists(mString))
		else if (Glib::file_test(mString, Glib::FILE_TEST_EXISTS))
		{
			//check for slash
			//mString = mString.Replace("/", "\\");
			filename = mString;
		}
		
		//Search for INCDIR
		//else if (File.Exists(wxGlobal.Settings.Directory.INCDIR + "\\" + mString))
		else if (Glib::file_test(Glib::ustring::compose("%1/%2", 
		                                                wxSETTINGS->Directory.INCDIR,
		                                                mString),
		                         Glib::FILE_TEST_EXISTS))
		{
			//mString = wxGlobal.Settings.Directory.INCDIR + "\\" + mString;
			filename = Glib::ustring::compose("%1/%2", 
			                                  wxSETTINGS->Directory.INCDIR,
			                                  mString);
		}
		
		//Search for SFDIR
		//else if (File.Exists(wxGlobal.Settings.Directory.SFDIR + "\\" + mString))
		else if (Glib::file_test(Glib::ustring::compose("%1/%2", 
		                                                wxSETTINGS->Directory.SFDIR,
		                                                mString),
		                         Glib::FILE_TEST_EXISTS))
		{
			//mString = wxGlobal.Settings.Directory.SFDIR + "\\" + mString;
			filename = Glib::ustring::compose("%1/%2", 
			                                  wxSETTINGS->Directory.SFDIR,
			                                  mString);
		}
		
		//Search for SSDIR
		//else if (File.Exists(wxGlobal.Settings.Directory.SSDIR + "\\" + mString))
		else if (Glib::file_test(Glib::ustring::compose("%1/%2", 
		                                                wxSETTINGS->Directory.SSDIR,
		                                                mString),
		                         Glib::FILE_TEST_EXISTS))
		{
			//mString = wxGlobal.Settings.Directory.SSDIR + "\\" + mString;
			filename = Glib::ustring::compose("%1/%2", 
			                                  wxSETTINGS->Directory.SSDIR,
			                                  mString);
			
		}
		
		//Search for SADIR
		//else if (File.Exists(wxGlobal.Settings.Directory.SADIR + "\\" + mString))
		else if (Glib::file_test(Glib::ustring::compose("%1/%2", 
		                                                wxSETTINGS->Directory.SADIR,
		                                                mString),
		                         Glib::FILE_TEST_EXISTS))
		{
			//mString = wxGlobal.Settings.Directory.SADIR + "\\" + mString;
			filename = Glib::ustring::compose("%1/%2", 
			                                  wxSETTINGS->Directory.SADIR,
			                                  mString);
		}
		
		//Search for MFDIR
		//else if (File.Exists(wxGlobal.Settings.Directory.MFDIR + "\\" + mString))
		else if (Glib::file_test(Glib::ustring::compose("%1/%2", 
		                                                wxSETTINGS->Directory.MFDIR,
		                                                mString),
		                         Glib::FILE_TEST_EXISTS))
		{
			//mString = wxGlobal.Settings.Directory.MFDIR + "\\" + mString;
			filename = Glib::ustring::compose("%1/%2", 
			                                  wxSETTINGS->Directory.MFDIR,
			                                  mString);
		}

		//System.Diagnostics.Debug.WriteLine(filename);
		//std::cout << "Hyperlinks: " << filename << std::endl;
		
		//if (!File.Exists(filename)) return;
		if (!Glib::file_test(filename, Glib::FILE_TEST_EXISTS)) return;

		
		//OPEN AUDIO FILES
		//if (mString.ToLower().EndsWith(".wav") ||
		//    mString.ToLower().EndsWith(".aif") ||
		//    mString.ToLower().EndsWith(".aiff"))
		if(Glib::str_has_suffix(filename.lowercase(), ".wav") ||
		   Glib::str_has_suffix(filename.lowercase(), ".aif") ||
		   Glib::str_has_suffix(filename.lowercase(), ".aiff"))
		{
			//if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WaveEditor))
			if(wxSETTINGS->Directory.WaveEditor != "")
			{
				//if (File.Exists(wxGlobal.Settings.Directory.WaveEditor))
				if(Glib::file_test(wxSETTINGS->Directory.WaveEditor, Glib::FILE_TEST_EXISTS))
				{
					//System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.WaveEditor,
					//                                 "\"" + mString + "\"");
					Glib::ustring temp = "\"";
					temp.append(filename);
					temp.append("\"");
					
					this->executeCommand(NULL, 
					                     wxSETTINGS->Directory.WaveEditor.c_str(), 
					                     temp.c_str());				
				}
				else
				{
					wxGLOBAL->ShowMessageBox("Cannot find External Wave Editor Path!\n"
					                         "Please select a valid path in File->Settings->Directories->Wave Editor executable",
					                         "WinXound info!",
					                         Gtk::BUTTONS_OK);
				}
			}
			else
			{
				if(Glib::file_test(wxSETTINGS->Directory.WavePlayer, Glib::FILE_TEST_EXISTS))
				{
					//System.Diagnostics.Process.Start("wmplayer", "\"" + mString + "\"");
					Glib::ustring temp = "\"";
					temp.append(filename);
					temp.append("\"");
					
					this->executeCommand(NULL, 
					                     wxSETTINGS->Directory.WavePlayer.c_str(), 
					                     temp.c_str());

				}
			}
		}
		
		//OPEN MIDI FILES
		//else if (mString.ToLower().EndsWith(".mid"))
		else if(Glib::str_has_suffix(filename.lowercase(), ".mid"))
		{
			if(Glib::file_test(wxSETTINGS->Directory.WavePlayer, Glib::FILE_TEST_EXISTS))
			{
				//System.Diagnostics.Process.Start("wmplayer", "\"" + mString + "\"");
				Glib::ustring temp = "\"";
				temp.append(filename);
				temp.append("\"");
				
				this->executeCommand(NULL, 
				                     wxSETTINGS->Directory.WavePlayer.c_str(), 
				                     temp.c_str());;
			}
		}
		
		//TRY TO OPEN OTHER FILES
		else
		{
			AddNewEditor(filename.c_str());
		}

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain OpenHyperLinks Error", "");
	}
}






void wxMain::SetHighlightLanguage(wxEditor* editor, bool refresh)
{
	Glib::ustring filename = editor->FileName;
	
	//Set Highlight language
	//if (filename.ToLower().EndsWith(".py") ||
	//    filename.ToLower().EndsWith(".pyw"))
	if(Glib::str_has_suffix(filename.lowercase(),".py") ||
	   Glib::str_has_suffix(filename.lowercase(),".pyw"))
	{
		//Python syntax
		if (wxSETTINGS->EditorProperties.UseMixedPython)
		{
			editor->textEditor->setHighlight("winxoundpython");
			editor->ConfigureEditorForPythonMixed(Opcodes);
		}
		else
		{
			editor->textEditor->setHighlight("python");
			editor->ConfigureEditorForPython();
		}

	}
	else if (Glib::str_has_suffix(filename.lowercase(),".lua") )
	{
		editor->textEditor->setHighlight("lua");
		editor->ConfigureEditorForLua();
	}
	else if (Glib::str_has_suffix(filename.lowercase(),".csd") ||
	         Glib::str_has_suffix(filename.lowercase(),".inc") ||
	         Glib::str_has_suffix(filename.lowercase(),".udo") ||
	         Glib::str_has_suffix(filename.lowercase(),".orc") ||
	         Glib::str_has_suffix(filename.lowercase(),".sco"))
	{
		//CSound syntax
		editor->textEditor->setHighlight("winxound");
		editor->ConfigureEditorForCSound(Opcodes, mRepository->GetUdoOpcodesList());
	}
	else
	{
		editor->textEditor->setHighlight("");
		editor->ConfigureEditorForText();
	}


	if (refresh)
	{
		//Refresh syntax
		editor->textEditor->RefreshSyntax();
	}

}



void wxMain::CheckUndoRedo()
{
	if (ActiveEditor() == NULL) return;
	
	//ToolStripUndo.Enabled = ActiveEditor.textEditor.CanUndo;
	toolbuttonUndo->set_sensitive(ActiveEditor()->textEditor->getCanUndo());
	//MenuEditUndo.Enabled = ActiveEditor.textEditor.CanUndo;
	menuitemUndo->set_sensitive(ActiveEditor()->textEditor->getCanUndo());

	//ToolStripRedo.Enabled = ActiveEditor.textEditor.CanRedo;
	toolbuttonRedo->set_sensitive(ActiveEditor()->textEditor->getCanRedo());
	//MenuEditRedo.Enabled = ActiveEditor.textEditor.CanRedo;
	menuitemRedo->set_sensitive(ActiveEditor()->textEditor->getCanRedo());
}



void wxMain::CreateNotebookCompilerTabs()
{
	//notebookCompiler tabs
	//gint index = notebookCompiler->append_page(*helpWindow, "Help");
	//gint index = notebookCompiler->append_page(*csoundRepository, "CSound Opcodes");
	//gint index = notebookCompiler->append_page(*codeRepository, "Code Repository");

	oldSelectedTab = 0;

	
	//COMPILER
	Glib::ustring compilerName = "Compiler ";
	if (wxSETTINGS->General.UseWinXoundFlags == true)
		compilerName.append("[WinXound flags]");
	else
		compilerName.append("[CsOptions flags]");
	
	Gtk::EventBox* eb1 = Gtk::manage(new Gtk::EventBox());
	compilerLabel = Gtk::manage(new Gtk::Label(compilerName));
	eb1->add(*compilerLabel);
	eb1->set_events(Gdk::BUTTON_PRESS_MASK);
	eb1->signal_button_press_event().connect(sigc::mem_fun(*this, &wxMain::on_Tab1_Clicked));
	eb1->show_all_children();
	gint index = notebookCompiler->append_page(*compiler->getTerminalWidget(), *eb1);


	//HELP
	Gtk::EventBox* eb2 = Gtk::manage(new Gtk::EventBox());
	eb2->add(*Gtk::manage(new Gtk::Label("Help")));
	eb2->set_events(Gdk::BUTTON_PRESS_MASK);
	eb2->signal_button_press_event().connect(sigc::mem_fun(*this, &wxMain::on_Tab2_Clicked));
	eb2->show_all_children();
	index = notebookCompiler->append_page(*HelpBrowser->getBrowserWidget(), *eb2);

	HelpBrowser->goHome();


	//TERMINAL
	Gtk::EventBox* eb3 = Gtk::manage(new Gtk::EventBox());
	eb3->add(*Gtk::manage(new Gtk::Label("Terminal")));
	eb3->set_events(Gdk::BUTTON_PRESS_MASK);
	eb3->signal_button_press_event().connect(sigc::mem_fun(*this, &wxMain::on_Tab3_Clicked));
	eb3->show_all_children();
	index = notebookCompiler->append_page(*terminal->getTerminalWidget(), *eb3);
	

	//CSOUND REPOSITORY
	Gtk::EventBox* eb4 = Gtk::manage(new Gtk::EventBox());
	eb4->add(*Gtk::manage(new Gtk::Label("CSound Opcodes")));
	eb4->set_events(Gdk::BUTTON_PRESS_MASK);
	eb4->signal_button_press_event().connect(sigc::mem_fun(*this, &wxMain::on_Tab4_Clicked));
	eb4->show_all_children();
	index = notebookCompiler->append_page(*mCSoundRepository, *eb4);


	//REPOSITORY
	Gtk::EventBox* eb5 = Gtk::manage(new Gtk::EventBox());
	eb5->add(*Gtk::manage(new Gtk::Label("UDO Repository")));
	eb5->set_events(Gdk::BUTTON_PRESS_MASK);
	eb5->signal_button_press_event().connect(sigc::mem_fun(*this, &wxMain::on_Tab5_Clicked));
	eb5->show_all_children();
	index = notebookCompiler->append_page(*mRepository, *eb5);


/*	//CABBAGE REPOSITORY
	Gtk::EventBox* eb6 = Gtk::manage(new Gtk::EventBox());
	eb6->add(*Gtk::manage(new Gtk::Label("Cabbage Repository")));
	eb6->set_events(Gdk::BUTTON_PRESS_MASK);
	eb6->signal_button_press_event().connect(sigc::mem_fun(*this, &wxMain::on_Tab6_Clicked));
	eb6->show_all_children();
	index = notebookCompiler->append_page(*mCabbageRepository, *eb6);*/



	
	/*
	//g_signal_stop_emission_by_name(notebookCompiler->gobj(), "switch-page");//"switch-page" );
	//g_signal_stop_emission_by_name(eb3->gobj(), "drag-enter");//"switch-page" );
	std::list<Gtk::TargetEntry> listTargets;
	//text/uri-list
	listTargets.push_back( Gtk::TargetEntry("") );

	notebookCompiler->drag_dest_set(listTargets);
	notebookCompiler->signal_drag_data_received().connect(
		sigc::mem_fun(*this, &wxMain::on_DragDataReceived),false);
	*/
	notebookCompiler->signal_drag_motion().connect(sigc::mem_fun(*this, &wxMain::on_labels_drag_motion));
	notebookCompiler->signal_drag_end().connect(sigc::mem_fun(*this, &wxMain::on_notebookCompiler_drag_end));
	notebookCompiler->signal_drag_leave().connect(sigc::mem_fun(*this, &wxMain::on_notebookCompiler_drag_leave));
	
}

void wxMain::on_notebookCompiler_drag_end(const Glib::RefPtr<Gdk::DragContext>& context)
{
	//wxGLOBAL->DebugPrint("NOTEBOOKCOMPILER DRAG_END","");
	isDragAndDrop = false;
}

void wxMain::on_notebookCompiler_drag_leave(const Glib::RefPtr<Gdk::DragContext>& context, guint time)
{
	//wxGLOBAL->DebugPrint("NOTEBOOKCOMPILER DRAG_LEAVE","");
	isDragAndDrop = false;
}

bool wxMain::on_labels_drag_motion(const Glib::RefPtr<Gdk::DragContext>& context, int x, int y, guint time)
{
	//wxGLOBAL->DebugPrint("NOTEBOOKCOMPILER DRAG_MOTION","");
	isDragAndDrop = true;
	//wxGLOBAL->DebugPrint("on_labels_drag_motion","");
	notebookCompiler->set_current_page(oldSelectedTab);
	return true;
}


bool wxMain::on_Tab1_Clicked(GdkEventButton* event)
{
	on_Tabs_Clicked(0);
	return false;
}
bool wxMain::on_Tab2_Clicked(GdkEventButton* event)
{
	on_Tabs_Clicked(1);
	return false;
}
bool wxMain::on_Tab3_Clicked(GdkEventButton* event)
{
	on_Tabs_Clicked(2);
	return false;
}
bool wxMain::on_Tab4_Clicked(GdkEventButton* event)
{
	on_Tabs_Clicked(3);
	return false;
}
bool wxMain::on_Tab5_Clicked(GdkEventButton* event)
{
	on_Tabs_Clicked(4);
	return false;
}

bool wxMain::on_Tab6_Clicked(GdkEventButton* event)
{
	on_Tabs_Clicked(5);
	return false;
}

void wxMain::on_Tabs_Clicked(gint currentIndex)
{
	//wxGLOBAL->DebugPrint("on_Tabs_Clicked","");
	
	if((wxMainWindow->get_height() - vpanedMain->get_position()) <= 120)
	{
		//vpanedMain->set_position(wxMainWindow->get_height() / TAB_POPUP_RATIO);
		vpanedMain->set_position(wxMainWindow->get_height() - oldHeight);
	}
	else if(currentIndex == oldSelectedTab)
	{
		on_menuitemShowCode_Clicked();
		if(ActiveEditor() != NULL)
			ActiveEditor()->SetFocus();
	}

	oldSelectedTab = currentIndex;
	
}

void wxMain::on_notebookCompiler_resize(Gdk::Rectangle& rect)
{	
	//Set oldHeight (the minimum is set to 155)
	if ((wxMainWindow->get_height() - vpanedMain->get_position()) > 120)  //155
		oldHeight = wxMainWindow->get_height() - vpanedMain->get_position();


	//Debug:
	/* 
	std::cout << "oldHeight: " << oldHeight 
		      << " - MainWindow height: " << wxMainWindow->get_height() 
		      << " - Notebook_Compiler_Size: " << NOTEBOOK_COMPILER_SIZE 
		      << " - Sum: " << wxMainWindow->get_height() - vpanedMain->get_position()
			  << " - Rect: " << rect.get_height()
		      << std::endl;
	*/
}

Glib::ustring wxMain::SaveBeforeCompile()
{
	if (ActiveEditor() == NULL) return "";

	if (ActiveEditor()->FileIsReadOnly == false)
	{
		if(!Glib::file_test(ActiveEditor()->FileName, Glib::FILE_TEST_EXISTS))
		{
			Glib::ustring tempFilename = wxGLOBAL->getSettingsPath();
			tempFilename.append("/");
			tempFilename.append(WINXOUND_UNTITLED);
			tempFilename.append(wxGLOBAL->getFileExtension(ActiveEditor()->FileName));

			std::cout << "tempFilename = " << tempFilename << std::endl;

			//Create a temp file inside the SettingsDir
			Glib::file_set_contents(tempFilename,
			                        ActiveEditor()->textEditor->getText());
			return tempFilename;
		}
		else
		{
			on_menuitemSave_Clicked();
			return ActiveEditor()->FileName;
		}
	}

	return "";
}


void wxMain::Compile()
{
	Compile("");
}

void wxMain::Compile(Glib::ustring additionalParams)
{
	if (ActiveEditor() == NULL) return;

	if (compiler->ProcessActive)
	{
		wxGLOBAL->ShowMessageBox(this,
		                         "Compiler is already running!\n"
		                         "Stop it manually and retry.",
		                         "WinXound Info",
		                         Gtk::BUTTONS_OK);
		return;
	}
	


	//Save file before to compile
	Glib::ustring TextEditorFileName = SaveBeforeCompile();

	
	//OLD BEHAVIOUR: Now WinXound save automatically the Untitled document
    //to a temporary file
	/*
	if (ActiveEditor()->FileName == UNTITLED_CSD ||
	    ActiveEditor()->FileName == UNTITLED_PY ||
	    ActiveEditor()->FileName == UNTITLED_LUA ||
		!Glib::file_test(ActiveEditor()->FileName, Glib::FILE_TEST_EXISTS))
	{
		wxGLOBAL->ShowMessageBox(this,
		                         "Please specify a valid filename before to compile.",
		                         "Compiler Error - Invalid filename",
		                         Gtk::BUTTONS_OK);
		return;
	}
	*/
	


	try
	{
		//COMPILE WITH CSOUND
		if (Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".csd") ||
		    Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".orc") ||
		    Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".sco"))
		{
			if (wxSETTINGS->Directory.CSoundConsole == "" ||
				!Glib::file_test(wxSETTINGS->Directory.CSoundConsole, Glib::FILE_TEST_EXISTS))
			{
				wxGLOBAL->ShowMessageBox(this,
				                         "Cannot find CSound compiler!\n"
				                         "Please select a valid path in File->Settings->Directories->CSound executable",
				                         "Compiler Error",
				                         Gtk::BUTTONS_OK);

				return;
			}

			
			additionalParams = CheckForAdditionalFlags(additionalParams);
			if(additionalParams == "[CANCEL]") return;
			
			//Set mCompilerEditor to the ActiveEditor(): Needed for CompilerCompleted
			mCompilerEditor = ActiveEditor();



			//COMPILE WITH CSOUND:
			//Set CSound compiler arguments
			Glib::ustring finalParams = wxGLOBAL->Trim(additionalParams);
			if(finalParams.size() > 0)
				finalParams.append(" ");
			finalParams.append(GetCSoundFlags());

			std::cout << "Final Params:" << finalParams << std::endl;
			
			/* Compile params:
			void wxTerminal::Compile(Glib::ustring compilerName,
						             Glib::ustring parameters,
						             Glib::ustring filename1,
						             Glib::ustring filename2)
			*/
			if(Glib::str_has_suffix(TextEditorFileName.lowercase(), ".orc"))
			{
				compiler->Compile(wxSETTINGS->Directory.CSoundConsole, //"/usr/bin/csound", 
				                  finalParams,
				                  TextEditorFileName,
				                  ActiveEditor()->GetCurrentOrcScoFile());
			}
			else if(Glib::str_has_suffix(TextEditorFileName.lowercase(), ".sco"))
			{
				compiler->Compile(wxSETTINGS->Directory.CSoundConsole, //"/usr/bin/csound", 
				                  finalParams,
				                  ActiveEditor()->GetCurrentOrcScoFile(),
				                  TextEditorFileName);
			}
			else
			{
				compiler->Compile(wxSETTINGS->Directory.CSoundConsole, //"/usr/bin/csound", 
				                  finalParams,
				                  TextEditorFileName,
				                  "");
			}

		}

		
		//COMPILE WITH PYTHON
		//else if (ActiveEditor.FileName.ToLower().EndsWith(".py") ||
		//         ActiveEditor.FileName.ToLower().EndsWith(".pyw"))
		else if (Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".py") ||
				 Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".pyw"))
		{
			//if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.PythonConsolePath) ||
			//    !File.Exists(wxGlobal.Settings.Directory.PythonConsolePath))
			if (wxSETTINGS->Directory.PythonConsolePath == "" ||
				!Glib::file_test(wxSETTINGS->Directory.PythonConsolePath, Glib::FILE_TEST_EXISTS))
			{
				wxGLOBAL->ShowMessageBox(this,
				                         "Cannot find Python compiler!\n"
				                         "Please select a valid path in File->Settings->Directories->Python executable",
				                         "Compiler Error",
				                         Gtk::BUTTONS_OK);

				return;
			}


			mCompilerEditor = ActiveEditor();

			/*
			wxCompilerConsole1.Title = ActiveEditor.FileName;
			string mArguments = wxGlobal.Settings.General.PythonDefaultFlags.Trim();
			if (string.IsNullOrEmpty(mArguments))
				mArguments = "-u";
			wxCompilerConsole1.Filename = wxGlobal.Settings.Directory.PythonConsolePath;
			wxCompilerConsole1.Arguments =
				mArguments + " " + "\"" + ActiveEditor.FileName + "\"";
			//With Python we send the temporary files to the filename directory
			wxCompilerConsole1.WorkingDirectory =
				Directory.GetParent(ActiveEditor.FileName).ToString();
			wxCompilerConsole1.StartCompiler(false);
			*/

			/*
			void wxTerminal::Compile(Glib::ustring compilerName,
                         Glib::ustring parameters,
                         Glib::ustring filename1,
                         Glib::ustring filename2)
			*/
			compiler->Compile(wxSETTINGS->Directory.PythonConsolePath, //"/usr/bin/csound", 
			                  wxGLOBAL->Trim(wxSETTINGS->General.PythonDefaultFlags),
			                  ActiveEditor()->FileName,
			                  "");			                         

		}


		//COMPILE WITH LUA
		//else if (ActiveEditor.FileName.ToLower().EndsWith(".lua"))
		else if (Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".lua"))
		{
			//if (string.IsNullOrEmpty(wxGlobal.Settings.Directory.LuaConsolePath) ||
			//    !File.Exists(wxGlobal.Settings.Directory.LuaConsolePath))
			if (wxSETTINGS->Directory.LuaConsolePath == "" ||
				!Glib::file_test(wxSETTINGS->Directory.LuaConsolePath, Glib::FILE_TEST_EXISTS))
			{
				wxGLOBAL->ShowMessageBox(this,
				                         "Cannot find Lua compiler!\n"
				                         "Please select a valid path in File->Settings->Directories->Lua executable",
				                         "Compiler Error",
				                         Gtk::BUTTONS_OK);

				return;
			}

			mCompilerEditor = ActiveEditor();

			/*
			wxCompilerConsole1.Title = ActiveEditor.FileName;
			string mArguments = wxGlobal.Settings.General.LuaDefaultFlags.Trim();
			if (string.IsNullOrEmpty(mArguments))
				mArguments = "";
			wxCompilerConsole1.Filename = wxGlobal.Settings.Directory.LuaConsolePath;
			wxCompilerConsole1.Arguments =
				mArguments + " " + "\"" + ActiveEditor.FileName + "\"";
			//We send the temporary files to the filename directory
			wxCompilerConsole1.WorkingDirectory =
				Directory.GetParent(ActiveEditor.FileName).ToString();
			wxCompilerConsole1.StartCompiler(false);
			*/

			compiler->Compile(wxSETTINGS->Directory.LuaConsolePath, //"/usr/bin/csound", 
			                  wxGLOBAL->Trim(wxSETTINGS->General.LuaDefaultFlags),
			                  ActiveEditor()->FileName,
			                  "");

		}

		//Disable menu compile item and toolbutton compile item
		//toolbuttonCompile->set_sensitive(FALSE);
		//menuitemCompile->set_sensitive(FALSE);

		//Set focus on compiler window
		on_menuitemShowCompiler_Clicked();
		compiler->SetFocus();
		
	}
	catch(...)
	{
		wxGLOBAL->DebugPrint("Form Main - MenuToolsCompile_Click Error");
		
		//toolbuttonCompile->set_sensitive(TRUE);
		//menuitemCompile->set_sensitive(TRUE);
	}

}

Glib::ustring wxMain::CheckForAdditionalFlags(Glib::ustring additionalParams)
{
	//Check for additional parameters 
	//[*=current editor filename
	//[?=ask for filename]

	//Examples:
	//[Render to filename]: -W -o"CompilerOutput.wav"
	//[Render to file using csd/orc/sco name]: -W -o"*.wav"
	//[Render to file asking for its name]: -W -o"?.wav"
	
	//if (additionalParams.Contains("*"))
	if(additionalParams.find("*") != Glib::ustring::npos)
	{
		if (ActiveEditor() != NULL)
		{
			//additionalParams = additionalParams.Replace("*", 
			//	Path.GetFileNameWithoutExtension(ActiveEditor.FileName));
			
			additionalParams = 
				additionalParams.replace(additionalParams.find("*"),
				                         1,
				                         wxGLOBAL->getFileNameWithoutExtension(ActiveEditor()->FileName));
		}
	}
	
	//else if (additionalParams.Contains("?"))
	else if(additionalParams.find("?") != Glib::ustring::npos)
	{
		//Save file 
		Gtk::FileChooserDialog dialog("Save File",
		                              Gtk::FILE_CHOOSER_ACTION_SAVE);
		dialog.set_transient_for(*this);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		//Add response buttons the the dialog:
		dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
		dialog.add_button(Gtk::Stock::SAVE, Gtk::RESPONSE_OK);

		dialog.set_select_multiple(FALSE);
		dialog.set_current_folder(wxSETTINGS->Directory.LastUsedPath);

		
		//Set dialog filename
		if (ActiveEditor() != NULL)
		{
			Glib::ustring fileToSave = Glib::path_get_basename(
				wxGLOBAL->getFileNameWithoutExtension(ActiveEditor()->FileName));
			
			if(!Glib::file_test(ActiveEditor()->FileName, Glib::FILE_TEST_EXISTS)) //FILE NOT FOUND:UNTITLED
			{
				fileToSave = fileToSave.lowercase();
			}

			
			/* 
			//Check for extension:
			size_t start = additionalParams.find("?");
			size_t end = additionalParams.find("\"",start);
			if(start != Glib::ustring::npos &&
			   end != Glib::ustring::npos)
			{
				start +=1;
				fileToSave.append(additionalParams.substr(start, end-start));
			}
			*/             
			
			dialog.set_current_name(Glib::path_get_basename(fileToSave));
		}

		
		//Ask for overwrite
		dialog.set_do_overwrite_confirmation(TRUE);
		

		//Add filters
		Gtk::FileFilter filter_any;
		filter_any.set_name("Any files");
		filter_any.add_pattern("*");
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

			//Single file
			std::string filename = dialog.get_filename();

			//additionalParams =
			//	additionalParams.Replace("?",   //-o"?.wav",
			//	                         saveFileDialog1.FileName);
			additionalParams = 
				additionalParams.replace(additionalParams.find("?"),
				                         1,
				                         filename);

		}
		else
		{
			return "[CANCEL]";
		}
	}

	//Replace quotes (") occurences
	while(true)
	{
		size_t index = additionalParams.find("\"");
		if(index == Glib::ustring::npos) break;
		
		additionalParams = 
				additionalParams.replace(index,
				                         1,
				                         "");
	}

	return additionalParams;
}

Glib::ustring wxMain::GetCSoundFlags()
{
	if (ActiveEditor() == NULL)
	{
		return wxGLOBAL->Trim(wxSETTINGS->General.CSoundDefaultFlags);
	}

	
	//Default WinXound flags for CSound:
	//-B4096 --displays --asciidisplay

	Glib::ustring flags = "";

	try
	{
		//menuitemUseWinXoundFlags->get_active();
		//if (MenuToolsUseDefaultFlags.Checked == false)
		if (menuitemUseWinXoundFlags->get_active() == FALSE)
		{
			//User has unchecked 'UseDefaultFlags' menu, so we skip default flags
			flags = "";
			//wxGLOBAL->DebugPrint("MenuToolsUseDefaultFlags = false", "");
		}

		//TODO: ... maybe must be tested a little more
		else if (CheckDisplaysFlag() == true)
		{
			//Found --displays option in the code so:
			//temporarily remove all 'display(s)' settings from the default winxound 
			//settings and leave the others
			Glib::ustring defaultflags = wxGLOBAL->Trim(wxSETTINGS->General.CSoundDefaultFlags);

			size_t found; //Index, Size, String
			found = defaultflags.find("--displays");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("--displays", "");
				defaultflags = defaultflags.replace(found, strlen("--displays"),"");

			found = defaultflags.find("--nodisplays");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("--nodisplays", "");
				defaultflags = defaultflags.replace(found, strlen("--nodisplays"),"");

			found = defaultflags.find("-d ");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("-d ", "");
				defaultflags = defaultflags.replace(found, strlen("-d ")," ");

			found = defaultflags.find("--asciidisplay");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("--asciidisplay", "");
				defaultflags = defaultflags.replace(found, strlen("--asciidisplay"),"");

			found = defaultflags.find("-g ");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("-g ", "");
				defaultflags = defaultflags.replace(found, strlen("-g ")," ");
			
			found = defaultflags.find("--postscriptdisplay");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("--postscriptdisplay", "");
				defaultflags = defaultflags.replace(found, strlen("--postscriptdisplay"),"");
			
			found = defaultflags.find("-G ");
			if(found != Glib::ustring::npos)
				//defaultflags = defaultflags.replace("-G ", "");
				defaultflags = defaultflags.replace(found, strlen("-G ")," ");

			
			flags = wxGLOBAL->Trim(defaultflags);
			flags.append(" ");

			//wxGLOBAL->DebugPrint("CheckDisplaysFlag = true", "");
		}

		else
		{
			//USE DEFAULT WINXOUND SETTINGS FOR CSOUND
			flags = wxGLOBAL->Trim(wxSETTINGS->General.CSoundDefaultFlags);
			//wxGLOBAL->DebugPrint("Default Flags used", "");
		}

		flags = wxGLOBAL->Trim(flags);

		//wxGLOBAL->DebugPrint("Flags", flags.c_str());

	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain::GetCSoundFlags Error");
		return wxGLOBAL->Trim(wxSETTINGS->General.CSoundDefaultFlags);
	}

	return flags;
}


bool wxMain::CheckDisplaysFlag()
{
	
	if (ActiveEditor() == NULL) return false;

	//If --displays flag is present we must show csound windows and suppress the
	//--asciidisplay or --postscriptdisplay
	try
	{
		//Find <CsOptions> tags
		gint findStart = ActiveEditor()->textEditor->FindText("<CsOptions>", true, true, false,
		                                                      false, false, true,
		                                                      0, ActiveEditor()->textEditor->getTextLength());

		gint findEnd = ActiveEditor()->textEditor->FindText("</CsOptions>", true, true, false,
		                                                   false, false, true,
		                                                   0, ActiveEditor()->textEditor->getTextLength());

		if (findStart > -1 && findEnd > -1)
		{
			gint linestart, lineend;
			linestart = ActiveEditor()->textEditor->getLineNumberFromPosition(findStart);
			lineend = ActiveEditor()->textEditor->getLineNumberFromPosition(findEnd);

			for (gint i = linestart; i < lineend; i++)
			{
				Glib::ustring text =
					ActiveEditor()->textEditor->getTextOfLine(i);

				//if (text.IndexOf(";") > -1)
				if(text.find(";") != Glib::ustring::npos)
				{
					//text = text.Remove(text.IndexOf(";"));
					size_t index = text.find(";");
					if(index != Glib::ustring::npos)
						text = text.substr(0, index);
					
					//if (text.Contains("--displays"))
					if(text.find("--displays") != Glib::ustring::npos)
					{
						//if (!text.Contains("--asciidisplay") &&
						//    !text.Contains("-g") &&
						//    !text.Contains("--postscriptdisplay") &&
						//    !text.Contains("-G"))
						if(text.find("--asciidisplay") == Glib::ustring::npos &&
						   text.find("-g") == Glib::ustring::npos &&
						   text.find("--postscriptdisplay") == Glib::ustring::npos &&
						   text.find("-G") == Glib::ustring::npos)
						{
							return true;
						}
					}
				}
			}
		}

	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain::CheckDisplayFlag Error");
	}

	return false;
}








void wxMain::CheckMenuConditions()
{

	//if (wxTabCode.TabPages.Count == 0)
	if(notebookCode->get_n_pages() == 0)
	{
		//this.Text = wxGlobal.TITLE;
		wxMainWindow->set_title(TITLE);

		toolbuttonUndo->set_sensitive( FALSE );
		menuitemUndo->set_sensitive( FALSE );
		toolbuttonRedo->set_sensitive( FALSE );
		menuitemRedo->set_sensitive( FALSE );

		//////////////////////////////
		menuitemSave->set_sensitive( FALSE );
		toolbuttonSave->set_sensitive( FALSE );
		menuitemSaveAs->set_sensitive( FALSE );
		toolbuttonSaveAs->set_sensitive( FALSE );
		menuitemSaveAll->set_sensitive( FALSE );
		menuitemClose->set_sensitive( FALSE );
		toolbuttonCloseDocument->set_sensitive( FALSE );
		menuitemCloseAll->set_sensitive( FALSE );

		/*
		menuitemImportOrcScoToNewCSD->set_sensitive( TRUE);
		menuitemImportOrcSco->set_sensitive( FALSE);
		menuitemImportOrc->set_sensitive( FALSE);
		menuitemImportSco->set_sensitive( FALSE);
		menuitemExportOrcSco->set_sensitive( FALSE );
		menuitemExportOrc->set_sensitive( FALSE );
		menuitemExportSco->set_sensitive( FALSE );
		*/
		CheckImportExportMenu();
		
		//menuitemPageSetup->set_sensitive( FALSE );
		menuitemPrint->set_sensitive( FALSE );
		menuitemPrintPreview->set_sensitive( FALSE );
		menuitemFileInfo->set_sensitive( FALSE );

		menuitemCut->set_sensitive( FALSE );
		menuitemCopy->set_sensitive( FALSE );
		menuitemPaste->set_sensitive( FALSE );
		menuitemDelete->set_sensitive( FALSE );
		menuitemSelectAll->set_sensitive( FALSE );
		
		menuitemFindLineNumber->set_sensitive( FALSE );
		toolbuttonFindLine->set_sensitive( FALSE );

		menuitemFindAndReplace->set_sensitive( FALSE );
		toolbuttonFind->set_sensitive( FALSE );
		menuitemFindNext->set_sensitive( FALSE );
		menuitemFindPrevious->set_sensitive( FALSE );
		menuitemJumpToCaret->set_sensitive( FALSE );

		menuitemShowHideWhiteSpaces->set_sensitive( FALSE );
		menuitemShowHideEOLS->set_sensitive( FALSE );
		//menuitemEOLConversion->set_sensitive( FALSE );
		menuitemLineEndingsCRLF->set_sensitive( FALSE );
		menuitemLineEndingsCR->set_sensitive( FALSE );
		menuitemLineEndingsLF->set_sensitive( FALSE );

		//menuitemCodeFormatting->set_sensitive( TRUE );
		menuitemFormatCode->set_sensitive( FALSE );
		menuitemFormatCodeAll->set_sensitive( FALSE );
		menuitemFormatCodeOptions->set_sensitive( TRUE );

		menuitemCodeRepositoryStoreSelectedText->set_sensitive( FALSE );

		//menuitemComments->set_sensitive( FALSE );
		menuitemCommentLine->set_sensitive( FALSE );
		menuitemRemoveCommentLine->set_sensitive( FALSE );

		menuitemListOpcodes->set_sensitive( FALSE );

		//menuitemBookmarks->set_sensitive( FALSE );
		menuitemInsertRemoveBookmark->set_sensitive( FALSE );
		menuitemRemoveAllBookmarks->set_sensitive( FALSE );
		menuitemGoToNextBookmark->set_sensitive( FALSE );
		menuitemGoToPreviousBookmark->set_sensitive( FALSE );

		menuitemNavigateBackward->set_sensitive( FALSE );
		menuitemNavigateForward->set_sensitive( FALSE );

		menuitemFullCode->set_sensitive( FALSE );
		menuitemSplitHorizontal->set_sensitive( FALSE );
		menuitemSplitHorizontalOrcSco->set_sensitive( FALSE );
		menuitemSplitVertical->set_sensitive( FALSE );
		menuitemSplitVerticalOrcSco->set_sensitive( FALSE );
		//tsCode->set_sensitive( FALSE );

		menuitemSetCaretOnPrimaryView->set_sensitive( FALSE );
		menuitemSetCaretOnSecondaryView->set_sensitive( FALSE );

		//MenuFontSize->set_sensitive( FALSE );
		menuitemResetTextZoom->set_sensitive( FALSE );
		toolbuttonResetZoom->set_sensitive( FALSE );
		toolbuttonZoomOut->set_sensitive( FALSE );
		toolbuttonZoomIn->set_sensitive( FALSE );
		//toolbuttonButtonFontSize->set_sensitive( FALSE );

		menuitemCompile->set_sensitive( FALSE );
		menuitemCompileWithAdditionalFlags->set_sensitive( FALSE );
		toolbuttonCompile->set_sensitive( FALSE );
		menuitemRunExternalGUI->set_sensitive( FALSE );
		toolbuttonCompileExternalGUI->set_sensitive( FALSE );
		//toolbuttonUpdateCabbageInstrument->set_sensitive( FALSE );

/*		menuitemCabbage->set_sensitive( TRUE );
		menuitemCabbageUpdate->set_sensitive( FALSE );
		menuitemCabbageExportVSTI->set_sensitive( FALSE );
		menuitemCabbageExportVST->set_sensitive( FALSE );
		menuitemCabbageExportAU->set_sensitive( FALSE );
		menuitemCabbageLookForInternetUpdates->set_sensitive( TRUE );*/

		menuitemFolding->set_sensitive( FALSE );
		menuitemFoldSingle->set_sensitive( FALSE );
		menuitemFoldAll->set_sensitive( FALSE );
		menuitemUnFoldAll->set_sensitive( FALSE );
		

		//MenuToolsRunCSoundAV->set_sensitive( FALSE );
		//toolbuttonCSoundAV->set_sensitive( FALSE );
		//MenuToolsRunPythonIdle->set_sensitive( FALSE );
		//menuitemUseWinXoundFlags->set_sensitive( FALSE );


		menuitemFind->set_sensitive( FALSE );
		menuitemComments->set_sensitive( FALSE );
		menuitemBookmarks->set_sensitive( FALSE );
		menuitemLineEndings->set_sensitive( FALSE );


		return;
	}

	if (ActiveEditor() == NULL) return;

	//this.Text = wxGlobal.TITLE + " - " + ActiveEditor.FileName;
	Glib::ustring temp = TITLE;
	temp.append(" - ");
	temp.append(ActiveEditor()->FileName);
	wxMainWindow->set_title(temp);

	CheckUndoRedo();

	menuitemSave->set_sensitive(ActiveEditor()->textEditor->isTextChanged());
	toolbuttonSave->set_sensitive(ActiveEditor()->textEditor->isTextChanged());

	//////////////////////////////
	//menuitemSave->set_sensitive( TRUE );
	//toolbuttonSave->set_sensitive( TRUE );
	menuitemSaveAs->set_sensitive( TRUE );
	toolbuttonSaveAs->set_sensitive( TRUE );
	menuitemSaveAll->set_sensitive( TRUE );
	menuitemClose->set_sensitive( TRUE );
	toolbuttonCloseDocument->set_sensitive( TRUE );
	menuitemCloseAll->set_sensitive( TRUE );

	/*
	menuitemImportOrcScoToNewCSD->set_sensitive( TRUE);
	menuitemImportOrcSco->set_sensitive( TRUE);
	menuitemImportOrc->set_sensitive( TRUE);
	menuitemImportSco->set_sensitive( TRUE);
	menuitemExportOrcSco->set_sensitive( TRUE );
	menuitemExportOrc->set_sensitive( TRUE );
	menuitemExportSco->set_sensitive( TRUE );
	*/
	CheckImportExportMenu();

	//menuitemPageSetup->set_sensitive( TRUE );
	menuitemPrint->set_sensitive( TRUE );
	menuitemPrintPreview->set_sensitive( TRUE );
	menuitemFileInfo->set_sensitive( TRUE );

	menuitemCut->set_sensitive( TRUE );
	menuitemCopy->set_sensitive( TRUE );
	menuitemPaste->set_sensitive( TRUE );
	menuitemDelete->set_sensitive( TRUE );
	menuitemSelectAll->set_sensitive( TRUE );

	menuitemFindLineNumber->set_sensitive( TRUE );
	toolbuttonFindLine->set_sensitive( TRUE );

	menuitemFindAndReplace->set_sensitive( TRUE );
	toolbuttonFind->set_sensitive( TRUE );
	menuitemFindNext->set_sensitive( TRUE );
	menuitemFindPrevious->set_sensitive( TRUE );
	menuitemJumpToCaret->set_sensitive( TRUE );

	menuitemShowHideWhiteSpaces->set_sensitive( TRUE );
	menuitemShowHideEOLS->set_sensitive( TRUE );
	//menuitemEOLConversion->set_sensitive( TRUE );
	menuitemLineEndingsCRLF->set_sensitive( TRUE );
	menuitemLineEndingsCR->set_sensitive( TRUE );
	menuitemLineEndingsLF->set_sensitive( TRUE );

	//menuitemCodeFormatting->set_sensitive( TRUE );
	menuitemFormatCode->set_sensitive( TRUE );
	menuitemFormatCodeAll->set_sensitive( TRUE );
	menuitemFormatCodeOptions->set_sensitive( TRUE );

	menuitemCodeRepositoryStoreSelectedText->set_sensitive( TRUE );

	//menuitemComments->set_sensitive( TRUE );
	menuitemCommentLine->set_sensitive( TRUE );
	menuitemRemoveCommentLine->set_sensitive( TRUE );

	menuitemListOpcodes->set_sensitive( TRUE );

	//menuitemBookmarks->set_sensitive( TRUE );
	menuitemInsertRemoveBookmark->set_sensitive( TRUE );
	menuitemRemoveAllBookmarks->set_sensitive( TRUE );
	menuitemGoToNextBookmark->set_sensitive( TRUE );
	menuitemGoToPreviousBookmark->set_sensitive( TRUE );

	menuitemNavigateBackward->set_sensitive( TRUE );
	menuitemNavigateForward->set_sensitive( TRUE );

	menuitemFullCode->set_sensitive( TRUE );
	menuitemSplitHorizontal->set_sensitive( TRUE );
	menuitemSplitHorizontalOrcSco->set_sensitive( TRUE );
	menuitemSplitVertical->set_sensitive( TRUE );
	menuitemSplitVerticalOrcSco->set_sensitive( TRUE );
	//tsCode->set_sensitive( TRUE );

	menuitemSetCaretOnPrimaryView->set_sensitive( TRUE );
	menuitemSetCaretOnSecondaryView->set_sensitive( TRUE );

	//MenuFontSize->set_sensitive( TRUE );
	menuitemResetTextZoom->set_sensitive( TRUE );
	toolbuttonResetZoom->set_sensitive( TRUE );
	toolbuttonZoomOut->set_sensitive( TRUE );
	toolbuttonZoomIn->set_sensitive( TRUE );
	//toolbuttonButtonFontSize->set_sensitive( TRUE );

	menuitemCompile->set_sensitive( TRUE );
	menuitemCompileWithAdditionalFlags->set_sensitive( TRUE );
	toolbuttonCompile->set_sensitive( TRUE );
	menuitemRunExternalGUI->set_sensitive( TRUE );
	toolbuttonCompileExternalGUI->set_sensitive( TRUE );
	//toolbuttonUpdateCabbageInstrument->set_sensitive( TRUE );

	/*menuitemCabbage->set_sensitive( TRUE );
	menuitemCabbageUpdate->set_sensitive( TRUE );
	menuitemCabbageExportVSTI->set_sensitive( TRUE );
	menuitemCabbageExportVST->set_sensitive( TRUE );
	menuitemCabbageExportAU->set_sensitive( TRUE );
	menuitemCabbageLookForInternetUpdates->set_sensitive( TRUE );*/

	menuitemFolding->set_sensitive( TRUE );
	menuitemFoldSingle->set_sensitive( TRUE );
	menuitemFoldAll->set_sensitive( TRUE );
	menuitemUnFoldAll->set_sensitive( TRUE );
	
	//MenuToolsRunCSoundAV->set_sensitive( TRUE );
	//toolbuttonCSoundAV->set_sensitive( TRUE );
	//MenuToolsRunPythonIdle->set_sensitive( TRUE );
	//menuitemUseWinXoundFlags->set_sensitive( TRUE );	

	menuitemFind->set_sensitive( TRUE );
	menuitemComments->set_sensitive( TRUE );
	menuitemBookmarks->set_sensitive( TRUE );
	menuitemLineEndings->set_sensitive( TRUE );

	
}

void wxMain::CheckImportExportMenu()
{
	if (ActiveEditor() == NULL)
	{
		//Import
		menuitemImportOrcScoToNewCSD->set_sensitive( TRUE);
		menuitemImportOrcSco->set_sensitive( FALSE);
		menuitemImportOrc->set_sensitive( FALSE);
		menuitemImportSco->set_sensitive( FALSE);
		//Export
		menuitemExport->set_sensitive( FALSE );
		menuitemExportOrcSco->set_sensitive( FALSE );
		menuitemExportOrc->set_sensitive( FALSE );
		menuitemExportSco->set_sensitive( FALSE );
		return;
	}

	//if (ActiveEditor.FileName.ToLower().EndsWith(".csd"))
	if(Glib::str_has_suffix(ActiveEditor()->FileName.lowercase(), ".csd"))
	{
		//Import
		menuitemImportOrcScoToNewCSD->set_sensitive( TRUE);
		menuitemImportOrcSco->set_sensitive( TRUE);
		menuitemImportOrc->set_sensitive( TRUE);
		menuitemImportSco->set_sensitive( TRUE);
		//Export
		menuitemExport->set_sensitive( TRUE );
		menuitemExportOrcSco->set_sensitive( TRUE );
		menuitemExportOrc->set_sensitive( TRUE );
		menuitemExportSco->set_sensitive( TRUE );
	}
	else
	{
		//Import
		menuitemImportOrcScoToNewCSD->set_sensitive( TRUE);
		menuitemImportOrcSco->set_sensitive( FALSE);
		menuitemImportOrc->set_sensitive( FALSE);
		menuitemImportSco->set_sensitive( FALSE);
		//Export
		menuitemExport->set_sensitive( FALSE );
		menuitemExportOrcSco->set_sensitive( FALSE );
		menuitemExportOrc->set_sensitive( FALSE );
		menuitemExportSco->set_sensitive( FALSE );
	}
}


void wxMain::ShowUaeMessageOnSave(const gchar* message)
{
	//UnauthorizedAccessException //ACCESS_DENIED
	wxGLOBAL->ShowMessageBox(this,
	                         "File Error: ACCESS DENIED.\n\n" 
	                         "If you want to save changes to this file please click on 'Save As'\n"
	                         "and therefore select a folder where you have full read and write\n"
	                         "permissions.\n\n"
	                         "WinXound cannot save the file to this directory!",
	                         "WinXound - File Saving error - Access denied",
	                         Gtk::BUTTONS_OK);
}

void wxMain::ShowUaeMessageOnLoad(const gchar* message)
{
	if(wxSETTINGS->General.ShowReadOnlyFileMessage == false) return;
	
	wxGLOBAL->ShowMessageBox(this,
	                         "Your current user account allows read-only access to this file.\n"
	                         "If you want to modify this file please save it " 
	                         "to a folder where you have full read and write permissions.\n\n"
	                         "WinXound will try to load the file in Read-Only mode.\n\n"
	                         "To disable this warning message uncheck \n"
	                         "'Show Read-Only File Alert'\n"
	                         "in your WinXound Settings (menu File->Settings->General tab).",
	                         "WinXound - ReadOnly File Access",
	                         Gtk::BUTTONS_OK);
													

}

void wxMain::on_HelpBrowser_csound_file_clicked(const gchar* file)
{
	//wxGLOBAL->DebugPrint("BROWSER CSD", g_filename_from_uri(file, NULL, NULL));
	if(strlen(file) > 0)
		AddNewEditor(g_filename_from_uri(file, NULL, NULL));
}




void wxMain::on_menuitemLineEndings_drop_down()
{
	if (ActiveEditor() == NULL) return;

	//wxGLOBAL->DebugPrint("WXMAIN", "menuitemLineEndings");
	
	//Check Line Endings
	//menuitemLineEndingsCRLF->set_active(FALSE);
	//menuitemLineEndingsCR->set_active(FALSE);
	//menuitemLineEndingsLF->set_active(FALSE);

	menuitemLineEndingsCRLF->set_label("Convert to _CRLF (Windows)");
	menuitemLineEndingsCR->set_label("Convert to _CR (Mac)");
	menuitemLineEndingsLF->set_label("Convert to _LF (Unix/OsX)");
	
	switch (ActiveEditor()->textEditor->getEOLMode())
	{
		case SC_EOL_CRLF:
			//MenuEolCRLF.Checked = true;
			//MenuEolCRLF.Text = "&CRLF (Windows)";
			//menuitemLineEndingsCRLF->set_active(TRUE);
			menuitemLineEndingsCRLF->set_label("* _CRLF (Windows)");
			break;

		case SC_EOL_CR:
			//MenuEolCR.Checked = true;
			//MenuEolCR.Text = "C&R (Mac)";
			//menuitemLineEndingsCR->set_active(TRUE);
			menuitemLineEndingsCR->set_label("* C_R (Mac)");
			break;

		case SC_EOL_LF:
			//MenuEolLF.Checked = true;
			//MenuEolLF.Text = "&LF (Unix/OsX)";
			//menuitemLineEndingsLF->set_active(TRUE);
			menuitemLineEndingsLF->set_label("* _LF (Unix/OsX)");
			break;
	}

}


void wxMain::CreateWinXoundIcon()
{

	Glib::ustring iconfile = Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath());
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
		wxMainWindow->set_icon_from_file(iconfile);

	/*
	Glib::RefPtr<Gdk::Pixbuf> icon_pix_buf = 
		Gdk::Pixbuf::create_from_file(
			Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath()), 48, 48, TRUE);
	if (icon_pix_buf) 
	{
		//gtk_window_set_icon(GTK_WINDOW(PWidget(wSciTE)), icon_pix_buf);
		wxMainWindow->set_icon(icon_pix_buf);
		//gdk_pixbuf_unref(icon_pix_buf);
		//icon_pix_buf->unreference(); //NOT NEEDED
		return;
	}
	*/
	
}

void wxMain::CreatePopupMenu()
{
	/*
	Gtk::Menu		PopupMenu;
	Gtk::MenuItem   PopupGoToDefinition;
	Gtk::MenuItem   PopupGoToReference;
	Gtk::MenuItem   PopupOpcodeHelp;
	Gtk::MenuItem   PopupOpenFile;
	Gtk::MenuItem   PopupComment;
	Gtk::MenuItem   PopupBookmarks;
	Gtk::MenuItem   PopupInsertFileAsText;
	*/

	
	//Fill popup menu:
	Gtk::Menu::MenuList& menulist = PopupMenu.items();

	//CUT, COPY, PASTE
	menulist.push_back(Gtk::Menu_Helpers::MenuElem("Cut",
		sigc::mem_fun(*this, &wxMain::on_menuitemCut_Clicked)));
	menulist.push_back(Gtk::Menu_Helpers::MenuElem("Copy",
		sigc::mem_fun(*this, &wxMain::on_menuitemCopy_Clicked)));
	menulist.push_back(Gtk::Menu_Helpers::MenuElem("Paste",
		sigc::mem_fun(*this, &wxMain::on_menuitemPaste_Clicked)));

	//SEPARATOR
	menulist.push_back(Gtk::Menu_Helpers::SeparatorElem());

	//COMMENT
	PopupComment.set_label("Comment");
	Gtk::Menu::MenuList& menulistcomment = PopupComment_submenu.items();
	menulistcomment.push_back(Gtk::Menu_Helpers::MenuElem("Apply",
		sigc::mem_fun(*this, &wxMain::on_menuitemCommentLine_Clicked)));
	menulistcomment.push_back(Gtk::Menu_Helpers::MenuElem("Remove",
		sigc::mem_fun(*this, &wxMain::on_menuitemRemoveCommentLine_Clicked)));
	PopupComment.set_submenu(PopupComment_submenu);
	menulist.push_back(PopupComment);

	//SEPARATOR
	menulist.push_back(Gtk::Menu_Helpers::SeparatorElem());

	//BOOKMARKS
	PopupBookmarks.set_label("Bookmarks");
	Gtk::Menu::MenuList& menulistbookmarks = PopupBookmarks_submenu.items();
	menulistbookmarks.push_back(Gtk::Menu_Helpers::MenuElem("Insert/Remove",
		sigc::mem_fun(*this, &wxMain::on_menuitemInsertRemoveBookmark_Clicked)));
	menulistbookmarks.push_back(Gtk::Menu_Helpers::SeparatorElem());
	menulistbookmarks.push_back(Gtk::Menu_Helpers::MenuElem("Remove All",
		sigc::mem_fun(*this, &wxMain::on_menuitemRemoveAllBookmarks_Clicked)));
	menulistbookmarks.push_back(Gtk::Menu_Helpers::MenuElem("Go to Next",
		sigc::mem_fun(*this, &wxMain::on_menuitemGoToNextBookmark_Clicked)));
	menulistbookmarks.push_back(Gtk::Menu_Helpers::MenuElem("Go to Previous",
		sigc::mem_fun(*this, &wxMain::on_menuitemGoToPreviousBookmark_Clicked)));
	PopupBookmarks.set_submenu(PopupBookmarks_submenu);
	menulist.push_back(PopupBookmarks);
	
	//SEPARATOR
	menulist.push_back(Gtk::Menu_Helpers::SeparatorElem());

	
	//Attach the callback functions to the activate signal
	//m_item_close.signal_activate().connect( sigc::mem_fun(*this, &ExampleWindow::on_menu_close) );

	//GO TO DEFINITION
	PopupGoToDefinition.set_label("Go to definition of ...");
	menulist.push_back(PopupGoToDefinition);
	PopupGoToDefinition.signal_activate().connect(
		sigc::mem_fun(*this, &wxMain::on_contextmenu_GoToDefinition_Clicked));

	//GO TO REFERENCE
	PopupGoToReference.set_label("Go to reference of ...");
	menulist.push_back(PopupGoToReference);
	PopupGoToReference.signal_activate().connect(
		sigc::mem_fun(*this, &wxMain::on_contextmenu_GoToReference_Clicked));

	//SEPARATOR
	menulist.push_back(Gtk::Menu_Helpers::SeparatorElem());
	
	//OPEN FILE
	PopupOpenFile.set_label("Open File");
	menulist.push_back(PopupOpenFile);
	PopupOpenFile.signal_activate().connect(
		sigc::mem_fun(*this, &wxMain::on_contextmenu_file_Clicked));

	//SEPARATOR
	menulist.push_back(Gtk::Menu_Helpers::SeparatorElem());

	//INSERT FILENAME AS TEXT
	PopupInsertFileAsText.set_label("Insert Filename as text");
	menulist.push_back(PopupInsertFileAsText);
	PopupInsertFileAsText.signal_activate().connect(
		sigc::mem_fun(*this, &wxMain::on_contextmenu_file_as_text_Clicked));
	
	//SEPARATOR
	menulist.push_back(Gtk::Menu_Helpers::SeparatorElem());

	//OPCODE HELP
	PopupOpcodeHelp.set_label("Opcode Help");
	menulist.push_back(PopupOpcodeHelp);
	PopupOpcodeHelp.signal_activate().connect(
		sigc::mem_fun(*this, &wxMain::on_menuitemOpcodeHelp_Clicked));

	PopupMenu.show_all();

}


//DRAG_DROP 
void wxMain::on_DragDataReceived(const Glib::RefPtr<Gdk::DragContext>& context, 
                                 int x, int y, 
                                 const Gtk::SelectionData& selection_data, 
                                 guint info, guint time)
{
	//wxGLOBAL->DebugPrint("WXMAIN", "DragDataReceived");


	

	const int length = selection_data.get_length();
	if(length >= 0 && 
	   selection_data.get_format() == 8)
	{
		//std::cout << "Received: " << selection_data.get_data_as_string() << std::endl;

		std::vector<Glib::ustring> file_list;
		file_list = selection_data.get_uris();

		if (file_list.size() > 0)
		{
			Glib::ustring path = ""; //= Glib::filename_from_uri(file_list[0]);

			for(uint i = 0; i < file_list.size(); i++)
			{
				path = "";
				
				if(file_list[i].size() > 0)
				{
					path = Glib::filename_from_uri(file_list[i]);

					if(Glib::file_test(path, Glib::FILE_TEST_EXISTS))
					{
						AddNewEditor(path.c_str());
					}
				}
			}
		}
	}

	context->drop_reply(true, time);
	context->drag_finish(false, false, time);

	
	
	//this->grab_focus();
	wxMainWindow->grab_focus();
	//if(ActiveEditor() != NULL)
	//	ActiveEditor()->SetFocus();
	//context.dest_window->grab_focus();

}

void wxMain::on_contextmenu_GoToDefinition_Clicked()
{
	if (ActiveEditor() == NULL) return;
	
	Glib::ustring curWord = 
			ActiveEditor()->textEditor->getWordAt(ActiveEditor()->textEditor->getCaretPosition());
	if(curWord == "") return;
	

	Glib::ustring Definition = wxGLOBAL->Trim(curWord);

	try
	{
		//if (Definition.EndsWith("."))
		if(Glib::str_has_suffix(Definition, "."))
		{
			//Definition = Definition.TrimEnd('.');
			gint size = Definition.size();
			Definition.resize(size-1);
		}

		//if (!ActiveEditor.UserOpcodes.Contains(Definition))
		if (g_hash_table_lookup(Opcodes, curWord.c_str()) == NULL)
		{
			////MACRO SEARCH 
			//if (Definition.StartsWith("g") ||
			//    Definition.StartsWith("$"))
			if(Glib::str_has_prefix(Definition, "g") ||
			   Glib::str_has_prefix(Definition, "$"))
			{
				//if (Definition.StartsWith("$"))
				if(Glib::str_has_prefix(Definition, "$"))
				{
					//Definition = Definition.TrimStart('$');
					Definition.erase(0,1);
				}
				//Global Type: search all text from start
				gint mPos =
					ActiveEditor()->textEditor->FindText(
					                                 Definition.c_str(), true, true,
					                                 false, true, false, true,
					                                 0, ActiveEditor()->textEditor->getTextLength());
				//ActiveEditor.StoreCursorPos(mPos);
				if(mPos > -1) ActiveEditor()->StoreCursorPos(mPos);
				return;

			}

			////LOCAL_TYPE SEARCH: search inside "instr" -> "endin" 
			else
			{
				gint posINSTR = -1;
				gint curPos = ActiveEditor()->textEditor->getCaretPosition();
				gint mFindPos = -1;

				//Search INSTR 
				mFindPos = ActiveEditor()->textEditor->FindText(
				                                            "instr", true, true, true,
				                                            false, false, true);
				if (mFindPos > -1)
				{
					posINSTR = mFindPos;
				}
				else return;

				mFindPos =
					ActiveEditor()->textEditor->FindText(
					                                 Definition.c_str(), true, true,
					                                 false, true, false, true,
					                                 posINSTR, curPos - 1); //Definition.Length);
				//ActiveEditor.StoreCursorPos(mFindPos);
				if(mFindPos > -1) ActiveEditor()->StoreCursorPos(mFindPos);
			}
		}

		/*TODO: ALSO IN OSX
		////USER OPCODE SEARCH
		//else if (ActiveEditor.UserOpcodes.Contains(Definition))
		if (g_hash_table_lookup(Opcodes, curWord.c_str()) != NULL)
		{
			ArrayList mc =
				ActiveEditor.textEditor.SearchText(
				                                   Definition, true, true, false, true);
			foreach (Int32 mPos in mc)
			{
				string mLine = ActiveEditor.textEditor.GetTextLine(
				                                                   ActiveEditor.textEditor.GetLineNumberFromPosition(
				                                                                                                     mPos));
				Int32 mDefinitionPos = mLine.IndexOf(Definition);
				Int32 mOpcodePos = mLine.IndexOf("opcode");
				if (mOpcodePos > -1 &&
				    mOpcodePos < mDefinitionPos)
				{
					ActiveEditor.textEditor.SetCaretPosition(mPos);
					ActiveEditor.textEditor.SetSelectionEnd(mPos + Definition.Length);
					//ActiveEditor.StoreCursorPos(mPos);
					break;
				}
			}
		}
		*/
	}

	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain on_contextmenu_GoToDefinition_Clicked", "ERROR");
	}
}


void wxMain::on_contextmenu_GoToReference_Clicked()
{
	if (ActiveEditor() == NULL) return;
	
	Glib::ustring curWord = 
			ActiveEditor()->textEditor->getWordAt(ActiveEditor()->textEditor->getCaretPosition());
	if(curWord == "") return;
	
	Glib::ustring Definition = wxGLOBAL->Trim(curWord);

	
	try
	{
		//if (!ActiveEditor.UserOpcodes.Contains(Definition))
		if (g_hash_table_lookup(Opcodes, curWord.c_str()) == NULL)
		{
			////MACRO TYPE: search all text from current position
			gint line = ActiveEditor()->textEditor->getCurrentLineNumber();
			Glib::ustring textOfLine = ActiveEditor()->textEditor->getTextOfLine(line);
			
			if (textOfLine.find("#define") != Glib::ustring::npos)
			{
				//Definition = "$" + Definition;
				Definition.insert(0,"$");

				gint mPos =
					ActiveEditor()->textEditor->FindText(
					                                 Definition.c_str(), true, true, false,
					                                 true, false, true,
					                                 ActiveEditor()->textEditor->getCaretPosition() + 1,
					                                 ActiveEditor()->textEditor->getTextLength());
				if(mPos > -1) ActiveEditor()->StoreCursorPos(mPos);
				return;
			}

			////GLOBAL TYPE: search all text from current position
			//else if (Definition.StartsWith("g") ||
			//         Definition.StartsWith("$"))
			else if(Glib::str_has_prefix(Definition, "g") ||
			        Glib::str_has_prefix(Definition, "$"))
			{

				gint mPos =
					ActiveEditor()->textEditor->FindText(
					                                 Definition.c_str(), true, true, false,
					                                 true, false, true,
					                                 ActiveEditor()->textEditor->getCaretPosition() + 1,
					                                 ActiveEditor()->textEditor->getTextLength());

				//ActiveEditor.StoreCursorPos(mPos);
				if(mPos > -1) ActiveEditor()->StoreCursorPos(mPos);
				return;

			}

			////LOCAL TYPE: search inside "instr" -> "endin" 
			else
			{

				gint posENDIN = -1;
				gint curPos = ActiveEditor()->textEditor->getCaretPosition();
				gint mFindPos = -1;

				//Search ENDIN 
				mFindPos = ActiveEditor()->textEditor->FindText(
				                                            "endin", true, true,
				                                            false, false, false, true);
				if (mFindPos > -1)
				{
					posENDIN = mFindPos;
				}
				else return;

				//Search reference
				mFindPos = ActiveEditor()->textEditor->FindText(
				                                            Definition.c_str(), true, true,
				                                            false, true, false, true,
				                                            curPos + 1, posENDIN);
				//ActiveEditor.StoreCursorPos(mFindPos);
				if(mFindPos > -1) ActiveEditor()->StoreCursorPos(mFindPos);
			}
		}

		/*
		//TODO: ALSO ON OSX
		//USER OPCODES SEARCH
		else if (ActiveEditor.UserOpcodes.Contains(Definition))
		{
			Int32 mFindPos = ActiveEditor.textEditor.FindText(
			                                                  Definition, true, true,
			                                                  false, true, false, true);
			//ActiveEditor.StoreCursorPos(mFindPos);
		}
		*/

	}

	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain on_contextmenu_GoToReference_Clicked", "ERROR");
	}
}









////////////////////////////////////////////////////////////////////////////////////////
//BOOKMARKS
////////////////////////////////////////////////////////////////////////////////////////
void wxMain::LoadBookmarks(wxEditor* editor)
{
	//Load Bookmarks ----------------------------------------------
	//if (!wxGlobal.isSyntaxType(editor)) return;

	//Load and Save bookmarks only on syntax styled documents 
	//(we skip others documents: txt, h, inc, udo)

	Glib::ustring languagecomment = "";

	if(Glib::str_has_suffix(editor->FileName.lowercase(), ".csd"))
		languagecomment = ";";
	else if(Glib::str_has_suffix(editor->FileName.lowercase(), ".py") ||
		    Glib::str_has_suffix(editor->FileName.lowercase(), ".pyw"))
		languagecomment = "#";
	else if(Glib::str_has_suffix(editor->FileName.lowercase(), ".lua"))
		languagecomment = "--";
	else return;


	try
	{
		//languagecomment + "[winxound_bookmarks"
		Glib::ustring textToFind = languagecomment;
		textToFind.append("[winxound_bookmarks");
		
		gint ret = editor->textEditor->FindText(textToFind.c_str(),
		                                        true,
		                                        true,
		                                        false,
		                                        false,
		                                        false,
		                                        false,
		                                        0,
		                                        -1);

		if (ret > -1)
		{
			gint eolLength = editor->textEditor->newLine().size();
			gint line = editor->textEditor->getLineNumberFromPosition(ret);

			Glib::ustring lineText = editor->textEditor->getTextOfLine(line);

			editor->textEditor->ReplaceTarget(ret,
			                                  editor->textEditor->getLineLength(line),
			                                  "");

			//lineText = lineText.Trim("]".ToCharArray());
			gint l = lineText.size();
			lineText.resize(l - 1);

			/*
			string[] splittedNumbers = lineText.Split(",".ToCharArray());

			for (Int32 index = 1; index < splittedNumbers.Length; index++)
			{
				if (!string.IsNullOrEmpty(splittedNumbers[index]))
					editor.textEditor.InsertBookmarkAt(Int32.Parse(splittedNumbers[index]));
			}
			*/

			//g_strsplit: returns :a newly-allocated NULL-terminated array of strings. 
			//Use g_strfreev() to free it. 
			gchar** splittedNumbers = g_strsplit(lineText.c_str(), ",", 0);
			int length = wxGLOBAL->ArrayLength(splittedNumbers);
			for(int index = 1; index < length; index ++)
			{
				//if (!string.IsNullOrEmpty(splittedNumbers[index]))
				if(strlen(splittedNumbers[index]) > 0)
				{
					editor->textEditor->InsertBookmarkAt(atoi(splittedNumbers[index]));
				}
			}
			g_strfreev(splittedNumbers);

			editor->BookmarksOnLoad = true;
		}

		editor->RefreshListBoxBookmarks();

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain LoadBookmarks", "ERROR");
	}
}


//Save bookmarks position
void wxMain::SaveBookmarks(wxEditor* editor)
{
	//if([[wxDefaults valueForKey:@"SaveBookmarks"] boolValue] &&
	//   [textEditor hasBookmarks])
	if (editor->FileIsReadOnly) return;


	try
	{
		//if (wxGlobal.Settings.EditorProperties.SaveBookmarks ||
		//    editor.BookmarksOnLoad == true)
		{
			Glib::ustring languagecomment = "";

			if(Glib::str_has_suffix(editor->FileName.lowercase(), ".csd"))
				languagecomment = ";";
			else if(Glib::str_has_suffix(editor->FileName.lowercase(), ".py") ||
			        Glib::str_has_suffix(editor->FileName.lowercase(), ".pyw"))
				languagecomment = "#";
			else if(Glib::str_has_suffix(editor->FileName.lowercase(), ".lua"))
				languagecomment = "--";
			else return;




			//if (wxGlobal.Settings.EditorProperties.SaveBookmarks &&
			//    editor.textEditor.HasBookmarks())
			if(wxSETTINGS->EditorProperties.SaveBookmarks &&
			   editor->textEditor->hasBookmarks())
			{
				//Append remmed bookmarks to the end of text
				//[textEditor AppendText:[NSString stringWithFormat:@"\n%@[winxound_bookmarks", languagecomment]];
				//if (editor.textEditor.GetText().EndsWith("\n") ||
				//    editor.textEditor.GetText().EndsWith("\r"))
				if(Glib::str_has_suffix(editor->textEditor->getText(), "\n") ||
				   Glib::str_has_suffix(editor->textEditor->getText(), "\r"))
				{
					Glib::ustring temp = languagecomment;
					temp.append("[winxound_bookmarks");
					editor->textEditor->AppendText(temp.c_str());
				}
				else
				{
					Glib::ustring temp = editor->textEditor->newLine();
					temp.append(languagecomment);
					temp.append("[winxound_bookmarks");
					//editor.textEditor.AppendText(editor.textEditor.NewLine + languagecomment + "[winxound_bookmarks");
					editor->textEditor->AppendText(temp.c_str());
					
				}

				gint mCurLine = 0;
				gint mBookLine = 0;
				do
				{
					//mBookLine = [textEditor MarkerNext:mCurLine markerMask:1];
					mBookLine = editor->textEditor->MarkerNext(mCurLine, 1);
					if (mBookLine == -1) break;

					//[textEditor AppendText:[NSString stringWithFormat:@",%d", mBookLine]];
					editor->textEditor->AppendText(
						Glib::ustring::compose(",%1",mBookLine).c_str());
					mCurLine = mBookLine + 1;
				}
				while (true);

				editor->textEditor->AppendText("]");
				editor->textEditor->SaveFile(editor->FileName.c_str());
			}
			else if (editor->BookmarksOnLoad == true)
			{
				editor->textEditor->SaveFile(editor->FileName.c_str());
			}

		}

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain SaveBookmarks", "ERROR");
	}

}
////////////////////////////////////////////////////////////////////////////////////////
//BOOKMARKS
////////////////////////////////////////////////////////////////////////////////////////




bool wxMain::FileIsUntitled(wxEditor* editor)
{
	return (editor->FileName == UNTITLED_CSD ||
	        editor->FileName == UNTITLED_PY ||
	        editor->FileName == UNTITLED_LUA);
}




void wxMain::OpenLastSessionFiles()
{
	wxGLOBAL->DebugPrint("OpenLastSessionFiles");
	
	for (uint i=0; i < wxSETTINGS->LastSessionFiles.size(); i++ )
	{
		Glib::ustring filename = wxSETTINGS->LastSessionFiles[i];
		if(Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
			AddNewEditor(filename.c_str());
	}
}




void wxMain::FirstStart()
{
	
	wxSETTINGS->General.FirstStart = false;

	wxGLOBAL->ShowMessageBox(this,
	                         "It seems that this is the first time that you launch WinXound.\n"
	                         "WinXound will now try to find CSound, Python and Lua compilers.\n\n"
	                         "If WinXound fails, please open the Settings window ('File->Settings' menu), "
	                         "click on 'Directories' tab and fill all required fields.\n\n"
	                         "You can later modify these values using the 'File->Settings' menu.",
	                         "WinXound - First Start Informations",
	                         Gtk::BUTTONS_OK);

	//Show Directories Tab
	//wxSETTINGS->OpenDirectoriesTab();

	
	Glib::ustring entryCSoundExecutable,
				  entryCSoundHelp,
				  entryCSoundExternalGUI,
				  entryPythonCompiler,
				  entryPythonExternalGUI,
				  entryLuaCompiler,
				  entryLuaExternalGUI,
				  entryWavePlayer,
				  entryWaveEditor,
				  entryCalculator;


	//Search binaries and assign strings
	wxSETTINGS->LookForBinaries(entryCSoundExecutable,
	                            entryCSoundHelp,
	                            entryCSoundExternalGUI,
	                            entryPythonCompiler,
	                            entryPythonExternalGUI,
	                            entryLuaCompiler,
	                            entryLuaExternalGUI,
	                            entryWavePlayer,
	                            entryWaveEditor,
	                            entryCalculator);


	
	//Assign settings global variables
	wxSETTINGS->Directory.CSoundConsole = entryCSoundExecutable;
	wxSETTINGS->Directory.CSoundHelpHTML = entryCSoundHelp;
	wxSETTINGS->Directory.Winsound = entryCSoundExternalGUI;
	wxSETTINGS->Directory.PythonConsolePath = entryPythonCompiler;
	wxSETTINGS->Directory.PythonIdlePath = entryPythonExternalGUI;
	wxSETTINGS->Directory.LuaConsolePath = entryLuaCompiler;
	wxSETTINGS->Directory.LuaGuiPath = entryLuaExternalGUI;
	wxSETTINGS->Directory.WavePlayer = entryWavePlayer;
	wxSETTINGS->Directory.WaveEditor = entryWaveEditor;
	wxSETTINGS->Directory.Calculator = entryCalculator;

	
	//Finally save settings
	wxSETTINGS->SaveSettings();

}



void wxMain::on_compiler_completed(Glib::ustring errorline, Glib::ustring wavefile)
{

	
	//toolbuttonCompile->set_sensitive(TRUE);
	//menuitemCompile->set_sensitive(TRUE);
	
	//wxGLOBAL->DebugPrint("wxMain::Compiler completed");
	if(mCompilerEditor == NULL || ActiveEditor() == NULL) return;

	try
	{
		if(errorline != "")
		{
			gint line = atoi(errorline.c_str());
			line--;
			if(line < 0) line = 0;

			mCompilerEditor->textEditor->GoToLine(line);
			mCompilerEditor->textEditor->SelectLine(line);
			mCompilerEditor->SetFocus();
		}
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxMain::on_compiler_completed::errorline Error");
	}


	//wxGLOBAL->DebugPrint(wavefile.c_str());

	if (mCompilerEditor != NULL)
 		mCompilerEditor->SetFocus();
	else if (ActiveEditor() != NULL)
		ActiveEditor()->SetFocus();
	
	
	if (wavefile != "" &&
	    wxSETTINGS->General.OpenSoundFileWith > 0)
	{
		std::cout << "Wavefile: " << wavefile << std::endl;
		
		//if (File.Exists(mWaveFile))
		if(Glib::file_test(wavefile, Glib::FILE_TEST_EXISTS))
		{
			//wxGlobal.LastWaveFile = mWaveFile;
			try
			{
				Glib::ustring filename = "\"";
				filename.append(wavefile);
				filename.append("\"");
				
				if (wxSETTINGS->General.OpenSoundFileWith == 1)
				{
					//System.Diagnostics.Process.Start("wmplayer", "\"" + mWaveFile + "\"");
					
					this->executeCommand(NULL, 
					                     wxSETTINGS->Directory.WavePlayer.c_str(), 
					                     filename.c_str());
				}
				else
				{
					//if (File.Exists(wxGlobal.Settings.Directory.WaveEditor))
					if(Glib::file_test(wxSETTINGS->Directory.WaveEditor, Glib::FILE_TEST_EXISTS))
					{
						//System.Diagnostics.Process.Start(wxGlobal.Settings.Directory.WaveEditor,
						//                                 "\"" + mWaveFile + "\"");
						this->executeCommand(NULL, 
						                     wxSETTINGS->Directory.WaveEditor.c_str(), 
						                     filename.c_str());
					}
					else
					{
						wxGLOBAL->ShowMessageBox(this,
						                         "Wave Editor Path not found!\n"
						                         "Please select a valid path in File->Settings->Directories->Wave Editor field.",
						                         "WinXound error!",
						                         Gtk::BUTTONS_OK);
					}
				}
			}
			catch (...)
			{
				wxGLOBAL->DebugPrint("wxMain::on_compiler_completed::wavefile Error");
			}
		}
	}

	
}








////////////////////////////////////////////////////////////////////////////////
// CABBAGE TOOLS
////////////////////////////////////////////////////////////////////////////////
void wxMain::on_menuitemCabbageUpdate_Clicked()
{
/*	if(ActiveEditor() == NULL) return;
	if(CheckForCabbageTags() == false) return;

	//Clear compiler text
	compiler->ClearCompilerText();
	
	Glib::ustring filename = SaveBeforeCompile();

	if(cabbageUtilities != NULL)
		cabbageUtilities->UpdateCabbageInstrument(filename);*/

}
void wxMain::on_menuitemCabbageExportVSTI_Clicked()
{
/*	if(ActiveEditor() == NULL) return;
	if(CheckForCabbageTags() == false) return;

	Glib::ustring filename = SaveBeforeCompile();
	
	if(cabbageUtilities != NULL)
		cabbageUtilities->ExportToVSTI(filename);*/
}
void wxMain::on_menuitemCabbageExportVST_Clicked()
{
/*	if(ActiveEditor() == NULL) return;
	if(CheckForCabbageTags() == false) return;
	
	Glib::ustring filename = SaveBeforeCompile();
	
	if(cabbageUtilities != NULL)
		cabbageUtilities->ExportToVST(filename);*/
}
void wxMain::on_menuitemCabbageExportAU_Clicked()
{
/*	if(ActiveEditor() == NULL) return;
	if(CheckForCabbageTags() == false) return;

	Glib::ustring filename = SaveBeforeCompile();
	
	if(cabbageUtilities != NULL)
		cabbageUtilities->ExportToAU(filename);*/
}

void wxMain::on_menuitemCabbageLookForInternetUpdates_Clicked()
{
/*	if(cabbageUtilities != NULL)
		cabbageUtilities->CheckForCabbageUpdatesOnInternet();*/
}

void wxMain::on_menuitemGoToCabbageWebsite_Clicked()
{
/*	//LINK: http://code.google.com/p/cabbage/
	try
	{	
		gtk_show_uri (NULL,
		              "http://code.google.com/p/cabbage/",
		              gtk_get_current_event_time(),
		              NULL);
	}

	catch(...)
	{
		wxGLOBAL->DebugPrint("wxMain - on_menuitemGoToCabbageWebsite_Clicked Error");
	}*/
}

void wxMain::on_cabbage_message_received(Glib::ustring message)
{
/*	//std::cout << "Cabbage message: " << message << std::endl;
	
	if(ActiveEditor() == NULL) return;

	if(Glib::str_has_prefix(message, "CABBAGE_FILE_UPDATED|"))
	{
		Glib::ustring filename = message.substr(22, message.size() - 22);
		//std::cout << "CABBAGE_FILE_UPDATED:" << filename << std::endl;
		
		if(Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
		{
			wxEditor* tempEditor = NULL;
			
			//Check if the loaded file is already open in another tab
			if(notebookCode->get_n_pages() > 0)
			{
				for(int i = 0; i < notebookCode->get_n_pages(); i++)
				{
					tempEditor = (wxEditor*)notebookCode->get_nth_page(i);

					if (tempEditor->FileName == filename)
					{
						notebookCode->set_current_page(i);

						tempEditor->textEditor->LoadFile(filename.c_str());
						tempEditor->textEditor->setSavePoint();

						//BRING TO FRONT (if specified in the settings)
						if(wxSETTINGS->General.BringWinXoundToFrontForCabbage)
							wxMainWindow->present();

					}
				}
			}
		}
		
	}
	else if(Glib::str_has_prefix(message, "CABBAGE_DEBUG|"))
	{
		Glib::ustring output = message.substr(15, message.size() - 15);
		//std::cout << "Cabbage debug received:" << output << std::endl;

		//output.insert(0, "Cabbage: ");
		//output.append("\r\n"); //TODO: Probably to remove with the new Cabbage
		//output.insert(0, "\r\n");
		//std::replace( output.begin(), output.end(), '\n', '\r\n' );
		Glib::RefPtr<Glib::Regex> my_regex = Glib::Regex::create("\n");
		Glib::ustring s = my_regex->replace(output, 
											0, 
											"\r\n", 
											Glib::REGEX_MATCH_NOTBOL);
		
		compiler->AppendCompilerText(s);
		
	}
	else if(Glib::str_has_prefix(message, "CABBAGE_CLEARTEXT"))
	{
		//std::cout << "Cabbage ClearText received" << std::endl;
		compiler->ClearCompilerText();
	}
	else if(Glib::str_has_prefix(message, "PLUGIN_UPDATE|"))
	{
		Glib::ustring filename = message.substr(15, message.size() - 15);
		std::cout << "CABBAGE_PLUGIN_UPDATE:" << filename << std::endl;
		
		if(Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
		{
			wxEditor* tempEditor = NULL;
			
			//Check if the loaded file is already open in another tab
			if(notebookCode->get_n_pages() > 0)
			{
				for(int i = 0; i < notebookCode->get_n_pages(); i++)
				{
					tempEditor = (wxEditor*)notebookCode->get_nth_page(i);

					if (tempEditor->FileName == filename)
					{
						notebookCode->set_current_page(i);

						tempEditor->textEditor->LoadFile(filename.c_str());
						tempEditor->textEditor->setSavePoint();

						//BRING TO FRONT (if specified in the settings)
						if(wxSETTINGS->General.BringWinXoundToFrontForCabbage)
							wxMainWindow->present();

						//FOUNDED SO WE HAVE TO RETURN
						return;

					}
				}
			}

			AddNewEditor(filename);
			//BRING TO FRONT (if specified in the settings)
			if(wxSETTINGS->General.BringWinXoundToFrontForCabbage)
				wxMainWindow->present();
				return;

		}
	}
*/	
}

bool wxMain::CheckForCabbageTags()
{
	if(ActiveEditor() == NULL) return false;

/*	//Find <CsOptions> tags
	gint findStart = ActiveEditor()->textEditor->FindText("<Cabbage>", true, true, false,
	                                                      false, false, true,
	                                                      0, ActiveEditor()->textEditor->getTextLength());

	gint findEnd = ActiveEditor()->textEditor->FindText("</Cabbage>", true, true, false,
	                                                    false, false, true,
	                                                    0, ActiveEditor()->textEditor->getTextLength());


	if (findStart == -1 && findEnd == -1) //CABBAGE TAGS NOT FOUND
	{
		wxGLOBAL->ShowMessageBox("Cabbage Tags not found!\n"
		                         "Please insert <Cabbage> and </Cabbage> tags in your code.",
		                         "WinXound - Cabbage info",
		                         Gtk::BUTTONS_OK);
		return false;
	}*/

	return true;

}




