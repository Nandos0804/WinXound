//
//  wxFindAndReplace.h
//  WinXound
//
//  Created by Stefano Bonetti on 23/02/10.
//  
//  This class implements a custom Find and Replace Window
//

#import <Cocoa/Cocoa.h>

@class TextEditorView;



@interface wxFindAndReplace : NSObject {

	IBOutlet NSWindow* windowFindAndReplace;
	IBOutlet NSSearchField* fieldFindText;
	IBOutlet NSSearchField* fieldReplaceText;
	IBOutlet NSTextField* resultInfo;
	
	@private
	TextEditorView* textEditor;
}


+ (wxFindAndReplace*) sharedInstance;


- (IBAction)wxFindAndReplaceFIND:(id)sender;
//- (IBAction)wxFindAndReplaceCANCEL:(id)sender;
- (IBAction)wxFindAndReplaceREPLACE:(id)sender;
- (IBAction)wxFindAndReplaceREPLACEANDFIND:(id)sender;
- (IBAction)wxFindAndReplaceFINDUPDOWN:(id)sender;
- (IBAction)wxFindAndReplaceREPLACEALL:(id)sender;


- (void)showFindAndReplace;
- (void)setOwner:(TextEditorView*)editor;
- (void)setSelection:(NSString*)selText;
- (void)findUP;
- (void)findDOWN;


@end
