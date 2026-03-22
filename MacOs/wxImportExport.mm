//
//  wxImportExport.m
//  WinXound
//
//  Created by Teto on 06/03/10.
//  Copyright 2010 __MyCompanyName__. All rights reserved.
//

#import "wxImportExport.h"
#import "wxGlobal.h"
#import "wxDocument.h"
#import "TextEditorView.h";
#import "wxMainController.h";
#import "ScintillaView.h";



@implementation wxImportExport


static id sharedInstance = nil;




#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
+ (wxImportExport*)sharedInstance
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








#pragma mark IMPORT
//--------------------------------------------------------------------------------------------------
// IMPORT METHODS 
//--------------------------------------------------------------------------------------------------

//Used by wxMainController (Open method)
- (NSString*) ImportORCSCO:(NSString*)filename//(ref string SciEditFileName)
{
	
	//RichTextBox tempOrc = new RichTextBox();
	//RichTextBox tempSco = new RichTextBox();
	NSString* tempOrc = @"";
	NSString* tempSco = @"";
	
	//string OrcFileName = SciEditFileName.Remove(SciEditFileName.Length - 4, 4) + ".orc";
	//string ScoFileName = SciEditFileName.Remove(SciEditFileName.Length - 4, 4) + ".sco";
	NSString* OrcFileName = [NSString stringWithFormat:@"%@.orc",[filename stringByDeletingPathExtension]];
	NSString* ScoFileName = [NSString stringWithFormat:@"%@.sco",[filename stringByDeletingPathExtension]];
	
	NSFileManager* fileManager = [NSFileManager defaultManager];
	
	//Verify that files exists and load they 
	//if (File.Exists(OrcFileName))
	if([fileManager fileExistsAtPath:OrcFileName]) 
	{
		//tempOrc.LoadFile(OrcFileName, RichTextBoxStreamType.PlainText);
		//tempOrc = [NSString stringWithContentsOfFile:OrcFileName encoding:NSUTF8StringEncoding error:nil];
		tempOrc = [wxMAIN getStringFromFilename:OrcFileName];
	}
	//if (File.Exists(ScoFileName))
	if([fileManager fileExistsAtPath:ScoFileName])
	{
		//tempSco.LoadFile(ScoFileName, RichTextBoxStreamType.PlainText);
		//tempSco = [NSString stringWithContentsOfFile:ScoFileName encoding:NSUTF8StringEncoding error:nil];
		tempSco = [wxMAIN getStringFromFilename:ScoFileName];
	}
	
	
	NSString* tempString = [NSString stringWithFormat:
							@"<CsoundSynthesizer>\n\n"
							"<CsOptions>\n"
							"-W -odac\n"					
							"</CsOptions>\n\n" 
							"<CsInstruments>\n"
							"%@\n"
							"</CsInstruments>\n\n" 
							"<CsScore>\n"
							"%@\n"
							"</CsScore>\n\n" 
							"</CsoundSynthesizer>",
							tempOrc, tempSco];
	
	
	return tempString;
	
}


//ACTIONS FOR FILE MENU: IMPORT/EXPORT
//Used by wxDocument class

// IMPORT ORCHESTRA 
//--------------------------------------------------------------------------------------------------
- (void) ImportORC:(TextEditorView*)textEditor owner:(wxDocument*)owner
{
	[self BrowseFile:@"orc" forWindow:[owner windowForSheet] editor:textEditor];
}

- (void) internalImportORC:(TextEditorView*)textEditor fromFilename:(NSString*)filename
{
	//NSString* tempOrc = [NSString stringWithContentsOfFile:filename 
	//											  encoding:NSUTF8StringEncoding 
	//												 error:nil];
	
	NSString* tempOrc = [wxMAIN getStringFromFilename:filename];
	
	if(tempOrc == nil) return;
	
	
	NSInteger startInstr = 0;
	NSInteger endInstr = 0;
	
	
	//Search for <CsInstruments> section: start and end
	//Find <CsInstruments> 
	startInstr = [textEditor FindText:@"<CsInstruments>"
					   MatchWholeWord:true
							MatchCase:true
						   IsBackward:false 
						   SelectText:false
						  ShowMessage:false 
							  SkipRem:true
								start:0
								  end:-1];
	
	if(startInstr == -1)
	{
		[wxMAIN ShowMessage:@"<CsInstruments> tag not found!" 
			informativeText:@"Please insert it in the code and retry." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	//Find </CsInstruments> 
	endInstr = [textEditor FindText:@"</CsInstruments>"
					 MatchWholeWord:true
						  MatchCase:true
						 IsBackward:false 
						 SelectText:false
						ShowMessage:false 
							SkipRem:true
							  start:0
								end:-1];
	
	if(endInstr == -1)
	{
		[wxMAIN ShowMessage:@"</CsInstruments> tag not found!" 
			informativeText:@"Please insert it in the code and retry." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	[textEditor setSelection:startInstr + 15
						 end:endInstr];
	
	[[textEditor getPrimaryView] setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
	[textEditor setSelectedText:[NSString stringWithFormat:@"\n%@\n", tempOrc]];
	[textEditor ConvertEOL:[textEditor GetEOLMode]];
	[[textEditor getPrimaryView] setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
}



// IMPORT SCORE 
//--------------------------------------------------------------------------------------------------
- (void) ImportSCO:(TextEditorView*) textEditor owner:(wxDocument*)owner
{
	[self BrowseFile:@"sco" forWindow:[owner windowForSheet] editor:textEditor];
}

- (void) internalImportSCO:(TextEditorView*)textEditor fromFilename:(NSString*)filename
{
	//NSString* tempSco = [NSString stringWithContentsOfFile:filename 
	//											  encoding:NSUTF8StringEncoding 
	//												 error:nil];
	
	NSString* tempSco = [wxMAIN getStringFromFilename:filename];
	
	if(tempSco == nil) return;
	
	
	NSInteger startScore = 0;
	NSInteger endScore = 0;
	
	
	//Search for <CsScore> section: start and end
	//Find <CsScore> 
	startScore = [textEditor FindText:@"<CsScore>"
					   MatchWholeWord:true
							MatchCase:true
						   IsBackward:false 
						   SelectText:false
						  ShowMessage:false 
							  SkipRem:true
								start:0
								  end:-1];
	
	if(startScore == -1)
	{
		[wxMAIN ShowMessage:@"<CsScore> tag not found!" 
			informativeText:@"Please insert it in the code and retry." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	//Find </CsScore> 
	endScore = [textEditor FindText:@"</CsScore>"
					 MatchWholeWord:true
						  MatchCase:true
						 IsBackward:false 
						 SelectText:false
						ShowMessage:false 
							SkipRem:true
							  start:0
								end:-1];
	
	if(endScore == -1)
	{
		[wxMAIN ShowMessage:@"</CsScore> tag not found!" 
			informativeText:@"Please insert it in the code and retry." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	[textEditor setSelection:startScore + 10
						 end:endScore];
	
	[[textEditor getPrimaryView] setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
	[textEditor setSelectedText:[NSString stringWithFormat:@"\n%@\n", tempSco]];
	[textEditor ConvertEOL:[textEditor GetEOLMode]];
	[[textEditor getPrimaryView] setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
	
}
//--------------------------------------------------------------------------------------------------
// IMPORT METHODS 
//--------------------------------------------------------------------------------------------------











#pragma mark EXPORT
//--------------------------------------------------------------------------------------------------
// EXPORT METHODS
//--------------------------------------------------------------------------------------------------
- (void) ExportOrcSco:(TextEditorView*) textEditor owner:(wxDocument*)owner
{
	saveAction = @"ExportOrcSco";
	
	[self SaveFile:[[owner fileName] stringByDeletingPathExtension] 
		 forWindow:[owner windowForSheet] 
			editor:textEditor];
}

- (void) internalExportOrcSco:(TextEditorView*)textEditor toFilename:(NSString*)filename
{
	@try
	{
		
		NSInteger startSection = 0;
		NSInteger endSection = 0;
		NSString* tempOrc = nil;
		NSString* tempSco = nil;
		
		NSString* text = [textEditor getText];
		
		
		//ORCHESTRA:
		//Search for <CsInstruments> section: start and end
		//Find <CsInstruments> 
		startSection = [self findTextPositionWithNSString:text stringToFind:@"<CsInstruments>"];
		
		if(startSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"<CsInstruments> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		//Find </CsInstruments> 
		endSection = [self findTextPositionWithNSString:text stringToFind:@"</CsInstruments>"];
		
		if(endSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"</CsInstruments> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		startSection += 15;
		NSRange sectionRange = NSMakeRange(startSection, endSection - startSection);
		tempOrc = [[text substringWithRange:sectionRange] 
				   stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
		
		
		
		
		//SCORE:
		//Search for <CsScore> section: start and end
		//Find <CsScore> 
		startSection = [self findTextPositionWithNSString:text stringToFind:@"<CsScore>"];

		if(startSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"<CsScore> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		//Find </CsScore> 
		endSection = [self findTextPositionWithNSString:text stringToFind:@"</CsScore>"];

		if(endSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"</CsScore> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		startSection += 9;
		sectionRange = NSMakeRange(startSection, endSection - startSection);
		tempSco = [[text substringWithRange:sectionRange] 
				   stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
		
		
		//Write Orc and Sco to disc
		NSString* outputFilename = [NSString stringWithFormat:@"%@.orc", filename];
		[tempOrc writeToFile:outputFilename atomically:YES encoding:NSUTF8StringEncoding error:nil];
		
		outputFilename = [NSString stringWithFormat:@"%@.sco", filename];
		[tempSco writeToFile:outputFilename atomically:YES encoding:NSUTF8StringEncoding error:nil];
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxImportExport -> internalExportOrcSco:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
	
}


- (void) ExportOrc:(TextEditorView*) textEditor owner:(wxDocument*)owner
{
	saveAction = @"ExportOrc";
	
	[self SaveFile:[[owner fileName] stringByDeletingPathExtension] 
		 forWindow:[owner windowForSheet] 
			editor:textEditor];
}

- (void) internalExportOrc:(TextEditorView*)textEditor toFilename:(NSString*)filename
{
	
	@try
	{
		
		NSInteger startSection = 0;
		NSInteger endSection = 0;
		NSString* tempOrc = nil;
		
		NSString* text = [textEditor getText];
		
		//ORCHESTRA:
		//Search for <CsInstruments> section: start and end
		//Find <CsInstruments> 
		startSection = [self findTextPositionWithNSString:text stringToFind:@"<CsInstruments>"];

		if(startSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"<CsInstruments> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		//Find </CsInstruments> 
		endSection = [self findTextPositionWithNSString:text stringToFind:@"</CsInstruments>"];

		if(endSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"</CsInstruments> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		startSection += 15;
		NSRange sectionRange = NSMakeRange(startSection, endSection - startSection);
		tempOrc = [[text substringWithRange:sectionRange] 
				   stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
		
		
		//Write Orc to disc
		NSString* outputFilename = [NSString stringWithFormat:@"%@.orc", filename];
		[tempOrc writeToFile:outputFilename atomically:YES encoding:NSUTF8StringEncoding error:nil];
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxImportExport -> internalExportOrc:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
	
}

- (void) ExportSco:(TextEditorView*) textEditor owner:(wxDocument*)owner
{
	saveAction = @"ExportSco";
	
	[self SaveFile:[[owner fileName] stringByDeletingPathExtension] 
		 forWindow:[owner windowForSheet] 
			editor:textEditor];
}

- (void) internalExportSco:(TextEditorView*)textEditor toFilename:(NSString*)filename
{
	
	@try
	{
		
		NSInteger startSection = 0;
		NSInteger endSection = 0;
		NSString* tempSco = nil;
		
		NSString* text = [textEditor getText];
		
		
		//SCORE:
		//Search for <CsScore> section: start and end
		//Find <CsScore> 		
		startSection = [self findTextPositionWithNSString:text stringToFind:@"<CsScore>"];

		if(startSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"<CsScore> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		//Find </CsScore> 		
		endSection = [self findTextPositionWithNSString:text stringToFind:@"</CsScore>"];

		if(endSection == NSNotFound)
		{
			[wxMAIN ShowMessage:@"</CsScore> tag not found!" 
				informativeText:@"Please insert it in the code and retry." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return;
		}
		
		startSection += 9;
		NSRange sectionRange = NSMakeRange(startSection, endSection - startSection);
		tempSco = [[text substringWithRange:sectionRange] 
				   stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
		
		//Write Sco to disc
		NSString* outputFilename = [NSString stringWithFormat:@"%@.sco", filename];
		[tempSco writeToFile:outputFilename atomically:YES encoding:NSUTF8StringEncoding error:nil];
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxImportExport -> internalExportSco:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
}



- (NSInteger) findTextPositionWithNSString:(NSString*)_text stringToFind:(NSString*)stringToFind
{
	@try
	{
		NSRange mFsFindPos;
		
		NSInteger mFsStart = 0;
		NSInteger mFsEnd = [_text length];
		NSInteger startOfLine = 0;
		NSInteger mFindRem = 0;
		
		
		do
		{
			mFsFindPos = [_text rangeOfString:stringToFind 
									  options:NSLiteralSearch
										range:NSMakeRange(0, mFsEnd - mFsStart)];
			
			
			////Find only whole words !!! (Aggiungere eventualmente il rem ; come possibile carattere iniziale
			//if (mFsFindPos.location != NSNotFound && 
			//	[[[NSCharacterSet whitespaceAndNewlineCharacterSet] invertedSet] characterIsMember:
			//	 [_text characterAtIndex:mFsFindPos.location - 1]]) 
			//{
			//	// Preceding character is alphanumeric
			//	mFsStart = mFsFindPos.location + 5;
			//	continue;
			//}
			//if (mFsFindPos.location + mFsFindPos.length < [_text length] && 
			//	[[[NSCharacterSet whitespaceAndNewlineCharacterSet] invertedSet] characterIsMember:
			//	 [_text characterAtIndex:mFsFindPos.location + mFsFindPos.length]]) 
			//{
			//	// Trailing character is alphanumeric
			//	mFsStart = mFsFindPos.location + 5;
			//	continue;
			//}
			
			
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
						mFsStart = mFsFindPos.location + 5;
						continue;
					}
					
					//NSString* temp = [_text substringWithRange:NSMakeRange(startOfLine, mFsFindPos.location - startOfLine)];
					//NSLog(@"temp: %@", temp);
					//if([[temp stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]] length] != 0)
					//{
					//	mFsStart = mFsFindPos.location + 5;
					//	continue;
					//}
				}
				else
				{
					mFsStart = mFsFindPos.location + 5;
					continue;
				}
				
				
				return mFsFindPos.location;

				
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
		NSLog(@"wxImportExport -> findTextPositionWithNSString Error: %@ - %@", [e name], [e reason]);
	}
	
	return NSNotFound;
	
}


//--------------------------------------------------------------------------------------------------
// EXPORT METHODS
//--------------------------------------------------------------------------------------------------












#pragma mark Open/Save panels
//--------------------------------------------------------------------------------------------------
// OPEN Panel implementation
//--------------------------------------------------------------------------------------------------
- (void) BrowseFile:(NSString*) extension forWindow:(NSWindow*)sheetWindow editor:(TextEditorView*)textEditor
{		
	NSOpenPanel *openPanel = [NSOpenPanel openPanel];
	[openPanel setResolvesAliases:YES];	
	[openPanel setAllowsMultipleSelection:false];
	[openPanel setCanChooseFiles: true];
	
	
	//MODAL WINDOW
	//NSInteger result = [openPanel runModalForDirectory:nil file:nil types:nil];
	
	//SHEET WINDOW
	[openPanel beginSheetForDirectory:nil
								 file:nil
								types:[NSArray arrayWithObjects: extension, nil]
					   modalForWindow:sheetWindow 
						modalDelegate:self
					   didEndSelector:@selector(openPanelDidEnd:
												returnCode:
												contextInfo:)
						  contextInfo:textEditor];
	
	
	
	//	if (result == NSOKButton) {
	//		NSLog([[openPanel filenames] objectAtIndex:0]);
	//	}
	
}


- (void) openPanelDidEnd:(NSOpenPanel *)panel returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	@try
	{
		if (returnCode == NSOKButton)
		{
			TextEditorView* textEditor = (TextEditorView*)contextInfo;
			NSString* filename = [[[panel URLs] objectAtIndex:0] path];
			
			
			if([[filename lowercaseString] hasSuffix:@".orc"])
				[self internalImportORC:textEditor fromFilename:filename];
			
			else if([[filename lowercaseString] hasSuffix:@".sco"])
				[self internalImportSCO:textEditor fromFilename:filename];
			
		}
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxImportExport -> openPanelDidEnd:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
}




//--------------------------------------------------------------------------------------------------
// SAVE Panel implementation
//--------------------------------------------------------------------------------------------------
- (void) SaveFile:(NSString*) filename forWindow:(NSWindow*)sheetWindow editor:(TextEditorView*)textEditor 
{
	NSSavePanel *spanel = [NSSavePanel savePanel];
	//NSString *path = @"/Documents";
	//[spanel setDirectory:[path stringByExpandingTildeInPath]];
	//[spanel setPrompt:NSLocalizedString(@"save_ok",nil)];
	
	//[spanel setRequiredFileType:@"rtfd"];
	
	//[openPanel setResolvesAliases:YES];	
	//[openPanel setAllowsMultipleSelection:false];
	//[openPanel setCanChooseFiles: true];
	
	[spanel beginSheetForDirectory:nil //NSHomeDirectory()
                              file:[filename lastPathComponent] //nil
					modalForWindow:sheetWindow
					 modalDelegate:self
					didEndSelector:@selector(didEndSaveSheet:returnCode:contextInfo:)
					   contextInfo:textEditor];
}

-(void)didEndSaveSheet:(NSSavePanel *)savePanel
			returnCode:(int)returnCode 
		   contextInfo:(void *)contextInfo
{
	@try
	{
		if (returnCode == NSOKButton)
		{
			TextEditorView* textEditor = (TextEditorView*)contextInfo;
			NSString* filename = [[savePanel URL] path];
			
			if([saveAction isEqualToString:@"ExportOrcSco"])
			{
				[self internalExportOrcSco:textEditor toFilename:filename];
			}
			else if([saveAction isEqualToString:@"ExportOrc"])
			{
				[self internalExportOrc:textEditor toFilename:filename];
			}
			else if([saveAction isEqualToString:@"ExportSco"])
			{
				[self internalExportSco:textEditor toFilename:filename];
			}
		}
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxImportExport -> didEndSaveSheet:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
	
	[saveAction release];
	
}






@end
