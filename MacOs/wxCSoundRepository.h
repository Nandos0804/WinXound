//
//  wxCSoundRepository.h
//  WinXound
//
//  Created by Teto on 01/06/10.
//  Copyright 2010 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@class TextEditorView;
@class wxNode;


@interface wxCSoundRepository : NSObject {

	//IBOutlet NSTableView* codeList;
	IBOutlet NSOutlineView* treeViewCSound;
	IBOutlet NSPanel* CSoundRepositoryWindow;
	IBOutlet NSTextField* labelDescription;
	IBOutlet NSTextField* labelSynopsis;

	@private
	TextEditorView* textEditor;
	wxNode*			rootNode;
	
	
}


+ (wxCSoundRepository*) sharedInstance;

- (IBAction) codeInsertText:(id)sender;
- (IBAction) codeInsertSynopsis:(id)sender;


- (void)setOwner:(TextEditorView*)editor;
- (void)refreshList;
- (void)FillTreeViewOpcodes;
- (void)displayCSoundRepository;
- (void)displayCSoundRepositoryAt:(NSPoint)point;

@end
