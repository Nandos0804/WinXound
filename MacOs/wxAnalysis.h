//
//  wxAnalysis.h
//  WinXound
//
//  Created by Stefano Bonetti on 04/03/10.
//  This class implement the Analysis window for CSound Analysis tools
//

#import <Cocoa/Cocoa.h>

@class wxCompiler;

@interface wxAnalysis : NSObject {

	IBOutlet NSComboBox* comboInput;
	IBOutlet NSTextField* textOutput;
	
	//IBOutlet NSBox* fileView;
	IBOutlet NSWindow* analysisWindow;
	IBOutlet NSTextView* compilerOutputAnalysis;
	IBOutlet NSTabView* tabView;
	IBOutlet NSButton* buttonStartCompiler;
	IBOutlet NSButton* buttonStopCompiler;
	IBOutlet NSButton* buttonStopBatch;
	IBOutlet NSBox* compilerHost;
	IBOutlet NSButton* browseInput;
	IBOutlet NSButton* browseOutput;
	
	
	@private
	NSInteger mCurrentIndex;
	BOOL mStopBatch;
	wxCompiler* compiler;
	
}

//@property (readwrite, retain) NSDictionary* _values;


- (IBAction) wxAnalysisButtonInputClick:(id)sender;
- (IBAction) wxAnalysisButtonOutputClick:(id)sender;
- (IBAction) wxAnalysisButtonHelpClick:(id)sender;
- (IBAction) wxAnalysisButtonResetClick:(id)sender;
- (IBAction) wxAnalysisStartAnalysisClick:(id)sender;
- (IBAction) wxAnalysisStopClick:(id)sender;
- (IBAction) wxAnalysisStopBatch:(id)sender;


- (void) initialize;
- (void) showAnalysisWindow;
- (void) SetDefaultValues:(NSString*) mUtilityName;
- (void) StartCompiler;
- (void) compilerCompleted;
- (void) setTabViewItemState:(NSTabViewItem*)tabViewItem;
//- (NSComboBox*) getInputComboBox:(NSInteger)buttonTag;
//- (NSTextField*) getOutputTextField:(NSInteger)buttonTag;




@end
