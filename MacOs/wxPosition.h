//
//  wxPosition.h
//  WinXound
//
//  Created by Stefano Bonetti on 16/02/10.
//  This class stores information about text caret position (GO FORWARD and GO BACK features)
//

#import <Cocoa/Cocoa.h>


@interface wxPosition : NSObject {

	NSInteger		CAPACITY;
	NSInteger		mIndex;
	NSInteger		oldPos;
	NSMutableArray* mArray;
	
}

- (void) StoreCursorPos:(NSInteger) position;
- (NSInteger) PreviousPos;
- (NSInteger) NextPos;
- (void) AddValue:(NSInteger) value;
- (NSInteger) Position;
- (void) ClearAll;


@end
