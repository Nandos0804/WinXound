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


#ifndef _WX_CABBAGE_H_
#define _WX_CABBAGE_H_

class wxInfo;

class wxCabbage
{
public:
	wxCabbage();
	virtual ~wxCabbage();

	void UpdateCabbageInstrument(Glib::ustring filename);

	void CheckForCabbageUpdatesOnInternet();
	void CheckForCabbageUpdatesOnInternet(bool isAutoUpdateCall);
	void ExportToVSTI(Glib::ustring filename);
	void ExportToVST(Glib::ustring filename);
	void ExportToAU(Glib::ustring filename);

	typedef sigc::signal<void, Glib::ustring>	type_signal_message_received;
	type_signal_message_received	signal_message_received(){return m_signal_message_received;};
	
protected:

	type_signal_message_received	m_signal_message_received;
	
	int fifoOUT;
	int fifoIN;
	//Glib::RefPtr<Glib::IOChannel> iochannelIN;
	//Glib::RefPtr<Glib::IOChannel> iochannelOUT;

	wxInfo*			infoWindow;

	bool MyCallback(Glib::IOCondition io_condition);
	unsigned long int Endian_DWord_Conversion(unsigned long int dword);
	gdouble GetVersionNumber(Glib::ustring versionText);
	bool Timer_Tick_Step_1();
	bool Timer_Tick_Step_2();
	void Step_2();
	void Step_3();
	void CreateProgressDialog();
	void on_compilerWindow_closed();
	bool executeCommand(const gchar* path, const gchar* cmd, const gchar* args);
	bool executeCommand(const gchar* path, const gchar* cmd, const gchar* args, bool Async);
	void performCabbageDirectoryCleanup();
	void RemoveDirectory(Glib::ustring dirname);
	void SendMessage(const gchar* message);
	
	void ActivateProgressFormEvents();
	void DeactivateProgressFormEvents();

	void CheckForCabbageConnection(Glib::ustring filename);

	bool Timer_Hide_InfoWindow();

private:
	

};

#endif // _WX_CABBAGE_H_
