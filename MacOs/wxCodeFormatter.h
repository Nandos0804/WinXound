//
//  wxCodeFormatter.h
//  WinXound
//
//  Created by Stefano Bonetti on 22/02/10.
//  
//  This class implements methods to format (indent) the source code
//

#import <Cocoa/Cocoa.h>

@class TextEditorView;


@interface wxCodeFormatter : NSObject {

	IBOutlet NSTextView* textView;
	IBOutlet NSWindow* optionsWindow; 
}

+ (wxCodeFormatter*) sharedInstance;


//- (IBAction) wxFormatterCANCEL:(id)sender;
- (IBAction) wxFormatterAPPLY:(id)sender;
- (IBAction) wxFormatterRESET:(id)sender;
- (IBAction) wxFormatterCLICK:(id)sender;


- (void) showOptionsWindow;
	
- (void) formatCode:(TextEditorView*)textEditor 
			  start:(NSInteger)start
				end:(NSInteger)end;

- (NSString*) parseString: (NSString*) stringWithSpaces;

- (BOOL) CheckScoreForGoodLine:(NSString*)textline;

- (void) FormatExample;



@end
