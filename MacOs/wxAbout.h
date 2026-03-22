//
//  wxAbout.h
//  WinXound
//
//  Created by Stefano Bonetti on 10/02/10.
//

#import <Cocoa/Cocoa.h>


@interface wxAbout : NSView {
	IBOutlet NSPanel* mainWindow;
	IBOutlet NSTextField* version;
}

- (void) showAboutWindow:(id)sender;
//- (id) hyperlinkFromString:(NSString*)inString withURL:(NSURL*)aURL;
- (id) infoValueForKey:(NSString*)key;

@end
