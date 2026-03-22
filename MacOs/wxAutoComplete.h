//
//  wxAutoComplete.h
//  WinXound
//
//  Created by Teto on 01/06/10.
//  Copyright 2010 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@class TextEditorView;
@class wxDocument;



@interface wxAutoComplete : NSObject {
	
	IBOutlet NSTableView* TableViewOpcodes;
	IBOutlet NSPanel* CSoundAutocompleteWindow;
	
	@private
	TextEditorView* textEditor;
	wxDocument*		document;
	NSMutableArray* opcodes;
	
	
}


+ (wxAutoComplete*) sharedInstance;

- (IBAction)wxCloseAutocompleteWindow:(id)sender;
- (IBAction)wxAutocompleteInsertText:(id)sender;
- (IBAction)wxDoubleClickInsertText:(id)sender;


- (void)setDocumentOwner:(wxDocument*)wxDoc withEditor:(TextEditorView*)editor;
- (void)displayAutocomplete;
- (void)displayAutocompleteAt:(NSPoint)point;
- (void)FillTableWithOpcodes;
- (void)insertAutocompleteString:(NSString*)text space:(NSString*)space opcode:(NSString*)opcode;



@end
