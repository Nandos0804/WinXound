//
//  MyDocument.m
//  WinXound
//
//  Created by Stefano Bonetti on 19/01/10.
//



#import "wxGlobal.h"
#import "wxDocument.h"
#import "ScintillaView.h"
#import "wxMainController.h"
#import "wxCompiler.h"
#import "wxNode.h"
#import "wxPosition.h"
#import "wxCodeFormatter.h"
#import "wxFindAndReplace.h"
#import "wxImportExport.h"
#import "wxCodeRepository.h"
#import "ImageAndTextCell.h"
#import "SeparatorCell.h"
#import "wxCSoundRepository.h"
#import "wxAutoComplete.h"




@implementation wxDocument


//TODO: CHANGE fileName with fileURL (fileName deprecated)


#pragma mark - NsDocument Overrides
//----------------------------------------------------------------------------------------------------------
// NSDOCUMENT OVERRIDES
//----------------------------------------------------------------------------------------------------------
- (id)init
{
    self = [super init];
    if (self) 
	{
    
        // Add your subclass-specific initialization here.
        // If an error occurs here, send a [self release] message and return nil.
		_Language = [[NSString alloc] init];
		_Compiler = [[wxCompiler alloc] init];
		_CursorPosition = [[wxPosition alloc] init];
		timerCounter = 0;
		isChanged = false;
		SplitMargins = 130;
		separatorCell = [[SeparatorCell alloc] init];
		LinkBoxHeight = 29;
    }
    return self;
}


- (NSString *)windowNibName
{
    // Override returning the nib file name of the document
    // If you need to use a subclass of NSWindowController or if your document supports multiple NSWindowControllers, you should remove this method and override -makeWindowControllers instead.
    return @"wxDocument";
}


- (void)windowControllerDidLoadNib:(NSWindowController *) aController
{
    [super windowControllerDidLoadNib:aController];
    
	// Add any code here that needs to be executed once the windowController has loaded the document's window.
	
	
	
	//1.
	//Set Window size
	NSInteger mWidth = [[wxDefaults valueForKey:@"DocumentWindowWidth"] integerValue];
	NSInteger mHeight = [[wxDefaults valueForKey:@"DocumentWindowHeight"] integerValue];
	if(mWidth < 690) mWidth = 690;
	if(mHeight < 350) mHeight = 350;
	[[self windowForSheet] setContentSize:NSMakeSize(mWidth, mHeight)];
	[[self windowForSheet] center];
	
	
	
	//2.
	//Add a new TextEditorView to our textEditorHost (NSBox) container
	NSRect newFrame = textEditorHost.frame;
	newFrame.origin.y = 1;
	newFrame.origin.x = 1;
	newFrame.size.width -= 2 * newFrame.origin.x;
	newFrame.size.height -= 2 * newFrame.origin.y;
	
	textEditor = [[TextEditorView alloc] initWithFrame: newFrame];
	if(textEditor == nil)
	{
		[wxMAIN ShowMessageError:@"wxDocument -> windowControllerDidLoadNib:" 
						   error:[NSString stringWithFormat:@"%@\n%@", 
								  @"Critical error!", @"TextEditorView Initialization Failed!"]];
		return;
	}
	[textEditorHost.contentView addSubview: textEditor];
	
	
	
	//3.
	//Close the main window (wxMainController -> StartWindow) ---
	[wxMAIN closeStartWindow];
	
	
	//Set Text from the current loaded file ---------------------
	if(_textContent != nil)
	{
		[textEditor setText: _textContent];
		[_textContent release];
		_textContent = nil;
	}
	//or create a New file with the default templates
	else
	{
		//NSLog([self fileType]);
		if([[self fileType] isEqualToString:@"CSound Files"])
		{
			[textEditor setText: [wxDefaults valueForKey:@"TemplatesCSound"]];
		}
		else if([[self fileType] isEqualToString:@"Python Files"])
		{
			[textEditor setText: [wxDefaults valueForKey:@"TemplatesPython"]];
		}
		else if([[self fileType] isEqualToString:@"Lua Files"])
		{
			[textEditor setText: [wxDefaults valueForKey:@"TemplatesLua"]];
		}
		else if([[self fileType] isEqualToString:@"CSound Cabbage Files"])
		{
			[textEditor setText: [wxDefaults valueForKey:@"TemplatesCabbage"]];
		}
	}
	
	
	//Set the base editor configuration -------------------------
	[self configureEditor];
	
	//Set and Check Line Endings (look for consistence)
	if([self fileURL] == nil)
	{
		//NEW FILES
		//Convert (templates) and Set line endings to LF (OsX default)
		[textEditor ConvertEOL:SC_EOL_LF];
		[textEditor setEolMode:SC_EOL_LF];
	}
	else
	{
		//LOADED FILES
		[self checkLineEndings];
	}
	
	//Display the actual font size ------------------------------
	[self SCI_ZOOM_Notification:nil];

	
	
	//FILL TreeViewStructure ------------------------------------
	NSTableColumn *tableColumn = [[treeViewStructure tableColumns] objectAtIndex:0];
	ImageAndTextCell *imageAndTextCell = [[[ImageAndTextCell alloc] init] autorelease];
	
	[imageAndTextCell setEditable:NO];
	NSInteger fSize = 11 + [[wxDefaults valueForKey:@"ExplorerFontSize"] integerValue];
	[imageAndTextCell setFont:[NSFont systemFontOfSize:fSize]];
	 
	[tableColumn setDataCell:imageAndTextCell];
	[treeViewStructure setAutoresizesOutlineColumn:YES];
	
	rootNode = [[wxNode alloc] init];
	wxNode *newNode1 = nil, *newNode2 = nil, *newNode3 = nil, *newNode4 = nil, *newNode5 = nil, *newNode6 = nil;
	
	newNode1 = [[wxNode alloc] init];
	newNode2 = [[wxNode alloc] init];
	newNode3 = [[wxNode alloc] init];
	newNode4 = [[wxNode alloc] init];
	newNode5 = [[wxNode alloc] init];
	newNode6 = [[wxNode alloc] init];
	
	newNode1.name = @"<CsoundSynthesizer>";
	newNode2.name = @"<CsOptions>";
	newNode3.name = @"<CsInstruments>";
	newNode4.name = @"<CsScore>";
	newNode5.name = @"---";
	newNode6.name = @"Bookmarks";
	
	newNode1.extendedname = @"<CsoundSynthesizer>";
	newNode2.extendedname = @"<CsOptions>";
	newNode3.extendedname = @"<CsInstruments>";
	newNode4.extendedname = @"<CsScore>";
	newNode5.extendedname = @"---";
	newNode6.extendedname = @"Bookmarks";
	
	[rootNode.children addObject:newNode1];
	[rootNode.children addObject:newNode2];
	[rootNode.children addObject:newNode3];
	[rootNode.children addObject:newNode4];
	[rootNode.children addObject:newNode5];
	[rootNode.children addObject:newNode6];
	
	[newNode1 release];
	[newNode2 release];
	[newNode3 release];
	[newNode4 release];
	[newNode5 release];
	[newNode6 release];
	
	[treeViewStructure reloadData];

	
	
	//Configure the Compiler Output (NSTextView) ------------------
	[self configureCompiler];
	
	//Set the Default Layout for this window ----------------------
	[self wxShowCode:self];
	
	
	
	//Load Bookmarks ----------------------------------------------
	if([self isSyntaxType]) //Load and Save bookmarks only on syntax styled documents (we skip others documents: txt, h)
	{
		@try 
		{
			NSString* languagecomment = nil;
			
			if([[self fileType] isEqualToString:@"CSound Files"] ||
			   [[self fileType] isEqualToString:@"CSound Orc Files"] ||
			   [[self fileType] isEqualToString:@"CSound Sco Files"] ||
			   [[self fileType] isEqualToString:@"CSound Cabbage Files"])
				languagecomment = @";";
			else if([[self fileType] isEqualToString:@"Python Files"])
				languagecomment = @"#";
			else if([[self fileType] isEqualToString:@"Lua Files"])
				languagecomment = @"--";
			else return;
			
			//if([[wxDefaults valueForKey:@"SaveBookmarks"] boolValue])
			{
				//@";[winxound_bookmarks"
				NSInteger ret = [textEditor FindText:[NSString stringWithFormat:@"%@[winxound_bookmarks", languagecomment] 
									  MatchWholeWord:true 
										   MatchCase:true 
										  IsBackward:false 
										  SelectText:false 
										 ShowMessage:false 
											 SkipRem:false
											   start:0
												 end:-1];
				
				if(ret > -1)
				{
					NSString* lineText = [textEditor getTextOfLine:[textEditor getLineNumberFromPosition:ret]];
					//[textEditor ReplaceTarget:ret - 1
					//				   length:[textEditor getLineLength:[textEditor getLineNumberFromPosition:ret]] + 1
					//			ReplaceString:@""];
					
					[textEditor ReplaceTarget:ret
									   length:[textEditor getLineLength:[textEditor getLineNumberFromPosition:ret]]
								ReplaceString:@""];
					
					NSArray* splittedNumbers = [lineText componentsSeparatedByString:@","];
					
					for(NSInteger index = 1; index < [splittedNumbers count]; index++)
					{
						if(![[splittedNumbers objectAtIndex:index] isEqualToString:@""])
							[textEditor InsertBookmarkAt:[[splittedNumbers objectAtIndex:index] integerValue]];
					}
					
					//[self wxInsertRemoveBookmark:nil];
				}
				[self wxInsertRemoveBookmark:nil];
			}
		}
		@catch (NSException * e) 
		{
			[wxMAIN ShowMessageError:@"wxDocument -> windowControllerDidLoadNib: -> LoadBookmarks" 
							   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		}
	}
	
	
	
	//Final settings ----------------------------------------------
	[textEditor setFocusOnPrimaryView];
	
	//Disable the default Undo Manager (we use Scintilla)
	[[self undoManager] disableUndoRegistration];
	[self setUndoManager:nil];
	//NSLog(@"%d", [self undoManager]);
	
	
//	//Check for Line Endings consistence
//	[self checkLineEndings];
	
	
	//Set SavePoint, empty the Undo Buffer and clear document dirty state
	[textEditor emptyUndoBuffer];
	[textEditor setSavePoint];
	[self CheckUndoRedo];
	
	//Start the timer for FindStructure method (treeviewstructure)
	isChanged = true;
	[self startTimer];	
	
	//Set this TextEditor the owner of wxFIND and wxCODEREP
	[wxFIND setOwner:textEditor];
	[wxCODEREP setOwner:textEditor];
	[wxCSOUNDREP setOwner:textEditor];
	[wxAUTOCOMP setDocumentOwner:self withEditor:textEditor];
	
	
	//Document Info Button
	[imageLock setHidden:true];
	if([self fileURL] != nil)
	{
		if([[NSFileManager defaultManager] isWritableFileAtPath:[[self fileURL] path]] == false)
		{
			//NSLog(@"NOT WRITABLE !!!!!!");
			[imageLock setHidden:false];
			[buttonDocumentInfo setTitle:@"Info: Read-Only"];
		}
	}
	
	
	//Store current File type
	currentFileType = [self fileType];
	
	
	//OrcSco check
	if([self IsOrcScoFile])
	{
		//[textEditorHost setFrame:NSMakeRect([textEditorHost frame].origin.x, 
		//									[textEditorHost frame].origin.y, 
		//									[textEditorHost frame].size.width, 
		//									[textEditorHost frame].size.height - 57)];
		[linkBoxHost setHidden:false];
		
		//[linkBoxComboBox setStringValue:@"PROVA PROVA"];
		[self lookForOrcScoFiles];
	}
	
	
	[self UpdateAdditionalFlagsPopupButtonList];
	
	
	//NSLog(@"EOL MODE: %d", [textEditor GetEOLMode]);
	//Menu delegate
	//[[NSApp mainMenu] setDelegate:self];
	//[[[[NSApp mainMenu] itemWithTitle:@"Edit"] submenu] setDelegate:self];

}



//FOCUS
- (void) windowDidBecomeMain: (NSNotification *) notification
{
	//NSLog(@"ACTIVE:%@", self);
	if(![panelGoToLine isVisible] && ![panelDocInfo isVisible])
	{
		[textEditor setFocus];
	}
	[wxFIND setOwner:textEditor];
	[wxCODEREP setOwner:textEditor];
	[wxCSOUNDREP setOwner:textEditor];
	[wxAUTOCOMP setDocumentOwner:self withEditor:textEditor];
}
- (void) windowDidResignMain: (NSNotification *) notification
{
	//NSLog(@"UNACTIVE");
	[textEditor removeFocus];
}
- (void) windowWillBeginSheet:(NSNotification *)notification
{
	[textEditor removeFocus];
}
- (void) windowDidEndSheet:(NSNotification *)notification
{
	[textEditor setFocus];
	[wxFIND setOwner:textEditor];
	[wxCODEREP setOwner:textEditor];
	[wxCSOUNDREP setOwner:textEditor]; //??? Really necessary
	[wxAUTOCOMP setDocumentOwner:self withEditor:textEditor]; //??? Really necessary
}

//DEALLOC
- (void) dealloc
{		
	NSLog(@"wxDOCUMENT DEALLOC");
	
	[separatorCell release];
	[rootNode release];
	
	[_Language release];
	[_CursorPosition release];
	[_Compiler release];
	
	[textEditor removeFromSuperview];
	[textEditor release];
	
	[super dealloc];
	
}


//WINDOW WILL CLOSE
- (void) windowWillClose: (NSNotification *) notification
{
	if([notification object] == mainWindow) //[self windowForSheet])
	{
		NSLog(@"wxDOCUMENT windowWillClose");
		
		[self stopTimer];
		
		[[NSNotificationCenter defaultCenter] removeObserver:self];		
		
		//NSUserDefaultsController *defaultsController = [NSUserDefaultsController sharedUserDefaultsController];
		//[defaultsController save:self];
		
		
		//Save Bookmarks on exit
		if([[wxDefaults valueForKey:@"SaveBookmarks"] boolValue])
		{
			
			//NSInteger ret = [wxMAIN ShowMessage:@"The document contains bookmarks." 
			//					informativeText:@"Would you like to save them?" 
			//					  defaultButton:@"YES" 
			//					alternateButton:@"NO" 
			//						otherButton:nil];
			//
			//if(ret == NSAlertFirstButtonReturn)
			
			//Save bookmarks only on syntax styled documents (CSound, Python, Lua)
			if([self isSyntaxType])
			{
				if([self fileURL] != nil && [self isDocumentEdited] == false)
				{
					if([[NSFileManager defaultManager] isWritableFileAtPath:[[self fileURL] path]])
					{
						[self saveBookmarksPosition];
						[self saveDocument:self];
						NSLog(@"BOOKMARKS SAVED");
					}
				}
			}
		}
		
		
	}
}



//SAVE DOCUMENT
- (NSData *)dataOfType:(NSString *)typeName error:(NSError **)outError
{
    // Insert code here to write your document to data of the specified type. If the given outError != NULL, ensure that you set *outError when returning nil.
    // You can also choose to override -fileWrapperOfType:error:, -writeToURL:ofType:error:, or -writeToURL:ofType:forSaveOperation:originalContentsURL:error: instead.
    // For applications targeted for Panther or earlier systems, you should use the deprecated API -dataRepresentationOfType:. In this case you can also choose to override -fileWrapperRepresentationOfType: or -writeToFile:ofType: instead.
	
    if ( outError != NULL ) 
	{
		*outError = [NSError errorWithDomain:NSOSStatusErrorDomain code:unimpErr userInfo:NULL];
	}
	
	
//	if ( outError == NULL )
//	{
//		//Document Info Button
//		[imageLock setHidden:true];
//		[buttonDocumentInfo setTitle:@"File Info"];
//	}
	
	//NSData* aData;
	//aData = [[textEditor getText] dataUsingEncoding: NSUTF8StringEncoding];
	[textEditor setSavePoint];
	return [[textEditor getText] dataUsingEncoding: NSUTF8StringEncoding];
}

- (void) setFileURL:(NSURL *)absoluteURL
{
	//This is called after a succesfully save operation 
	//We also update some Document Interface stuffs
	[super setFileURL:absoluteURL];
	
	//Set Document Info Button
	if([self fileURL] != nil)
	{
		if([[NSFileManager defaultManager] isWritableFileAtPath:[[self fileURL] path]] == false)
		{
			//NSLog(@"NOT WRITABLE !!!!!!");
			[imageLock setHidden:false];
			[buttonDocumentInfo setTitle:@"Info: Read-Only"];
		}
		else 
		{
			[imageLock setHidden:true];
			[buttonDocumentInfo setTitle:@"File Info"];
		}
	}
	
}


//LOAD DOCUMENT
- (BOOL)readFromData:(NSData *)data ofType:(NSString *)typeName error:(NSError **)outError
{
    // Insert code here to read your document from the given data of the specified type.  If the given outError != NULL, ensure that you set *outError when returning NO.
    // You can also choose to override -readFromFileWrapper:ofType:error: or -readFromURL:ofType:error: instead. 
    // For applications targeted for Panther or earlier systems, you should use the deprecated API -loadDataRepresentation:ofType. In this case you can also choose to override -readFromFile:ofType: or -loadFileWrapperRepresentation:ofType: instead.
    
    if ( outError != NULL ) 
	{
		*outError = [NSError errorWithDomain:NSOSStatusErrorDomain code:unimpErr userInfo:NULL];
	}
	
	//NSLog(@"READFROMDATA CALLED");
	
	
	//1. FIRST LOAD METHOD: use NSString
	_textContent = [[NSString alloc]  initWithData:data encoding:NSUTF8StringEncoding];
	//_textContent = [[NSString alloc]  initWithBytes:[data bytes]
	//										 length:[data length] 
	//									   encoding: NSUTF8StringEncoding];
	
	
	//2. IF 1. FAILS: use NSAttributedString
	if(_textContent == nil)
	{
		//NSCharacterEncodingDocumentOption = NSUTF8StringEncoding
		NSDictionary* options = [NSDictionary  dictionaryWithObject:NSPlainTextDocumentType forKey:NSDocumentTypeDocumentOption];
		NSAttributedString *attrString = [[NSAttributedString alloc] initWithData:data 
																		  options:options 
															   documentAttributes:nil 
																			error:outError];
		_textContent = [[attrString string] copy];
		
		[attrString release];
	}
	
	
	//3. IF 3. FAILS: show error message and return NO
	if(_textContent == nil)
	{
		[wxMAIN ShowMessage:@"WinXound Loading Error" 
			informativeText:@"Unable to load file: File format unrecognized." 
			  defaultButton:@"OK" 
			alternateButton:nil 
				otherButton:nil];
		return NO;
	}
	
	return YES;
}



- (void) printDocument:(id)sender
//- (void)printShowingPrintPanel:(BOOL)flag
{
	//NSLog(@"PRINT");
	
	// set printing properties
	NSPrintInfo* MyPrintInfo = [self printInfo];
	
	[MyPrintInfo setHorizontalPagination:NSFitPagination];
	[MyPrintInfo setHorizontallyCentered:NO];
	[MyPrintInfo setVerticallyCentered:NO];
	
	[MyPrintInfo setLeftMargin:56.0]; //72 = 25mm 56 = 20mm //45 = 15.9mm //28 = 10mm
	[MyPrintInfo setRightMargin:56.0];
	[MyPrintInfo setTopMargin:56.0];
	[MyPrintInfo setBottomMargin:56.0];
	
	// create new view just for printing
	// [MyPrintInfo imageablePageBounds]];
	NSTextView *printView = [[NSTextView alloc]initWithFrame: 
							 //NSMakeRect(0.0, 0.0, 8.5 * 72, 11.0 * 72)];
							 NSMakeRect(0.0, 0.0, [MyPrintInfo paperSize].width, [MyPrintInfo paperSize].height)];
							 //[MyPrintInfo imageablePageBounds]];
	[printView setFont:[textEditor getTextEditorFont]];
	[printView setString:[textEditor getText]];
	
	
	NSPrintOperation *op;
	
	// copy the textview into the printview
	//NSRange textViewRange = NSMakeRange(0, [[textView textStorage] length]);
	//NSRange printViewRange = NSMakeRange(0, [[printView textStorage] length]);
	
	//[printView replaceCharactersInRange: printViewRange 
	//							withRTF:[textView RTFFromRange: textViewRange]];
	
	op = [NSPrintOperation printOperationWithView: printView printInfo: MyPrintInfo];
	
	[op setShowsPrintPanel: true];
	[op setShowsProgressPanel:true];
	
	
	NSPrintPanel *printPanel = [op printPanel];
    [printPanel setOptions:[printPanel options] | NSPrintPanelShowsPageSetupAccessory];    
	
	
	[self runModalPrintOperation: op delegate: nil didRunSelector: NULL 
					 contextInfo: NULL];
	
	[printView release];
	
}


- (BOOL) validateMenuItem:(NSMenuItem *)menuItem
{	
	
	//NSLog( @"validateMenuItem: %@, %@", menuItem, [menuItem title] );
	//NSLog(@"VALIDATE IN NSDOCUMENT");
	
	//	SEL act = [menuItem action];
	//	if ( act == @selector(cut:)
	

	if([[menuItem title] isEqualToString:@"Revert to Saved"])
	{
		return ([self isDocumentEdited] && ([self fileName] != nil));
	}
	if([[menuItem title] isEqualToString:@"Set caret on Secondary View"])
	{
		return ([textEditor getIsSplitted]);
	}
	
	
	//Set Line Endings state according to document EOLS
	if([[menuItem title] isEqualToString:@"Unix - OsX (LF)"])
	{
		if([textEditor GetEOLMode] == SC_EOL_LF)
			[menuItem setState:NSOnState];
		else
			[menuItem setState:NSOffState];
	}
	if([[menuItem title] isEqualToString:@"Mac (CR)"])
	{
		if([textEditor GetEOLMode] == SC_EOL_CR)
			[menuItem setState:NSOnState];
		else
			[menuItem setState:NSOffState];
	}
	if([[menuItem title] isEqualToString:@"Windows (CRLF)"])
	{
		if([textEditor GetEOLMode] == SC_EOL_CRLF)
			[menuItem setState:NSOnState];
		else
			[menuItem setState:NSOffState];
	}
	
	
	//Disable Comments if the document is not Syntax Type document (txt or others)
	if([self isSyntaxType] == false)
	{
		if ([menuItem action] == @selector(wxCommentLine:) ||
			[menuItem action] == @selector(wxUnCommentLine:))
		{
			//NSLog(@"VALIDATE IN NSDOCUMENT: Comments");
			return false;
		}
	}
	
	
	//Validate Import/Export menu
	if ([menuItem action] == @selector(wxImportOrcSco:) ||
		[menuItem action] == @selector(wxImportOrc:) ||
		[menuItem action] == @selector(wxImportSco:) ||
		[menuItem action] == @selector(wxExportOrcSco:) ||
		[menuItem action] == @selector(wxExportOrc:) ||
		[menuItem action] == @selector(wxExportSco:))
	{
		return ([[self fileType] isEqualToString:@"CSound Files"]);
	}
	
	//Set "Format Code" menu title
	if ([menuItem action] == @selector(wxFormatCode:))
	{
		if([self isSyntaxType] == false) return false;
		
		if([[textEditor getSelectedText] length] > 0)
		{
			[menuItem setTitle:@"Format Selected Text"];
		}
		else
			[menuItem setTitle:@"Format Line"];
	}
	
	
	//Validate "Save selected text to Repository"
	if([menuItem action] == @selector(wxSaveUserCodeToRepository:))
	{
		return ([[textEditor getSelectedText] length] > 0);
	}
	
	
	//Validate AdditionalFlags menu
	if([menuItem action] == @selector(CompileWithAdditionalFlagsClicked:) ||
	   [menuItem action] == @selector(CompileExternalWithAdditionalFlagsClicked:))
	{
		return ([[self fileType] isEqualToString:@"CSound Files"] ||
				[[self fileType] isEqualToString:@"CSound Orc Files"] ||
				[[self fileType] isEqualToString:@"CSound Sco Files"]);
	}
		
	
	return true;
	//return [super validateMenuItem:menuItem];
	
}

- (BOOL) revertToContentsOfURL:(NSURL *)absoluteURL ofType:(NSString *)typeName error:(NSError **)outError
{
	//NSLog(@"RevertToContentsOfURL");
	//[NSURL fileURLWithPath:filename]  

	BOOL success = [super revertToContentsOfURL:absoluteURL ofType:typeName error:outError];


	if (success) 
	{		
		if(_textContent != nil)
		{
			[textEditor setText: _textContent];
			[textEditor emptyUndoBuffer];
			[textEditor setSavePoint];
			[self updateChangeCount:NSChangeCleared];
			[_textContent release];
			_textContent = nil;
		}
    }
	
	return success;
}


- (BOOL) saveToURL:(NSURL *)absoluteURL ofType:(NSString *)typeName forSaveOperation:(NSSaveOperationType)saveOperation error:(NSError **)outError
{
	BOOL ret = [super saveToURL:absoluteURL ofType:typeName forSaveOperation:saveOperation error:outError];
	
	NSLog(@"Document type: %@", [self fileType]);
	
	if(currentFileType != [self fileType])
	{
		NSLog(@"Document type changed to: %@", [self fileType]);
		currentFileType = [self fileType];
		[self configureEditor];
		[textEditor refreshSyntax];
	}
	return ret;
}

//----------------------------------------------------------------------------------------------------------
// NSDOCUMENT OVERRIDES
//----------------------------------------------------------------------------------------------------------







#pragma mark - OrcSco
//----------------------------------------------------------------------------------------------------------
// ORC/SCO METHODS
//----------------------------------------------------------------------------------------------------------
- (bool) IsOrcScoFile
{
	if([[[[self fileURL] path] lowercaseString] hasSuffix:@".orc"] ||
	   [[[[self fileURL] path] lowercaseString] hasSuffix:@".sco"] ||
	   [[self fileType] isEqualToString:@"CSound Orc Files"] ||
	   [[self fileType] isEqualToString:@"CSound Sco Files"])
	{
		return true;
	}
	
	return false;	
}

- (void) lookForOrcScoFiles
{
	//NSLog(@"LOOKFOR:%@", [[[self fileURL] path] stringByDeletingLastPathComponent]);
	
	NSString *dir = [[[self fileURL] path] stringByDeletingLastPathComponent];
	NSArray *dirContents = [[NSFileManager defaultManager] contentsOfDirectoryAtPath:dir error:nil];
	//directoryContentsAtPath:bundleRoot];
	NSString* predicate = nil;
	
	if([[[[self fileURL] path] lowercaseString] hasSuffix:@".orc"])
		predicate = @"self ENDSWITH '.sco'";
	else 
		predicate = @"self ENDSWITH '.orc'";
	NSArray *onlyOrcSco = [dirContents filteredArrayUsingPredicate:[NSPredicate predicateWithFormat:predicate]];
	
	if(onlyOrcSco == nil) return;
	if([onlyOrcSco count] < 1) return;
	
	
	//Append files to the combobox
	for(NSString* file in onlyOrcSco)
	{
		[linkBoxComboBox addItemWithObjectValue:[NSString stringWithFormat:@"%@/%@",dir, file]];
	}
	
	
	//Try to select the same named file
	NSString* value = nil;
	NSString* currentFileName = [self displayName]; //[[[[self fileURL] path] lastPathComponent] stringByDeletingPathExtension]
	for(NSInteger index = 0; index < [linkBoxComboBox numberOfItems]; index++)
	{
		value = [[[linkBoxComboBox itemObjectValueAtIndex:index] lastPathComponent] stringByDeletingPathExtension];
		NSLog(@"AddFile:%@", value);
		
		if([value rangeOfString:[[self fileURL] path]].location != NSNotFound ||
		   [currentFileName rangeOfString:value].location != NSNotFound)
		{
			[linkBoxComboBox selectItemAtIndex:index];
			break;
		}
		
	}
	
}

- (IBAction)wxLinkBoxClear:(id)sender
{
	//[linkBoxComboBox removeAllItems];
	[linkBoxComboBox setStringValue:@""];
}

- (IBAction)wxLinkBoxBrowse:(id)sender
{
	//NSInteger tagSender = [(NSButton *)sender tag];
	//NSLog(@"%d",tagSender);
	
	NSOpenPanel *openPanel = [NSOpenPanel openPanel];
	
	[openPanel setResolvesAliases:YES];
	[openPanel setAllowsMultipleSelection:true];
	[openPanel setCanChooseFiles: true];
	[openPanel setCanChooseDirectories:false];
	
	NSString* linkFile = nil;
	if([[[[self fileURL] path] lowercaseString] hasSuffix:@".orc"])
		linkFile = @"sco";
	else 
		linkFile = @"orc";
	
	
	//SHEET WINDOW
	[openPanel beginSheetForDirectory:nil
								 file:nil
								types:[NSArray arrayWithObjects:linkFile,nil] //[NSArray arrayWithObjects:@"wav",@"aiff",@"aif",nil]
					   modalForWindow:[self windowForSheet]
						modalDelegate:self
					   didEndSelector:@selector(openPanelDidEnd:
												returnCode:
												contextInfo:)
						  contextInfo:@"Orc/Sco file linker"];
}

- (void)openPanelDidEnd:(NSOpenPanel *)panel returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	@try
	{
		if (returnCode == NSOKButton)
		{
			NSArray *filesToOpen = [panel URLs];
			
			NSInteger index = -1;
			for(NSURL* url in filesToOpen)
			{
				NSInteger index = [linkBoxComboBox indexOfItemWithObjectValue:[url path]];
				
				if(index == NSNotFound)
					[linkBoxComboBox addItemWithObjectValue:[url path]];
			}
			
			
			if(index != -1)
				[linkBoxComboBox selectItemAtIndex:index];
			else
				//Select the last added file
				[linkBoxComboBox selectItemAtIndex:[linkBoxComboBox numberOfItems] - 1];
			
			
		}
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> openPanelDidEnd:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
}


- (IBAction)wxLinkBoxOpenLink:(id)sender
{
	if([self IsOrcScoFile])
	{
		if([[NSFileManager defaultManager] fileExistsAtPath:[linkBoxComboBox stringValue]])
			[wxMAIN wxOpenOrcScoDocumentWithFilename:[linkBoxComboBox stringValue]];
	}
	
}
//----------------------------------------------------------------------------------------------------------
// ORC/SCO METHODS
//----------------------------------------------------------------------------------------------------------








#pragma mark - Timer Stuffs
//----------------------------------------------------------------------------------------------------------
// TIMER STUFFS (Timer + FindStructure + relative stuffs)
//----------------------------------------------------------------------------------------------------------
// WinXound use a timer to start the FindStructure method: the timer is fired after about 1.2 seconds (based on SCI_MODIFIED)
// wxGlobal.h define the interval (#define FIND_STRUCTURE_INTERVAL 6)
- (void) startTimer
{
	_Timer = [NSTimer scheduledTimerWithTimeInterval:0.2 target:self selector:@selector(timerFireMethod:) userInfo:nil repeats:YES];
}

- (void) stopTimer
{
	if (_Timer != nil)
	{
		if([_Timer isValid]) 
			[_Timer invalidate];
		_Timer = nil;
	}
}

- (void)timerFireMethod:(NSTimer*)theTimer
{
	//NSLog(@"TIMER FIRED");
	timerCounter ++;
	if(timerCounter >= FIND_STRUCTURE_INTERVAL)
	{
		timerCounter = 0;
		if(isChanged == true && [self isSyntaxType]) //Skip findStructure for Non-Syntax documents
		{
			[NSThread detachNewThreadSelector:@selector(findStructure:) toTarget:self withObject:[textEditor getText]];
			isChanged = false;
		}
	}
}

//----------------------------------------------------------------------------------------------------------
// TIMER STUFFS
//----------------------------------------------------------------------------------------------------------











#pragma mark - Scintilla Notifications
//----------------------------------------------------------------------------------------------------------
// SCINTILLA NOTIFICATIONS (and relative stuffs)
//----------------------------------------------------------------------------------------------------------

//SCN_UPDATEUI
//Either the text or styling of the document has changed or the selection range has changed. 
//Now would be a good time to update any container UI elements that depend on document or view state.
- (void) SCI_UPDATEUI_Notification: (NSNotification *) notification
{
	// - CHECK FOR BRACKET
	// - CURRENT WORD
	// - DISPLAY INTELLITIP	
	
	//NSLog(@"SCI_UPDATEUI_Notification inside wxDocument");
	
	if([self isSyntaxType] == false) return;
	
	
	//Check for matching bracket (update state)
	if([[wxDefaults valueForKey:@"EditorShowMatchingBracket"] boolValue] == true)
		[self checkForBracket];
	
	
	//Display the current Word description (Intellitip)
	@try 
	{
		NSString* currentWord = [textEditor getCurrentWord];
		
		if(currentWord == nil) return;
		if([currentWord length] < 2) return;
		
//		NSString* description = [wxMAIN getOpcodeValue: currentWord];
//		NSArray* items = [description componentsSeparatedByString:@";"];
//		if ([description length] > 0)
//		{
//			[intelliTipTitle setStringValue: [NSString stringWithFormat:@"[%@] - %@", currentWord, [items objectAtIndex:1]]];
//			[intelliTipParams setStringValue: [NSString stringWithFormat:@"Parameters: %@", [items objectAtIndex:2]]];
//		}
		[self setIntelliTipTextForOpcode:currentWord];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -> SCI_UPDATEUI_Notification Error: %@ - %@", [e name], [e reason]);
	}
	
	
//	//Autocompletion window
//	//Autocompletion check (line change)
//	curLine = tEditor.textEditor.GetLineNumberFromPosition(
//														   tEditor.textEditor.GetCaretPosition());
//	if (curLine != oldLine)
//	{
//		oldLine = curLine;
//		if (ListBoxAutoComplete.Visible)
//			ListBoxAutoComplete.Hide();
//	}
//	
//	//If Autocompletion is visible find current word in ListBox
//	if (ListBoxAutoComplete.Visible)
//	{
//		int index = ListBoxAutoComplete.FindString(gCurWord);
//		// Determine if a valid index is returned. Select the item if it is valid.
//		if (index != -1)
//			ListBoxAutoComplete.SetSelected(index, true);
//		
//	}
	
}

- (void) setIntelliTipTextForOpcode:(NSString*)opcode
{
	NSString* description = [wxMAIN getOpcodeValue: opcode];
	NSArray* items = [description componentsSeparatedByString:@";"];
	if ([description length] > 0)
	{
		[intelliTipTitle setStringValue: [NSString stringWithFormat:@"[%@] - %@", opcode, [items objectAtIndex:1]]];
		[intelliTipParams setStringValue: [NSString stringWithFormat:@"Parameters: %@", [items objectAtIndex:2]]];
	}
}

- (void) checkForBracket
{
	@try 
	{
		//Int32 caretpos = tEditor.textEditor.GetCaretPosition();
		NSInteger caretpos = [textEditor getCaretPosition];
		
		//if (tEditor.textEditor.GetCharAt(caretpos - 1) == ')')
		if([[textEditor getCharAt:(caretpos - 1)] isEqualToString:@")"])
		{
			//Int32 pos1 = tEditor.textEditor.BraceMatch(caretpos - 1, 0);
			NSInteger pos1 = [textEditor BraceMatch:(caretpos -1) maxReStyle:0];
			if (pos1 < 0)
			{
				//tEditor.textEditor.BraceHiglight(-1, -1);
				[textEditor BraceHiglight:-1 pos2:-1];
				return;
			}
			
			//tEditor.textEditor.BraceHiglight(pos1, caretpos - 1);
			[textEditor BraceHiglight:pos1 pos2:(caretpos - 1)];
		}
		//else if (tEditor.textEditor.GetCharAt(caretpos) == '(')
		else if ([[textEditor getCharAt:caretpos] isEqualToString:@"("])
		{
			//Int32 pos1 = tEditor.textEditor.BraceMatch(caretpos, 0);
			NSInteger pos1 = [textEditor BraceMatch:caretpos maxReStyle:0];
			if (pos1 < 0) return;
			
			//tEditor.textEditor.BraceHiglight(pos1, caretpos);
			[textEditor BraceHiglight:pos1 pos2:caretpos];
		}
		else
		{
			//tEditor.textEditor.BraceHiglight(-1, -1);
			[textEditor BraceHiglight:-1 pos2:-1];
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -> checkForBracket Error: %@ - %@", [e name], [e reason]);
	}
	
}


//SCN_MODIFIED
//This notification is sent when the text or styling of the document changes or is about to change. 
//You can set a mask for the notifications that are sent to the container with SCI_SETMODEVENTMASK. 
//The notification structure contains information about what changed, how the change occurred and 
//whether this changed the number of lines in the document. No modifications may be performed while 
//in a SCN_MODIFIED event.
- (void) SCI_MODIFIED_Notification: (NSNotification *) notification
{
	//NSLog(@"SCI_MODIFIED_NOTIFICATION inside wxDocument");
	
	//-SET UNDO/REDO STATE
	[self CheckUndoRedo];
	
	//-TIMER for FindStructure operation
	timerCounter = 0;
	isChanged = true;
	
}
//Check Undo and Redo State (notify NSDocument about dirty state)
- (void) CheckUndoRedo
{	
	if([[textEditor getPrimaryView] getGeneralProperty:SCI_GETMODIFY parameter:0])
	{
		[self updateChangeCount:NSChangeDone];
	}
	else 
	{
		[self updateChangeCount:NSChangeCleared];
	}
	
	
	@try
	{
		[toolbarUNDO setEnabled:[textEditor getCanUndo]];
		[toolbarREDO setEnabled:[textEditor getCanRedo]];
	}
	@catch (NSException * e){}
	//@catch(id ue) // this is equivalent to C++: catch(...) 
	@catch(id ue){}
	
}


//SCI_MOD_CONTAINER
//This is called by undo/redo if they contain EOLS modifications
- (void) SCI_MOD_CONTAINER_Notification: (NSNotification *) notification
{
	//Workaround for mod_container notification problem!
	//We must recall CheckUndoRedo after a little amount of time (100ms).
	//This method update the Undo and Redo toolbar buttons to the real 
	//Scintilla state (EOLS+CanUndo+CanRedo).
	
	//NSLog(@"wxDocument: SCI_MOD_CONTAINER_Notification");
	NSInteger token = [[[notification userInfo] objectForKey:@"token"] integerValue]; 
	
	if(token == 2)
		[NSTimer scheduledTimerWithTimeInterval:0.1 target:self selector:@selector(CheckTimer:) userInfo:nil repeats:NO];	
}
- (void)CheckTimer:(NSTimer*)theTimer
{
	NSLog(@"wxDocument: SCI_MOD_CONTAINER_CheckTimer");
	
	[self CheckUndoRedo];
	
	/*OLD: Remove it!
	@try
	{
		[self CheckUndoRedo];
		////[toolbarUNDO setEnabled:[textEditor getCanUndo]];
		////[toolbarREDO setEnabled:[textEditor getCanRedo]];
	}
	@catch (NSException * e) {}
	@catch(id ue){}
	*/
}

		 
//SCN_ZOOM
//This notification is generated when the user zooms the display using the keyboard or the SCI_SETZOOM method is called. 
//This notification can be used to recalculate positions, such as the width of the line number margin to maintain sizes 
//in terms of characters rather than pixels.
- (void) SCI_ZOOM_Notification: (NSNotification *) notification
{
	@try
	{
		//NSLog(@"ZOOM NOTIFICATION");
		NSInteger fontSize = [[textEditor getPrimaryView] getGeneralProperty: SCI_STYLEGETSIZE parameter: STYLE_DEFAULT];
		NSInteger percent = ((fontSize + [textEditor getZoomForView1]) * 100 / fontSize);
		
		if([textEditor getIsSplitted])
		{
			NSInteger fontSize2 = [[textEditor getSecondaryView] getGeneralProperty: SCI_STYLEGETSIZE parameter: STYLE_DEFAULT];
			NSInteger percent2 = ((fontSize2 + [textEditor getZoomForView2]) * 100 / fontSize2);
			[textFieldFontSize setStringValue:[NSString stringWithFormat:@"Zoom: %d%%/%d%%",percent, percent2]];
		}
		else 
		{
			[textFieldFontSize setStringValue:[NSString stringWithFormat:@"Zoom: %d%%",percent]];
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -> SCI_ZOOM_Notification Error: %@ - %@", [e name], [e reason]);
	}
	
}
		 

//LEFT MOUSE DOWN NOTIFICATION
- (void) SCI_LEFTMOUSE_Notification: (NSNotification *) notification
{
	//STORE CURSOR POSITION
	//NSLog(@"LEFT MOUSE DOWN");
	
	[_CursorPosition StoreCursorPos:[textEditor getCaretPosition]];
}


//LEFT MOUSE DOWN + MODIFIER KEY (ALT or CMD ???)
//Maintained as notification for future shortcuts (CTRL+CLICK) - 
//For the moment it is called by the context menu ("Open File")
- (void) SCI_LEFTMOUSEHYPERLINKS_Notification: (NSNotification *) notification
{
	//HYPERLINKS
	
	NSLog(@"HYPERLINKS");
	
	if([[textEditor getSelectedText] length] > 0) return;
	
	@try
	{	
		//Look only for strings style (csound language = 6)
		if(([textEditor GetStyleAt:[textEditor getCaretPosition]] == 6 && [_Language isEqualToString:@"csound"]) ||
		   ([textEditor GetStyleAt:[textEditor getCaretPosition]] == 6 && [_Language isEqualToString:@"lua"]) ||
		   ([textEditor GetStyleAt:[textEditor getCaretPosition]] == 3 && [_Language isEqualToString:@"python"]) ||
		   ([textEditor GetStyleAt:[textEditor getCaretPosition]] == 4 && [_Language isEqualToString:@"python"])) //single quoted string
		{
			NSString* stringChar = nil;
			NSString* mString = nil;
			NSInteger mStart = 0;
			NSInteger mEnd = 0;
			NSInteger mCaretPosition = [textEditor getCaretPosition]; //tEditor.textEditor.GetCaretPosition();
			NSInteger mCurrentLineNumber = [textEditor getCurrentLineNumber]; //tEditor.textEditor.GetCurrentLineNumber();
			NSInteger mLineStart = [textEditor getPositionFromLineNumber:mCurrentLineNumber];
			NSInteger mLineEnd = [textEditor GetLineEndPosition:[textEditor getCurrentLineNumber]];
			
			
			if([textEditor GetStyleAt:[textEditor getCaretPosition]] == 4 && [_Language isEqualToString:@"python"]) //single quoted string
			{
				stringChar = @"'";   
			}
			else
				stringChar = @"\"";
			
			
			//			for (Int32 c = mCaretPosition;
			//				 c > tEditor.textEditor.GetPositionFromLineNumber(mCurrentLineNumber);
			//				 c--)
			for (NSInteger c = mCaretPosition;
				 c >= mLineStart;
				 c--)
			{
				//if (tEditor.textEditor.GetCharAt(c) == '"')
				if([[textEditor getCharAt:c] isEqualToString:stringChar])
				{
					mStart = c + 1;
					break;
				}
			}
			
			//			for (Int32 c = mCaretPosition;
			//				 c > tEditor.textEditor.GetPositionFromLineNumber(mCurrentLineNumber);
			//				 c++)
			for (NSInteger c = mCaretPosition;
				 c <= mLineEnd;
				 c++)
			{
				//if (tEditor.textEditor.GetCharAt(c) == '"')
				if([[textEditor getCharAt:c] isEqualToString:stringChar])
				{
					mEnd = c;
					break;
				}
			}
			
			if ((mEnd - mStart) > 0 && mStart >= 0)
			{
				//mString = tEditor.textEditor.GetText().Substring(mStart, mEnd - mStart);
				//mString = [[textEditor getText] substringWithRange:NSMakeRange(mStart, mEnd-mStart)];
				mString = [textEditor getTextRange:NSMakeRange(mStart, mEnd-mStart)];
				[self OpenHyperLinks:mString];
			}
			
		}
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> SCI_LEFTMOUSEHYPERLINKS_Notificaiton:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];

	}
	
}

- (void) OpenHyperLinks:(NSString*)mString
{
	
	NSFileManager *fileManager = [NSFileManager defaultManager];

	//Search for current file directory
	//if (File.Exists(Path.GetDirectoryName(ActiveEditor.FileName) + "\\" + mString))
//	NSString* path = [NSString stringWithFormat:@"%@/%@",
//					  [[self fileName] stringByDeletingLastPathComponent],
//					  mString];
	if([fileManager fileExistsAtPath:[NSString stringWithFormat:@"%@/%@",
									  [[self fileName] stringByDeletingLastPathComponent],
									  mString]])
	{
		//mString = Path.GetDirectoryName(ActiveEditor.FileName) + "\\" + mString;
		mString = [NSString stringWithFormat:@"%@/%@",
				   [[self fileName] stringByDeletingLastPathComponent],
				   mString];
	}
	//Search for full path
	//else if (File.Exists(mString))
	else if ([fileManager fileExistsAtPath:mString])
	{
		//check for slash
		//mString = mString.Replace("/", "\\"); //WINDOWS SPECIFIC ONLY
	}
	//Search for INCDIR
	//else if (File.Exists(wxGlobal.Settings.Directory.INCDIR + "\\" + mString))
	else if ([fileManager fileExistsAtPath:[NSString stringWithFormat:@"%@/%@",
											[wxDefaults valueForKey:@"INCDIRPath"],
											mString]])
	{
		//mString = wxGlobal.Settings.Directory.INCDIR + "\\" + mString;
		mString = [NSString stringWithFormat:@"%@/%@",
				   [wxDefaults valueForKey:@"INCDIRPath"],
				   mString];
	}
	//Search for SFDIR
	//else if (File.Exists(wxGlobal.Settings.Directory.SFDIR + "\\" + mString))
	else if ([fileManager fileExistsAtPath:[NSString stringWithFormat:@"%@/%@",
											[wxDefaults valueForKey:@"SFDIRPath"],
											mString]])
	{
		mString = [NSString stringWithFormat:@"%@/%@",
				   [wxDefaults valueForKey:@"SFDIRPath"],
				   mString];
	}
	//Search for SSDIR
	//else if (File.Exists(wxGlobal.Settings.Directory.SSDIR + "\\" + mString))
	else if ([fileManager fileExistsAtPath:[NSString stringWithFormat:@"%@/%@",
											[wxDefaults valueForKey:@"SSDIRPath"],
											mString]])
	{
		mString = [NSString stringWithFormat:@"%@/%@",
				   [wxDefaults valueForKey:@"SSDIRPath"],
				   mString];
	}
	//Search for SADIR
	//else if (File.Exists(wxGlobal.Settings.Directory.SADIR + "\\" + mString))
	else if ([fileManager fileExistsAtPath:[NSString stringWithFormat:@"%@/%@",
											[wxDefaults valueForKey:@"SADIRPath"],
											mString]])
	{
		mString = [NSString stringWithFormat:@"%@/%@",
				   [wxDefaults valueForKey:@"SADIRPath"],
				   mString];
	}
	//Search for MFDIR
	//else if (File.Exists(wxGlobal.Settings.Directory.MFDIR + "\\" + mString))
	else if ([fileManager fileExistsAtPath:[NSString stringWithFormat:@"%@/%@",
											[wxDefaults valueForKey:@"MFDIRPath"],
											mString]])
	{
		mString = [NSString stringWithFormat:@"%@/%@",
				   [wxDefaults valueForKey:@"MFDIRPath"],
				   mString];
	}
	
	//NSLog(mString);
	
	//if (!File.Exists(mString)) return;
	if(![fileManager fileExistsAtPath:mString]) return;
	
	//OPEN AUDIO FILES
//	if (mString.ToLower().EndsWith(".wav") ||
//		mString.ToLower().EndsWith(".aif") ||
//		mString.ToLower().EndsWith(".aiff"))
	if([[mString lowercaseString] hasSuffix:@".wav"] ||
	   [[mString lowercaseString] hasSuffix:@".aif"] ||
	   [[mString lowercaseString] hasSuffix:@".aiff"])
	{
		//if (!string.IsNullOrEmpty(wxGlobal.Settings.Directory.WaveEditor))
		if([wxDefaults valueForKey:@"WaveEditorPath"] != nil &&
		   [[wxDefaults valueForKey:@"WaveEditorPath"] length] > 0)
		{
			//if (File.Exists(wxGlobal.Settings.Directory.WaveEditor))
			if([fileManager fileExistsAtPath:[wxDefaults valueForKey:@"WaveEditorPath"]])
			{
//				System.Diagnostics.Process.Start(
//												 wxGlobal.Settings.Directory.WaveEditor,
//												 "\"" + mString + "\"");
				//[[NSWorkspace sharedWorkspace] launchApplication:
				// [NSString stringWithFormat:@"%@ @"/Applications/QuickTime Player.app"];
				
				//-a /Applications/QuteCsound.app --args "/Users/teto/Desktop/113.csd"
				NSTask* task = [[NSTask alloc] init];
				[task setLaunchPath:@"/usr/bin/open"];
				//[task setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
				[task setArguments:[NSArray arrayWithObjects:
									@"-a", 
									[wxDefaults valueForKey:@"WaveEditorPath"],
									mString,
									nil]]; 
				
				[task launch];
				[task release];
				
				

			}
			else
			{
//				MessageBox.Show("Cannot find External Wave Editor Path!" + newline +
//								"Please select a valid path in File->Settings->Wave Editor executable",
//								"Compiler error!",
//								MessageBoxButtons.OK,
//								MessageBoxIcon.Error);
				[wxMAIN ShowMessage:@"Cannot find External Wave Editor Path!" 
					informativeText:@"Please select a valid path in File->Settings->Wave Editor executable" 
					  defaultButton:@"OK" 
					alternateButton:nil 
						otherButton:nil];
			}
		}
		else
		{
			//System.Diagnostics.Process.Start("wmplayer", "\"" + mString + "\"");
			//@"/Applications/QuickTime Player.app"
			NSTask* task = [[NSTask alloc] init];
			[task setLaunchPath:@"/usr/bin/open"];
			//[task setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
			[task setArguments:[NSArray arrayWithObjects:
								@"-a", 
								@"/Applications/QuickTime Player.app",
								mString,
								nil]]; 
			
			[task launch];
			[task release];
		}
	}
	
	//OPEN MIDI FILES
	//else if (mString.ToLower().EndsWith(".mid"))
	else if ([[mString lowercaseString] hasSuffix:@".mid"])
	{
		//System.Diagnostics.Process.Start("wmplayer", "\"" + mString + "\"");
		NSTask* task = [[NSTask alloc] init];
		[task setLaunchPath:@"/usr/bin/open"];
		//[task setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
		[task setArguments:[NSArray arrayWithObjects:
							@"-a", 
							@"/Applications/QuickTime Player.app",
							mString,
							nil]]; 
		
		[task launch];
		[task release];
	}
	
	//TRY TO OPEN ALLOWED FILES WITH WINXOUND
	else if([[mString lowercaseString] hasSuffix:@".csd"] ||
			[[mString lowercaseString] hasSuffix:@".orc"] ||
			[[mString lowercaseString] hasSuffix:@".sco"] ||
			[[mString lowercaseString] hasSuffix:@".inc"] ||
			[[mString lowercaseString] hasSuffix:@".udo"] ||
			[[mString lowercaseString] hasSuffix:@".py"] ||
			[[mString lowercaseString] hasSuffix:@".pyw"] ||
			[[mString lowercaseString] hasSuffix:@".lua"] ||
			[[mString lowercaseString] hasSuffix:@".txt"] ||
			[[mString lowercaseString] hasSuffix:@".h"])
	{
		[wxMAIN wxOpenDocumentWithFilename:mString];
	}

}


// RIGHT MOUSE DOWN NOTIFICATION (CONTEXT MENU STUFFS)
- (void) SCI_RIGHTMOUSE_Notification: (NSNotification *) notification
{
	//NSLog(@"RIGHTMOUSE NOTIFICATION");
	
	//CUT-COPY-PASTE
	BOOL enabled = ([[textEditor getSelectedText] length] > 0);
	NSMenuItem *itemCut = [contextMenu itemWithTag:0];
	NSMenuItem *itemCopy = [contextMenu itemWithTag:1];
	NSMenuItem *itemPaste = [contextMenu itemWithTag:2];
	[itemCut setEnabled:enabled];
	[itemCopy setEnabled:enabled];
	[itemPaste setEnabled:[textEditor getCanPaste]];
	
	
	//OTHERS
	NSMenuItem *itemComment = [contextMenu itemWithTag:3];
	NSMenuItem *itemDefinition = [contextMenu itemWithTag:5];
	NSMenuItem *itemReference = [contextMenu itemWithTag:6];
	NSMenuItem *itemOpenFile = [contextMenu itemWithTag:7];
	NSMenuItem *itemHelp = [contextMenu itemWithTag:8];
	
	[itemDefinition setTitle:[NSString stringWithFormat:@"Go to definition of ..."]];
	[itemReference setTitle:[NSString stringWithFormat:@"Go to reference of ..."]];
	[itemOpenFile setEnabled:false];
	[itemComment setEnabled: true];
	[itemDefinition setEnabled:true];
	[itemReference setEnabled:true];
	[itemHelp setEnabled:true];
	
	if([self isSyntaxType])
	{
		if([wxMAIN getOpcodeValue:[textEditor getCurrentWord]] != nil)
		{
			[itemHelp setTitle:[NSString stringWithFormat:@"\"%@\" Help", [textEditor getCurrentWord]]];
			[itemDefinition setEnabled:false];
			[itemReference setEnabled:false];
		}
		else
		{
			[itemHelp setTitle:@"Opcodes Help"];
			[itemDefinition setTitle:[NSString stringWithFormat:@"Go to definition of %@", [textEditor getCurrentWord]]];
			[itemReference setTitle:[NSString stringWithFormat:@"Go to reference of %@", [textEditor getCurrentWord]]];
		}	
		
		//if([textEditor GetStyleAt:[textEditor getCaretPosition]] == 6)
		NSLog(@"textstyle: %d", [textEditor GetStyleAt:[textEditor getCaretPosition]]);
		if(([textEditor GetStyleAt:[textEditor getCaretPosition]] == 6 && [_Language isEqualToString:@"csound"]) ||
		   ([textEditor GetStyleAt:[textEditor getCaretPosition]] == 6 && [_Language isEqualToString:@"lua"]) ||
		   ([textEditor GetStyleAt:[textEditor getCaretPosition]] == 3 && [_Language isEqualToString:@"python"]) ||
		   ([textEditor GetStyleAt:[textEditor getCaretPosition]] == 4 && [_Language isEqualToString:@"python"]))
		{
			[itemOpenFile setEnabled:true];
		}
	}
	else
	{
		[itemComment setEnabled: false];
		[itemDefinition setEnabled:false];
		[itemReference setEnabled:false];
		[itemOpenFile setEnabled:false];
		[itemHelp setEnabled:true];
	}
	
	[_CursorPosition StoreCursorPos:[textEditor getCaretPosition]];
	
}


//ESCAPE KEY DOWN - STOP COMPILER
- (void) SCI_ESCAPEKEYDOWN_Notification: (NSNotification *) notification
{
	//[self wxStopCompiler:self];
	if([buttonStopCompiler isHidden] == false)
		[buttonStopCompiler performClick:self];
}


//SCI_LEFTMOUSE_QUOTES_Notification (Select string with quotes)
- (void) SCI_LEFTMOUSE_QUOTES_Notification: (NSNotification *) notification
{

	NSLog(@"SELECT QUOTES");
	
	NSInteger curPos = [[[notification userInfo] objectForKey:@"position"] integerValue]; 
	
	if(([textEditor GetStyleAt:curPos] == 6 && [_Language isEqualToString:@"csound"]) ||
	   ([textEditor GetStyleAt:curPos] == 6 && [_Language isEqualToString:@"lua"]) ||
	   ([textEditor GetStyleAt:curPos] == 3 && [_Language isEqualToString:@"python"]) ||
	   ([textEditor GetStyleAt:curPos] == 4 && [_Language isEqualToString:@"python"]))
	{

		NSString* stringChar = nil;
		
		
		if([textEditor GetStyleAt:curPos] == 4 && [_Language isEqualToString:@"python"]) //single quoted string
		{
			stringChar = @"'";   
		}
		else
			stringChar = @"\"";
		
		
		NSPoint p = [textEditor getQuotesPosition:curPos withQuote:stringChar];
		if(p.x > 0 && p.y > 0)
		{
			[textEditor setSelection:p.x-1 end:p.y+1];
		}

		
	}
}

//----------------------------------------------------------------------------------------------------------
// SCINTILLA NOTIFICATIONS
//----------------------------------------------------------------------------------------------------------















#pragma mark - Split View Overrides
//----------------------------------------------------------------------------------------------------------
// SPLIT VIEW OVERRIDES
//----------------------------------------------------------------------------------------------------------
- (CGFloat) splitView:(NSSplitView *)splitView constrainMinCoordinate:(CGFloat)proposedMinimumPosition ofSubviewAt:(NSInteger)dividerIndex
{
	//NSLog(@"MIN");
	return proposedMinimumPosition + SplitMargins;
}

- (CGFloat) splitView:(NSSplitView *)splitView constrainMaxCoordinate:(CGFloat)proposedMaximumPosition ofSubviewAt:(NSInteger)dividerIndex
{
	//NSLog(@"MAX");
	return proposedMaximumPosition - SplitMargins;
}

//- (void) splitViewDidResizeSubviews:(NSNotification *)notification
//{
//	if([mainSplitWindow isSubviewCollapsed:treeViewStructureHost])
//	{
//		//TODO: CHECK COLLAPSED STATE ACCORDIND TO THE MENU (SHOW EXPLORER)
//	}
//}

- (BOOL) splitView:(NSSplitView *)splitView canCollapseSubview:(NSView *)subview
{
	if(subview == treeViewStructureHost)
	{
		//NSLog(@"istreeview");
		return true;
	}
	else return false;
}

//- (BOOL) splitView:(NSSplitView *)splitView shouldHideDividerAtIndex:(NSInteger)dividerIndex
//{
//	return true;
//}

- (void) splitView:(NSSplitView *)splitView resizeSubviewsWithOldSize:(NSSize)oldSize
{
	//NSLog(@"resizeSubviewsWithOldSize");
	if (splitView == mainSplitWindow) {
        // Here is how you customize the tiling behavior of a split view.
        // You start by setting the width of the left subview and
        // make the right subview fill in the rest of the split view
        // 
        // someValueOfYourChoosing is what ever you want it to be
        // withing the bounds of the splitview, you can implement the same
        // constraints here to agree with the constraints you placed on
        // dividers being dragged. You have to keep in mind that a window
        // doesn't care about the constraints placed here, so constrain
        // the window and the superview to mySplitView first before
        // thinking about how to constrain dimensions here. 
		
		float dividerThickness = [splitView dividerThickness];
		
		NSRect newFrame = [splitView frame];//[splitView frame];
		NSRect leftFrame = [treeViewStructureHost frame];
		NSRect rightFrame = [codeView frame];
		
		leftFrame.size.width = (leftFrame.size.width < SplitMargins ? SplitMargins : leftFrame.size.width); 
		leftFrame.size.height = newFrame.size.height;
		leftFrame.origin = NSMakePoint(0,0);
		
		rightFrame.size.width = newFrame.size.width - leftFrame.size.width - dividerThickness;
		rightFrame.size.height = newFrame.size.height;
		rightFrame.origin.x = leftFrame.size.width + dividerThickness;
		
		if(rightFrame.size.width < SplitMargins) rightFrame.size.width = SplitMargins;
		
		[treeViewStructureHost setFrame:leftFrame];
		// remember not to blindly set the frame of a view you think
		// is the right subview. In this case you make sure that
		// rightTableView is the correct subview to resize by observing
		// what gets logged for the superview description
		[codeView setFrame:rightFrame];
		//[CompilerSplitWindow adjustSubviews];
		[splitView adjustSubviews];
    }
	else 
	{
		
		//float dividerThickness = [splitView dividerThickness];
		
		NSRect newFrame = [splitView frame]; 
		NSRect upFrame = [upView frame];
		NSRect downFrame = [downView frame];
		
		////upFrame.size.width = newFrame.size.width;
        //upFrame.size.height = (upFrame.size.height < SplitMargins ? SplitMargins : upFrame.size.height); 
		//upFrame.size.height = newFrame.size.height - downFrame.size.height - dividerThickness;
		
		if(upFrame.size.height < SplitMargins) upFrame.size.height = SplitMargins;
		
		//downFrame.size.width = newFrame.size.width;
		//if(downFrame.size.height <= SplitMargins)
		//{
		//	NSInteger height = newFrame.size.height - upFrame.size.height - dividerThickness;
		//	downFrame.size.height = (height <= SplitMargins ? SplitMargins : height);
		//}
		
		//downFrame.origin.y = leftFrame.size.width + dividerThickness;
		if(downFrame.size.height < SplitMargins) downFrame.size.height = SplitMargins;
		//downFrame.size.height = (downFrame.size.height <= SplitMargins ? SplitMargins : downFrame.size.height);
		downFrame.size.height = SplitMargins;
		
        [upView setFrame:upFrame];
        [downView setFrame:downFrame];
		[splitView adjustSubviews];
	}
	
}

//- (CGFloat) splitView:(NSSplitView *)splitView constrainSplitPosition:(CGFloat)proposedPosition ofSubviewAt:(NSInteger)dividerIndex
//{
//	if(dividerIndex == 0)
//	return proposedPosition + 100;
//}

//- (void) splitViewWillResizeSubviews:(NSNotification *)notification
//{
//	NSLog(@"splitViewWillResizeSubviews");
//}


//-(NSRect)splitView:(NSSplitView *)splitView additionalEffectiveRectOfDividerAtIndex:(NSInteger)dividerIndex {
//	return [[self resizeView] convertRect:[[self resizeView] bounds] toView:splitView]; 
//}

//----------------------------------------------------------------------------------------------------------
// SPLIT VIEW OVERRIDES
//----------------------------------------------------------------------------------------------------------














#pragma mark - IB Actions
//----------------------------------------------------------------------------------------------------------
// IB ACTIONS
//----------------------------------------------------------------------------------------------------------
//- (IBAction)wxUndo:(id)sender
//{
//	NSLog(@"wxUNDO:");
//	[textEditor PerformUndo];
//}
//- (IBAction)wxRedo:(id)sender
//{
//	NSLog(@"wxUNDO:");
//	[textEditor PerformRedo];
//}
//- (IBAction)wxDelete:(id)sender
//{
//	//[mOwner setStringProperty:SCI_REPLACESEL parameter:0 value:@""];
//	if([[textEditor getSelectedText] length] > 0)
//		[textEditor setSelectedText:@""];
//}
//- (IBAction)wxCut:(id)sender
//{
//	[textEditor PerformCut];
//}
////- (IBAction)wxCopy:(id)sender
////{
////	[textEditor PerformCopy];
////}
//- (IBAction)wxPaste:(id)sender
//{
//	[textEditor PerformPaste];
//}


- (IBAction)wxShowFullCode:(id)sender
{
	[textEditor RemoveSplit];
	[self SCI_ZOOM_Notification:nil];
}
- (IBAction)wxSplitHorizontal:(id)sender
{
	//[textEditor RemoveSplit];
	[textEditor Split];
	if([textEditor getIsSplitted])
	{
		[textEditor setFirstVisibleLineAtView:
		 [textEditor getFirstVisibleLineAtView:1] view:2];
	}
	[self SCI_ZOOM_Notification:nil];
}
- (IBAction)wxSplitHorizontalOrcSco:(id)sender
{
	//[textEditor RemoveSplit];
	[textEditor Split];
	[self showOrcSco];
	[self SCI_ZOOM_Notification:nil];
}
- (IBAction)wxSplitVertical:(id)sender
{
	//[textEditor RemoveSplit];
	[textEditor SplitVertical];
	if([textEditor getIsSplitted])
	{
		[textEditor setFirstVisibleLineAtView:
		 [textEditor getFirstVisibleLineAtView:1] view:2];
	}
	[self SCI_ZOOM_Notification:nil];
}
- (IBAction)wxSplitVerticalOrcSco:(id)sender
{
	//TODO:
	//NSLog(@"SplitVerticalOrcSco:");
	//[textEditor RemoveSplit];
	[textEditor SplitVertical];
	[self showOrcSco];
	[self SCI_ZOOM_Notification:nil];
}
- (IBAction)wxInsertRemoveBookmark:(id)sender
{
	if(sender != nil)
		[textEditor InsertRemoveBookmark];
	
	@try 
	{
		//Refresh Bookmarks node (for TreeViewStructure)
		NSInteger mBookLine = 0;
		NSInteger CurLine = 0;
		NSInteger mIndex = 0;
		NSString* mText = nil;
		NSString* mExtendedText = nil;
		
		
		wxNode* node = nil;	
		node = [self retrieveNodeByName:@"Bookmarks"];
		
		[node.children removeAllObjects];
		wxNode* nodeBookmarks = nil;
		
		do
		{
			//mBookLine = textEditor.PrimaryView.MarkerNext(CurLine, 1);
			mBookLine = [textEditor MarkerNext:CurLine markerMask:1];
			if (mBookLine == -1) break;
			
			mIndex += 1;
			//mText = textEditor.PrimaryView.GetLine(mBookLine);
			mExtendedText = [textEditor getTextOfLine:mBookLine];
			mText = [self parseString:mExtendedText];
			
			if([mText length] == 0)
			{
				mText = @"...";
			}
			
			//mText = Regex.Replace(mText, @"\s+", " ").Trim();
			//ListBoxBookmarks.Items.Add(String.Format("{0:G}", mIndex) + ". " + mText);
			
			//[_BookmarksFolder addObject:mText];
			nodeBookmarks = [[wxNode alloc] init];
			nodeBookmarks.name = mText;
			nodeBookmarks.extendedname = mExtendedText;
			[node.children addObject:nodeBookmarks];
			[nodeBookmarks release];
			
			CurLine = mBookLine + 1;
		}
		while (true);
		
		[self refreshExplorer];
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxInsertRemoveBookmark:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
}

- (IBAction)wxRemoveAllBookmarks:(id)sender
{
	[textEditor RemoveAllBookmarks];
	
	//[_BookmarksFolder removeAllObjects];
	wxNode* node = 	[self retrieveNodeByName:@"Bookmarks"];
	if(node != nil)
	{
		[node.children removeAllObjects];
		[self refreshExplorer];
	}
}

- (IBAction)wxGotoNextBookmark:(id)sender
{
	[textEditor GoToNextBookmark];
}

- (IBAction)wxGotoPreviousBookmark:(id)sender
{
	[textEditor GoToPreviousBookmark];
}

- (IBAction)wxTreeViewMouseClick:(id)sender
{
	@try
	{
		wxNode* item = [treeViewStructure itemAtRow:[treeViewStructure selectedRow]];
		
		NSInteger ret = -1;
		
		
		if ([item.name isEqualToString: @"Bookmarks"])
		{
			//DO NOTHING
		}
		
		else if ([[[treeViewStructure parentForItem:item] name] isEqualToString:@"Bookmarks"])
		{
			//NSLog(@"INSIDE BOOKMARKS");
			NSInteger CurLine = 0;
			NSInteger BookLine = 0;
			NSInteger mIndex = [treeViewStructure selectedRow] - [treeViewStructure rowForItem:[treeViewStructure parentForItem:item]] - 1;
			
			for (NSInteger Ciclo = 0; Ciclo < mIndex + 1; Ciclo++)
			{
				//BookLine = textEditor.GetFocusedEditor.MarkerNext(CurLine, 1);
				BookLine = [textEditor MarkerNext:CurLine markerMask:1];
				CurLine = BookLine + 1;
			}
			
			//ListBoxBookmarks.SelectedIndex = -1;
			//textEditor.GetFocusedEditor.GotoLine(BookLine);
			[textEditor GoToLine:BookLine];
			ret = BookLine;
		}
		
		else if ([[[treeViewStructure parentForItem:item] name] isEqualToString:@"<CsInstruments>"])
		{
			////[textEditor GoToLine:[textEditor getLineNumberFromPosition:item.location]];
			NSInteger mStart = 0;
			do 
			{
				ret = [textEditor FindTextEx:item.extendedname
							  MatchWholeWord:true 
								   MatchCase:true 
								  IsBackward:false 
								  SelectText:false 
								 ShowMessage:false 
									 SkipRem:false 
									   start:mStart 
										 end:-1
									useRegEx:false];
				
				if(ret > -1)
				{
					NSString* textOfLine = [[textEditor getTextOfLine:[textEditor getLineNumberFromPosition:ret]] 
											stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
					
					//NSInteger length = [textOfLine length];
					
					//NSLog(@"Scintilla lineLength: %d - Node line length: %d", length, [item.extendedname length]);
					
					if([textOfLine length] == [item.extendedname length])
					{
						//[textEditor setCaretPosition:item.location];
						[textEditor GoToLine:[textEditor getLineNumberFromPosition:ret]];
						ret = 1;
						break;
					}
					mStart = ret + 5;
				}
				else
				{
					ret = -1;
					break;
				}
				
			}
			while (true);
			
		}
		
		else if ([[[treeViewStructure parentForItem:item] name] isEqualToString:@"<CsScore>"] &&
				 [item.name hasPrefix:@"s"])
		{
			//if([item.name hasPrefix:@"s"])
			{
				NSInteger cycles = [[item.name substringFromIndex:2] integerValue];
				NSInteger mStart = 0;
				
				for(NSInteger c = 0; c < cycles; c++)
				{
					ret = [textEditor FindTextEx:item.extendedname //item.name
								  MatchWholeWord:true 
									   MatchCase:true 
									  IsBackward:false 
									  SelectText:false 
									 ShowMessage:false 
										 SkipRem:false 
										   start:mStart 
											 end:-1
										useRegEx:false];
					if(ret > -1)
					{
						mStart = ret + 2;
					}
					else break;
				}
				if(ret > -1)
				{
					[textEditor GoToLine:[textEditor getLineNumberFromPosition:ret]];
				}
			}
		}
		
		else if([item.extendedname length] > 0)
		{
			NSInteger mStart = 0;
			NSInteger mEnd = -1;
			
			//BOOL backward = false;
			if ([[[treeViewStructure parentForItem:item] name] isEqualToString:@"<CsScore>"])
			{
				//Search backward
				//backward = true;
				mStart = [textEditor getTextLength];
				mEnd = 0;
			}
			
			ret = [textEditor FindTextEx:item.extendedname
						  MatchWholeWord:true 
							   MatchCase:true 
							  IsBackward:false 
							  SelectText:true 
							 ShowMessage:false 
								 SkipRem:false 
								   start:mStart 
									 end:mEnd
								useRegEx:false];
		}
		
		if(ret > -1)
		{
			[textEditor SelectLine:[textEditor getCurrentLineNumber] SetAsFirstVisibleLine:true];
			//[textEditor setFirstVisibleLine:[textEditor getCurrentLineNumber] -1];
		}
		
		[textEditor setFocus];
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxTreeViewMouseClick:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
}


- (IBAction)wxShowHideWhiteSpaces:(id)sender
{
	[textEditor setShowSpaces:![textEditor getShowSpaces]];
}
- (IBAction)wxShowHideLineEndings:(id)sender
{
	[textEditor setShowEOLMarker:![textEditor getShowEOLMarker]];
}


- (IBAction)wxGoToLine:(id)sender
{
	if(panelGoToLine != nil)
	{
		[NSApp beginSheet:panelGoToLine 
		   modalForWindow:[self windowForSheet]
			modalDelegate:self 
		   didEndSelector:nil  //@selector(sheetDidFinish:returnCode:contextInfo:) 
			  contextInfo:nil];
		//[fontPanel makeKeyAndOrderFront:self];
	}
}

- (IBAction)wxGoToLineOK:(id)sender
{
	//[NSApp endSheet:panelGoToLine returnCode: NSOKButton];
	[panelGoToLine close];
	[NSApp endSheet:panelGoToLine];
	
	[textEditor GoToLine:[panelGoToLineTextField intValue] - 1];
	[textEditor SelectLine:[textEditor getCurrentLineNumber] SetAsFirstVisibleLine:true];
}

- (void)goToLine:(NSInteger) lineNumber
{
	[textEditor GoToLine:lineNumber - 1];
	[textEditor SelectLine:[textEditor getCurrentLineNumber] SetAsFirstVisibleLine:false];
}

- (IBAction)wxGoToLineCANCEL:(id)sender
{
	[panelGoToLine close];
	[NSApp endSheet:panelGoToLine];
}

//- (void)sheetDidFinish:(NSWindow *)sheet returnCode:(NSInteger)returnCode contextInfo:(void *)contextInfo;
//{
//	NSLog(@"Return code %d", returnCode);
//	//[sheet close];
//	//[NSApp endSheet:sheet];
//	[sheet orderOut:self]; ???
//}


- (IBAction)wxFindAndReplace:(id)sender
{	
	//Show Find and Replace Window
	[wxFIND showFindAndReplace];
}

- (IBAction)wxFindSetSelection:(id)sender
{
	if([[textEditor getSelectedText] length] > 0)
	{
		[wxFIND setSelection:[textEditor getSelectedText]];
	}
}

- (IBAction)wxFindNext:(id)sender
{
	[wxFIND findDOWN];
}

- (IBAction)wxFindPrevious:(id)sender
{
	[wxFIND findUP];
}

- (IBAction)wxJumpToSelection:(id)sender
{
	if([[textEditor getSelectedText] length] > 0)
	{
		[textEditor ScrollCaret];
		//[textEditor GoToLine:[textEditor getLineNumberFromPosition:[textEditor getSelectionStart]]];
	}
}


- (IBAction)wxShowCode:(id)sender
{
	//SplitMargins
	[compilerSplitWindow setPosition:[compilerSplitWindow frame].size.height - SplitMargins ofDividerAtIndex:0];
}
- (IBAction)wxShowCompiler:(id)sender
{
	//[compilerSplitWindow setPosition:SplitMargins + 100 ofDividerAtIndex:0];
	[compilerSplitWindow setPosition:SplitMargins ofDividerAtIndex:0];
}


- (IBAction)wxShowOpcodeHelp:(id)sender
{
	[wxMAIN showOpcodeHelp:[textEditor getCurrentWord]];
}

- (IBAction)wxCommentLine:(id)sender
{
	@try
	{
		
		NSString* languagecomment = nil;
		
		if([[self fileType] isEqualToString:@"CSound Files"] ||
		   [[self fileType] isEqualToString:@"CSound Orc Files"] ||
		   [[self fileType] isEqualToString:@"CSound Sco Files"] ||
		   [[self fileType] isEqualToString:@"CSound Cabbage Files"])
			languagecomment = @";";
		else if([[self fileType] isEqualToString:@"Python Files"])
			languagecomment = @"#";
		else if([[self fileType] isEqualToString:@"Lua Files"])
			languagecomment = @"--";
		else return;
		
		//GetFocusedEditor.LineFromPosition(GetFocusedEditor.GetSelectionStart());
		NSInteger mLineStart = [textEditor getLineNumberFromPosition:[textEditor getSelectionStart]];
		
		//GetFocusedEditor.LineFromPosition(GetFocusedEditor.GetSelectionEnd());
		NSInteger mLineEnd = [textEditor getLineNumberFromPosition:[textEditor getSelectionEnd]];
		
		
		//Disable comment if the end of the selection (caret)
		//is located at the start of the line
		//if (GetFocusedEditor.GetSelText().Length > 0)
		if([[textEditor getSelectedText] length] > 0)
		{
			if (mLineEnd > mLineStart) //if multiple lines selected
			{
				NSInteger curPosition =
					[textEditor getSelectionEnd] -						  //GetFocusedEditor.GetSelectionEnd() -
					[textEditor getPositionFromLineNumber:mLineEnd];      //GetFocusedEditor.PositionFromLine(mLineEnd);
				
				if (curPosition == 0)
				{
					mLineEnd -= 1;
					if (mLineEnd < mLineStart)
						mLineEnd = mLineStart;
				}
			}
		}
		
		
		//for (Int32 mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
		for(NSInteger mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
		{
			//GetFocusedEditor.InsertText(GetFocusedEditor.PositionFromLine(mLine), 
			//							languagecomment);
			[textEditor InsertText:[textEditor getPositionFromLineNumber:mLine] text:languagecomment];
		}
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxCommentLine:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
}

- (IBAction)wxUnCommentLine:(id)sender
{
	@try
	{
		NSString* languagecomment = nil;
		
		if([[self fileType] isEqualToString:@"CSound Files"] ||
		   [[self fileType] isEqualToString:@"CSound Orc Files"] ||
		   [[self fileType] isEqualToString:@"CSound Sco Files"] ||
		   [[self fileType] isEqualToString:@"CSound Cabbage Files"])
			languagecomment = @";";
		else if([[self fileType] isEqualToString:@"Python Files"])
			languagecomment = @"#";
		else if([[self fileType] isEqualToString:@"Lua Files"])
			languagecomment = @"--";
		else return;
		
		
		NSInteger mLineStart = [textEditor getLineNumberFromPosition:[textEditor getSelectionStart]];	
		NSInteger mLineEnd = [textEditor getLineNumberFromPosition:[textEditor getSelectionEnd]];
		
		NSInteger curPos = 0;
		NSInteger curLinePos = 0;
		NSString* curText = nil;
		//NSRange range = NSMakeRange(0, 0);
		
		//for (Int32 mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
		for (NSInteger mLine = mLineStart; mLine < (mLineEnd + 1); mLine++)
		{
			//curPos = GetFocusedEditor.PositionFromLine(mLine);
			curPos = [textEditor getPositionFromLineNumber:mLine];
			//curText = GetFocusedEditor.GetLine(mLine);
			curText = [textEditor getTextOfLine:mLine];
			//curLinePos = curText.IndexOf(languagecomment);
			curLinePos = [curText rangeOfString:languagecomment].location;
			if (curLinePos != NSNotFound)
			{
				//GetFocusedEditor.GotoPos(curPos + curLinePos);
				[textEditor setCaretPosition:curPos + curLinePos];
				//foreach (char c in languagecomment.ToCharArray())
				for(NSInteger i = 0; i < [languagecomment length]; i++)
				{
					//GetFocusedEditor.Clear(); //Equivalent to: 'Canc' key
					[textEditor PerformDelete];
				}
			}
//			else 
//			{
//				return;
//			}
			
		}
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxUnCommentLine:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
	
}

- (IBAction)wxLineEndsConverter:(id)sender
{
	NSInteger eolMode = SC_EOL_CRLF;
	
	switch ([sender tag])
	{
		case 0:
			eolMode = SC_EOL_LF;
			break;
			
		case 1:
			eolMode = SC_EOL_CR;
			break;
			
		case 2:
			eolMode = SC_EOL_CRLF;
			break;
	}
	
	[textEditor ConvertEOL:eolMode];
	[textEditor setEolMode:eolMode];

}

- (IBAction)wxZoomFontSize:(id)sender
{
	@try
	{
		NSSegmentedControl* control = sender;
		NSInteger index = [control selectedSegment];
		
		//[control setEnabled:true forSegment:0];
		
		NSInteger zoom = [textEditor getZoom];
		
		switch (index) 
		{
			case 0:
				if(zoom > -10) [textEditor setZoom:[textEditor getZoom] - 1];
				break;
			case 1:
				if(zoom < 20) [textEditor setZoom:[textEditor getZoom] + 1];
				break;
			case 2:
				[textEditor setZoomForView1:0];
				[textEditor setZoomForView2:0];
				break;
		}
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxZoomFontSize:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
	
	[self SCI_ZOOM_Notification:nil];
}

- (IBAction)wxResetTextZoom:(id)sender
{
	[textEditor setZoomForView1:0];
	[textEditor setZoomForView2:0];
	[self SCI_ZOOM_Notification:nil];
}

- (IBAction)wxGotoDefinitionOf:(id)sender
{
	if([[textEditor getCurrentWord] length] < 1) return;
	
	NSString* Definition = [textEditor getCurrentWord];
	
	
	@try
	{
		//if (Definition.EndsWith("."))
		if([Definition hasSuffix:@"."])
		{
			//Definition = Definition.TrimEnd('.');
			Definition = [Definition substringToIndex:[Definition length] - 1];
		}
		
		//if (!ActiveEditor.UserOpcodes.Contains(Definition))
		if([wxMAIN getOpcodeValue:Definition] == nil)
		{
			////MACRO SEARCH 
			if ([Definition hasPrefix:@"g"] ||
				[Definition hasPrefix:@"$"])
			{
				if ([Definition hasPrefix:@"$"])
				{
					Definition = [Definition substringFromIndex:1]; //Definition.TrimStart('$');
				}
				//Global Type: search all text from start
				//				Int32 mPos =
				//				ActiveEditor.textEditor.FindText(
				//												 Definition, true, true,
				//												 false, true, false, true,
				//												 0, ActiveEditor.textEditor.GetTextLength());
				NSInteger mPos = 
				[textEditor FindTextEx:Definition
						MatchWholeWord:true 
							 MatchCase:true 
							IsBackward:false 
							SelectText:true 
						   ShowMessage:false 
							   SkipRem:true
								 start:0
								   end:[textEditor getTextLength]
							  useRegEx:false];
				
				
				
				//ActiveEditor.StoreCursorPos(mPos);
				if(mPos > -1) [_CursorPosition StoreCursorPos:mPos];
				return;
				
			}
			
			////LOCAL_TYPE SEARCH: search inside "instr" -> "endin" 
			else
			{
				NSInteger posINSTR = -1;
				NSInteger curPos = [textEditor getCaretPosition]; //ActiveEditor.textEditor.GetCaretPosition();
				NSInteger mFindPos = -1;
				
				//Search INSTR 
				//				mFindPos = ActiveEditor.textEditor.FindText(
				//															"instr", true, true, true,
				//															false, false, true);
				mFindPos = [textEditor FindText:@"instr"
								 MatchWholeWord:true 
									  MatchCase:true 
									 IsBackward:true 
									 SelectText:false 
									ShowMessage:false 
										SkipRem:true];
				
				if (mFindPos > -1)
				{
					posINSTR = mFindPos;
				}
				else return;
				
				//				mFindPos =
				//				ActiveEditor.textEditor.FindText(
				//												 Definition, true, true,
				//												 false, true, false, true,
				//												 posINSTR, curPos - 1); //Definition.Length);
				mFindPos = [textEditor FindTextEx:Definition
								   MatchWholeWord:true 
										MatchCase:true 
									   IsBackward:false 
									   SelectText:true 
									  ShowMessage:false 
										  SkipRem:true
											start:posINSTR
											  end:curPos - 1
										 useRegEx:false];
				
				//ActiveEditor.StoreCursorPos(mFindPos);
				if(mFindPos > -1) [_CursorPosition StoreCursorPos:mFindPos];
			}
		}
		
		//TODO:
		////USER OPCODE SEARCH
		//else if (ActiveEditor.UserOpcodes.Contains(Definition))
		//{
		//	ArrayList mc =
		//	ActiveEditor.textEditor.SearchText(
		//									   Definition, true, true, false, true);
		//	foreach (Int32 mPos in mc)
		//	{
		//		string mLine = ActiveEditor.textEditor.GetTextLine(
		//														   ActiveEditor.textEditor.GetLineNumberFromPosition(
		//																											 mPos));
		//		Int32 mDefinitionPos = mLine.IndexOf(Definition);
		//		Int32 mOpcodePos = mLine.IndexOf("opcode");
		//		if (mOpcodePos > -1 &&
		//			mOpcodePos < mDefinitionPos)
		//		{
		//			ActiveEditor.textEditor.SetCaretPosition(mPos);
		//			ActiveEditor.textEditor.SetSelectionEnd(mPos + Definition.Length);
		//			//ActiveEditor.StoreCursorPos(mPos);
		//			break;
		//		}
		//	}
		//}
		
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxGotoDefinitionOf:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}

}


- (IBAction)wxGotoReferenceOf:(id)sender
{	
	//if (string.IsNullOrEmpty(gCurWord)) return;
	if([[textEditor getCurrentWord] length] < 1) return;
	
	//wxEditor tEditor = ActiveEditor;
	NSString* Definition = [textEditor getCurrentWord];
	
	@try
	{
		
		//if (!ActiveEditor.UserOpcodes.Contains(Definition))
		if([wxMAIN getOpcodeValue:Definition] == nil)
		{
			////MACRO TYPE: search all text from current position
			//if (ActiveEditor.textEditor.GetTextLine(ActiveEditor.textEditor.GetCurrentLineNumber()).Contains("#define"))
			if([[textEditor getTextOfLine:[textEditor getCurrentLineNumber]] rangeOfString:@"#define"].location != NSNotFound)
			{
				NSString* NewDefinition = [NSString stringWithFormat:@"$%@", Definition];
				
				//				NSInteger mPos = ActiveEditor.textEditor.FindText(
				//												 Definition, true, true, false,
				//												 true, false, true,
				//												 ActiveEditor.textEditor.GetCaretPosition() + 1,
				//												 ActiveEditor.textEditor.GetTextLength());
				
				NSInteger mPos = 
				[textEditor FindTextEx:NewDefinition
						MatchWholeWord:true 
							 MatchCase:true 
							IsBackward:false 
							SelectText:true 
						   ShowMessage:false 
							   SkipRem:true
								 start:[textEditor getCaretPosition] + 1
								   end:[textEditor getTextLength]
							  useRegEx:false];
				
				//ActiveEditor.StoreCursorPos(mPos);
				if(mPos > -1) [_CursorPosition StoreCursorPos:mPos];
				return;
			}
			
			////GLOBAL TYPE: search all text from current position
			//			else if (Definition.StartsWith("g") ||
			//					 Definition.StartsWith("$"))
			else if ([Definition hasPrefix:@"g"] ||
					 [Definition hasPrefix:@"$"])
			{
				
				NSInteger mPos = 
				[textEditor FindTextEx:Definition
						MatchWholeWord:true 
							 MatchCase:true 
							IsBackward:false 
							SelectText:true 
						   ShowMessage:false 
							   SkipRem:true
								 start:[textEditor getCaretPosition] + 1
								   end:[textEditor getTextLength]
							  useRegEx:false];
				
				//ActiveEditor.StoreCursorPos(mPos);
				if(mPos > -1) [_CursorPosition StoreCursorPos:mPos];
				return;
				
			}
			
			////LOCAL TYPE: search inside "instr" -> "endin" 
			else
			{
				
				NSInteger posENDIN = -1;
				NSInteger curPos = [textEditor getCaretPosition]; //ActiveEditor.textEditor.GetCaretPosition();
				NSInteger mFindPos = -1;
				
				//Search ENDIN 
				//				mFindPos = ActiveEditor.textEditor.FindText(
				//															"endin", true, true,
				//															false, false, false, true);
				mFindPos = [textEditor FindText:@"endin"
								 MatchWholeWord:true 
									  MatchCase:true 
									 IsBackward:false 
									 SelectText:false 
									ShowMessage:false 
										SkipRem:true];
				
				
				if (mFindPos > -1)
				{
					posENDIN = mFindPos;
				}
				else return;
				
				//Search reference
				//				mFindPos = ActiveEditor.textEditor.FindText(
				//															Definition, true, true,
				//															false, true, false, true,
				//															curPos + 1, posENDIN);
				mFindPos = [textEditor FindTextEx:Definition
								   MatchWholeWord:true 
										MatchCase:true 
									   IsBackward:false 
									   SelectText:true 
									  ShowMessage:false 
										  SkipRem:true
											start:curPos + 1
											  end:posENDIN
										 useRegEx:false];
				
				//ActiveEditor.StoreCursorPos(mFindPos);
				if(mFindPos > -1) [_CursorPosition StoreCursorPos:mFindPos];
			}
		}
		
		//TODO:
		////USER OPCODES SEARCH
		//else if (ActiveEditor.UserOpcodes.Contains(Definition))
		//{
		//	Int32 mFindPos = ActiveEditor.textEditor.FindText(
		//													  Definition, true, true,
		//													  false, true, false, true);
		//	//ActiveEditor.StoreCursorPos(mFindPos);
		//}
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> wxGotoReferenceOf:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
	
}



- (IBAction)wxCursorPositionNext:(id)sender
{
	//NSLog(@"NEXT");
	[textEditor setCaretPosition:[_CursorPosition NextPos]];
	[textEditor updateCaret];
	//[textEditor SelectLine:[textEditor getCurrentLineNumber]];
}

- (IBAction)wxCursorPositionPrevious:(id)sender
{
	//NSLog(@"PREVIOUS");
	[textEditor setCaretPosition:[_CursorPosition PreviousPos]];
	[textEditor updateCaret];
	//[textEditor SelectLine:[textEditor getCurrentLineNumber]];
}


- (IBAction)wxGotoHyperlink:(id)sender
{
	//NSLog(@"wxGOTOHYPERLINK");
	[self SCI_LEFTMOUSEHYPERLINKS_Notification: nil];
}

- (IBAction)wxFormatCode:(id)sender
{	
	//Format only syntax documents (csound, python, lua)
	if([self isSyntaxType] == false) return;

	//NSLog(@"Format Line/Selection");
	[[wxCodeFormatter sharedInstance] formatCode:textEditor
										   start:[textEditor getSelectionStart] 
											 end:[textEditor getSelectionEnd]];
	
}

- (IBAction)wxFormatCodeAll:(id)sender
{
	//Format only syntax documents (csound, python, lua)
	if([self isSyntaxType] == false) return;
	
	//NSLog(@"Format All");
	[[wxCodeFormatter sharedInstance] formatCode:textEditor
										   start:-1 
											 end:-1];
}



- (IBAction)wxSetFocusOnPrimaryView:(id)sender
{
	//NSLog(@"wxSetFocusOnPrimaryView");
	[textEditor setFocusOnPrimaryView];
	[textEditor ScrollCaret];
}
- (IBAction)wxSetFocusOnSecondaryView:(id)sender
{
	//NSLog(@"wxSetFocusOnSecondaryView");
	if([textEditor getIsSplitted])
	{
		[textEditor setFocusOnSecondaryView];
		[textEditor ScrollCaret];
	}
}


- (IBAction)wxImportOrcSco:(id)sender
{
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setResolvesAliases:YES];
	[oPanel setAllowsMultipleSelection:false];
	[oPanel setCanChooseFiles: true];
	[oPanel setCanChooseDirectories:false];
	
	NSArray *filesToOpen;
    NSString *theFileName;
	
	int result = [oPanel runModalForDirectory:nil
										 file:nil
										types:[NSArray arrayWithObjects:@"orc",@"sco",nil]];
	
	
    if (result == NSOKButton) 
	{
        filesToOpen = [oPanel filenames];
        theFileName = [filesToOpen objectAtIndex:0];
		NSString* temp = [wxIMPEXP ImportORCSCO:theFileName];
		[self setTextContent: temp];
    }
}

- (IBAction)wxImportOrc:(id)sender
{
	[wxIMPEXP ImportORC:textEditor owner:self];
}

- (IBAction)wxImportSco:(id)sender
{
	[wxIMPEXP ImportSCO:textEditor owner:self];
}

- (IBAction)wxExportOrcSco:(id)sender
{
	[wxIMPEXP ExportOrcSco:textEditor owner:self];
}
- (IBAction)wxExportOrc:(id)sender
{
	[wxIMPEXP ExportOrc:textEditor owner:self];
}
- (IBAction)wxExportSco:(id)sender
{
	[wxIMPEXP ExportSco:textEditor owner:self];
}



- (IBAction)wxShowDocInfo:(id)sender
{
	if(panelDocInfo != nil)
	{
		if([self fileURL] != nil)
		{
			[labelInfoName setStringValue:[[[self fileURL] path] lastPathComponent]];
			[labelInfoFullPath setStringValue:[[self fileURL] path]];
			[labelInfoFileType setStringValue:[self fileType]];
			NSString* rights = 
			([[NSFileManager defaultManager] isWritableFileAtPath:[[self fileURL] path]] ? @"Read/Write" : @"Read only");
			[labelInfoFileRights setStringValue:rights];
		}
		else 
		{
			[labelInfoName setStringValue:@""];
			[labelInfoFullPath setStringValue:@""];
			[labelInfoFileType setStringValue:@""];
			[labelInfoFileRights setStringValue:@""];
		}
		
		NSString* eols = @"";
		switch ([textEditor GetEOLMode]) 
		{
			case 0:
				eols = @"CRLF (Windows)";
				break;
			case 1:
				eols = @"CR (Mac)";
				break;
			case 2:
				eols = @"LF (OsX-Unix)";
				break;
		}
		[labelInfoEols setStringValue:eols];
		
		[labelInfoEncoding setStringValue:@"Unicode (UTF-8)"];
		[labelInfoTotalLines setStringValue:[NSString stringWithFormat:@"%d", [textEditor getLinesCount]]];
		[labelInfoTotalChars setStringValue:[NSString stringWithFormat:@"%d", [textEditor getTextLength]]];
		
		[NSApp beginSheet:panelDocInfo 
		   modalForWindow:[self windowForSheet]
			modalDelegate:self 
		   didEndSelector:nil
			  contextInfo:nil];
			
	}
}
- (IBAction)wxShowDocInfoOK:(id)sender
{
	[panelDocInfo close];
	[NSApp endSheet:panelDocInfo];
}



- (IBAction)wxShowCodeRepositoryWindow:(id)sender
{
	//	NSPoint p = NSMakePoint([[textEditor getPrimaryView] getGeneralProperty:SCI_POINTXFROMPOSITION parameter:0 extra:[textEditor getCaretPosition]],
	//							[[textEditor getPrimaryView] getGeneralProperty:SCI_POINTYFROMPOSITION parameter:0 extra:[textEditor getCaretPosition]]);
	//	p.x += [[self windowForSheet] frame].origin.x;
	//	p.y = [textEditor frame].size.height - p.y;
	//	p.y += [[self windowForSheet] frame].origin.y - 100;
	
	//Set window position
	NSPoint p = NSMakePoint([[self windowForSheet] frame].origin.x + [[self windowForSheet] frame].size.width / 2 - 305, 
							[[self windowForSheet] frame].origin.y + [downView frame].size.height - 285);
	
	[wxCODEREP setOwner:textEditor];
	//[wxCODEREP displayCodeRepository];
	[wxCODEREP displayCodeRepositoryAt:p];
	//[textEditor setFocus];
}

- (IBAction)wxSaveUserCodeToRepository:(id)sender
{
	//if([[textEditor getSelectedText]length] > 0)
	{
		[wxCODEREP setOwner:textEditor];
		[wxCODEREP displayInputBoxPanel:[self windowForSheet]];
	}
	
}

- (IBAction)wxShowCSoundRepositoryWindow:(id)sender
{
	//	NSPoint p = NSMakePoint([[textEditor getPrimaryView] getGeneralProperty:SCI_POINTXFROMPOSITION parameter:0 extra:[textEditor getCaretPosition]],
	//							[[textEditor getPrimaryView] getGeneralProperty:SCI_POINTYFROMPOSITION parameter:0 extra:[textEditor getCaretPosition]]);
	//	p.x += [[self windowForSheet] frame].origin.x;
	//	p.y = [textEditor frame].size.height - p.y;
	//	p.y += [[self windowForSheet] frame].origin.y - 100;
	
	//Set window position
	NSPoint p = NSMakePoint([[self windowForSheet] frame].origin.x + [[self windowForSheet] frame].size.width / 2 - 290, 
							[[self windowForSheet] frame].origin.y + [downView frame].size.height - 308);
	
	[wxCSOUNDREP setOwner:textEditor];
	[wxCSOUNDREP displayCSoundRepositoryAt:p];
	//[textEditor setFocus];
	
}

- (IBAction)wxShowAutocompleteWindow:(id)sender
{
	
	//Ensure caret visibility	
	NSInteger curLine = [textEditor getCurrentLineNumber]; //ActiveEditor.textEditor.GetCurrentLineNumber();
	NSInteger firstVisibleLine = [textEditor getFirstVisibleLine]; //ActiveEditor.textEditor.GetFirstVisibleLine();
	NSInteger lastVisibleLine = firstVisibleLine +
								[textEditor getLinesOnScreen]; //ActiveEditor.textEditor.LinesOnScreen();
	
	//If caret is not visible:
	if(!(curLine >= firstVisibleLine && curLine <= lastVisibleLine))
	{
		[textEditor ScrollCaret];
		//[textEditor GoToLine:curLine];
		//[textEditor setFirstVisibleLine: curLine];
	}
	
	
	
	
	NSInteger curPos = [textEditor getCaretPosition];
	//NSInteger curLine = [textEditor getLineNumberFromPosition:curPos];
	NSInteger nextPos = [textEditor getPositionFromLineNumber:curLine+1];
	NSInteger offsetX = 0;
	NSInteger offsetY = 7;
	
	NSPoint p = 
		NSMakePoint(
			[[textEditor getFocusedEditor] getGeneralProperty:SCI_POINTXFROMPOSITION parameter:0 extra:curPos] + offsetX,
			[[textEditor getFocusedEditor] getGeneralProperty:SCI_POINTYFROMPOSITION parameter:0 extra:nextPos] + offsetY);
	
	
	//if([[textEditor getFocusedEditor] isFlipped])
	//NSLog(@"ISFLIPPED: %d", [[textEditor getFocusedEditor] content]);
	
//	NSRect frameRect = [[textEditor getFocusedEditor] bounds];
//    NSAffineTransform* xform = [NSAffineTransform transform];
//    [xform translateXBy:0.0 yBy:frameRect.size.height];
//    [xform scaleXBy:1.0 yBy:-1.0];
//    [xform concat];
//	NSPoint p = [xform transformPoint:o];
	
	
	
	//Revert y coordinates
	if([textEditor getIsSplitted] == false)
		p.y = [codeView frame].size.height - p.y;
	else
	{
		if([textEditor getFocusedEditor] == [textEditor getPrimaryView])
		{
			p.y = [codeView frame].size.height - p.y;
		}
		else
			p.y = [codeView frame].size.height - [[textEditor getSecondaryView] frame].size.height - 
				  - [textEditor dividerThickness] - [NSScroller scrollerWidth] - p.y - 12;
	}
	
	
	//Modify x coordinates (based on Explorer width)
	if([treeViewStructureHost isHidden] == false)
		p.x += [treeViewStructureHost frame].size.width;
	
	
	//NSPoint result = [[self windowForSheet]convert convertPoint: p fromView: nil];
	NSPoint result = [[codeView window] convertBaseToScreen:p];

	
	[wxAUTOCOMP setDocumentOwner:self withEditor:textEditor];
	[wxAUTOCOMP displayAutocompleteAt:result];
	//[textEditor setFocus];
	
}


// ONLY FOR DEBUG
//- (IBAction)wxTEST:(id)sender
//{
//
//}
//----------------------------------------------------------------------------------------------------------
// IB ACTIONS
//----------------------------------------------------------------------------------------------------------











#pragma mark - Compiler Actions
//----------------------------------------------------------------------------------------------------------
// COMPILER ACTIONS
//----------------------------------------------------------------------------------------------------------
- (NSString*) saveFileBeforeCompile
{
	
	//Check if WinXound can compile the file (only csound, python or lua extensions are allowed)
	if([self isSyntaxType] == false)
	{
		[wxMAIN ShowMessage:@"WinXound cannot compile this file!" 
			informativeText:@"Only CSound, Python or Lua files are allowed." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return @"";
	}
	
	
	//TRY TO SAVE THE FILE
	if([self fileURL] == nil ||
	   ![[NSFileManager defaultManager] fileExistsAtPath:[[self fileURL] path]])
	{
		//Old behaviour:
		//[wxMAIN ShowMessage:@"File not saved!" 
		//	informativeText:@"Please save the document before to call the compiler." 
		//	  defaultButton:@"OK"
		//	alternateButton:nil 
		//		otherButton:nil];
		//[self saveDocument:self];
		//return @"";
		

		NSString* tempFilename = @"";
		NSString* tempExtension = @"";
		
		//Search for User Application Support directory
		NSArray* paths = NSSearchPathForDirectoriesInDomains(NSApplicationSupportDirectory, NSUserDomainMask, YES);
		if(paths != nil)
		{
			if([paths count] > 0)
			{
				NSString* winxoundPath = [[NSString stringWithFormat:@"%@/WinXound/Temp", [paths objectAtIndex:0]] retain];
				
				//If the directory does not exist we create it
				NSFileManager *fileManager = [NSFileManager defaultManager];
				if([fileManager fileExistsAtPath:winxoundPath] == false)
				{
					[fileManager createDirectoryAtPath:winxoundPath 
						   withIntermediateDirectories:YES 
											attributes:nil 
												 error:nil];
				}
					
				//Check for the correct extension:
				if([[self fileType] isEqualToString:@"CSound Files"])
					tempExtension = @".csd";
				else if([[self fileType] isEqualToString:@"CSound Orc Files"])
					tempExtension = @".orc";
				else if([[self fileType] isEqualToString:@"CSound Sco Files"])
					tempExtension = @".sco";
				else if([[self fileType] isEqualToString:@"Python Files"])
					tempExtension = @".py";
				else if([[self fileType] isEqualToString:@"Lua Files"])
					tempExtension = @".lua";
							
				
				tempFilename = [NSString stringWithFormat:@"%@/WinXound_Untitled%@", winxoundPath, tempExtension];

				NSLog(@"File Saved before compile (temporary file)");
				[textEditor SaveFile:tempFilename];
				
			}
		}
		
		return tempFilename;
	}
	else 
	{
		if([[NSFileManager defaultManager] isWritableFileAtPath:[[self fileURL] path]])
		{
			NSLog(@"File Saved before compile");
			[self saveDocument:self];
		}
	}
	
	return [self fileName];
}


- (IBAction)wxCompile:(id)sender
{
	[self Compile:@"" External:false];
}

- (void)Compile:(NSString*)additionalParams External:(bool)external
{
	NSString* compiler = nil;
	NSString* parameters = nil;

	//WE MUST SAVE FILE BEFORE COMPILE
	//Old behaviour:
	//if([[self saveFileBeforeCompile] isEqualToString:@""]) return;
	//New one:
	NSString* TextEditorFileName = [self saveFileBeforeCompile];
	if([TextEditorFileName isEqualToString:@""]) return;
	
	NSString* FirstFilename = TextEditorFileName;
	NSString* SecondFilename = nil;
	
	//LUA COMPILER
	if([[TextEditorFileName lowercaseString] hasSuffix:@".lua"])
	{
		if([[wxDefaults valueForKey:@"LuaConsolePath"] isEqualToString:@""])
		{
			[wxMAIN ShowMessage:@"Lua Compiler not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		compiler = [wxDefaults valueForKey:@"LuaConsolePath"];
		parameters = [wxDefaults valueForKey:@"LuaDefaultFlags"];
	}
	
	//PYTHON COMPILER
	else if ([TextEditorFileName hasSuffix:@".py"] ||
			 [TextEditorFileName hasSuffix:@".pyw"])
	{
		if([[wxDefaults valueForKey:@"PythonConsolePath"] isEqualToString:@""])
		{
			[wxMAIN ShowMessage:@"Python Compiler not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		compiler = [wxDefaults valueForKey:@"PythonConsolePath"];
		parameters = [wxDefaults valueForKey:@"PythonDefaultFlags"];
	}
	
	//CSOUND COMPILER
	else if ([TextEditorFileName hasSuffix:@".csd"] ||
			 [TextEditorFileName hasSuffix:@".orc"] ||
			 [TextEditorFileName hasSuffix:@".sco"])
	{
		if([[wxDefaults valueForKey:@"CSoundConsolePath"] isEqualToString:@""])
		{
			[wxMAIN ShowMessage:@"CSound Compiler not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
		}
		compiler = [wxDefaults valueForKey:@"CSoundConsolePath"];
		
		//Check for the additional flags
		additionalParams = [self CheckForAdditionalFlags:additionalParams];
		if([additionalParams isEqualToString:@"[CANCEL]"])
			return;

		//parameters = [self getCSoundFlags];
		parameters = [NSString stringWithFormat:@"%@ %@", additionalParams, [self getCSoundFlags]];
		
		if ([TextEditorFileName hasSuffix:@".orc"])
		{
			SecondFilename = [linkBoxComboBox stringValue];
		}
		else if([TextEditorFileName hasSuffix:@".sco"])
		{
			FirstFilename = [linkBoxComboBox stringValue];
			SecondFilename = TextEditorFileName;
		}

	}
	else 
	{
		return;
	}


	if(external == false) //COMPILE INTERNALLY
	{
		
		//?? Maybe: when compile, show up compiler window ?? Option ??
		//[self wxShowCompiler:self];
		
		[buttonStopCompiler setHidden:false];
		[buttonPauseCompiler setHidden:false];
		
		[_Compiler compile:compiler //@"/usr/local/bin/csound" 
				parameters:parameters //@"-B4096 --displays --asciidisplay" 
				 filename1:FirstFilename //TextEditorFileName //[self fileName] 
				 filename2:SecondFilename //nil
					output:compilerOutput
					button:buttonStopCompiler
			   buttonPause:buttonPauseCompiler
					 owner:self];
	}
	else //COMPILE EXTERNALLY (TERMINAL)
	{
		NSString* scriptName = nil;
		if(SecondFilename == nil)
			scriptName = [NSString stringWithFormat:@"\"%@ %@ \\\"%@\\\"\"",compiler, parameters, TextEditorFileName]; //[self fileName]];
		else
			scriptName = [NSString stringWithFormat:@"\"%@ %@ \\\"%@\\\" \\\"%@\\\"\"",compiler, parameters, FirstFilename, SecondFilename];
		
		//tell application \"Terminal\" to do script "%@ %@ \"%@\""
		//	NSString *s = [NSString stringWithFormat:
		//				   @"tell application \"Terminal\" to do script %@ activate", scriptName];
		
		
		NSString *s = [NSString stringWithFormat:
					   @"tell application \"System Events\" \n"
					   "if (count(processes whose name is \"Terminal\")) is 0 then \n"
					   "tell application \"Terminal\" \n"
					   "activate \n"
					   "do script %@ in window 1 \n"
					   "end tell \n"
					   "else \n"
					   "tell application \"Terminal\" \n"
					   "activate \n"
					   "do script %@ \n"
					   "end tell \n"
					   "end if \n"
					   "end tell", scriptName, scriptName];
		
		
		NSAppleScript *as = [[[NSAppleScript alloc] initWithSource: s] autorelease];
		[as executeAndReturnError:nil];
		//[as release];
	}
	
}


- (void)UpdateAdditionalFlagsPopupButtonList
{
	/*
	[AdditionalFlagsPopup removeAllItems];
	NSString* imageName = [[NSBundle mainBundle] pathForResource:@"compile_16" ofType:@"png"];
	NSImage* imageObj = [[[NSImage alloc] initWithContentsOfFile:imageName] autorelease];
	[AdditionalFlagsPopup addItemWithTitle:@""];
	[[AdditionalFlagsPopup itemAtIndex:0] setImage:imageObj];
	*/
	
	//Clear all items except the first one (it contains the Compile image)
	for(NSInteger index = [AdditionalFlagsPopup numberOfItems] - 1; index > 0; index--)
	{
		[AdditionalFlagsPopup removeItemAtIndex:index];
	}
	
	//Set the AdditionalFlags menu's
	NSArray* lines = [[wxDefaults valueForKey:@"CSoundAdditionalFlags"] componentsSeparatedByString:@"\n"];
	
	//Add items and connect to the documents method (CompileWithAdditionFlagsClicked ... look at wxDocument!)
	for(NSString* line in lines)
	{
		if([line length] > 0)
			[AdditionalFlagsPopup addItemWithTitle:line];
	}
}

- (IBAction)wxAdditionalFlagsPopupClicked:(id)sender
{
	NSPopUpButton* pb = (NSPopUpButton*) sender;	
	NSString* temp = [[pb selectedItem] title]; //[pb title];
	if([temp isEqualToString:@""]) return;
	
	NSString* options = [self GetAdditionalFlagsOptions:temp];
	
	if(options != nil)
		if(![options isEqualToString:@"[ERROR]"])
			[self Compile:options External:false];
	
}

- (void) CompileWithAdditionalFlagsClicked:(id)sender
{
	NSMenuItem* mi = (NSMenuItem*)sender;
	NSString* temp = [mi title];
	if([temp isEqualToString:@""]) return;
	
	NSString* options = [self GetAdditionalFlagsOptions:temp];
	
	if(options != nil)
		if(![options isEqualToString:@"[ERROR]"])
			[self Compile:options External:false];
}

- (void) CompileExternalWithAdditionalFlagsClicked:(id)sender
{
	NSMenuItem* mi = (NSMenuItem*)sender;
	NSString* temp = [mi title];
	if([temp isEqualToString:@""]) return;
	
	NSString* options = [self GetAdditionalFlagsOptions:temp];
	
	if(options != nil)
		if(![options isEqualToString:@"[ERROR]"])
			[self Compile:options External:true];
}

- (NSString*) GetAdditionalFlagsOptions:(NSString*)options
{
	@try 
	{
		NSArray* splittedOptions = [options componentsSeparatedByString:@"]:"];
		NSString* options = [splittedOptions objectAtIndex:1];
		
		return options;
	}
	@catch (NSException * e)
	{
		[wxMAIN ShowMessage:@"Additional Flags: Invalid syntax!" 
			informativeText:@"Please specify a valid command ([Description]: Flags) in your WinXound Preferences (Compiler tab)."
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		
	}
	
	return @"[ERROR]";
}




- (IBAction)wxStopCompiler:(id)sender
{
	if(_Compiler != nil)
		[_Compiler stopProcess];
}

- (IBAction)wxSuspendResumeCompiler:(id)sender
{
	if([_Compiler processIsSuspended])
	{
		[_Compiler resumeProcess];
	}
	else
	{
		[_Compiler suspendProcess];
	}
}

- (NSString*) getCSoundFlags
{
	NSString* parameters = [wxDefaults valueForKey:@"CSoundDefaultFlags"];
	//If --displays flag is present we must show csound windows and suppress the
	//--asciidisplay or --postscriptdisplay
	@try
	{
		//Find <CsOptions> tags
		NSInteger findStart = [textEditor FindText:@"<CsOptions>" 
									MatchWholeWord:true 
										 MatchCase:true 
										IsBackward:false 
										SelectText:false 
									   ShowMessage:false 
										   SkipRem:true 
											 start:0 
											   end:-1];

		NSInteger findEnd = [textEditor FindText:@"</CsOptions>" 
									MatchWholeWord:true 
										 MatchCase:true 
										IsBackward:false 
										SelectText:false 
									   ShowMessage:false 
										   SkipRem:true 
											 start:0 
											   end:-1];
		
		if (findStart > -1 && findEnd > -1)
		{
			NSInteger linestart, lineend;
			linestart = [textEditor getLineNumberFromPosition:findStart];
			lineend = [textEditor getLineNumberFromPosition:findEnd];
			
			for (NSInteger i = linestart; i < lineend; i++)
			{
				NSString* text = [textEditor getTextOfLine:i];
				
				//if (text.IndexOf(";") > -1)
				if([text rangeOfString:@";"].location != NSNotFound)
				{
					//text = text.Remove(text.IndexOf(";"));
					text = [text substringToIndex:[text rangeOfString:@";"].location];
					//if (text.Contains("--displays") /*||
					//	text.Contains("--nodisplays") ||
					//	text.Contains("-d")*/)
					if([text rangeOfString:@"--displays"].location != NSNotFound)
					{
						//if (!text.Contains("--asciidisplay") &&
						//	!text.Contains("-g") &&
						//	!text.Contains("--postscriptdisplay") &&
						//	!text.Contains("-G"))
						if([text rangeOfString:@"--asciidisplay"].location == NSNotFound &&
						   [text rangeOfString:@"-g"].location == NSNotFound &&
						   [text rangeOfString:@"--postscriptdisplay"].location == NSNotFound &&
						   [text rangeOfString:@"-G"].location == NSNotFound)
						{
							//Found --displays option so:
							//temporarily remove all 'display(s)' settings from the default winxound 
							//settings and leave the others
							NSString* defaultflags = parameters;
							//defaultflags = defaultflags.Replace("--displays", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"--displays" 
																				   withString:@""];
							//defaultflags = defaultflags.Replace("--nodisplays", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"--nodisplays" 
																				   withString:@""];			
							//defaultflags = defaultflags.Replace("-d ", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"-d " 
																				   withString:@" "];				
							//defaultflags = defaultflags.Replace("--asciidisplay", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"--asciidisplay" 
																				   withString:@""];
							//defaultflags = defaultflags.Replace("-g ", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"-g " 
																				   withString:@" "];
							//defaultflags = defaultflags.Replace("--postscriptdisplay", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"--postscriptdisplay" 
																				   withString:@""];
							//defaultflags = defaultflags.Replace("-G ", "");
							defaultflags = [defaultflags stringByReplacingOccurrencesOfString:@"-G " 
																				   withString:@" "];
							
							//NSLog(@"CheckDisplaysFlag = true - parameters: %@", defaultflags);
							
							return [defaultflags stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
						}
					}
				}
			}
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -> getCSoundFlags Error: %@ - %@", [e name], [e reason]);
	}
	
	return parameters;
}


//private string CheckForAdditionalFlags(string additionalParams)
- (NSString*) CheckForAdditionalFlags:(NSString*)additionalParams
{
	//Check for additional parameters 
	//[*=current editor filename]
	//[?=ask for filename]
	
	//if (additionalParams.Contains("*"))
	if([additionalParams rangeOfString:@"*"].location != NSNotFound)
	{
		//additionalParams = additionalParams.Replace("*", Path.GetFileNameWithoutExtension(ActiveEditor.FileName));
		additionalParams = [additionalParams stringByReplacingOccurrencesOfString:@"*"
																	   withString:[[[self fileURL] path] stringByDeletingPathExtension]];
		
	}
	//else if (additionalParams.Contains("?"))
	else if([additionalParams rangeOfString:@"?"].location != NSNotFound)
	{
		NSSavePanel *spanel = [NSSavePanel savePanel];
		//[spanel setDirectory:[path stringByExpandingTildeInPath]];
		//[spanel setPrompt:NSLocalizedString(@"save_ok",nil)];
		//[spanel setRequiredFileType:@"rtfd"];
		
		int result = [spanel runModalForDirectory:nil
											 file:[[[[self fileURL] path] lastPathComponent] stringByDeletingPathExtension]];
		
		if (result == NSOKButton) 
		{
			NSString* theFileName = [spanel filename];
			//-o"?.wav"
			additionalParams = [additionalParams stringByReplacingOccurrencesOfString:@"?" 
																		   withString:theFileName];
		}
		else
			additionalParams = @"[CANCEL]";
		
	}
	//else
	//	additionalParams = [additionalParams stringByReplacingOccurrencesOfString:@"\""
	//																   withString:@""];
	
	return additionalParams;
}


//COMPILE EXTERNAL
- (IBAction) wxCompileExternal:(id)sender
{
	[self Compile:@"" External: true];
}


//EXTERNAL GUI
- (IBAction)wxCallExternalGui:(id)sender
{
	if([[self saveFileBeforeCompile] isEqualToString:@""]) return;
	
	NSString* compiler = nil;
	
	//LUA GUI
	if([[[self fileName] lowercaseString] hasSuffix:@".lua"])
	{
		if([[wxDefaults valueForKey:@"LuaExternalGuiPath"] isEqualToString:@""])
		{
			[wxMAIN ShowMessage:@"Lua External Gui not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		compiler = [wxDefaults valueForKey:@"LuaExternalGuiPath"];
	}
	
	//PYTHON GUI
	else if ([[[self fileName] lowercaseString] hasSuffix:@".py"] ||
			 [[[self fileName] lowercaseString] hasSuffix:@".pyw"])
	{
		if([[wxDefaults valueForKey:@"PythonExternalGuiPath"] isEqualToString:@""])
		{
			[wxMAIN ShowMessage:@"Python External Gui not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		compiler = [wxDefaults valueForKey:@"PythonExternalGuiPath"];
	}
	
	//CSOUND GUI
	else
	{
		if([[wxDefaults valueForKey:@"CSoundExternalGuiPath"] isEqualToString:@""])
		{
			[wxMAIN ShowMessage:@"CSound External Gui not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
		}
		compiler = [wxDefaults valueForKey:@"CSoundExternalGuiPath"];
	}
	
	
	//-a /Applications/QuteCsound.app --args "/Users/teto/Desktop/113.csd"
	NSTask* task = [[NSTask alloc] init];
	[task setLaunchPath:@"/usr/bin/open"];
	//[task setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
	[task setArguments:[NSArray arrayWithObjects:
						@"-a", 
						compiler,
						@"--args",
						[self fileName], //[self fileName],
						nil]]; 
	
	[task launch];
	[task release];
	
	//The following method doesn't function for qutecsound probably because it expect argv arguments passed:
	//[[NSWorkspace sharedWorkspace] openFile:[self fileName] withApplication:compiler];
	
}

//----------------------------------------------------------------------------------------------------------
// COMPILER ACTIONS
//----------------------------------------------------------------------------------------------------------










#pragma mark - Configurations
//----------------------------------------------------------------------------------------------------------
// VARIOUS CONFIGURATIONS
//----------------------------------------------------------------------------------------------------------
- (void) configureEditor
{
	//Reset all configurations
	[textEditor styleResetToDefault];
	[textEditor styleClearAll];

	
	//Switch configuration to file type
	if([[self fileType] isEqualToString:@"Python Files"])
	{
		//Python syntax
		//if (wxGlobal.Settings.EditorProperties.UseMixedPython)
		//[[wxDefaults valueForKey:@"EditorUseMixedPython"] boolValue]]
		if([[wxDefaults valueForKey:@"EditorUseMixedPython"] boolValue] == true)
		{
			////editor.SetHighlight("winxoundpython", "");
			[self configureEditorForPythonMixed: [wxMAIN getOpcodes]];
		}
		else
		{
			////editor.SetHighlight("python", "");
			[self configureEditorForPython];
		}
		//Refresh syntax
		[textEditor refreshSyntax];
	}
	//else if ([[filename lowercaseString] hasSuffix:@".lua"])
	else if([[self fileType] isEqualToString:@"Lua Files"])
	{
		////editor.SetHighlight("lua", "");
		[self configureEditorForLua];
	}
	else if([[self fileType] isEqualToString:@"CSound Files"] ||
			[[self fileType] isEqualToString:@"CSound Orc Files"] ||
			[[self fileType] isEqualToString:@"CSound Sco Files"] ||
			[[self fileType] isEqualToString:@"CSound Cabbage Files"] )
	{
		//CSound syntax
		[self configureEditorForCSound:[wxMAIN getOpcodes]];
	}
	else
	{
		[self configureEditorForNonSyntaxFiles];
	}


	
	//TextEditor common settings:
	[textEditor setTextEditorFont:
	 [NSFont fontWithName: [wxDefaults valueForKey:@"EditorFontName"]
					 size: [[wxDefaults valueForKey:@"EditorFontSize"] floatValue]]];
	[textEditor setShowLineNumbers:[[wxDefaults valueForKey:@"EditorShowLineNumbers"] boolValue]]; 
	[self showExplorer:[[wxDefaults valueForKey:@"EditorShowExplorer"] boolValue]]; 
	[self showOnlineHelp: [[wxDefaults valueForKey:@"EditorShowIntellitip"] boolValue]]; 
	[self showToolbar: [[wxDefaults valueForKey:@"EditorShowToolbar"] boolValue]]; 
	[textEditor setShowSpaces:false];
	[textEditor setShowEOLMarker:false];
	[textEditor setShowMatchingBracket:[[wxDefaults valueForKey:@"EditorShowMatchingBracket"] boolValue]];
	[textEditor	setShowVerticalRuler:[[wxDefaults valueForKey:@"EditorShowVerticalRuler"] boolValue]];
	[textEditor	setMarkCaretLine:[[wxDefaults valueForKey:@"EditorMarkCaretLine"] boolValue]];
	[textEditor setTabIndent:[[wxDefaults valueForKey:@"EditorTabIndent"] integerValue]];
	
	
	[textEditor setZoom:0];
	
	
//	//SET EOL MODE: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2)
//	if([[textEditor getText] rangeOfString:@"\r\n"].location != NSNotFound)
//	{
//		[textEditor setEolMode:SC_EOL_CRLF];
//	}
//	else if([[textEditor getText] rangeOfString:@"\r"].location != NSNotFound)
//	{
//		[textEditor setEolMode:SC_EOL_CR];
//	}
//	else 
//		[textEditor setEolMode:SC_EOL_LF];


		
	//TODO: HORIZONTAL SCROLLBAR
	//[textEditor setScrollWidth: 600];
	[textEditor setScrollWidth: [[textEditor getPrimaryView] getTextRectangleWidth]];
	[textEditor setScrollWidthTracking: true];

	

	//Associate the context menu
	[textEditor setContextMenu:contextMenu];
	
	
	
	//Add events - notifications handling
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_UPDATEUI_Notification:) 
												 name:SCIUpdateUINotification 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_UPDATEUI_Notification:) 
												 name:SCIUpdateUINotification 
											   object:[textEditor getSecondaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_MODIFIED_Notification:) 
												 name:NSTextDidChangeNotification 
											   object:[textEditor getPrimaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_ZOOM_Notification:) 
												 name:@"SCIZoomChanged" 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_ZOOM_Notification:) 
												 name:@"SCIZoomChanged" 
											   object:[textEditor getSecondaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_LEFTMOUSE_Notification:) 
												 name:@"SCILeftMouseDown" 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_LEFTMOUSE_Notification:) 
												 name:@"SCILeftMouseDown" 
											   object:[textEditor getSecondaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_LEFTMOUSEHYPERLINKS_Notification:) 
												 name:@"SCILeftMouseDownHyperlinks" 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_LEFTMOUSEHYPERLINKS_Notification:) 
												 name:@"SCILeftMouseDownHyperlinks" 
											   object:[textEditor getSecondaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_RIGHTMOUSE_Notification:) 
												 name:@"SCIRightMouseDown" 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_RIGHTMOUSE_Notification:) 
												 name:@"SCIRightMouseDown" 
											   object:[textEditor getSecondaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_ESCAPEKEYDOWN_Notification:) 
												 name:@"SCIEscapeKeyDown" 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_ESCAPEKEYDOWN_Notification:) 
												 name:@"SCIEscapeKeyDown" 
											   object:[textEditor getSecondaryView]];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_LEFTMOUSE_QUOTES_Notification:) 
												 name:@"SCILeftMouseSelectQuotes" 
											   object:[textEditor getPrimaryView]];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_LEFTMOUSE_QUOTES_Notification:) 
												 name:@"SCILeftMouseSelectQuotes" 
											   object:[textEditor getSecondaryView]];
	
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_MOD_CONTAINER_Notification:) 
												 name:@"SCIModContainer" 
											   object:[textEditor getPrimaryView]];
	
}

- (void) configureEditorForNonSyntaxFiles
{	
	for (NSInteger mIndex = 0; mIndex < 34; mIndex++)
	{
		[textEditor StyleSetFore:mIndex htmlcolor:@"#000000"];
		[textEditor StyleSetBack:mIndex htmlcolor:@"#FFFFFF"];
		[textEditor StyleSetBold:mIndex bold:false];
		[textEditor StyleSetItalic:mIndex italic:false];
	}
	
	//MARGINS STYLE "33"
	[textEditor StyleSetFore:33 htmlcolor:@"#828282"]; //F0F0F0
	[textEditor StyleSetBack:33 htmlcolor:@"#F4F4F4"]; //808080
	
	//TEXT SELECTION
	//	[textEditor setSelFore:true htmlcolor:@"#000000"];
	[textEditor setSelBack:true htmlcolor:@"#A0C8FF"];

	//CARET COLOR (Same as Text Fore Color)
	[textEditor setCaretFore:@"#000000"];
	
}

/*
 
 //CABBAGE
 string CabbageWidgets = "form rslider hslider vslider " +
 "button checkbox combobox groupbox keyboard";
 string CabbageWords = "channel pos caption size value  " +
 "onoffCaption items colour ";
 
 textEditor.SetKeyWords(0, KeyWordList + CabbageWords);
 textEditor.SetKeyWords(2, CabbageWidgets);
 
 string TagWordList = "<CsVersion> </CsVersion> " +
 "<CsoundSynthesizer> </CsoundSynthesizer> " +
 "<CsOptions> </CsOptions> " +
 "<CsInstruments> </CsInstruments> " +
 "<CsScore> </CsScore> " +
 "<CsVersion> </CsVersion> " +
 "<CsLicence> </CsLicence> " +
 "<Cabbage> </Cabbage> ";
 textEditor.SetKeyWords(1, TagWordList);
 
 textEditor.SetKeyWords(3, " instr endin ");
 
 
 */
 
 - (void) configureEditorForCSound:(NSDictionary*) opcodes
{

	_Language = @"csound";
	[textEditor setHighlight:@"winxound"];
	
	if (opcodes != nil)
	{
		//Set Keywords list
		NSMutableString* KeyWordList = [[NSMutableString alloc] init];
		
		for(NSString *aKey in opcodes)
		{
			if ([aKey isEqualToString:@"instr"] || [aKey isEqualToString:@"endin"]) continue;

			[KeyWordList appendString:aKey];
			[KeyWordList appendString:@" "];
		}
		
		//Add keywords for Cabbage:
		NSString* CabbageWidgets = @"form rslider hslider vslider "
								    "button checkbox combobox groupbox keyboard";
		NSString* CabbageWords = @"channel pos caption size value  "
								  "onoffCaption items colour ";
		[KeyWordList appendString:CabbageWords];

		
		[textEditor setKeyWords:0 keyWordList: [KeyWordList copy]];
		//[textEditor setKeyWords:0 keyWordList:[NSString stringWithString:KeyWordList]];
		//textEditor.SetKeyWords(0, KeyWordList);
		[KeyWordList release];
		
		NSString* TagWordList = @"<CsVersion> </CsVersion> "
							    "<CsoundSynthesizer> </CsoundSynthesizer> " 
								"<CsOptions> </CsOptions> "
								"<CsInstruments> </CsInstruments> " 
								"<CsScore> </CsScore> "  
								"<CsVersion> </CsVersion> " 
								"<CsLicence> </CsLicence> "
								"<Cabbage> </Cabbage> ";
		
		[textEditor setKeyWords:1 keyWordList: TagWordList];
		
		[textEditor setKeyWords:2 keyWordList: CabbageWidgets];
		
		[textEditor setKeyWords:3 keyWordList: @"instr endin "];
		
	}
	
	[textEditor setWordChars:@"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_"];
	
	[self SciEditSetFontsAndStyles];

}



- (void) configureEditorForLua
{

	_Language = @"lua";
	[textEditor setHighlight:@"lua"];
		
	//Set Keywords list
	NSMutableString* keywords = [[[NSMutableString alloc] init] autorelease];
	[keywords appendString:
		@"and break do else elseif " 
		"end false for function if " 
		"in local nil not or repeat return " 
		"then true until while "];
	
	NSMutableString* keywords2 = [[[NSMutableString alloc] init] autorelease];
	[keywords appendString:
		@"_VERSION assert collectgarbage dofile " 
		"error gcinfo loadfile loadstring " 
		"print rawget rawset require tonumber " 
		"tostring type unpack "];
	
	NSMutableString* keywords3 = [[[NSMutableString alloc] init] autorelease];
	NSMutableString* keywords4 = [[[NSMutableString alloc] init] autorelease];
	
	//Add Lua 4:
	[keywords2 appendString:
		@"_ALERT _ERRORMESSAGE _INPUT _PROMPT _OUTPUT " 
		"_STDERR _STDIN _STDOUT call dostring foreach " 
		"foreachi getn globals newtype" 
		"sort tinsert tremove "];
	
	[keywords3 appendString:
		@"abs acos asin atan atan2 ceil cos deg exp " 
		"floor format frexp gsub ldexp log log10 max " 
		"min mod rad random randomseed " 
		"sin sqrt strbyte strchar strfind strlen " 
		"strlower strrep strsub strupper tan "];
	
	[keywords4 appendString:
		@"openfile closefile readfrom writeto appendto " 
		"remove rename flush seek tmpfile tmpname read write " 
		"clock date difftime execute exit getenv setlocale time "];
	
	//Add Lua 5:
	[keywords2 appendString:
		@"_G getfenv getmetatable ipairs loadlib next pairs pcall " 
		"rawequal setfenv setmetatable xpcall " 
		"string table math coroutine io os debug " 
		"load module select "];
	
	[keywords3 appendString:
		@"string.byte string.char string.dump string.find " 
		"string.len string.lower string.rep string.sub " 
		"string.upper string.format string.gfind string.gsub " 
		"table.concat table.foreach table.foreachi table.getn " 
		"table.sort table.insert table.remove table.setn " 
		"math.abs math.acos math.asin math.atan math.atan2 " 
		"math.ceil math.cos math.deg math.exp " 
		"math.floor math.frexp math.ldexp math.log " 
		"math.log10 math.max math.min math.mod " 
		"math.pi math.pow math.rad math.random math.randomseed " 
		"math.sin math.sqrt math.tan string.gmatch string.match " 
		"string.reverse table.maxn math.cosh math.fmod " 
		"math.modf math.sinh math.tanh math.huge "];
	
	[keywords4 appendString:
		@"coroutine.create coroutine.resume coroutine.status " 
		"coroutine.wrap coroutine.yield io.close io.flush " 
		"io.input io.lines io.open io.output io.read io.tmpfile " 
		"io.type io.write io.stdin io.stdout io.stderr " 
		"os.clock os.date os.difftime os.execute os.exit " 
		"os.getenv os.remove os.rename " 
		"os.setlocale os.time os.tmpname " 
		"coroutine.running package.cpath package.loaded " 
		"package.loadlib package.path package.preload " 
		"package.seeall io.popen "];
	
	
//	[textEditor setKeyWords:0 keyWordList: keywords];
//	[textEditor setKeyWords:1 keyWordList: keywords2];
//	[textEditor setKeyWords:2 keyWordList: keywords3];
//	[textEditor setKeyWords:3 keyWordList: keywords4];
	
	[textEditor setKeyWords:0 keyWordList: [NSString stringWithString:keywords]];
	[textEditor setKeyWords:1 keyWordList: [NSString stringWithString:keywords2]];
	[textEditor setKeyWords:2 keyWordList: [NSString stringWithString:keywords3]];
	[textEditor setKeyWords:3 keyWordList: [NSString stringWithString:keywords4]];
	
	[self SciEditSetFontsAndStyles];

}

- (void) configureEditorForPython
{

	_Language = @"python";
	[textEditor setHighlight:@"python"];
	
	//Set Keywords list
	NSString* keywords = @"and as assert break class continue def del elif " 
						"else except exec finally for from global if import " 
						"in is lambda None not or pass print raise return " 
						"try while with yield";
	
	//textEditor.SetKeyWords(0, keywords);
	[textEditor setKeyWords:0 keyWordList: keywords];
	
	[self SciEditSetFontsAndStyles];
		
}

- (void) configureEditorForPythonMixed:(NSDictionary*) opcodes
{
	
	_Language = @"python";
	[textEditor setHighlight:@"winxoundpython"];
	
	//Set Keywords list
	NSString* keywords = @"and as assert break class continue def del elif " 
						"else except exec finally for from global if import " 
						"in is lambda None not or pass print raise return " 
						"try while with yield";
	
	//textEditor.SetKeyWords(0, keywords);
	[textEditor setKeyWords:0 keyWordList: keywords];
	
	if (opcodes != nil)
	{
		//Set Keywords list
		NSMutableString* KeyWordList = [[NSMutableString alloc] init];
		
		for(NSString *aKey in opcodes)
		{
			if ([aKey isEqualToString:@"instr"] || [aKey isEqualToString:@"endin"]) continue;
			
			[KeyWordList appendString:aKey];
			[KeyWordList appendString:@" "];
		}

		[textEditor setKeyWords:1 keyWordList: [KeyWordList copy]];
		[KeyWordList release];
		
	}

	
	[self SciEditSetFontsAndStyles];
		
}




- (void) SciEditSetFontsAndStyles
{
	
	//Set styles from 0 to 33
	//STYLE_NUMBERS_MARGINS = 33;
	for (NSInteger mIndex = 0; mIndex < 34; mIndex++)
	{
		[textEditor StyleSetFore:mIndex htmlcolor:[[wxMAIN getSettings] StyleGetForeColor:_Language stylenumber:mIndex]];
		
		[textEditor StyleSetBack:mIndex htmlcolor:[[wxMAIN getSettings] StyleGetBackColor:_Language stylenumber:mIndex]];
		
		[textEditor StyleSetBold:mIndex bold:[[wxMAIN getSettings] StyleGetBold:_Language stylenumber:mIndex]];
		
		[textEditor StyleSetItalic:mIndex italic:[[wxMAIN getSettings] StyleGetItalic:_Language stylenumber:mIndex]];
	}
	
	//DEFAULT STYLE "34" STYLE_BRACELIGHT
	[textEditor StyleSetFore:STYLE_BRACELIGHT 
				   htmlcolor:[[wxMAIN getSettings] StyleGetForeColor:_Language stylenumber:32]]; //Same as TextForeColor
	[textEditor StyleSetBack:STYLE_BRACELIGHT 
				   htmlcolor:[[wxMAIN getSettings] StyleGetBackColor:_Language stylenumber:256]]; //TextSelectionBackColor
	
	
	//TEXT SELECTION (style 256)
//	[textEditor setSelFore:true 
//				 htmlcolor:[[wxMAIN getSettings] StyleGetForeColor:_Language stylenumber:256]];
	[textEditor setSelBack:true 
				   htmlcolor:[[wxMAIN getSettings] StyleGetBackColor:_Language stylenumber:256]];
	
	
	//BOOKMARKS (style 257)
	[textEditor MarkerSetBack:0 
				 htmlcolor:[[wxMAIN getSettings] StyleGetBackColor:_Language stylenumber:257]];
	[textEditor MarkerSetAlpha:0 
				 intcolor:[[wxMAIN getSettings] StyleGetAlpha:_Language stylenumber:257]];
	
	
	//VERTICAL RULER (style 258)
	[textEditor setEdgeColor:[[wxMAIN getSettings] StyleGetForeColor:_Language stylenumber:258]];

	
	//CARET LINE MARKER (style 259)
	[textEditor setCaretLineBack:[[wxMAIN getSettings] StyleGetForeColor:_Language stylenumber:259]];

	
	//CARET COLOR (Same as Text Fore Color)
	[textEditor setCaretFore:[[wxMAIN getSettings] StyleGetForeColor:_Language stylenumber:32]];
	
		

//	// Line number style.
//	[mEditor setColorProperty: SCI_STYLESETFORE parameter: STYLE_LINENUMBER fromHTML: @"#F0F0F0"];
//	[mEditor setColorProperty: SCI_STYLESETBACK parameter: STYLE_LINENUMBER fromHTML: @"#808080"];
	

}



- (void) configureCompiler
{
	//Compiler Output settings (NSTextView)
	////[compilerOutput setFont:[NSFont fontWithName:@"Andale Mono" size:10]];
	[compilerOutput setFont:
	 [NSFont fontWithName: [wxDefaults valueForKey:@"CompilerFontName"]
					 size: [[wxDefaults valueForKey:@"CompilerFontSize"] floatValue]]];
	
	[compilerOutput setGrammarCheckingEnabled:false];
	[compilerOutput setContinuousSpellCheckingEnabled:false];
	[compilerOutput setAutomaticLinkDetectionEnabled:false];
	[compilerOutput setAutomaticQuoteSubstitutionEnabled:false];
	[compilerOutput setAcceptsGlyphInfo:false];
	[compilerOutput setRichText:false];
	
	[self setUseWinXoundFlags: [[wxDefaults valueForKey:@"UseWinxoundFlags"] boolValue]];
}

- (void) configureExplorer
{
	NSTableColumn *tableColumn = [[treeViewStructure tableColumns] objectAtIndex:0];
	ImageAndTextCell *imageAndTextCell = [[[ImageAndTextCell alloc] init] autorelease];
	
	[imageAndTextCell setEditable:NO];
	NSInteger fSize = 11 + [[wxDefaults valueForKey:@"ExplorerFontSize"] integerValue];
	[imageAndTextCell setFont:[NSFont systemFontOfSize:fSize]];
	
	[tableColumn setDataCell:imageAndTextCell];
	[treeViewStructure setAutoresizesOutlineColumn:true];
	
	
	//[self refreshExplorer];
	//-TIMER for FindStructure operation
	timerCounter = 0;
	isChanged = true;
}

- (void) setTextContent:(NSString*)text
{
	[[textEditor getPrimaryView] setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
	[textEditor setText:text];
	[textEditor ConvertEOL:[textEditor GetEOLMode]];
	[[textEditor getPrimaryView] setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
}

- (void) checkLineEndings
{
	
	//Check Line Endings (look also for consistence: aka mixed eols)
	//SET EOL MODE: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2)
	
	bool FileIsNotConsistent = false;
	
	NSLog(@"checkLineEndings: START");
	
	//string s = editor.textEditor.GetText().Replace("\r\n", "");
	NSString* s = [[textEditor getText] stringByReplacingOccurrencesOfString:@"\r\n" withString:@""];

	
	//(editor.textEditor.GetTextLength() - s.Length) / 2;
	NSInteger crlfOccurrences = ([textEditor getTextLength] - [s length]) / 2; 
	NSInteger crOccurrences = 0;
	NSInteger lfOccurrences = 0;
	
	
	//Check if CRLF correspond to TextEditor lines - 1
	//If not check also for LF
	if (crlfOccurrences != [textEditor getLinesCount] - 1)
	{
		//Check if LF correspond to TextEditor lines - 1
		//If not check also for CR
		lfOccurrences = [s length] - [[s stringByReplacingOccurrencesOfString:@"\n" withString:@""] length];
		if (lfOccurrences != [textEditor getLinesCount] - 1)
		{
			crOccurrences = [s length] - [[s stringByReplacingOccurrencesOfString:@"\r" withString:@""] length];
		}
	}
	

//	NSString* report = [NSString stringWithFormat:
//					   @"Filename: %@\n"
//					   "CRLF:\t%d\n"
//					   "CR:\t%d\n"
//					   "LF:\t%d",
//					   [self fileName],
//					   crlfOccurrences,
//					   crOccurrences, 
//					   lfOccurrences];
//					   
//	NSLog(@"Report: %@", report);
	
	
	//CRLF
	if (crlfOccurrences > crOccurrences &&
		crlfOccurrences > lfOccurrences)
	{
		//Set EOL mode to CRLF
		[textEditor setEolMode:SC_EOL_CRLF];
		
		//Check for CRLF coherence
		if (crOccurrences > 0 || lfOccurrences > 0)
		{
			//Incoherent EOLS!!! Convert all to CRLF
			[textEditor ConvertEOL:SC_EOL_CRLF];
		}
	}
	
	//CR
	else if (crOccurrences > crlfOccurrences &&
			 crOccurrences > lfOccurrences)
	{
		//Set EOL mode to CR
		[textEditor setEolMode:SC_EOL_CR];
		
		//Check for CR coherence
		if (crlfOccurrences > 0 || lfOccurrences > 0)
		{
			//Incoherent EOLS!!! Convert all to CR
			[textEditor ConvertEOL:SC_EOL_CR];
		}
	}
	
	//LF
	else if (lfOccurrences > crlfOccurrences &&
			 lfOccurrences > crOccurrences)
	{
		//Set EOL mode to LF
		[textEditor setEolMode:SC_EOL_LF];
		
		//Check for LF coherence
		if (crlfOccurrences > 0 || crOccurrences > 0)
		{
			//Incoherent EOLS!!! Convert all to LF
			[textEditor ConvertEOL:SC_EOL_LF];
		}
	}
	
	else
	{
		//Convert and Set default LF mode for OsX
		[textEditor ConvertEOL:SC_EOL_LF];
		[textEditor setEolMode:SC_EOL_LF];
	}
	
	if (FileIsNotConsistent)
	{
		//Save the file after conversion ???
	}
	
	NSLog(@"checkLineEndings: END");
}
//----------------------------------------------------------------------------------------------------------
// VARIOUS CONFIGURATIONS
//----------------------------------------------------------------------------------------------------------






//Various Utility stuffs
- (wxNode*) retrieveNodeByName:(NSString*) nodeName
{
	wxNode* node = nil;
	
	@try
	{
		for(NSInteger index = 0; index < [treeViewStructure numberOfRows]; index++)
		{
			//NSLog(@"%@", [[treeViewStructure itemAtRow:index] name]);
			if([[treeViewStructure itemAtRow:index] name] == nodeName)
			{
				node = [treeViewStructure itemAtRow:index];
				//NSLog(@"%@", [[treeViewStructure itemAtRow:index] name]);
			}
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -> retrieveNodeByName Error: %@ - %@", [e name], [e reason]);
	}
	
	return node;
}

//Save bookmarks position
- (void) saveBookmarksPosition
{
	@try
	{
		if([[wxDefaults valueForKey:@"SaveBookmarks"] boolValue] &&
		   [textEditor hasBookmarks])
		{
			NSString* languagecomment = nil;
			
			if([[self fileType] isEqualToString:@"CSound Files"])
				languagecomment = @";";
			else if([[self fileType] isEqualToString:@"Python Files"])
				languagecomment = @"#";
			else if([[self fileType] isEqualToString:@"Lua Files"])
				languagecomment = @"--";
			else return;
			
			
			//NSInteger ret = [textEditor FindText:@";[winxound_bookmarks" 
			NSInteger ret = [textEditor FindText:[NSString stringWithFormat:@"%@[winxound_bookmarks", languagecomment]
								  MatchWholeWord:true 
									   MatchCase:true 
									  IsBackward:false 
									  SelectText:false 
									 ShowMessage:false 
										 SkipRem:false
										   start:0
											 end:-1];
			
			if (ret > -1)
			{
				//NSString* lineText = [textEditor getTextOfLine:[textEditor getLineNumberFromPosition:ret]];
				//[textEditor ReplaceTarget:ret - 1
				//				   length:[textEditor getLineLength:[textEditor getLineNumberFromPosition:ret]] + 1
				//			ReplaceString:@""];
				[textEditor ReplaceTarget:ret
								   length:[textEditor getLineLength:[textEditor getLineNumberFromPosition:ret]]
							ReplaceString:@""];
			}
			
			
			////[textEditor AppendText:@"\n;[winxound_bookmarks"];
			////[textEditor AppendText:[NSString stringWithFormat:@"\n%@[winxound_bookmarks", languagecomment]];
			if ([[textEditor getText] hasSuffix:@"\n"] ||
				[[textEditor getText] hasSuffix:@"\r"])
			{
				//editor.textEditor.AppendText(languagecomment + "[winxound_bookmarks");
				[textEditor AppendText:[NSString stringWithFormat:@"%@[winxound_bookmarks", languagecomment]];
			}
			else
			{
				//editor.textEditor.AppendText(editor.textEditor.NewLine + languagecomment + "[winxound_bookmarks");
				[textEditor AppendText:[NSString stringWithFormat:@"%@%@[winxound_bookmarks", [textEditor newLine], languagecomment]];
			}
			
			NSInteger mCurLine = 0;
			NSInteger mBookLine = 0;
			do
			{
				//mBookLine = textEditor.PrimaryView.MarkerNext(CurLine, 1);
				mBookLine = [textEditor MarkerNext:mCurLine markerMask:1];
				if (mBookLine == -1) break;
				
				[textEditor AppendText:[NSString stringWithFormat:@",%d", mBookLine]];
				mCurLine = mBookLine + 1;
			}
			while (true);
			[textEditor AppendText:[NSString stringWithFormat:@"]", mBookLine]];
		}
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxDocument -> saveBookmarksPosition:" 
						 error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		//NSLog(@"wxDocument -> saveBookmarksPosition Error: %@ - %@", [e name], [e reason]);
	}
}

//Method to check if the document is a coloured and structured document
- (BOOL)isSyntaxType
{
	if([self fileType] != nil)
	{
		if([[self fileType] isEqualToString:@"CSound Files"] ||
		   [[self fileType] isEqualToString:@"CSound Orc Files"] ||
		   [[self fileType] isEqualToString:@"CSound Sco Files"] ||
		   [[self fileType] isEqualToString:@"CSound Cabbage Files"] ||
		   [[self fileType] isEqualToString:@"Python Files"] ||
		   [[self fileType] isEqualToString:@"Lua Files"])
		{
			return true;
		}
	}
	
	return false;
}

//Various Utility stuffs






#pragma mark - Show/Hide Tools
//----------------------------------------------------------------------------------------------------------
// SHOW - HIDE TOOLS (and relative stuffs)
//----------------------------------------------------------------------------------------------------------
- (void) showLineNumbers:(BOOL)value
{
	[textEditor setShowLineNumbers:value];
}

- (void) showExplorer:(BOOL)value
{
	if(value == false)
	{
		//HIDE EXPLORER
		[mainSplitWindow setPosition:0 ofDividerAtIndex:0];
		[treeViewStructureHost setHidden:true];
	}
	else
	{
		//SHOW EXPLORER
		[mainSplitWindow setPosition:200 ofDividerAtIndex:0];
		[treeViewStructureHost setHidden:false];
	}
}

- (void) showOnlineHelp:(BOOL)value
{
	/*
	if(value == true)
	{
		//SHOW INTELLITIP
		[intelliTipHost setHidden:false];
		[textEditorHost setFrame:NSMakeRect([textEditorHost frame].origin.x, 
											[intelliTipHost bounds].size.height + 6, //3 
											[textEditorHost frame].size.width, 
											[codeView frame].size.height - [intelliTipHost bounds].size.height - 10)];  //4
	}
	else 
	{
		//HIDE INTELLITIP
		[intelliTipHost setHidden:true];
		[textEditorHost setFrame:NSMakeRect([textEditorHost frame].origin.x, 
											4, 
											[textEditorHost frame].size.width, 
											[codeView frame].size.height - 8)];
	}
	*/
	
	NSInteger offset = 0;
	if([self IsOrcScoFile])
		offset = LinkBoxHeight;
	
	
	if(value == true)
	{
		//SHOW INTELLITIP
		[intelliTipHost setHidden:false];
		[textEditorHost setFrame:NSMakeRect([textEditorHost frame].origin.x, 
											[intelliTipHost bounds].size.height + 6, //3 
											[textEditorHost frame].size.width, 
											[codeView frame].size.height - [intelliTipHost bounds].size.height - 10 - offset)];  //4
	}
	else 
	{
		//HIDE INTELLITIP
		[intelliTipHost setHidden:true];
		[textEditorHost setFrame:NSMakeRect([textEditorHost frame].origin.x, 
											4, 
											[textEditorHost frame].size.width, 
											[codeView frame].size.height - 8 - offset)];
		
	}
	
}

- (void) showToolbar:(BOOL)value
{
	[toolbar setVisible:value];
}




//OTHER METHODS
//Switch between WinXound flags or CsOptions flags 
- (void) setUseWinXoundFlags:(BOOL) value
{	
	if(value == true)
	{
		[compilerHost setTitle:@"Compiler [WinXound flags]"];
	}
	else 
	{
		[compilerHost setTitle:@"Compiler [CsOptions flags]"];
	}
}

//Retrieve CsInstruments and CsScore positions (for Splitting view and Show Orc and Sco positions)
- (void) showOrcSco
{
	if (![textEditor getIsSplitted]) return;
	
	
	NSInteger mFindPos = -1;
	NSString* StringToFind = nil;
	
	//Find <CsInstruments> tag
	StringToFind = @"<CsInstruments>";
	
	mFindPos = [textEditor FindTextEx:StringToFind 
					   MatchWholeWord:true 
							MatchCase:true 
						   IsBackward:false 
						   SelectText:false 
						  ShowMessage:false 
							  SkipRem:true
								start:0
								  end:[textEditor getTextLength]
							 useRegEx:false];
	
	if (mFindPos > -1)
	{
		//ActiveEditor.SetFocusOnPrimaryView();
		[textEditor setFocusOnPrimaryView];
		//ActiveEditor.textEditor.SetCaretPosition(mFindPos);
		[textEditor setCaretPosition:mFindPos];
		//ActiveEditor.textEditor.SetFirstVisibleLine(ActiveEditor.textEditor.GetLineNumberFromPosition(mFindPos), 1);
		[textEditor setFirstVisibleLineAtView:[textEditor getLineNumberFromPosition:mFindPos] view:1];
	}
	
	//Find <CsScore> 
	StringToFind = @"<CsScore>";

	mFindPos = [textEditor FindTextEx:StringToFind 
					   MatchWholeWord:true 
							MatchCase:true 
						   IsBackward:false 
						   SelectText:false 
						  ShowMessage:false 
							  SkipRem:true
								start:0
								  end:[textEditor getTextLength]
							 useRegEx:false];
	
	if (mFindPos > -1)
	{
		//ActiveEditor.SetFocusOnSecondaryView();
		[textEditor setFocusOnSecondaryView];
		//ActiveEditor.textEditor.SetCaretPosition(mFindPos);
		[textEditor setCaretPosition:mFindPos];
		//ActiveEditor.textEditor.SetFirstVisibleLine(ActiveEditor.textEditor.GetLineNumberFromPosition(mFindPos), 2);
		[textEditor setFirstVisibleLineAtView:[textEditor getLineNumberFromPosition:mFindPos] view:2];
	}
	
	[textEditor setFocusOnPrimaryView];
	
}
//----------------------------------------------------------------------------------------------------------
// SHOW - HIDE TOOLS
//----------------------------------------------------------------------------------------------------------

















#pragma mark - NSOutline Data Source
//----------------------------------------------------------------------------------------------------------
// NSOUTLINE OVERRIDES
//----------------------------------------------------------------------------------------------------------
- (NSInteger)outlineView:(NSOutlineView *)outlineView numberOfChildrenOfItem:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		return [node.children count];
	}
	
	return 0;
}

- (id)outlineView:(NSOutlineView *)outlineView child:(NSInteger)index ofItem:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		return [node.children objectAtIndex:index];
	}
	
	return nil;
}

- (BOOL)outlineView:(NSOutlineView *)outlineView isItemExpandable:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		//return node.isGroup;
		if(node.children != nil)
			return ([node.children count] > 0);
	}
	
	return false;
}

- (id)outlineView:(NSOutlineView *)outlineView objectValueForTableColumn:(NSTableColumn *)tableColumn byItem:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		return node.name;
	}
	
	return nil;
}

- (void) outlineView:(NSOutlineView *)outlineView willDisplayCell:(id)cell forTableColumn:(NSTableColumn *)tableColumn item:(id)item
{
	
	wxNode *node = (wxNode*)item;
	if ([cell isKindOfClass:[ImageAndTextCell class]])
	{
		
		NSColor* color = nil;
		
		if([node.name isEqualToString:@"Bookmarks"])
			color = [NSColor colorWithCalibratedRed:.14 green:.14 blue:.14 alpha:1];
		//image = exGrey;
		
		else if([[[outlineView parentForItem:node] name] isEqualToString:@"<CsOptions>"])
			color = [NSColor colorWithCalibratedRed:0 green:.25 blue:.02 alpha:1];
		//image = exGreen;
		
		else if([[[outlineView parentForItem:node] name] isEqualToString:@"Bookmarks"])
			color = [NSColor colorWithCalibratedRed:.7 green:.7 blue:.7 alpha:1];
		//image = exWhite;	
		
		else if([[[outlineView parentForItem:node] name] isEqualToString:@"<CsScore>"])
		{
			if([node.name hasPrefix:@"f"])
			{
				color = [NSColor colorWithCalibratedRed:.84 green:.63 blue:.39 alpha:1];
			}
			else if([node.name hasPrefix:@"#def"])
			{
				color = [NSColor colorWithCalibratedRed:.74 green:.25 blue:.73 alpha:1];
			}
			else if([node.name hasPrefix:@"s"])
			{
				color = [NSColor colorWithCalibratedRed:.74 green:.14 blue:.14 alpha:1];
			}
		}	
		
		else if ([node.name isEqualToString:@"<CsInstruments>"])
			color = [NSColor colorWithCalibratedRed:.55 green:0 blue:0 alpha:1];
		//image = exRed;
		
		else if ([node.name isEqualToString:@"<CsoundSynthesizer>"])
			color = [NSColor colorWithCalibratedRed:.55 green:0 blue:0 alpha:1];
		//image = exRed;
		
		else if ([node.name isEqualToString:@"<CsScore>"])
			color = [NSColor colorWithCalibratedRed:.55 green:0 blue:0 alpha:1];
		//image = exRed;
		
		else if ([node.name isEqualToString:@"<CsOptions>"])
			color = [NSColor colorWithCalibratedRed:.55 green:0 blue:0 alpha:1];
		//image = exRed;
		
		else if([node.name hasPrefix:@"instr"])
			color = [NSColor colorWithCalibratedRed:.047 green:.36 blue:.53 alpha:1];
		//image = exBlue;
		
		else if([node.name hasPrefix:@"#def"])
			color = [NSColor colorWithCalibratedRed:.74 green:.25 blue:.73 alpha:1];
		//image = exViolet;
		
		else if([node.name hasPrefix:@"opc"])
			color = [NSColor colorWithCalibratedRed:1 green:.42 blue:0 alpha:1];
		//image = exOrange;
		
		else
			color = [NSColor colorWithCalibratedRed:.89 green:.89 blue:.89 alpha:1];
		//image = exWhite;
		
		
		//NSImage* image = [[[NSImage alloc] initWithContentsOfFile: path] autorelease];
		//NSImage* iconImage = //[[NSWorkspace sharedWorkspace] iconForFile:urlStr];
		
		//[(ImageAndTextCell*)cell setImage:image];
		[(ImageAndTextCell*)cell setColor:color];
	}
	
	
}


- (NSCell *)outlineView:(NSOutlineView *)outlineView dataCellForTableColumn:(NSTableColumn *)tableColumn item:(id)item
{
	//NSCell* returnCell = [tableColumn dataCell];
	wxNode *node = (wxNode*)item;
	
	if ([node.name isEqualToString:@"---"])
	{
		// we are being asked for the cell for the single and only column
		//BaseNode* node = [item representedObject];
		//if ([node nodeIcon] == nil && [[node nodeTitle] length] == 0)
		return separatorCell;
	}
	
	return [tableColumn dataCell];
	
}


- (BOOL)outlineView:(NSOutlineView *)outlineView shouldSelectItem:(id)item
{
	wxNode *node = (wxNode*)item;
	if([node.name isEqualToString:@"---"] ||
	   [node.name isEqualToString:@"Bookmarks"])
		return false;
	
	return true;
}

//----------------------------------------------------------------------------------------------------------
// NSOUTLINE OVERRIDES
//----------------------------------------------------------------------------------------------------------













#pragma mark - Find Structure (Explorer)
//----------------------------------------------------------------------------------------------------------
// FIND STRUCTURE
//----------------------------------------------------------------------------------------------------------
- (void)findStructure:(NSString*)_text
{
	//This is a secondary thread started by the timer tick event
	
	if ([[NSThread currentThread] isCancelled]) return;
	
	
	NSLog(@"FIND STRUCTURE START");
	
	
	NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
	
	
	NSString* mFsKey = nil;
	NSString* mFsValue = nil;
	NSInteger mFsEnd = [_text length];
	NSInteger mFindRem = 0;
	

	wxNode* Root = [[wxNode alloc] init];
	wxNode* CsOptions = [[wxNode alloc] init];
	wxNode* CsInstruments = [[wxNode alloc] init];
	wxNode* CsScore = [[wxNode alloc] init];

	
	@try
	{
		if([[wxDefaults valueForKey:@"ExplorerShowOptions"] boolValue] && 
		   //[[self fileType] isEqualToString:@"CSound Files"])
		   ![[self fileType] isEqualToString:@"CSound Orc Files"] &&
		   ![[self fileType] isEqualToString:@"CSound Sco Files"]) 
		{
			if ([[NSThread currentThread] isCancelled]) return;
			//NSLog(@"thread inside findStructure: %@", [NSThread currentThread]);
			
			//OPTIONS
			NSInteger mStartCsOptions = [_text rangeOfString:@"<CsOptions>"
													 options:NSLiteralSearch
													   range:NSMakeRange(0, mFsEnd)].location;
			
			if (mStartCsOptions != NSNotFound)
			{
				mStartCsOptions += 11;
				NSInteger mEndCsOptions = [_text rangeOfString:@"</CsOptions>"
													   options:NSLiteralSearch
														 range:NSMakeRange(mStartCsOptions, mFsEnd - mStartCsOptions)].location;
				
				if (mEndCsOptions != NSNotFound)
				{
					NSString* inText = [_text substringWithRange:NSMakeRange(mStartCsOptions, mEndCsOptions - mStartCsOptions)];
					NSArray* lines = [inText componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]]; 
					
					for(NSString* s in lines)
					{
						//SKIP INVALID LINES (REM)
						mFindRem = [[s stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]] 
									rangeOfString:@";" options:NSLiteralSearch].location;
						
						if(mFindRem == 0)
						{
							continue;
						}
						else if([s length] > 0)
						{
							
							//[_editor getTextOfLine:mFsCurLine];
							//mFsValue = [_text substringWithRange:NSMakeRange(mFsFindPos.location, mFsCurLineEnd - mFsFindPos.location)]; 
							mFsValue = s;
							mFsValue = [mFsValue stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
							mFsKey = [self parseString:mFsValue];
							
							wxNode* nodeCsOptions = [[wxNode alloc] init];
							nodeCsOptions.name = mFsKey;
							nodeCsOptions.extendedname = mFsValue;
							[CsOptions.children addObject:nodeCsOptions];
							[nodeCsOptions release];
							break;
						}
						
					}
				}	
			}	
		}
		
		
		//CSINTRUMENTS: MACROS (#DEFINE)
		if([[wxDefaults valueForKey:@"ExplorerShowInstrMacros"] boolValue])
			[self findString:@"#define" inText:_text withNode:CsInstruments isScore:false];	
		
		//CSINTRUMENTS: OPCODE
		if([[wxDefaults valueForKey:@"ExplorerShowInstrOpcodes"] boolValue])
			[self findString:@"opcode" inText:_text withNode:CsInstruments isScore:false];	
		
		//CSINTRUMENTS: INSTR
		if([[wxDefaults valueForKey:@"ExplorerShowInstrInstruments"] boolValue])
			[self findString:@"instr" inText:_text withNode:CsInstruments isScore:false];		
		
		
		//CSSCORE: Functions
		if([[wxDefaults valueForKey:@"ExplorerShowScoreFunctions"] boolValue])
			[self findStringInScore:@"f" inText:_text withNode:CsScore];
		
		//CSSCORE: Macros
		if([[wxDefaults valueForKey:@"ExplorerShowScoreMacros"] boolValue])
			[self findString:@"#define" inText:_text withNode:CsScore isScore:true];	
		
		//CSSCORE: Sections
		if([[wxDefaults valueForKey:@"ExplorerShowScoreSections"] boolValue])
			[self findStringInScore:@"s" inText:_text withNode:CsScore];	
		
		
		
		
		//FINALLY
		//Add childrens to Root node
		[Root.children addObject:CsOptions];
		[Root.children addObject:CsInstruments];
		[Root.children addObject:CsScore];
		
		
		[CsOptions release];
		[CsInstruments release];
		[CsScore release];
		
	}
	
	
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -----------> findStructure Error: %@ - %@", [e name], [e reason]);
	}
	
	
	@finally 
	{
		NSLog(@"FIND STRUCTURE ENDED");
		
		//Call bgThreadIsDone on the main thread (JOB is finished: notify primary thread - refresh TreeViewStructure) 
		[self performSelectorOnMainThread:@selector(bgThreadIsDone:) withObject:Root waitUntilDone:NO];
		
		[pool release];
	}
	
}

- (BOOL) findStringInScore:(NSString*)stringToFind inText:(NSString*)_text withNode:(wxNode*)passedNode
{
	//CSSCORE
	
	if ([[NSThread currentThread] isCancelled]) return false;
	
	NSString* mFsKey = nil;
	NSString* mFsValue = nil;
	NSInteger mFsEnd = [_text length];
	NSInteger mFindRem = 0;
	
	
	NSInteger mStartCsScore = [_text rangeOfString:@"<CsScore>"
										   options:NSLiteralSearch
											 range:NSMakeRange(0, mFsEnd)].location;
	
	//Added for Orc/Sco:
	if(mStartCsScore == NSNotFound)
	{
		//If the file is not SCO we skip find
		if(![[self fileType] isEqualToString:@"CSound Sco Files"]) return false;
		mStartCsScore = 0;
	}
	
	
	if (mStartCsScore != NSNotFound)
	{
		mStartCsScore += 8;
		NSInteger mEndCsScore = [_text rangeOfString:@"</CsScore>"
											 options:NSLiteralSearch
											   range:NSMakeRange(mStartCsScore, mFsEnd - mStartCsScore)].location;
		
		if (mEndCsScore == NSNotFound) mEndCsScore = mFsEnd;
		
		if (mEndCsScore != NSNotFound)
		{
			NSString* inText = [_text substringWithRange:NSMakeRange(mStartCsScore, mEndCsScore - mStartCsScore)];
			NSArray* lines = [inText componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];
			
			NSInteger f = -1;
			NSInteger index = 1;
			
			for(NSString* s in lines)
			{
				//SKIP INVALID LINES (REM)
				mFindRem = [[s stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]] 
							rangeOfString:@";" options:NSLiteralSearch].location;
				
				if(mFindRem == 0)
				{
					continue;
				}
				else if([s length] > 0)
				{
					
					//[_editor getTextOfLine:mFsCurLine];
					//mFsValue = [_text substringWithRange:NSMakeRange(mFsFindPos.location, mFsCurLineEnd - mFsFindPos.location)];
					f = [[s stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]] 
						 rangeOfString:stringToFind options:NSLiteralSearch].location;
					if(f == 0)
					{
						mFsValue = s;
						mFsValue = [mFsValue stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
						mFsKey = [self parseString:mFsValue];
						
						if([stringToFind isEqualToString:@"s"])
						{
							mFsKey = [NSString stringWithFormat:@"%@ %d", mFsKey, index];
							index ++;
						}
						
						wxNode* nodeTemp = [[wxNode alloc] init];
						nodeTemp.name = mFsKey;
						nodeTemp.extendedname = mFsValue;
						[passedNode.children addObject:nodeTemp];
						[nodeTemp release];
					}
				}
			}
		}	
	}	
	
	return true;
}


- (BOOL) findString:(NSString*)stringToFind inText:(NSString*) _text withNode:(wxNode*)passedNode isScore:(BOOL)isScore
{
	@try 
	{
		NSString* mFsKey = nil;
		NSString* mFsValue = nil;
		NSInteger mFsStart = 0;
		NSRange   mFsFindPos;
		NSInteger mFsEnd = [_text length];
		NSInteger mFsCurLineEnd = 0;
		NSInteger startOfLine = 0;
		NSInteger mFindRem = 0;
		
		
		if(isScore) //Score
		{
			mFsStart = [_text rangeOfString:@"<CsScore>"
									options:NSLiteralSearch
									  range:NSMakeRange(0, mFsEnd)].location;
			
			mFsEnd = [_text rangeOfString:@"</CsScore>"
								  options:NSLiteralSearch
									range:NSMakeRange(0, [_text length])].location;
		}
		else //Instruments
		{
			mFsStart = [_text rangeOfString:@"<CsInstruments>"
									options:NSLiteralSearch
									  range:NSMakeRange(0, mFsEnd)].location;
			
			mFsEnd = [_text rangeOfString:@"</CsInstruments>"
								  options:NSLiteralSearch
									range:NSMakeRange(0, [_text length])].location;
		}
		
		
		//Added for Orc/Sco:
		//if(mFsStart == NSNotFound) mFsStart = 0;
		if(mFsStart == NSNotFound)
		{
			if([[self fileType] isEqualToString:@"CSound Files"]) return false;
			if([[self fileType] isEqualToString:@"CSound Orc Files"] &&
			   isScore == true) return false;
			
			mFsStart = 0;
		}
		
		if(mFsEnd == NSNotFound || mFsEnd > [_text length]) 
			mFsEnd = [_text length];
		
		
		//NSLog(@"thread inside findString: %@", [NSThread currentThread]);
		
		do
		{
			if ([[NSThread currentThread] isCancelled]) break;
			
			mFsFindPos = [_text rangeOfString:stringToFind
									  options:NSLiteralSearch
										range:NSMakeRange(mFsStart, mFsEnd - mFsStart)];
			
			
			//Find only whole words !!! (Aggiungere eventualmente il rem ; come possibile carattere iniziale
			if (mFsFindPos.location != NSNotFound && 
				[[[NSCharacterSet whitespaceAndNewlineCharacterSet] invertedSet] characterIsMember:
				 [_text characterAtIndex:mFsFindPos.location - 1]]) 
			{
				// Preceding character is alphanumeric
				mFsStart = mFsFindPos.location + [stringToFind length];
				continue;
			}
			if (mFsFindPos.location + mFsFindPos.length < [_text length] && 
				[[[NSCharacterSet whitespaceAndNewlineCharacterSet] invertedSet] characterIsMember:
				 [_text characterAtIndex:mFsFindPos.location + mFsFindPos.length]]) 
			{
				// Trailing character is alphanumeric
				mFsStart = mFsFindPos.location + [stringToFind length];
				continue;
			}
			
			
			
			if (mFsFindPos.location != NSNotFound)
			{
				
				//SKIP INVALID LINES (REM)
				startOfLine = [_text rangeOfCharacterFromSet:[NSCharacterSet newlineCharacterSet] 
													 options:NSBackwardsSearch 
													   range:NSMakeRange(0, mFsFindPos.location)].location + 1;
				
				if(startOfLine != NSNotFound)
				{
					mFindRem = [_text rangeOfString:@";" 
											options:NSLiteralSearch 
											  range:NSMakeRange(startOfLine, mFsFindPos.location - startOfLine)].location;
					
					if(mFindRem != NSNotFound)
					{
						mFsStart = mFsFindPos.location + [stringToFind length];
						continue;
					}
					
					//NSString* temp = [_text substringWithRange:NSMakeRange(startOfLine, mFsFindPos.location - startOfLine)];
					//NSLog(@"temp: %@", temp);
					//if([[temp stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]] length] != 0)
					//{
					//	mFsStart = mFsFindPos.location + [stringToFind length];
					//	continue;
					//}
				}
				else
				{
					mFsStart = mFsFindPos.location + [stringToFind length];
					continue;
				}
				
				
				
				
				mFsStart = mFsFindPos.location + [stringToFind length];		
				//mFsCurLine = [_editor getLineNumberFromPosition:mFsFindPos];
				mFsCurLineEnd = [_text rangeOfCharacterFromSet:[NSCharacterSet newlineCharacterSet] 
													   options:nil 
														 range:NSMakeRange(mFsStart, mFsEnd - mFsStart)].location;
				
				//[_editor getTextOfLine:mFsCurLine];
				//mFsValue = [_text substringWithRange:NSMakeRange(mFsFindPos.location, mFsCurLineEnd - mFsFindPos.location)]; 
				mFsValue = [_text substringWithRange:NSMakeRange(startOfLine, mFsCurLineEnd - startOfLine)];
				mFsValue = [mFsValue stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
				mFsKey = [self parseString:mFsValue];
				
				wxNode* nodeTemp = [[wxNode alloc] init];
				nodeTemp.name = mFsKey;
				nodeTemp.extendedname = mFsValue;
				[passedNode.children addObject:nodeTemp];
				[nodeTemp release];
				
			}
			else
			{
				break;
			}
			
		}
		while (true);
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -----------> findString Error: %@ - %@", [e name], [e reason]);
		return false;
	}
	
	return true;
}

//Parse string and remove white spaces and other characters
- (NSString*) parseString: (NSString*) stringWithSpaces
{
	NSArray *comps = [stringWithSpaces componentsSeparatedByCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	NSMutableArray *nonemptyComps = [[[NSMutableArray alloc] init] autorelease];
	
	// only copy non-empty entries
	for (NSString *oneComp in comps)
	{
		if (![oneComp isEqualToString:@""])
		{
			[nonemptyComps addObject:oneComp];
		}
	}
	NSString *stringRejoined = [nonemptyComps componentsJoinedByString:@" "];
	
	return stringRejoined; //[stringRejoined substringFromIndex:5];
}


//Refresh TreeViewStructure (Code explorer)
- (void) refreshExplorer
{
	@try 
	{
		[treeViewStructure reloadData];
		[treeViewStructure expandItem:nil expandChildren:YES];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxDocument -> refreshExplorer Error: %@ - %@", [e name], [e reason]);
	}
}


//Thread is finished - copy the returned array into root Instruments child
- (void)bgThreadIsDone:(wxNode*) node
{
	//NSLog(@"FindSTRUCTURE_NOTIFICATION_ENDED");
	
	//NSLog(@"thread inside bgThreadIsDone: %@", [NSThread currentThread]);
	
	wxNode* Options =  [self retrieveNodeByName:@"<CsOptions>"];
	wxNode* Instruments = [self retrieveNodeByName:@"<CsInstruments>"];
	wxNode* Score = [self retrieveNodeByName:@"<CsScore>"];
	
	if(Options != nil)
	{
		if([[[node.children objectAtIndex:0] children] count] > 0)
		{
			Options.children = [NSMutableArray arrayWithArray:[[node.children objectAtIndex:0] children]];
		}
		else
		{
			Options.children = nil;
		}
	}
	if(Instruments != nil)
	{
		if([[[node.children objectAtIndex:1] children] count] > 0)
		{
			Instruments.children = [NSMutableArray arrayWithArray:[[node.children objectAtIndex:1] children]];
		}
		else
		{
			Instruments.children = nil;
		}
	}
	if(Score != nil)
	{
		if([[[node.children objectAtIndex:2] children] count] > 0)
		{
			Score.children = [NSMutableArray arrayWithArray:[[node.children objectAtIndex:2] children]];
		}
		else
		{
			Score.children = nil;
		}
	}
	
	
	[self refreshExplorer];
	
	
	//Node was allocated by Self.FindStructure method so we must release it!!! //TO PREVENT MEMORY LEAKS
	[node release]; 
	
	
	
	//Refresh bookmarks
	[self wxInsertRemoveBookmark:nil];
	
}
//----------------------------------------------------------------------------------------------------------
// FIND STRUCTURE
//----------------------------------------------------------------------------------------------------------








#pragma mark - Cabbage
//----------------------------------------------------------------------------------------------------------
// CABBAGE IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------
- (IBAction)wxCabbageUpdate:(id)sender
{
	NSString* TextEditorFileName = [self saveFileBeforeCompile]; 
	[wxMAIN UpdateCabbage:TextEditorFileName]; //[[self fileURL] path]];
}
- (IBAction)wxCabbageExportVSTI:(id)sender
{
	NSString* TextEditorFileName = [self saveFileBeforeCompile]; 
	[wxMAIN CabbageExportVSTI:TextEditorFileName];
}
- (IBAction)wxCabbageExportVST:(id)sender
{
	NSString* TextEditorFileName = [self saveFileBeforeCompile]; 
	[wxMAIN CabbageExportVST:TextEditorFileName];
}
- (IBAction)wxCabbageExportAU:(id)sender
{
	NSString* TextEditorFileName = [self saveFileBeforeCompile]; 
	[wxMAIN CabbageExportAU:TextEditorFileName];
}
- (void) UpdateCurrentFileForCabbage
{
	
	//tempEditor.LoadFile(filename);
	////[textEditor LoadFile:[[self fileURL] path]];
	[textEditor setText:[wxMAIN getStringFromFilename: [[self fileURL] path]]];
	
	
	//tempEditor.textEditor.SetSavePoint();
	//[textEditor setSavePoint];
	[self saveDocument:self];
	
	NSLog(@"UpdateCurrentFileForCabbage");
	
	//if (wxGlobal.Settings.General.BringWinXoundToFrontForCabbage)
	if([[wxDefaults valueForKey:@"BringWinXoundToFrontForCabbage"] boolValue] == true)
	{
		NSLog(@"BRING TO FRONT");
		
		[NSApp activateIgnoringOtherApps:YES];
		[[self windowForSheet] makeKeyAndOrderFront:self];
		
	}
	
}








@end
