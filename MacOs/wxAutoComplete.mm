//
//  wxAutoComplete.m
//  WinXound
//
//  Created by Teto on 01/06/10.
//  Copyright 2010 __MyCompanyName__. All rights reserved.
//

#import "wxAutoComplete.h"
#import "TextEditorView.h"
#import "wxGlobal.h"
#import "wxMainController.h"
#import "ScintillaView.h"
#import "wxDocument.h"

@implementation wxAutoComplete


static id sharedInstance = nil;



#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
+ (wxAutoComplete*)sharedInstance
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

- (void) dealloc
{	
	[opcodes release];
	
	[super dealloc];
}

- (void) awakeFromNib
{
	[TableViewOpcodes setTarget:self];
	[TableViewOpcodes setDataSource:self];
	[TableViewOpcodes setDelegate:self];
	[TableViewOpcodes setDoubleAction:@selector(wxDoubleClickInsertText:)];
}

//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------





#pragma mark Various methods
//--------------------------------------------------------------------------------------------------
// VARIOUS
//--------------------------------------------------------------------------------------------------
- (void)setDocumentOwner:(wxDocument*)wxDoc withEditor:(TextEditorView*)editor
{
	textEditor = editor;
	document = wxDoc;
}

- (void)FillTableWithOpcodes
{

	opcodes = [[NSMutableArray alloc] init];
	
	//LOAD OPCODES DATABASE (DICTIONARY)
	NSError* error = nil;
	NSString* path = [[NSBundle mainBundle] pathForResource: @"opcodes" 
													 ofType: @"txt" inDirectory: nil];	
	NSString* opc = [NSString stringWithContentsOfFile: path
											  encoding: NSUTF8StringEncoding
												 error: &error];
	if (error && [[error domain] isEqual: NSCocoaErrorDomain])
		NSLog(@"%@", error);
	
	
	NSArray* lines = [opc componentsSeparatedByString:@"\r\n"];
	//NSArray* lines = [opc componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];
	

	//Fill Opcodes List
	for(NSString* mString in lines)
	{
		NSString* temp = [mString stringByReplacingOccurrencesOfString:@"\"" withString:@""];
		NSArray* split = [temp componentsSeparatedByString:@";"];
		NSString* text = [split objectAtIndex:0];
		
		if([opcodes containsObject:text] == false && 
		   [text length] > 0)
		{
			[opcodes addObject:text];
		}
		
	}
	
	//Sort opcodes alphabetically
	[opcodes sortUsingSelector:@selector(localizedCaseInsensitiveCompare:)];

	[TableViewOpcodes reloadData];
	
}



- (void)displayAutocomplete
{
	[self displayAutocompleteAt:NSMakePoint(0, 0)];
}

- (void)displayAutocompleteAt:(NSPoint)point
{
	if (CSoundAutocompleteWindow == nil) 
	{
		[NSBundle loadNibNamed:@"wxAutocompleteWindow" owner:self];
		[CSoundAutocompleteWindow setShowsToolbarButton:NO];
		[CSoundAutocompleteWindow setDelegate:self];
	}
	
	if(CSoundAutocompleteWindow != nil)
	{
		//Set window position if specified
		if(point.x != 0 && point.y != 0)
		{
			[CSoundAutocompleteWindow setFrameOrigin:point];
		}
		
		[CSoundAutocompleteWindow setAlphaValue:0.9];
		//[CSoundAutocompleteWindow setStyleMask:NSBorderlessWindowMask];
		
		
		//[TableViewOpcodes selectRow:0 byExtendingSelection:false];
		[TableViewOpcodes selectRowIndexes:[NSIndexSet indexSetWithIndex:0] byExtendingSelection:NO];
		if([[textEditor getCurrentWord] length] > 0)
		{
			//NSInteger index = [opcodes indexOfObject:[textEditor getCurrentWord]];
			for (NSInteger index = 0; index < [opcodes count]; index++)
			{
				if([[opcodes objectAtIndex:index] hasPrefix:[textEditor getCurrentWord]])
				{
					//[TableViewOpcodes selectRow:index byExtendingSelection:false];
					[TableViewOpcodes selectRowIndexes:[NSIndexSet indexSetWithIndex:index] byExtendingSelection:NO];
					break;
				}
			}
			//if(index > 0)
			//	[TableViewOpcodes selectRow:index byExtendingSelection:false];
		}
		[TableViewOpcodes scrollRowToVisible:[TableViewOpcodes selectedRow]];

	
		[[textEditor getFocusedEditor] backend]->ShowCaret();
		[CSoundAutocompleteWindow makeKeyAndOrderFront:textEditor];
		[CSoundAutocompleteWindow makeFirstResponder:TableViewOpcodes];
		//[NSApp runModalForWindow:CSoundAutocompleteWindow];
	}
	
}


- (void) windowDidResignMain: (NSNotification *) notification
{
	//NSLog(@"windowDidResignMain");
	if(CSoundAutocompleteWindow != nil)
	{
		if([CSoundAutocompleteWindow isVisible])
			[CSoundAutocompleteWindow setIsVisible:false];
	}
}

- (void) windowDidResignKey:(NSNotification *)notification
{
	//NSLog(@"windowDidResignKey");
	if(CSoundAutocompleteWindow != nil)
	{
		if([CSoundAutocompleteWindow isVisible])
			[CSoundAutocompleteWindow setIsVisible:false];
	}
}

- (IBAction)wxCloseAutocompleteWindow:(id)sender
{
	[CSoundAutocompleteWindow close];
	[NSApp stopModal];
}

- (IBAction)wxDoubleClickInsertText:(id)sender
{
	if(textEditor == nil)
	{
		[self wxCloseAutocompleteWindow:nil];
		return;
	}
	
	if([TableViewOpcodes selectedRow] < 0) return;
	   
	NSString* text = [opcodes objectAtIndex:[TableViewOpcodes selectedRow]];
	[self insertAutocompleteString:text space:@"" opcode:text];
	
	[self wxCloseAutocompleteWindow:nil];
}

- (IBAction)wxAutocompleteInsertText:(id)sender
{
	
	if(textEditor == nil)
	{
		[self wxCloseAutocompleteWindow:nil];
		return;
	}
	
	if([TableViewOpcodes selectedRow] < 0) return;
	
	
	NSButton* temp = (NSButton*)sender;

	if([[temp title] isEqualToString:@"enter"])
	{
		NSString* text = [opcodes objectAtIndex:[TableViewOpcodes selectedRow]];
		
		//[textEditor AddText:[opcodes objectAtIndex:[TableViewOpcodes selectedRow]]];
		[self insertAutocompleteString:text space:@"" opcode:text];
	}
	
	else if([[temp title] isEqualToString:@"shift_enter"])
	{
		//NSLog(@"SYNOPSIS");
		
		NSString* opcode =  [opcodes objectAtIndex:[TableViewOpcodes selectedRow]];
		NSString* description = [wxMAIN getOpcodeValue: opcode];
		NSString* text = [description stringByReplacingOccurrencesOfString:@"\"" withString:@""];
		NSArray* items = [text componentsSeparatedByString:@";"];
		
		if ([description length] > 0)
		{
			NSString* synopsis = [items objectAtIndex:2];
			
			if([[synopsis lowercaseString] rangeOfString:@"not available"].location != NSNotFound) return;
			
			//[textEditor AddText:synopsis];
			[self insertAutocompleteString:synopsis space:@"" opcode:opcode];
		}
		
	}
	
	else if([[temp title] isEqualToString:@"space"])
	{
		NSString* text = [opcodes objectAtIndex:[TableViewOpcodes selectedRow]]; 
		//[textEditor AddText:text];
		[self insertAutocompleteString:text space:@" " opcode:text];
	}
	
	else if([[temp title] isEqualToString:@"tab"])
	{
		NSString* text = [opcodes objectAtIndex:[TableViewOpcodes selectedRow]]; 
		//[textEditor AddText:text];
		[self insertAutocompleteString:text space:@"\t" opcode:text];
	}
	
	[self wxCloseAutocompleteWindow:nil];
	
}

- (void)insertAutocompleteString:(NSString*)text space:(NSString*)space opcode:(NSString*)opcode
{
	if(textEditor == nil) return;
	
	NSInteger pos = [textEditor getCaretPosition];
	if (pos < 0) pos = 0;
	
	NSInteger start = pos;
	NSInteger end = pos;
	
	NSInteger wordStart = [textEditor getWordStart:pos];
	NSInteger wordEnd = [textEditor getWordEnd:pos];
	if(wordStart < start)
	{
		start -= (start - wordStart);
	}
	if(pos < wordEnd)
	{
		end += (wordEnd - pos);
	}
	
	
	[[textEditor getFocusedEditor] setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
	{
		if(start != pos)
		{
			[textEditor ReplaceTarget:start 
							   length:end - start 
						ReplaceString:@""];
		}

		[textEditor AddText:[NSString stringWithFormat:@"%@%@", text, space]];
	}
	[[textEditor getFocusedEditor] setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
	
	
	//Show opcode info into the parent document intellitip
	if(document != nil)
	{
		[document setIntelliTipTextForOpcode:opcode];
	}
	
}











#pragma mark - TableVIEW Overrides
//----------------------------------------------------------------------------------------------------------
// TableVIEW OVERRIDES AND IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------
- (int)numberOfRowsInTableView:(NSTableView *)tableView
{
	if(opcodes != nil)
	{
		return [opcodes count]; 
	}
	return 0;
}



//Implement the protocol method to retrieve the object value for a table column.
- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(int)row
{
	@try
	{
		return [opcodes objectAtIndex:row];
	}
	
	@catch (NSException * e) 
	{
		NSLog(@"wxAutoComplete -> objectValueForTableColumn Error: %@ - %@", [e name], [e reason]);
	}
	
	return nil;
}



//- (void) tableViewSelectionDidChange:(NSNotification *)notification
//{
//	if([[notification object] isEqualTo:codeList])
//	{		
//		[self loadFileName:[codeList selectedRow]];
//	}
//}

//- (void) tableView:(NSTableView *)tableView willDisplayCell:(id)cell forTableColumn:(NSTableColumn *)tableColumn row:(NSInteger)row
//{
//	//NSLog(@"Cell string: %@", [cell stringValue]);
//	
//	if([tableView selectedRow] == row)
//	{
//		[cell setTextColor:[NSColor whiteColor]];
//	}
//	else if([[cell stringValue] hasSuffix:@".udo"])
//	{
//		[cell setTextColor:[NSColor colorWithCalibratedRed:0.0 
//													 green:0.0 
//													  blue:0.3 
//													 alpha:1.0]];
//	}
//	else
//	{
//		//[cell setTextColor:[NSColor blackColor]];
//		[cell setTextColor:[NSColor colorWithCalibratedRed:0.0 
//													 green:0.3 
//													  blue:0.0 
//													 alpha:1.0]];
//	}
//}

//----------------------------------------------------------------------------------------------------------
// TableVIEW OVERRIDES AND IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------


@end
