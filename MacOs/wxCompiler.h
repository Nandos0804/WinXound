//
//  wxCompiler.h
//  WinXound
//
//  Created by Stefano Bonetti on 26/01/10.
//

#import <Cocoa/Cocoa.h>

@class wxDocument;

@interface wxCompiler : NSObject {
	
	NSTask*					task;
	
	NSMutableDictionary*	_envVariables;
	NSTextView*				_output;
	NSString*				_fontName;
	NSInteger				_fontSize;
	NSFont*					_font;
	NSButton*				_button;
	NSButton*				_buttonPause;
	id						_owner;
	NSString*				_compilerName;
	
	BOOL					_isSuspended;
}


- (void) compile: (NSString*) compilerName 
	  parameters: (NSString*) parameters 
	   filename1: (NSString*) filename1
	   filename2: (NSString*) filename2
		  output: (NSTextView*) output
		  button: (NSButton*) button
		   owner: (id) owner;

- (void) compile: (NSString*) compilerName 
	  parameters: (NSString*) parameters 
	   filename1: (NSString*) filename1
	   filename2: (NSString*) filename2
		  output: (NSTextView*) output
		  button: (NSButton*) button
	 buttonPause: (NSButton*) buttonPause
		   owner: (id) owner;

- (void) getData: (NSNotification *)aNotification;

- (void) stopProcess;

- (void) TaskCompleted:(BOOL)stoppedByUser;
- (void)TaskCompletedNotification:(NSNotification *)aNotification;
- (void)compilerCompleted;
- (void) suspendProcess;
- (void) resumeProcess;
- (BOOL) processIsRunning;
- (BOOL) processIsSuspended;

- (NSString*)findError:(NSString*)text;
- (NSString*)findSounds:(NSString*)text;

@end
