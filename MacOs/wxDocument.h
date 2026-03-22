//
//  MyDocument.h
//  WinXound
//
//  Created by Stefano Bonetti on 19/01/10.
//

//  MAIN EDITOR WINDOW (NSDOCUMENT) for WinXound.


#import <Cocoa/Cocoa.h>
#import "TextEditorView.h"


@class wxNode;
@class wxPosition;
@class wxCompiler;
@class ImageAndTextCell;
@class SeparatorCell;
//@class TextEditorView;

@interface wxDocument : NSDocument
{
	IBOutlet NSWindow* mainWindow;
	
	IBOutlet NSOutlineView* treeViewStructure;
	IBOutlet NSBox* intelliTipHost;
	IBOutlet NSTextField* intelliTipTitle;
	IBOutlet NSTextField* intelliTipParams;
	IBOutlet NSBox* textEditorHost;
	
	//Four View
	IBOutlet NSView* codeView;
	IBOutlet NSView* treeViewStructureHost;
	IBOutlet NSView* upView;
	IBOutlet NSView* downView;
	
	IBOutlet NSSplitView* mainSplitWindow;
	IBOutlet NSSplitView* compilerSplitWindow;
	IBOutlet NSTextView* compilerOutput;
	IBOutlet NSBox* compilerHost;
	IBOutlet NSButton* buttonStopCompiler;
	IBOutlet NSButton* buttonPauseCompiler;
	
	IBOutlet NSPanel* panelGoToLine;
	IBOutlet NSTextField* panelGoToLineTextField;
	
	IBOutlet NSImageView* imageLock;
	IBOutlet NSButton* buttonDocumentInfo;

	IBOutlet NSPanel* panelDocInfo;
	IBOutlet NSTextField* labelInfoName;
	IBOutlet NSTextField* labelInfoFullPath;
	IBOutlet NSTextField* labelInfoFileType;
	IBOutlet NSTextField* labelInfoFileRights;
	IBOutlet NSTextField* labelInfoEols;
	IBOutlet NSTextField* labelInfoEncoding;
	IBOutlet NSTextField* labelInfoTotalLines;
	IBOutlet NSTextField* labelInfoTotalChars;
	
	IBOutlet NSTextField* textFieldFontSize;
	IBOutlet NSMenu* contextMenu;
	
	IBOutlet NSToolbar*			toolbar;
	IBOutlet NSToolbarItem*		toolbarUNDO;
	IBOutlet NSToolbarItem*		toolbarREDO;
	
	//OrcSco
	IBOutlet NSBox* linkBoxHost;
	IBOutlet NSComboBox* linkBoxComboBox;
	IBOutlet NSPopUpButton* AdditionalFlagsPopup;

	

	@private
	TextEditorView* textEditor;
	wxNode*			rootNode;
	NSTimer*		_Timer;
	wxPosition*		_CursorPosition;
	NSString*		_textContent;
	NSString*		_Language;
	wxCompiler*		_Compiler;
	NSInteger		timerCounter;
	BOOL			isChanged;
	NSInteger		SplitMargins;
	NSString*		currentFileType;
	SeparatorCell*  separatorCell;
	NSInteger		LinkBoxHeight;
	
}

//ACTIONS
//- (IBAction)wxUndo:(id)sender;
//- (IBAction)wxRedo:(id)sender;
//- (IBAction)wxDelete:(id)sender;
//- (IBAction)wxCut:(id)sender;
//- (IBAction)wxCopy:(id)sender;
//- (IBAction)wxPaste:(id)sender;
- (IBAction)wxShowFullCode:(id)sender;
- (IBAction)wxSplitHorizontal:(id)sender;
- (IBAction)wxSplitHorizontalOrcSco:(id)sender;
- (IBAction)wxSplitVertical:(id)sender;
- (IBAction)wxSplitVerticalOrcSco:(id)sender;
- (IBAction)wxCompile:(id)sender;
- (IBAction)wxStopCompiler:(id)sender;
- (IBAction)wxSuspendResumeCompiler:(id)sender;
- (IBAction)wxCompileExternal:(id)sender;
- (IBAction)wxInsertRemoveBookmark:(id)sender;
- (IBAction)wxRemoveAllBookmarks:(id)sender;
- (IBAction)wxGotoNextBookmark:(id)sender;
- (IBAction)wxGotoPreviousBookmark:(id)sender;
- (IBAction)wxTreeViewMouseClick:(id)sender;
- (IBAction)wxShowHideWhiteSpaces:(id)sender;
- (IBAction)wxShowHideLineEndings:(id)sender;
- (IBAction)wxGoToLine:(id)sender;
- (IBAction)wxGoToLineOK:(id)sender;
- (IBAction)wxGoToLineCANCEL:(id)sender;
- (IBAction)wxCallExternalGui:(id)sender;
- (IBAction)wxFindAndReplace:(id)sender;
- (IBAction)wxFindSetSelection:(id)sender;
- (IBAction)wxFindNext:(id)sender;
- (IBAction)wxFindPrevious:(id)sender;
- (IBAction)wxJumpToSelection:(id)sender;
- (IBAction)wxShowCode:(id)sender;
- (IBAction)wxShowCompiler:(id)sender;
- (IBAction)wxShowOpcodeHelp:(id)sender;
- (IBAction)wxCommentLine:(id)sender;
- (IBAction)wxUnCommentLine:(id)sender;
- (IBAction)wxLineEndsConverter:(id)sender;
- (IBAction)wxZoomFontSize:(id)sender;
- (IBAction)wxResetTextZoom:(id)sender;
//- (IBAction)wxInsertMacro:(id)sender;
- (IBAction)wxGotoDefinitionOf:(id)sender;
- (IBAction)wxGotoReferenceOf:(id)sender;
- (IBAction)wxCursorPositionPrevious:(id)sender;
- (IBAction)wxCursorPositionNext:(id)sender;
- (IBAction)wxGotoHyperlink:(id)sender;
- (IBAction)wxFormatCode:(id)sender;
- (IBAction)wxFormatCodeAll:(id)sender;
- (IBAction)wxSetFocusOnPrimaryView:(id)sender;
- (IBAction)wxSetFocusOnSecondaryView:(id)sender;
- (IBAction)wxImportOrc:(id)sender;
- (IBAction)wxImportSco:(id)sender;
- (IBAction)wxExportOrcSco:(id)sender;
- (IBAction)wxExportOrc:(id)sender;
- (IBAction)wxExportSco:(id)sender;
- (IBAction)wxShowDocInfo:(id)sender;
- (IBAction)wxShowDocInfoOK:(id)sender;
- (IBAction)wxShowCodeRepositoryWindow:(id)sender;
- (IBAction)wxSaveUserCodeToRepository:(id)sender;
- (IBAction)wxShowCSoundRepositoryWindow:(id)sender;
- (IBAction)wxShowAutocompleteWindow:(id)sender;
- (IBAction)wxImportOrcSco:(id)sender;

- (IBAction)wxLinkBoxClear:(id)sender;
- (IBAction)wxLinkBoxBrowse:(id)sender;
- (IBAction)wxLinkBoxOpenLink:(id)sender;
- (IBAction)wxAdditionalFlagsPopupClicked:(id)sender;



//For debugging
//- (IBAction)wxTEST:(id)sender;

//Cabbage stuffs
- (IBAction)wxCabbageUpdate:(id)sender;
- (IBAction)wxCabbageExportVSTI:(id)sender;
- (IBAction)wxCabbageExportVST:(id)sender;
- (IBAction)wxCabbageExportAU:(id)sender;
- (void) UpdateCurrentFileForCabbage;


//Various methods
- (void) configureEditor;
- (void) configureEditorForNonSyntaxFiles;
- (void) configureEditorForCSound:(NSDictionary*) opcodes;
- (void) configureEditorForLua;
- (void) configureEditorForPython;
- (void) configureEditorForPythonMixed:(NSDictionary*) opcodes;
- (void) SciEditSetFontsAndStyles;
- (void) configureCompiler;
- (void) configureExplorer;
- (void) setTextContent:(NSString*)text;
- (void) startTimer;
- (void) stopTimer;
- (NSString*) parseString: (NSString*) stringWithSpaces;
- (void) refreshExplorer;
- (void) CheckUndoRedo;
- (void) showOrcSco;
- (void) checkForBracket;
- (void) showLineNumbers:(BOOL)value;
- (void) showExplorer:(BOOL)value;
- (void) showOnlineHelp:(BOOL)value;
- (void) showToolbar:(BOOL)value;
- (void) setUseWinXoundFlags:(BOOL) value;
- (void) OpenHyperLinks:(NSString*)mString;
- (wxNode*) retrieveNodeByName:(NSString*) nodeName;
- (void)goToLine:(NSInteger) lineNumber;
- (NSString*) saveFileBeforeCompile;
- (void) saveBookmarksPosition;
- (BOOL)isSyntaxType;
- (NSString*) getCSoundFlags;
- (BOOL) findString:(NSString*)stringToFind inText:(NSString*) _text withNode:(wxNode*)passedNode isScore:(BOOL)isScore;
- (BOOL) findStringInScore:(NSString*)stringToFind inText:(NSString*)_text withNode:(wxNode*)passedNode;
- (void) checkLineEndings;
- (void) setIntelliTipTextForOpcode:(NSString*)opcode;
- (void) CheckTimer:(NSTimer*)theTimer;
- (void) lookForOrcScoFiles;
- (bool) IsOrcScoFile;
- (void) CompileWithAdditionalFlagsClicked:(id)sender;
- (void) CompileExternalWithAdditionalFlagsClicked:(id)sender;
- (NSString*) CheckForAdditionalFlags:(NSString*)additionalParams;
- (void)Compile:(NSString*)additionalParams External:(bool)external;
- (void)UpdateAdditionalFlagsPopupButtonList;
- (NSString*) GetAdditionalFlagsOptions:(NSString*)options;


//Scintilla notifications
- (void) SCI_UPDATEUI_Notification: (NSNotification *) notification;
- (void) SCI_MODIFIED_Notification: (NSNotification *) notification;
- (void) SCI_ZOOM_Notification: (NSNotification *) notification;
- (void) SCI_LEFTMOUSE_Notification: (NSNotification *) notification;
- (void) SCI_LEFTMOUSEHYPERLINKS_Notification: (NSNotification *) notification;
- (void) SCI_ESCAPEKEYDOWN_Notification: (NSNotification *) notification;
- (void) SCI_LEFTMOUSE_QUOTES_Notification: (NSNotification *) notification;
- (void) SCI_MOD_CONTAINER_Notification: (NSNotification *) notification;


@end
