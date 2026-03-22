//
//  wxNode.h
//  WinXound
//
//  Created by Stefano Bonetti on 31/01/10.
//  Based on an example by Kevin Wojniak on 4/12/08.
//

#import <Cocoa/Cocoa.h>


@interface wxNode : NSObject {
	NSString *name;
	NSString *extendedname;
	NSMutableArray *children;
	BOOL isGroup;
}

@property (readwrite, retain) NSString *name;
@property (readwrite, retain) NSString *extendedname;
@property (readwrite, retain) NSMutableArray *children;
@property (readwrite) BOOL isGroup;

@end
