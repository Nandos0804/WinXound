//
//  wxPlayer.m
//  WinXound
//
//  Created by Stefano Bonetti on 19/01/10.
//
//

#import "wxPlayer.h"
#import "wxGlobal.h"


@implementation wxPlayer

static id sharedInstance = nil;



#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
+ (wxPlayer*)sharedInstance
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
		
		mLastFileName = @"";
		HH = 0;
		MM = 0;
		SS = 0;
		
    }
	
	return sharedInstance;
}

- (void) dealloc
{	
	//Release objects
	if(mPlayer != nil) [mPlayer release];
	[super dealloc];
}


- (void) windowWillClose:(NSNotification *)notification
{
	//NSLog(@"Player Window - windowWillClose notification");
	
	if([mPlayer isPlaying]) 
		[self wxStop:self];
}

- (bool) IsWindowVisible
{
	if(PlayerWindow != nil)
	{
		return [PlayerWindow isVisible];
	}
	
	return false;
}



- (void) showPlayerWindow
{
	if (PlayerWindow == nil) 
	{
		[NSBundle loadNibNamed:@"wxPlayerWindow" owner:self];
		[PlayerWindow setShowsToolbarButton:NO];
	}
	
	
	//if (!string.IsNullOrEmpty(mLastFileName))
	if(mLastFileName != nil)
	{
		if(![mLastFileName isEqualToString:@""])
		{
			//bool ret = mPlayer.OpenFile(mLastFileName);
			mPlayer = [[NSSound alloc] initWithContentsOfFile: mLastFileName
												  byReference: NO];
			if (mPlayer != nil)
			{
				[mPlayer setDelegate:self];
				//labelFilename.Text = mLastFileName;
				[LabelFileName setStringValue:mLastFileName];
				//this.Text = mTitle + " [Total time: " + DisplayTime(mPlayer.Duration) + "]";
				[PlayerWindow setTitle:[NSString stringWithFormat:@"WinXound Player [%@]", 
										[self DisplayTime:[mPlayer duration]]]];
				//trackBarPosition.Maximum = mPlayer.Duration;
				[SliderPosition setMaxValue:[mPlayer duration]];
				[SliderPosition setIntegerValue:0];
				
				//buttonPlay.Text = "Play (P)";
				[ButtonPlay setTitle:@"Play (P)"];
				//buttonPlay.Enabled = true;
				[ButtonPlay setEnabled: true];
				//buttonStop.Enabled = false;
				[ButtonStop setEnabled:false];
				//trackBarPosition.Enabled = true;
				[SliderPosition setEnabled:true];
				//buttonWaveEditor.Enabled = true;
				[ButtonWaveEditor setEnabled:true];
				
				[PlayerWindow makeKeyAndOrderFront:self];
				return;
			}
		}
	}
	
	//trackBarPosition.Value = 0;
	[SliderPosition setIntegerValue:0];
	[SliderPosition setMaxValue:1.0];
	
	//labelFilename.Text = "";
	[LabelFileName setStringValue:@""];
	//trackBarPosition.Enabled = false;
	[SliderPosition setEnabled:false];
	//buttonPlay.Text = "Play (P)";
	[ButtonPlay setTitle:@"Play (P)"];
	//buttonPlay.Enabled = false;
	[ButtonPlay setEnabled:false];
	//buttonStop.Enabled = false;
	[ButtonStop setEnabled:false];
	//buttonWaveEditor.Enabled = false;
	[ButtonWaveEditor setEnabled:true];
	//this.Text = mTitle;
	
	[PlayerWindow setTitle:@"WinXound Player"];
	[PlayerWindow makeKeyAndOrderFront:self];
	
}

- (NSString*) DisplayTime:(NSInteger) seconds
{
	HH = seconds / 60 / 60;
	MM = seconds / 60;
	SS = seconds % 60;
	
	NSString* tempString = [NSString stringWithFormat:@"%02i:%02i:%02i",HH, MM, SS];
	
	return tempString;
}



- (IBAction)sliderMoved:(id)sender {
	
    SEL trackingEndedSelector = @selector(sliderEnded:);
    
	[NSObject cancelPreviousPerformRequestsWithTarget:self
											 selector:trackingEndedSelector object:sender];
	
    [self performSelector:trackingEndedSelector 
			   withObject:sender
			   afterDelay:0.0];
	
    // do whatever you want to do during tracking here
	//NSLog(@"%d", [sender integerValue]);
	
	[LabelTime setStringValue:[self DisplayTime:[sender integerValue]]];
	[mPlayer setCurrentTime:[sender integerValue]];
	
}

- (void)sliderEnded:(id)sender {
    // do whatever you want to do when tracking ends here
	NSLog(@"Done tracking");
	[LabelTime setStringValue:[self DisplayTime:[sender integerValue]]];
	[mPlayer setCurrentTime:[sender integerValue]];
}


//----------------------------------------------------------------------------------------------------------
// TIMER STUFFS
//----------------------------------------------------------------------------------------------------------
- (void) startTimer
{
	mTimer = [NSTimer scheduledTimerWithTimeInterval:0.25 target:self 
											selector:@selector(timerFireMethod:) 
											userInfo:nil 
											 repeats:YES];
}

- (void) stopTimer
{
	if (mTimer != nil)
	{
		if([mTimer isValid]) 
			[mTimer invalidate];
		mTimer = nil;
	}
}

- (void)timerFireMethod:(NSTimer*)theTimer
{
	//NSLog(@"wxPLAYER - TIMER TICK");
	//labelTime.Text = DisplayTime(mPlayer.Position);
	[LabelTime setStringValue:[self DisplayTime:[mPlayer currentTime]]];
	//trackBarPosition.Value = mPlayer.Position;
	[SliderPosition setIntegerValue:[mPlayer currentTime]];
	
}







- (IBAction)wxPlay:(id)sender
{
	
	bool ret = [mPlayer play];
	
	if(!ret && [[ButtonPlay title] isEqualToString:@"Pause (P)"]) //Player is playing
	{
		[mPlayer pause];
		
		//NSLog(@"wxPLAYER - PAUSE");
		[self stopTimer];
		
		[ButtonPlay setTitle:@"Play (P)"];
		[ButtonStop setEnabled:true];
		
		
	}
	else //Player is paused
	{
		//NSLog(@"wxPLAYER - PLAY/RESUME");
		//mPlayer.PlaySound();
		[mPlayer resume];
		
		[self startTimer];
		
		//buttonPlay.Text = "Pause (P)";
		[ButtonPlay setTitle:@"Pause (P)"];
		//buttonStop.Enabled = true;
		[ButtonStop setEnabled:true];
	}
	
	

	
}
- (IBAction)wxStop:(id)sender
{
	[self stopTimer];
	[mPlayer stop];
}

- (IBAction)wxCallWaveEditor:(id)sender
{
	if([mPlayer isPlaying]) 
		[self wxStop:self];
	
	if([[NSFileManager defaultManager] fileExistsAtPath:[wxDefaults valueForKey:@"WaveEditorPath"]])
	{
		NSTask* itask = [[NSTask alloc] init];
		[itask setLaunchPath:@"/usr/bin/open"];
		//[itask setCurrentDirectoryPath:@"/Users/Teto/Desktop"];
		[itask setArguments:[NSArray arrayWithObjects:
							 @"-a", 
							 [wxDefaults valueForKey:@"WaveEditorPath"],
							 mLastFileName,
							 nil]]; 
		
		[itask launch];
		[itask release];
	}
	
}

- (IBAction)wxBrowse:(id)sender
{
	if([mPlayer isPlaying]) 
		[self wxStop:self];
	
	int result;
	
    NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
    NSArray *filesToOpen;
    NSString *theFileName;
    //NSMutableArray *fileTypes = [NSSound soundUnfilteredFileTypes];
	NSArray* fileTypes = [NSSound soundUnfilteredTypes];
	
	// All file types NSSound understands
    [oPanel setAllowsMultipleSelection:NO];
	[oPanel setResolvesAliases:YES];	
    result = [oPanel runModalForDirectory:nil //applicationDirectory //NSHomeDirectory() 
									 file:nil
									types:fileTypes];
	
	
    if (result == NSOKButton) 
	{
		
        filesToOpen = [oPanel filenames];
		
        theFileName = [filesToOpen objectAtIndex:0];
		
        NSLog(@"Open Panel Returned: %@.\n", theFileName);
		
        mLastFileName = theFileName;
		
		[self showPlayerWindow];
		
    } 
	
}


- (IBAction)wxExitPlayer:(id)sender
{
	[mPlayer stop];
	[self stopTimer];
	
	[PlayerWindow close];
	
}

- (NSString*) getFileName
{
	return mLastFileName;
}
- (void) setFileName:(NSString*)filename
{
	mLastFileName = filename;
}


- (void) sound:(NSSound *)sound didFinishPlaying:(BOOL)aBool   //STOPPED
{
	//labelTime.Text = "00:00:00"; //DisplayTime(mPlayer.Position);
	[LabelTime setStringValue:@"00:00:00"];
	//trackBarPosition.Value = 0;
	[SliderPosition setIntegerValue:0];
	
	//buttonPlay.Text = "Play (P)";
	[ButtonPlay setTitle:@"Play (P)"];
	//buttonStop.Enabled = false;
	[ButtonStop setEnabled:false];
}



@end





