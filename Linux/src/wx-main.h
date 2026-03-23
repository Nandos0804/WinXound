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
#include "gtkmm_compat.h"

class wxAbout;
class wxEditor;
class wxSettings;
class wxTerminal;
class wxBrowser;
class wxFindAndReplace;
class wxFindLine;
class wxAnalysis;
class wxCSoundRepository;
class wxRepository;
class wxCodeFormatter;
//class wxPipe;
class wxCabbage;
class wxCabbageRepository;


#ifndef _WX_MAIN_H_
#define _WX_MAIN_H_

/*
#ifndef PLAT_GTK
#define PLAT_GTK 1
#endif

#ifndef GTK
#define GTK
#endif

#ifndef __cplusplus
#define __cplusplus
#endif
 */

class wxMain: public Gtk::Window 
{
public:
	wxMain(int argc, char *argv[]);
	virtual ~wxMain();
	Gtk::Window& get_window() const { return *wxMainWindow; }

	void AddNewEditor(Glib::ustring filename);
	void AddNewEditor(Glib::ustring filename, Glib::ustring linkFile);
	void AddNewEditor(Glib::ustring filename, Glib::ustring linkFile, bool importOperation);

	bool RemoveEditor(wxEditor* editor);
	bool RemoveAllEditors();
	void SetHighlightLanguage(wxEditor* editor, bool refresh);
	void Compile();
	void Compile(Glib::ustring additionalParams);
	bool executeCommand(const gchar* path, const gchar* cmd, const gchar* args);
	//Scintilla notifications
	void onDocumentUpdateUi(wxEditor* editor);
	void onZoomNotification(wxEditor* editor);
	void onMouseDown(wxEditor* editor, GdkEventButton* event);
	
	bool on_Tab1_Clicked(GdkEventButton* event);
	bool on_Tab2_Clicked(GdkEventButton* event);
	bool on_Tab3_Clicked(GdkEventButton* event);
	bool on_Tab4_Clicked(GdkEventButton* event);
	bool on_Tab5_Clicked(GdkEventButton* event);
	bool on_Tab6_Clicked(GdkEventButton* event);
	void on_Tabs_Clicked(gint currentIndex);
	

	bool on_labels_drag_motion(const Glib::RefPtr<Gdk::DragContext>& context, int x, int y, guint time);
	void on_notebookCompiler_drag_end(const Glib::RefPtr<Gdk::DragContext>& context);
	void on_notebookCompiler_drag_leave(const Glib::RefPtr<Gdk::DragContext>& context, guint time);
	bool isDragAndDrop;

	
	//bool on_configure_event (GdkEventConfigure *e);
	//wxAbout*		AboutWindow;
	void onSciModified(wxEditor* editor);
	void onSciModContainer(wxEditor* editor);
	void onUriDropped(wxEditor* editor, Glib::ustring filename, gint view);
	void CheckUndoRedo();
	void CheckMenuConditions();
	void ShowUaeMessageOnSave(const gchar* message);
	void ShowUaeMessageOnLoad(const gchar* message);
	void ShowEolAlertMessage(const gchar* eol, const gchar* report);
	void CheckLineEndings(wxEditor* editor);
	void on_HelpBrowser_csound_file_clicked(const gchar* file);
	bool on_wxMainWindow_delete_event(GdkEventAny* event);
	bool FindNext(const gchar* text, 
	              bool MatchWholeWord,
	              bool MatchCase);
	bool FindPrevious(const gchar* text, 
	                  bool MatchWholeWord,
	                  bool MatchCase);
	void Replace(const gchar* StringToFind,
	             const gchar* ReplaceString,
	             bool MatchWholeWord,
	             bool MatchCase,
	             bool FromCaretPosition,
	             bool FCPUp, bool ReplaceAll);
	void GoToLineNumber(gint linenumber);
	void OpenHyperLinks(const gchar* path);
	
	void on_contextmenu_file_Clicked();
	void on_contextmenu_file_as_text_Clicked();

	void on_contextmenu_GoToDefinition_Clicked();
	void on_contextmenu_GoToReference_Clicked();

	void LoadBookmarks(wxEditor* editor);
	void SaveBookmarks(wxEditor* editor);
	bool FileIsUntitled(wxEditor* editor);
	void OpenLastSessionFiles();

	void on_compiler_completed(Glib::ustring errorline, Glib::ustring wavefile);
	void onEditorFocus(bool isEntered);
	void onEditorOrcScoSwitchRequest();
	void onEditorOrcScoShowList();

	bool CheckForCabbageTags();

	void FirstStart();

	void LinkOrcScoFiles(wxEditor* editor);
	std::vector<std::string> LookForOrcScoFiles(wxEditor* editor);

	//Glib::ustring	gCurWord;
	
protected:

	void onSavePointLeft();
	void onSavePointReached();

	void on_DragDataReceived(const Glib::RefPtr<Gdk::DragContext>& context, 
	                         int x, int y, 
	                         const Gtk::SelectionData& selection_data, 
	                         guint info, guint time);

	void on_settings_save_Clicked();

	void onSignalAutocompletionSynopsis(wxEditor* editor,
	                                    GdkEventKey* event,
	                                    const gchar* opcode);

private:
	Gtk::Notebook*  notebookCode;
	Gtk::Notebook*  notebookCompiler;
	Gtk::Window*	wxMainWindow;
	Gtk::Window*	compilerWindow;
	Gtk::VPaned*	vpanedMain;
	Gtk::Toolbar*   toolbar;
	Gtk::Statusbar* statusbarInfo;
	Gtk::Label*		compilerLabel;
	
	Gtk::Menu		PopupMenu;
	Gtk::MenuItem   PopupGoToDefinition;
	Gtk::MenuItem   PopupGoToReference;
	Gtk::MenuItem   PopupOpcodeHelp;
	Gtk::MenuItem   PopupOpenFile;
	Gtk::MenuItem   PopupComment;
	Gtk::Menu		PopupComment_submenu;
	Gtk::MenuItem   PopupBookmarks;
	Gtk::Menu		PopupBookmarks_submenu;
	Gtk::MenuItem	PopupInsertFileAsText;

	//std::string		LastUsedPath;

	//gint			OriginX;
	//gint			OriginY;
	gint			oldLine;
	gint			oldSelectedTab;
	gint			oldHeight;


	bool			mWorkingDir;
	bool			mWinXoundIsReadOnly;

	//wxSettings*			settings;
	GHashTable*				Opcodes;
	wxTerminal*				compiler;
	wxTerminal*				terminal;
	wxBrowser*				HelpBrowser;
	wxAbout*				AboutWindow;
	wxFindAndReplace*		FindAndReplace;
	wxFindLine*				FindLine;
	wxAnalysis*				Analysis;
	wxEditor*				mCompilerEditor;
	wxCSoundRepository*		mCSoundRepository;
	wxRepository*			mRepository;
	wxCodeFormatter*		mFormatter;
	//wxPipe*				namedPipe;
	wxCabbage*				cabbageUtilities;
	wxCabbageRepository*	mCabbageRepository;

	//FormFindAndReplace mFormFindAndReplace = new FormFindAndReplace();
	//FormFindLine mFindLine = new FormFindLine();


	wxEditor* ActiveEditor();
	void setMenuSignals(Glib::RefPtr<Gtk::Builder> builder);
	bool on_main_quit_signal();
	void on_notebookEditor_switch_page(Gtk::Widget* page, guint page_num);
	void on_notebookCompiler_switch_page(Gtk::Widget* page, guint page_num);
	void on_notebookCompiler_resize(Gdk::Rectangle& rect);
	//bool on_wxMainWindow_configure_event(GdkEventConfigure* event);
	void CreateNotebookCompilerTabs();
	void ShowOrcSco();
	void AddRecentFilesToMenu(const gchar* filename);
	void ClearStatubarInfo();
	void CreateWinXoundIcon();
	Glib::ustring SaveBeforeCompile();
	Glib::ustring GetCSoundFlags();
	Glib::ustring CheckForAdditionalFlags(Glib::ustring additionalParams);
	bool CheckDisplaysFlag();
	void CreatePopupMenu();
	bool on_newdialog_keypress(GdkEventKey* event);
	void CheckForBracket(wxEditor* tEditor);
	//void on_settings_save_Clicked();
	bool TimerMod_Tick();
	bool on_key_press_event(GdkEventKey* event);

	void on_cabbage_message_received(Glib::ustring message);

	void UpdateAdditionalFlagsMenu();
	void on_popup_menu_position(int& x, int& y, bool& push_in);

	void CheckImportExportMenu();

	
	//Menu signals
	void on_menuitemNew_Clicked();
	void on_menuitemOpen_Clicked();
	void on_menuitemSave_Clicked();
	void on_menuitemSaveAs_Clicked();
	void on_menuitemSaveAll_Clicked();
	void on_menuitemClose_Clicked();
	void on_menuitemCloseAll_Clicked();
	void on_menuitemFileInfo_Clicked();
	void on_menuitemImportOrcScoToNewCSD_Clicked();
	void on_menuitemImportOrcSco_Clicked();
	void on_menuitemImportOrc_Clicked();
	void on_menuitemImportSco_Clicked();
	void on_menuitemExportOrcSco_Clicked();
	void on_menuitemExportOrc_Clicked();
	void on_menuitemExportSco_Clicked();
	void on_menuitemPrint_Clicked();
	void on_menuitemPrintPreview_Clicked();
	void on_menuitemSettings_Clicked();
	void on_menuRecentFiles_Clicked(Glib::ustring file); /////
	void on_menuitemExit_Clicked();
	
	void on_menuitemUndo_Clicked();
	void on_menuitemRedo_Clicked();
	void on_menuitemCut_Clicked();
	void on_menuitemCopy_Clicked();
	void on_menuitemPaste_Clicked();
	void on_menuitemDelete_Clicked();
	void on_menuitemSelectAll_Clicked();
	void on_menuitemFindAndReplace_Clicked();
	void on_menuitemFindNext_Clicked();
	void on_menuitemFindPrevious_Clicked();
	void on_menuitemJumpToCaret_Clicked();
	void on_menuitemFindLineNumber_Clicked();
	void on_menuitemCommentLine_Clicked();
	void on_menuitemRemoveCommentLine_Clicked();
	void on_menuitemInsertRemoveBookmark_Clicked();
	void on_menuitemRemoveAllBookmarks_Clicked();
	void on_menuitemGoToNextBookmark_Clicked();
	void on_menuitemGoToPreviousBookmark_Clicked();
	void on_menuitemFormatCode_Clicked();
	void on_menuitemFormatCodeAll_Clicked();
	void FormatCode(bool allLines);
	void on_menuitemFormatCodeOptions_Clicked();
	void on_menuitemCodeRepositoryShowWindow_Clicked();
	void on_menuitemCodeRepositoryStoreSelectedText_Clicked();
	void on_menuitemListOpcodes_Clicked();
	void on_menuitemCSoundOpcodesRepository_Clicked();
	void on_menuitemLineEndings_drop_down();
	void on_menuitemLineEndingsCRLF_Clicked();
	void on_menuitemLineEndingsCR_Clicked();
	void on_menuitemLineEndingsLF_Clicked();

	//Folding
	void on_menuitemFoldSingle_Clicked();
	void on_menuitemFoldAll_Clicked();
	void on_menuitemUnFoldAll_Clicked();

	void on_menuitemLineNumbers_Clicked();
	void on_menuitemExplorer_Clicked();
	void on_menuitemOnlineHelp_Clicked();
	void on_menuitemToolbar_Clicked();
	void on_menuitemShowAllTools_Clicked();
	void on_menuitemHideAllTools_Clicked();
	void on_menuitemFullCode_Clicked();
	void on_menuitemSplitHorizontal_Clicked();
	void on_menuitemSplitHorizontalOrcSco_Clicked();
	void on_menuitemSplitVertical_Clicked();
	void on_menuitemSplitVerticalOrcSco_Clicked();
	void on_menuitemShowCode_Clicked();
	void on_menuitemShowCompiler_Clicked();
	void on_menuitemShowHelp_Clicked();
	void on_menuitemShowHideWhiteSpaces_Clicked();
	void on_menuitemShowHideEOLS_Clicked();
	void on_menuitemNavigateBackward_Clicked();
	void on_menuitemNavigateForward_Clicked();
	void on_menuitemSetCaretOnPrimaryView_Clicked();
	void on_menuitemSetCaretOnSecondaryView_Clicked();
	void on_menuitemScreenPositionUP_Clicked();
	void on_menuitemScreenPositionDOWN_Clicked();
	void on_menuitemScreenPositionLEFT_Clicked();
	void on_menuitemScreenPositionRIGHT_Clicked();
	void on_menuitemScreenPositionRESET_Clicked();

	void on_menuitemCompile_Clicked();
	void on_menuitemCompileWithAdditionalFlags_Clicked(Glib::ustring value);
	void on_menuitemCompileExternal_Clicked();
	void on_menuitemUseWinXoundFlags_Clicked();
	void on_menuitemRunExternalGUI_Clicked();
	void on_menuitemAnalysis_Clicked();
	void on_menuitemMediaPlayer_Clicked();
	void on_menuitemExternalWaveEditor_Clicked();
	void on_menuitemCalculator_Clicked();
	void on_menuitemTerminal_Clicked();
	void on_menuitemWinXoundTest_Clicked();

	void on_menuitemWinXoundHelp_Clicked();
	void on_menuitemOpcodeHelp_Clicked();
	void on_menuitemOpcodesIndexHelp_Clicked();
	void on_menuitemCSoundHelp_Clicked();
	void on_menuitemCSoundTutorial_Clicked();
	void on_menuitemFlossManual_Clicked();
	void on_menuitemUdoDatabase_Clicked();
	void on_menuitemCabbageManual_Clicked();
	void on_menuitemCabbageManualInternet_Clicked();
	void on_menuitemAbout_Clicked();

	void on_toolbuttonCSoundOpcodes_Clicked();
	void on_toolbuttonCSoundFlags_Clicked();
	void on_toolbuttonZoomOut_Clicked();
	void on_toolbuttonZoomIn_Clicked();
	void on_toolbuttonResetZoom_Clicked();
	void on_toolbuttonCompilerWindow_Clicked();
	//void on_toolbuttonUpdateCabbageInstrument_Clicked();
	void on_compilerWindow_closing();
	void on_compilerWindow_closed();
	bool on_compilerWindow_key_press_event(GdkEventKey* event);
	//void on_compilerWindow_check_resize();
	//bool on_compilerWindow_configure_event(GdkEventConfigure *e);
	bool on_compilerWindow_expose_event(GdkEventExpose* e);
	
	//CABBAGE:
	void on_menuitemCabbageUpdate_Clicked();
	void on_menuitemCabbageExportVSTI_Clicked();
	void on_menuitemCabbageExportVST_Clicked();
	void on_menuitemCabbageExportAU_Clicked();
	void on_menuitemCabbageLookForInternetUpdates_Clicked();
	void on_menuitemGoToCabbageWebsite_Clicked();
		
	//Menu items
	Gtk::CheckMenuItem*  menuitemLineNumbers;
	Gtk::CheckMenuItem*  menuitemExplorer;
	Gtk::CheckMenuItem*  menuitemOnlineHelp;
	Gtk::CheckMenuItem*  menuitemToolbar;
	Gtk::CheckMenuItem*  menuitemUseWinXoundFlags;

	//TOOLBAR
	Gtk::ToolButton* toolbuttonNew;
	Gtk::ToolButton* toolbuttonOpen;
	Gtk::ToolButton* toolbuttonSave;
	Gtk::ToolButton* toolbuttonSaveAs;
	Gtk::ToolButton* toolbuttonCloseDocument;
	Gtk::ToolButton* toolbuttonUndo;
	Gtk::ToolButton* toolbuttonRedo;
	Gtk::ToolButton* toolbuttonResetZoom;
	Gtk::ToolButton* toolbuttonZoomOut;
	Gtk::ToolButton* toolbuttonZoomIn;
	Gtk::ToolButton* toolbuttonFind;
	Gtk::ToolButton* toolbuttonFindLine;
	//Gtk::ToolButton* toolbuttonCompile;
	Gtk::MenuToolButton* toolbuttonCompile;
	Gtk::ToolButton* toolbuttonCompileExternalGUI;
	//Gtk::ToolButton* toolbuttonUpdateCabbageInstrument;
	//Gtk::MenuToolButton* toolbuttonCompileWithAdditionalFlags;
	Gtk::ToolButton* toolbuttonCSoundUtilities;
	Gtk::ToolButton* toolbuttonCSoundHelp;
	Gtk::ToolButton* toolbuttonCSoundOpcodes;
	Gtk::ToolButton* toolbuttonCSoundFlags;
	Gtk::ToolButton* toolbuttonCompilerWindow;

	//MENU FILE
	Gtk::MenuItem* menuitemNew;
	Gtk::MenuItem* menuitemOpen;
	Gtk::MenuItem* menuitemSave;
	Gtk::MenuItem* menuitemSaveAs;
	Gtk::MenuItem* menuitemSaveAll;
	Gtk::MenuItem* menuitemClose;
	Gtk::MenuItem* menuitemCloseAll;
	Gtk::MenuItem* menuitemFileInfo;
	Gtk::MenuItem* menuitemImportOrcScoToNewCSD;
	Gtk::MenuItem* menuitemImportOrcSco;
	Gtk::MenuItem* menuitemImportOrc;
	Gtk::MenuItem* menuitemImportSco;
	Gtk::MenuItem* menuitemExport;
	Gtk::MenuItem* menuitemExportOrcSco;
	Gtk::MenuItem* menuitemExportOrc;
	Gtk::MenuItem* menuitemExportSco;
	Gtk::MenuItem* menuitemPrint;
	Gtk::MenuItem* menuitemPrintPreview;
	Gtk::MenuItem* menuitemSettings;
	Gtk::MenuItem* menuitemRecentFiles;
	Gtk::Menu*	   RecentFilesSubMenu;
	Gtk::MenuItem* menuitemExit;

	//MENU EDIT
	Gtk::MenuItem* menuitemUndo;
	Gtk::MenuItem* menuitemRedo;
	Gtk::MenuItem* menuitemCut;
	Gtk::MenuItem* menuitemCopy;
	Gtk::MenuItem* menuitemPaste;
	Gtk::MenuItem* menuitemDelete;
	Gtk::MenuItem* menuitemSelectAll;
	Gtk::MenuItem* menuitemFind;
	Gtk::MenuItem* menuitemFindAndReplace;
	Gtk::MenuItem* menuitemFindNext;
	Gtk::MenuItem* menuitemFindPrevious;
	Gtk::MenuItem* menuitemJumpToCaret;
	Gtk::MenuItem* menuitemFindLineNumber;
	Gtk::MenuItem* menuitemComments;
	Gtk::MenuItem* menuitemCommentLine;
	Gtk::MenuItem* menuitemRemoveCommentLine;
	Gtk::MenuItem* menuitemBookmarks;
	Gtk::MenuItem* menuitemInsertRemoveBookmark;
	Gtk::MenuItem* menuitemRemoveAllBookmarks;
	Gtk::MenuItem* menuitemGoToNextBookmark;
	Gtk::MenuItem* menuitemGoToPreviousBookmark;
	Gtk::MenuItem* menuitemFormatCode;
	Gtk::MenuItem* menuitemFormatCodeAll;
	Gtk::MenuItem* menuitemFormatCodeOptions;
	Gtk::MenuItem* menuitemCodeRepositoryShowWindow;
	Gtk::MenuItem* menuitemCodeRepositoryStoreSelectedText;
	Gtk::MenuItem* menuitemListOpcodes;
	Gtk::MenuItem* menuitemCSoundOpcodesRepository;
	Gtk::MenuItem* menuitemFolding;
	Gtk::MenuItem* menuitemFoldSingle;
	Gtk::MenuItem* menuitemFoldAll;
	Gtk::MenuItem* menuitemUnFoldAll;
	Gtk::MenuItem* menuitemLineEndings;
	Gtk::MenuItem* menuitemLineEndingsCRLF;
	Gtk::MenuItem* menuitemLineEndingsCR;
	Gtk::MenuItem* menuitemLineEndingsLF;
	Gtk::MenuItem* menuitemResetTextZoom;

	//MENU VIEW
	Gtk::MenuItem* menuitemShowAllTools;
	Gtk::MenuItem* menuitemHideAllTools;
	Gtk::MenuItem* menuitemFullCode;
	Gtk::MenuItem* menuitemSplitHorizontal;
	Gtk::MenuItem* menuitemSplitHorizontalOrcSco;
	Gtk::MenuItem* menuitemSplitVertical;
	Gtk::MenuItem* menuitemSplitVerticalOrcSco;
	Gtk::MenuItem* menuitemShowCode;
	Gtk::MenuItem* menuitemShowCompiler;
	Gtk::MenuItem* menuitemShowHelp;
	Gtk::MenuItem* menuitemShowHideWhiteSpaces;
	Gtk::MenuItem* menuitemShowHideEOLS;
	Gtk::MenuItem* menuitemNavigateBackward;
	Gtk::MenuItem* menuitemNavigateForward;
	Gtk::MenuItem* menuitemSetCaretOnPrimaryView;
	Gtk::MenuItem* menuitemSetCaretOnSecondaryView;
	Gtk::MenuItem* menuitemScreenPositionUP;
	Gtk::MenuItem* menuitemScreenPositionDOWN;
	Gtk::MenuItem* menuitemScreenPositionLEFT;
	Gtk::MenuItem* menuitemScreenPositionRIGHT;
	Gtk::MenuItem* menuitemScreenPositionRESET;
	Gtk::MenuItem* menuitemCompilerWindow;

	//MENU TOOLS
	Gtk::MenuItem* menuitemCompile;
	Gtk::MenuItem* menuitemCompileWithAdditionalFlags;
	Gtk::Menu*	   AdditionalFlagsSubMenu;
	Gtk::Menu*	   AdditionalFlagsSubMenu2;
	Gtk::MenuItem* menuitemRunExternalGUI;
	Gtk::MenuItem* menuitemCabbage;
	Gtk::MenuItem* menuitemCabbageUpdate;
	Gtk::MenuItem* menuitemCabbageExportVSTI;
	Gtk::MenuItem* menuitemCabbageExportVST;
	Gtk::MenuItem* menuitemCabbageExportAU;
	Gtk::MenuItem* menuitemCabbageLookForInternetUpdates;
	Gtk::MenuItem* menuitemGoToCabbageWebsite;
	Gtk::MenuItem* menuitemAnalysis;
	Gtk::MenuItem* menuitemMediaPlayer;
	Gtk::MenuItem* menuitemExternalWaveEditor;
	Gtk::MenuItem* menuitemCalculator;
	Gtk::MenuItem* menuitemWinXoundTest;

	//MENU HELP
	Gtk::MenuItem* menuitemWinXoundHelp;
	Gtk::MenuItem* menuitemOpcodeHelp;
	Gtk::MenuItem* menuitemOpcodesIndexHelp;
	Gtk::MenuItem* menuitemCSoundHelp;
	Gtk::MenuItem* menuitemCSoundFlagsHelp;
	Gtk::MenuItem* menuitemCSoundTutorial;
	Gtk::MenuItem* menuitemFlossManual;
	Gtk::MenuItem* menuitemUdoDatabase;
	Gtk::MenuItem* menuitemCabbageManual;
	Gtk::MenuItem* menuitemCabbageManualInternet;
	Gtk::MenuItem* menuitemAbout;

	
};

#endif // _WX_MAIN_H_
