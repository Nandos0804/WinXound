//
//  wxCodeFormatter.m
//  WinXound
//
//  Created by Stefano Bonetti on 22/02/10.
//  
//

#import "wxCodeFormatter.h"
#import "wxGlobal.h"
#import "wxMainController.h"
#import "TextEditorView.h"
#import "ScintillaView.h"



@implementation wxCodeFormatter


static id sharedInstance = nil;



#pragma mark Initialization
//--------------------------------------------------------------------------------------------------
// Initialization
//--------------------------------------------------------------------------------------------------
+ (wxCodeFormatter*)sharedInstance
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

//+ (void)initialize {
//		
//}

//--------------------------------------------------------------------------------------------------
// Initialization
//--------------------------------------------------------------------------------------------------






#pragma mark FormatCode
//--------------------------------------------------------------------------------------------------
// FormatCode and related methods
//--------------------------------------------------------------------------------------------------
- (void) formatCode:(TextEditorView*)textEditor 
			  start:(NSInteger)start
				end:(NSInteger)end
{
	
	NSString* textline = @"";
	NSString* tempString = @"";
	BOOL isSinglelineRem = false;
	BOOL isMultilineRem = false;
	NSInteger remIndex = 0;
	//BOOL isOrchestra = false;
	BOOL isInstrument = false;
	BOOL isScore = false;
	BOOL isReplaced = false;
	//char[] delimiter = new char[] { ' ', '\t', '\r', '\n' };
	NSCharacterSet* separator = [NSCharacterSet characterSetWithCharactersInString:@"\t\r\n "];  //whitespaceAndNewlineCharacterSet
	
	
	NSInteger mStart = 0;
	NSInteger mEnd = [textEditor getLinesCount];
	
	
	//Set start and end line search range
	if(start > -1)
	{
		mStart = [textEditor getLineNumberFromPosition: start];
		
		NSInteger r = [textEditor FindText:@"<CsScore>" //@"instr" 
							MatchWholeWord:true 
								 MatchCase:true 
								IsBackward:true 
								SelectText:false 
							   ShowMessage:false 
								   SkipRem:true];
		
		if(r > -1) 
			isScore = true;
		else
		{
			r = [textEditor FindText:@"instr" 
					  MatchWholeWord:true 
						   MatchCase:true 
						  IsBackward:true 
						  SelectText:false 
						 ShowMessage:false 
							 SkipRem:true];
			
			if(r > -1) 
				isInstrument = true;
		}
	}
	if(end > -1)
	{
		mEnd = [textEditor getLineNumberFromPosition: end] + 1;
		if(mEnd > [textEditor getLinesCount])
			mEnd = [textEditor getLinesCount];
	}
	
	
	
	NSInteger tempindex = 0;
	NSString* line = nil;
	NSInteger lineLength = 0;
	
	
	[[textEditor getPrimaryView] setGeneralProperty:SCI_BEGINUNDOACTION parameter:0 value:0];
	
	@try
	{
		//Hack to restore cursor position at start position after Undo action
		//[textEditor setCaretPosition:0];
		//[textEditor InsertText:0 text:@" "]; 
		
		//for (NSInteger i = 0; i < [textEditor getLinesCount]; i++)
		for (NSInteger i = mStart; i < mEnd; i++)
		{
			//Get line length without NewLine characters
			//SCI_GETLINEENDPOSITION(line) - SCI_POSITIONFROMLINE(line)
			lineLength = [textEditor GetLineEndPosition:i] - [textEditor getPositionFromLineNumber:i];
			
			line = [textEditor getTextOfLine:i];
			
			//Skip empty lines
			if([line length] < 2) continue;
			
			
			//Retrieve text of the line (trim whitespaces and newline chars) //mEditor.GetTextLine(i).TrimStart();
			textline = [line stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]]; 
			////textline = [line stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]]; 
			
			//Skip line without chars (after TrimStart())
			if([textline length] < 2) continue;	
			
			
			
			//Start parsing
			isSinglelineRem = false;
			isReplaced = false;
			
			NSArray* words = [textline componentsSeparatedByCharactersInSet:separator]; 
			//foreach (string word in textline.Split(delimiter))
			for (NSString* word in words)
			{
				//Skip empty words
				//if (string.IsNullOrEmpty(word)) continue;
				if([word length] < 1) continue;
				
				
				//Check for single line rem
				//if (word.Contains(";") && isMultilineRem == false)
				if([word rangeOfString:@";"].location != NSNotFound &&
				   isMultilineRem == false)
				{
					isSinglelineRem = true;
					remIndex = [word rangeOfString:@";"].location; //textline.IndexOf(";");
					break;
				}
				
				
				//Check for Multiline rem ('/*' and '*/')
				//if (word.Contains(@"/*") && isSinglelineRem == false)
				if([word rangeOfString:@"/*"].location != NSNotFound &&
				   isSinglelineRem == false)
				{
					isMultilineRem = true;
				}
				//if (word.Contains(@"*/") && isMultilineRem == true /*&& isSinglelineRem == false*/)
				if([word rangeOfString:@"*/"].location != NSNotFound)
				{
					isMultilineRem = false;
				}
				if (isMultilineRem == true) continue;
				
				
				////Check for CsInstruments section
				//if (word == "<CsInstruments>" && isSinglelineRem == false 
				//    && isMultilineRem == false)
				//    isOrchestra = true;
				//if (word == "</CsInstruments>" && isSinglelineRem == false 
				//    && isMultilineRem == false)
				//    isOrchestra = false;
				
				
				//Check for CsScore section
				//if (word == "<CsScore>" && isSinglelineRem == false
				//	&& isMultilineRem == false)
				//	isScore = true;
				if([word isEqualToString:@"<CsScore>"] && isSinglelineRem == false 
				   && isMultilineRem == false)
					isScore = true;
				
				//if (word == "</CsScore>" && isSinglelineRem == false
				//	&& isMultilineRem == false)
				//	isScore = false;
				if([word isEqualToString:@"</CsScore>"] && isSinglelineRem == false 
				   && isMultilineRem == false)
					isScore = false;
				
				//If is CsScore section skip opcodes check
				if (isScore == true) break;
				
				
				
				//Check for opcodes
				if ([word isEqualToString:@"instr"] ||
					[word isEqualToString:@"endin"] ||
					[word isEqualToString:@"opcode"] ||
					[word isEqualToString:@"endop"])
				{
					//Set isInstruments for further opcodes search
					//if (word == "instr" && isSinglelineRem == false && isMultilineRem == false)
					//	isInstrument = true;
					if([word isEqualToString:@"instr"] && isSinglelineRem == false && isMultilineRem == false)
						isInstrument = true;
					//if (word == "endin" && isSinglelineRem == false && isMultilineRem == false)
					//	isInstrument = false;
					if([word isEqualToString:@"endin"] && isSinglelineRem == false && isMultilineRem == false)
						isInstrument = false;
					
					//if (wxGlobal.Settings.CodeFormat.FormatInstruments)
					if([[wxDefaults valueForKey:@"CodeFormatInstruments"] boolValue])
					{
						textline = [self parseString:textline];
						//Style 1: Add tab to start
						//if (wxGlobal.Settings.CodeFormat.InstrumentsType == 0)
						if([[wxDefaults valueForKey:@"CodeFormatInstrumentsType"] integerValue] == 0)
						{
							//textline = "\t" + textline;
							textline = [NSString stringWithFormat:@"\t%@", textline];
							//mEditor.ReplaceTarget(startpos, mEditor.GetLineLength(i), textline);
							//[finalText appendString:[NSString stringWithFormat:@"%@\n", textline]];
							[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
											   length:lineLength //[textEditor getLineLength:i]
										ReplaceString:textline];
							
						}
						//Style 2: No tab space at start
						else
						{
							//line = textline;
							//mEditor.ReplaceTarget(startpos, mEditor.GetLineLength(i), textline);
							//[finalText appendString:[NSString stringWithFormat:@"%@\n", textline]];
							[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
											   length:lineLength //[textEditor getLineLength:i]
										ReplaceString:textline];
						}
						
						isReplaced = true;
					}
					break;
				}
				
				//else if (word.Contains("sr") ||
				//		 word.Contains("kr") ||
				//		 word.Contains("ksmps") ||
				//		 word.Contains("nchnls"))
				//else if ([word rangeOfString:@"sr"].location != NSNotFound  ||
				//		 [word rangeOfString:@"kr"].location != NSNotFound  ||
				//		 [word rangeOfString:@"ksmps"].location != NSNotFound  ||
				//		 [word rangeOfString:@"nchnls"].location != NSNotFound)
				else if ([word isEqualToString:@"sr"] ||
						 [word isEqualToString:@"kr"] ||
						 [word isEqualToString:@"ksmps"] ||
						 [word isEqualToString:@"nchnls"] ||
						 [word isEqualToString:@"0dbfs"] ||
						 [word isEqualToString:@"sr="] ||
						 [word isEqualToString:@"kr="] ||
						 [word isEqualToString:@"ksmps="] ||
						 [word isEqualToString:@"nchnls="] ||
						 [word isEqualToString:@"0dbfs="])
				{
					//if (wxGlobal.Settings.CodeFormat.FormatHeader)
					if([[wxDefaults valueForKey:@"CodeFormatHeader"] boolValue])
					{
						//textline = Regex.Replace(textline, @"\s+", " ") + newline;
						textline = [NSString stringWithFormat:@"%@",[self parseString: textline]];
						
						//tempindex = textline.IndexOf("=");
						tempindex = [textline rangeOfString:@"="].location;
						if (tempindex > -1)
							//textline = "\t" + textline.Substring(0, tempindex).Trim() +
							//"\t" + textline.Substring(tempindex).Trim() + newline;
							textline = [NSString stringWithFormat:@"\t%@\t%@",
										[textline substringToIndex:tempindex],
										[textline substringFromIndex:tempindex]];
						
						//mEditor.ReplaceTarget(startpos, mEditor.GetLineLength(i), textline);
						//[finalText appendString:[NSString stringWithFormat:@"%@\n", textline]];
						[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
										   length:lineLength //[textEditor getLineLength:i] 
									ReplaceString:textline];
						
						isReplaced = true;
					}
					break;
				}
				
				//else if (word == "if" ||
				//		 word == "elseif" ||
				//		 word == "endif")
				else if ([word isEqualToString:@"if"] ||
						 [word isEqualToString:@"elseif"] ||
						 [word isEqualToString:@"endif"])
				{
					//mEditor.ReplaceTarget(startpos, mEditor.GetLineLength(i), textline);
					//line = textline;
					//[finalText appendString:[NSString stringWithFormat:@"%@\n", textline]];
					[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
									   length:lineLength //[textEditor getLineLength:i] 
								ReplaceString:textline];
					
					isReplaced = true;
					break;
				}
				
				//else if (KeyWords.Contains(word) || word == "=")
				else if ([wxMAIN getOpcodeValue:word] != nil ||
						 [word isEqualToString:@"="])
				{				
					//Check inside instr/endin section
					if([[wxDefaults valueForKey:@"CodeFormatInstruments"] boolValue] &&
					   isInstrument == true)
					{
						//Replace multispace with one space
						//textline = Regex.Replace(textline, @"\s+", " ");
						textline = [self parseString:textline];
						
						//Split line in single words
						//string[] split = textline.Split(delimiter);
						NSArray* split = [textline componentsSeparatedByCharactersInSet:separator];
						//string mCompose = "";
						NSMutableString* mCompose = [[[NSMutableString alloc] init] autorelease];
						
						//if (wxGlobal.Settings.CodeFormat.InstrumentsType == 0)
						if([[wxDefaults valueForKey:@"CodeFormatInstrumentsType"] integerValue] == 0)
						{
							//for (Int32 n = 0; n < split.Length; n++)
							for(NSString* s in split)
							{
								//if (split[n] == word)
								if([s isEqualToString:word])
								{
									//mCompose += "\t" + split[n] + "\t";
									[mCompose appendString:[NSString stringWithFormat:@"\t%@\t", s]];
								}
								//else mCompose += split[n] + " ";
								else [mCompose appendString:[NSString stringWithFormat:@"%@ ", s]];
							}
							////[mCompose appendString:@"\n"];
						}
						else
						{
							//mCompose = "\t" + textline;
							[mCompose appendString:[NSString stringWithFormat:@"\t%@", textline]];
						}
						
						//[finalText appendString:[NSString stringWithFormat:@"%@\n", mCompose]];
						[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
										   length:lineLength //[textEditor getLineLength:i] - 1
									ReplaceString:mCompose];
						isReplaced = true;
						break;
						
					}
					else if(isInstrument == true)
					{
						isReplaced = false;
						break;
					}
					
					//check outside (but inside CsInstruments tags)
					else if(isInstrument == false)
					{
						textline = [self parseString:textline];
						NSArray* split = [textline componentsSeparatedByCharactersInSet:separator];
						NSMutableString* mCompose = [[[NSMutableString alloc] init] autorelease];
						
						//for (Int32 n = 0; n < split.Length; n++)
						for (NSInteger n = 0; n < [split count]; n++)
						{
							//if (split[n] == word)
							if([[split objectAtIndex:n] isEqualToString:word])
							{
								//opcode is after the first word
								if (n > 0)
								{
									//mCompose += "\t"; //+ split[n] + "\t";
									[mCompose appendString:@"\t"];
								}
								//mCompose += split[n] + "\t";
								[mCompose appendString:[NSString stringWithFormat:@"%@\t", [split objectAtIndex:n]]];
							}
							//else mCompose += split[n] + " ";
							else [mCompose appendString:[NSString stringWithFormat:@"%@ ", [split objectAtIndex:n]]];
						}
						
						//[finalText appendString:[NSString stringWithFormat:@"%@\n", mCompose]];
						[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
										   length:lineLength //[textEditor getLineLength:i] 
									ReplaceString:mCompose];
						isReplaced = true;
						break;
					}
					
				}
			}
			
			if (isSinglelineRem == true &&
				isMultilineRem == false &&
				isScore == true)
			{
				
				//if (CheckScoreForGoodLine(textline) == false) continue;
				if ([self CheckScoreForGoodLine:textline] == false) continue;
				
				//remIndex = textline.IndexOf(";");
				remIndex = [textline rangeOfString:@";"].location;
				//tempString = textline.Substring(0, remIndex);
				tempString = [textline substringToIndex:remIndex];
				tempString = [self parseString:tempString];
				tempString = [tempString stringByReplacingOccurrencesOfString:@" " withString:@"\t"];
				//textline = Regex.Replace(tempString, @"\s+", " ") +
				//textline.Substring(remIndex);
				textline = [NSString stringWithFormat:@"%@%@", 
							tempString,
							[textline substringFromIndex:remIndex]];
				
			}
			else if (isSinglelineRem == false &&
					 isMultilineRem == false &&
					 isScore == true)
			{
				//if (CheckScoreForGoodLine(textline) == false) continue;
				if ([self CheckScoreForGoodLine:textline] == false) continue;
				
				//textline = Regex.Replace(textline, @"\s+", " ") + newline;
				textline = [self parseString:textline];
				//textline = Regex.Replace(textline, " ", "\t");
				textline = [textline stringByReplacingOccurrencesOfString:@" " withString:@"\t"];
			}
			
			//Finally replace line
			if (isReplaced == false)
			{
				//mEditor.ReplaceTarget(startpos, mEditor.GetLineLength(i), textline);
				//[finalText appendString:[NSString stringWithFormat:@"%@\n", textline]];
				[textEditor ReplaceTarget:[textEditor getPositionFromLineNumber:i] 
								   length:lineLength //[textEditor getLineLength:i] 
							ReplaceString:textline];
			}
			
		}
		
		//[textEditor setCaretPosition:[textEditor getTextLength]];
		[textEditor setCaretPosition:[textEditor getPositionFromLineNumber:mEnd]];
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxCodeFormatter -> formatCode:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
		
	}
	
	[[textEditor getPrimaryView] setGeneralProperty:SCI_ENDUNDOACTION parameter:0 value:0];
	
}


- (BOOL) CheckScoreForGoodLine:(NSString*)textline
{
	//if (textline.StartsWith("f") &&
	//	wxGlobal.Settings.CodeFormat.FormatFunctions == false)
	if([textline hasPrefix:@"f"] &&
	   [[wxDefaults valueForKey:@"CodeFormatFunctions"] boolValue] == false)
		return false;
	
	//	if (textline.StartsWith("i") &&
	//		wxGlobal.Settings.CodeFormat.FormatScoreInstruments == false)
	//		return false;
	if([textline hasPrefix:@"i"] &&
	   [[wxDefaults valueForKey:@"CodeFormatScoreInstruments"] boolValue] == false)
		return false;
	
	//	if (textline.StartsWith("t") &&
	//		wxGlobal.Settings.CodeFormat.FormatTempo == false)
	//		return false;
	if([textline hasPrefix:@"t"] &&
	   [[wxDefaults valueForKey:@"CodeFormatTempo"] boolValue] == false)
		return false;
	
	return true;
}


//Parse string and remove white spaces and other characters
- (NSString*) parseString: (NSString*) stringWithSpaces
{
	NSArray *comps = [stringWithSpaces componentsSeparatedByCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	NSMutableArray *nonemptyComps = [[[NSMutableArray alloc] init] autorelease];
	
	// only copy non-empty entries
	for (NSString *oneComp in comps)
	{
		if (![oneComp isEqualToString:@""])
		{
			[nonemptyComps addObject:oneComp];
		}
	}
	NSString *stringRejoined = [nonemptyComps componentsJoinedByString:@" "];
	
	return stringRejoined; //[stringRejoined substringFromIndex:5];
}

//--------------------------------------------------------------------------------------------------
// FormatCode and related methods
//--------------------------------------------------------------------------------------------------










#pragma mark IBActions
//--------------------------------------------------------------------------------------------------
// IBACTIONS AND RELATED METHODS
//--------------------------------------------------------------------------------------------------
- (void) showOptionsWindow
{
	if (optionsWindow == nil) {
		[NSBundle loadNibNamed:@"wxFormatter" owner:self];
		[optionsWindow setShowsToolbarButton:NO];
		//[preferencesWindow setTitle:@"WinXound Settings"];
	}
	
	[optionsWindow makeKeyAndOrderFront:self];
	[self FormatExample];
	
	
	//	[NSApp beginSheet:preferencesWindow 
	//	   modalForWindow:nil
	//		modalDelegate:self 
	//	   didEndSelector:nil  //@selector(sheetDidFinish:returnCode:contextInfo:) 
	//		  contextInfo:nil];
}

//- (IBAction) wxFormatterCANCEL:(id)sender
//{
//	[[NSUserDefaultsController sharedUserDefaultsController] revert:self];
//	[optionsWindow close];
//	//[NSApp endSheet:optionsWindow];
//	
//}

- (IBAction) wxFormatterAPPLY:(id)sender
{
	//[[NSUserDefaultsController sharedUserDefaultsController] save:self];
	NSUserDefaultsController *defaultsController = [NSUserDefaultsController sharedUserDefaultsController];
	[defaultsController save:self];
	[optionsWindow close];
	//[NSApp endSheet:optionsWindow];
}

- (IBAction) wxFormatterRESET:(id)sender
{
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"CodeFormatHeader"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"CodeFormatInstruments"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"CodeFormatFunctions"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"CodeFormatScoreInstruments"];
	[wxDefaults setValue:[NSNumber numberWithBool:true] forKey:@"CodeFormatTempo"];
	[wxDefaults setValue:[NSNumber numberWithInteger:0] forKey:@"CodeFormatInstrumentsType"];
	
	[self FormatExample];
}


- (IBAction) wxFormatterCLICK:(id)sender
{
	[self FormatExample];
}


- (void) FormatExample
{
	//textEditorExample.ClearAllText();
	[textView setString:@""];
	
	
	//textEditorExample.TabIndent = wxGlobal.Settings.EditorProperties.DefaultTabSize
	//[textView setFont:[NSFont fontWithName:@"Andale Mono" size:12]];
	
	//[[textView textStorage] beginEditing];
	
	
	[[[textView textStorage] mutableString] appendString:
	 @"<CsoundSynthesizer>\n"
	 "<CsOptions>\n" 
	 "-W -odac\n"
	 "</CsOptions>\n"
	 "<CsInstruments>\n"
	 ";Simple Oscili\n"];
	
	//if (checkBoxFormatHeader.Checked)
	if([[wxDefaults valueForKey:@"CodeFormatHeader"] boolValue])
	{
		[[[textView textStorage] mutableString] appendString:
		 @"\tsr     = 44100\n" 
		 "\tkr     = 4410\n"
		 "\tksmps  = 10\n" 
		 "\tnchnls = 1\n\n"];
	}
	else
	{
		[[[textView textStorage] mutableString] appendString:
		 @"sr = 44100\n"
		 "kr = 4410\n"
		 "ksmps = 10\n"
		 "nchnls = 1\n\n"];
	}
	
	//if (checkBoxFormatInstruments.Checked)
	if([[wxDefaults valueForKey:@"CodeFormatInstruments"] boolValue])
	{
		//if (rbInstrumentsType1.Checked)
		if([[wxDefaults valueForKey:@"CodeFormatInstrumentsType"] integerValue] == 0)
		{
			[[[textView textStorage] mutableString] appendString:
			 @"\tinstr 1\n" 
			 "a1\toscili\t10000, 440, 1\n" 
			 "\tout\ta1\n" 
			 "\tendin\n"];
		}
		else
		{
			[[[textView textStorage] mutableString] appendString:
			 @"instr 1\n" 
			 "\ta1 oscili 10000, 440, 1\n" 
			 "\tout a1\n" 
			 "endin\n"];
		}
	}
	else
	{
		[[[textView textStorage] mutableString] appendString:
		 @"instr 1\n" 
		 "a1 oscili 10000, 440, 1\n" 
		 "out a1\n" 
		 "endin\n"];
	}
	
	[[[textView textStorage] mutableString] appendString:
	 @"</CsInstruments>\n" 
	 "<CsScore>\n"];
	
	
	//if (checkBoxFormatFunctions.Checked)
	if([[wxDefaults valueForKey:@"CodeFormatFunctions"] boolValue])
	{
		[[[textView textStorage] mutableString] appendString:
		 @"f1\t0\t4096\t10\t1\n"];
	}
	else
	{
		[[[textView textStorage] mutableString] appendString:
		 @"f1 0 4096 10 1\n"];
	}
	
	//if (checkBoxFormatScoreInstruments.Checked)
	if([[wxDefaults valueForKey:@"CodeFormatScoreInstruments"] boolValue])
	{
		[[[textView textStorage] mutableString] appendString:
		 @"i1\t0\t3\n"];
	}
	else
	{
		[[[textView textStorage] mutableString] appendString:
		 @"i1 0 3\n"];
	}
	
	//if (checkBoxFormatTempo.Checked)
	if([[wxDefaults valueForKey:@"CodeFormatTempo"] boolValue])
	{
		[[[textView textStorage] mutableString] appendString:
		 @"t\t0\t60\t40\t60\t45\t30\n"];
	}
	else
	{
		[[[textView textStorage] mutableString] appendString:
		 @"t 0 60 40 60 45 30\n"];
	}
	
	[[[textView textStorage] mutableString] appendString:
	 @"</CsScore>\n" 
	 "</CsoundSynthesizer>"];
	
	
	[textView setFont:[NSFont fontWithName:@"Andale Mono" size:12]];
	//[[textView textStorage] endEditing];
}







/////////////////////////////////////////////////////////////////////////////////////
//For the moment unused
//private ArrayList FindBlockOfRems()
//{
//	ArrayList tempArray = new ArrayList();
//	
//	string pattern = @"/\*";
//	MatchCollection mcStart = Regex.Matches(mEditor.GetText(), pattern);
//	
//	pattern = @"\*/";
//	MatchCollection mcEnd = Regex.Matches(mEditor.GetText(), pattern);
//	
//	foreach (Match m in mcStart)
//	{
//		tempArray.Add(m.Index.ToString() + ",");
//	}
//	
//	for (Int32 i = 0; i < tempArray.Count; i++)
//	{
//		if (mcEnd[i] != null)
//			tempArray[i] = tempArray[i].ToString() + mcEnd[i].Index.ToString();
//	}
//	
//	return tempArray;
//	
//}



@end
