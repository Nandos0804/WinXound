//
//  wxPlayer.h
//  WinXound
//
//  Created by Stefano Bonetti on 19/01/10.
//
//

#import <Cocoa/Cocoa.h>


@interface wxPlayer : NSObject {

	IBOutlet NSWindow*		PlayerWindow; 
	IBOutlet NSButton*		ButtonPlay;
	IBOutlet NSButton*		ButtonStop;
	IBOutlet NSButton*		ButtonWaveEditor;
	IBOutlet NSSlider*		SliderPosition;
	IBOutlet NSTextField*   LabelTime;
	IBOutlet NSTextField*	LabelFileName;

	@private
	NSSound*	mPlayer;
	NSString*	mLastFileName;
	NSInteger	HH, MM, SS;
	NSTimer*	mTimer;
	
}

+ (wxPlayer*) sharedInstance;

- (IBAction)wxPlay:(id)sender;
- (IBAction)wxStop:(id)sender;
- (IBAction)wxCallWaveEditor:(id)sender;
- (IBAction)wxBrowse:(id)sender;
- (IBAction)wxExitPlayer:(id)sender;
- (IBAction)sliderMoved:(id)sender;

- (void) startTimer;
- (void) stopTimer;
- (void) timerFireMethod:(NSTimer*)theTimer;
- (bool) IsWindowVisible;


- (void) showPlayerWindow;
- (void) setFileName:(NSString*)filename;
- (NSString*) getFileName;
- (NSString*) DisplayTime:(NSInteger)seconds;


@end
