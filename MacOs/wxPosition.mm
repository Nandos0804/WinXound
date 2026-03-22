//
//  wxPosition.m
//  WinXound
//
//  Created by Stefano Bonetti on 16/02/10.
//

#import "wxPosition.h"


@implementation wxPosition


//- (id) initWithCapacity:(NSUInteger)numItems
//{
//	CAPACITY = numItems;
//	self = [super initWithCapacity:numItems];
//	return self;
//}

- (id) init
{
	self = [super init];
	if (self) 
	{
		CAPACITY = 20;
		mIndex = 0;
		oldPos = -1;
		mArray = [[NSMutableArray alloc] init];
	}
	return self;
}

- (void) dealloc
{
	[mArray release];
	[super dealloc];
}



- (void) StoreCursorPos:(NSInteger) position
{
	NSInteger newPos = position;
	
	if (newPos != oldPos)
	{
		[self AddValue:newPos];
		oldPos = newPos;
	}
}


- (NSInteger) PreviousPos
{
	if ([mArray count] == 0) return 0;
	
	@try
	{
		mIndex++;
		if (mIndex >= [mArray count]) mIndex = [mArray count] - 1;
		if (mIndex >= CAPACITY) mIndex = CAPACITY - 1;
		
		//NSLog(@"Index: %d", mIndex);
		return [[mArray objectAtIndex:mIndex]integerValue];
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxPosition -> PreviousPos Error: %@ - %@", [e name], [e reason]);
	}
	
	return 0;
}

- (NSInteger) NextPos
{
	if ([mArray count] == 0) return 0;
	
	@try
	{
		mIndex--;
		if (mIndex < 0) mIndex = 0;
		
		//NSLog(@"Index: %d", mIndex);
		return [[mArray objectAtIndex:mIndex]integerValue];
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxPosition -> NextPos Error: %@ - %@", [e name], [e reason]);
	}
	
	return 0;
}

- (void) AddValue:(NSInteger) value
{
	@try
	{
		if ([mArray count] <= CAPACITY)
		{
			[mArray insertObject:[NSNumber numberWithInteger:value] atIndex:0];
			mIndex = 0;
		}
		else
		{
			[mArray removeObjectAtIndex:CAPACITY];
			[mArray insertObject:[NSNumber numberWithInteger:value] atIndex:0];
			mIndex = 0;
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxPosition -> AddValue Error: %@ - %@", [e name], [e reason]);
	}
}

- (NSInteger) Position
{
	return mIndex;
}

- (void) ClearAll
{
	[mArray removeAllObjects];
	mIndex = 0;
	oldPos = -1;
}






@end
