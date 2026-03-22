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

#ifndef _WX_FINDLINE_H_
#define _WX_FINDLINE_H_

class wxMain;

class wxFindLine
{
public:
	wxFindLine(wxMain* owner);
	virtual ~wxFindLine();


	void associateWidgets(Glib::RefPtr<Gtk::Builder> builder);
	void on_buttonExit_Clicked();
	void on_buttonFindLine_Clicked();
	bool on_key_press_event(GdkEventKey* event);
	void showWindowAt(gint x, gint y);
	void closeWindow();

	Gtk::Window*		FindLineWindow;
	Gtk::SpinButton*	spinbuttonLineNumber;
	Gtk::Button*		buttonFindLine;
	Gtk::Button*		buttonExit;

protected:

private:
	wxMain*				mOwner;

};

#endif // _WX_FINDLINE_H_
