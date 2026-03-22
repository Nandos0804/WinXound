//
//  wxCompiler.m
//  WinXound
//
//  Created by Stefano Bonetti on 26/01/10.
//

#import "wxCompiler.h"
#import "wxGlobal.h"
#import "wxDocument.h"
#import "wxAnalysis.h"
#import "wxPlayer.h"



@implementation wxCompiler



#pragma mark Dealloc
//--------------------------------------------------------------------------------------------------
// DEALLOC - Release various stuffs
//--------------------------------------------------------------------------------------------------
- (void)dealloc
{
	if(task!=nil)
	{
		if([task isRunning])
		{
			//[[NSNotificationCenter defaultCenter] removeObserver:self 
			//												name:NSTaskDidTerminateNotification 
			//											  object:task];
			[self stopProcess];
		}    
	}
	
	
	//if(task != nil) [task release];
	[[NSNotificationCenter defaultCenter] removeObserver:self];
	
	//[_envVariables release];
    [super dealloc];
}








#pragma mark Compile actions
//--------------------------------------------------------------------------------------------------
// COMPILE
//--------------------------------------------------------------------------------------------------
- (void) compile: (NSString*) compilerName 
	  parameters: (NSString*) parameters 
	   filename1: (NSString*) filename1
	   filename2: (NSString*) filename2
		  output: (NSTextView*) output
		  button: (NSButton*) button
		   owner: (id) owner
{
	
	//if task is already running we must return
	if(task != nil)
	{
		if([task isRunning])
			return;
	}
	
	
	//Start Async Compiler task
	_output = output;
	_envVariables = [[NSMutableDictionary alloc] init];
	_fontName = [wxDefaults valueForKey:@"CompilerFontName"];
	_fontSize = [[wxDefaults valueForKey:@"CompilerFontSize"] integerValue];
	_font = [NSFont fontWithName:_fontName size:_fontSize];
	_button = button;
	_compilerName = compilerName;
	_owner = owner;
	_isSuspended = false;
	
	BOOL showArguments = false;
	
	
	if(task != nil)
	{
		[task release];
		task = nil;
	}
	
	task = [[NSTask alloc] init];
	
	
	
	//TASK SET ENVIRONMENT
	//If SFDIR is not defined or SFDIR checkbox is unchecked 
	//we redirect the compiled soundfile to the csd file directory
	//NSLog(@"UseSFDIR: %d", [[wxDefaults valueForKey:@"UseSFDIR"] boolValue]);
	if([[wxDefaults valueForKey:@"SFDIRPath"] length] == 0 ||
	   [[wxDefaults valueForKey:@"UseSFDIR"] boolValue] == false)
	{
		[_envVariables setValue:[filename1 stringByDeletingLastPathComponent] forKey:@"SFDIR"];
		//NSLog(@"SFDIR: %@", [filename1 stringByDeletingLastPathComponent]);
	}
	else
	{
		[_envVariables setValue:[wxDefaults valueForKey:@"SFDIRPath"] forKey:@"SFDIR"];
	}
	[_envVariables setValue:[wxDefaults valueForKey:@"SSDIRPath"] forKey:@"SSDIR"];
	[_envVariables setValue:[wxDefaults valueForKey:@"SADIRPath"] forKey:@"SADIR"];
	[_envVariables setValue:[wxDefaults valueForKey:@"MFDIRPath"] forKey:@"MFDIR"];
	[_envVariables setValue:[wxDefaults valueForKey:@"INCDIRPath"] forKey:@"INCDIR"];
	//OPCODEDIR and OPCODEDIR64
	if([[wxDefaults valueForKey:@"OPCODEDIRPath"] length] > 0)
	{
		////stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]
		[_envVariables setValue:[wxDefaults valueForKey:@"OPCODEDIRPath"] forKey:@"OPCODEDIR"];
		[_envVariables setValue:[wxDefaults valueForKey:@"OPCODEDIRPath"] forKey:@"OPCODEDIR64"];
	}
	[task setEnvironment:_envVariables];
	
	
	
	//TASK SET LAUNCH PATH
	//[task setLaunchPath: @"/usr/local/bin/csound"];
	[task setLaunchPath:compilerName];
	
	
	//TASK ARGUMENTS
	NSMutableArray *arguments = [[NSMutableArray alloc] init];
	
	
	//ONLY FOR CSOUND COMPILER !!!
	//DISABLE ANSI ESCAPE SEQUENCES FOR COLOR OUTPUT (Otherwise strange chars will appear)
	if([[_compilerName lowercaseString] rangeOfString:@"csound"].location != NSNotFound)
	{
		[arguments addObject:@"-+msg_color=false"]; 	
	}
	
	
	
	//PARAMETERS:
	parameters = [parameters stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	if([[wxDefaults valueForKey:@"UseWinxoundFlags"] boolValue] == true ||
	   [[_compilerName lowercaseString] rangeOfString:@"csound"].location == NSNotFound)
	{
		NSArray* parSep = [parameters componentsSeparatedByString:@" -"];
		
		if(parSep != nil)
		{
			if([parSep count] > 1) //Multiple flags
			{
				//NSInteger index = 0;
				for(NSString* s in parSep)
				{
					if([s length] > 0)
					{
						//if([_compilerName lowercaseString] rangeOfString:@"csound"].location == NSNotFound)
						//[arguments addObject:s];
						//if(index == 0)
						if([s isEqualToString:[parSep objectAtIndex:0]])
						{
							[arguments addObject:s];
						}
						else 
						{
							[arguments addObject:[NSString stringWithFormat:@"-%@",
												  [s stringByReplacingOccurrencesOfString:@"\"" withString:@""]]];
						}
	
						//index++;
					}
				}
			}
			else //Single flag
			{
				[arguments addObject:parameters];
			}
		}
		showArguments = true;
	}
	
	
	
	//Set arguments
	[arguments addObject:filename1];
	if(filename2 != nil)
		[arguments addObject:filename2];
	
	[task setArguments: arguments];
	
	
	//NSLog(@"Compiler arguments: %@ - Environment: %@", [task arguments], [task environment]);
	//DEBUG:
	for(NSString* s in arguments)
	{
		NSLog(@"Compiler arguments: %@", s);
	}
	
	
	
	//SET TASK PIPING
	//NSPipe *_pipe;
	//_pipe = [NSPipe pipe];
	//[task setStandardOutput: _pipe];
	[task setStandardOutput: [NSPipe pipe]];
	[task setStandardError: [task standardOutput]];
	
	
	//TASK OBSERVERS
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(getData:) 
												 name: NSFileHandleReadCompletionNotification 
											   object: [[task standardOutput] fileHandleForReading]];
	
	
	//[[NSNotificationCenter defaultCenter] addObserver:self 
	//										 selector:@selector(TaskCompletedNotification:) 
	//											 name:NSTaskDidTerminateNotification 
	//										   object:task];
	
	
	// We tell the file handle to go ahead and read in the background asynchronously, and notify
	// us via the callback registered above when we signed up as an observer.  The file handle will
	// send a NSFileHandleReadCompletionNotification when it has data that is available.
	[[[task standardOutput] fileHandleForReading] readInBackgroundAndNotify];
	
	
	
	if(_output != nil)
	{
		//We clear the output text only if the owner is not wxAnalysis
		if(![_owner isKindOfClass:[wxAnalysis class]])
			[_output setString:@""];
		
		if(showArguments && [arguments count] > 0)
		{
			//Print compiler arguments:
			NSInteger startIndex = 0;
			if([[_compilerName lowercaseString] rangeOfString:@"csound"].location != NSNotFound)
				startIndex = 1;
			
			[[[_output textStorage] mutableString] appendString:@"Compiler arguments (added by WinXound):\n"];
			for (NSInteger i = startIndex; i < [arguments count]; i++)
			{
				[[[_output textStorage] mutableString] appendString:[NSString stringWithFormat:@"%@ ",[arguments objectAtIndex:i]]];
			}
			[[[_output textStorage] mutableString] appendString:@"\n\n"];
		}
		
		//[_output setFont:[NSFont fontWithName:_fontName size:_fontSize]];
		[[[_output textStorage] mutableString] appendString:@"------- Compiler Started -------\n"];
		
		[_output setFont:[NSFont fontWithName:_fontName size:_fontSize]];
	}
	
	
	//TASK START
	// launch the task asynchronously
	[task launch]; 
	
	if(_buttonPause != nil)
	{
		[_buttonPause setTitle:@"Pause"];
	}
	
	[_envVariables release];
	[arguments release];
}

- (void) compile: (NSString*) compilerName 
	  parameters: (NSString*) parameters 
	   filename1: (NSString*) filename1
	   filename2: (NSString*) filename2
		  output: (NSTextView*) output
		  button: (NSButton*) button
	 buttonPause: (NSButton*) buttonPause
		   owner: (id) owner
{
	_buttonPause = buttonPause;
	
	[self compile:compilerName 
	   parameters:parameters 
		filename1:filename1 
		filename2:filename2 
		   output:output 
		   button:button 
			owner:owner];
	
}





// This method is called asynchronously when data is available from the task's file handle.
// We just pass the data along to the controller as an NSString.
- (void) getData: (NSNotification *)aNotification
{
    NSData *data = [[aNotification userInfo] objectForKey:NSFileHandleNotificationDataItem];
    // If the length of the data is zero, then the task is basically over - there is nothing
    // more to get from the handle so we may as well shut down.
	
	if(data == nil) // We're finished here
        [self TaskCompletedNotification:nil];
	
	if ([data length])
	{
		// Send the data on to the controller; we can't just use +stringWithUTF8String: here
		// because -[data bytes] is not necessarily a properly terminated string.
		// -initWithData:encoding: on the other hand checks -[data length]
		//[controller appendOutput: [[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease]];
		
		@try
		{
			if(_output != nil)
			{
				[_output setFont:[NSFont fontWithName:_fontName size:_fontSize]];
				
				[[[_output textStorage] mutableString] appendString:
				 [[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease]];
				
				//[textEditor AppendText:[[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease]];
				
				NSRange range;
				range = NSMakeRange ([[_output string] length], 0);
				[_output scrollRangeToVisible: range];
			}
		}
		@catch(id ue)
		{
			NSLog(@"wxCompiler::getData Error");
		}
		
		[[aNotification object] readInBackgroundAndNotify]; 
		
	} 
	else 
	{
        // We're finished here too
        [self TaskCompletedNotification:nil];
    }
    
    // we need to schedule the file handle go read more data in the background again.
    //[[aNotification object] readInBackgroundAndNotify];  
}


- (void)TaskCompletedNotification:(NSNotification *)aNotification
{
	if(task == nil) return;
	
	//[[NSNotificationCenter defaultCenter] removeObserver:self 
	//												name:NSTaskDidTerminateNotification 
	//											  object:task];
	
	[task waitUntilExit];
	[self TaskCompleted:false];
}

- (void) TaskCompleted:(BOOL)stoppedByUser
{
	[[NSNotificationCenter defaultCenter] removeObserver:self];
	
	@try
	{
		//NSLog(@"wxCompiler::TaskCompleted called");
		if(task != nil)
		{
			// Make sure the task has actually stopped!
			if(_isSuspended == true && stoppedByUser == true)
			{
				[task resume];
			}
			
			[task terminate];
			
			NSData *data;
			if(_output != nil && task != nil)
			{
				while ((data = [[[task standardOutput] fileHandleForReading] availableData]) && [data length]) 
				{
					NSString *string = [[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease];
					if (string != nil) 
					{
						//[asynchronousTaskResult appendString:string];
						[_output setFont:[NSFont fontWithName:_fontName size:_fontSize]];
						[[[_output textStorage] mutableString] appendString:string];
						
						//[[[_output textStorage] mutableString] appendString:
						//	[[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease]];
						//[textEditor AppendText:[[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] autorelease]];
						
						NSRange range;
						range = NSMakeRange ([[_output string] length], 0);
						[_output scrollRangeToVisible: range];
					}
				}
			}
		}
	}
	@catch(id ue)
	{
		NSLog(@"wxCompiler::TaskCompleted Error");
	}
	
	
	@try
	{
		if(task != nil)
		{
			[task release];
			task = nil;
		}
	}
	@catch(id ue){}
	
	
	if(_output != nil)
	{
		//[_output setFont:[NSFont fontWithName:@"Andale Mono" size:12]];
		[_output setFont:[NSFont fontWithName:_fontName size:_fontSize]];
		
		if(stoppedByUser)
			[[[_output textStorage] mutableString] appendString:@"------- Terminated by user -------\n"];
		
		[[[_output textStorage] mutableString] appendString:@"------- Compiler Ended -------"];
		
		NSRange range;
		range = NSMakeRange ([[_output string] length], 0);
		[_output scrollRangeToVisible: range];
	}
	
	
	if(_button != nil)
		[_button setHidden:true];
	if(_buttonPause != nil)
		[_buttonPause setHidden:true];
	
	
	[[NSNotificationCenter defaultCenter] removeObserver:self];
	
	
	if(_output != nil && _owner != nil)
	{
		//wxDocument: if _owner is wxDocument and the user hasn't stopped the compiler, 
		//we must CHECK FOR ERRORS or SOUNDFILES (call self compilerCompleted method)
		if([_owner isKindOfClass:[wxDocument class]])
		{
			if(stoppedByUser == false)
				[self compilerCompleted];
		}
		//wxAnalysis tool: call the wxAnalysis->compilerCompleted method
		else if([_owner isKindOfClass:[wxAnalysis class]])
			[_owner compilerCompleted];
	}
	
	
}




// If the task ends, there is no more data coming through the file handle even when the notification is
// sent, or the process object is released, then this method is called.
- (void) stopProcess
{
	[self TaskCompleted:true];
}

- (void) suspendProcess
{
	if(task!=nil)
	{
		if([task isRunning])
			_isSuspended = [task suspend];
		
		if(_isSuspended && _buttonPause != nil)
		{
			[_buttonPause setTitle:@"Resume"];
		}
	}
	else _isSuspended = false;
	
}

- (void) resumeProcess
{
	if(task!=nil)
	{
		[task resume];
	}
	
	_isSuspended = false;
	if(_buttonPause != nil)
	{
		[_buttonPause setTitle:@"Pause"];
	}
}

- (BOOL) processIsRunning
{
	if(task!=nil)
	{
		return [task isRunning];
	}
	return false;
}

- (BOOL) processIsSuspended
{
	if(task!=nil)
	{
		return _isSuspended;
	}
	return false;
}











#pragma mark Compiler Completed
//--------------------------------------------------------------------------------------------------
// COMPILER COMPLETED AND RELATED METHODS
//--------------------------------------------------------------------------------------------------
- (void)compilerCompleted
{
	@try
	{
		if([[_compilerName lowercaseString] rangeOfString:@"csound"].location != NSNotFound)
		{
			NSString* text = [_output string];
			NSString* errorLine = [self findError:text];
			NSString* soundLine = [self findSounds:text];
			
			if([errorLine length] > 0)
			{
				NSInteger errorLineNumber = [errorLine integerValue];
				//NSLog(@"ERROR LINE:%d", errorLineNumber);
				[_owner goToLine:errorLineNumber];
				return;
			}
			
			else if([soundLine length] > 0)
			{
				//NSLog(@"SOUND LINE:%@", soundLine);
				
				//if (!string.IsNullOrEmpty(mWaveFile) &&
				//	wxGlobal.Settings.General.OpenSoundFileWith > 0)
				if([[wxDefaults valueForKey:@"OpenSoundFileWith"] integerValue] > 0)
				{
					//0 = nothing - 1 = Media Player - 2 = Wave Editor
					if([[NSFileManager defaultManager] fileExistsAtPath:soundLine])
					{
						//MEDIA PLAYER = 1
						if([[wxDefaults valueForKey:@"OpenSoundFileWith"] integerValue] == 1)
						{
							if([[wxDefaults valueForKey:@"DefaultWavePlayer"] integerValue] == 1)
							{
								
								//System.Diagnostics.Process.Start("wmplayer", "\"" + mWaveFile + "\"");
								//@"/Applications/QuickTime Player.app"
								NSTask* itask = [[NSTask alloc] init];
								[itask setLaunchPath:@"/usr/bin/open"];
								//[itask setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
								[itask setArguments:[NSArray arrayWithObjects:
													 @"-a", 
													 @"/Applications/QuickTime Player.app",
													 soundLine,
													 nil]]; 
								
								[itask launch];
								[itask release];
							}
							else 
							{
								[wxPLAYER setFileName:soundLine];
								[wxPLAYER showPlayerWindow];
							}

						}
						//WAVE EDITOR = 2
						else
						{
							if([[NSFileManager defaultManager] fileExistsAtPath:[wxDefaults valueForKey:@"WaveEditorPath"]])
							{
								NSTask* itask = [[NSTask alloc] init];
								[itask setLaunchPath:@"/usr/bin/open"];
								//[itask setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
								[itask setArguments:[NSArray arrayWithObjects:
													 @"-a", 
													 [wxDefaults valueForKey:@"WaveEditorPath"],
													 soundLine,
													 nil]]; 
								
								[itask launch];
								[itask release];
							}
						}
					}
				}
			}
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxCompiler -> compilerCompleted Error: %@ - %@", [e name], [e reason]);
	}
}


//TRY TO FIND ERRORS INTO THE COMPILATION OUTPUT
- (NSString*)findError:(NSString*)text
{
	NSString* returnString = nil;
	NSInteger mStart = 0;
	
	@try
	{
		//NSString* StringToFind = @"error:";
		NSString* currentLine = nil;
		NSRange mFsFindPos;
		
		NSArray *lines = [text componentsSeparatedByString:@"\n"];
		
		//for(NSString* line in lines)
		for(NSInteger index = 0; index < [lines count]; index++)
		{
			currentLine = [lines objectAtIndex:index];
			
			mFsFindPos = [currentLine rangeOfString:@"error:" 
											options:NSLiteralSearch];
			
			
			if (mFsFindPos.location != NSNotFound)
			{			
				mFsFindPos = [currentLine rangeOfString:@"line"];
				if (mFsFindPos.location != NSNotFound)
				{
					if(returnString == nil)
					{
						returnString = [currentLine substringFromIndex:mFsFindPos.location + 4];
						returnString = [returnString stringByTrimmingCharactersInSet:
										[NSCharacterSet characterSetWithCharactersInString:@": "]];
						
						if(_output != nil)
						{
							[[[_output textStorage] mutableString] appendString:@"\n\n"];
							mStart = [[_output string] length];
							[[[_output textStorage] mutableString] appendString:@"Compiler errors:\n"];
						}
					}
					
					if(_output != nil)
					{
						[_output setFont:[NSFont fontWithName:_fontName size:_fontSize]];
						
						[[[_output textStorage] mutableString] appendString:@"--> "];
						[[[_output textStorage] mutableString] appendString:currentLine];
						[[[_output textStorage] mutableString] appendString:@"\n"];
						
						//NSRange range;
						//range = NSMakeRange ([[_output string] length], 0);
						//[_output scrollRangeToVisible: range];
					}
				}
			}
		}
		
	}
	@catch (NSException * e)
	{
		NSLog(@"wxCompiler -> findError Error: %@ - %@", [e name], [e reason]);
	}
	
	
	if(returnString != nil)
	{
		if(_output != nil && mStart > 0)
		{
			NSRange range = NSMakeRange (mStart, [[_output string] length] - mStart); 
			//NSRange range = NSMakeRange (mStart, 16); 
			[_output setTextColor:[NSColor colorWithCalibratedRed:0.7 green:0 blue:0 alpha:1] range:range];
			
			//[_output setSelectedRange:range];
			//[_output setNeedsDisplay:YES];
			
			range = NSMakeRange ([[_output string] length], 0);
			[_output scrollRangeToVisible: range];


		}
		
		return returnString;
	}
	else 
		return @"";
}


//TRY TO FIND SOUND FILES REFERENCE INTO THE COMPILATION OUTPUT
- (NSString*)findSounds:(NSString*)text
{
	
	@try
	{
		NSInteger mFindPos = -1;
		NSString* currentLine = nil;
		
		NSArray *lines = [text componentsSeparatedByString:@"\n"];
		
		
		//writing 2048-byte blks of shorts to /Users/teto/Desktop/fm_01.wav (WAV)
		//1034 2048-byte soundblks of shorts written to /Users/teto/Desktop/fm_01.wav (WAV)
		
		//for(NSString* line in lines)
		for(NSInteger index = 0; index < [lines count]; index++)
		{
			
			currentLine = [lines objectAtIndex:index];
			
			if([currentLine rangeOfString:@"written"].location == NSNotFound)
			{
				if([currentLine rangeOfString:@"writing"].location == NSNotFound)
					continue;
			}
			
			//SEARCH FOR: .wav or .aiff or .aif
			NSInteger extensionLength = 0;
			mFindPos = [currentLine rangeOfString:@".wav"].location;
			extensionLength = 4;
			
			if(mFindPos == NSNotFound)
			{
				mFindPos = [currentLine rangeOfString:@".aiff"].location;
				extensionLength = 5;
			}
			if(mFindPos == NSNotFound)
			{
				mFindPos = [currentLine rangeOfString:@".aif"].location;
				extensionLength = 4;
			}
			
			
			NSString* tempString = nil;
			
			if (mFindPos != NSNotFound)
			{
				NSInteger mFindStart = -1;
				mFindStart = [currentLine rangeOfString:@"written to"].location;
				if (mFindStart != NSNotFound)
				{
					
					//mLine = mLine.Remove(mFindPos + 1);
					NSString* mLine = [currentLine substringToIndex:mFindPos + extensionLength];
					//tempString = mLine.Substring(mFindStart + 11);
					tempString = [mLine substringFromIndex:mFindStart + 11]; //[NSString stringWithFormat:@"%@.wav", [mLine substringFromIndex:mFindStart + 11]];
					return tempString;
				}
				else
				{
					mFindStart = [currentLine rangeOfString:@"writing"].location;
					if(mFindStart != NSNotFound)
					{
						mFindStart = [currentLine rangeOfString:@"to"].location;
						if(mFindStart != NSNotFound)
						{
							NSString* mLine = [currentLine substringToIndex:mFindPos + extensionLength];
							tempString = [mLine substringFromIndex:mFindStart + 3];
							return tempString;
						}
					}
				}
			}		
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxCompiler -> findSounds Error: %@ - %@", [e name], [e reason]);
	}
	
	return @"";
	
}





@end
