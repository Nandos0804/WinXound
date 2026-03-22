//
//  wxCSoundRepository.mm
//  WinXound
//
//  Created by Teto on 01/06/10.
//  Copyright 2010 __MyCompanyName__. All rights reserved.
//

#import "wxCSoundRepository.h"
#import "TextEditorView.h"
#import "wxGlobal.h"
#import "wxMainController.h"
#import "ScintillaView.h"
#import "wxNode.h"


@implementation wxCSoundRepository


static id sharedInstance = nil;



#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
+ (wxCSoundRepository*)sharedInstance
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
		
    }
	
	return sharedInstance;
}

- (void) dealloc
{
	[rootNode release];
	
	[super dealloc];
}

//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------











#pragma mark Various methods
//--------------------------------------------------------------------------------------------------
// VARIOUS
//--------------------------------------------------------------------------------------------------
- (void)setOwner:(TextEditorView*)editor
{
	textEditor = editor;
}

- (void)FillTreeViewOpcodes
{

	//Allocate Root Node
	rootNode = [[wxNode alloc] init];
		
	NSInteger index = -1;
	NSInteger indexInside = -1;
	NSString* itemText = nil;
	NSString* oldItemText = nil;
	NSString* oldItemTextInside = nil;
	
	
	//LOAD OPCODES DATABASE (DICTIONARY)
	NSError* error = nil;
	NSString* path = [[NSBundle mainBundle] pathForResource: @"opcodes" 
													 ofType: @"txt" inDirectory: nil];	
	NSString* opc = [NSString stringWithContentsOfFile: path
											  encoding: NSUTF8StringEncoding
												 error: &error];
	if (error && [[error domain] isEqual: NSCocoaErrorDomain])
		NSLog(@"%@", error);
	
	
	//NSArray* lines = [opc componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];
	NSArray* lines = [opc componentsSeparatedByString:@"\r\n"];
	
	
	
	//Fill Root Node
	for(NSString* mString in lines)
	{
		NSString* temp = [mString stringByReplacingOccurrencesOfString:@"\"" withString:@""];
		NSArray* split = [temp componentsSeparatedByString:@";"];
		
		if([split count] < 3) continue;
		
		
		//itemText = split[1].Split(":".ToCharArray())[0];
		itemText = [[[split objectAtIndex:1] componentsSeparatedByString:@":"] objectAtIndex:0];
		
		//if (itemText.ToLower() == "utilities") continue;
		if ([[itemText lowercaseString] isEqualToString:@"utilities"]) continue;
		if ([[itemText lowercaseString] isEqualToString:@"csoundav opcodes"]) continue;
		
		
		if (![oldItemText isEqualToString:itemText])
		{
			//Create and add new node
			wxNode* n = [[wxNode alloc] init];
			n.name = itemText;
			[rootNode.children addObject:n];
			index = [rootNode.children indexOfObjectIdenticalTo:n];
			[n release];
			oldItemText = itemText;
			indexInside = 0;
		}

		wxNode* parent = [rootNode.children objectAtIndex:index];
		
		
		//if (split[1].Split(":".ToCharArray()).Length > 1)
		//if([[[split objectAtIndex:1] componentsSeparatedByString:@":"] count] > 1)
		if([[split objectAtIndex:1] rangeOfString:@":"].location != NSNotFound)
		{
			//string itemTextInside = split[1].Split(":".ToCharArray())[1];
			NSString* itemTextInside = [[[split objectAtIndex:1] componentsSeparatedByString:@":"] objectAtIndex:1];
			
			if(![oldItemTextInside isEqualToString:itemTextInside])
			{				
				wxNode* ins = [[wxNode alloc] init];
				ins.name = itemTextInside;
				[parent.children addObject:ins];
				indexInside = [parent.children indexOfObjectIdenticalTo:ins];
				[ins release];
				oldItemTextInside = itemTextInside;
			}

			wxNode* inside = [parent.children objectAtIndex:indexInside];
			
			//treeViewOpcodes.Nodes[index].Nodes[i].Nodes.Add(split[0]);
			wxNode* t = [[wxNode alloc] init];
			t.name = [split objectAtIndex:0];
			[inside.children addObject:t];
			[t release];
			
			
		}
		else
		{
			//treeViewOpcodes.Nodes[index].Nodes.Add(split[0]);
			wxNode* x = [[wxNode alloc] init];
			x.name = [split objectAtIndex:0];
			[parent.children addObject:x];
			[x release];	
		}
		
	}
	
	[treeViewCSound setDataSource:self];
	[treeViewCSound setDelegate:self];
	
	[treeViewCSound reloadData];
	
}

- (void)displayCSoundRepository
{
	[self displayCSoundRepositoryAt:NSMakePoint(0, 0)];
}

- (void)displayCSoundRepositoryAt:(NSPoint)point
{
	if (CSoundRepositoryWindow == nil) {
		[NSBundle loadNibNamed:@"wxCSoundRepository" owner:self];
		[CSoundRepositoryWindow setShowsToolbarButton:NO];		
	}
	
	if(CSoundRepositoryWindow != nil)
	{
		//[self refreshList];
		
		//Set window position if specified
		if(point.x != 0 && point.y != 0)
		{
			[CSoundRepositoryWindow setFrameOrigin:point];
		}
		
		[CSoundRepositoryWindow makeKeyAndOrderFront:self];
		[CSoundRepositoryWindow makeFirstResponder:treeViewCSound];
		
	}
	
}




-(void) outlineViewSelectionDidChange:(NSNotification *)notification
{
	wxNode* item = [treeViewCSound itemAtRow:[treeViewCSound selectedRow]];
	if([item.children count] > 0) return;
	
	//NSLog(@"NAME: %@", item.name);
	
	NSString* description = nil;
	NSString* synopsis = nil;
	NSString* opcodeValue = [wxMAIN getOpcodeValue: item.name];
	if ([opcodeValue length] <= 0) return;
	
	NSArray* items = [opcodeValue componentsSeparatedByString:@";"];
	if(items == nil) return;
	if([items count] < 3) return;
 
	
	//Get Description
	description = [items objectAtIndex:1];
	//Get Synopsis
	synopsis = [items objectAtIndex:2];
	
	[labelDescription setStringValue: description];
	[labelSynopsis setStringValue: synopsis];
}




//Insert Opcode Text Action
- (IBAction) codeInsertText:(id)sender
{
	wxNode* item = [treeViewCSound itemAtRow:[treeViewCSound selectedRow]];
	if([item.children count] > 0) return;
	
	//Convert line endings before to insert text
	NSString* text = [textEditor convertEolOfText:item.name];
	
	//Check text
	if(text == nil) return;
	
	
	//Add text
	if([[textEditor getSelectedText] length] > 0)
	{
		[textEditor ReplaceText:text];
	}
	else
	{
		//[textEditor InsertText:[textEditor getCaretPosition] text:text];
		//[textEditor setCaretPosition:([textEditor getCaretPosition] + [text length])];
		[textEditor AddText:text];
	}
	
	[CSoundRepositoryWindow close];
}


//Insert Opcode Synopsis Action
- (IBAction) codeInsertSynopsis:(id)sender
{
	wxNode* item = [treeViewCSound itemAtRow:[treeViewCSound selectedRow]];
	if([item.children count] > 0) return;
	
	//Convert line endings before to insert text
	//NSString* text = [textEditor convertEolOfText:item.name];
	//[mString stringByReplacingOccurrencesOfString:@"\"" withString:@""];
	
	NSString* text = nil;
	NSString* opcodeValue = [wxMAIN getOpcodeValue: item.name];
	if ([opcodeValue length] <= 0) return;
	
	NSArray* items = [opcodeValue componentsSeparatedByString:@";"];
	if(items == nil) return;
	if([items count] < 3) return;
	
	//Get Synopsis
	if ([opcodeValue length] > 0) 
		text = [[items objectAtIndex:2] stringByReplacingOccurrencesOfString:@"\"" withString:@""];
	
	
	//Check text
	if(text == nil) return;
	if([[text lowercaseString] rangeOfString:@"not available"].location != NSNotFound) return;

	
	//Add text
	if([[textEditor getSelectedText] length] > 0)
	{
		[textEditor ReplaceText:text];
	}
	else
	{
		//[textEditor InsertText:[textEditor getCaretPosition] text:text];
		//[textEditor setCaretPosition:([textEditor getCaretPosition] + [text length])];
		[textEditor AddText:text];
	}
	
	[CSoundRepositoryWindow close];
}



- (void)refreshList
{
	if(CSoundRepositoryWindow != nil)
	{
		[treeViewCSound reloadData];
	}
}

//--------------------------------------------------------------------------------------------------
// VARIOUS
//--------------------------------------------------------------------------------------------------












#pragma mark - NSOutline Data Source
//----------------------------------------------------------------------------------------------------------
// NSOUTLINE OVERRIDES
//----------------------------------------------------------------------------------------------------------
- (NSInteger)outlineView:(NSOutlineView *)outlineView numberOfChildrenOfItem:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		return [node.children count];
	}
	
	return 0;
}

- (id)outlineView:(NSOutlineView *)outlineView child:(NSInteger)index ofItem:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		return [node.children objectAtIndex:index];
	}
	
	return nil;
}

- (BOOL)outlineView:(NSOutlineView *)outlineView isItemExpandable:(id)item
{
	wxNode *node = (wxNode*)item;//(item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		if(node.children != nil)
			return ([node.children count] > 0);
	}
	
	return false;
}

- (id)outlineView:(NSOutlineView *)outlineView objectValueForTableColumn:(NSTableColumn *)tableColumn byItem:(id)item
{
	wxNode *node = (item == nil ? rootNode : (wxNode*)item);
	if(node != nil)
	{
		return node.name;
	}
	
	return nil;
}

//- (void) outlineView:(NSOutlineView *)outlineView willDisplayCell:(id)cell forTableColumn:(NSTableColumn *)tableColumn item:(id)item
//{
//	wxNode *node = (wxNode*)item;	
//}


//- (NSCell *)outlineView:(NSOutlineView *)outlineView dataCellForTableColumn:(NSTableColumn *)tableColumn item:(id)item
//{
//	//NSCell* returnCell = [tableColumn dataCell];
//	wxNode *node = (wxNode*)item;
//	
//	if ([node.name isEqualToString:@"---"])
//	{
//		// we are being asked for the cell for the single and only column
//		//BaseNode* node = [item representedObject];
//		//if ([node nodeIcon] == nil && [[node nodeTitle] length] == 0)
//		return separatorCell;
//	}
//	
//	return [tableColumn dataCell];
//	
//}


//- (BOOL)outlineView:(NSOutlineView *)outlineView shouldSelectItem:(id)item
//{
//	wxNode *node = (wxNode*)item;
//	if([node.name isEqualToString:@"---"] ||
//	   [node.name isEqualToString:@"Bookmarks"])
//		return false;
//	
//	return true;
//}

//----------------------------------------------------------------------------------------------------------
// NSOUTLINE OVERRIDES
//----------------------------------------------------------------------------------------------------------





@end
