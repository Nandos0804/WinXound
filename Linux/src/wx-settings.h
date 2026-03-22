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

//Class Usage:
//wxSettings::getInstance()->setCurrentMode();

#include <gtkmm.h>

#ifndef _WX_SETTINGS_H_
#define _WX_SETTINGS_H_

#define wxSETTINGS wxSettings::getInstance()



class wxSettings
{


public:
    static wxSettings* getInstance( void);

	virtual ~wxSettings();
	
	/// <summary>
	/// GENERAL STRUCTURE
	/// </summary>
	struct mGeneral
	{
		//General
		bool ShowUtilitiesMessage;
		bool ShowImportOrcScoMessage;
		bool ShowReadOnlyFileMessage;
		bool BringWinXoundToFrontForCabbage;
		//public FormWindowState WindowState;
		Gdk::Point WindowSize;
		Gdk::Point WindowPosition;
		Gdk::Point CompilerWindowSize;
        Gdk::Point CompilerWindowPosition;
		bool FirstStart;
		bool FindWholeWord;
		bool FindMatchCase;
		bool ReplaceFromCaret;
		bool ShowToolbar;
		//public List<bool> ToolBarItems;
		Glib::ustring CompilerFontName;
		gint DefaultWavePlayer;
		gint CompilerFontSize;
		gint StartupAction;
		gint OrcScoImport;
		bool UseWinXoundFlags;
		bool CheckForCabbageUpdates;
		Glib::ustring CSoundAdditionalFlags;
		//CSound
		Glib::ustring CSoundDefaultFlags;
		gint OpenSoundFileWith;
		//Python
		Glib::ustring PythonDefaultFlags;
		//Lua
		Glib::ustring LuaDefaultFlags;
	} General;

	/// <summary>
	/// DIRECTORY STRUCTURE
	/// </summary>
	struct mDirectory
	{
		//General
		Glib::ustring WavePlayer;
		Glib::ustring WaveEditor;
		Glib::ustring Calculator;
		Glib::ustring WorkingDir;
		Glib::ustring LastUsedPath;
		//CSound
		Glib::ustring CSoundConsole;
		Glib::ustring Winsound;
		Glib::ustring CSoundAV;
		Glib::ustring CSoundAVHelp;
		Glib::ustring CSoundHelpHTML;
		Glib::ustring SFDIR;
		Glib::ustring SSDIR;
		Glib::ustring SADIR;
		Glib::ustring MFDIR;
		Glib::ustring INCDIR;
		Glib::ustring OPCODEDIR;
		bool UseSFDIR;
		//Python
		Glib::ustring PythonConsolePath;
		Glib::ustring PythonIdlePath;
		//Lua
		Glib::ustring LuaConsolePath;
		Glib::ustring LuaGuiPath;
		//Cabbage
		Glib::ustring CabbagePath;
	} Directory;

	/// <summary>
	/// EDITOR PROPERTIES STRUCTURE
	/// </summary>
	struct mEditorProperties
	{
		//General
		Glib::ustring DefaultFontName;
		gint DefaultFontSize;
		gint DefaultTabSize;
		bool ShowVerticalRuler;
		bool ShowMatchingBracket;
		bool ShowLineNumbers;
		bool ShowFoldLine;
		bool ShowIntelliTip;
		bool ShowExplorer;
		bool MarkCaretLine;
		bool SaveBookmarks;
		bool ExplorerShowOptions;
		bool ExplorerShowInstrMacros;
		bool ExplorerShowInstrOpcodes;
		bool ExplorerShowInstrInstruments;
		bool ExplorerShowScoreFunctions;
		bool ExplorerShowScoreMacros;
		bool ExplorerShowScoreSections;
		gint ExplorerFontSize;

		//Language specific style values
		//0=StyleNumber // 1=ForeColor // 2=BackColor // 3=Bold
		//4=Italic // 5=Alpha // 6=EolFilled // 7=FriendlyName
		GList* CSoundStyles;
		GList* PythonStyles;
		bool UseMixedPython;
		GList* LuaStyles;
	} EditorProperties;


	/// <summary>
	/// CODEFORMAT STRUCTURE
	/// </summary>
	struct mCodeFormat
	{
		//CSound
		bool FormatHeader;
		bool FormatInstruments;
		bool FormatFunctions;
		bool FormatScoreInstruments;
		bool FormatTempo;
		gint InstrumentsType;
		gint TabIndent;
	} CodeFormat;

	struct mTemplates
	{
		Glib::ustring CSound;
		Glib::ustring Python;
		Glib::ustring Lua;
		Glib::ustring Cabbage;
	} Templates;








	
	
    //Place Public (non-static) class methods here
	//GList* RecentFiles;
	//GQueue* RecentFiles;
	std::vector<std::string> RecentFiles;
	std::vector<std::string> LastSessionFiles;
	bool LoadSettings(bool readOnly);
	bool LoadSettings(Glib::ustring filename, bool readOnly);
	bool SaveSettings();
	bool SaveSettings(Glib::ustring filename);
	void CreateDefaultWinXoundSettings();
	void CreateDefaultTemplates();
	void CreateDefaultCSoundStyles();
	void CreateDefaultPythonStyles();
	void CreateDefaultLuaStyles();
	GList* SelectLanguage(Glib::ustring language);
	Glib::ustring GetValueAt(Glib::ustring s, gint i);
	Glib::ustring StyleGetFriendlyName(Glib::ustring language, gint stylenumber);
	Glib::ustring StyleGetForeColor(Glib::ustring language, gint stylenumber);
	Glib::ustring StyleGetBackColor(Glib::ustring language, gint stylenumber);
	bool StyleGetBold(Glib::ustring language, gint stylenumber);
	bool StyleGetItalic(Glib::ustring language, gint stylenumber);
	gint StyleGetAlpha(Glib::ustring language, gint stylenumber);
	bool StyleGetEolFilled(Glib::ustring language, gint stylenumber);
	gint StyleGetStyleNumber(Glib::ustring text);
	void RecentFilesRemoveExcess();
	void RecentFilesInsert(const gchar* mFileName);
	void LastSessionFilesClear();
	void LastSessionFilesInsert(const gchar* mFileName);
	void showWindowAt(gint x, gint y);
	void closeWindow();
	void associateWidgets(Glib::RefPtr<Gtk::Builder> builder);
	void FreeSyntaxLists();
	void FreeSyntaxLists(Glib::ustring language);
	void SetEnvironmentVariables();
	void RefreshSyntaxColors(Glib::ustring language);
	void OpenDirectoriesTab();
	void LookForBinaries(Glib::ustring& entryCSoundExecutable,
	                     Glib::ustring& entryCSoundHelp,
	                     Glib::ustring& entryCSoundExternalGUI,
	                     Glib::ustring& entryPythonCompiler,
	                     Glib::ustring& entryPythonExternalGUI,
	                     Glib::ustring& entryLuaCompiler,
	                     Glib::ustring& entryLuaExternalGUI,
	                     Glib::ustring& entryWavePlayer,
	                     Glib::ustring& entryWaveEditor,
	                     Glib::ustring& entryCalculator);
	
	void TEST();

	Gtk::Window*		settingsWindow;



	//SIGNALS
	//signal_settings_save
	typedef sigc::signal<void>		type_signal_settings_save;
	type_signal_settings_save		signal_settings_save(){return m_signal_settings_save;};

	
protected:
	void SettingsSaveNotification();
	type_signal_settings_save		m_signal_settings_save;

	void CreateSyntaxPanelItems(GList* styleList);
	void RetrieveSyntaxPanelItems(GList** styleList);

	Gtk::TreeView* treeCSoundColors;
	Gtk::TreeView* treePythonColors;
	Gtk::TreeView* treeLuaColors;
	void PickColor(gint index, Glib::RefPtr<Gdk::Pixbuf> pixbuf, Glib::ustring language);
	bool on_syntax_colors_Clicked(GdkEventButton* event);

	void on_buttonDefaultSyntaxCSound_Clicked();
	void on_buttonDefaultSyntaxPython_Clicked();
	void on_buttonDefaultSyntaxLua_Clicked();
	
	Glib::ustring getDefaultAlternativeCSoundFlags();

	//Tree model columns:
	class syntaxColumns : public Gtk::TreeModel::ColumnRecord
	{
	public:
		Gtk::TreeModelColumn<Glib::ustring> FriendlyName;
			
		//Gtk::TreeModelColumn<Glib::ustring> ForeColor;
		//Gtk::TreeModelColumn<Glib::ustring> BackColor;
		Gtk::TreeModelColumn< Glib::RefPtr < Gdk::Pixbuf > > ForeColor;
		Gtk::TreeModelColumn< Glib::RefPtr < Gdk::Pixbuf > > BackColor;

		Gtk::TreeModelColumn<bool> Bold;
		Gtk::TreeModelColumn<bool> Italic;
		Gtk::TreeModelColumn<int> Alpha;

		Gtk::TreeModelColumn<int> StyleNumber;
			
		
		syntaxColumns()
		{
			add(FriendlyName); 
			add(ForeColor);
			add(BackColor);
			add(Bold);
			add(Italic);
			add(Alpha);
			add(StyleNumber);
		}
			
	};	
	syntaxColumns	synColumns;
	//Glib::RefPtr<Gtk::TreeStore> synRefTreeModel;



	
	
private:
    static wxSettings*  instance;
    wxSettings(void);

	Glib::ustring GetField(Glib::ustring s);
	Glib::ustring GetValue(Glib::ustring s);

	const gchar* d;
	GList* tempList;


	//WINDOW WIDGETS
	Gtk::RadioButton*   radiobuttonStartupActionNothing;
	Gtk::RadioButton*   radiobuttonStartupActionCSound;
	Gtk::RadioButton*   radiobuttonStartupActionPython;
	Gtk::RadioButton*   radiobuttonStartupActionLua;
	Gtk::RadioButton*   radiobuttonStartupActionLastSession;

	Gtk::RadioButton*   radiobuttonOSIAskAlways;
	Gtk::RadioButton*   radiobuttonOSIConvertToCsd;
	Gtk::RadioButton*   radiobuttonOSIOpenSeparately;

	Gtk::RadioButton*   radiobuttonWavePlayerInternal;
	Gtk::RadioButton*   radiobuttonExternalMediaPlayer;

	Gtk::CheckButton*   checkbuttonCabbageFileUpdated;
	Gtk::CheckButton*   checkbuttonCheckForCabbageUpdates;
	
	Gtk::CheckButton*   checkbuttonShowReadOnlyAlert;
	Gtk::TextView*		textviewTemplatesCSound;
	Gtk::TextView*		textviewTemplatesPython;
	Gtk::TextView*		textviewTemplatesLua;
	Gtk::TextView*		textviewTemplatesCabbage;
	Gtk::TextView*		textviewAlternativeCSoundFlags;
	Gtk::Button*		buttonDefaultAlternativeCSoundFlags;
	
	Gtk::Button*		buttonDefaultTemplates;
	Gtk::Entry*			entryCSoundExecutable;
	Gtk::Button*		buttonBrowseCSoundExecutable;
	Gtk::Entry*			entryCSoundHelp;
	Gtk::Button*		buttonBrowseCSoundHelp;
	Gtk::Entry*			entryCSoundExternalGUI;
	Gtk::Button*		buttonBrowseCSoundExternalGUI;
	Gtk::Entry*			entryWavePlayer;
	Gtk::Button*		buttonBrowseWavePlayer;
	Gtk::Entry*			entryWaveEditor;
	Gtk::Button*		buttonBrowseWaveEditor;
	Gtk::Entry*			entryCalculator;
	Gtk::Button*		buttonBrowseCalculator;
	Gtk::Entry*			entryWorkingDirectory;
	Gtk::Button*		buttonBrowseWorkingDirectory;
	Gtk::Entry*			entryPythonCompiler;
	Gtk::Button*		buttonBrowsePythonCompiler;
	Gtk::Entry*			entryPythonExternalGUI;
	Gtk::Button*		buttonBrowsePythonExternalGUI;
	Gtk::Entry*			entryLuaCompiler;
	Gtk::Button*		buttonBrowseLuaCompiler;
	Gtk::Entry*			entryLuaExternalGUI;
	Gtk::Button*		buttonBrowseLuaExternalGUI;

	Gtk::Entry*			entryCabbageExe;
	Gtk::Button*		buttonBrowseCabbageExe;
	Gtk::Button*		buttonClearCabbageExe;
	
	Gtk::Button*		buttonAutoSearchPaths;
	Gtk::CheckButton*   checkbuttonSFDIR;
	Gtk::Entry*			entrySFDIR;
	Gtk::Button*		buttonBrowseSFDIR;
	Gtk::Button*		buttonClearSFDIR;
	Gtk::Entry*			entrySSDIR;
	Gtk::Button*		buttonBrowseSSDIR;
	Gtk::Button*		buttonClearSSDIR;
	Gtk::Entry*			entrySADIR;
	Gtk::Button*		buttonBrowseSADIR;
	Gtk::Button*		buttonClearSADIR;
	Gtk::Entry*			entryMFDIR;
	Gtk::Button*		buttonBrowseMFDIR;
	Gtk::Button*		buttonClearMFDIR;
	Gtk::Entry*			entryINCDIR;
	Gtk::Button*		buttonBrowseINCDIR;
	Gtk::Button*		buttonClearINCDIR;
	Gtk::Entry*			entryOPCODEDIR;
	Gtk::Button*		buttonBrowseOPCODEDIR;
	Gtk::Button*		buttonClearOPCODEDIR;
	Gtk::Entry*			entryCompilerFontName;
	Gtk::Button*		buttonBrowseFontsCompiler;
	Gtk::SpinButton*	spinbuttonCompilerFontSize;
	Gtk::Button*		buttonDefaultFontCompiler;
	Gtk::Entry*			entryCSoundCompilerFlags;
	Gtk::Button*		buttonCSoundDefaultFlags;
	Gtk::RadioButton*   radiobuttonOpenSoundfileWithNothing;
	Gtk::RadioButton*   radiobuttonOpenSoundfileWithMediaPlayer;
	Gtk::RadioButton*   radiobuttonOpenSoundfileWithWaveEditor;
	Gtk::Entry*			entryPythonCompilerFlags;
	Gtk::Button*		buttonPythonDefaultFlags;
	Gtk::Entry*			entryLuaCompilerFlags;
	Gtk::Button*		buttonLuaDefaultFlags;
	Gtk::Entry*			entryTextFontName;
	Gtk::Button*		buttonBrowseFontsText;
	Gtk::SpinButton*	spinbuttonTextFontSize;
	Gtk::SpinButton*	spinbuttonTextTabIndentSize;
	Gtk::Button*		buttonDefaultFontText;
	Gtk::CheckButton*   checkbuttonTextShowMatchingBracket;
	Gtk::CheckButton*   checkbuttonTextSaveBookmarks;
	Gtk::CheckButton*   checkbuttonTextMarkCaretLine;
	Gtk::CheckButton*   checkbuttonTextShowVerticalRuler;
	Gtk::CheckButton*   checkbuttonShowFoldLine;
	Gtk::Button*		buttonDefaultTextProperties;
	Gtk::Button*		buttonDefaultSyntaxCSound;
	Gtk::Button*		buttonDefaultSyntaxPython;
	Gtk::Button*		buttonDefaultSyntaxLua;
	Gtk::CheckButton*   checkbuttonOptions;
	Gtk::CheckButton*   checkbuttonInstrMacros;
	Gtk::CheckButton*   checkbuttonInstrOpcode;
	Gtk::CheckButton*   checkbuttonInstrInstr;
	Gtk::CheckButton*   checkbuttonScoreFunctions;
	Gtk::CheckButton*   checkbuttonScoreMacros;
	Gtk::CheckButton*   checkbuttonScoreSections;
	Gtk::RadioButton*   radiobuttonExplorerFontSmall;
	Gtk::RadioButton*   radiobuttonExplorerFontMedium;
	Gtk::RadioButton*   radiobuttonExplorerFontLarge;
	Gtk::Button*		buttonCreateDesktopShortcut;
	Gtk::Button*		buttonExportSettings;
	Gtk::Button*		buttonImportSettings;
	Gtk::Button*		buttonCancel;
	Gtk::Button*		buttonSaveSettings;
	Gtk::VBox*			vboxCSoundColors;
	Gtk::VBox*			vboxPythonColors;
	Gtk::VBox*			vboxLuaColors;
	Gtk::CheckButton*   checkbuttonPythonMixed;
	Gtk::Notebook*		notebookColors;

	Gtk::Frame*			frameDefaultMediaPlayer;

	void on_buttonSaveSettings_Clicked();
	void on_buttonCancel_Clicked();
	void ApplySettings();
	bool on_key_press_event(GdkEventKey* event);
	void on_buttonBrowseFontsText_Clicked();
	void on_buttonBrowseFontsCompiler_Clicked();
	void on_buttonDefaultTemplates_Clicked();
	void on_buttonExportSettings_Clicked();
	void on_buttonImportSettings_Clicked();
	void on_buttonCreateDesktopShortcut_Clicked();
	void on_buttonDefaultTextProperties_Clicked();
	void on_buttonDefaultFontText_Clicked();
	void on_buttonDefaultFontCompiler_Clicked();
	void on_buttonBrowse_Clicked(Glib::ustring name);
	void on_buttonBrowseDirectory_Clicked(Glib::ustring name);
	void on_buttonClearEnvironment_Clicked(Glib::ustring name);
	void on_buttonCSoundDefaultFlags_Clicked();
	void on_buttonPythonDefaultFlags_Clicked();
	void on_buttonLuaDefaultFlags_Clicked();
	void on_buttonAutoSearchPaths_Clicked();
	void on_buttonDefaultAlternativeCSoundFlags_Clicked();

	
	
	
};

#endif // _WX_SETTINGS_H_
