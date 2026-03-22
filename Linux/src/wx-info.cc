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

#include "wx-info.h"
#include "wx-global.h"

#define INFO_WIDTH 400
#define INFO_HEIGHT 100

wxInfo::wxInfo()  	
{
	CreateNewInfoWindow();
}
wxInfo::~wxInfo()
{
	//delete labelInfo;
	wxGLOBAL->DebugPrint("wxInfo released");
}


void wxInfo::CreateNewInfoWindow()
{
	hide();

	set_keep_above(TRUE); //Gtk::WINDOW_TOPLEVEL);
	
	set_title("WinXound - Cabbage Info");

	labelInfo =Gtk::manage(new Gtk::Label(""));
	add(*labelInfo);
	show_all_children();

	set_size_request(INFO_WIDTH, INFO_HEIGHT);;
	set_resizable(FALSE);


	Glib::ustring iconfile = wxGLOBAL->getIconsPath();
	iconfile.append("/winxound_48.png");
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
		set_icon_from_file(iconfile);
	
}


void wxInfo::showWindow()
{
	show_all();

	gint width = this->get_screen()->get_width();
	gint height = this->get_screen()->get_height();
	gint x = (width / 2) - (INFO_WIDTH / 2);
	gint y = (height / 2) - (INFO_HEIGHT / 2);
	move(x,y);

	present();
	this->grab_focus();

}

void wxInfo::showWindowAt(gint x, gint y)
{
	show_all();
	
	move(x - (get_width() / 2), 
	     y - (get_height() / 2));

	present();
	this->grab_focus();
	
}
void wxInfo::hideWindow()
{
	//GTK_WINDOW_TOPLEVEL
	hide_all();
}

void wxInfo::setLabelText(Glib::ustring text)
{
	labelInfo->set_text(text);
}


bool wxInfo::on_key_press_event(GdkEventKey* event)
{
	//wxGLOBAL->DebugPrint("KEY", "PRESSED");
	if (event->keyval == GDK_Escape)
	{
		this->hideWindow();
	}
	return false;
}


