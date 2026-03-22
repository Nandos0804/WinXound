// Scintilla source code edit control
/** @file LexWinXound.cxx
 ** Lexer for WinXound (CSound).
 ** Original File by Neil Hodgson
 ** Modded for WinXound by Stefano Bonetti 
 **/
// Copyright 1998-2002 by Neil Hodgson <neilh@scintilla.org>
// The License.txt file describes the conditions under which this software may be distributed.



#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <stdio.h>
#include <stdarg.h>

#include "Platform.h"

#include "PropSet.h"
#include "Accessor.h"
#include "StyleContext.h"
#include "KeyWords.h"
#include "Scintilla.h"
#include "SciLexer.h" //CONST

#define KEYWORD_BOXHEADER 1
#define KEYWORD_FOLDCONTRACTED 2

#ifdef SCI_NAMESPACE
using namespace Scintilla;
#endif



#define SCE_WXOUND_DEFAULT 0
#define SCE_WXOUND_COMMENT 1
#define SCE_WXOUND_COMMENTLINE 2
#define SCE_WXOUND_COMMENTDOC 3
#define SCE_WXOUND_NUMBER 4
#define SCE_WXOUND_WORD1 5
#define SCE_WXOUND_STRING 6
#define SCE_WXOUND_CHARACTER 7
#define SCE_WXOUND_UUID 8
#define SCE_WXOUND_PREPROCESSOR 9
#define SCE_WXOUND_OPERATOR 10
#define SCE_WXOUND_IDENTIFIER 11
#define SCE_WXOUND_STRINGEOL 12
#define SCE_WXOUND_VERBATIM 13
#define SCE_WXOUND_REGEX 14
#define SCE_WXOUND_COMMENTLINEDOC 15
#define SCE_WXOUND_WORD2 16
#define SCE_WXOUND_COMMENTDOCKEYWORD 17
#define SCE_WXOUND_COMMENTDOCKEYWORDERROR 18
#define SCE_WXOUND_GLOBALCLASS 19
#define SCE_WXOUND_WORD3 20
#define SCE_WXOUND_WORD4 21






//#define BUILD_AS_EXTERNAL_LEXER		//NEEDED TO COMPILE THE EXTERNAL LEXER AS DLL

///////////////////////////////////////////////////////////////////////////////////////////
//BUILD_AS_EXTERNAL_LEXER - START
///////////////////////////////////////////////////////////////////////////////////////////
#ifdef BUILD_AS_EXTERNAL_LEXER

#include "WindowAccessor.h"
#include "ExternalLexer.h"

#if PLAT_WIN
#include <windows.h>
#endif


static const char* LexerName = "winxound";
static void InternalLex(unsigned int startPos, int length,
						int initStyle, char *words[], 
						WindowID window, char *props);
static void ColouriseCppDoc(unsigned int startPos, int length, int initStyle, 
							WordList *keywordlists[],
                            Accessor &styler, bool caseSensitive);
static bool IsOKBeforeRE(const int ch);
static inline bool IsAWordChar(const int ch);
static inline bool IsAWordStart(const int ch);
static inline bool IsAValidNumber(const int ch);
static inline bool IsADoxygenChar(const int ch);
static inline bool IsStateComment(const int state);
static inline bool IsStateString(const int state);
static bool IsStreamCommentStyle(int style);
static void FoldNoBoxCppDoc(unsigned int startPos, int length, int initStyle,
                            Accessor &styler);
static void FoldCppDoc(unsigned int startPos, int length, int initStyle, WordList *[],
                       Accessor &styler);
static void ColouriseCppDocSensitive(unsigned int startPos, int length, int initStyle, WordList *keywordlists[],
                                     Accessor &styler);


#ifdef TRACE
void Platform::DebugPrintf(const char *format, ...) {
	char buffer[2000];
	va_list pArguments;
	va_start(pArguments, format);
	vsprintf(buffer,format,pArguments);
	va_end(pArguments);
	Platform::DebugDisplay(buffer);
}
#else
void Platform::DebugPrintf(const char *, ...) {
}
#endif

bool Platform::IsDBCSLeadByte(int codePage, char ch) {
	return ::IsDBCSLeadByteEx(codePage, ch) != 0;
}

long Platform::SendScintilla(WindowID w, unsigned int msg, unsigned long wParam, long lParam) {
	return ::SendMessage(reinterpret_cast<HWND>(w), msg, wParam, lParam);
}

long Platform::SendScintillaPointer(WindowID w, unsigned int msg, unsigned long wParam, void *lParam) {
	return ::SendMessage(reinterpret_cast<HWND>(w), msg, wParam,
		reinterpret_cast<LPARAM>(lParam));
}


//EXPORT FUNCTIONS NEEDED BY SCINTILLA
int EXT_LEXER_DECL GetLexerCount()
{
	return 1;
}
void EXT_LEXER_DECL GetLexerName(unsigned int Index, char *name, int buflength)
{
    // below useless evaluation(s) to suppress "not used" warnings
	Index;
	// return as much of our lexer name as will fit (what's up with Index?)
	if (buflength > 0) {
		buflength--;
		int n = strlen(LexerName);
		if (n > buflength)
			n = buflength;
		memcpy(name, LexerName, n), name[n] = '\0';
	}
}
void EXT_LEXER_DECL Fold(unsigned int lexer, unsigned int startPos, 
						 int length, int initStyle, char *words[], 
						 WindowID window, char *props)
{
}
void EXT_LEXER_DECL Lex(unsigned int lexer, unsigned int startPos, int length, 
						int initStyle, char *words[], WindowID window, char *props)
{
	// below useless evaluation(s) to suppress "not used" warnings
	lexer;
	// build expected data structures and do the Fold
	InternalLex(startPos, length, initStyle, words, window, props);
}




static void InternalLex(unsigned int startPos, int length,
						int initStyle, char *words[], 
						WindowID window, char *props)
{
	// create and initialize a WindowAccessor (including contained PropSet)
	PropSet ps;
	ps.SetMultiple(props);
	WindowAccessor wa(window, ps);

	// create and initialize WordList(s)
	int nWL = 0;
	for (; words[nWL]; nWL++) ;	// count # of WordList PTRs needed
	WordList** wl = new WordList* [nWL + 1];// alloc WordList PTRs
	int i = 0;
	for (; i < nWL; i++) {
		wl[i] = new WordList();	// (works or THROWS bad_alloc EXCEPTION)
		wl[i]->Set(words[i]);
	}
	wl[i] = 0;
	
	// call our "internal" folder/lexer (... then do Flush!)
	ColouriseCppDoc(startPos, length, initStyle, wl, wa, true);
	wa.Flush();

	// clean up before leaving
	for (i = nWL - 1; i >= 0; i--)
		delete wl[i];
	delete [] wl;
}

#endif
///////////////////////////////////////////////////////////////////////////////////////////
//BUILD_AS_EXTERNAL_LEXER - END
///////////////////////////////////////////////////////////////////////////////////////////





static bool IsOKBeforeRE(const int ch) {
	return (ch == '(') || (ch == '=') || (ch == ',');
}


static inline bool IsAWordChar(const int ch) {
	return (ch < 0x80) && (isalnum(ch) || ch == '.' || ch == '_' || ch == 60 || 
		    ch == 62 || ch == 47);
	//return (ch < 0xFF) && (isalnum(ch) || (ch > 32 && ch < 40) || 
	//		/* ch == 44 || */ ch == 46 /* || ch == 47 */ || ch == 58 || ch == 60  || 
	//		ch > 61 && ch < 65 || ch > 90 && ch < 94 || ch > 94 && ch < 97 || 
	//		ch > 122);
}

static inline bool IsAValidNumber(const int ch) {
	//return (ch < 0x80) && (isalpha(ch) || ch == '.' || ch == '_' || ch == 60 || 
	//					   ch == 62 || ch == 47);
	return (ch == '.');
}


static inline bool IsAWordStart(const int ch) {
	return (ch < 0x80) && (isalnum(ch) || ch == '_'|| ch == 60 || ch == 62);
	//return (ch < 0xFF) && (isalnum(ch) || ch == 33 || ch == 37  || (ch > 37 && ch < 40) || 
	//		/* ch == 44 || */ ch == 46 /* || ch == 47 */ || ch == 58 || ch == 60  || 
	//		ch > 61 && ch < 65 || ch > 90 && ch < 94 || ch > 94 && ch < 97 || 
	//		ch > 122);
}


static inline bool IsADoxygenChar(const int ch) {
	return false;
	/*return (islower(ch) || ch == '$' || ch == '@' ||
	        ch == '\\' || ch == '&' || ch == '<' ||
	        ch == '>' || ch == '#' || ch == '{' ||
	        ch == '}' || ch == '[' || ch == ']');*/
}


static inline bool IsStateComment(const int state) {
	return ((state == SCE_WXOUND_COMMENT) ||
	        (state == SCE_WXOUND_COMMENTLINE) ||
	        (state == SCE_WXOUND_COMMENTDOC) ||
	        (state == SCE_WXOUND_COMMENTDOCKEYWORD) ||
	        (state == SCE_WXOUND_COMMENTDOCKEYWORDERROR));
}


static inline bool IsStateString(const int state) {
	return ((state == SCE_WXOUND_STRING) || (state == SCE_WXOUND_VERBATIM));
}




static void ColouriseCppDoc(unsigned int startPos, int length, int initStyle, 
							WordList *keywordlists[],
                            Accessor &styler, bool caseSensitive) {

	WordList &keywords1 = *keywordlists[0];
	WordList &keywords2 = *keywordlists[1];
	WordList &keywords3 = *keywordlists[2];
	WordList &keywords4 = *keywordlists[3];

	//bool stylingWithinPreprocessor = styler.GetPropertyInt("styling.within.preprocessor") != 0;


	// Do not leak onto next line
	if (initStyle == SCE_WXOUND_STRINGEOL)
		initStyle = SCE_WXOUND_DEFAULT;

	int chPrevNonWhite = ' ';
	int visibleChars = 0;
	
	StyleContext sc(startPos, length, initStyle, styler);


	for (; sc.More(); sc.Forward()) {

		if (sc.atLineStart && (sc.state == SCE_WXOUND_STRING)) {
			// Prevent SCE_WXOUND_STRINGEOL from leaking back to previous line
			sc.SetState(SCE_WXOUND_STRING);
		}


		// PREVIOUS STATE
		// Determine if the current state should terminate.
		if (sc.state == SCE_WXOUND_OPERATOR) {
			sc.SetState(SCE_WXOUND_DEFAULT);
		} else if (sc.state == SCE_WXOUND_NUMBER) {
			if (!IsAValidNumber(sc.ch)) {					//MODDED: !IsAWordChar
				sc.SetState(SCE_WXOUND_DEFAULT);
			}
		} else if (sc.state == SCE_WXOUND_IDENTIFIER) {
			if (!IsAWordChar(sc.ch)) {
				char s[100];
				sc.GetCurrent(s, sizeof(s));
				if (keywords1.InList(s) && s != "0dbfs") {	//ADDED: && s != "0dbfs"
					sc.ChangeState(SCE_WXOUND_WORD1);
				} else if (keywords2.InList(s)) {
					sc.ChangeState(SCE_WXOUND_WORD2);
				} else if (keywords3.InList(s)) {
					sc.ChangeState(SCE_WXOUND_WORD3);
				} else if (keywords4.InList(s)) {
					sc.ChangeState(SCE_WXOUND_WORD4);
				} 
				sc.SetState(SCE_WXOUND_DEFAULT);
			}
		} else if (sc.state == SCE_WXOUND_PREPROCESSOR) {
			//if (stylingWithinPreprocessor) {
				if (IsASpace(sc.ch)) {
					sc.SetState(SCE_WXOUND_DEFAULT);
				}
			//} else {
			//	if ((sc.atLineEnd) || (sc.Match('/', '*')) || (sc.Match('/', '/'))) {
			//		sc.SetState(SCE_WXOUND_DEFAULT);
			//	}
			//}
		} else if (sc.state == SCE_WXOUND_COMMENT) {
			if (sc.Match('*', '/')) {
				sc.Forward();
				sc.ForwardSetState(SCE_WXOUND_DEFAULT);
			}
		} else if (sc.state == SCE_WXOUND_COMMENTLINE) { 
			if (sc.atLineEnd) {
				sc.SetState(SCE_WXOUND_DEFAULT);
				visibleChars = 0;
			}
		} else if (sc.state == SCE_WXOUND_STRING) {
			if (sc.ch == '\\') {
				if (sc.chNext == '\"' || sc.chNext == '\'' || sc.chNext == '\\') {
					sc.Forward();
				}
			} else if (sc.ch == '\"') {
				sc.ForwardSetState(SCE_WXOUND_DEFAULT);
			} else if (sc.atLineEnd) {
				sc.ChangeState(SCE_WXOUND_STRINGEOL);
				sc.ForwardSetState(SCE_WXOUND_DEFAULT);
				visibleChars = 0;
			}
		} else if (sc.state == SCE_WXOUND_CHARACTER) {
			if (sc.atLineEnd) {
				sc.ChangeState(SCE_WXOUND_STRINGEOL);
				sc.ForwardSetState(SCE_WXOUND_DEFAULT);
				visibleChars = 0;
			} else if (sc.ch == '\\') {
				if (sc.chNext == '\"' || sc.chNext == '\'' || sc.chNext == '\\') {
					sc.Forward();
				}
			} else if (sc.ch == '\'') {
				sc.ForwardSetState(SCE_WXOUND_DEFAULT);
			}
		}



		
		// NEW STATE
		// Determine if a new state should be entered.
		if (sc.state == SCE_WXOUND_DEFAULT) {
			/*
			if (sc.Match('0')) {							//ADDED FOR '0dbfs'
				char s[10] = "0dbfs";						//ADDED
				if(sc.Match(s)) {							//ADDED
					sc.SetState(SCE_WXOUND_IDENTIFIER);		//ADDED
				}											//ADDED
				else {
					sc.SetState(SCE_WXOUND_NUMBER);
				}
			 */
			if (sc.Match("0dbfs")) {
				sc.SetState(SCE_WXOUND_IDENTIFIER);
			} else if (IsADigit(sc.ch) || (sc.ch == '.' && IsADigit(sc.chNext))) {
				sc.SetState(SCE_WXOUND_NUMBER);
			} else if (IsAWordStart(sc.ch) || (sc.ch == '@')) {
				sc.SetState(SCE_WXOUND_IDENTIFIER);
			} else if (sc.Match(';')) {
				sc.SetState(SCE_WXOUND_COMMENTLINE);
			} else if (sc.Match('/', '*')) {
				sc.SetState(SCE_WXOUND_COMMENT);
				sc.Forward();	// Eat the * so it isn't used for the end of the comment
			} else if (sc.ch == '\"') {
				sc.SetState(SCE_WXOUND_STRING);
			} else if (sc.ch == '\'') {
				sc.SetState(SCE_WXOUND_CHARACTER);
			} else if (sc.ch == '#' && visibleChars == 0) {
				// Preprocessor commands are alone on their line
				sc.SetState(SCE_WXOUND_PREPROCESSOR);
				// Skip whitespace between # and preprocessor word
				do {
					sc.Forward();
				} while ((sc.ch == ' ' || sc.ch == '\t') && sc.More());
				if (sc.atLineEnd) {
					sc.SetState(SCE_WXOUND_DEFAULT);
				}
			} else if (sc.ch == '=') {
				sc.SetState(SCE_WXOUND_OPERATOR);
			// Coulorize MACRO
			} else if (sc.ch == '$') {
				sc.SetState(SCE_WXOUND_PREPROCESSOR);
			}
		}

		// Probabilmente da eliminare
		if (sc.atLineEnd) {
			// Reset states to begining of colourise so no surprises
			// if different sets of lines lexed.
			chPrevNonWhite = ' ';
			visibleChars = 0;
		}
		if (!IsASpace(sc.ch)) {
			chPrevNonWhite = sc.ch;
			visibleChars++;
		}
	}

	//MODDED
	if (sc.state == SCE_WXOUND_IDENTIFIER) {
		if (!IsAWordChar(sc.ch)) {
			char s[100];
			if (caseSensitive) {
				sc.GetCurrent(s, sizeof(s));
			} else {
				sc.GetCurrentLowered(s, sizeof(s));
			}
			if (keywords1.InList(s)) {
				sc.ChangeState(SCE_WXOUND_WORD1);
			} else if (keywords2.InList(s)) {
				sc.ChangeState(SCE_WXOUND_WORD2);
			} else if (keywords3.InList(s)) {
				sc.ChangeState(SCE_WXOUND_WORD3);
			} else if (keywords4.InList(s)) {
				sc.ChangeState(SCE_WXOUND_WORD4);
			}
			sc.SetState(SCE_WXOUND_DEFAULT);
		}
	}

	sc.Complete();
}


static bool IsStreamCommentStyle(int style) {
	return style == SCE_WXOUND_COMMENT ||
	       style == SCE_WXOUND_COMMENTDOC ||
	       style == SCE_WXOUND_COMMENTDOCKEYWORD ||
	       style == SCE_WXOUND_COMMENTDOCKEYWORDERROR;
}



// Store both the current line's fold level and the next lines in the
// level store to make it easy to pick up with each increment
// and to make it possible to fiddle the current level for "} else {".
static void FoldNoBoxCppDoc(unsigned int startPos, int length, int initStyle,
                            Accessor &styler) {
	bool foldComment = styler.GetPropertyInt("fold.comment") != 0;
	bool foldPreprocessor = styler.GetPropertyInt("fold.preprocessor") != 0;
	bool foldCompact = styler.GetPropertyInt("fold.compact", 1) != 0;
	bool foldAtElse = styler.GetPropertyInt("fold.at.else", 0) != 0;
	unsigned int endPos = startPos + length;
	int visibleChars = 0;
	int lineCurrent = styler.GetLine(startPos);
	int levelCurrent = SC_FOLDLEVELBASE;
	if (lineCurrent > 0)
		levelCurrent = styler.LevelAt(lineCurrent-1) >> 16;
	int levelMinCurrent = levelCurrent;
	int levelNext = levelCurrent;
	char chNext = styler[startPos];
	int styleNext = styler.StyleAt(startPos);
	int style = initStyle;
	for (unsigned int i = startPos; i < endPos; i++) {
		char ch = chNext;
		chNext = styler.SafeGetCharAt(i + 1);
		int stylePrev = style;
		style = styleNext;
		styleNext = styler.StyleAt(i + 1);
		bool atEOL = (ch == '\r' && chNext != '\n') || (ch == '\n');
		if (foldComment && IsStreamCommentStyle(style)) {
			if (!IsStreamCommentStyle(stylePrev)) {
				levelNext++;
			} else if (!IsStreamCommentStyle(styleNext) && !atEOL) {
				// Comments don't end at end of line and the next character may be unstyled.
				levelNext--;
			}
		}
		if (foldComment && (style == SCE_WXOUND_COMMENTLINE)) {
			if ((ch == '/') && (chNext == '/')) {
				char chNext2 = styler.SafeGetCharAt(i + 2);
				if (chNext2 == '{') {
					levelNext++;
				} else if (chNext2 == '}') {
					levelNext--;
				}
			}
		}
		if (foldPreprocessor && (style == SCE_WXOUND_PREPROCESSOR)) {
			if (ch == '#') {
				unsigned int j = i + 1;
				while ((j < endPos) && IsASpaceOrTab(styler.SafeGetCharAt(j))) {
					j++;
				}
				if (styler.Match(j, "region") || styler.Match(j, "if")) {
					levelNext++;
				} else if (styler.Match(j, "end")) {
					levelNext--;
				}
			}
		}
		if (style == SCE_WXOUND_OPERATOR) {
			if (ch == '{') {
				// Measure the minimum before a '{' to allow
				// folding on "} else {"
				if (levelMinCurrent > levelNext) {
					levelMinCurrent = levelNext;
				}
				levelNext++;
			} else if (ch == '}') {
				levelNext--;
			}
		}
		if (atEOL) {
			int levelUse = levelCurrent;
			if (foldAtElse) {
				levelUse = levelMinCurrent;
			}
			int lev = levelUse | levelNext << 16;
			if (visibleChars == 0 && foldCompact)
				lev |= SC_FOLDLEVELWHITEFLAG;
			if (levelUse < levelNext)
				lev |= SC_FOLDLEVELHEADERFLAG;
			if (lev != styler.LevelAt(lineCurrent)) {
				styler.SetLevel(lineCurrent, lev);
			}
			lineCurrent++;
			levelCurrent = levelNext;
			levelMinCurrent = levelCurrent;
			visibleChars = 0;
		}
		if (!isspacechar(ch))
			visibleChars++;
	}
}


static void FoldCppDoc(unsigned int startPos, int length, int initStyle, WordList *[],
                       Accessor &styler) {
	FoldNoBoxCppDoc(startPos, length, initStyle, styler);
}

static const char * const cppWordLists[] = {
            "Primary keywords and identifiers",
            "Secondary keywords and identifiers",
            "Documentation comment keywords",
            "Unused",
            "Global classes and typedefs",
            0,
        };



#ifndef BUILD_AS_EXTERNAL_LEXER
static void ColouriseCppDocSensitive(unsigned int startPos, int length, 
									 int initStyle, WordList *keywordlists[],
                                     Accessor &styler) {
	ColouriseCppDoc(startPos, length, initStyle, keywordlists, styler, true);
}

LexerModule lmWinXound(999, ColouriseCppDocSensitive, "winxound", FoldCppDoc, cppWordLists);
#endif
