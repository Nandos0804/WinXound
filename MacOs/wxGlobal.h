/*
 *  wxGlobal.h
 *  WinXound
 *
 *  Created by Stefano Bonetti on 27/01/10.
 *
 */


//Various
#define wxMAIN [wxMainController sharedInstance]
#define wxFIND [wxFindAndReplace sharedInstance]
#define wxIMPEXP [wxImportExport sharedInstance]
#define wxCODEREP [wxCodeRepository sharedInstance]
#define wxCSOUNDREP [wxCSoundRepository sharedInstance]
#define wxAUTOCOMP [wxAutoComplete sharedInstance]
#define wxPLAYER [wxPlayer sharedInstance]

#define FIND_STRUCTURE_INTERVAL 6

#define wxDefaults [[NSUserDefaultsController sharedUserDefaultsController] values]
