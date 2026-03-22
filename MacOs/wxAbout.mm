//
//  wxAbout.m
//  WinXound
//
//  Created by Stefano Bonetti on 10/02/10.
//

#import "wxAbout.h"


@implementation wxAbout


- (id) init
{
	self = [super init];
	return self;
}


- (void) showAboutWindow:(id)sender
{
	if (mainWindow == nil) {
		[NSBundle loadNibNamed:@"wxAbout" owner:self];
		[mainWindow setShowsToolbarButton:NO];
	}
	
	
	//SET WINDOW BACKGROUND
	[mainWindow setBackgroundColor:[NSColor colorWithPatternImage:[NSImage imageNamed:@"FormAboutBackgroundImage.jpg"]]];
	//[mainWindow makeKeyAndOrderFront:sender];
	
	
	//RETRIEVE AND DISPLAY VERSION NUMBER
	NSString* versionStr = [self infoValueForKey:@"CFBundleShortVersionString"];	
	[version setStringValue:[NSString stringWithFormat:@"version %@", versionStr]];
	
	
	//Finally display About window.
	[mainWindow makeKeyAndOrderFront:sender];

}

- (id) infoValueForKey:(NSString*)key
{
    if ([[[NSBundle mainBundle] localizedInfoDictionary] objectForKey:key])
        return [[[NSBundle mainBundle] localizedInfoDictionary] objectForKey:key];
	
    return [[[NSBundle mainBundle] infoDictionary] objectForKey:key];
}


//OLD
//- (id) hyperlinkFromString:(NSString*)inString withURL:(NSURL*)aURL
//{
//    NSMutableAttributedString* attrString = [[NSMutableAttributedString alloc] initWithString: inString];
//    NSRange range = NSMakeRange(0, [attrString length]);
//	
//    [attrString beginEditing];
//    [attrString addAttribute:NSLinkAttributeName value:[aURL absoluteString] range:range];
//	
//    // make the text appear in blue
//    [attrString addAttribute:NSForegroundColorAttributeName value:[NSColor blueColor] range:range];
//	
//    // next make the text appear with an underline
//    [attrString addAttribute:
//	 NSUnderlineStyleAttributeName value:[NSNumber numberWithInt:NSSingleUnderlineStyle] range:range];
//	
//    [attrString endEditing];
//	
//    return [attrString autorelease];
//}



@end
