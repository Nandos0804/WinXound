//
//  wxNode.m
//  WinXound
//
//  Created by Stefano Bonetti on 31/01/10.
//  Based on an example by Kevin Wojniak on 4/12/08.
//

#import "wxNode.h"


@implementation wxNode

@synthesize name, extendedname, children, isGroup;

- (id)init
{
	if (self = [super init])
	{
		self.name = nil;
		self.extendedname = nil;
		self.children = [NSMutableArray array];
		self.isGroup = NO;
	}
	
	return self;
}

- (void)dealloc
{
	//NSLog(@"wxNode DEALLOC");
	self.name = nil;
	self.extendedname = nil;
	self.children = nil;
	[super dealloc];
}


@end
