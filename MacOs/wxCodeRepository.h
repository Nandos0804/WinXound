//
//  wxCodeRepository.h
//  WinXound
//
//  Created by Stefano Bonetti on 28/03/10.
//

#import <Cocoa/Cocoa.h>

@class TextEditorView;


@interface wxCodeRepository : NSObject {
	
	IBOutlet NSTableView* codeList;
	
	//IBOutlet NSWindow* codeWindow;
	IBOutlet NSPanel* codeWindow;
	
	IBOutlet NSBox* codeEditorHost;
	TextEditorView* codeEditor;
	
	IBOutlet NSTextField* codeInputName;
	IBOutlet NSPanel* codeInputWindow;
	IBOutlet NSTextField* labelPath;
	
	NSMutableArray* files;
	NSString* winxoundPath;
	TextEditorView* textEditor;
}


+ (wxCodeRepository*) sharedInstance;

- (void)displayCodeRepository;
- (void)displayCodeRepositoryAt:(NSPoint)point;
- (void)setOwner:(TextEditorView*)editor;
- (void)displayInputBoxPanel:(NSWindow*) window;
- (void)initializeRepository;
- (void)refreshList;
- (void)loadFileName:(NSInteger)index;
- (void)configureCodeEditor;
- (void)initializeCodeEditor;

- (IBAction)codeInsertText:(id)sender;
- (IBAction)wxInputBoxPanelOK:(id)sender;
- (IBAction)wxInputBoxPanelCANCEL:(id)sender;
- (IBAction)codeRemoveFile:(id)sender;
- (IBAction)codeSaveTextChanges:(id)sender;
- (IBAction)codeRestoreDefaultFiles:(id)sender;
//- (IBAction)wxOpenRepositoryFolder:(id)sender;


@end
