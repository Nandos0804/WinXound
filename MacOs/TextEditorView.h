//
//  TextEditorView.h
//  WinXound
//
//  Created by Stefano Bonetti on 20/01/10.
//

//  This is the main TEXT_EDITOR view for the Scintilla based text editor
//  Here we add the split capabilities (two views, primary and secondary)
//  and other useful methods


#import <Cocoa/Cocoa.h>

@class ScintillaView;


//TextEditorView interface
@interface TextEditorView : NSSplitView {
	
	@public
	ScintillaView*		textView1;
	ScintillaView*		textView2;
			
	@private
	ScintillaView*		_OldFocusedEditor;
	BOOL				_ShowMatchingBracket;
	NSInteger			SCI_WIDTH_MARGINS;
	NSString*			SCI_MEASURE_STRING;

}


//- (void) SCI_UPDATEUI_Notification: (NSNotification *) notification;
//- (void) SCI_MODIFIED_Notification: (NSNotification *) notification;


- (NSInteger) getScrollWidth;
- (void) setScrollWidth: (NSInteger) pixelWidth;
- (void) setScrollWidthTracking:(BOOL) tracking;


- (void) setCaretFore: (NSString*) htmlColor;
- (void) setCaretLineBack: (NSString*) htmlColor;
- (void) setEdgeColor: (NSString*) htmlColor;


- (void) setSelFore:(BOOL)useSel htmlcolor: (NSString*) htmlColor;
- (void) setSelBack:(BOOL)useSel htmlcolor: (NSString*) htmlColor;

- (BOOL) isValidChar:(NSString*) c;
- (void) ScrollCaret;

- (void) StyleSetFore: (NSInteger) index htmlcolor: (NSString*) htmlColor;
- (void) StyleSetBack: (NSInteger) index htmlcolor: (NSString*) htmlColor;

- (void) StyleSetBold: (NSInteger) index bold: (BOOL) bold;
- (void) StyleSetItalic: (NSInteger) index italic: (BOOL) italic;

- (void) styleResetToDefault;
- (void) styleClearAll;

- (NSFont*) getTextEditorFont;
- (void) setTextEditorFont: (NSFont*) val;

- (BOOL) getShowSpaces;
- (void) setShowSpaces:(BOOL) val;

- (BOOL) getShowEOLMarker;
- (void) setShowEOLMarker:(BOOL) val;


//- (void) convertEols:(NSInteger) eolMode; //SCI_CONVERTEOLS(int eolMode)

- (BOOL) getEndAtLastLine;
- (void) setEndAtLastLine:(BOOL) val;

- (BOOL) getShowMatchingBracket;
- (void) setShowMatchingBracket:(BOOL) val;

- (BOOL) getCanUndo;
- (BOOL) getCanRedo;

- (NSInteger) getTabIndent;
- (void) setTabIndent:(NSInteger) val;

- (BOOL) getShowLineNumbers;
- (void) setShowLineNumbers:(BOOL) val;

- (BOOL) getMarkCaretLine;
- (void) setMarkCaretLine:(BOOL) val;

- (BOOL) getShowVerticalRuler;
- (void) setShowVerticalRuler: (BOOL) val;

- (BOOL) getReadOnly;
- (void) setReadOnly:(BOOL) val;

- (ScintillaView*) getPrimaryView;
- (ScintillaView*) getSecondaryView;


- (NSString*) getText;
- (void) setText: (NSString*) val;

//-(PrintDocument) getPrintDocument;

- (BOOL) getIsSplitted;

- (void) Split;
- (void) SplitVertical;
- (void) RemoveSplit;

- (void) LoadFile:(NSString*) filename;
- (void) SaveFile:(NSString*) filename;


- (ScintillaView*) getFocusedEditor;
- (void) setFocus;
- (void) removeFocus;
- (void) setFocusOnPrimaryView;
- (void) setFocusOnSecondaryView;

- (void) setHighlight: (NSString*) languageName;

- (void) setKeyWords: (NSInteger) keyWordSet 
		 keyWordList: (NSString*) keyWordList;

- (void) setWordChars: (NSString*) wordChars;

- (void) setCodePage: (NSInteger) codepage;

- (NSString*) getTextOfLine: (NSInteger) lineNumber;
- (NSString*) getTextOfLineWithEol: (NSInteger) lineNumber;
- (NSString*) getTextRange:(NSRange)range;

- (NSInteger) getCurrentLineNumber;

- (NSInteger) getLineLength: (NSInteger) lineNumber;

- (void) ClearAllText;

- (NSInteger) getCaretPosition;
- (void) setCaretPosition: (NSInteger) position;
- (void) updateCaret;

- (void) GoToLine: (NSInteger) lineNumber;

- (void) InsertText: (NSInteger) position 
			   text: (NSString*) text;

- (void) AddText: (NSString*) text; //Add text at current caret position
- (void) AppendText: (NSString*) text; //Add text at the end of document

- (NSString*) getCharAt: (NSInteger) position;

- (NSInteger) getTextLength;

- (NSInteger) getLinesCount;

- (void) setFirstVisibleLine: (NSInteger) lineNumber;
- (void) setFirstVisibleLineAtView: (NSInteger) lineNumber 
							  view: (NSInteger) view;
- (NSInteger) getFirstVisibleLine;
- (NSInteger) getFirstVisibleLineAtView: (NSInteger) view;

- (NSInteger) getLinesOnScreen;

- (BOOL) getCanPaste;
- (void) PerformUndo;
- (void) PerformRedo;
- (void) PerformCopy;
- (void) PerformCut;
- (void) PerformPaste;
- (void) PerformDelete;
- (void) PerformSelectAll;

- (BOOL) isTextChanged;

- (void) setSavePoint;

- (void) emptyUndoBuffer;

- (NSInteger) getZoom;
- (NSInteger) getZoomForView1;
- (NSInteger) getZoomForView2;
- (void) setZoom: (NSInteger) zoom;
- (void) setZoomForView1: (NSInteger) zoom;
- (void) setZoomForView2: (NSInteger) zoom;

- (NSInteger) FindText: (NSString*) text 
		MatchWholeWord: (BOOL) MatchWholeWord 
			 MatchCase: (BOOL) MatchCase 
			IsBackward: (BOOL) IsBackward 
			SelectText: (BOOL) SelectText 
		   ShowMessage: (BOOL) ShowMessage 
			   SkipRem: (BOOL) SkipRem;

- (NSInteger) FindText: (NSString*) text 
		MatchWholeWord: (BOOL) MatchWholeWord 
			 MatchCase: (BOOL) MatchCase 
			IsBackward: (BOOL) IsBackward 
			SelectText: (BOOL) SelectText 
		   ShowMessage: (BOOL) ShowMessage 
			   SkipRem: (BOOL) SkipRem
				 start: (NSInteger) Start
				   end: (NSInteger) End;

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

- (NSMutableArray*) SearchText: (NSString*) text 
			   MatchWholeWord: (BOOL) MatchWholeWord 
					MatchCase: (BOOL) MatchCase 
				   IsBackward: (BOOL) IsBackward 
					  SkipRem: (BOOL) SkipRem;

- (NSMutableArray*) SearchTextEx: (NSString*) text 
			   MatchWholeWord: (BOOL) MatchWholeWord 
					MatchCase: (BOOL) MatchCase 
				   IsBackward: (BOOL) IsBackward 
					  SkipRem: (BOOL) SkipRem
						start: (NSInteger) start;

- (void) ReplaceTarget: (NSInteger) offset
				length: (NSInteger) length
		 ReplaceString: (NSString*) ReplaceString;

- (void) ReplaceText: (NSString*) replaceString;

- (NSInteger) ReplaceAllText: (NSString*) StringToFind
			   ReplaceString: (NSString*) ReplaceString
			  MatchWholeWord: (BOOL) MatchWholeWord 
				   MatchCase: (BOOL) MatchCase 
		   FromCaretPosition: (BOOL) FromCaretPosition 
					   FCPUp: (BOOL) FCPUp; 


- (NSInteger) getLineNumberFromPosition:(NSInteger) position;
- (NSInteger) getPositionFromLineNumber:(NSInteger) linenumber;

- (NSInteger) getWordStart:(NSInteger) position;
- (NSInteger) getWordEnd:(NSInteger) position;
- (NSString*) getWordAt:(NSInteger) position;
- (NSString*) getCurrentWord;


- (void) setSelection: (NSInteger) start end:(NSInteger) end;
- (void) setSelectionStart: (NSInteger) start;
- (void) setSelectionEnd: (NSInteger) end;
- (NSInteger) getSelectionStart;
- (NSInteger) getSelectionEnd;
- (NSString*) getSelectedText;
- (void) setSelectedText: (NSString*) text;


- (void) MarkerSetFore: (NSInteger) number htmlcolor: (NSString*) htmlColor;
- (void) MarkerSetBack: (NSInteger) number htmlcolor: (NSString*) htmlColor;
- (void) MarkerSetAlpha: (NSInteger) number intcolor: (NSInteger) intColor;
- (NSInteger) MarkerGet:(NSInteger)linenumber;
- (void) MarkerDelete:(NSInteger)linenumber;
- (void) MarkerAdd:(NSInteger)linenumber;
- (void) MarkerDeleteAll:(NSInteger)markerNumber;
- (NSInteger) MarkerNext:(NSInteger)lineStart markerMask:(NSInteger)markerMask;
- (NSInteger) MarkerPrevious:(NSInteger)lineStart markerMask:(NSInteger)markerMask;
- (void) InsertRemoveBookmark;
- (void) InsertBookmarkAt:(NSInteger) bookPosition;
- (void) RemoveAllBookmarks;
- (void) GoToNextBookmark;
- (void) GoToPreviousBookmark;
- (BOOL) hasBookmarks;

- (NSInteger) GetStyleAt:(NSInteger) position;

- (BOOL) IsRemAt: (NSInteger) position;

- (NSInteger) GetLineEndPosition: (NSInteger) linenumber;

//- (void) Comment;
//- (void) UnComment;

- (void) SelectLine: (NSInteger) linenumber;
- (void) SelectLine: (NSInteger) linenumber SetAsFirstVisibleLine: (BOOL) SetAsFirstVisibleLine;


//EOLS
- (void) setEolMode:(NSInteger) eolMode;
- (NSInteger) GetEOLMode;
- (NSInteger) GetEolModeReal;
- (void) ConvertEOL: (NSInteger) eolMode;
- (BOOL) GetViewEOL;
- (void) SetPasteConvertEndings:(BOOL) convert;
- (NSString*) convertEolOfText:(NSString*)text;
- (NSString*) newLine;


- (NSInteger) BraceMatch: (NSInteger) pos 
			  maxReStyle:(NSInteger)maxReStyle;
- (void) BraceHiglight: (NSInteger) pos1
				  pos2: (NSInteger)pos2;

- (void) setContextMenu:(NSMenu*) menu;

- (NSString*) getFontName:(NSInteger)styleNumber;
- (NSInteger) measureTextWidth:(NSString*) text 
						  font:(NSFont*) font;
- (void) setSearchFlags: (NSInteger) searchflags;
- (void) setTargetStart:(NSInteger) mStart;
- (void) setTargetEnd:(NSInteger) mEnd;
- (NSInteger) searchInTarget:(NSString*) text;
- (void) setColorProperty: (int) property fromHTML: (NSString*) fromHTML;

- (void) refreshSyntax;

- (NSPoint) getQuotesPosition:(NSInteger)position withQuote:(NSString*)quote;
- (NSString*) getTextInQuotes:(NSInteger)position withQuote:(NSString*)quote;
- (NSInteger) getPositionFromPoint:(NSPoint) point;
- (NSInteger) getPositionFromPointClose:(NSPoint) point;



@end
