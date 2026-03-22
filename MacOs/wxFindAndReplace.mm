//
//  wxFindAndReplace.m
//  WinXound
//
//  Created by Stefano Bonetti on 23/02/10.
//  
//

#import "wxFindAndReplace.h"
#import "TextEditorView.h"
#import "wxDocument.h"
#import "wxGlobal.h"


@implementation wxFindAndReplace


static id sharedInstance = nil;



#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
+ (wxFindAndReplace*)sharedInstance
{
	if (sharedInstance == nil) { 
		sharedInstance = [[self alloc] init];
	}
	
	return sharedInstance;
} 


- (id)init 
{
	if (sharedInstance == nil) {
        sharedInstance = [super init];
    }
	
    return sharedInstance;
}


//OVERRIDES
- (BOOL) control:(NSControl *)control textView:(NSTextView *)textView doCommandBySelector:(SEL)commandSelector
{
	//NSLog(@"control");
	//  if (commandSelector == @selector(insertNewline:)) {
	//	    // enter pressed
	//	    result = YES;
	//  }
	
	//Catch Escape Key Event !!!
	if(commandSelector == @selector(cancelOperation:))
	{
		[windowFindAndReplace close];
		return YES;
	}
	
    return NO;
}
//--------------------------------------------------------------------------------------------------
// Initialization
//--------------------------------------------------------------------------------------------------











#pragma mark Settings
//--------------------------------------------------------------------------------------------------
// Properties and settings
//--------------------------------------------------------------------------------------------------
- (void)setOwner:(TextEditorView*)editor
{
	textEditor = editor;
}

- (void)setSelection:(NSString*)selText
{
	[fieldFindText setStringValue:selText];
}

//- (TextEditorView*)getTextEditor
//{
//    id obj = [[NSApp mainWindow] firstResponder];
//    return (obj && [obj isKindOfClass:[TextEditorView class]]) ? obj : nil;
//}
//
//- (wxDocument*)getDocument
//{
//    //id obj = [[NSApp mainWindow] firstResponder];
//    //return (obj && [obj isKindOfClass:[wxDocument class]]) ? obj : nil;
//	return [[NSDocumentController sharedDocumentController] currentDocument];
//}

//--------------------------------------------------------------------------------------------------
// Properties and settings
//--------------------------------------------------------------------------------------------------









#pragma mark IBActions
//--------------------------------------------------------------------------------------------------
// IBACTIONS AND RELATED METHODS
//--------------------------------------------------------------------------------------------------
- (void)showFindAndReplace;
{	
	if (windowFindAndReplace == nil) 
	{
		[NSBundle loadNibNamed:@"wxFindAndReplace" owner:self];
		[windowFindAndReplace setShowsToolbarButton:NO];
	}
	
	[windowFindAndReplace makeKeyAndOrderFront:self];

	if([[textEditor getSelectedText] length] > 0)
		[fieldFindText setStringValue:[textEditor getSelectedText]];
	
}

- (IBAction)wxFindAndReplaceFIND:(id)sender
{
	
	//NSLog(@"FIND");
	if([[fieldFindText stringValue] length] < 1) return;
	
	[resultInfo setStringValue:@""];
	
	NSInteger ret =
		[textEditor FindText:[fieldFindText stringValue]
			  MatchWholeWord:[[wxDefaults valueForKey:@"FindWholeWord"] boolValue]
				   MatchCase:[[wxDefaults valueForKey:@"FindMatchCase"] boolValue]
				  IsBackward:false 
				  SelectText:true 
				 ShowMessage:false 
					 SkipRem:false];
	
	[textEditor removeFocus];
	
	if(ret < 0)
	{
		[resultInfo setStringValue:@"Text not found"];
	}
	
}

- (IBAction)wxFindAndReplaceREPLACE:(id)sender
{	
	[textEditor ReplaceText:[fieldReplaceText stringValue]];	
}

- (IBAction)wxFindAndReplaceREPLACEALL:(id)sender
{	
	
	if([[fieldFindText stringValue] length] < 1) return;
	
	[resultInfo setStringValue:@""];
	NSInteger ret = 
		[textEditor ReplaceAllText:[fieldFindText stringValue] 
					 ReplaceString:[fieldReplaceText stringValue]
					MatchWholeWord:[[wxDefaults valueForKey:@"FindWholeWord"] boolValue]
						 MatchCase:[[wxDefaults valueForKey:@"FindMatchCase"] boolValue]
				 FromCaretPosition:[[wxDefaults valueForKey:@"ReplaceFromCaret"] boolValue] 
							 FCPUp:[[wxDefaults valueForKey:@"ReplaceUp"] boolValue]];
	
	if(ret <= 0)
	{
		[resultInfo setStringValue:@"Text not found"];
	}
	else 
	{
		[resultInfo setStringValue:[NSString stringWithFormat:@"%d occurence(s) replaced", ret]];
	}

	
}

- (IBAction)wxFindAndReplaceREPLACEANDFIND:(id)sender
{	
	if([[textEditor getSelectedText] length] > 0)
	{
		[self wxFindAndReplaceREPLACE:self];
		[self wxFindAndReplaceFIND:self];
	}
}

- (IBAction)wxFindAndReplaceFINDUPDOWN:(id)sender
{
	NSSegmentedControl* control = sender;
	if(control == nil) return;
	
	NSInteger index = [control selectedSegment];
	
	switch (index) 
	{
		//UP
		case 0:
			[self findUP];
			break;
			
		//DOWN
		case 1:
			[self findDOWN];
			break;
	}
}



- (void)findUP
{
	if([[fieldFindText stringValue] length] < 1) return;
	
	NSInteger ret = -1;
	[resultInfo setStringValue:@""];
	
	ret = 
	[textEditor FindText:[fieldFindText stringValue]
		  MatchWholeWord:[[wxDefaults valueForKey:@"FindWholeWord"] boolValue]
			   MatchCase:[[wxDefaults valueForKey:@"FindMatchCase"] boolValue]
			  IsBackward:true 
			  SelectText:true 
			 ShowMessage:false 
				 SkipRem:false];
	
	[textEditor removeFocus];
	
	if(ret < 0)
	{
		[resultInfo setStringValue:@"Text not found"];
	}
}

- (void)findDOWN
{
	[self wxFindAndReplaceFIND:self];
}







@end
