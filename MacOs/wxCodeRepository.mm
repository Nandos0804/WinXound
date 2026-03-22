//
//  wxCodeRepository.m
//  WinXound
//
//  Created by Stefano Bonetti on 28/03/10.
//

#import "wxCodeRepository.h"
#import "TextEditorView.h"
#import "wxGlobal.h"
#import "wxMainController.h"
#import "ScintillaView.h"


@implementation wxCodeRepository


static id sharedInstance = nil;



#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
+ (wxCodeRepository*)sharedInstance
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
		
		files = [[NSMutableArray alloc] init];
    }

	return sharedInstance;
}

- (void) dealloc
{
	[codeEditor release];
	[files release];
	[winxoundPath release];
	
	[super dealloc];
}

//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------











#pragma mark Various methods
//--------------------------------------------------------------------------------------------------
// VARIOUS
//--------------------------------------------------------------------------------------------------
- (void)setOwner:(TextEditorView*)editor
{
	textEditor = editor;
}

- (void)initializeCodeEditor
{
	if (codeWindow == nil) {
		[NSBundle loadNibNamed:@"wxRepository" owner:self];
		[codeWindow setShowsToolbarButton:NO];
	}
	
	if(codeEditor == nil)
	{
		//Add editor preview (codeEditor)
		NSRect newFrame = codeEditorHost.frame;
		newFrame.origin.y = 0;
		newFrame.origin.x = 0;
		newFrame.size.width -= 2; //2 * newFrame.origin.x;
		newFrame.size.height -= 2; //2 * newFrame.origin.y;
		
		codeEditor = [[TextEditorView alloc] initWithFrame: newFrame];
		
		if(codeEditor == nil)
		{
			[wxMAIN ShowMessageError:@"wxCodeRepository -> displayCodeRepositoryAt:" 
							   error:[NSString stringWithFormat:@"%@\n%@", 
									  @"Critical error!", @"codeEditor Initialization Failed!"]];
			return;
		}
		
		[codeEditorHost.contentView addSubview: codeEditor];
		
		[self configureCodeEditor];
	}
	
}

- (void)initializeRepository
{
	//Check for codeEditor initialization
	[self initializeCodeEditor];
	
	//Search for User Application Support directory
	NSArray* paths = NSSearchPathForDirectoriesInDomains(NSApplicationSupportDirectory, NSUserDomainMask, YES);
	if(paths != nil)
	{
		if([paths count] > 0)
		{
			//if(winxoundPath != nil)
			//	[winxoundPath release];
			
			winxoundPath = [[NSString stringWithFormat:@"%@/WinXound/CodeRepository", [paths objectAtIndex:0]] retain];
			
			//If the directory does not exist we create it and copy some examples
			NSFileManager *fileManager = [NSFileManager defaultManager];
			if([fileManager fileExistsAtPath:winxoundPath] == false)
			{
				[fileManager createDirectoryAtPath:winxoundPath 
					   withIntermediateDirectories:YES 
										attributes:nil 
											 error:nil];
								
				//Create some example files
				NSString* output = nil;
				NSArray* paths = [[NSBundle mainBundle] pathsForResourcesOfType:@"udc" inDirectory:nil];
				for(NSString* s in paths)
				{
					output = [NSString stringWithFormat:@"%@/%@.txt",winxoundPath,[[s lastPathComponent] stringByDeletingPathExtension]];
					//NSLog(@"file: %@ to: %@", s, output);
					[fileManager copyItemAtPath:s toPath:output error:nil];
				}
				
				//Copy udo files
				paths = [[NSBundle mainBundle] pathsForResourcesOfType:@"udo" inDirectory:nil];
				for(NSString* s in paths)
				{
					output = [NSString stringWithFormat:@"%@/%@",winxoundPath,[s lastPathComponent]];
					//NSLog(@"file: %@ to: %@", s, output);
					[fileManager copyItemAtPath:s toPath:output error:nil];
				}
				
			}
			
		}
	}
}

- (void)displayCodeRepository
{
	[self displayCodeRepositoryAt:NSMakePoint(0, 0)];
}

- (void)displayCodeRepositoryAt:(NSPoint)point
{
	if (codeWindow == nil) {
		[NSBundle loadNibNamed:@"wxRepository" owner:self];
		[codeWindow setShowsToolbarButton:NO];
		
		/*
		//Add editor preview (codeEditor)
		NSRect newFrame = codeEditorHost.frame;
		newFrame.origin.y = 0;
		newFrame.origin.x = 0;
		newFrame.size.width -= 2; //2 * newFrame.origin.x;
		newFrame.size.height -= 2; //2 * newFrame.origin.y;
		
		codeEditor = [[TextEditorView alloc] initWithFrame: newFrame];
		if(codeEditor == nil)
		{
			[wxMAIN ShowMessageError:@"wxCodeRepository -> displayCodeRepositoryAt:" 
							   error:[NSString stringWithFormat:@"%@\n%@", 
									  @"Critical error!", @"codeEditor Initialization Failed!"]];
			return;
		}
		[codeEditorHost.contentView addSubview: codeEditor];
		
		[self configureCodeEditor];
		*/
		
	}
	
	if(codeWindow != nil)
	{
		[self refreshList];
		
		if([codeList selectedRow] == -1)
		{
			[codeList selectRowIndexes:[NSIndexSet indexSetWithIndex:0] byExtendingSelection:NO];
			[codeList scrollRowToVisible:0];
		}
		else
		{
			[codeList scrollRowToVisible:[codeList selectedRow]];
		}

		
		//Set window position if specified
		if(point.x != 0 && point.y != 0)
		{
			[codeWindow setFrameOrigin:point];
		}
		
		[labelPath setStringValue:[NSString stringWithFormat:@"Repository path: %@", winxoundPath]];
		[codeWindow makeFirstResponder:codeList];
		[codeWindow makeKeyAndOrderFront:self];
	}
		 
}


- (void)configureCodeEditor
{
	//codeEditor base settings:
	[codeEditor setTextEditorFont:[NSFont fontWithName:@"Andale Mono" size:11]];
	[codeEditor setShowLineNumbers:false]; 
	[codeEditor setShowSpaces:false];
	[codeEditor setShowEOLMarker:false];
	[codeEditor setShowMatchingBracket:false];
	[codeEditor	setShowVerticalRuler:false];
	[codeEditor	setMarkCaretLine:false];
	[codeEditor setTabIndent:[[wxDefaults valueForKey:@"EditorTabIndent"] integerValue]];
	[codeEditor setZoom:0];
	[codeEditor setEolMode:SC_EOL_LF];
	
	//[codeEditor setScrollWidth: [[codeEditor getPrimaryView] getTextRectangleWidth]];
	[codeEditor setScrollWidthTracking: true];
	
	
	[codeEditor setHighlight:@"winxound"];
	[codeEditor setKeyWords:0 keyWordList: @"setksmps xin xout"];
	[codeEditor setKeyWords:1 keyWordList: @"opcode endop "];
	[codeEditor setWordChars:@"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_"];
	
	//Set styles from 0 to 33
	//STYLE_NUMBERS_MARGINS = 33;
	for (NSInteger mIndex = 0; mIndex < 34; mIndex++)
	{
		[codeEditor StyleSetFore:mIndex htmlcolor:[[wxMAIN getSettings] StyleGetForeColor:@"csound" stylenumber:mIndex]];
		[codeEditor StyleSetBack:mIndex htmlcolor:[[wxMAIN getSettings] StyleGetBackColor:@"csound" stylenumber:mIndex]];
		[codeEditor StyleSetBold:mIndex bold:[[wxMAIN getSettings] StyleGetBold:@"csound" stylenumber:mIndex]];
		[codeEditor StyleSetItalic:mIndex italic:[[wxMAIN getSettings] StyleGetItalic:@"csound" stylenumber:mIndex]];
	}
		
	//TEXT SELECTION (style 256)
	[codeEditor setSelBack:true 
				 htmlcolor:[[wxMAIN getSettings] StyleGetBackColor:@"csound" stylenumber:256]];
	
	//CARET COLOR (Same as Text Fore Color)
	[codeEditor setCaretFore:[[wxMAIN getSettings] StyleGetForeColor:@"csound" stylenumber:32]];
	
	
}


//Display panel for Name Input
- (void)displayInputBoxPanel:(NSWindow*) window
{
	if (codeInputWindow == nil) {
		[NSBundle loadNibNamed:@"wxRepository" owner:self];
		[codeInputWindow setShowsToolbarButton:NO];
	}
	
	if(codeInputWindow != nil)
	{
		[NSApp beginSheet:codeInputWindow 
		   modalForWindow:window
			modalDelegate:self 
		   didEndSelector:nil
			  contextInfo:nil];
	}
}

- (IBAction)wxInputBoxPanelCANCEL:(id)sender
{
	[codeInputWindow close];
	[NSApp endSheet:codeInputWindow];
}

- (IBAction)wxInputBoxPanelOK:(id)sender
{
	[codeInputWindow close];
	[NSApp endSheet:codeInputWindow];
	
	if([[textEditor getSelectedText] length] > 0)
	{
		NSString* filename = [codeInputName stringValue];
		if(filename == nil || [filename length] < 1) return;
		
		NSString* path = [NSString stringWithFormat:@"%@/%@.txt", winxoundPath, filename];
		
		[[textEditor getSelectedText] writeToFile:path 
									   atomically:YES 
										 encoding:NSUTF8StringEncoding 
											error:nil];
		
		[self refreshList];
		
		
		
		//Select the new row
		NSInteger index = 0;
		NSCell* cell = nil;
		
		for(NSInteger i = 0; i < [codeList numberOfRows]; i++)
		{
			cell = [codeList preparedCellAtColumn:0 row:i];
			//NSLog([cell title]);
			if([[cell title] isEqualToString: [NSString stringWithFormat:@"%@.txt", filename]])
			{
				index = i;
				break;
			}
		}
		
		//NSIndexSet* nset = [[NSIndexSet alloc] initWithIndex:index];
		//[fontTable selectRowIndexes:nset byExtendingSelection:NO];
		//[fontTable scrollRowToVisible:index];
		//[nset release];
		[codeList selectRowIndexes:[NSIndexSet indexSetWithIndex:index] byExtendingSelection:NO];
		[codeList scrollRowToVisible:index];
	}
	
}

//Insert Text Action
- (IBAction) codeInsertText:(id)sender
{
	//Convert line endings before to insert text
	NSString* text = [textEditor convertEolOfText:[codeEditor getText]];
	
	if([[textEditor getSelectedText] length] > 0)
	{
		[textEditor ReplaceText:text];
	}
	else
	{
		//[textEditor InsertText:[textEditor getCaretPosition] text:text];
		//[textEditor setCaretPosition:([textEditor getCaretPosition] + [text length])];
		[textEditor AddText:text];
	}
	
	[codeWindow close];
}

//Remove File Action
- (IBAction)codeRemoveFile:(id)sender
{
	NSInteger selRow = [codeList selectedRow];
	
	NSString* filename = [files objectAtIndex:selRow];
	NSString* path = [NSString stringWithFormat:@"%@/%@", winxoundPath, filename];
	
	if([[NSFileManager defaultManager] fileExistsAtPath:path])
	{
		[[NSFileManager defaultManager] removeItemAtPath:path error:nil];
	}
	
	[self refreshList];
	
	if(selRow < [files count])
		[codeList selectRowIndexes:[NSIndexSet indexSetWithIndex:selRow] byExtendingSelection:NO];
	//else
	//	[codeList selectRowIndexes:[NSIndexSet indexSetWithIndex:selRow-1] byExtendingSelection:NO];
	
	[self loadFileName:[codeList selectedRow]];
}

- (void)refreshList
{
	if(codeWindow != nil)
	{
		[files removeAllObjects];
		
		
		NSArray* temp = [[NSFileManager defaultManager] contentsOfDirectoryAtPath:winxoundPath error:nil];
		
		for(NSString* filename in temp)
		{
			//Skip hidden files and files does not terminate with txt or udo extension
			if(![filename hasPrefix:@"."])
			{
				if([[filename lowercaseString] hasSuffix:@".txt"] ||
				   [[filename lowercaseString] hasSuffix:@".udo"])
				{
					[files addObject:filename]; 
				}
			}
		}
		
		[codeList reloadData];
	}
}

- (IBAction)codeSaveTextChanges:(id)sender
{	
	NSString* filename = [files objectAtIndex:[codeList selectedRow]];
	if(filename == nil || [filename length] < 1) return;
	
	NSString* path = [NSString stringWithFormat:@"%@/%@", winxoundPath, filename];
	
	if([[NSFileManager defaultManager] fileExistsAtPath:path])
	{
		[[codeEditor getText] writeToFile:path 
							   atomically:YES 
								 encoding:NSUTF8StringEncoding 
									error:nil];
		[codeEditor setSavePoint];
	}
}


//- (IBAction)wxOpenRepositoryFolder:(id)sender
//{
//	if(winxoundPath != nil)
//		[[NSWorkspace sharedWorkspace] openFile:winxoundPath];
//}


- (void)loadFileName:(NSInteger)index
{
	NSString* filename = [files objectAtIndex:index];
	NSString* path = [NSString stringWithFormat:@"%@/%@", winxoundPath, filename];
	
	if([[NSFileManager defaultManager] fileExistsAtPath:path])
	{
		//NSLog(@"tableViewSelectionDidChange: %@", winxoundPath);
		//[codeEditor setText:[NSString stringWithContentsOfFile:path
		//											  encoding:NSUTF8StringEncoding
		//												 error:nil]];
		
		[codeEditor setText:[wxMAIN getStringFromFilename:path]];
		
		[codeEditor setSavePoint];
		[codeEditor emptyUndoBuffer];
	}
}

- (IBAction)codeRestoreDefaultFiles:(id)sender
{
	
	NSInteger ret = [wxMAIN ShowMessage:@"WinXound Code Repository alert!" 
						informativeText:@"This operation will restore the default WinXound Repository files. "
										"If you have modified some original files, all changes will be lost. \n"
										"Other files added by you will not be changed or deleted."
						  defaultButton:@"Proceed"
						alternateButton:@"Cancel" 
							otherButton:nil];
	
	//	NSAlertDefaultReturn means the user pressed the default button.
	//	NSAlertAlternateReturn means the user pressed the alternate button.
	//	NSAlertOtherReturn means the user pressed the other button.
	//	NSAlertErrorReturn means an error occurred while running the alert panel.
	//  NSAlertFirstButtonReturn  = 1000,
	//  NSAlertSecondButtonReturn  = 1001,
	//  NSAlertThirdButtonReturn  = 1002
	
	if (ret != NSAlertFirstButtonReturn) return;
	
	
	NSArray* paths = NSSearchPathForDirectoriesInDomains(NSApplicationSupportDirectory, NSUserDomainMask, YES);
	NSFileManager *fileManager = [NSFileManager defaultManager];
	
	if(paths != nil)
	{
		if([paths count] > 0)
		{
			//If WinXound Path does not exist we recreate it
			if([fileManager fileExistsAtPath:winxoundPath] == false)
			{
				if(winxoundPath != nil)
					[winxoundPath release];
				
				winxoundPath = [[NSString stringWithFormat:@"%@/WinXound/CodeRepository", [paths objectAtIndex:0]] retain];
				
				[fileManager createDirectoryAtPath:winxoundPath 
					   withIntermediateDirectories:YES 
										attributes:nil 
											 error:nil];
			}
			
			//If path exist check for existing files
			if([fileManager fileExistsAtPath:winxoundPath])
			{
				//Create some example files
				NSString* output = nil;
				NSArray* paths = [[NSBundle mainBundle] pathsForResourcesOfType:@"udc" inDirectory:nil];
				for(NSString* s in paths)
				{
					output = [NSString stringWithFormat:@"%@/%@.txt",winxoundPath,[[s lastPathComponent] stringByDeletingPathExtension]];
					
					if([fileManager fileExistsAtPath:output])
					{
						[fileManager removeItemAtPath:output error:nil];
					}
					[fileManager copyItemAtPath:s toPath:output error:nil];
				}
				
				//Copy udo files
				paths = [[NSBundle mainBundle] pathsForResourcesOfType:@"udo" inDirectory:nil];
				for(NSString* s in paths)
				{
					output = [NSString stringWithFormat:@"%@/%@",winxoundPath,[s lastPathComponent]];
					
					if([fileManager fileExistsAtPath:output])
					{
						[fileManager removeItemAtPath:output error:nil];
					}
					[fileManager copyItemAtPath:s toPath:output error:nil];
				}
			}
			
			[self refreshList];
			if([files count] > 0)
			{
				[codeList selectRowIndexes:[NSIndexSet indexSetWithIndex:0] byExtendingSelection:NO];
				[self loadFileName:[codeList selectedRow]];
			}
			
		}
	}
	
}

//--------------------------------------------------------------------------------------------------
// VARIOUS
//--------------------------------------------------------------------------------------------------












#pragma mark - TableVIEW Overrides
//----------------------------------------------------------------------------------------------------------
// TableVIEW OVERRIDES AND IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------
- (int)numberOfRowsInTableView:(NSTableView *)tableView
{
	if([[NSFileManager defaultManager] fileExistsAtPath:winxoundPath])
	{
		return [files count]; 
	}
	return 0;
}

//Implement the protocol method to retrieve the object value for a table column.
- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(int)row
{
	@try
	{
		return [files objectAtIndex:row];
	}
	
	@catch (NSException * e) 
	{
		NSLog(@"wxCodeRepository -> objectValueForTableColumn Error: %@ - %@", [e name], [e reason]);
	}
	
	return nil;
}

- (BOOL) tableView:(NSTableView *)tableView shouldSelectRow:(NSInteger)row
{
	if([codeEditor isTextChanged])
	{
		NSInteger index = [codeList selectedRow];
		
		NSInteger ret = [wxMAIN ShowMessage:@"Code has changed!" 
							informativeText:@"Do you want to save it?"
							  defaultButton:@"Save" 
							alternateButton:@"Discard changes" 
								otherButton:nil];
		
		//  NSAlertFirstButtonReturn  = 1000,
		//  NSAlertSecondButtonReturn  = 1001,
		//  NSAlertThirdButtonReturn  = 1002
		
		if (ret == NSAlertFirstButtonReturn) //SAVE BUTTON PRESSED
		{
			NSString* filename = [files objectAtIndex:index];
			if(filename == nil || [filename length] < 1) return true;
			
			NSString* path = [NSString stringWithFormat:@"%@/%@", winxoundPath, filename];
			
			if([[NSFileManager defaultManager] fileExistsAtPath:path])
			{
				[[codeEditor getText] writeToFile:path 
									   atomically:YES 
										 encoding:NSUTF8StringEncoding 
											error:nil];
			}
			
		}		
	}
	
	return true;
}

- (void) tableViewSelectionDidChange:(NSNotification *)notification
{
	if([[notification object] isEqualTo:codeList])
	{		
		[self loadFileName:[codeList selectedRow]];
	}
}

- (void) tableView:(NSTableView *)tableView willDisplayCell:(id)cell forTableColumn:(NSTableColumn *)tableColumn row:(NSInteger)row
{
	//NSLog(@"Cell string: %@", [cell stringValue]);
	
	if([tableView selectedRow] == row)
	{
		[cell setTextColor:[NSColor whiteColor]];
	}
	else if([[[cell stringValue] lowercaseString] hasSuffix:@".udo"])
	{
		[cell setTextColor:[NSColor colorWithCalibratedRed:0.0 
													 green:0.0 
													  blue:0.3 
													 alpha:1.0]];
	}
	else
	{
		//[cell setTextColor:[NSColor blackColor]];
		[cell setTextColor:[NSColor colorWithCalibratedRed:0.0 
													 green:0.3 
													  blue:0.0 
													 alpha:1.0]];
	}
}

//----------------------------------------------------------------------------------------------------------
// TableVIEW OVERRIDES AND IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------




@end
