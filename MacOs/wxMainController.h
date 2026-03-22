//
//  wxMainController.h
//  WinXound
//
//  Created by Stefano Bonetti on 19/01/10.
//

#import <Cocoa/Cocoa.h>
#import "wxSettings.h"
#import <WebKit/WebKit.h>

//@class wxSettings;


@interface wxMainController : NSObject { //NSWindowController { //NSObject {

	IBOutlet NSWindow* StartWindow;
	IBOutlet NSTableView* RecentFiles;
	IBOutlet NSWindow* HelpWindow;
	IBOutlet WebView* helpBrowser;
	IBOutlet NSSearchField* helpSearchField;
	IBOutlet NSButton* buttonBack;
	IBOutlet NSButton* buttonForward;
	IBOutlet NSTextField* version;
	
	IBOutlet NSMenu* menuCompileWithAdditionalOptions;
	IBOutlet NSMenu* menuCompileExternalWithAdditionalOptions;
	
	@private
	FSEventStreamRef _stream;
    FSEventStreamContext* _context;
	//NSTimer* _Timer;
	
	//For Cabbage connection
	NSFileHandle*   fifoOUT;
	NSFileHandle*   fifoIN;
}


+ (wxMainController*) sharedInstance;


+ (void)setupDefaults;
- (NSMutableDictionary*) getOpcodes;
- (NSString*) getOpcodeValue:(NSString*) opcode;
- (wxSettings*) getSettings;
- (NSInteger)ShowMessage:(NSString *)message 
		 informativeText:(NSString *)informativeText 
		   defaultButton:(NSString *)defaultButton 
		 alternateButton:(NSString *)alternateButton 
			 otherButton:(NSString *)otherButton;
- (void) ShowMessageError:(NSString*)title error:(NSString*)error;
- (void) showOpcodeHelp:(NSString*) opcode;
- (void) showHelpFor:(NSString*) fileHtml;
- (void) displayStartWindow;
- (void)closeStartWindow;
//+ (void)applyPreferencesToOpenedDocuments;
- (void)applyPreferencesToOpenedDocuments;
- (void)applySettings;
- (void) wxOpenDocumentWithFilename:(NSString*)filename;
- (void) wxOpenOrcScoDocumentWithFilename:(NSString*)filename;
- (id) GetInfoValueForKey:(NSString*)key;
- (BOOL) checkForOrcSco:(NSString*)filename;
- (void)createNewCsoundFileWithContentOfString:(NSString*)text;
- (void) refreshRecentList;
- (NSString*)getStringFromFilename:(NSString*)filename;
- (void) UpdateAdditionalFlagsMenu;


- (IBAction)wxNewCsoundFile:(id)sender;
- (IBAction)wxNewPythonFile:(id)sender;
- (IBAction)wxNewLuaFile:(id)sender;
- (IBAction)wxOpenRecent:(id)sender;
- (IBAction)wxOpen:(id)sender;
- (IBAction)wxNewDocument:(id)sender;
- (IBAction)wxCloseAllDocuments:(id)sender;
- (IBAction)wxShowLineNumbers:(id)sender;
- (IBAction)wxShowExplorer:(id)sender;
- (IBAction)wxShowOnlineHelp:(id)sender;
- (IBAction)wxShowToolbar:(id)sender;
- (IBAction)wxShowAllTools:(id)sender;
- (IBAction)wxHideAllTools:(id)sender;
- (IBAction)wxShowPreferences:(id)sender;
- (IBAction)wxCallMediaPlayer:(id)sender;
- (IBAction)wxCallWaveEditor:(id)sender;
- (IBAction)wxCallCalculator:(id)sender;
- (IBAction)wxCallCommandLine:(id)sender;
- (IBAction)wxShowHelp:(id)sender;
- (IBAction)goHome:(id)sender;
- (IBAction)wxShowWinXoundHelp:(id)sender;
- (IBAction)wxShowCSoundHelp:(id)sender;
- (IBAction)wxShowCSoundFlagsHelp:(id)sender;
- (IBAction)wxOpenAboutPanel:(id)sender;
- (IBAction)wxUseWinxoundFlags:(id)sender;
- (IBAction)wxShowCodeFormatOptions:(id)sender;
- (IBAction)wxWinXoundTest:(id)sender;
- (IBAction)wxHelpFindNext:(id)sender;
- (IBAction)wxHelpFindPrevious:(id)sender;
- (IBAction)wxHelpFindButtons:(id)sender;
- (IBAction)wxShowAnalysisTool:(id)sender;
- (IBAction)wxHelpShowTutorials:(id)sender;
- (IBAction)wxHelpShowFlossManual:(id)sender;
- (IBAction)wxShowCSoundOpcodesHelp:(id)sender;
- (IBAction)wxImportOrcScoToNewCsdFile:(id)sender;


- (IBAction)wxNewOrcFile:(id)sender;
- (IBAction)wxNewScoFile:(id)sender;
- (IBAction)wxNewCabbageFile:(id)sender;


//Cabbage stuffs
- (bool) UpdateCabbage:(NSString*)filename;
- (bool) CabbageExportVSTI:(NSString*)filename;
- (bool) CabbageExportVST:(NSString*)filename;
- (bool) CabbageExportAU:(NSString*)filename;
//- (bool) SendCabbageMessage:(NSString*)message;
- (bool) SendCabbageMessage:(NSString*)message withFilename:(NSString*)filename;
- (void) dataReady:(NSNotification *)n;

//- (bool) CheckForCabbageConnection;




@end
