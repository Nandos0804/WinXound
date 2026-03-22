//
//  TextEditorView.m
//  WinXound
//
//  Created by Stefano Bonetti on 20/01/10.
//

#import "TextEditorView.h"
#import "ScintillaView.h"



//--------------------------------------------------------------------------------------------------
//TextEditorView: the main text area view with splitted window and other useful stuffs
//--------------------------------------------------------------------------------------------------
@implementation TextEditorView



#pragma mark - Init and Overrides
//--------------------------------------------------------------------------------------------------
// INIT - OTHERRIDES - NOTIFICATIONS
//--------------------------------------------------------------------------------------------------
-(id) initWithFrame:(NSRect)frameRect
{
	NSLog(@"TextEditorView: initWithFrame CALLED.");
	
	if (!(self == [super initWithFrame:frameRect])) 
	{
		return nil;
    }
	
	
	SCI_WIDTH_MARGINS = 50;
	SCI_MEASURE_STRING = @"0000000";
	
	//NSRect newFrame = frameRect;
	//newFrame.size.width -= 2 * newFrame.origin.x;
	//newFrame.size.height -= 3 * newFrame.origin.y;
	//NSRect newFrame = NSMakeRect(0, 0, 100, 100);
	
	
	//Create two ScintillaView
	textView1 = [[ScintillaView alloc] initWithFrame:frameRect];
	textView2 = [[ScintillaView alloc] initWithFrame:frameRect];										   
	//textView1 = [[ScintillaView alloc] init];
	//textView2 = [[ScintillaView alloc] init];
	
	
	//Set Scintilla DocPointer for secondary view equal to first view
	[textView2 setGeneralProperty: SCI_SETDOCPOINTER 
						parameter:0 
							value:[textView1 getGeneralProperty:SCI_GETDOCPOINTER parameter:0]];
	
	//SET CODEPAGE TO UTF8
	[textView1 setGeneralProperty:SCI_SETCODEPAGE parameter:SC_CP_UTF8 value:0];
	[textView2 setGeneralProperty:SCI_SETCODEPAGE parameter:SC_CP_UTF8 value:0];
	
	
	_OldFocusedEditor = nil;
	_ShowMatchingBracket = false;
	

	//Add scintilla views to out NSSplitView class
	[self addSubview:textView1];
	[self addSubview:textView2];
	//[[[self subviews] objectAtIndex:0] setView:textView1];
	//[[[self subviews] objectAtIndex:1] setView:textView2];
	
	//Various autoresizing stuffs
	[self setAutoresizesSubviews: YES];
	[self setAutoresizingMask: NSViewWidthSizable | NSViewHeightSizable];
	[self adjustSubviews];
	

	//To hide a view with divider position
	//[self setPosition: frameRect.size.height ofDividerAtIndex:0];
	//[self setPosition:[self maxPossiblePositionOfDividerAtIndex: 0]  ofDividerAtIndex:0];
	
	//To remove and add a view from SplitView
	//[textView2 removeFromSuperview];
	//[[self subviews] removeObject:textView2];
	//[self addSubview:textView2];
	
	//[self setDividerStyle:NSSplitViewDividerStyleThin];
	


	//Various Settings:
	[self setTextEditorFont: [NSFont fontWithName:@"Andale Mono" size: 12]];
	
	//Assign margin size for Line Numbers and other margins
	[textView1 setGeneralProperty:SCI_MARKERDEFINE parameter:0 value:SC_MARK_ARROW]; //SC_MARK_BACKGROUND //SC_MARK_FULLRECT
	[textView1 setGeneralProperty: SCI_SETMARGINTYPEN parameter: 0 value: SC_MARGIN_NUMBER];
	[textView1 setGeneralProperty: SCI_SETMARGINTYPEN parameter: 1 value: SC_MARGIN_SYMBOL];
	[textView1 setGeneralProperty: SCI_SETMARGINTYPEN parameter: 2 value: 0];
	[textView1 setGeneralProperty: SCI_SETMARGINWIDTHN parameter: 0 value: SCI_WIDTH_MARGINS];
	[textView1 setGeneralProperty: SCI_SETMARGINWIDTHN parameter: 1 value: 10];
	[textView1 setGeneralProperty: SCI_SETMARGINWIDTHN parameter: 2 value: 0];
	
	[textView2 setGeneralProperty:SCI_MARKERDEFINE parameter:0 value:SC_MARK_ARROW]; //SC_MARK_BACKGROUND
	[textView2 setGeneralProperty: SCI_SETMARGINTYPEN parameter: 0 value: SC_MARGIN_NUMBER];
	[textView2 setGeneralProperty: SCI_SETMARGINTYPEN parameter: 1 value: SC_MARGIN_SYMBOL];
	[textView2 setGeneralProperty: SCI_SETMARGINTYPEN parameter: 2 value: 0];
	[textView2 setGeneralProperty: SCI_SETMARGINWIDTHN parameter: 0 value: SCI_WIDTH_MARGINS];
	[textView2 setGeneralProperty: SCI_SETMARGINWIDTHN parameter: 1 value: 10];
	[textView2 setGeneralProperty: SCI_SETMARGINWIDTHN parameter: 2 value: 0];
	[self setShowLineNumbers: true];

	
	//Attivare con la versione 2.02 assieme a SCI_SetAdditionalCaretsVisible = 0 (false)
	[textView1 setGeneralProperty: SCI_SETVIRTUALSPACEOPTIONS parameter: 1 value: 0]; 
	[textView2 setGeneralProperty: SCI_SETVIRTUALSPACEOPTIONS parameter: 1 value: 0]; 
	[textView1 setGeneralProperty: SCI_SETADDITIONALCARETSVISIBLE parameter: 0 value: 0]; 
	[textView2 setGeneralProperty: SCI_SETADDITIONALCARETSVISIBLE parameter: 0 value: 0];

	
	
	//Add events - notifications handling
//	[[NSNotificationCenter defaultCenter] addObserver:self 
//											 selector:@selector(SCI_UPDATEUI_Notification:) 
//												 name:SCIUpdateUINotification 
//											   object:nil];
//	[[NSNotificationCenter defaultCenter] addObserver:self 
//											 selector:@selector(SCI_MODIFIED_Notification:) 
//												 name:NSTextDidChangeNotification 
//											   object:textView1];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_ZOOM_Notification:) 
												 name:@"SCIZoomChanged" 
											   object:textView1];
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_ZOOM_Notification:) 
												 name:@"SCIZoomChanged" 
											   object:textView2];
	
	[[NSNotificationCenter defaultCenter] addObserver:self 
											 selector:@selector(SCI_MOD_CONTAINER_Notification:) 
												 name:@"SCIModContainer" 
											   object:textView1];
	
	
	
	[textView1 setGeneralProperty:SCI_USEPOPUP parameter:0 value:0];
	[textView2 setGeneralProperty:SCI_USEPOPUP parameter:0 value:0];
	

	
	[self RemoveSplit];
	[self setFocusOnPrimaryView];
	_OldFocusedEditor = textView1;
	

	
	
	//Finally return our initialized class object
	NSLog(@"TextEditorView initWithFrame: OK");	
	return self;
	
}


//- (void) sendNotification: (NSString*) notificationName
//{
//	NSNotificationCenter* center = [NSNotificationCenter defaultCenter];
//	[center postNotificationName: notificationName object: self];
//}

////EVENTS - NOTIFICATIONS
//- (void) SCI_UPDATEUI_Notification: (NSNotification *) notification
//{
//	//- CHECK FOR BRACKET
//	//- CURRENT WORD
//	//- DISPLAY INTELLITIP
//	
//	//NSLog(@"SCI_UPDATEUI_NOTIFICATION inside TextEditorView");
//	//[[NSNotificationCenter defaultCenter] postNotificationName:TextEditorUpdateUINotification object:self];
//}
//
//- (void) SCI_MODIFIED_Notification: (NSNotification *) notification
//{
//	//[textView1 setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
//	//[textView1 setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
//	//NSLog(@"SCI_MODIFIED_NOTIFICATION inside TextEditorView");
//	//[[NSNotificationCenter defaultCenter] postNotificationName:TextEditorModifiedNotification object:self];
//}

- (void) SCI_ZOOM_Notification: (NSNotification*) notification
{
	ScintillaView* sender = [notification object];
	NSInteger zoom = [sender getGeneralProperty:SCI_GETZOOM parameter:0];
	NSFont* font = [self getTextEditorFont];
	NSFont* tempFont = [NSFont fontWithName:[font familyName] size:[font pointSize] + zoom];
	
	SCI_WIDTH_MARGINS = [self measureTextWidth:SCI_MEASURE_STRING font:tempFont];
	
	//NSLog(@"%d",SCI_WIDTH_MARGINS);
	
	if([sender getGeneralProperty:SCI_GETMARGINWIDTHN parameter:0] > 0)
	{
		[sender setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:SCI_WIDTH_MARGINS];
	}
}

- (void) SCI_MOD_CONTAINER_Notification: (NSNotification*) notification
{

	//After Undo/Redo we must check for real line endings to synchronize
	//Scintilla Eol Mode
	
	NSInteger eolModeReal = [self GetEolModeReal];
	NSInteger eolModeScintilla = [self GetEOLMode];
	
	if (eolModeReal != eolModeScintilla)
	{
		[self setEolMode:eolModeReal];
	}  
	
	//NSLog(@"TextEditorView: SCI_MOD_CONTAINER - Real:%d Scintilla:%d", eolModeReal, eolModeScintilla);
	
}

//- (void)drawDividerInRect:(NSRect)aRect
//{	
//
//}

- (void) dealloc {

	NSLog(@"TextEditorView DEALLOC");
	
	//Remove events handling
	[[NSNotificationCenter defaultCenter] removeObserver:self]; //Remove ALL Previous Notifications registration !!!

	
	_OldFocusedEditor = nil;
	[SCI_MEASURE_STRING release];
	
	//Remove views
	[textView2 removeFromSuperview];
	[textView1 removeFromSuperview];
	
	
	//RELEASE TWO TIMES BECAUSE INNERVIEW RETAIN SCINTILLA (so one release for innerview and one for scintillaView)
	//NSLog(@"TextView retain count: %d", [textView2 retainCount]);
	NSLog(@"START DEALLOC - TextView retain count1: %d - retain count2: %d", [textView1 retainCount], [textView2 retainCount]);
	[textView2 release];
	[textView2 release];
	textView2 = nil;
	
	[textView1 release];
	[textView1 release];
	textView1 = nil;
	
	[super dealloc];
	
	NSLog(@"END DEALLOC - TextView retain count1: %d - retain count2: %d", [textView1 retainCount], [textView2 retainCount]);
	
}





#pragma mark METHODS
//--------------------------------------------------------------------------------------------------
//TextEditorView: METHODS
//--------------------------------------------------------------------------------------------------
- (NSString*) getFontName:(NSInteger)styleNumber
{
	
	//NSLog(@"getFontName called");
	NSString* result = nil;
	const char* buffer = new char[256];
	
	////[self backend]->WndProc(SCI_STYLEGETFONT, styleNumber, (sptr_t)buffer);			
	[[self getFocusedEditor] setReferenceProperty:SCI_STYLEGETFONT parameter:styleNumber value:buffer];
	result = [NSString stringWithUTF8String: buffer];
	
	delete[] buffer;   //TO PREVENT MEMORY LEAKS
	
	return result;
	
}

//SCI_TEXTWIDTH(int styleNumber, const char *text)
- (NSInteger) measureTextWidth:(NSString*) text 
						  font:(NSFont*) font
{
	//return (NSInteger)[self backend]->WndProc(SCI_TEXTWIDTH, STYLE_DEFAULT, (sptr_t)text);	
	CGSize textSize = 
	NSSizeToCGSize([text sizeWithAttributes:[NSDictionary dictionaryWithObject:font forKey: NSFontAttributeName]]);	
	return (NSInteger)textSize.width;
}

- (void) setSearchFlags: (NSInteger) searchflags
{
	[textView1 setGeneralProperty:SCI_SETSEARCHFLAGS parameter:searchflags value:0];
	[textView2 setGeneralProperty:SCI_SETSEARCHFLAGS parameter:searchflags value:0];
}

- (void) setTargetStart:(NSInteger) mStart
{
	[textView1 setGeneralProperty:SCI_SETTARGETSTART parameter:mStart value:0];
	[textView2 setGeneralProperty:SCI_SETTARGETSTART parameter:mStart value:0];
}

- (void) setTargetEnd:(NSInteger) mEnd
{
	[textView1 setGeneralProperty:SCI_SETTARGETEND parameter:mEnd value:0];
	[textView2 setGeneralProperty:SCI_SETTARGETEND parameter:mEnd value:0];
}

- (NSInteger) searchInTarget:(NSString*) text
{
	const char* rawValue = [text UTF8String];
	return [textView1 backend]->WndProc(SCI_SEARCHINTARGET, [text length], (sptr_t)rawValue);
	
	//NSInteger ret = [textView1 backend]->WndProc(SCI_SEARCHINTARGET, [text length], (sptr_t)rawValue);
	//delete[] rawValue;
	//return ret;
}

- (void) setColorProperty: (int) property fromHTML: (NSString*) fromHTML
{
	if ([fromHTML length] > 3 && [fromHTML characterAtIndex: 0] == '#')
	{
		bool longVersion = [fromHTML length] > 6;
		int index = 1;
		
		char value[3] = {0, 0, 0};
		value[0] = [fromHTML characterAtIndex: index++];
		if (longVersion)
			value[1] = [fromHTML characterAtIndex: index++];
		else
			value[1] = value[0];
		
		unsigned rawRed;
		[[NSScanner scannerWithString: [NSString stringWithUTF8String: value]] scanHexInt: &rawRed];
		
		value[0] = [fromHTML characterAtIndex: index++];
		if (longVersion)
			value[1] = [fromHTML characterAtIndex: index++];
		else
			value[1] = value[0];
		
		unsigned rawGreen;
		[[NSScanner scannerWithString: [NSString stringWithUTF8String: value]] scanHexInt: &rawGreen];
		
		value[0] = [fromHTML characterAtIndex: index++];
		if (longVersion)
			value[1] = [fromHTML characterAtIndex: index++];
		else
			value[1] = value[0];
		
		unsigned rawBlue;
		[[NSScanner scannerWithString: [NSString stringWithUTF8String: value]] scanHexInt: &rawBlue];
		
		long color = (rawBlue << 16) + (rawGreen << 8) + rawRed;
		[textView1 backend]->WndProc(property, color, 0);
		[textView2 backend]->WndProc(property, color, 0);
	}
}



//SCI_SETSCROLLWIDTH(int pixelWidth)
- (void) setScrollWidth: (NSInteger) pixelWidth
{
	[textView1 setGeneralProperty: SCI_SETSCROLLWIDTH parameter: pixelWidth value: 0];
	[textView2 setGeneralProperty: SCI_SETSCROLLWIDTH parameter: pixelWidth value: 0];

}
//SCI_GETSCROLLWIDTH
- (NSInteger) getScrollWidth
{
	return [textView1 getGeneralProperty:SCI_GETSCROLLWIDTH parameter:0];
}

//SCI_SETSCROLLWIDTHTRACKING(bool tracking)

//SCI_GETSCROLLWIDTHTRACKING
- (void) setScrollWidthTracking:(BOOL) tracking
{
	[textView1 setGeneralProperty: SCI_SETSCROLLWIDTHTRACKING parameter: tracking value: 0];
	[textView2 setGeneralProperty: SCI_SETSCROLLWIDTHTRACKING parameter: tracking value: 0];
}



- (void) setContextMenu:(NSMenu*) menu
{
	[[textView1 content] setMenu:menu];
	[[textView2 content] setMenu:menu];
}

- (void) setCaretFore: (NSString*) htmlColor
{
	[self setColorProperty:SCI_SETCARETFORE fromHTML:htmlColor];
	//[textView2 setColorProperty:SCI_SETCARETFORE fromHTML:htmlColor];
}
- (void) setCaretLineBack: (NSString*) htmlColor
{
	[self setColorProperty:SCI_SETCARETLINEBACK fromHTML:htmlColor];
	//[textView2 setColorProperty:SCI_SETCARETLINEBACK fromHTML:htmlColor];
}

- (void) setEdgeColor: (NSString*) htmlColor
{
	[self setColorProperty:SCI_SETEDGECOLOUR fromHTML:htmlColor];
	//[textView2 setColorProperty:SCI_SETEDGECOLOUR fromHTML:htmlColor];
}

- (void) setSelFore:(BOOL)useSel htmlcolor: (NSString*) htmlColor
{
	[textView1 setColorProperty: SCI_SETSELFORE parameter: useSel fromHTML: htmlColor];
	[textView2 setColorProperty: SCI_SETSELFORE parameter: useSel fromHTML: htmlColor];
}
- (void) setSelBack:(BOOL)useSel htmlcolor: (NSString*) htmlColor
{
	[textView1 setColorProperty: SCI_SETSELBACK parameter: useSel fromHTML: htmlColor];
	[textView2 setColorProperty: SCI_SETSELBACK parameter: useSel fromHTML: htmlColor];
}

- (void) StyleSetFore: (NSInteger) index htmlcolor: (NSString*) htmlColor
{
	[textView1 setColorProperty: SCI_STYLESETFORE parameter: index fromHTML: htmlColor];
	[textView2 setColorProperty: SCI_STYLESETFORE parameter: index fromHTML: htmlColor];
}
- (void) StyleSetBack: (NSInteger) index htmlcolor: (NSString*) htmlColor
{
	[textView1 setColorProperty: SCI_STYLESETBACK parameter: index fromHTML: htmlColor];
	[textView2 setColorProperty: SCI_STYLESETBACK parameter: index fromHTML: htmlColor];
}
- (void) StyleSetBold: (NSInteger) index bold: (BOOL) bold
{
	[textView1 setGeneralProperty: SCI_STYLESETBOLD parameter: index value: bold];
	[textView1 setGeneralProperty: SCI_STYLESETBOLD parameter: index value: bold];
}
- (void) StyleSetItalic: (NSInteger) index italic: (BOOL) italic
{
	[textView1 setGeneralProperty: SCI_STYLESETITALIC parameter: index value: italic];
	[textView1 setGeneralProperty: SCI_STYLESETITALIC parameter: index value: italic];
}


- (NSFont*) getTextEditorFont	//OK
{	
	NSString* fontname = [[NSString alloc] init];
	fontname = [self getFontName:STYLE_DEFAULT];
	
	NSInteger fontsize = 0;
	fontsize = (NSInteger)[textView1 getGeneralProperty:SCI_STYLEGETSIZE parameter:STYLE_DEFAULT extra:0];
	
	//NSLog(@"fontname = %@ - fontsize = %d", fontname, fontsize);
	return [NSFont fontWithName:fontname size:fontsize];
	
}

- (void) setTextEditorFont: (NSFont*) newfont	//OK
{
	for (int i = 0; i < 34; i++)
	{
		[textView1 setStringProperty:SCI_STYLESETFONT parameter:i value:[newfont displayName]];
		[textView2 setStringProperty:SCI_STYLESETFONT parameter:i value:[newfont displayName]];
		[textView1 setGeneralProperty:SCI_STYLESETSIZE parameter:i value:[newfont pointSize]];
		[textView2 setGeneralProperty:SCI_STYLESETSIZE parameter:i value:[newfont pointSize]];
	}
	
	
	//MEASURE TEXT WIDTH AND SET MARGINS (FOR LINE NUMBERS)
	//NSFont* font = [self getTextEditorFont];
	NSFont* tempFont = [NSFont fontWithName:[newfont familyName] size:[newfont pointSize] + [self getZoom]];
	SCI_WIDTH_MARGINS = [self measureTextWidth:SCI_MEASURE_STRING font:tempFont];
	
	if([textView1 getGeneralProperty:SCI_GETMARGINWIDTHN parameter:0] > 0)
	{
		[textView1 setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:SCI_WIDTH_MARGINS];
		[textView2 setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:SCI_WIDTH_MARGINS];

	}
}

- (BOOL) getShowSpaces //OK
{
	//return Perform(mDirectPointer, SciConst.SCI_GETVIEWWS, 0, 0);
	return (BOOL)[textView1 getGeneralProperty:SCI_GETVIEWWS parameter:0];
}
- (void) setShowSpaces:(BOOL) val //OK
{
	//Perform(mDirectPointer, SciConst.SCI_SETVIEWWS, wsMode, 0);
	if (val == true)
	{
		[textView1 setGeneralProperty:SCI_SETVIEWWS parameter:SCWS_VISIBLEALWAYS value:0];
		[textView2 setGeneralProperty:SCI_SETVIEWWS parameter:SCWS_VISIBLEALWAYS value:0];
	}
	else
	{
		[textView1 setGeneralProperty:SCI_SETVIEWWS parameter:0 value:0];
		[textView2 setGeneralProperty:SCI_SETVIEWWS parameter:0 value:0];
	}
}

- (BOOL) getShowEOLMarker //OK
{
	return [textView1 getGeneralProperty:SCI_GETVIEWEOL parameter:0];
}
- (void) setShowEOLMarker:(BOOL) val //OK
{
	//Perform(mDirectPointer, SciConst.SCI_SETVIEWEOL, (Int32)(visible ? 1 : 0), 0);
	[textView1 setGeneralProperty:SCI_SETVIEWEOL parameter:(val ? 1 : 0) value:0];
	[textView2 setGeneralProperty:SCI_SETVIEWEOL parameter:(val ? 1 : 0) value:0];
}

- (BOOL) getEndAtLastLine //OK
{
	//return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETENDATLASTLINE, 0, 0));
	return [textView1 getGeneralProperty:SCI_GETENDATLASTLINE parameter:0];
}
- (void) setEndAtLastLine:(BOOL) val //OK
{
	//Perform(mDirectPointer, SciConst.SCI_SETENDATLASTLINE, Convert.ToInt32(endAtLastLine), 0);
	[textView1 setGeneralProperty:SCI_SETENDATLASTLINE parameter:(val ? 1 : 0) value:0];
	[textView2 setGeneralProperty:SCI_SETENDATLASTLINE parameter:(val ? 1 : 0) value:0];
}

- (BOOL) getShowMatchingBracket
{
	return _ShowMatchingBracket;
}

- (void) setShowMatchingBracket:(BOOL) val
{
	_ShowMatchingBracket = val;
	[self BraceHiglight:-1 pos2:-1];
	
}


- (BOOL) getCanUndo	//OK
{
	//return Perform(mDirectPointer, SciConst.SCI_CANUNDO, 0, 0);
	return [textView1 getGeneralProperty:SCI_CANUNDO parameter:0];
}

- (BOOL) getCanRedo	//OK
{
	//return Perform(mDirectPointer, SciConst.SCI_CANREDO, 0, 0);
	return [textView1 getGeneralProperty:SCI_CANREDO parameter:0];
}


- (NSInteger) getTabIndent	//OK
{
	//return Perform(mDirectPointer, SciConst.SCI_GETTABWIDTH, 0, 0);
	return [textView1 getGeneralProperty:SCI_GETTABWIDTH parameter:0];
}

- (void) setTabIndent:(NSInteger) val	//OK
{
	//Perform(mDirectPointer, SciConst.SCI_SETTABWIDTH, widthInChars, 0);
	[textView1 setGeneralProperty:SCI_SETTABWIDTH parameter:val value:0];
	[textView2 setGeneralProperty:SCI_SETTABWIDTH parameter:val value:0];
}


- (BOOL) getShowLineNumbers	//OK
{
	//return (textView1.GetMarginWidthN(0) > 0 ? true : false);
	return ([textView1 getGeneralProperty:SCI_GETMARGINWIDTHN parameter:0] > 0);
}

- (void) setShowLineNumbers:(BOOL) val	//OK
{
	//SCI_WIDTH_MARGINS = [textView1 measureTextWidth:SCI_MEASURE_STRING font:[self getTextEditorFont]];
	//MEASURE TEXT WIDTH AND SET MARGINS (FOR LINE NUMBERS)
	NSFont* font = [self getTextEditorFont];
	NSFont* tempFont = [NSFont fontWithName:[font familyName] size:[font pointSize] + [self getZoom]];
	SCI_WIDTH_MARGINS = [self measureTextWidth:SCI_MEASURE_STRING font:tempFont];
	
	if(val == true)
	{
		[textView1 setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:SCI_WIDTH_MARGINS];
		[textView2 setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:SCI_WIDTH_MARGINS];
	}
	else 
	{
		[textView1 setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:0];
		[textView2 setGeneralProperty:SCI_SETMARGINWIDTHN parameter:0 value:0];
	}
}


- (BOOL) getMarkCaretLine	//OK
{
	//return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETCARETLINEVISIBLE, 0, 0));
	return [textView1 getGeneralProperty:SCI_GETCARETLINEVISIBLE parameter:0];
}

- (void) setMarkCaretLine:(BOOL) val	//OK
{
	// Perform(mDirectPointer, SciConst.SCI_SETCARETLINEVISIBLE, Convert.ToInt32(show), 0);
	[textView1 setGeneralProperty:SCI_SETCARETLINEVISIBLE parameter:(val ? 1 : 0) value:0];
	[textView2 setGeneralProperty:SCI_SETCARETLINEVISIBLE parameter:(val ? 1 : 0) value:0];
}


- (BOOL) getShowVerticalRuler	//OK
{
	//return Perform(mDirectPointer, SciConst.SCI_GETEDGEMODE, 0, 0);
	return [textView1 getGeneralProperty:SCI_GETEDGEMODE parameter:0];
}

- (void) setShowVerticalRuler: (BOOL) val	//OK
{
	if (val == true)
	{
		//wxGlobal.Preferences.Syntax.VerticalRulerColor
		//Perform(mDirectPointer, SciConst.SCI_SETEDGECOLUMN, 80, 0);
		[textView1 setGeneralProperty:SCI_SETEDGECOLUMN parameter:80 value:0];
		[textView2 setGeneralProperty:SCI_SETEDGECOLUMN parameter:80 value:0];
		[textView1 setGeneralProperty:SCI_SETEDGEMODE parameter:1 value:0];
		[textView2 setGeneralProperty:SCI_SETEDGEMODE parameter:1 value:0];
	}
	else
	{
		[textView1 setGeneralProperty:SCI_SETEDGEMODE parameter:0 value:0];
		[textView2 setGeneralProperty:SCI_SETEDGEMODE parameter:0 value:0];
	}
}


- (BOOL) getReadOnly	//OK
{
	//return Convert.ToBoolean(Perform(mDirectPointer, SciConst.SCI_GETREADONLY, 0, 0));
	return [textView1 getGeneralProperty:SCI_GETREADONLY parameter:0];
}

- (void) setReadOnly:(BOOL) val	//OK
{
	//[textView1 setEditable:val];
	//[textView2 setEditable:val];
	[textView1 setGeneralProperty:SCI_SETREADONLY parameter:val ? 1 : 0 value:0];
	[textView2 setGeneralProperty:SCI_SETREADONLY parameter:val ? 1 : 0 value:0];
}


- (ScintillaView*) getPrimaryView
{
	return textView1;
}
- (ScintillaView*) getSecondaryView
{
	return textView2;
}



- (NSString*) getText	//OK
{
	return [textView1 string];
}

- (void) setText: (NSString*) val	//OK
{
	[textView1 setString: val];
}


//TODO: for color printing feature
//-(PrintDocument) getPrintDocument;



- (BOOL) getIsSplitted	//OK
{
	return ([[self subviews] count] > 1);
}

- (void) Split
{
	[self RemoveSplit];

	@try 
	{
		NSInteger splitPos = [textView1 frame].size.height / 2;
		[self addSubview:textView2];
		[self setVertical:NO];
		[self setPosition:splitPos ofDividerAtIndex:0];
		[self adjustSubviews];
		[self setPosition:splitPos ofDividerAtIndex:0];
		
		
		[[textView2 content] resignFirstResponder];
		[[self window] makeFirstResponder:nil];
		[self setFocusOnPrimaryView];
		
		//NSLog(@"%d", [[self subviews] count]);
	}
	@catch (NSException * e) 
	{
		NSLog(@"TextEditorView -> Split Error: %@ - %@", [e name], [e reason]);
	}
}

- (void) SplitVertical
{
	[self RemoveSplit];

	@try
	{
		NSInteger splitPos = [textView1 frame].size.width / 2;
		[self addSubview:textView2];
		[self setVertical:YES];
		[self setPosition:splitPos ofDividerAtIndex:0];
		[self adjustSubviews];
		[self setPosition:splitPos ofDividerAtIndex:0];
		
		
		[[textView2 content] resignFirstResponder];
		[[self window] makeFirstResponder:nil];
		[self setFocusOnPrimaryView];
	}
	@catch (NSException * e) 
	{
		NSLog(@"TextEditorView -> SplitVertical Error: %@ - %@", [e name], [e reason]);
	}
}

- (void) RemoveSplit	//OK
{
	//removefromsuperview
	//[[self subviews] removeObject:textView2];
	
	@try
	{
		[textView2 removeFromSuperview];
		[self adjustSubviews];
		
		[[textView2 content] resignFirstResponder];
		[[self window] makeFirstResponder:nil];
		[self setFocusOnPrimaryView];
	}
	@catch (NSException * e) 
	{
		NSLog(@"TextEditorView -> RemoveSplit Error: %@ - %@", [e name], [e reason]);
	}
}






- (void) LoadFile:(NSString*) filename
{
	NSError* error = nil;

	NSString *stringFromFileAtPath = [[NSString alloc]
                                      initWithContentsOfFile:filename
                                      encoding:NSUTF8StringEncoding
                                      error:&error];
	if (stringFromFileAtPath == nil) 
	{
		// an error occurred
		NSLog(@"TextEditorView -> Error reading file at %@\n%@",
              filename, [error localizedFailureReason]);
		return;
	}
	
	[textView1 setString: stringFromFileAtPath];

}

- (void) SaveFile:(NSString*) filename
{
	NSError *error = nil;
	BOOL ok = [[self getText] writeToFile:filename atomically:YES
			   encoding:NSUTF8StringEncoding error:&error]; //NSUnicodeStringEncoding
	
	if (!ok) 
	{
		// an error occurred
		NSLog(@"TextEditorView -> Error writing file at %@\n%@",
              filename, [error localizedFailureReason]);
	}
}

- (void) setHighlight: (NSString*) languageName		//OK
{
	[textView1 setStringProperty:SCI_SETLEXERLANGUAGE parameter:0 value:languageName];	
	[textView2 setStringProperty:SCI_SETLEXERLANGUAGE parameter:0 value:languageName];	
}


- (void) setKeyWords: (NSInteger) keyWordSet		//OK
		 keyWordList: (NSString*) keyWordList
{
	const char* keyWordSetCHAR = [keyWordList UTF8String];
	[textView1 setReferenceProperty: SCI_SETKEYWORDS parameter: keyWordSet value: keyWordSetCHAR];
	[textView2 setReferenceProperty: SCI_SETKEYWORDS parameter: keyWordSet value: keyWordSetCHAR];
	////delete[] keyWordSetCHAR;
}


- (void) setWordChars: (NSString*) wordChars		//OK
{
	[textView1 setStringProperty:SCI_SETWORDCHARS parameter:0 value:wordChars];
	[textView2 setStringProperty:SCI_SETWORDCHARS parameter:0 value:wordChars];
}


- (void) setCodePage: (NSInteger) codepage			//OK
{
	[textView1 setGeneralProperty:SCI_SETCODEPAGE parameter:codepage value:0];
	[textView2 setGeneralProperty:SCI_SETCODEPAGE parameter:codepage value:0];
}


- (NSString*) getTextOfLine: (NSInteger) lineNumber		//OK 
{
	NSString* result = nil;
	
	NSInteger start = [self getPositionFromLineNumber:lineNumber];
	NSInteger end = [self GetLineEndPosition:lineNumber];
	NSRange range = NSMakeRange(start, end - start);
	
	////result = [[self getText] substringWithRange:range];
	result = [self getTextRange:range];
	
	return result;
	
}

- (NSString*) getTextOfLineWithEol: (NSInteger) lineNumber		
{
	NSString* result = nil;
	
	NSInteger start = [self getPositionFromLineNumber:lineNumber];
	NSInteger length = [self getLineLength:lineNumber];
	
	NSRange range = NSMakeRange(start, length);
	
	////result = [[self getText] substringWithRange:range];
	result = [self getTextRange:range];
	
	return result;
}

- (NSString*) getTextRange:(NSRange)range
{
	//Sci_TextRange ???
	//[[self getFocusedEditor] backend]->xyz;
	
	NSInteger start = range.location;
	NSInteger end = range.location + range.length;
	NSMutableString* temp = [[NSMutableString alloc] init];
	
	for(start; start < end; start++)
	{
		[temp appendString:[self getCharAt:start]];
	}
	
	NSString* ret = [NSString stringWithString:temp];
	[temp release];
	
	return ret;
	
}


- (NSInteger) getCurrentLineNumber		//OK
{
	//return GetFocusedEditor.LineFromPosition(GetFocusedEditor.GetCurrentPos());
	NSInteger curPosition = 0;
	curPosition = [[self getFocusedEditor] getGeneralProperty:SCI_GETCURRENTPOS parameter:0];
	
	return [[self getFocusedEditor] getGeneralProperty:SCI_LINEFROMPOSITION parameter:curPosition]; 
}


- (NSInteger) getLineLength: (NSInteger) lineNumber
{
	//return [[self getFocusedEditor] getGeneralProperty:SCI_LINELENGTH parameter:lineNumber];
	return [textView1 getGeneralProperty:SCI_LINELENGTH parameter:lineNumber];
}


- (void) ClearAllText
{
	[textView1 setString:@""];
}


- (NSInteger) getCaretPosition
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETCURRENTPOS parameter:0];
}

- (void) setCaretPosition: (NSInteger) position
{
	[[self getFocusedEditor] setGeneralProperty:SCI_GOTOPOS parameter:position value:0];
}

- (void) updateCaret
{
	[[self getFocusedEditor] backend]->UpdateCaret();
}


- (void) GoToLine: (NSInteger) lineNumber	//OK
{
	[[self getFocusedEditor] setGeneralProperty:SCI_GOTOLINE parameter:lineNumber value:0];
}


- (void) InsertText: (NSInteger) position 
			   text: (NSString*) text
{
	//CONVERT EOLS in order to match default Eol
	//text = [textView1 backend]->ConvertEOL(text);
	
	//[textView1 setStringProperty:SCI_INSERTTEXT parameter:position value:text];
	[[self getFocusedEditor] setStringProperty:SCI_INSERTTEXT parameter:position value:text];
}

//Add text at current caret position
- (void) AddText: (NSString*) text
{
	NSInteger length = [text length];
	//[textView1 setStringProperty:SCI_ADDTEXT parameter:length value:text];
	[[self getFocusedEditor] setStringProperty:SCI_ADDTEXT parameter:length value:text];
}

//Add text at the end of document
- (void) AppendText: (NSString*) text
{
	NSInteger length = [text length];
	//[textView1 setStringProperty:SCI_APPENDTEXT parameter:length value:text];
	[[self getFocusedEditor] setStringProperty:SCI_APPENDTEXT parameter:length value:text];
}

- (NSString*) getCharAt: (NSInteger) position	//OK
{
	@try 
	{
		unichar KeyDec = [textView1 getGeneralProperty:SCI_GETCHARAT parameter:position];
		//KeyDec > 32 ??
		if (KeyDec > 0 & KeyDec < 128)
		{
			return [NSString stringWithCharacters:&KeyDec length:1];
		}
	}
	@catch (NSException * e) 
	{
		NSLog(@"TextEditorView -> getCharAt Error: %@ - %@", [e name], [e reason]);
	}

	return @"";

}


////////////////
//SCI_POSITIONFROMPOINT(int x, int y)
//SCI_POSITIONFROMPOINTCLOSE(int x, int y)

- (NSInteger) getPositionFromPoint:(NSPoint) point
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_POSITIONFROMPOINT parameter:point.x extra:point.y];
}

- (NSInteger) getPositionFromPointClose:(NSPoint) point
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_POSITIONFROMPOINTCLOSE parameter:point.x extra:point.y];
}

- (NSPoint) getQuotesPosition:(NSInteger)position withQuote:(NSString*)quote
{
	
	NSInteger mStart = 0;
	NSInteger mEnd = 0;
	NSInteger mCaretPosition = position;
	NSInteger mCurrentLineNumber = [self getLineNumberFromPosition:position];
	NSInteger mLineStart = [self getPositionFromLineNumber:mCurrentLineNumber];
	NSInteger mLineEnd = [self GetLineEndPosition:mCurrentLineNumber];
	
	
	for (NSInteger c = mCaretPosition - 1;
		 c >= mLineStart;
		 c--)
	{
		if ([[self getCharAt:c] isEqualToString:quote])
		{
			mStart = c + 1;
			break;
		}
	}
	
	for (NSInteger c = mCaretPosition;
		 c <= mLineEnd;
		 c++)
	{
		if ([[self getCharAt:c] isEqualToString:quote]) 
		{
			mEnd = c;
			break;
		}
	}
	
	if(mStart > 0 && mEnd > 0)
		return NSMakePoint(mStart, mEnd);
	else 
		return NSMakePoint(0,0);

}

- (NSString*) getTextInQuotes:(NSInteger)position withQuote:(NSString*)quote
{
	NSString* mString = nil;
	NSPoint quotesPosition = [self getQuotesPosition:position withQuote:quote];
	
	
	//if ((mEnd - mStart) > 0 && mStart >= 0)
	if((quotesPosition.y - quotesPosition.x) > 0 &&
	   quotesPosition.x > 0)
	{
		//mString = this.GetText().Substring(mStart, mEnd - mStart);
		//return mString;
		mString = [self getTextRange:NSMakeRange(quotesPosition.x, (quotesPosition.y - quotesPosition.x))];
		return mString;
	}
	
	return @"";
}







- (NSInteger) getTextLength
{
	return [textView1 getGeneralProperty:SCI_GETTEXTLENGTH parameter:0];
}


- (NSInteger) getLinesCount
{
	return [textView1 getGeneralProperty:SCI_GETLINECOUNT parameter:0];
}


- (void) setFirstVisibleLine: (NSInteger) lineNumber
{
	if([[textView2 window] firstResponder] == [textView2 content])
	{
		[self setFirstVisibleLineAtView:lineNumber view:2];
	}
	else [self setFirstVisibleLineAtView:lineNumber view:1];
}

- (void) setFirstVisibleLineAtView: (NSInteger) lineNumber 
							  view: (NSInteger) view
{	
	NSInteger firstVisibleLine = 0;
	if (view == 2)
	{
		firstVisibleLine = [textView2 getGeneralProperty:SCI_GETFIRSTVISIBLELINE parameter:0];
		
		[textView2 setGeneralProperty:SCI_GOTOLINE parameter:lineNumber value:0];
		//[textView2 setGeneralProperty:SCI_LINESCROLL parameter:0  value:(lineNumber - firstVisibleLine)];
		[textView2 setGeneralProperty:SCI_SETFIRSTVISIBLELINE parameter:lineNumber value:0];
		[self setFocusOnSecondaryView];
	}
	else
	{
		firstVisibleLine = [textView1 getGeneralProperty:SCI_GETFIRSTVISIBLELINE parameter:0];
		
		[textView1 setGeneralProperty:SCI_GOTOLINE parameter:lineNumber value:0];
		//[textView1 setGeneralProperty:SCI_LINESCROLL parameter:0  value:(lineNumber - firstVisibleLine)];
		[textView1 setGeneralProperty:SCI_SETFIRSTVISIBLELINE parameter:lineNumber value:0];
		[self setFocusOnPrimaryView];
	}
}

- (NSInteger) getFirstVisibleLine
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETFIRSTVISIBLELINE parameter:0];
}

- (NSInteger) getFirstVisibleLineAtView: (NSInteger) view
{
	if (view == 2)
	{
		return [textView2 getGeneralProperty:SCI_GETFIRSTVISIBLELINE parameter:0];
	}
	else
	{
		return [textView1 getGeneralProperty:SCI_GETFIRSTVISIBLELINE parameter:0];
	}
}


- (NSInteger) getLinesOnScreen
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_LINESONSCREEN parameter:0];
}


- (BOOL) getCanPaste
{
	return [[self getFocusedEditor] backend]->CanPaste();
}

- (void) PerformUndo
{
	[[self getFocusedEditor] setGeneralProperty:SCI_UNDO parameter:0 value:0];
}

- (void) PerformRedo
{
	[[self getFocusedEditor] setGeneralProperty:SCI_REDO parameter:0 value:0];
}

- (void) PerformCopy
{
	//[[self getFocusedEditor] setGeneralProperty:SCI_COPY parameter:0 value:0];
	//[[[self getFocusedEditor] content] copy:self];
	[[self getFocusedEditor] backend]->Copy();
}

- (void) PerformCut
{
	//[[self getFocusedEditor] setGeneralProperty:SCI_CUT parameter:0 value:0];
	[[self getFocusedEditor] backend]->Cut();
}

- (void) PerformPaste
{
	//[[self getFocusedEditor] setGeneralProperty:SCI_PASTE parameter:0 value:0];
	[[self getFocusedEditor] backend]->Paste();
}

- (void) PerformDelete
{
	[[self getFocusedEditor] setGeneralProperty:SCI_CLEAR parameter:0 value:0];
}

- (void) PerformSelectAll
{
	[[self getFocusedEditor] setGeneralProperty:SCI_SELECTALL parameter:0 value:0];
}


- (BOOL) isTextChanged
{
	return ([textView1 getGeneralProperty:SCI_GETMODIFY parameter:0] != 0);
}


- (void) setSavePoint
{
	[textView1 setGeneralProperty:SCI_SETSAVEPOINT parameter:0 value:0];
}


- (void) emptyUndoBuffer
{
	[textView1 setGeneralProperty:SCI_EMPTYUNDOBUFFER parameter:0 value:0];
}


- (NSInteger) getZoom
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETZOOM parameter:0];
}

- (NSInteger) getZoomForView1
{
	return [textView1 getGeneralProperty:SCI_GETZOOM parameter:0];
}

- (NSInteger) getZoomForView2
{
	return [textView2 getGeneralProperty:SCI_GETZOOM parameter:0];
}

- (void) setZoom: (NSInteger) zoom
{
	[[self getFocusedEditor] setGeneralProperty:SCI_SETZOOM parameter:zoom value:0];
}
- (void) setZoomForView1: (NSInteger) zoom
{
	[textView1 setGeneralProperty:SCI_SETZOOM parameter:zoom value:0];
}
- (void) setZoomForView2: (NSInteger) zoom
{
	[textView2 setGeneralProperty:SCI_SETZOOM parameter:zoom value:0];
}





- (NSInteger) FindText: (NSString*) text 
		MatchWholeWord: (BOOL) MatchWholeWord 
			 MatchCase: (BOOL) MatchCase 
			IsBackward: (BOOL) IsBackward 
			SelectText: (BOOL) SelectText 
		   ShowMessage: (BOOL) ShowMessage 
			   SkipRem: (BOOL) SkipRem
{
	return [self FindTextEx:text 
			 MatchWholeWord:MatchWholeWord 
				  MatchCase:MatchCase 
				 IsBackward:IsBackward 
				 SelectText:SelectText 
				ShowMessage:ShowMessage 
					SkipRem:SkipRem 
					  start:-1 
						end:-1
				   useRegEx:false];
}

- (NSInteger) FindText: (NSString*) text 
		MatchWholeWord: (BOOL) MatchWholeWord 
			 MatchCase: (BOOL) MatchCase 
			IsBackward: (BOOL) IsBackward 
			SelectText: (BOOL) SelectText 
		   ShowMessage: (BOOL) ShowMessage 
			   SkipRem: (BOOL) SkipRem
				 start: (NSInteger) Start
				   end: (NSInteger) End
{
	return [self FindTextEx:text 
			 MatchWholeWord:MatchWholeWord 
				  MatchCase:MatchCase 
				 IsBackward:IsBackward 
				 SelectText:SelectText 
				ShowMessage:ShowMessage 
					SkipRem:SkipRem 
					  start:Start 
						end:End
				   useRegEx:false];
}


- (NSInteger) FindTextEx: (NSString*) text 
		  MatchWholeWord: (BOOL) MatchWholeWord 
			   MatchCase: (BOOL) MatchCase 
			  IsBackward: (BOOL) IsBackward 
			  SelectText: (BOOL) SelectText 
		     ShowMessage: (BOOL) ShowMessage 
			     SkipRem: (BOOL) SkipRem
				   start: (NSInteger) start
					 end: (NSInteger) end
				useRegEx: (BOOL) useRegEx;
{

	if ([self getTextLength] > 0)
	{
		
		NSString* StringToFind = text;
		
		NSInteger mStart = [self getCaretPosition]; //GetFocusedEditor.GetCurrentPos();
		NSInteger mEnd = [self getTextLength]; //GetFocusedEditor.GetTextLength();
		NSInteger mSearchFlags = 0;
		NSInteger mFindPos = -1;
		
		
		if (start > -1) mStart = start;
		if (end > -1) mEnd = end;
		
		if (MatchWholeWord)
		{
			mSearchFlags |= SCFIND_WHOLEWORD;
		}
		
		if (MatchCase)
		{
			mSearchFlags |= SCFIND_MATCHCASE;
		}
		
		if(useRegEx)	//ADD REGULAR EXPRESSION SEARCH
		{
			mSearchFlags |= SCFIND_REGEXP;
		}
		
		if (IsBackward)
		{
			//Search backward
			mStart = [self getCaretPosition] - 1; //GetFocusedEditor.GetCurrentPos() - 1;
			mEnd = 0;
		}
			
		
		//Search routine
		@try
		{
			[self setSearchFlags:mSearchFlags];
			//GetFocusedEditor.SetSearchFlags(mSearchFlags);
			
			do
			{
				//[[self getFocusedEditor] setTargetStart:mStart]; //GetFocusedEditor.SetTargetStart(mStart);
				//[[self getFocusedEditor] setTargetEnd:mEnd]; //GetFocusedEditor.SetTargetEnd(mEnd);
				[self setTargetStart:mStart];
				[self setTargetEnd:mEnd];
				
				
				
				
				//mFindPos = [[self getFocusedEditor] searchInTarget:StringToFind]; //GetFocusedEditor.SearchInTarget(StringToFind);
				mFindPos = [self searchInTarget:StringToFind];
				if (mFindPos > -1)
				{
					mStart = mFindPos + [StringToFind length];//1;
					//if (!(this.IsRemAt(mFindPos) && SkipRem == true))
					//if (![self IsRemAt:mFindPos] && SkipRem == true)
					{
						if (SelectText == true)
						{
							[self setSelectionStart:mFindPos];
							[self setSelectionEnd:mFindPos + [StringToFind length]];
							[self ScrollCaret];
							[self setFocus];
							return mFindPos;
						}
						else if (SelectText == false)
						{
							return mFindPos;
						}
					}
				}
				else
				{
					if (ShowMessage == true)
						
						NSRunAlertPanel(@"WinXound Find and Replace", 
										@"Text not Found", 
										@"OK", 
										nil, nil);
						
						return -1;
				}
			}
			while (true);
			
		}
		
		@catch (NSException* ex)
		{
			return -1;
		}
		
	}
	return -1;
}




- (NSMutableArray*) SearchText: (NSString*) text 
				MatchWholeWord: (BOOL) MatchWholeWord 
					 MatchCase: (BOOL) MatchCase 
					IsBackward: (BOOL) IsBackward 
					   SkipRem: (BOOL) SkipRem
{
	return [self SearchTextEx:text 
			 MatchWholeWord:MatchWholeWord
				  MatchCase:MatchCase
				 IsBackward:IsBackward
					SkipRem:SkipRem	
					  start:-1];
}


- (NSMutableArray*) SearchTextEx: (NSString*) text 
				  MatchWholeWord: (BOOL) MatchWholeWord 
					   MatchCase: (BOOL) MatchCase 
					  IsBackward: (BOOL) IsBackward 
						 SkipRem: (BOOL) SkipRem
						   start: (NSInteger) start
{
	if ([self getTextLength] > 0)
	{
		
		//ArrayList mMatches = new ArrayList();
		NSMutableArray* mMatches = [[NSMutableArray alloc] init];
		
		NSString* StringToFind = text;
		NSInteger mStart = 0;
		if (start > 0) mStart = start;
		NSInteger mFindPos = -1;
		NSInteger mTextLength = [self getTextLength];
		
		do
		{
//			mFindPos = this.FindText(StringToFind, MatchWholeWord, MatchCase,
//									 IsBackward, false, false, SkipRem,
//									 mStart, this.GetTextLength());
			[self FindTextEx:StringToFind 
				MatchWholeWord:MatchWholeWord 
				   MatchCase:MatchCase 
				  IsBackward:IsBackward 
				  SelectText:false 
				 ShowMessage:false 
					 SkipRem:SkipRem	
					   start:mStart
						 end:mTextLength
					useRegEx:false];
			
			if (mFindPos > -1)
			{
				mStart = mFindPos + 1;
				//mMatches.Add(mFindPos);
				//[NSNumber numberWithInt: myInt]
				[mMatches addObject: [NSNumber numberWithInt:mFindPos]];
			}
			else
			{
				break;
			}
		}
		while (true);
		
		return mMatches;
		
	}
	
	else return nil;
}


- (void) ReplaceTarget: (NSInteger) offset
				length: (NSInteger) length
		 ReplaceString: (NSString*) ReplaceString
{
	[self setTargetStart:offset]; //GetFocusedEditor.SetTargetStart(offset);
	[self setTargetEnd:(offset + length)]; //GetFocusedEditor.SetTargetEnd(offset + length);
	
	//GetFocusedEditor.ReplaceTarget(-1, ReplaceString);
	//[[self getFocusedEditor] setReferenceProperty:SCI_REPLACETARGET parameter:length value:ReplaceString];
	[[self getFocusedEditor] setStringProperty:SCI_REPLACETARGET parameter:-1 value:ReplaceString];
}


- (void) ReplaceText: (NSString*) replaceString
{
	if ([[self getSelectedText] length] > 0 &&
		[self getSelectedText] != nil)
	{
		[self setSelectedText:replaceString];
		//[self setFocus];
	}
}


- (NSInteger) ReplaceAllText: (NSString*) StringToFind
			   ReplaceString: (NSString*) ReplaceString
			  MatchWholeWord: (BOOL) MatchWholeWord 
				   MatchCase: (BOOL) MatchCase 
		   FromCaretPosition: (BOOL) FromCaretPosition 
					   FCPUp: (BOOL) FCPUp
{
	//if ([[self getSelectedText] length] < 1) return;

	NSInteger mStart = 0;
	NSInteger mEnd = [self getTextLength];
	NSInteger mFindPos = -1;
	NSInteger mSearchFlags = 0;
	NSInteger mTotalOcc = 0;
	
	if (FromCaretPosition)
	{
		mStart = [self getCaretPosition]; //this.GetCaretPosition();
		if (FCPUp)
		{
			mStart = [self getCaretPosition] - 1; //GetFocusedEditor.GetCurrentPos() - 1;
			mEnd = 0;
		}
	}
		
	if (MatchWholeWord)
	{
		mSearchFlags |= SCFIND_WHOLEWORD;
	}
	if (MatchCase)
	{
		mSearchFlags |= SCFIND_MATCHCASE;
	}
	//GetFocusedEditor.SetSearchFlags(mSearchFlags);
	[self setSearchFlags:mSearchFlags];	

	do
	{
		//GetFocusedEditor.SetTargetStart(mStart);
		[self setTargetStart:mStart];
		//GetFocusedEditor.SetTargetEnd(mEnd);
		[self setTargetEnd:mEnd];
		
		
		
		//mFindPos = GetFocusedEditor.SearchInTarget(StringToFind);
		mFindPos = [self searchInTarget:StringToFind];
		if (mFindPos > -1)
		{
			mStart = mFindPos + [ReplaceString length]; //ReplaceString.Length;
			//GetFocusedEditor.SetSelectionStart(mFindPos);
			[self setSelectionStart:mFindPos];
			//GetFocusedEditor.SetSelectionEnd(mFindPos + StringToFind.Length);
			[self setSelectionEnd:mFindPos + [StringToFind length]];
			//GetFocusedEditor.ReplaceSel(ReplaceString);
			[self setSelectedText:ReplaceString];
			mTotalOcc++;
		}
		else
		{
			if (mStart == 0)
			{
				//NSRunAlertPanel(@"WinXound Find and Replace", 
				//				@"Text not Found", 
				//				@"OK", 
				//				nil, nil);
				return 0;
			}
			else
			{
				//NSRunAlertPanel(@"WinXound Find and Replace", 
				//				[NSString stringWithFormat:@"%d occurence(s) replaced", mTotalOcc],  
				//				@"OK", 
				//				nil, nil);
				return mTotalOcc;
			}
			
			break;
		}
	}
	while (true);
	
	
	return 0;

}



- (NSInteger) getLineNumberFromPosition:(NSInteger) position
{
	//NSLog(@"getLineNumberFromPosition: %d",[[self getFocusedEditor] getGeneralProperty:SCI_LINEFROMPOSITION parameter:position]);
	return [textView1 getGeneralProperty:SCI_LINEFROMPOSITION parameter:position];
}

- (NSInteger) getPositionFromLineNumber:(NSInteger) linenumber
{
	return [textView1 getGeneralProperty:SCI_POSITIONFROMLINE	parameter:linenumber];
}



- (NSString*) getCurrentWord
{
	return [self getWordAt:[self getCaretPosition]];
	//tEditor.textEditor.GetWordAt(tEditor.textEditor.GetCaretPosition());
}


- (NSInteger) getWordStart:(NSInteger) position
{
	//return [textView1 backend]->WndProc(SCI_WORDSTARTPOSITION, position, true);		
	
	NSInteger StartPos = 0;
	NSString* c = nil;
	for (StartPos = position; StartPos > 0; StartPos--)
	{
		c = [self getCharAt:(StartPos - 1)];
		if (![self isValidChar:c]) break;
	}
	return StartPos;
}

- (NSInteger) getWordEnd:(NSInteger) position
{
	//return [textView1 backend]->WndProc(SCI_WORDENDPOSITION, position, true);	
	
	NSInteger EndPos = 0;
	NSString* c = nil;
	for (EndPos = position;
		 EndPos < [self getTextLength]; EndPos++)
	{
		// Find the end of the word. 
		c = [self getCharAt:EndPos]; 
		if (![self isValidChar:c]) break;
	}
	return EndPos;
}

- (NSString*) getWordAt:(NSInteger) position
{
	NSMutableString* WordAt = [[NSMutableString alloc] init];
	NSMutableString* tempWord = [[NSMutableString alloc] init];
	
	NSString* c = nil;
	NSInteger StartPos = 0;
	NSInteger EndPos = 0;
	
	
	@try
	{
		for (StartPos = position; StartPos > 0; StartPos--)
		{
			c = [self getCharAt:(StartPos - 1)];
			if ([self isValidChar:c])
			{
				[tempWord appendString:c];
				//tempWord += c;
			}
			else break;
		}
		
		for (NSInteger ciclo = [tempWord length] - 1; ciclo >= 0; ciclo--)
		{	
			//myMutableString appendFormat:@"%C",char
			[WordAt appendFormat:@"%C", [tempWord characterAtIndex:ciclo]];
			//WordAt += tempWord[ciclo];
		}
		
		//for (EndPos = GetFocusedEditor.GetCurrentPos();
		//     EndPos < GetFocusedEditor.GetTextLength(); EndPos++)
		for (EndPos = position;
			 EndPos < [self getTextLength]; EndPos++)
		{
			// Find the end of the word. 
			c = [self getCharAt:EndPos]; //GetFocusedEditor.GetCharAt(EndPos);
			if ([self isValidChar:c])
			{
				[WordAt appendString:c];
				//WordAt += c;
			}
			else break;
		}
		
	}
	@catch (NSException * e) 
	{
		NSLog(@"TextEditorView -> getWordAt Error: %@ - %@", [e name], [e reason]);
	}
	
	
	
	NSString*  result = [NSString stringWithString:WordAt];
	
	[tempWord release];
	[WordAt release];
	
	if(result != nil)
		return result;
	else
		return @"";
	
}

- (BOOL) isValidChar:(NSString*) c
{
	//	return (char.IsLetterOrDigit(c) || char.IsNumber(c) || c == '_' ||
	//			c == '$');
	NSCharacterSet* cSet = [NSCharacterSet alphanumericCharacterSet];
	return ([c isEqualToString: @"#"] ||
			[c rangeOfCharacterFromSet:cSet].length > 0 ||
			[c isEqualToString: @"_"] || 
			[c isEqualToString: @"$"]);
	
}



- (void) setSelection: (NSInteger) start end:(NSInteger) end
{
	[self setSelectionStart:start];
	[self setSelectionEnd:end];
}

- (void) setSelectionStart: (NSInteger) start
{
	[[self getFocusedEditor] setGeneralProperty:SCI_SETSELECTIONSTART parameter:start value:0];
}

- (void) setSelectionEnd: (NSInteger) end
{
	[[self getFocusedEditor] setGeneralProperty:SCI_SETSELECTIONEND parameter:end value:0];
}

- (NSInteger) getSelectionStart
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETSELECTIONSTART parameter:0];
}

- (NSInteger) getSelectionEnd
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETSELECTIONEND parameter:0];
}

- (NSString*) getSelectedText
{
	return [[self getFocusedEditor] selectedString];
}

- (void) setSelectedText: (NSString*) text
{
	[[self getFocusedEditor] setStringProperty:SCI_REPLACESEL parameter:0 value:text];
}



//SCI_STYLERESETDEFAULT
- (void) styleResetToDefault
{
	[textView1 setGeneralProperty:SCI_STYLERESETDEFAULT parameter:0 value:0];
	[textView2 setGeneralProperty:SCI_STYLERESETDEFAULT parameter:0 value:0];
}

- (void) styleClearAll
{
	[textView1 setGeneralProperty:SCI_STYLECLEARALL parameter:0 value:0];
	[textView2 setGeneralProperty:SCI_STYLECLEARALL parameter:0 value:0];
}


- (NSInteger) GetStyleAt:(NSInteger) position
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETSTYLEAT parameter:position];
}


- (BOOL) IsRemAt: (NSInteger) position
{
	//TODO:
	return false;
}


- (NSInteger) GetLineEndPosition: (NSInteger) linenumber
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETLINEENDPOSITION parameter:linenumber];
}


//- (void) Comment
//{
//}
//
//- (void) UnComment
//{
//}


- (void) SelectLine: (NSInteger) linenumber
{
	[self SelectLine: linenumber SetAsFirstVisibleLine: false];
}

- (void) SelectLine: (NSInteger) linenumber SetAsFirstVisibleLine: (BOOL) SetAsFirstVisibleLine
{
	if (SetAsFirstVisibleLine == true)
	{
		//this.SetFirstVisibleLine(linenumber);
		[self setFirstVisibleLine:linenumber];
	}
	else [self ScrollCaret];

	NSInteger mPos = [self getPositionFromLineNumber:linenumber];
	NSInteger mEndPos = [self GetLineEndPosition:linenumber];
	[self setSelectionStart:mPos];
	[self setSelectionEnd:mEndPos];
	[self setFocus];
}
	
	 
- (void) ScrollCaret
{
	[[self getFocusedEditor] setGeneralProperty:SCI_SCROLLCARET parameter:0 value:0];
}


/////////

#pragma mark - EOLS

- (NSString*) convertEolOfText:(NSString*)text
{
	return [textView1 backend]->ConvertEOL(text);
}

- (void) setEolMode:(NSInteger) eolMode
{
	[textView1 setGeneralProperty:SCI_SETEOLMODE parameter:eolMode value:0];
	[textView2 setGeneralProperty:SCI_SETEOLMODE parameter:eolMode value:0];
}

- (NSInteger) GetEOLMode
{
	return [textView1 getGeneralProperty:SCI_GETEOLMODE parameter:0];
}

- (NSInteger) GetEolModeReal
{
	//string t = this.GetText();
	NSString* t = [self getText];
	
	//if (t.Contains("\r\n"))
	if ([t rangeOfString:@"\r\n"].location != NSNotFound)
	{
		return SC_EOL_CRLF;
	}
	//else if (t.Contains("\r"))
	else if ([t rangeOfString:@"\r"].location != NSNotFound)
	{
		return SC_EOL_CR;
	}
	//else if (t.Contains("\n"))
	else if ([t rangeOfString:@"\n"].location != NSNotFound)
	{
		return SC_EOL_LF;
	}
	else
	{
		return SC_EOL_LF;
	}
}

- (NSString*)GetEolModeReport
{
	//Check Line Endings
	//SET EOL MODE: SC_EOL_CRLF (0), SC_EOL_CR (1), or SC_EOL_LF (2)
	
	//string s = this.GetText().Replace("\r\n", "");
	NSString* s = [[self getText] stringByReplacingOccurrencesOfString:@"\r\n" withString:@""];
	
	NSInteger crlfOccurrences = ([self getTextLength] - [s length]) / 2; 
	
	NSInteger crOccurrences = [s length] - [[s stringByReplacingOccurrencesOfString:@"\n" withString:@""] length];
	
	NSInteger lfOccurrences =[s length] - [[s stringByReplacingOccurrencesOfString:@"\r" withString:@""] length];
	
	
	NSString* report = nil;
	//CRLF
	if(crlfOccurrences > 0 && crOccurrences == 0 && lfOccurrences == 0)
		report = [NSString stringWithFormat:@"CRLF (%@)", crlfOccurrences];
	//CR
	else if(crOccurrences > 0 && crlfOccurrences == 0 && lfOccurrences == 0)
		report = [NSString stringWithFormat:@"CR (%@)", crOccurrences];
	//LF
	else if(lfOccurrences > 0 && crlfOccurrences == 0 && crOccurrences == 0)
		report = [NSString stringWithFormat:@"LF (%@)", lfOccurrences];
	//MIXED
	else
		report = @"mixed eols";
		
	
	return report;
}



- (void) ConvertEOL: (NSInteger) eolMode
{
	NSLog(@"ConvertEOL: START");
	
//	//OLD:
//	[textView1 setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
//	[textView1 setGeneralProperty:SCI_ADDUNDOACTION parameter:1 value:0];
//	[textView1 setGeneralProperty:SCI_CONVERTEOLS parameter:eolMode value:0];
//	[textView1 setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
	
	NSString* newEol = nil;

	switch (eolMode)
	{
		case SC_EOL_CRLF:
			newEol = @"\r\n";
			break;
			
		case SC_EOL_CR:
			newEol = @"\r";
			break;
			
		case SC_EOL_LF:
			newEol = @"\n";
			break;
			
		default:
			newEol = @"\n";
			break;
	}
	
	//1. convert all eols in "\n"
	//string temp = this.GetText().Replace("\r\n", "\n").Replace("\r", "\n");
	NSString* a = [[self getText] stringByReplacingOccurrencesOfString:@"\r\n" withString:@"\n"];
	NSString* temp = [a stringByReplacingOccurrencesOfString:@"\r" withString:@"\n"];

	
	//2. convert "\n" to desired format
	[textView1 setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
	[self setText:@""];
	//Add this action to SCI_MOD_CONTAINER (for Undo/Redo notification)
	[textView1 setGeneralProperty:SCI_ADDUNDOACTION parameter:1 value:0];
	
	if([newEol isEqualToString:@"\n"] == false) 
		[self setText:[temp stringByReplacingOccurrencesOfString:@"\n" withString:newEol]];
	else
		[self setText:temp];
	
	[textView1 setGeneralProperty:SCI_ADDUNDOACTION parameter:2 value:0];
	[textView1 setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
	
	
	//3. set Scintilla EOL Mode
	//this.SetEolMode(eolMode);
	[self setEolMode:eolMode];
	
	
	
	NSLog(@"ConvertEOL: END");
}







- (BOOL) GetViewEOL
{
	return [[self getFocusedEditor] getGeneralProperty:SCI_GETVIEWEOL parameter:0];
}

//SCI_SETPASTECONVERTENDINGS(bool convert)
- (void) SetPasteConvertEndings:(BOOL) convert
{
	[textView1 setGeneralProperty:SCI_SETPASTECONVERTENDINGS parameter:convert ? 1 : 0 value:0];
	[textView2 setGeneralProperty:SCI_SETPASTECONVERTENDINGS parameter:convert ? 1 : 0 value:0];
}

- (NSString*) newLine
{
	////[textView1 setGeneralProperty:SCI_NEWLINE parameter:0 value:0];
	
	NSInteger eolMode = [self GetEOLMode];
	NSString* newEol = nil;
	
	switch (eolMode)
	{
		case SC_EOL_CRLF:
			newEol = @"\r\n";
			break;
			
		case SC_EOL_CR:
			newEol = @"\r";
			break;
			
		case SC_EOL_LF:
			newEol = @"\n";
			break;
			
		default:
			newEol = @"\n";
			break;
	}
	
	return newEol;
	
}






- (NSInteger) BraceMatch: (NSInteger) pos 
			  maxReStyle:(NSInteger)maxReStyle
{
	//return [textView1 backend]->WndProc(SCI_WORDENDPOSITION, position, true);		
	return [[self getFocusedEditor] backend]->WndProc(SCI_BRACEMATCH, pos, maxReStyle);
}

- (void) BraceHiglight: (NSInteger) pos1
				  pos2: (NSInteger)pos2
{
	[[self getFocusedEditor] setGeneralProperty:SCI_BRACEHIGHLIGHT parameter:pos1 value:pos2];
}

- (void) refreshSyntax
{
	[textView1 setGeneralProperty:SCI_COLOURISE parameter:0 value:-1];
	[textView2 setGeneralProperty:SCI_COLOURISE parameter:0 value:-1];
}




#pragma mark - Focus
//------------------------------------------------------------------------------------------------
// FOCUS IMPLEMENTATION
//------------------------------------------------------------------------------------------------
- (ScintillaView*) getFocusedEditor	//TO CHECK
{
	if([[textView2 window] firstResponder] == [textView2 content])
	{
		return textView2;
	}
	else if ([[textView1 window] firstResponder] == [textView1 content])
	{
		return textView1;
	}
	else 
	{
		if(_OldFocusedEditor != nil)
			return _OldFocusedEditor;
	}
	
	return textView1;
}

- (void) setFocus
{
	[[self window] makeFirstResponder: [[self getFocusedEditor] content]];
	//_OldFocusedEditor = [[self getFocusedEditor] content];
}

- (void) removeFocus
{
	_OldFocusedEditor = [self getFocusedEditor];
	[[self window] makeFirstResponder:nil];
}

- (void) setFocusOnPrimaryView	//OK
{
	[[self window] makeFirstResponder: [textView1 content]];
	//_OldFocusedEditor = [textView1 content];
}

- (void) setFocusOnSecondaryView	//OK
{
	[[self window] makeFirstResponder: [textView2 content]];
	//_OldFocusedEditor = [textView2 content];
}






#pragma mark - Bookmarks
//------------------------------------------------------------------------------------------------
// BOOKMARKS
//------------------------------------------------------------------------------------------------
- (void) MarkerSetFore: (NSInteger) number htmlcolor: (NSString*) htmlColor
{
	[textView1 setColorProperty: SCI_MARKERSETFORE parameter: number fromHTML: htmlColor];
	[textView2 setColorProperty: SCI_MARKERSETFORE parameter: number fromHTML: htmlColor];
}

- (void) MarkerSetBack: (NSInteger) number htmlcolor: (NSString*) htmlColor
{
	[textView1 setColorProperty: SCI_MARKERSETBACK parameter: number fromHTML: htmlColor];
	[textView2 setColorProperty: SCI_MARKERSETBACK parameter: number fromHTML: htmlColor];
}

- (void) MarkerSetAlpha: (NSInteger) number intcolor: (NSInteger) intColor // intColor value = 0 - 256
{
	[textView1 setGeneralProperty: SCI_MARKERSETALPHA parameter: number value: intColor];
	[textView2 setGeneralProperty: SCI_MARKERSETALPHA parameter: number value: intColor];
}

- (NSInteger) MarkerGet:(NSInteger)linenumber
{
	//return Perform(mDirectPointer, SciConst.SCI_MARKERGET, line, 0);
	return [textView1 getGeneralProperty:SCI_MARKERGET parameter:linenumber];
}

- (void) MarkerDelete:(NSInteger)linenumber
{
	 //Perform(mDirectPointer, SciConst.SCI_MARKERDELETE, line, markerNumber);
	[textView1 setGeneralProperty:SCI_MARKERDELETE parameter:linenumber value:0];
	[textView2 setGeneralProperty:SCI_MARKERDELETE parameter:linenumber value:0];
}

- (void) MarkerAdd:(NSInteger)linenumber
{
	[textView1 setGeneralProperty:SCI_MARKERADD parameter:linenumber value:0];
	[textView2 setGeneralProperty:SCI_MARKERADD parameter:linenumber value:0];
}

- (void) MarkerDeleteAll:(NSInteger)markerNumber
{
	//Perform(mDirectPointer, SciConst.SCI_MARKERDELETEALL, markerNumber, 0);
	[textView1 setGeneralProperty:SCI_MARKERDELETEALL parameter:markerNumber value:0];
	[textView2 setGeneralProperty:SCI_MARKERDELETEALL parameter:markerNumber value:0];
}

- (NSInteger) MarkerNext:(NSInteger)lineStart markerMask:(NSInteger)markerMask
{
	//return Perform(mDirectPointer, SciConst.SCI_MARKERNEXT, lineStart, markerMask);
	return [textView1 getGeneralProperty:SCI_MARKERNEXT parameter:lineStart extra:markerMask];
}

- (NSInteger) MarkerPrevious:(NSInteger)lineStart markerMask:(NSInteger)markerMask
{
	//return Perform(mDirectPointer, SciConst.SCI_MARKERPREVIOUS, lineStart, markerMask);
	return [textView1 getGeneralProperty:SCI_MARKERPREVIOUS parameter:lineStart extra:markerMask];

}


- (void) InsertRemoveBookmark
{
	
	//Int32 mLine = this.GetCurrentLineNumber();
	NSInteger mLine = [self getCurrentLineNumber];
	
	//If Bookmark already exist we remove it
	if([self MarkerGet:mLine] > 0)
	{
		[self MarkerDelete:mLine];
		
	}
	else //Bookmark doesn't exist so we add it
	{
		[self MarkerAdd:mLine];		
	}
}

- (BOOL) hasBookmarks
{
	NSInteger mLine = [self MarkerNext:0 markerMask:1];
	return (mLine > -1);
}

- (void) InsertBookmarkAt:(NSInteger) linePosition
{
	[self MarkerAdd:linePosition];
}

- (void) RemoveAllBookmarks
{
	//textView1.MarkerDeleteAll(0);
	//textView2.MarkerDeleteAll(0);
	[self MarkerDeleteAll:0];
}

- (void) GoToNextBookmark
{
	//Int32 mLine = GetFocusedEditor.MarkerNext(this.GetCurrentLineNumber() + 1, 1);
	NSInteger mLine = [self MarkerNext:[self getCurrentLineNumber]+1 markerMask:1];
	if (mLine > -1)
		//this.GoToLine(mLine);
		[self GoToLine:mLine];
}

- (void) GoToPreviousBookmark
{
	//Int32 mLine = GetFocusedEditor.MarkerPrevious(this.GetCurrentLineNumber() - 1, 1);
	NSInteger mLine = [self MarkerPrevious:[self getCurrentLineNumber]-1 markerMask:1];
	if (mLine > -1)
		//this.GoToLine(mLine);
		[self GoToLine:mLine];
}

		 
// - (void) InsertRemoveBookmark
//{
//	NSInteger mLine = [self getCurrentLineNumber];
//	
//	if([[self getFocusedEditor] getGeneralProperty:SCI_MARKERGET parameter:mLine] > 0) //Bookmark already exist so we remove it
//	{
//		[textView1 setGeneralProperty:SCI_MARKERDELETE parameter:mLine value:0];
//		[textView2 setGeneralProperty:SCI_MARKERDELETE parameter:mLine value:0];
//	}
//	else //Bookmark doesn't exist so we add it
//	{
//		[textView1 setGeneralProperty:SCI_MARKERADD parameter:mLine value:0];
//		[textView2 setGeneralProperty:SCI_MARKERADD parameter:mLine value:0];
//	}
//}
// 
// - (void) RemoveAllBookmarks
//{
//	[textView1 setGeneralProperty:SCI_MARKERDELETEALL parameter:0 value:0];
//	[textView2 setGeneralProperty:SCI_MARKERDELETEALL parameter:0 value:0];
//}
		 


@end 









