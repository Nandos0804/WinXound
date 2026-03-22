//
//  wxMainController.m
//  WinXound
//
//  Created by Stefano Bonetti on 19/01/10.
//

#import "wxMainController.h"
#import "wxSettings.h"
#import "wxDocument.h"
#import "wxGlobal.h"
#import "wxAbout.h"
#import "wxCodeFormatter.h"
#import "wxAnalysis.h"
#import "wxImportExport.h"
#import "wxCodeRepository.h"
#import "wxCSoundRepository.h"
#import "wxAutoComplete.h"
#import "wxPlayer.h"




//Callback for FSEvent
static void feCallback(ConstFSEventStreamRef streamRef, 
					   void *clientCallBackInfo, 
					   size_t numEvents, void *eventPaths, 
					   const FSEventStreamEventFlags eventFlags[], 
					   const FSEventStreamEventId eventIds[]) 
{
    
	char **paths = (char**)eventPaths;
    int i;
    for (i = 0; i < numEvents; i++) 
	{
		//[NSNumber numberWithUnsignedLongLong:eventIds[i]], @"id",
		//[NSString stringWithUTF8String:paths[i]], @"path",
		//[NSString stringWithFormat:@"%04x",eventFlags[i]] , @"flag",

		if([[[NSString stringWithUTF8String:paths[i]] lowercaseString] rangeOfString:@"trash"].location != NSNotFound)
		{
			//NSLog(@"Trash detected!");
			wxMainController *ctrl = (wxMainController *)clientCallBackInfo;
			[ctrl refreshRecentList];
			break;
		}
	}
}







//----------------------------------------------------------------------------------------------------------
// wxMainController IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------
@implementation wxMainController


static id sharedInstance = nil;

NSMutableDictionary* Opcodes = nil;
wxSettings* Settings = nil;
BOOL hasOpenedFile = false;
wxAnalysis* Analysis = nil;



#pragma mark - Various Overrides
//----------------------------------------------------------------------------------------------------------
// INITIALIZATION AND VARIOUS OVERRIDES
//----------------------------------------------------------------------------------------------------------
+ (wxMainController*)sharedInstance
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

+ (void)initialize {
	
	NSLog(@"wxMainController: INITIALIZE");
	
	//LOAD USER DEFAULT DATA
	[self setupDefaults];
	
	
	//LOAD GLOBAL SETTINGS
	Settings = [[wxSettings alloc] init];
	[Settings initialize];
	
	
	//ALLOC AND INIT ANALYSIS TOOL
	Analysis = [[wxAnalysis alloc] init];
	[Analysis initialize];
	
	
	//LOAD OPCODES DATABASE (DICTIONARY)
	NSError* error = nil;
	NSString* path = [[NSBundle mainBundle] pathForResource: @"opcodes" 
													 ofType: @"txt" inDirectory: nil];	
	NSString* opc = [NSString stringWithContentsOfFile: path
											  encoding: NSUTF8StringEncoding
												 error: &error];
	if (error && [[error domain] isEqual: NSCocoaErrorDomain])
		NSLog(@"%@", error);
		
	
	NSArray* lines = [opc componentsSeparatedByString:@"\r\n"];  // CRLF from Windows opcodes.txt
	//NSArray* lines = [opc componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];
	
	
	Opcodes = [[NSMutableDictionary alloc] init];
		
	NSEnumerator* enumerator = [lines objectEnumerator];
	id obj;
	@try 
	{
		while(obj = [enumerator nextObject]) 
		{
			NSArray* items = [obj componentsSeparatedByString:@";"];
			if([items count] > 2)
			{
				NSString* k = [items objectAtIndex:0];
				NSString* key = [k stringByReplacingOccurrencesOfString:@"\"" withString:@""];
				NSString* value = [NSString stringWithFormat:@"%@;%@;%@", 
								   [items objectAtIndex:1], 
								   [items objectAtIndex:2],
								   [items objectAtIndex:3]];
				//NSLog(@"key: %@ - value: %@", key, value);
				[Opcodes setValue:value forKey:key];
			}
		}
	}
	@catch (NSException* e) 
	{
		NSLog(@"Error: %@", e);
	}

	
}


- (BOOL) applicationShouldHandleReopen:(NSApplication *)sender hasVisibleWindows:(BOOL)flag
{
	//NSLog(@"wxMainController: applicationShouldHandleReopen");
	
	if([[[NSDocumentController sharedDocumentController] documents] count] == 0 &&
	   [HelpWindow isVisible] == false &&
	   [wxPLAYER IsWindowVisible] == false)
		[self displayStartWindow];
	
	//if(flag == false)
	//	[self displayStartWindow];

	return false;
}

- (void) applicationDidUnhide:(NSNotification *)notification
{
	//NSLog(@"wxMainController: applicationDidUnhide");
	
	if([[[NSDocumentController sharedDocumentController] documents] count] == 0 &&
	   [HelpWindow isVisible] == false  &&
	   [wxPLAYER IsWindowVisible] == false)
		[self displayStartWindow];
}

//- (void) applicationDidBecomeActive:(NSNotification *)notification
//{
//	NSLog(@"wxMainController: applicationDidBecomeActive");
//	if([[[NSDocumentController sharedDocumentController] documents] count] == 0)
//	[self displayStartWindow];
//}

- (void) applicationDidFinishLaunching:(NSNotification *)notification
{
	NSLog(@"wxMainController: applicationDidFinishLaunching");
	
	[RecentFiles setDataSource: self];
	[RecentFiles setDoubleAction:@selector(wxOpenRecent:)];
	[RecentFiles reloadData];
	
	
	//OLD: Help substituted with pdf format!
	//[self goHome:self];
	//NSString* path = [[NSBundle mainBundle] pathForResource: @"winxound_help" 
	//												 ofType: @"html" inDirectory: nil];	
	//[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:path]]];
	
	
	//Display WinXound version
	NSString* versionStr = [self GetInfoValueForKey:@"CFBundleShortVersionString"];
	[version setStringValue:[NSString stringWithFormat:@"version %@", versionStr]];
	
	
	//Set default font size for WebView (HelpBrowser)
	[[helpBrowser preferences] setDefaultFontSize:14];
	[[helpBrowser preferences] setDefaultFixedFontSize:14];
	
	
	////////////////////////////////////////////////////////////
	//Set FSEvent to check recent files changes (delete, rename)
	CFStringRef _path = CFSTR("/");
    CFArrayRef _pathsToWatch = CFArrayCreate(NULL, (const void **)&_path, 1, NULL);
    
    //Use context only to simply pass wxMainController object
	_context = (FSEventStreamContext*)malloc(sizeof(FSEventStreamContext));
    _context->version = 0;
    _context->info = (void*)self; 
    _context->retain = NULL;
    _context->release = NULL;
    _context->copyDescription = NULL;
	
    _stream = FSEventStreamCreate(NULL,
								  &feCallback,
								  _context,
								  _pathsToWatch,
								  kFSEventStreamEventIdSinceNow, /* Or a previous event ID */
								  1.0, /* Latency in seconds */
								  kFSEventStreamCreateFlagNone
								  );
	
	//kFSEventStreamCreateFlagIgnoreSelf only OsX 10.6
	
    FSEventStreamScheduleWithRunLoop(_stream, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);    
	FSEventStreamStart(_stream); 
	
	CFRelease(_pathsToWatch); //Release CF object
	////////////////////////////////////////////////////////////
	
	
	//Load the Code Repository Files
	[wxCODEREP initializeRepository];
	[wxCSOUNDREP FillTreeViewOpcodes];
	[wxAUTOCOMP FillTableWithOpcodes];
	
	
	
	//Update the AdditionalFlags menu:
	[self UpdateAdditionalFlagsMenu];
	
}

- (void) UpdateAdditionalFlagsMenu
{
	//Set the AdditionalFlags menu's
	NSLog(@"Update AdditionalFlags menu");
	
	NSArray* lines = [[wxDefaults valueForKey:@"CSoundAdditionalFlags"] componentsSeparatedByString:@"\n"];
	for(NSMenuItem* mi in [menuCompileWithAdditionalOptions itemArray])
	{
		[menuCompileWithAdditionalOptions removeItem:mi];
	}
	for(NSMenuItem* mi in [menuCompileExternalWithAdditionalOptions itemArray])
	{
		[menuCompileExternalWithAdditionalOptions removeItem:mi];
	}
	
	//Add items and connect to the documents method (CompileWithAdditionFlagsClicked ... look at wxDocument!)
	for(NSString* line in lines)
	{
		//NSLog(@"Create AdditionalFlags menu: %@", line);
		
		if([line length] > 0) 
		{
			[menuCompileWithAdditionalOptions addItemWithTitle:line
														action:@selector(CompileWithAdditionalFlagsClicked:) 
												 keyEquivalent:@""];
			
			[menuCompileExternalWithAdditionalOptions addItemWithTitle:line
																action:@selector(CompileExternalWithAdditionalFlagsClicked:) 
														 keyEquivalent:@""];
		}
	}
}


- (id) GetInfoValueForKey:(NSString*)key
{
    if ([[[NSBundle mainBundle] localizedInfoDictionary] objectForKey:key])
        return [[[NSBundle mainBundle] localizedInfoDictionary] objectForKey:key];
	
    return [[[NSBundle mainBundle] infoDictionary] objectForKey:key];
}



//- (BOOL) applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)sender
//{
//	return NO;
//}


- (BOOL)applicationShouldOpenUntitledFile:(NSApplication *)sender
{
	//STARTUP ACTION (NEW DOCUMENT Yes or No)
	if([[wxDefaults valueForKey:@"StartupAction"] integerValue] == 3)
	{
		[self wxNewLuaFile:self];
	}
	else if([[wxDefaults valueForKey:@"StartupAction"] integerValue] == 2)
	{
		[self wxNewPythonFile:self];
	}
	else if([[wxDefaults valueForKey:@"StartupAction"] integerValue] == 1)
	{
		[self wxNewCsoundFile:self];
	}
	else 
	{
		if(!hasOpenedFile) [self displayStartWindow];
	}

	return NO;
}


//- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender
//{
//	//	NSLog(@"APPLICATION_SHOULD_TERMINATE_CALLED!!!");
//	//	
//	//	int answer = NSRunAlertPanel(@"Close",@"Are you Certain?",
//	//								 @"Close",@"Cancel", nil);
//	//	
//	//	if(answer == NSAlertDefaultReturn)
//	//		return NSTerminateNow;
//	//	
//	//	return NSTerminateCancel;
//	
//	NSLog(@"applicationShouldTerminate: Opcodes Release - DEALLOC_CALLED!!!");
//	
//	return NSTerminateNow;
//}


- (void) applicationWillTerminate:(NSNotification *)notification
{
	NSLog(@"wxMainController: applicationWillTerminate:");
	
	//Close and cleanup all opened documents
	//[self wxCloseAllDocuments:self];
	
	//Stop and cleanup FSEvent stuffs
	FSEventStreamStop(_stream);
	FSEventStreamInvalidate(_stream); /* will remove from runloop */
	FSEventStreamRelease(_stream);
	free(_context);
	
	//Save user settings
	NSUserDefaultsController *defaultsController = [NSUserDefaultsController sharedUserDefaultsController];
	[defaultsController save:self];
	
	//Release various objects
	[Opcodes release];
	[Settings release];
	[Analysis release];
	
	
	//Delete temp files
	//Search for User Application Support directory
	NSArray* paths = NSSearchPathForDirectoriesInDomains(NSApplicationSupportDirectory, NSUserDomainMask, YES);
	if(paths != nil)
	{
		if([paths count] > 0)
		{
			NSString* winxoundPath = [[NSString stringWithFormat:@"%@/WinXound/Temp", [paths objectAtIndex:0]] retain];
			
			//If the directory exist we delete the temporary files
			if([[NSFileManager defaultManager] fileExistsAtPath:winxoundPath])
			{
				//[[NSFileManager defaultManager] removeItemAtPath:winxoundPath error:nil];
				//NSFileManager* fm = [[[NSFileManager alloc] init] autorelease];
				NSDirectoryEnumerator* en = [[NSFileManager defaultManager] enumeratorAtPath:winxoundPath];    
				while (NSString* file = [en nextObject]) 
				{
					[[NSFileManager defaultManager] removeItemAtPath:[winxoundPath stringByAppendingPathComponent:file] error:nil];
				}
				
			}
		}
	}
	
}

- (BOOL) application:(NSApplication *)sender openFile:(NSString *)filename
{
	//[self closeStartWindow];
	
	if([self checkForOrcSco:filename] == false)
	{
		NSDocument* doc	=
		[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:[NSURL fileURLWithPath:filename]
																			   display:YES 
																				 error:nil];
		if(doc == nil) return false;
	}
	
	hasOpenedFile = true;
	return true;
	

}

- (BOOL) validateMenuItem:(NSMenuItem *)menuItem
{
	//NSLog(@"wxMainController: validateMenuItem");
	
	//Close All:
	if([[menuItem title] isEqualToString:@"Close All"])
	{
		//NSLog(@"wxMainController: VALIDATE MENU CLOSE ALL");
		return ([[[NSDocumentController sharedDocumentController] documents] count] > 1);
	}
	
	if([menuItem action] == @selector(CompileWithAdditionalFlagsClicked:))
	{
		return ([[[NSDocumentController sharedDocumentController] documents] count] > 0);
	}
	
	/* Moved to wxDocument...
	//Cabbage Update:
	if([menuItem action] == @selector(wxCabbageUpdate:))
	{
		return ([[[NSDocumentController sharedDocumentController] documents] count] > 0);
	}
	*/

	//Find menu:
	if([[menuItem title] isEqualToString:@"Find and Replace"] ||
	   [[menuItem title] isEqualToString:@"Find Next"] ||
	   [[menuItem title] isEqualToString:@"Find Previous"] ||
	   [[menuItem title] isEqualToString:@"Use Selection for Find"])
	{
		return ([[[NSDocumentController sharedDocumentController] documents] count] > 0 ||
				[HelpWindow isVisible]);
	}
	
	
	//Clear Recent Documents:
	if([menuItem action] == @selector(clearRecentDocuments:))
	{
		//NSLog(@"wxMainController: VALIDATE clearRecentDocuments");
		return ([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count] > 0);
	}
		
	
	return true;
}

//OVERRIDE NSDOCUMENTCONTROLLER clearRecentDocuments (to check documents for RecentFiles tableView)
- (IBAction)clearRecentDocuments:(id)sender
{
	//NSLog(@"wxMainController: clearRecentDocuments");
	[[NSDocumentController sharedDocumentController] clearRecentDocuments:sender];
	[RecentFiles reloadData];
}

//----------------------------------------------------------------------------------------------------------
// INITIALIZATION AND VARIOUS OVERRIDES
//----------------------------------------------------------------------------------------------------------
















#pragma mark - Various METHODS
//----------------------------------------------------------------------------------------------------------
// VARIOUS USEFUL METHODS
//----------------------------------------------------------------------------------------------------------
- (NSMutableDictionary*) getOpcodes
{
	return Opcodes;
}

- (NSString*) getOpcodeValue:(NSString*) opcode
{
	NSString* temp = [Opcodes valueForKey:opcode];
	return temp;
}

- (wxSettings*) getSettings
{
	return Settings;
}

- (void) displayStartWindow
{
	if(StartWindow != nil)
	{
		[RecentFiles reloadData];
		if([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count] > 0)
			[RecentFiles selectRowIndexes:0 byExtendingSelection:NO];
		
		[StartWindow makeKeyAndOrderFront:self];
		[StartWindow makeFirstResponder:version]; 
		
		//[self startTimer];
	}
}

- (void)closeStartWindow
{
	if(StartWindow != nil)
	{
		[StartWindow close];
		//[self stopTimer];
	}
}

- (void)applySettings
{
	//OLD: REMOVE !!!
	//NSUserDefaultsController *defaultsController = [NSUserDefaultsController sharedUserDefaultsController];
	//[defaultsController save:self];
}

- (void) refreshRecentList
{
	if(StartWindow != nil)
	{
		if([StartWindow isVisible])
		{
			//if([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count] !=
			//   [RecentFiles numberOfRows])
			{	
				[RecentFiles reloadData];
			}
		}
	}
}

- (NSInteger)ShowMessage:(NSString *)message 
		 informativeText:(NSString *)informativeText 
		   defaultButton:(NSString *)defaultButton 
		 alternateButton:(NSString *)alternateButton 
			 otherButton:(NSString *)otherButton
{	
	NSAlert *alert = [[NSAlert alloc] init];
	
	[alert setMessageText:message];
	[alert setInformativeText:informativeText];
	
	if (defaultButton != nil) {
		[alert addButtonWithTitle:defaultButton];
	}
	if (alternateButton != nil) {
		[alert addButtonWithTitle:alternateButton];
	}
	if (otherButton != nil) {
		[alert addButtonWithTitle:otherButton];
	}
	
	if([message rangeOfString:@"WinXound Error:"].location != NSNotFound)
	{
		[alert setAlertStyle:NSCriticalAlertStyle];
	}
	
	//return [alert runModal];
	NSInteger ret = [alert runModal];
	[alert release];
	
	return ret;
	
}

- (void) ShowMessageError:(NSString*)title error:(NSString*)error;
{
	//METHOD TO VISUALIZE AND REPORT ERRORS IN WINXOUND
	
	NSInteger ret = [self ShowMessage:[NSString stringWithFormat:@"WinXound Error:\n%@", title] 
					  informativeText:[NSString stringWithFormat:
									   @"%@\n\n"
									   "If you would like to signal this error message to the author "
									   "please click the 'OK' button (thanks for your help).", error]
						defaultButton:@"OK" 
					  alternateButton:@"Cancel" 
						  otherButton:nil];
	
	//	NSAlertDefaultReturn means the user pressed the default button.
	//	NSAlertAlternateReturn means the user pressed the alternate button.
	//	NSAlertOtherReturn means the user pressed the other button.
	//	NSAlertErrorReturn means an error occurred while running the alert panel.
	//  NSAlertFirstButtonReturn  = 1000,
	//  NSAlertSecondButtonReturn  = 1001,
	//  NSAlertThirdButtonReturn  = 1002
	
	if (ret == NSAlertFirstButtonReturn) //OK BUTTON PRESSED - Send an email to authors
	{
		//NSLog(@"OK BUTTON PRESSED");
		//%0a=linefeed &0d=carriage return
		
		NSString* EmailLink = 
			[NSString stringWithFormat:
			@"mailto:stefano_bonetti@tin.it?"
			 "subject=WinXound Error Notification [OSX]"
			 "&body=Title: %@\nError: %@",    
			title, error];
		
		NSString* finalLink = [EmailLink stringByAddingPercentEscapesUsingEncoding:NSASCIIStringEncoding];
		//NSLog(@"%@",[NSURL URLWithString:finalLink]);
		[[NSWorkspace sharedWorkspace] openURL:[NSURL URLWithString:finalLink]];
		//LSOpenCFURLRef( (CFURLRef) EmailLink, NULL);
		
	}

	
}

+ (void)setupDefaults
{
    NSString *userDefaultsValuesPath;
    NSDictionary *userDefaultsValuesDict;
    
	//NSDictionary *initialValuesDict;
    //NSArray *resettableUserDefaultsKeys;
	
	
	
    // load the default values for the user defaults
    userDefaultsValuesPath=[[NSBundle mainBundle] pathForResource:@"UserDefaults"
														   ofType:@"plist"];
    userDefaultsValuesDict=[NSDictionary dictionaryWithContentsOfFile:userDefaultsValuesPath];
	
	
	
	//ADD LANGUAGES SYNTAX
	//CSOUND
	NSString* userCSoundStylesPath=[[NSBundle mainBundle] pathForResource:@"CSoundSyntax"
																   ofType:@"plist"];
	NSDictionary* userCSoundStylesDict = [NSDictionary dictionaryWithContentsOfFile:userCSoundStylesPath];
	[userDefaultsValuesDict setValue:userCSoundStylesDict forKey:@"CSoundStyles"];
	
	//PYTHON
	NSString* userPythonStylesPath=[[NSBundle mainBundle] pathForResource:@"PythonSyntax"
																   ofType:@"plist"];
	NSDictionary* userPythonStylesDict = [NSDictionary dictionaryWithContentsOfFile:userPythonStylesPath];
	[userDefaultsValuesDict setValue:userPythonStylesDict forKey:@"PythonStyles"];
	
	//LUA
	NSString* userLuaStylesPath=[[NSBundle mainBundle] pathForResource:@"LuaSyntax"
																ofType:@"plist"];
	NSDictionary* userLuaStylesDict = [NSDictionary dictionaryWithContentsOfFile:userLuaStylesPath];
	[userDefaultsValuesDict setValue:userLuaStylesDict forKey:@"LuaStyles"];
	
	
	
	
	//ADD ANALYSIS VALUES
	NSString* valuesPath=[[NSBundle mainBundle] pathForResource:@"analysis"
														 ofType:@"plist"];
	NSDictionary* analysisValuesDict =[NSDictionary dictionaryWithContentsOfFile:valuesPath];
	//[userDefaultsValuesDict setValue:analysisValuesDict forKey:@"Analysis"];
	[userDefaultsValuesDict setValuesForKeysWithDictionary:analysisValuesDict];
	
	
	
    // set all in the standard user defaults
    [[NSUserDefaults standardUserDefaults] registerDefaults:userDefaultsValuesDict];
	
    // if your application supports resetting a subset of the defaults to
    // factory values, you should set those values
    // in the shared user defaults controller
    //resettableUserDefaultsKeys=[NSArray arrayWithObjects:@"Value1",@"Value2",@"Value3",nil];
    //initialValuesDict=[userDefaultsValuesDict dictionaryWithValuesForKeys:resettableUserDefaultsKeys];
	
    // Set the initial values in the shared user defaults controller
    [[NSUserDefaultsController sharedUserDefaultsController] setInitialValues:userDefaultsValuesDict];
	
	
	[[NSUserDefaultsController sharedUserDefaultsController] setAppliesImmediately:true];
}

- (NSString*)getStringFromFilename:(NSString*)filename
{
	
	NSString* ret = nil;
	
	//1. FIRST LOAD METHOD: use NSString
	ret =  [[NSString stringWithContentsOfFile:filename 
									  encoding:NSUTF8StringEncoding 
										 error:nil] copy];	
	
	//2. IF 1. FAILS: use NSAttributedString
	if(ret == nil)
	{
		//NSCharacterEncodingDocumentOption = NSUTF8StringEncoding
		NSDictionary* options = [NSDictionary  dictionaryWithObject:NSPlainTextDocumentType forKey:NSDocumentTypeDocumentOption];
		NSAttributedString *attrString = [[NSAttributedString alloc] initWithURL:[NSURL fileURLWithPath:filename] 
																		 options:options 
															  documentAttributes:nil 
																		   error:nil];		
		if(attrString != nil)
		{
			ret = [[attrString string] copy];
			[attrString release];
		}
	}
	
	return [ret autorelease];
	
}

//----------------------------------------------------------------------------------------------------------
// VARIOUS USEFUL METHODS
//----------------------------------------------------------------------------------------------------------


















#pragma mark - IBActions
//----------------------------------------------------------------------------------------------------------
// IBACTIONS ADN RELATIVE STUFFS
//----------------------------------------------------------------------------------------------------------
- (IBAction)wxNewCsoundFile:(id)sender
{	
	[self createNewCsoundFileWithContentOfString:nil];
}

- (void)createNewCsoundFileWithContentOfString:(NSString*)text
{
	NSDocument* doc = [[NSDocumentController sharedDocumentController] makeUntitledDocumentOfType:@"CSound Files" error:nil];
	if(doc != nil) 
	{
		[[NSDocumentController sharedDocumentController] addDocument:doc];
		//[doc setFileName:@"Untitled.csd"];
		//[doc setFileType:@"csd"];
		[doc makeWindowControllers];
		[doc showWindows];
		[self closeStartWindow];
		
		if(text != nil)
		{
			wxDocument* wxdoc = (wxDocument*) doc;
			[wxdoc setTextContent:text];
		}
	}
}

- (BOOL) checkForOrcSco:(NSString*)filename
{
	if([[[filename lastPathComponent] lowercaseString] hasSuffix:@".orc"] ||
	   [[[filename lastPathComponent] lowercaseString] hasSuffix:@".sco"])
	{
		
		//Alert message for importing orc/sco
		//NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
		//NSLog(@"%d",[wxDefaults valueForKey:@"ShowImportOrcScoMessage"]);
		
		//if([[wxDefaults valueForKey:@"ShowImportOrcScoMessage"] boolValue])
		if([[wxDefaults valueForKey:@"OrcScoImport"] integerValue] == 0) //SHOW MESSAGE
		{
			
			NSAlert *alert = [[[NSAlert alloc] init] autorelease];
			[alert setMessageText:     @"WinXound Orc/Sco Import:"];
			[alert setInformativeText: @"Would you like to convert your Orc/Sco file to a new Csd document?\n\n"
										"To change the default Orc/Sco import action please look at:\n"
										"WinXound Preferences->General->Orc/Sco Import field."];
			[alert addButtonWithTitle: @"No"];
			[alert addButtonWithTitle: @"Yes"];
			[alert setDelegate:self];
			[alert setAlertStyle:NSWarningAlertStyle];
			NSInteger ret = [alert runModal];
			
			if(ret != NSAlertFirstButtonReturn)
			{
				NSString* temp = [wxIMPEXP ImportORCSCO:filename];
				[self createNewCsoundFileWithContentOfString:temp];
				return true; //Stop other processes to loading the file
			}
			
			
		}
		else if([[wxDefaults valueForKey:@"OrcScoImport"] integerValue] == 1) //CONVERT TO CSD
		{
			NSString* temp = [wxIMPEXP ImportORCSCO:filename];
			[self createNewCsoundFileWithContentOfString:temp];
			return true; //Stop other processes to loading the file			
		}
	}
	
	
	return false; //Ok for loading the file!
}


- (IBAction)wxNewOrcFile:(id)sender
{
	//NSDocument* doc = [[NSDocumentController sharedDocumentController] openUntitledDocumentAndDisplay:NO error:nil];
	NSDocument* doc = [[NSDocumentController sharedDocumentController] makeUntitledDocumentOfType:@"CSound Orc Files" error:nil];
	if(doc != nil) 
	{
		[[NSDocumentController sharedDocumentController] addDocument:doc];
		[doc makeWindowControllers];
		[doc showWindows];
		[self closeStartWindow];
	}
}
- (IBAction)wxNewScoFile:(id)sender
{
	//NSDocument* doc = [[NSDocumentController sharedDocumentController] openUntitledDocumentAndDisplay:NO error:nil];
	NSDocument* doc = [[NSDocumentController sharedDocumentController] makeUntitledDocumentOfType:@"CSound Sco Files" error:nil];
	if(doc != nil) 
	{
		[[NSDocumentController sharedDocumentController] addDocument:doc];
		[doc makeWindowControllers];
		[doc showWindows];
		[self closeStartWindow];
	}
}
- (IBAction)wxNewCabbageFile:(id)sender
{
	//NSDocument* doc = [[NSDocumentController sharedDocumentController] openUntitledDocumentAndDisplay:NO error:nil];
	NSDocument* doc = [[NSDocumentController sharedDocumentController] makeUntitledDocumentOfType:@"CSound Cabbage Files" error:nil];
	if(doc != nil) 
	{
		[[NSDocumentController sharedDocumentController] addDocument:doc];
		[doc makeWindowControllers];
		[doc showWindows];
		[self closeStartWindow];
	}
}


- (IBAction)wxNewPythonFile:(id)sender
{
	//NSDocument* doc = [[NSDocumentController sharedDocumentController] openUntitledDocumentAndDisplay:NO error:nil];
	NSDocument* doc = [[NSDocumentController sharedDocumentController] makeUntitledDocumentOfType:@"Python Files" error:nil];
	if(doc != nil) 
	{
		[[NSDocumentController sharedDocumentController] addDocument:doc];
		//[doc setFileName:@"Untitled.py"];
		//[doc setFileType:@"py"];
		[doc makeWindowControllers];
		[doc showWindows];
		[self closeStartWindow];
	}
}
- (IBAction)wxNewLuaFile:(id)sender
{
	//NSDocument* doc = [[NSDocumentController sharedDocumentController] openUntitledDocumentAndDisplay:NO error:nil];
	NSDocument* doc = [[NSDocumentController sharedDocumentController] makeUntitledDocumentOfType:@"Lua Files" error:nil];
	if(doc != nil) 
	{
		[[NSDocumentController sharedDocumentController] addDocument:doc];
		//[doc setFileName:@"Untitled.lua"];
		//[doc setFileType:@"lua"];
		[doc makeWindowControllers];
		[doc showWindows];
		[self closeStartWindow];
	}
}

- (IBAction)wxOpenRecent:(id)sender
{
	if([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count] <= 0) return;
	

	NSArray* files = [[NSDocumentController sharedDocumentController] recentDocumentURLs];
	//	NSInteger index = [RecentFiles selectedRow];
	//	
	//	if([self checkForOrcSco:[[files objectAtIndex:index] path]] == false)
	//		[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:[files objectAtIndex:index] display:YES error:nil];
	
	
	NSIndexSet* indexSet = [RecentFiles selectedRowIndexes];
	NSUInteger bufSize = [indexSet count];
	NSUInteger* buf = (NSUInteger *)calloc(bufSize, sizeof(NSUInteger)); //new unsigned int[bufSize]; //(NSUInteger *)calloc(bufSize, sizeof(NSUInteger));
	
	@try 
	{
		NSRange range = NSMakeRange([indexSet firstIndex], [indexSet lastIndex] + 1);
		[indexSet getIndexes:buf maxCount:bufSize inIndexRange:&range];
		for(NSUInteger i = 0; i != bufSize; i++)
		{
			NSUInteger index = buf[i];
			if([self checkForOrcSco:[[files objectAtIndex:index] path]] == false)
				[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:[files objectAtIndex:index] 
																					   display:YES 
																						 error:nil];
		}
		
		//delete[] buf;
	}
	@catch (NSException * e) 
	{
		[self ShowMessageError:@"wxMainController -> wxOpenRecent:" 
						 error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
	@finally 
	{
		//NSLog(@"buf deleted");
		delete[] buf;
	}

}

- (IBAction)wxOpen:(id)sender
{
	
	//NSLog(@"WXOPEN");
	NSArray* filenames = [[NSDocumentController sharedDocumentController] URLsFromRunningOpenPanel];
	if (filenames != nil)
	{
		[self closeStartWindow];
		for(NSURL* url in filenames)
		{
			//Check for orc/sco extension
			if([self checkForOrcSco:[url path]] == false)			
				[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:url display:YES error:nil];
		}
	}
	
}

- (IBAction)wxImportOrcScoToNewCsdFile:(id)sender
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
		[self createNewCsoundFileWithContentOfString:temp];
		
    } 
	
}

- (void) wxOpenOrcScoDocumentWithFilename:(NSString*)filename
{
	if (filename != nil)
	{
		[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:[NSURL fileURLWithPath:filename]  
																				   display:YES 
																					 error:nil];
	}
}

- (void) wxOpenDocumentWithFilename:(NSString*)filename
{
	if (filename != nil)
	{
		//[self closeStartWindow];
		
		//Check for orc/sco extension
		if([self checkForOrcSco:filename] == false)
			[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:[NSURL fileURLWithPath:filename]  
																				   display:YES 
																					 error:nil];
	}
}

- (IBAction)wxNewDocument:(id)sender
{
	//NSLog(@"NEW DOCUMENT");
	
	[self displayStartWindow];
}

- (IBAction)wxCloseAllDocuments:(id)sender
{
	[[NSDocumentController sharedDocumentController] closeAllDocumentsWithDelegate:nil didCloseAllSelector:nil contextInfo:nil];
}

- (IBAction)wxShowLineNumbers:(id)sender
{
	//BOOL currentValue = [[wxDefaults valueForKey:@"EditorShowLineNumbers"] boolValue];
	BOOL newValue = ![[wxDefaults valueForKey:@"EditorShowLineNumbers"] boolValue];
	//[wxDefaults setValue:[NSNumber numberWithBool:newValue] forKey:@"EditorShowLineNumbers"];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		//[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"EditorShowMatchingBracket"];
		[doc showLineNumbers: newValue];
	}
	
	//[self applySettings];
}

- (IBAction)wxShowExplorer:(id)sender
{
	//BOOL currentValue = [[wxDefaults valueForKey:@"EditorShowExplorer"] boolValue];
	BOOL newValue = ![[wxDefaults valueForKey:@"EditorShowExplorer"] boolValue];
	//[wxDefaults setValue:[NSNumber numberWithBool:newValue] forKey:@"EditorShowExplorer"];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		//Settings->EditorProperties.ShowExplorer = [doc showExplorer];
		[doc showExplorer: newValue];
	}
	
	//[self applySettings];
}

- (IBAction)wxShowOnlineHelp:(id)sender
{
	//BOOL currentValue = [[wxDefaults valueForKey:@"EditorShowIntellitip"] boolValue];
	BOOL newValue = ![[wxDefaults valueForKey:@"EditorShowIntellitip"] boolValue];
	//[wxDefaults setValue:[NSNumber numberWithBool:newValue] forKey:@"EditorShowIntellitip"];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		//Settings->EditorProperties.ShowIntelliTip = [doc showOnlineHelp];
		[doc showOnlineHelp: newValue];
	}
	
	//[self applySettings];
}

- (IBAction)wxShowToolbar:(id)sender
{
	//BOOL currentValue = [[wxDefaults valueForKey:@"EditorShowToolbar"] boolValue];
	BOOL newValue = ![[wxDefaults valueForKey:@"EditorShowToolbar"] boolValue];
	//[wxDefaults setValue:[NSNumber numberWithBool:newValue] forKey:@"EditorShowIntellitip"];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		[doc showToolbar: newValue];
	}
	
	//[self applySettings];
}

- (IBAction)wxShowAllTools:(id)sender
{
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"EditorShowLineNumbers"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"EditorShowExplorer"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"EditorShowIntellitip"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"EditorShowToolbar"];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		[doc showLineNumbers: true];
		[doc showExplorer: true];
		[doc showOnlineHelp: true];
		[doc showToolbar: true];
	}
	
	//[self applySettings];
}

- (IBAction)wxHideAllTools:(id)sender
{
	[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"EditorShowLineNumbers"];
	[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"EditorShowExplorer"];
	[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"EditorShowIntellitip"];
	[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"EditorShowToolbar"];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		[doc showLineNumbers: false];
		[doc showExplorer: false];
		[doc showOnlineHelp: false];
		[doc showToolbar: false];
	}
	
	//[self applySettings];
}

- (IBAction)wxUseWinxoundFlags:(id)sender
{	
	//[self applySettings];
	
	BOOL newValue = ![[wxDefaults valueForKey:@"UseWinxoundFlags"] boolValue];
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		[doc setUseWinXoundFlags:newValue];
	}
}


- (IBAction)wxShowPreferences:(id)sender
{
	//[self closeStartWindow];
	[Settings ShowPreferencesWindow];
}

//+ (void)applyPreferencesToOpenedDocuments
- (void)applyPreferencesToOpenedDocuments
{	
	[self UpdateAdditionalFlagsMenu];
	
	for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
	{
		[doc configureEditor];
		[doc configureCompiler];
		[doc configureExplorer];
		[doc UpdateAdditionalFlagsPopupButtonList];
	}
}

- (IBAction)wxCallMediaPlayer:(id)sender
{
	if([[wxDefaults valueForKey:@"DefaultWavePlayer"] integerValue] == 1)
	{
		if([[NSFileManager defaultManager] fileExistsAtPath:@"/Applications/QuickTime Player.app"])
		{
			[[NSWorkspace sharedWorkspace] launchApplication:@"/Applications/QuickTime Player.app"];
			return;
		}
	}
	
	[wxPLAYER showPlayerWindow];
}

- (IBAction)wxCallWaveEditor:(id)sender
{	
	if(![[NSFileManager defaultManager] fileExistsAtPath:[wxDefaults valueForKey:@"WaveEditorPath"]])
	{
		[wxMAIN ShowMessage:@"Wave Editor Application not found!" 
			informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	[[NSWorkspace sharedWorkspace] launchApplication:[wxDefaults valueForKey:@"WaveEditorPath"]];
}

- (IBAction)wxCallCalculator:(id)sender
{
	if([[NSFileManager defaultManager] fileExistsAtPath:@"/Applications/Calculator.app"])
		[[NSWorkspace sharedWorkspace] launchApplication:@"/Applications/Calculator.app"];
	
}

- (IBAction)wxCallCommandLine:(id)sender
{

	if(![[NSFileManager defaultManager] fileExistsAtPath:@"/Applications/Utilities/Terminal.app"]) return;
	
	NSTask* task = [[NSTask alloc] init];
	[task setLaunchPath:@"/usr/bin/open"];
	[task setArguments:[NSArray arrayWithObjects:
						@"/Applications/Utilities/Terminal.app",
						nil]]; 
	[task launch];
	[task release];
}


- (IBAction)wxShowHelp:(id)sender
{
	if(HelpWindow != nil) 
	{
		[[HelpWindow windowController] setShouldCascadeWindows: NO];
		[HelpWindow setFrameAutosaveName:@"WindowHelpPositionAuto"];
		[HelpWindow makeKeyAndOrderFront:self];
	}
}
- (IBAction)goHome:(id)sender
{
	NSString* urlText = [wxDefaults valueForKey:@"CSoundHelpHtmlPath"];
	
	if(urlText != nil)
	{
		if([[NSFileManager defaultManager] fileExistsAtPath:urlText]) // && [urlText length] > 0)
		{
			[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:urlText]]];
		}
	}
}
- (void)webView:(WebView *)sender didFinishLoadForFrame:(WebFrame *)frame
{
	[buttonBack setEnabled:[sender canGoBack]];
    [buttonForward setEnabled:[sender canGoForward]];
}



- (IBAction)wxShowWinXoundHelp:(id)sender
{
	NSString* path = [[NSBundle mainBundle] pathForResource: @"WinXound Help" 
													 ofType: @"pdf" inDirectory: nil];
	
//	if(path != nil && [path length] > 0)
//		[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:path]]];
//	[self wxShowHelp:self];
	
	[[NSWorkspace sharedWorkspace] openFile:path];
}

- (IBAction)wxShowCSoundHelp:(id)sender
{
	[self goHome:self];
	[self wxShowHelp:self];
}

- (IBAction)wxShowCSoundFlagsHelp:(id)sender
{
	//Show the Opcodes list reference part of the manual
	NSString* path = [[wxDefaults valueForKey:@"CSoundHelpHtmlPath"] stringByDeletingLastPathComponent];
	NSString* mFile = [NSString stringWithFormat:@"%@/CommandFlags.html", path];
	if([[NSFileManager defaultManager] fileExistsAtPath:mFile])
	{
		[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:mFile]]];
		[self wxShowHelp:self];
	}
}

- (IBAction)wxShowCSoundOpcodesHelp:(id)sender
{
	//Show the Opcodes list reference part of the manual
	NSString* path = [[wxDefaults valueForKey:@"CSoundHelpHtmlPath"] stringByDeletingLastPathComponent];
	NSString* mFile = [NSString stringWithFormat:@"%@/PartReference.html", path];
	if([[NSFileManager defaultManager] fileExistsAtPath:mFile])
	{
		[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:mFile]]];
		[self wxShowHelp:self];
	}
}

- (void) showOpcodeHelp:(NSString*) opcode
{
	NSString* path = [[wxDefaults valueForKey:@"CSoundHelpHtmlPath"] stringByDeletingLastPathComponent];
	NSString* mFile = [NSString stringWithFormat:@"%@/%@.html", path, opcode];
	
	
	//Check if opcode exists
	if([Opcodes valueForKey:opcode] != nil)
	{
		if(path != @"")
		{			
			//Some words of the manual need a name switch
			//0dbfs
			if ([opcode isEqualToString:@"0dbfs"]) //gCurWord == "0dbfs")
			{
				mFile = [NSString stringWithFormat:@"%@/Zerodbfs.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\Zerodbfs.html";
			}
			
			//tb family
			if ([opcode hasPrefix:@"tb"]) //(gCurWord.StartsWith("tb") &&
				//char.IsDigit(gCurWord, 2))
			{
				mFile = [NSString stringWithFormat:@"%@/tb.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\tb.html";
			}
			//PyAssign family
			if ([opcode hasPrefix:@"pyassign"] ||  //(gCurWord.StartsWith("pyassign") ||
				[opcode hasPrefix:@"pylassign"])   //gCurWord.StartsWith("pylassign"))
			{
				mFile = [NSString stringWithFormat:@"%@/pyassign.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\pyassign.html";
			}
			//PyCall family
			if ([opcode hasPrefix:@"pycall"] ||   //(gCurWord.StartsWith("pycall") ||
				[opcode hasPrefix:@"pylcall"])    //gCurWord.StartsWith("pylcall"))
			{
				mFile = [NSString stringWithFormat:@"%@/pycall.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\pycall.html";
			}
			//PyEval family
			if ([opcode hasPrefix:@"pyeval"] ||    //(gCurWord.StartsWith("pyeval") ||
				[opcode hasPrefix:@"pyleval"])     //gCurWord.StartsWith("pyleval"))
			{
				mFile = [NSString stringWithFormat:@"%@/pyeval.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\pyeval.html";
			}
			//PyExec family
			if ([opcode hasPrefix:@"pyexec"] ||    //(gCurWord.StartsWith("pyexec") ||
				[opcode hasPrefix:@"pylexec"])     //gCurWord.StartsWith("pylexec"))
			{
				mFile = [NSString stringWithFormat:@"%@/pyexec.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\pyexec.html";
			}
			//Pyrun family
			if ([opcode hasPrefix:@"pyrun"] ||    //(gCurWord.StartsWith("pyrun") ||
				[opcode hasPrefix:@"pylrun"])     //gCurWord.StartsWith("pylrun"))
			{
				mFile = [NSString stringWithFormat:@"%@/pyrun.html", path];
				//Path.GetDirectoryName(wxGlobal.Settings.Directory.CSoundHelpHTML) +
				//"\\pyrun.html";
			}
			
		
			//Finally try to load the html file
			if([[NSFileManager defaultManager] fileExistsAtPath:mFile])
			{
				//HelpBrowser.Navigate(mFile);
				//MenuViewShowHelp_Click(null, null);
				[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:mFile]]];
				[self wxShowHelp:self];
			}
			else
			{
				//Display a "not found" error
				////HelpBrowser.Navigate(wxGlobal.Settings.Directory.CSoundHelpHTML);
				//HelpBrowser.Navigate(Application.StartupPath + "\\Help\\error.html");
				//MenuViewShowHelp_Click(null, null);
				NSString* path = [[NSBundle mainBundle] pathForResource: @"error" 
																 ofType: @"html" inDirectory: nil];	
				[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:path]]];
				[self wxShowHelp:self];
			}
		}
	}
	else
	{
		if(path != @"")
		{
			//If it is not an opcode we show the Opcodes list reference part of the manual
			mFile = [NSString stringWithFormat:@"%@/PartReference.html", path];
			if([[NSFileManager defaultManager] fileExistsAtPath:mFile])
			{
				[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:mFile]]];
				[self wxShowHelp:self];
			}
		}
	}
	
}

- (void) showHelpFor:(NSString*) fileHtml
{
	NSString* path = [[wxDefaults valueForKey:@"CSoundHelpHtmlPath"] stringByDeletingLastPathComponent];
	NSString* mFile = [NSString stringWithFormat:@"%@/%@.html", path, fileHtml];
	
	if([[NSFileManager defaultManager] fileExistsAtPath:mFile])
	{
		[[helpBrowser mainFrame] loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:mFile]]];
		[self wxShowHelp:self];
	}

}

- (IBAction)wxHelpShowTutorials:(id)sender
{
	//NSLog(@"SHOW TUTORIALS");
	[[NSWorkspace sharedWorkspace] openURL:[NSURL URLWithString:@"http://www.csounds.com/tutorials"]];	
}

- (IBAction)wxHelpShowFlossManual:(id)sender
{
	[[NSWorkspace sharedWorkspace] openURL:[NSURL URLWithString:@"http://booki.flossmanuals.net/csound/"]];	
}

- (IBAction)wxOpenAboutPanel:(id)sender
{	
	wxAbout* about = [[[wxAbout alloc] init] autorelease];
	[about showAboutWindow:self];
}

- (IBAction)wxShowCodeFormatOptions:(id)sender
{
	[[wxCodeFormatter sharedInstance] showOptionsWindow];
}


- (IBAction)wxWinXoundTest:(id)sender
{
	@try 
	{
		NSFileManager *fileManager = [NSFileManager defaultManager];
		
		
		NSString* input =[[NSBundle mainBundle] pathForResource:@"WinXound TEST"
														 ofType:@"csd"];
		
		NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDesktopDirectory, NSUserDomainMask, YES);
		NSString* desktopPath = [paths objectAtIndex:0];
		
		NSString *output = [NSString stringWithFormat:@"%@/WinXound TEST.csd",desktopPath];
		
		//Copy the file to the User Desktop Path and try to open it
		[fileManager copyItemAtPath:input toPath:output error:nil];
		
		if([fileManager fileExistsAtPath:output])
		{
			[self closeStartWindow];
			NSError* error = nil;
			
			[[NSDocumentController sharedDocumentController] openDocumentWithContentsOfURL:[NSURL fileURLWithPath:output] 
																				   display:YES 
																					 error:&error];
			
			if (error && [[error domain] isEqual: NSCocoaErrorDomain])
				NSLog(@"%@", error);
		}
		
	}
	@catch (NSException * e) 
	{
		[self ShowMessageError:@"wxMainController -> wxWinXoundTest:" 
						 error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
}

- (IBAction)wxHelpFindNext:(id)sender
{
	//NSLog(@"wxHelpFindNext");
	if([HelpWindow isVisible])
		[helpBrowser searchFor:[helpSearchField stringValue] direction:YES caseSensitive:false wrap:true];
}

- (IBAction)wxHelpFindPrevious:(id)sender
{
	//NSLog(@"wxHelpFindPrevious");
	if([HelpWindow isVisible])
		[helpBrowser searchFor:[helpSearchField stringValue] direction:NO caseSensitive:false wrap:true];
}

- (IBAction)wxHelpFindButtons:(id)sender
{
	@try
	{
		NSSegmentedControl* control = sender;
		NSInteger index = [control selectedSegment];

		switch (index) 
		{
			case 0:
				[self wxHelpFindPrevious:self];
				break;
			case 1:
				[self wxHelpFindNext:self];
				break;
		}
	
	}
	@catch (NSException * e) 
	{
		[self ShowMessageError:@"wxMainController -> wxHelpFindButtons:" 
						 error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
}

- (IBAction)wxFindAndReplace:(id)sender
{
	[HelpWindow makeFirstResponder:helpSearchField];
	[[helpSearchField cell] performClick:self];
}


- (IBAction)wxShowAnalysisTool:(id)sender
{
	[Analysis showAnalysisWindow];
}

- (IBAction)wxFindNext:(id)sender
{
	[self wxHelpFindNext:self];
}

- (IBAction)wxFindPrevious:(id)sender
{
	[self wxHelpFindPrevious:self];
}

- (IBAction)wxFindSetSelection:(id)sender
{
	//if([[[helpBrowser selectedDOMRange] markupString] length] > 0)
	//	[helpSearchField setStringValue:[[helpBrowser selectedDOMRange] markupString]];
	NSString *sel;
	sel = [helpBrowser stringByEvaluatingJavaScriptFromString:@"(function (){return window.getSelection().toString();})();"];
	if([sel length] > 0)
		[helpSearchField setStringValue:sel];
}

- (void) webView:(WebView *)sender didStartProvisionalLoadForFrame:(WebFrame *)frame
{	
	@try 
	{
		NSString* filename = [[[[frame provisionalDataSource] request] URL] path];
		//NSLog(@"request filename: %@", filename);
		if([[filename lowercaseString] hasSuffix:@".csd"] ||
		   [[filename lowercaseString] hasSuffix:@".orc"] ||
		   [[filename lowercaseString] hasSuffix:@".sco"])
			
		{
			if([[NSFileManager defaultManager] fileExistsAtPath:filename])
			{
				[self wxOpenDocumentWithFilename:filename];
				[frame stopLoading];
			}
		}
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxMainController -> didStartProvisionalLoadForFrame Error: %@ - %@", [e name], [e reason]);
	}	
}

//- (void) webView:(WebView *)sender didCommitLoadForFrame:(WebFrame *)frame
//{
//	//	//NSLog(@"request: %@", [[[frame dataSource] request] URL]);
//	//	NSString* filename = [[[[frame dataSource] request] URL] path];
//	//	NSLog(@"request filename: %@", filename);
//	//	if([filename hasSuffix:@".csd"])
//	//	{
//	//		[self wxOpenDocumentWithFilename:filename];
//	//	}
//}

//----------------------------------------------------------------------------------------------------------
// IBACTIONS
//----------------------------------------------------------------------------------------------------------













#pragma mark - TableVIEW Overrides
//----------------------------------------------------------------------------------------------------------
// TableVIEW OVERRIDES ADN IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------
- (int)numberOfRowsInTableView:(NSTableView *)tableView{
	return ([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count]);
}

//Implement the protocol method to retrieve the object value for a table column.
//- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(int)row
//{
//	NSString* path = [[[NSDocumentController sharedDocumentController] recentDocumentURLs] objectAtIndex:row];
//	NSString* fileName = [path lastPathComponent];  //[[path lastPathComponent] stringByDeletingPathExtension];
//	return fileName;
//}

//Implement the protocol method to retrieve the object value for a table column.
- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(int)row
{
	@try
	{
		if([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count] > 0)
		{
			NSURL* urlPath = [[[NSDocumentController sharedDocumentController] recentDocumentURLs] objectAtIndex:row];
			NSString* fileName = [urlPath path];
			return [fileName lastPathComponent];
		}
	}

	@catch (NSException * e) 
	{
		NSLog(@"wxMainController -> objectValueForTableColumn Error: %@ - %@", [e name], [e reason]);
	}
	
	return nil;
}
//----------------------------------------------------------------------------------------------------------
// TableVIEW OVERRIDES ADN IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------





#pragma mark - Cabbage tools
//----------------------------------------------------------------------------------------------------------
// CABBAGE IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------
//Cabbage Messages:
//CABBAGE_UPDATE
//CABBAGE_EXPORT_VSTI
//CABBAGE_EXPORT_VST
//CABBAGE_EXPORT_AU

- (bool) UpdateCabbage:(NSString*)filename
{
	
	//return [self SendCabbageMessage:[NSString stringWithFormat:@"CABBAGE_UPDATE| %@", filename]];
	return [self SendCabbageMessage:@"CABBAGE_UPDATE" withFilename:filename];
	
	NSLog(@"UpdateCabbage: %@", filename);
		
}
- (bool) CabbageExportVSTI:(NSString*)filename
{
	//return [self SendCabbageMessage:[NSString stringWithFormat:@"CABBAGE_EXPORT_VSTI| %@", filename]];
	return [self SendCabbageMessage:@"CABBAGE_EXPORT_VSTI" withFilename:filename];
}

- (bool) CabbageExportVST:(NSString*)filename
{
	//return [self SendCabbageMessage:[NSString stringWithFormat:@"CABBAGE_EXPORT_VST| %@", filename]];
	return [self SendCabbageMessage:@"CABBAGE_EXPORT_VST" withFilename:filename];
}

- (bool) CabbageExportAU:(NSString*)filename
{
	//return [self SendCabbageMessage:[NSString stringWithFormat:@"CABBAGE_EXPORT_AU| %@", filename]];
	return [self SendCabbageMessage:@"CABBAGE_EXPORT_AU" withFilename:filename];
}

- (bool) SendCabbageMessage:(NSString*)message withFilename:(NSString*)filename
{
		
	//if(mkfifo("/tmp/cabbage", 0666) == -1 ) return; //0644
	
	// Open and use the fifo as you would any file in Cocoa, but remember that it's a FIFO
	//NSFileHandle* fifoOUT = [NSFileHandle fileHandleForWritingAtPath:@"/tmp/cabbage_in"]; 
	//fileHandleForUpdatingAtPath //fileHandleForWritingAtPath
	
	//if (fifoOUT == nil && [[NSFileManager defaultManager] fileExistsAtPath:@"/tmp/cabbage_out"])
	
	
	if(![[NSFileManager defaultManager] fileExistsAtPath:@"/tmp/cabbage_out"])
	{
		NSLog(@"Cabbage_out NOT FOUND");
		
		//CabbagePath: start Cabbage application
		if([[NSFileManager defaultManager] fileExistsAtPath:[wxDefaults valueForKey:@"CabbagePath"]])
		{
			//Without passing filename as argument:
			//[[NSWorkspace sharedWorkspace] launchApplication:[wxDefaults valueForKey:@"CabbagePath"]];
			
			//Passing filename as argument:
			//System.Diagnostics.Process.Start("wmplayer", "\"" + mWaveFile + "\"");
			//@"/Applications/QuickTime Player.app"
			NSTask* itask = [[NSTask alloc] init];
			[itask setLaunchPath:@"/usr/bin/open"];
			//[itask setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
			[itask setArguments:[NSArray arrayWithObjects:
								 @"-a", 
								 [wxDefaults valueForKey:@"CabbagePath"],
								 filename,
								 nil]]; 
			
			[itask launch];
			[itask release];
			
			
			
			//Wait for some seconds to check if the connection is established
			//Look at "/tmp/cabbage_out"
			NSDate *previousDate = [NSDate date];
			
			while (true)
			{
				//System.Diagnostics.Debug.WriteLine("LOOP");
				NSLog(@"LOOP: %d", (int)[previousDate timeIntervalSinceNow]);
				
				//System.Threading.Thread.Sleep(250);
				[NSThread sleepForTimeInterval:0.250f];
				
				//if (mPipeClient.SendMessage("TEST")) break;
				if([[NSFileManager defaultManager] fileExistsAtPath:@"/tmp/cabbage_out"])
					break;
				
				if((int)[previousDate timeIntervalSinceNow] <= -3) break;
			}
		}
		else
		{
			[wxMAIN ShowMessage:@"Cabbage Application not found!" 
				informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
				  defaultButton:@"OK"
				alternateButton:nil 
					otherButton:nil];
			return false;
		}
	}
	
	//Open fifoOUT
	fifoOUT = [NSFileHandle fileHandleForWritingAtPath:@"/tmp/cabbage_in"];
	
	//Return Bad file descriptor if cabbage_in doesn't exist;
	//const char * path = "/tmp/cabbage_in";
	//int fd = open(path, O_WRONLY | O_NDELAY);  //O_RDWR
	//fifoOUT = [[NSFileHandle alloc] initWithFileDescriptor:fd closeOnDealloc: YES];
	
	
	
	if(fifoOUT != nil)
	{
		//Magic 1
		UInt32 i = 4071923244U;
		NSData * magic1 = [NSData dataWithBytes: &i length: sizeof(i)];
		
		//Magic 2
		NSString* str = [NSString stringWithFormat:@"%@| %@", message, filename]; //message; //@"test_string";
		UInt32 str_length = [str length];
		NSData* magic2 = [NSData dataWithBytes: &str_length length: sizeof(str_length)];
		
		//Data
		NSData* data = [str dataUsingEncoding:NSUTF8StringEncoding];
		
		
		NSMutableData* dataToWrite = [[NSMutableData alloc] initWithData:magic1]; //autorelease];
		[dataToWrite appendData:magic2];
		[dataToWrite appendData:data];
		
		[fifoOUT writeData:dataToWrite];
		
		[fifoOUT closeFile];
		[dataToWrite release];
		
		NSLog(@"Cabbage updated");
	}
	
	
	
	if(fifoIN == nil && [[NSFileManager defaultManager] fileExistsAtPath:@"/tmp/cabbage_out"])
	{
		//fifoIN = [[NSFileHandle fileHandleForReadingAtPath:@"/tmp/cabbage_out"] retain];
		const char * path = "/tmp/cabbage_out";
		int fd = open(path, O_RDONLY | O_NDELAY); //O_RDWR
		fifoIN = [[NSFileHandle alloc] initWithFileDescriptor:fd closeOnDealloc: YES]; //retain];
		
		[[NSNotificationCenter defaultCenter] removeObserver:self name:NSFileHandleReadCompletionNotification object:fifoIN];
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(dataReady:)
													 name:NSFileHandleReadCompletionNotification
												   object:fifoIN];
		[fifoIN readInBackgroundAndNotify];
		
		NSLog(@"fifoIN created");
	}
	
	return true;
}

- (void)dataReady:(NSNotification *)n
{
	NSData *d;
	d = [[n userInfo] valueForKey:NSFileHandleNotificationDataItem];
	
	if ([d length])
	{
		NSInteger length = [d length];
		char* buffer = new char[length - 8];
		
		[d getBytes:buffer range:NSMakeRange(8, length - 8)];
		NSString* data = [NSString stringWithUTF8String: buffer];
		
		NSLog(@"dataReady:%d bytes\ndata:%@",
			  [d length], 
			  data);
		
		//CHECK FOR MESSAGE CONTENT:
		//if (message.Contains("CABBAGE_FILE_UPDATED|"))
		if([data rangeOfString:@"CABBAGE_FILE_UPDATED|"].location != NSNotFound)
		{
			//string filename = message.Split("|".ToCharArray())[1].Trim();
			NSString* filename = [data substringFromIndex:22];
			NSLog(@"Cabbage filename received: %@", filename);
			//filename = [filename stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]];
			
			//if (File.Exists(filename))
			//if([[NSFileManager defaultManager] fileExistsAtPath:filename]) 
			{
				for (wxDocument* doc in [[NSDocumentController sharedDocumentController] documents])
				{
					NSLog(@"Document name: |%@| - |%@|", [[doc fileURL] path], filename);
					NSLog(@"Document length: %d - FileName length: %d", [[[doc fileURL] path] length], [filename length]);
						  
					//if([[[doc fileURL] path] isEqualToString:filename])
					if([filename rangeOfString:[[doc fileURL] path]].location != NSNotFound)
					{
						NSLog(@"--> Cabbage filename updated: %@", filename);
						[doc UpdateCurrentFileForCabbage];

						break;
					}
				}
			}
		}
		
		
		delete[] buffer;
	}
	
	[fifoIN readInBackgroundAndNotify];
}

//----------------------------------------------------------------------------------------------------------
// CABBAGE IMPLEMENTATION
//----------------------------------------------------------------------------------------------------------












//OLD Method to check recent files changes (delete)
//- (void) startTimer
//{
//	if(_Timer == nil)
//		_Timer = [NSTimer scheduledTimerWithTimeInterval:1 target:self selector:@selector(timerFireMethod:) userInfo:nil repeats:YES];
//}
//
//- (void) stopTimer
//{
//	if (_Timer != nil)
//	{
//		if([_Timer isValid]) 
//			[_Timer invalidate];
//		_Timer = nil;
//	}
//}
//
//- (void)timerFireMethod:(NSTimer*)theTimer
//{
//	if([StartWindow isVisible])
//	{
//		if([[[NSDocumentController sharedDocumentController] recentDocumentURLs] count] !=
//		   [RecentFiles numberOfRows])
//		{	
//			[RecentFiles reloadData];
//		}
//	}
//}















//----------------------------------------------------------------------------------------------------------
// REMMED STUFFS - USEFUL EXAMPLES
//----------------------------------------------------------------------------------------------------------
//ERRORS:
//NSError *theError = nil;
//BOOL success = [myDoc writeToURL:[self docURL] ofType:@"html" error:&theError];
//
//if (success == NO) {
//    // Maybe try to determine cause of error and recover first.
//    NSAlert *theAlert = [NSAlert alertWithError:theError];
//    [theAlert runModal]; // Ignore return value.
//}


//Hack to show all files in NSOpenPanel:
//	NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
//	NSDictionary *appDefaults = [NSDictionary dictionaryWithObject:
//								 @"YES" forKey: @"AppleShowAllFiles"];
//	[defaults registerDefaults:appDefaults];


//SAVE NSSTRING:
// NSString* newFilename =
// 	[NSString stringWithFormat:@"%@.csd",[[url path] stringByDeletingPathExtension]];
// //Save temp string to newFilename path
// [temp writeToFile:newFilename 
// 	   atomically:YES 
// 		 encoding:NSUTF8StringEncoding 
// 			error:nil];
// if([[NSFileManager defaultManager] fileExistsAtPath:newFilename])
// 	[self wxOpenDocumentWithFilename:newFilename];


//USING REGEX WITH NSSTRING:
//In your Cocoa application this test would look like this:
//
//NSString *mystring = @"Hello World!";
//NSString *regex = @".*l{2,}.*";
//
//NSPredicate *regextest = [NSPredicate
//						  predicateWithFormat:@"SELF MATCHES %@", regex];
//
//if ([regextest evaluateWithObject:mystring] == YES) {
//	NSLog(@"Match!");
//} else {
//	NSLog(@"No match!");
//}
//
// OR: one line solution
// Let's assume the NSString to be checked is stored in saveText
//if(
//   [[NSPredicate predicateWithFormat:@"SELF MATCHES %@", @".*[a-z]{2,}\\..*" ]
//	evaluateWithObject:saveText] )
//{
//	// do something
//}



@end
