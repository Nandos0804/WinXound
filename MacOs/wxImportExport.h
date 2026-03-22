//
//  wxImportExport.h
//  WinXound
//
//  Created by Teto on 06/03/10.
//  Copyright 2010 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@class TextEditorView;
@class wxDocument;

@interface wxImportExport : NSObject {

	@private
	NSString* saveAction;
}


+ (wxImportExport*) sharedInstance;


- (NSString*) ImportORCSCO:(NSString*)filename;

- (void) ImportORC:(TextEditorView*)textEditor owner:(wxDocument*)owner;
- (void) internalImportORC:(TextEditorView*)textEditor fromFilename:(NSString*)filename;
- (void) ImportSCO:(TextEditorView*)textEditor owner:(wxDocument*)owner;
- (void) internalImportSCO:(TextEditorView*)textEditor fromFilename:(NSString*)filename;
- (void) ExportOrcSco:(TextEditorView*) textEditor owner:(wxDocument*)owner;
- (void) internalExportOrcSco:(TextEditorView*)textEditor toFilename:(NSString*)filename;
- (void) ExportOrc:(TextEditorView*) textEditor owner:(wxDocument*)owner;
- (void) internalExportOrc:(TextEditorView*)textEditor toFilename:(NSString*)filename;
- (void) ExportSco:(TextEditorView*) textEditor owner:(wxDocument*)owner;
- (void) internalExportSco:(TextEditorView*)textEditor toFilename:(NSString*)filename;


- (void) BrowseFile:(NSString*) extension forWindow:(NSWindow*)sheetWindow editor:(TextEditorView*)textEditor;
- (void) SaveFile:(NSString*) filename forWindow:(NSWindow*)sheetWindow editor:(TextEditorView*)textEditor;

- (NSInteger) findTextPositionWithNSString:(NSString*)_text stringToFind:(NSString*)stringToFind;

@end
