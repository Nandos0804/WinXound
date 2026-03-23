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

#include "wx-findandreplace.h"
#include "wx-main.h"
#include "wx-textEditor.h"
#include "wx-global.h"
#include "wx-settings.h"



wxFindAndReplace::wxFindAndReplace(wxMain* owner)
{

	mOwner = owner;
	
	//Create WinXound Settings Window
	Glib::RefPtr<Gtk::Builder> builder;
	try
	{
		//builder = Gtk::Builder::create_from_file(UI_FINDANDREPLACE_FILE);
		builder = Gtk::Builder::create_from_file(
			Glib::ustring::compose("%1/winxound_findandreplace.ui",wxGLOBAL->getSrcPath()));
	}
	catch (const Glib::FileError & ex)
	{
		wxGLOBAL->DebugPrint("wxFindAndReplace constructor - Builder Error - Critical",
		                     ex.what().c_str());
		return;
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxFindAndReplace constructor - Builder Error - Critical");
		return;
	}

	//Locate and Associate wxMainWindow
	builder->get_widget("windowFindAndReplace", findWindow);
	associateWidgets(builder);
	findWindow->set_border_width(2);
	findWindow->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxFindAndReplace::on_key_press_event));

	Glib::ustring iconfile = Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath());
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
		findWindow->set_icon_from_file(iconfile);

	//findWindow->hide();
	//findWindow->set_title(TITLE);
	//findWindow->move(0, 0);
	//findWindow->resize(400,300);
}

wxFindAndReplace::~wxFindAndReplace()
{
	delete findWindow;
	
	wxGLOBAL->DebugPrint("wxFindAndReplace released");
}


bool wxFindAndReplace::on_key_press_event(GdkEventKey* event)
{
	//wxGLOBAL->DebugPrint("KEY", "PRESSED");
	if (event->keyval == GDK_KEY_Escape)
	{
		on_buttonExit_Clicked();
	}
	return false;
}


void wxFindAndReplace::showWindowAt(gint x, gint y)
{               
	//Show window
	findWindow->show_all();
	findWindow->present(); //Deiconify if necessary
	
	findWindow->move(x - (findWindow->get_width()), 
	 			     y - (findWindow->get_height()));

	checkbuttonMatchWholeWord->set_active(wxSETTINGS->General.FindWholeWord);
	checkbuttonMatchCase->set_active(wxSETTINGS->General.FindMatchCase);
	checkbuttonReplaceFromCaret->set_active(wxSETTINGS->General.ReplaceFromCaret);
	
	findWindow->grab_focus();
}

void wxFindAndReplace::closeWindow()
{	
	wxSETTINGS->General.FindWholeWord = checkbuttonMatchWholeWord->get_active();
	wxSETTINGS->General.FindMatchCase = checkbuttonMatchCase->get_active();
	wxSETTINGS->General.ReplaceFromCaret = checkbuttonReplaceFromCaret->get_active();
	
	findWindow->hide();
}


void wxFindAndReplace::associateWidgets(Glib::RefPtr<Gtk::Builder> builder)
{
	
	//WINDOW WIDGETS
	builder->get_widget("entryFindText",entryFindText);
	builder->get_widget("buttonFindPrevious",buttonFindPrevious);
	builder->get_widget("buttonFindNext",buttonFindNext);
	builder->get_widget("checkbuttonMatchWholeWord",checkbuttonMatchWholeWord);
	builder->get_widget("checkbuttonMatchCase",checkbuttonMatchCase);
	builder->get_widget("entryReplaceText",entryReplaceText);
	builder->get_widget("buttonReplace",buttonReplace);
	builder->get_widget("buttonReplaceAll",buttonReplaceAll);
	builder->get_widget("checkbuttonReplaceFromCaret",checkbuttonReplaceFromCaret);
	builder->get_widget("radiobuttonUp",radiobuttonUp);
	builder->get_widget("radiobuttonDown",radiobuttonDown);
	builder->get_widget("buttonExit",buttonExit);
	builder->get_widget("labelInfo",labelInfo);

	buttonExit->signal_clicked().connect(
		sigc::mem_fun(*this, &wxFindAndReplace::on_buttonExit_Clicked));

	buttonFindPrevious->signal_clicked().connect(
		sigc::mem_fun(*this, &wxFindAndReplace::on_buttonFindPrevious_Clicked));

	buttonFindNext->signal_clicked().connect(
		sigc::mem_fun(*this, &wxFindAndReplace::on_buttonFindNext_Clicked));

	buttonReplace->signal_clicked().connect(
		sigc::mem_fun(*this, &wxFindAndReplace::on_buttonReplace_Clicked));

	buttonReplaceAll->signal_clicked().connect(
		sigc::mem_fun(*this, &wxFindAndReplace::on_buttonReplaceAll_Clicked));

}


void wxFindAndReplace::on_buttonFindPrevious_Clicked()
{
	bool ret = mOwner->FindPrevious(entryFindText->get_text().c_str(),
	                                checkbuttonMatchWholeWord->get_active(),
	                                checkbuttonMatchCase->get_active());
	if(ret)
		labelInfo->set_text("");
	else		
		labelInfo->set_text(" Text not found");
}

void wxFindAndReplace::on_buttonFindNext_Clicked()
{
	bool ret = mOwner->FindNext(entryFindText->get_text().c_str(),
	                            checkbuttonMatchWholeWord->get_active(),
	                            checkbuttonMatchCase->get_active());

	if(ret)
		labelInfo->set_text("");
	else
		labelInfo->set_text(" Text not found");
}


/*
void wxMain::Replace(const gchar* StringToFind,
                     const gchar* ReplaceString,
                     bool MatchWholeWord,
                     bool MatchCase,
                     bool FromCaretPosition,
                     bool FCPUp, bool ReplaceAll)
*/

void wxFindAndReplace::on_buttonReplace_Clicked()
{
	mOwner->Replace(entryFindText->get_text().c_str(),
	                entryReplaceText->get_text().c_str(),
	                checkbuttonMatchWholeWord->get_active(),
	                checkbuttonMatchCase->get_active(),
	                checkbuttonReplaceFromCaret->get_active(),
	                radiobuttonUp->get_active(),
	                false);
}

void wxFindAndReplace::on_buttonReplaceAll_Clicked()
{
	mOwner->Replace(entryFindText->get_text().c_str(),
	                entryReplaceText->get_text().c_str(),
	                checkbuttonMatchWholeWord->get_active(),
	                checkbuttonMatchCase->get_active(),
	                checkbuttonReplaceFromCaret->get_active(),
	                radiobuttonUp->get_active(),
	                true);

}

void wxFindAndReplace::on_buttonExit_Clicked()
{
	closeWindow();
}

