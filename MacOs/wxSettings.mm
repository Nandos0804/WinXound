//
//  wxSettings.m
//  WinXound
//
//  Created by Stefano Bonetti on 25/01/10.
//

#import "wxSettings.h"
#import "wxGlobal.h"
#import "wxMainController.h"

@implementation wxSettings




#pragma mark - Initialization and Dealloc
//--------------------------------------------------------------------------------------------------
// INIT AND DEALLOC
//--------------------------------------------------------------------------------------------------
- (void)initialize
{
	NSLog(@"wxSETTINGS initialize");
	self = [super init];
	tagSender = 0;
	
	KeyListCSound = [[[wxDefaults valueForKey: @"CSoundStyles"] allKeys] retain];
	KeyListPython = [[[wxDefaults valueForKey: @"PythonStyles"] allKeys] retain];
	KeyListLua = [[[wxDefaults valueForKey: @"LuaStyles"] allKeys] retain];
	
	//[syntaxListCSound setDataSource: self];
	[syntaxListCSound setDelegate:self];
	[syntaxListCSound setTarget:self];
	[syntaxListCSound reloadData];
	
	//[syntaxListPython setDataSource: self];
	[syntaxListPython setDelegate:self];
	[syntaxListPython setTarget:self];
	[syntaxListPython reloadData];
	
	//[syntaxListLua setDataSource: self];
	[syntaxListLua setDelegate:self];
	[syntaxListLua setTarget:self];
	[syntaxListLua reloadData];
	
}

- (void) dealloc
{
	[KeyListCSound release];
	[KeyListPython release];
	[KeyListLua release];
	[super dealloc];
}

//--------------------------------------------------------------------------------------------------
// INIT AND DEALLOC
//--------------------------------------------------------------------------------------------------












#pragma mark - Styles Info methods
//--------------------------------------------------------------------------------------------------
// Useful Methods to retrieve lexer style information from database (for syntax highlight and style)
//--------------------------------------------------------------------------------------------------
- (NSString*) StyleGetFriendlyName:(NSString*) lang 
					   stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			//if (GetValues(s)[0] == Convert.ToString(stylenumber))
			if([self GetValues:s] != nil)
			//if ([[[self GetValues: s] objectAtIndex:0] integerValue] == stylenumber) 
			{
				return [[self GetValues: s] objectAtIndex:6];
			}
		}
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetFriendlyName Error: %@ - %@", [e name], [e reason]);
	}	
		
	return @"";
}

- (NSString*) StyleGetForeColor:(NSString*) lang 
					stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			if([self GetValues:s] != nil)
			{
				return [[self GetValues: s] objectAtIndex:0];
			}
		}
		return [self StyleGetForeColor:lang stylenumber:32];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetForeColor Error: %@ - %@", [e name], [e reason]);
	}
	
	return @"#000000"; //Default Fore color (Black)
}

- (NSString*) StyleGetBackColor:(NSString*) lang 
					stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			if([self GetValues:s] != nil) 
			{
				return [[self GetValues: s] objectAtIndex:1];
			}
		}
		return [self StyleGetBackColor:lang stylenumber:32];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetBackColor Error: %@ - %@", [e name], [e reason]);
	}
	
	return @"FFFFFF"; //Default Back color (White);
}

- (BOOL) StyleGetBold:(NSString*) lang 
		  stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			if([self GetValues:s] != nil)
			{
				NSString* value = [[self GetValues: s] objectAtIndex:2];
				return [value isEqualToString:@"True"];
				//return ([[self GetValues: s] objectAtIndex:2] == @"True");
			}
		}
		return [self StyleGetBold:lang stylenumber:32];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetBold Error: %@ - %@", [e name], [e reason]);
	}
	
	return false;
}

- (BOOL) StyleGetItalic:(NSString*) lang 
			stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			if([self GetValues:s] != nil)
			{
				NSString* value = [[self GetValues: s] objectAtIndex:3];
				return [value isEqualToString:@"True"];
				//return ([[self GetValues: s] objectAtIndex:3] == @"True");
			}
		}
		return [self StyleGetItalic:lang stylenumber:32];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetItalic Error: %@ - %@", [e name], [e reason]);
	}
		
	return false;
}

- (NSInteger) StyleGetAlpha:(NSString*) lang 
				stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	if (tempList == nil) return 40;
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			if([self GetValues:s] != nil)
			{
				return [[[self GetValues: s] objectAtIndex:4] integerValue];
			}
		}
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetAlpha Error: %@ - %@", [e name], [e reason]);
	}	
		
	return 40; //Default Alpha for Bookmarks;
}

- (BOOL) StyleGetEolFilled:(NSString*) lang 
			   stylenumber:(NSInteger) stylenumber
{
	tempList = [self SelectLanguage:lang];
	if (tempList == nil) return false;
	
	@try
	{
		NSString* s = [tempList valueForKey:[NSString stringWithFormat:@"%d", stylenumber]];
		if(s != nil)
		{
			if([self GetValues:s] != nil)
			{
				return ([[self GetValues: s] objectAtIndex:5] == @"True");
			}
		}
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> StyleGetEolFilled Error: %@ - %@", [e name], [e reason]);
	}	
		
	return false;
}

//- (NSInteger) StyleGetListPosition:(NSString*) lang 
//					   stylenumber:(NSInteger) stylenumber
//{
//	tempList = [self SelectLanguage:lang];
//	if (tempList == nil) return -1;
//	
//	NSInteger index = 0;
//	for(NSString* s in tempList)
//	{
//		//if (GetValues(s)[0] == Convert.ToString(stylenumber))
//		if ([[[self GetValues: s] objectAtIndex:0] integerValue] == stylenumber)
//		{
//			return index;
//		}
//		index++;
//	}
//	return -1;
//}

- (NSDictionary*) SelectLanguage: (NSString*) lang
{
	if ([lang isEqualToString:@"csound"]) {
		return [wxDefaults valueForKey: @"CSoundStyles"];
	} else if ([lang isEqualToString:@"python"]) {
		return [wxDefaults valueForKey: @"PythonStyles"];
	} else if ([lang isEqualToString:@"lua"]) {
		return [wxDefaults valueForKey: @"LuaStyles"];
	}
	return nil;
}


- (NSArray*) GetValues: (NSString*) s
{	
	@try
	{
		//return s.Split(",".ToCharArray());
		return [s componentsSeparatedByString:@","];
	}
	@catch (NSException *e)
	{
		//		System.Diagnostics.Debug.WriteLine(
		//										   "wxSettings.EditorProperties.GetValues Error: " + ex.Message);
		//return new string[] { "" };
		//return [[NSArray* alloc]
		NSArray* temp = [NSArray arrayWithObject: @""];
		return temp;
	}
	return nil;
}

//--------------------------------------------------------------------------------------------------
// Useful Methods to retrieve lexer style information from database (for syntax highlight and style)
//--------------------------------------------------------------------------------------------------













#pragma mark Color conversions
//--------------------------------------------------------------------------------------------------
// COLOR CONVERSIONS
//--------------------------------------------------------------------------------------------------
- (NSColor*)HexToNSColor:(NSString*)inColorString
{
	@try 
	{
		NSString* hexColor = [inColorString substringFromIndex:1];
		
		NSColor* result    = nil;
		unsigned colorCode = 0;
		unsigned char redByte, greenByte, blueByte;
		
		if (nil != hexColor)
		{
			NSScanner* scanner = [NSScanner scannerWithString:hexColor];
			(void) [scanner scanHexInt:&colorCode]; // ignore error
		}
		redByte   = (unsigned char)(colorCode >> 16);
		greenByte = (unsigned char)(colorCode >> 8);
		blueByte  = (unsigned char)(colorCode);     // masks off high bits
		
		result = [NSColor
				  colorWithCalibratedRed:(CGFloat)redByte    / 0xff
				  green:(CGFloat)greenByte / 0xff
				  blue:(CGFloat)blueByte   / 0xff
				  alpha:1.0];
		
		return result;
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> HexToNSColor Error: %@ - %@", [e name], [e reason]);
	}	
	
	return [NSColor blackColor];
}

- (NSString*)NSColorToHex:(NSColor*)color
{
	
	@try
	{
		float redFloatValue, greenFloatValue, blueFloatValue;
		int redIntValue, greenIntValue, blueIntValue;
		NSString *redHexValue, *greenHexValue, *blueHexValue;
		
		//Convert the NSColor to the RGB color space before we can access its components
		NSColor *convertedColor=[color colorUsingColorSpaceName:NSCalibratedRGBColorSpace];
		
		if(convertedColor)
		{
			// Get the red, green, and blue components of the color
			[convertedColor getRed:&redFloatValue green:&greenFloatValue blue:&blueFloatValue alpha:NULL];
			
			// Convert the components to numbers (unsigned decimal integer) between 0 and 255
			redIntValue=redFloatValue*255.99999f;
			greenIntValue=greenFloatValue*255.99999f;
			blueIntValue=blueFloatValue*255.99999f;
			
			// Convert the numbers to hex strings
			redHexValue=[NSString stringWithFormat:@"%02x", redIntValue];
			greenHexValue=[NSString stringWithFormat:@"%02x", greenIntValue];
			blueHexValue=[NSString stringWithFormat:@"%02x", blueIntValue];
			
			// Concatenate the red, green, and blue components' hex strings together with a "#"
			return [NSString stringWithFormat:@"#%@%@%@", redHexValue, greenHexValue, blueHexValue];
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> NSColorToHex Error: %@ - %@", [e name], [e reason]);
	}
	
	return @"#000000";
}
//--------------------------------------------------------------------------------------------------
// COLOR CONVERSIONS
//--------------------------------------------------------------------------------------------------














#pragma mark IBACTIONS
//--------------------------------------------------------------------------------------------------
// IBACTIONS and other related methods
//--------------------------------------------------------------------------------------------------

- (void) ShowPreferencesWindow
{
	if (preferencesWindow == nil) 
	{
		[NSBundle loadNibNamed:@"wxPreferences" owner:self];
		[preferencesWindow setShowsToolbarButton:NO];
		//[preferencesWindow setTitle:@"WinXound Settings"];
	}
	
	[preferencesWindow makeKeyAndOrderFront:self];
	
	[[NSUserDefaultsController sharedUserDefaultsController] setAppliesImmediately:false];
	
}

- (IBAction) wxSettingsCANCEL:(id)sender
{
	[[NSUserDefaultsController sharedUserDefaultsController] revert:self];
	[preferencesWindow close];
	
	[[NSUserDefaultsController sharedUserDefaultsController] setAppliesImmediately:true];
	
	if([NSColorPanel sharedColorPanelExists])
	{
		[[NSColorPanel sharedColorPanel] close];
	}	
}
- (IBAction) wxSettingsAPPLY:(id)sender
{
	NSUserDefaultsController *defaultsController = [NSUserDefaultsController sharedUserDefaultsController];
	[defaultsController setAppliesImmediately:true];
	[defaultsController save:self];
	[preferencesWindow close];
	
	//[[NSUserDefaultsController sharedUserDefaultsController] setAppliesImmediately:true];
	
	if([NSColorPanel sharedColorPanelExists])
	{
		[[NSColorPanel sharedColorPanel] close];
	}
	
	////[wxMainController applyPreferencesToOpenedDocuments]; 
	[wxMAIN applyPreferencesToOpenedDocuments];
	
}

- (IBAction) wxSettingsSetDefaultTemplates:(id)sender
{	
	NSString* csoundTemplate =
		@"<CsoundSynthesizer>\n\n\n" 
		"<CsOptions>\n\n" 
		"</CsOptions>\n\n\n" 
		"<CsInstruments>\n\n" 
		"</CsInstruments>\n\n\n" 
		"<CsScore>\n\n" 
		"</CsScore>\n\n\n" 
		"</CsoundSynthesizer>";
	
	NSString* cabbageTemplate =
		@"<Cabbage>\n"
		"</Cabbage>\n\n"
		"<CsoundSynthesizer>\n\n"	
		"<CsOptions>\n"
		"-d -n\n"
		"</CsOptions>\n\n"
		"<CsInstruments>\n"
		"sr = 44100\n"
		"ksmps = 32\n"
		"nchnls = 2\n\n"
		"instr 1\n"
		"endin\n"
		"</CsInstruments>\n\n"
		"<CsScore>\n"
		"i1 0 1000\n"
		"</CsScore>\n\n"
		"</CsoundSynthesizer>";
	
	[wxDefaults setValue:@"" forKey:@"TemplatesPython"];
	[wxDefaults setValue:@"" forKey:@"TemplatesLua"];
	[wxDefaults setValue:csoundTemplate forKey:@"TemplatesCSound"];
	[wxDefaults setValue:cabbageTemplate forKey:@"TemplatesCabbage"];
}


- (IBAction) wxSettingsSetDefaultCompilerFont:(id)sender
{
	[wxDefaults setValue:@"Andale Mono" forKey:@"CompilerFontName"];
	[wxDefaults setValue:[NSNumber numberWithInteger:12] forKey:@"CompilerFontSize"];
}
- (IBAction) wxSettingsSetDefaultEditorFont:(id)sender
{
	[wxDefaults setValue:@"Andale Mono" forKey:@"EditorFontName"];
	[wxDefaults setValue:[NSNumber numberWithInteger:12] forKey:@"EditorFontSize"];
	[wxDefaults setValue:[NSNumber numberWithInteger:8] forKey:@"EditorTabIndent"];
}
- (IBAction) wxSettingsSetDefaultEditorProperties:(id)sender
{
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"EditorShowMatchingBracket"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"SaveBookmarks"];
	[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"EditorMarkCaretLine"];
	[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"EditorShowVerticalRuler"];
}
- (IBAction) wxSettingsSetCSoundDefaultFlags:(id)sender
{
	[wxDefaults setValue:@"-B4096 --displays --asciidisplay" forKey:@"CSoundDefaultFlags"];
}
- (IBAction) wxSettingsSetPythonDefaultFlags:(id)sender
{
	[wxDefaults setValue:@"-u" forKey:@"PythonDefaultFlags"];
}
- (IBAction) wxSettingsSetLuaDefaultFlags:(id)sender
{
	[wxDefaults setValue:@"" forKey:@"LuaDefaultFlags"];
}


- (IBAction) wxAutoSearchPaths:(id)sender
{
	NSFileManager *fileManager = [NSFileManager defaultManager];
	
	//CSOUND COMPILER
	if([fileManager fileExistsAtPath:@"/usr/local/bin/csound"])
	{
		[wxDefaults setValue:@"/usr/local/bin/csound" forKey:@"CSoundConsolePath"];
	}
	
	//PYTHON COMPILER
	if([fileManager fileExistsAtPath:@"/usr/bin/python"])
	{
		[wxDefaults setValue:@"/usr/bin/python" forKey:@"PythonConsolePath"];
	}
	
	//TODO: LUA COMPILER
	
	
	//CABBAGE APPLICATION
	if([fileManager fileExistsAtPath:@"/Applications/Cabbage.app"])
	{
		[wxDefaults setValue:@"/Applications/Cabbage.app" forKey:@"CabbagePath"];
	}
	
	//QUTECSOUND
	NSArray *paths = NSSearchPathForDirectoriesInDomains(NSApplicationDirectory, NSLocalDomainMask, YES);
	if(paths != nil)
	{
		NSString *applicationDirectory = [paths objectAtIndex:0];
		NSString *quteCSoundPath = [NSString stringWithFormat:@"%@/QuteCsound.app",applicationDirectory];
		//NSLog(quteCSoundPath);
		//NSArray* files = [fileManager contentsOfDirectoryAtPath:applicationDirectory error:nil];
		if([fileManager fileExistsAtPath:quteCSoundPath])
		{
			[wxDefaults setValue:quteCSoundPath forKey:@"CSoundExternalGuiPath"];
		}
	}
	
	//CSOUND HTML HELP
	paths = NSSearchPathForDirectoriesInDomains(NSLibraryDirectory, NSLocalDomainMask, YES);
	if(paths != nil)
	{
		NSString *libraryDirectory = [paths objectAtIndex:0];
		NSString *manualPath = [NSString stringWithFormat:@"%@/Frameworks/CsoundLib.Framework/Resources/Manual/index.html", libraryDirectory];
		if([fileManager fileExistsAtPath:manualPath])
		{
			[wxDefaults setValue:manualPath forKey:@"CSoundHelpHtmlPath"];
		}
	}
	
	//PYTHON IDLE
	if([fileManager fileExistsAtPath:@"/usr/bin/idle"])
	{
		[wxDefaults setValue:@"/usr/bin/idle" forKey:@"PythonExternalGuiPath"];
	}
	
	//TODO: LUA EXTERNAL
	
}


- (IBAction) wxSettingsFontPanelOK:(id)sender
{
	//NSLog(@"OK");
	NSCell* cell = [fontTable preparedCellAtColumn:0 row:[fontTable selectedRow]];
	NSString* title = [cell title];
	
	NSString* keyFontName = nil;
	NSString* keyFontSize = nil;
	NSInteger size = [[fontSize stringValue] integerValue];
	
	
	//Tag:0 = Compiler Tag:1 = Editor
	if(tagSender == 0)
	{
		keyFontName = @"CompilerFontName";
		keyFontSize = @"CompilerFontSize";
	}
	else
	{
		keyFontName = @"EditorFontName";
		keyFontSize = @"EditorFontSize";
	}
	
	
	[wxDefaults setValue:title forKey:keyFontName];
	[wxDefaults setValue:[NSNumber numberWithInteger:size] forKey:keyFontSize];

	[fontPanel close];
	[NSApp endSheet:fontPanel];
}

- (IBAction) wxSettingsFontPanelCANCEL:(id)sender
{
	[fontPanel close];
	[NSApp endSheet:fontPanel];
}

- (IBAction) wxShowFontPanel:(id)sender
{	
	tagSender = [(NSButton *)sender tag];
	
	if(fontPanel == nil) return;
	
	[fontTable setDelegate:self];
	[fontTable setDataSource: self];
	
	NSCell* cell = nil;
	NSInteger index = 0;

	
	NSString* keyValue = nil;
	//Tag:0 = Compiler Tag:1 = Editor
	if(tagSender == 0)
	{
		keyValue = @"CompilerFontName";
		NSString* path = [[NSBundle mainBundle] pathForResource: @"compiler_example" 
														 ofType: @"txt" inDirectory: nil];	
		[fontText setStringValue:[NSString stringWithContentsOfFile: path
															  encoding: NSUTF8StringEncoding
																 error: nil]];
		[fontSize setStringValue: [[wxDefaults valueForKey:@"CompilerFontSize"] stringValue]];
	}
	else
	{
		keyValue = @"EditorFontName";
		NSString* path = [[NSBundle mainBundle] pathForResource: @"csd_example" 
														 ofType: @"txt" inDirectory: nil];	
		[fontText setStringValue:[NSString stringWithContentsOfFile: path
															  encoding: NSUTF8StringEncoding
																 error: nil]];
		[fontSize setStringValue: [[wxDefaults valueForKey:@"EditorFontSize"] stringValue]];
	}

	for(NSInteger i = 0; i < [fontTable numberOfRows]; i++)
	{
		cell = [fontTable preparedCellAtColumn:0 row:i];
		//NSLog([cell title]);
		if([[cell title] isEqualToString: [wxDefaults valueForKey:keyValue]])
		{
			index = i;
			break;
		}
	}
	
	NSIndexSet* nset = [[NSIndexSet alloc] initWithIndex:index];
	[fontTable selectRowIndexes:nset byExtendingSelection:NO];
	[fontTable scrollRowToVisible:index];
	[nset release];
	
	
	//SHOW THE FONT PANEL
	if(fontPanel != nil)
	{
		[NSApp beginSheet:fontPanel modalForWindow:preferencesWindow modalDelegate:nil didEndSelector:nil contextInfo:nil];
	}
	
	[fontTable reloadData];
}

- (void) tableViewSelectionDidChange:(NSNotification *)notification
{
	if([[notification object] isEqualTo:fontTable])
	{
		//NSLog(@"CHANGED");
		NSCell* cell = [fontTable preparedCellAtColumn:0 row:[fontTable selectedRow]];
		//NSFont* font = [NSFont fontWithName:[cell title] size:[[wxDefaults valueForKey:@"EditorFontSize"] floatValue]];
		NSFont* font = [NSFont fontWithName:[cell title] size:[fontSize floatValue]];
		
		[fontText setFont:font];
	}
//	else if([[notification object] isEqualTo:syntaxListCSound])
//	{
//		NSLog(@"%d", [syntaxListCSound selectedRow]);
//	}
}

- (void) controlTextDidChange:(NSNotification *)obj
{
	if([obj object] == fontSize)
	{
		//NSLog(@"fontSize changed"); 
		//[self tableViewSelectionDidChange:nil];
		NSInteger size = [[fontSize stringValue] integerValue];
		if(size > 0 && size < 100)
		{
			NSString* fontname = [[fontText font] fontName];
			NSFont* font = [NSFont fontWithName:fontname size:size];
			[fontText setFont:font];
			//NSLog(@"fontname");
		}
	}
}

- (IBAction) wxResetDocumentSize:(id)sender
{
	[wxDefaults setValue:[NSNumber numberWithInteger:950] forKey:@"DocumentWindowWidth"];
	[wxDefaults setValue:[NSNumber numberWithInteger:700] forKey:@"DocumentWindowHeight"];
}

- (IBAction) wxResetAllToDefault:(id)sender
{
	//NSInteger ret = [wxMAIN ShowMessage:@"All values will be reverted to factory defaults.\nWould you like to continue?"
	//					informativeText:@""
	//					  defaultButton:@"YES"
	//					alternateButton:@"NO"
	//						otherButton:@"Cancel"];
	
	//if (ret == NSAlertFirstButtonReturn)
	{	
		[self wxResetSyntaxCSound:self];
		[self wxResetSyntaxPython:self];
		[self wxResetSyntaxLua:self];
		
		[[NSUserDefaultsController sharedUserDefaultsController] revertToInitialValues:self];
	}
}

//--------------------------------------------------------------------------------------------------
// IBACTIONS and other related methods
//--------------------------------------------------------------------------------------------------

















#pragma mark TableView
//--------------------------------------------------------------------------------------------------
// TABLEVIEW IMPLEMENTATIONS AND RELATED METHODS
//--------------------------------------------------------------------------------------------------
- (int)numberOfRowsInTableView:(NSTableView *)tableView{

	if([tableView isEqualTo:fontTable])
	{
	   NSFontManager *fontManager = [NSFontManager sharedFontManager];
	   return [[fontManager availableFontFamilies] count];
	}
	
	//NSLog(@"tableView %@", tableView);
	
	if ([tableView isEqualTo:syntaxListCSound])
	{
		return [[wxDefaults valueForKey: @"CSoundStyles"] count];
	}
	else if ([tableView isEqualTo:syntaxListPython])
	{
		return [[wxDefaults valueForKey: @"PythonStyles"] count];
	}
	else if ([tableView isEqualTo:syntaxListLua])
	{
		return [[wxDefaults valueForKey: @"LuaStyles"] count];
	}
	
	return 0;
}

- (BOOL) tableView:(NSTableView *)tableView shouldEditTableColumn:(NSTableColumn *)tableColumn row:(NSInteger)row
{
	return NO;
}

//Implement the protocol method to retrieve the object value for a table column.
- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(int)row
{
	@try
	{
		//FONT TABLE
		if([tableView isEqualTo:fontTable])
		{
			NSString* fontName = [[[NSFontManager sharedFontManager] availableFontFamilies] objectAtIndex:row];
			return fontName;
		}
		
		//SYNTAX TABLES
		else if ([tableView isEqualTo:syntaxListCSound] ||
				 [tableView isEqualTo:syntaxListPython] ||
				 [tableView isEqualTo:syntaxListLua])
		{
			if([[[tableColumn headerCell] stringValue] isEqualToString:@"Style Name"])
			{
				if([[tableColumn identifier] isEqualToString:@"STYLE_CSOUND"])
				{
					NSInteger styleNumber = [[KeyListCSound objectAtIndex:row] integerValue];
					return [self StyleGetFriendlyName:@"csound" stylenumber:styleNumber];
				}
				else if([[tableColumn identifier] isEqualToString:@"STYLE_PYTHON"])
				{
					NSInteger styleNumber = [[KeyListPython objectAtIndex:row] integerValue];
					return [self StyleGetFriendlyName:@"python" stylenumber:styleNumber];
				}
				else if([[tableColumn identifier] isEqualToString:@"STYLE_LUA"])
				{
					NSInteger styleNumber = [[KeyListLua objectAtIndex:row] integerValue];
					return [self StyleGetFriendlyName:@"lua" stylenumber:styleNumber];
				}
			}
			
			if([[[tableColumn headerCell] stringValue] isEqualToString:@"Fore Color"])
			{
				if([[tableColumn identifier] isEqualToString:@"FORE_CSOUND"])
				{
					NSInteger styleNumber = [[KeyListCSound objectAtIndex:row] integerValue];
					
					[[tableColumn dataCell] setBackgroundColor:
					 [self HexToNSColor:[self StyleGetForeColor:@"csound" stylenumber:styleNumber]]];
				}
				else if([[tableColumn identifier] isEqualToString:@"FORE_PYTHON"])
				{
					NSInteger styleNumber = [[KeyListPython objectAtIndex:row] integerValue];
					
					[[tableColumn dataCell] setBackgroundColor:
					 [self HexToNSColor:[self StyleGetForeColor:@"python" stylenumber:styleNumber]]];
				}
				else if([[tableColumn identifier] isEqualToString:@"FORE_LUA"])
				{
					NSInteger styleNumber = [[KeyListLua objectAtIndex:row] integerValue];
					
					[[tableColumn dataCell] setBackgroundColor:
					 [self HexToNSColor:[self StyleGetForeColor:@"lua" stylenumber:styleNumber]]];
				}
				
				return @""; //[self StyleGetForeColor:@"csound" stylenumber:styleNumber];
			}
			
			if([[[tableColumn headerCell] stringValue] isEqualToString:@"Back Color"])
			{
				if([[tableColumn identifier] isEqualToString:@"BACK_CSOUND"])
				{
					NSInteger styleNumber = [[KeyListCSound objectAtIndex:row] integerValue];
					
					[[tableColumn dataCell] setBackgroundColor:
					 [self HexToNSColor:[self StyleGetBackColor:@"csound" stylenumber:styleNumber]]];
				}
				else if([[tableColumn identifier] isEqualToString:@"BACK_PYTHON"])
				{
					NSInteger styleNumber = [[KeyListPython objectAtIndex:row] integerValue];
					
					[[tableColumn dataCell] setBackgroundColor:
					 [self HexToNSColor:[self StyleGetBackColor:@"python" stylenumber:styleNumber]]];
				}
				else if([[tableColumn identifier] isEqualToString:@"BACK_LUA"])
				{
					NSInteger styleNumber = [[KeyListLua objectAtIndex:row] integerValue];
					
					[[tableColumn dataCell] setBackgroundColor:
					 [self HexToNSColor:[self StyleGetBackColor:@"lua" stylenumber:styleNumber]]];
				}
				
				return @""; //[self StyleGetBackColor:@"csound" stylenumber:styleNumber];
			}
			
			if([[[tableColumn headerCell] stringValue] isEqualToString:@"Bold"])
			{
				if([[tableColumn identifier] isEqualToString:@"BOLD_CSOUND"])
				{
					NSInteger styleNumber = [[KeyListCSound objectAtIndex:row] integerValue];
					return [NSNumber numberWithBool:[self StyleGetBold:@"csound" stylenumber:styleNumber]];
				}
				else if([[tableColumn identifier] isEqualToString:@"BOLD_PYTHON"])
				{
					NSInteger styleNumber = [[KeyListPython objectAtIndex:row] integerValue];
					return [NSNumber numberWithBool:[self StyleGetBold:@"python" stylenumber:styleNumber]];
				}
				else if([[tableColumn identifier] isEqualToString:@"BOLD_LUA"])
				{
					NSInteger styleNumber = [[KeyListLua objectAtIndex:row] integerValue];
					return [NSNumber numberWithBool:[self StyleGetBold:@"lua" stylenumber:styleNumber]];
				}
				
			}
			
			if([[[tableColumn headerCell] stringValue] isEqualToString:@"Italic"])
			{
				if([[tableColumn identifier] isEqualToString:@"ITALIC_CSOUND"])
				{
					NSInteger styleNumber = [[KeyListCSound objectAtIndex:row] integerValue];
					return [NSNumber numberWithBool:[self StyleGetItalic:@"csound" stylenumber:styleNumber]];
				}
				else if([[tableColumn identifier] isEqualToString:@"ITALIC_PYTHON"])
				{
					NSInteger styleNumber = [[KeyListPython objectAtIndex:row] integerValue];
					return [NSNumber numberWithBool:[self StyleGetItalic:@"python" stylenumber:styleNumber]];
				}
				else if([[tableColumn identifier] isEqualToString:@"ITALIC_LUA"])
				{
					NSInteger styleNumber = [[KeyListLua objectAtIndex:row] integerValue];
					return [NSNumber numberWithBool:[self StyleGetItalic:@"lua" stylenumber:styleNumber]];
				}
				
			}
		}
		
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> tableView Error: %@ - %@", [e name], [e reason]);
	}
	
	return nil;
}




- (IBAction) wxChangeBoldState:(id)sender
{
	NSString* language = nil;
	
	if([sender isEqualTo:syntaxListCSound])
	{
		if([syntaxListCSound clickedColumn] != 3) return;
		language = @"csound";
	}
	else if([sender isEqualTo:syntaxListPython])
	{
		if([syntaxListPython clickedColumn] != 3) return;
		language = @"python";
	}
	else if([sender isEqualTo:syntaxListLua])
	{
		if([syntaxListLua clickedColumn] != 3) return;
		language = @"lua";
	}
	else return;
	
	
	NSButtonCell* checkBox = sender;
	NSString* valueForDefault = nil;
	NSInteger arrayIndex = 0;
	NSInteger index = 0;
	
	if([language isEqualToString:@"csound"])
	{
		arrayIndex = [syntaxListCSound selectedRow];
		index = [[KeyListCSound objectAtIndex:arrayIndex] integerValue];
		valueForDefault = @"CSoundStyles";
	}
	else if([language isEqualToString:@"python"])
	{
		arrayIndex = [syntaxListPython selectedRow];
		index = [[KeyListPython objectAtIndex:arrayIndex] integerValue];
		valueForDefault = @"PythonStyles";
	}
	else if([language isEqualToString:@"lua"])
	{
		arrayIndex = [syntaxListLua selectedRow];
		index = [[KeyListLua objectAtIndex:arrayIndex] integerValue];
		valueForDefault = @"LuaStyles";
	}

	//NSLog(@"SYNTAX LANGUAGE: %@", language);
	//NSLog(@"SYNTAX SELECTED ROW: %d", arrayIndex);
	//NSLog(@"SYNTAX SELECTED STYLE: %d", index);
	
	NSString* ForeColor = [self StyleGetForeColor:language stylenumber:index];
	NSString* BackColor = [self StyleGetBackColor:language stylenumber:index];
	NSString* Bold = ([checkBox integerValue] ? @"True" : @"False");
	NSString* Italic = ([self StyleGetItalic:language stylenumber:index] ? @"True" : @"False");
	NSString* Alpha = [NSString stringWithFormat:@"%d",[self StyleGetAlpha:language stylenumber:index]];
	NSString* Eol =([self StyleGetEolFilled:language stylenumber:index] ? @"True" : @"False");
	NSString* FriendlyName = [self StyleGetFriendlyName:language stylenumber:index];
	
	NSString* value = [NSString stringWithFormat:@"%@,%@,%@,%@,%@,%@,%@",
					   ForeColor, BackColor, Bold, Italic, Alpha, Eol, FriendlyName];
	
	//NSLog(@"SYNTAX STRING STYLE: %@", value);
	
	[[wxDefaults valueForKey: valueForDefault] setValue:value forKey:[[NSNumber numberWithInteger:index] stringValue]];
	
}

- (IBAction) wxChangeItalicState:(id)sender
{
	NSString* language = nil;
	
	if([sender isEqualTo:syntaxListCSound])
	{
		if([syntaxListCSound clickedColumn] != 4) return;
		language = @"csound";
	}
	else if([sender isEqualTo:syntaxListPython])
	{
		if([syntaxListPython clickedColumn] != 4) return;
		language = @"python";
	}
	else if([sender isEqualTo:syntaxListLua])
	{
		if([syntaxListLua clickedColumn] != 4) return;
		language = @"lua";
	}
	else return;
	
	NSButtonCell* checkBox = sender;
	NSString* valueForDefault = nil;
	NSInteger arrayIndex = 0;
	NSInteger index = 0;
	
	if([language isEqualToString:@"csound"])
	{
		arrayIndex = [syntaxListCSound selectedRow];
		index = [[KeyListCSound objectAtIndex:arrayIndex] integerValue];
		valueForDefault = @"CSoundStyles";
	}
	else if([language isEqualToString:@"python"])
	{
		arrayIndex = [syntaxListPython selectedRow];
		index = [[KeyListPython objectAtIndex:arrayIndex] integerValue];
		valueForDefault = @"PythonStyles";
	}
	else if([language isEqualToString:@"lua"])
	{
		arrayIndex = [syntaxListLua selectedRow];
		index = [[KeyListLua objectAtIndex:arrayIndex] integerValue];
		valueForDefault = @"LuaStyles";
	}
	
	//NSLog(@"SYNTAX LANGUAGE: %@", language);
	//NSLog(@"SYNTAX SELECTED ROW: %d", arrayIndex);
	//NSLog(@"SYNTAX SELECTED STYLE: %d", index);
	
	
	NSString* ForeColor = [self StyleGetForeColor:language stylenumber:index];
	NSString* BackColor = [self StyleGetBackColor:language stylenumber:index];
	NSString* Bold = ([self StyleGetBold:language stylenumber:index] ? @"True" : @"False");
	NSString* Italic = ([checkBox integerValue] ? @"True" : @"False");
	NSString* Alpha = [NSString stringWithFormat:@"%d",[self StyleGetAlpha:language stylenumber:index]];
	NSString* Eol = ([self StyleGetEolFilled:language stylenumber:index] ? @"True" : @"False");
	NSString* FriendlyName = [self StyleGetFriendlyName:language stylenumber:index];
	
	NSString* value = [NSString stringWithFormat:@"%@,%@,%@,%@,%@,%@,%@",
					   ForeColor, BackColor, Bold, Italic, Alpha, Eol, FriendlyName];
	
	//NSLog(@"SYNTAX STRING STYLE: %@", value);
	
	[[wxDefaults valueForKey: valueForDefault] setValue:value forKey:[[NSNumber numberWithInteger:index] stringValue]];
	
}




- (void) tabView:(NSTabView *)tabView didSelectTabViewItem:(NSTabViewItem *)tabViewItem
{
	//NSLog(@"TABVIEW ITEM SELECTED");
	@try
	{
		[syntaxListCSound reloadData];
		[syntaxListPython reloadData];
		[syntaxListLua reloadData];
		if([NSColorPanel sharedColorPanelExists])
		{
			[[NSColorPanel sharedColorPanel] close];
		}
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxSettings -> tabView Error: %@ - %@", [e name], [e reason]);
	}
}


- (IBAction) wxChangeColor:(id)sender
{
	
	NSTableView* syntaxList = nil;
	NSArray* keyList = nil;
	NSString* language = nil;
	
	if([sender isEqualTo:syntaxListCSound])
	{
		syntaxList = syntaxListCSound;
		keyList = KeyListCSound;
		language = @"csound";
	}
	else if([sender isEqualTo:syntaxListPython])
	{
		syntaxList = syntaxListPython;
		keyList = KeyListPython;
		language = @"python";
	}
	else if([sender isEqualTo:syntaxListLua])
	{
		syntaxList = syntaxListLua;
		keyList = KeyListLua;
		language = @"lua";
	}
	else return;

	if([syntaxList clickedRow] < 0) return;
	
	
	//NSLog(@"wxChangeColor - row: %d - Column %d", [syntaxList clickedRow], [syntaxList clickedColumn]);
	
	
	if([syntaxList clickedColumn] > 0 && [syntaxList clickedColumn] < 3)
	{
		
		NSColorPanel *colorPanel = [NSColorPanel sharedColorPanel];
		
		NSInteger arrayIndex = [syntaxList selectedRow];
		NSInteger index = [[keyList objectAtIndex:arrayIndex] integerValue];
		NSString* color = nil;
		
		[[NSColorPanel sharedColorPanel] setAction:nil];
		[colorPanel setTarget:nil];

		//FORE COLOR
		if([syntaxList clickedColumn] == 1)
		{
			color = [self StyleGetForeColor:language stylenumber:index];
			[colorPanel setColor:[self HexToNSColor:color]];
			[colorPanel setAction:@selector(changeForeColor:)];
			
		}
		//BACK COLOR
		else if([syntaxList clickedColumn] == 2)
		{
			color = [self StyleGetBackColor:language stylenumber:index];
			[colorPanel setColor:[self HexToNSColor:color]];
			[colorPanel setAction:@selector(changeBackColor:)];
		}
		
		//[colorPanel setDelegate:syntaxList];
		[colorPanel setTarget:self];
		
		[colorPanel makeKeyAndOrderFront:self];
		
	}	
}


- (void) changeForeColor:(id)sender
{
	NSTableView* syntaxList = nil;
	NSArray* keyList = nil;
	NSString* language = nil;
	NSString* valueForDefault = nil;

	
	if([[sender delegate] isEqualTo:syntaxListCSound])
	{
		syntaxList = syntaxListCSound;
		keyList = KeyListCSound;
		language = @"csound";
		valueForDefault = @"CSoundStyles";
	}
	else if([[sender delegate] isEqualTo:syntaxListPython])
	{
		syntaxList = syntaxListPython;
		keyList = KeyListPython;
		language = @"python";
		valueForDefault = @"PythonStyles";
	}
	else if([[sender delegate] isEqualTo:syntaxListLua])
	{
		syntaxList = syntaxListLua;
		keyList = KeyListLua;
		language = @"lua";
		valueForDefault = @"LuaStyles";
	}
	else return;
	
	
	NSInteger arrayIndex = [syntaxList selectedRow];
	NSInteger index = [[keyList objectAtIndex:arrayIndex] integerValue];
	NSColorPanel* cp = sender;
	
	//NSLog(@"SYNTAX LANGUAGE: %@", language);
	//NSLog(@"SYNTAX SELECTED ROW: %d", arrayIndex);
	//NSLog(@"SYNTAX SELECTED STYLE: %d", index);
	
	
	
	NSString* ForeColor = [self NSColorToHex:[cp color]];
	NSString* BackColor = [self StyleGetBackColor:language stylenumber:index];
	NSString* Bold = ([self StyleGetBold:language stylenumber:index] ? @"True" : @"False");
	NSString* Italic =  ([self StyleGetItalic:language stylenumber:index] ? @"True" : @"False");
	NSString* Alpha = [NSString stringWithFormat:@"%d",[self StyleGetAlpha:language stylenumber:index]];
	NSString* Eol = ([self StyleGetEolFilled:language stylenumber:index] ? @"True" : @"False");
	NSString* FriendlyName = [self StyleGetFriendlyName:language stylenumber:index];
	
	NSString* value = [NSString stringWithFormat:@"%@,%@,%@,%@,%@,%@,%@",
					   ForeColor, BackColor, Bold, Italic, Alpha, Eol, FriendlyName];
	
	//NSLog(@"SYNTAX STRING STYLE: %@", value);
	
	[[wxDefaults valueForKey: valueForDefault] setValue:value forKey:[[NSNumber numberWithInteger:index] stringValue]];
	[syntaxList reloadData];
	
}

- (void) changeBackColor:(id)sender
{
	NSTableView* syntaxList = nil;
	NSArray* keyList = nil;
	NSString* language = nil;
	NSString* valueForDefault = nil;
	
	
	if([[sender delegate] isEqualTo:syntaxListCSound])
	{
		syntaxList = syntaxListCSound;
		keyList = KeyListCSound;
		language = @"csound";
		valueForDefault = @"CSoundStyles";
	}
	else if([[sender delegate] isEqualTo:syntaxListPython])
	{
		syntaxList = syntaxListPython;
		keyList = KeyListPython;
		language = @"python";
		valueForDefault = @"PythonStyles";
	}
	else if([[sender delegate] isEqualTo:syntaxListLua])
	{
		syntaxList = syntaxListLua;
		keyList = KeyListLua;
		language = @"lua";
		valueForDefault = @"LuaStyles";
	}
	else return;
	
	
	NSInteger arrayIndex = [syntaxList selectedRow];
	NSInteger index = [[keyList objectAtIndex:arrayIndex] integerValue];
	NSColorPanel* cp = sender;
	
	//NSLog(@"SYNTAX LANGUAGE: %@", language);
	//NSLog(@"SYNTAX SELECTED ROW: %d", arrayIndex);
	//NSLog(@"SYNTAX SELECTED STYLE: %d", index);
	
	
	
	NSString* ForeColor = [self StyleGetForeColor:language stylenumber:index];
	NSString* BackColor = [self NSColorToHex:[cp color]];
	NSString* Bold = ([self StyleGetBold:language stylenumber:index] ? @"True" : @"False");
	NSString* Italic =  ([self StyleGetItalic:language stylenumber:index] ? @"True" : @"False");
	NSString* Alpha = [NSString stringWithFormat:@"%d",[self StyleGetAlpha:language stylenumber:index]];
	NSString* Eol = ([self StyleGetEolFilled:language stylenumber:index] ? @"True" : @"False");
	NSString* FriendlyName = [self StyleGetFriendlyName:language stylenumber:index];
	
	NSString* value = [NSString stringWithFormat:@"%@,%@,%@,%@,%@,%@,%@",
					   ForeColor, BackColor, Bold, Italic, Alpha, Eol, FriendlyName];
	
	//NSLog(@"SYNTAX STRING STYLE: %@", value);
	
	[[wxDefaults valueForKey: valueForDefault] setValue:value forKey:[[NSNumber numberWithInteger:index] stringValue]];
	[syntaxList reloadData];
}


- (IBAction) wxResetSyntaxCSound:(id)sender
{
	NSString* userCSoundStylesPath=[[NSBundle mainBundle] pathForResource:@"CSoundSyntax"
																   ofType:@"plist"];
	NSDictionary* userCSoundStylesDict = [NSDictionary dictionaryWithContentsOfFile:userCSoundStylesPath];
	
	[wxDefaults setValue:userCSoundStylesDict forKey:@"CSoundStyles"];
	[syntaxListCSound reloadData];
}

- (IBAction) wxResetSyntaxPython:(id)sender
{
	NSString* userPythonStylesPath=[[NSBundle mainBundle] pathForResource:@"PythonSyntax"
																   ofType:@"plist"];
	NSDictionary* userPythonStylesDict = [NSDictionary dictionaryWithContentsOfFile:userPythonStylesPath];
	
	[wxDefaults setValue:userPythonStylesDict forKey:@"PythonStyles"];
	[syntaxListPython reloadData];
}

- (IBAction) wxResetSyntaxLua:(id)sender
{
	NSString* userLuaStylesPath=[[NSBundle mainBundle] pathForResource:@"LuaSyntax"
																   ofType:@"plist"];
	NSDictionary* userLuaStylesDict = [NSDictionary dictionaryWithContentsOfFile:userLuaStylesPath];
	
	[wxDefaults setValue:userLuaStylesDict forKey:@"LuaStyles"];
	[syntaxListLua reloadData];
}

- (IBAction) wxResetCSoundAdditionalFlags:(id)sender
{
	NSString* addFlags =
	@"[Realtime output]: -odac\n"
	"[Render to filename]: -W -o\"CompilerOutput.wav\"\n"
	"[Render to file using csd/orc/sco name]: -W -o\"*.wav\"\n"
	"[Render to file asking for its name]: -W -o\"?.wav\"";
	
	[wxDefaults setValue:addFlags forKey:@"CSoundAdditionalFlags"];
}

//--------------------------------------------------------------------------------------------------
// TABLEVIEW IMPLEMENTATIONS AND RELATED METHODS
//--------------------------------------------------------------------------------------------------

			 
			 






#pragma mark Browse buttons
//--------------------------------------------------------------------------------------------------
// Browse buttons implementation
//--------------------------------------------------------------------------------------------------
- (IBAction) wxBrowseFile:(id)sender
{
	if (![sender isKindOfClass:[NSButton class]])
        return;
	
	tagSender = [(NSButton *)sender tag];
	//NSLog(@"%d",tagSender);
	

	NSOpenPanel *openPanel = [NSOpenPanel openPanel];
	[openPanel setResolvesAliases:YES];	
	[openPanel setAllowsMultipleSelection:false];
	
	
	//Browse for environment directories 
	//No file selection allowed
	if(tagSender >= 60 && tagSender <= 65)
	{
		[openPanel setCanChooseFiles: false];
		[openPanel setCanChooseDirectories:true];
	}
	
	//Set initial directory for some paths
	NSArray *paths = nil;
	NSString *applicationDirectory = nil;
	switch (tagSender) {
		case 12:
		case 21:
		case 31:
		case 41:
		case 50:
			paths = NSSearchPathForDirectoriesInDomains(NSApplicationDirectory, NSLocalDomainMask, YES);
			applicationDirectory = [paths objectAtIndex:0];
			break;
		default:
			break;
	}
	
	//Undocumented feature !!
	//[[openPanel _navView] setShowsHiddenFiles:YES];
	
	
	//SHEET WINDOW
	[openPanel beginSheetForDirectory:applicationDirectory
						 file:nil
						types:nil  //[NSArray arrayWithObjects:@"txt",@"rtf",@"html",nil]
			   modalForWindow:preferencesWindow  //[self window]
				modalDelegate:self
			   didEndSelector:@selector(openPanelDidEnd:
										returnCode:
										contextInfo:)
				  contextInfo:nil];

}

- (void)openPanelDidEnd:(NSOpenPanel *)panel returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	if (returnCode == NSOKButton)
	{
		//NSArray *filesToOpen = [panel filenames];
		NSString* filename = [[panel filenames] objectAtIndex:0];
		
		//NSLog([[panel filenames] objectAtIndex:0]);
		
		switch (tagSender) {
			//Browse directories
			case 10:
				[wxDefaults setValue:filename forKey:@"CSoundConsolePath"];
				break;
			case 11:
				[wxDefaults setValue:filename forKey:@"CSoundHelpHtmlPath"];
				break;
			case 12:
				[wxDefaults setValue:filename forKey:@"CSoundExternalGuiPath"];
				break;
			case 20:
				[wxDefaults setValue:filename forKey:@"PythonConsolePath"];
				break;
			case 21:
				[wxDefaults setValue:filename forKey:@"PythonExternalGuiPath"];
				break;
			case 30:
				[wxDefaults setValue:filename forKey:@"LuaConsolePath"];
				break;
			case 31:
				[wxDefaults setValue:filename forKey:@"LuaExternalGuiPath"];
				break;
			case 41:
				[wxDefaults setValue:filename forKey:@"CabbagePath"];
				break;
			case 50:
				[wxDefaults setValue:filename forKey:@"WaveEditorPath"];
				break;
				
			//Browse Environment
			case 60:
				[wxDefaults setValue:filename forKey:@"SFDIRPath"];
				break;
			case 61:
				[wxDefaults setValue:filename forKey:@"SSDIRPath"];
				break;
			case 62:
				[wxDefaults setValue:filename forKey:@"SADIRPath"];
				break;
			case 63:
				[wxDefaults setValue:filename forKey:@"MFDIRPath"];
				break;
			case 64:
				[wxDefaults setValue:filename forKey:@"INCDIRPath"];
				break;
			case 65:
				[wxDefaults setValue:filename forKey:@"OPCODEDIRPath"];
				break;
			default:
				break;
		}
	}
}





@end
