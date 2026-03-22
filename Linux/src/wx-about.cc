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

#include "wx-about.h"
#include "wx-global.h"


wxAbout::wxAbout()  	
{
	CreateNewAboutWindow();
}
wxAbout::~wxAbout()
{
	wxGLOBAL->DebugPrint("wxAbout released");
}


void wxAbout::CreateNewAboutWindow()
{
	hide();
	set_resizable(FALSE);
	set_title("About WinXound"); 

	Gtk::VBox* vbox = Gtk::manage(new Gtk::VBox());
	//vbox->set_size_request(200,-1);

	
	//TITLE
	//Gtk::Label* title = new Gtk::Label(TITLE, 0.50, 0.50, false);
	Gtk::Label* title = Gtk::manage(new Gtk::Label("WinXound", 0.50, 0.50, false));
	title->modify_font(Pango::FontDescription("bold 20"));                               
	vbox->pack_start(*title, FALSE, FALSE, 0);


	//SUBTITLE1
	Gtk::Label* subtitle1 = Gtk::manage(new Gtk::Label("an open source editor for CSound",
	                                                   0.50, 0.50, false));
	subtitle1->modify_font(Pango::FontDescription("normal 14"));  
	vbox->pack_start(*subtitle1, FALSE, FALSE, 0);
	//SUBTITLE2
	Gtk::Label* subtitle2 = Gtk::manage(new Gtk::Label("developed by Stefano Bonetti",
	                                                   0.50, 0.50, false));
	subtitle2->modify_font(Pango::FontDescription("normal 14"));  
	vbox->pack_start(*subtitle2, FALSE, FALSE, 0);

	
	//VERSION
	Glib::ustring ver = "\n";
	ver.append(VERSION);
	ver.append("\n");
	Gtk::Label* version = Gtk::manage(new Gtk::Label(ver, 0.50, 0.50, false));
	version->modify_font(Pango::FontDescription("normal 12")); 
	vbox->pack_start(*version, FALSE, FALSE, 0);

	
	//DIVIDER
	vbox->pack_start(*Gtk::manage(new Gtk::HSeparator()), TRUE, TRUE, 0);

	
	//INFORMATIONS
	Glib::ustring infoText =
		"\nThe Text Editor is entirely based on Scintilla by Neil Hodgson (version 2.11).\n"
		"Scintilla is Copyright by Neil Hodgson (neilh@scintilla.org).\n"
        "CSound is copyright by Barry Vercoe and John ffitch.\n\n"
		"This program is freeware and not intended for commercial use.\n"
        "You could use the source code freely.\n"
        "Altered source versions must be plainly marked as such,\n"
        "and must not be misrepresented as being the original software.\n"
        "If you use this software in a product, an acknowledgment in\n"
        "the product documentation would be appreciated.\n";
	Gtk::Label* info = Gtk::manage(new Gtk::Label(infoText, 0.0, 0.50, false));
	info->modify_font(Pango::FontDescription("normal 8"));  
	vbox->pack_start(*info, FALSE, FALSE, 0);


	//DIVIDER
	vbox->pack_start(*Gtk::manage(new Gtk::HSeparator()), FALSE, FALSE, 0);


	//MAILS
	Gtk::Label* mailTo = Gtk::manage(new Gtk::Label("\nFor any problem or suggestion please Mail to:", 
	                                                0.50, 0.50, false));
	mailTo->modify_font(Pango::FontDescription("normal 10")); 
	//mailTo->set_track_visited_links(FALSE);
	vbox->pack_start(*mailTo, FALSE, FALSE, 0);
	
	Gtk::Label* mails = Gtk::manage(new Gtk::Label("", 0.50, 0.50, false));
	mails->set_markup("<a href=\"mailto:stefano_bonetti@tin.it\">stefano_bonetti@tin.it</a>  or  "
	                  "<a href=\"mailto:stefano_bonetti@alice.it\">stefano_bonetti@alice.it</a>");
	mails->modify_font(Pango::FontDescription("normal 10")); 
	//mails->set_track_visited_links(FALSE);
	vbox->pack_start(*mails, FALSE, FALSE, 0);


	//WEBSITE
	Gtk::Label* webSite = Gtk::manage(new Gtk::Label("", 0.50, 0.50, false));
	webSite->set_markup("\nWinXound Website: <a href=\"http://winxound.codeplex.com\">winxound.codeplex.com</a>");
	webSite->modify_font(Pango::FontDescription("normal 10")); 
	//webSite->set_track_visited_links(FALSE);
	vbox->pack_start(*webSite, FALSE, FALSE, 0);
	                  
	                  
	////VERSION
	//Gtk::Label* version = new Gtk::Label(VERSION, 0.0, 0.50, false);
	//version->set_alignment(Gtk::ALIGN_RIGHT);
	//version->modify_font(Pango::FontDescription("normal 14")); 
	//vbox->pack_start(*version, FALSE, FALSE, 0);

	mailTo->set_can_focus(FALSE);
	mails->set_can_focus(FALSE);
	webSite->set_can_focus(FALSE);

	set_border_width(10);
	add(*vbox);

	Glib::ustring iconfile = Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath());
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
		this->set_icon_from_file(iconfile);
	
}

void wxAbout::showWindowAt(gint x, gint y)
{
	show_all();
	
	move(x - (get_width() / 2), 
	     y - (get_height() / 2));
	this->grab_focus();
	
}
void wxAbout::hideWindow()
{
	hide_all();
}


bool wxAbout::on_key_press_event(GdkEventKey* event)
{
	//wxGLOBAL->DebugPrint("KEY", "PRESSED");
	if (event->keyval == GDK_Escape)
	{
		this->hideWindow();
	}
	return false;
}


