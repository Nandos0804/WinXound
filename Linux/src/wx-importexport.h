/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * winxound_gtkmm
 * Copyright (C) Stefano Bonetti 2010 <stefano_bonetti@tin.it>
 * 
 * winxound_gtkmm is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * winxound_gtkmm is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


#include <gtkmm.h>


#ifndef _WX_IMPORTEXPORT_H_
#define _WX_IMPORTEXPORT_H_

class wxTextEditor;

class wxImportExport
{
public:
	wxImportExport();
	virtual ~wxImportExport();

	Glib::ustring ImportORCSCO(const gchar* filename);
	void ImportORC(wxTextEditor* textEditor, const gchar* OrcFileName);
	void ImportSCO(wxTextEditor* textEditor, const gchar* ScoFileName);
	void ExportOrcSco(wxTextEditor* textEditor, const gchar* ScoFileName);
	void ExportORC(wxTextEditor* textEditor, const gchar* filename);
	void ExportSCO(wxTextEditor* textEditor, const gchar* filename);
	
protected:

private:

};

#endif // _WX_IMPORTEXPORT_H_
