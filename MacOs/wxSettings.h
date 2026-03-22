//
//  wxSettings.h
//  WinXound
//
//  Created by Stefano Bonetti on 25/01/10.
//

#import <Cocoa/Cocoa.h>



@interface wxSettings : NSObject //<NSWindowDelegate>
{
	@public
	NSArray* KeyListCSound;
	NSArray* KeyListPython;
	NSArray* KeyListLua;
	
	NSDictionary*  tempList; //private List<string> tempList;
	NSInteger tagSender;
	
	IBOutlet NSWindow* preferencesWindow;
	IBOutlet NSPanel* fontPanel;
	IBOutlet NSTableView* fontTable;
	IBOutlet NSTextField* fontText;
	IBOutlet NSTextField* fontSize;
	
	IBOutlet NSTableView* syntaxListCSound;
	IBOutlet NSTableView* syntaxListPython;
	IBOutlet NSTableView* syntaxListLua;
	
	IBOutlet NSTabView* syntaxColorsTabView;

}


- (void)initialize;


- (IBAction) wxSettingsCANCEL:(id)sender;
- (IBAction) wxSettingsAPPLY:(id)sender;
- (IBAction) wxBrowseFile:(id)sender;
- (IBAction) wxSettingsSetDefaultTemplates:(id)sender;
- (IBAction) wxSettingsSetDefaultCompilerFont:(id)sender;
- (IBAction) wxSettingsSetDefaultEditorFont:(id)sender;
- (IBAction) wxSettingsSetDefaultEditorProperties:(id)sender;
- (IBAction) wxSettingsSetCSoundDefaultFlags:(id)sender;
- (IBAction) wxSettingsSetPythonDefaultFlags:(id)sender;
- (IBAction) wxSettingsSetLuaDefaultFlags:(id)sender;
- (IBAction) wxAutoSearchPaths:(id)sender;
- (IBAction) wxSettingsFontPanelOK:(id)sender;
- (IBAction) wxShowFontPanel:(id)sender;
- (IBAction) wxSettingsFontPanelCANCEL:(id)sender;
- (IBAction) wxChangeBoldState:(id)sender;
- (IBAction) wxChangeItalicState:(id)sender;
- (IBAction) wxChangeColor:(id)sender;
- (IBAction) wxResetSyntaxCSound:(id)sender;
- (IBAction) wxResetSyntaxPython:(id)sender;
- (IBAction) wxResetSyntaxLua:(id)sender;
- (IBAction) wxResetAllToDefault:(id)sender;
- (IBAction) wxResetDocumentSize:(id)sender;
- (IBAction) wxResetCSoundAdditionalFlags:(id)sender;


- (NSString*) StyleGetFriendlyName:(NSString*) lang 
					   stylenumber:(NSInteger) stylenumber;
- (NSString*) StyleGetForeColor:(NSString*) lang 
					stylenumber:(NSInteger) stylenumber;
- (NSString*) StyleGetBackColor:(NSString*) lang 
					stylenumber:(NSInteger) stylenumber;
- (BOOL) StyleGetBold:(NSString*) lang 
		  stylenumber:(NSInteger) stylenumber;
- (BOOL) StyleGetItalic:(NSString*) lang 
			stylenumber:(NSInteger) stylenumber;
- (NSInteger) StyleGetAlpha:(NSString*) lang 
				stylenumber:(NSInteger) stylenumber;
- (BOOL) StyleGetEolFilled:(NSString*) lang 
			   stylenumber:(NSInteger) stylenumber;
//- (NSInteger) StyleGetListPosition:(NSString*) lang 
//					   stylenumber:(NSInteger) stylenumber;
- (NSDictionary*) SelectLanguage: (NSString*) lang;
- (NSArray*) GetValues: (NSString*) s;
//- (void) SetEnvironmentVariables;
- (void) ShowPreferencesWindow;


@end
