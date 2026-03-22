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


#include "wx-cabbage.h"
#include "wx-settings.h"
#include "wx-global.h"
#include <iostream>
#include <fstream>
#include <fcntl.h>
#include <stdint.h>

#include "wx-info.h"

#include <webkit/webkit.h>


WebKitDownload* download;
WebKitNetworkRequest* request;
Glib::ustring currentVersion = "";
Glib::ustring webVersionName = "";
double webVersionNumber = 0.0;
Glib::ustring url = "";
bool mIsAutoUpdateCall = false;
gint totalDownloadSize = 0;
bool InterruptedByUser = false;
sigc::connection close_signal_connection;
sigc::connection fifoIn_callback;




wxCabbage::wxCabbage() 	
{

	//CREATE THE DOWNLOAD INFO PROGRESS DIALOG:
	//CreateProgressDialog();
	infoWindow = new wxInfo();

	fifoIN = -1;
	fifoOUT = -1;
	
}

wxCabbage::~wxCabbage(void)
{	
	if(download != NULL)
		g_object_unref(download);
	
	if(request != NULL)
		g_object_unref(request);

	//delete labelInfo;
	delete infoWindow;

	close(fifoOUT);
	close(fifoIN);
	
	wxGLOBAL->DebugPrint("wxCabbage released");
}


void wxCabbage::CheckForCabbageConnection(Glib::ustring filename)
{
	if(!Glib::file_test(wxSETTINGS->Directory.CabbagePath, Glib::FILE_TEST_EXISTS))
		return;

	if(fifoOUT != -1 && 
	   !Glib::file_test("/tmp/cabbage_in", Glib::FILE_TEST_EXISTS)) 
	{
		close(fifoOUT); //return; //Connection already established
		fifoOUT = -1;
	}

	//system("ps -A | grep cabbage > filtereedoutput");
	Glib::ustring path = wxGLOBAL->getCabbagePath();
	path.append("/processes.txt");
	Glib::ustring command = "ps -A > ";
	command.append(path);
	system(command.c_str());

	                              
	if(Glib::file_test(path, Glib::FILE_TEST_EXISTS))
	{
		Glib::ustring temp = Glib::file_get_contents(path);

		if(temp.find(CABBAGE_NAME) != Glib::ustring::npos &&
		   Glib::file_test("/tmp/cabbage_out", Glib::FILE_TEST_EXISTS) &&
		   Glib::file_test("/tmp/cabbage_in", Glib::FILE_TEST_EXISTS))
		{
			std::cout << "-----> CABBAGE ALREADY LAUNCHED" << std::endl;
		}
		else
		{
			std::cout << "-----> TRY TO START CABBAGE" << std::endl;

			infoWindow->set_title("WinXound - Cabbage info");
			infoWindow->setLabelText("Starting Cabbage, please wait ...");
			infoWindow->showWindow();
			
			//Try to start a new Cabbage section:
			/*
			this->executeCommand(wxGLOBAL->getCabbagePath().c_str(), //NULL
			                     wxSETTINGS->Directory.CabbagePath.c_str(), 
			                     filename.c_str(), true);
			*/

			
			Glib::ustring commandForCabbage = "";
			commandForCabbage.append(wxSETTINGS->Directory.CabbagePath.c_str());
			commandForCabbage.append("&");
			system(commandForCabbage.c_str());

			
			for(gint t = 0; t < 10; t++)
			{
				
				system(command.c_str());
				temp = Glib::file_get_contents(path);
				if(temp.find(CABBAGE_NAME) != Glib::ustring::npos)
					break;
					
				/*
				fifoOUT = open("/tmp/cabbage_in", O_WRONLY | O_NONBLOCK);
				if(fifoOUT != -1)
				{
					//close(fifoOUT);
					break;
				}
				*/
						
				usleep(250000);
			}

			usleep(250000);			
			Glib::signal_timeout().connect(sigc::mem_fun(*this, &wxCabbage::Timer_Hide_InfoWindow), 600);
		}
	}

	remove(path.c_str());


	//CHECK ALSO FOR READ CHANNEL
	if(fifoIN != -1) 
	{
		if(fifoIn_callback != NULL)
			fifoIn_callback.disconnect();
		
		close(fifoIN);
		fifoIN == -1;
	}
	//if(fifoIN == -1)
	{
		fifoIN = open("/tmp/cabbage_out", O_RDWR); //O_RDONLY | O_NDELAY | O_NONBLOCK);
		//iochannelIN = Glib::IOChannel::create_from_fd(fifoIN);
		std::cout << "fifoIN Created: " << fifoIN << std::endl;
		//return;
		if (fifoIN != -1)
		{
			fifoIn_callback = Glib::signal_io().connect(
				sigc::mem_fun(*this, &wxCabbage::MyCallback), fifoIN, 
					Glib::IO_IN | Glib::IO_ERR | Glib::IO_HUP | Glib::IO_NVAL);
		}
		else
			std::cout << "error opening fifoIN" << std::endl;
	}
	
	std::cout << "CheckForCabbageConnection ended" << std::endl;
	
}

bool wxCabbage::Timer_Hide_InfoWindow()
{
	infoWindow->hideWindow();
	return false;
}

void wxCabbage::SendMessage(const gchar* message)
{
	
	//WRITE CHANNEL
	//fifoOUT = open("/tmp/cabbage_in", O_WRONLY);

	
	for(gint t = 0; t < 10; t++)
	{
		std::cout << "CABBAGE LOOP: " << t << std::endl;
		fifoOUT = open("/tmp/cabbage_in", O_WRONLY); // | O_NONBLOCK);
		if(fifoOUT != -1)
			break;
		usleep(250000);
	}
	
	
	if(fifoOUT != -1)
	{
		//Create the Juce Message Header
		guint32 messageHeader[2];
		messageHeader [0] = (guint32)4071923244;
		messageHeader [1] = (guint32)strlen(message);
	
		//MemoryBlock messageData (sizeof (messageHeader) + message.getSize());
		gint bufferLength = sizeof(messageHeader) + strlen(message);   // + 1 (Not needed);
	    gchar* buffer = new gchar[bufferLength];
		memset (buffer,'\0', bufferLength);

		memcpy(buffer, &messageHeader, sizeof(messageHeader));
		////copyFrom (buffer, message, sizeof(messageHeader), strlen(message));
		memcpy(buffer + 8, message, strlen(message));
		std::cout << "Size of MessageHeader: " << sizeof(messageHeader)
				  << "\nSize of Message: " << strlen(message)
			      << "\nData sent: " << buffer + 8 << std::endl;


		write(fifoOUT, buffer, bufferLength);
		close(fifoOUT);
		delete[] buffer;
		
		std::cout << "Update Cabbage" << std::endl;
		
	}


}

void wxCabbage::UpdateCabbageInstrument(Glib::ustring filename)
{
	CheckForCabbageConnection(filename);
	
	Glib::ustring message = 
		Glib::ustring::compose("CABBAGE_UPDATE| %1", filename);
	
	this->SendMessage(message.c_str());

}

unsigned long int wxCabbage::Endian_DWord_Conversion(unsigned long int dword) 
{
   return ((dword>>24)&0x000000FF) | 
		  ((dword>>8)&0x0000FF00) | 
		  ((dword<<8)&0x00FF0000) | 
		  ((dword<<24)&0xFF000000);
}




// this will be our signal handler for read operations
// it will print out the message sent to the fifo
bool wxCabbage::MyCallback(Glib::IOCondition io_condition)
{
	//std::cout << "wxCabbage::CALLBACK -> " << 
	//	         (io_condition & Glib::IO_IN) <<std::endl;
	////return false;
	
	if ((io_condition & Glib::IO_IN) == 0) 
	{
		std::cout << "Invalid fifo response" << std::endl;
		fifoIn_callback.disconnect();
		close(fifoIN);
		fifoIN = -1;
		return false;
	}
	else 
	{
		try
		{
			//Glib::ustring buf;
			//iochannelIN->read_line(buf);
			//std::cout << buf;

			char buf[4096];
			//char* buf = new char[4096];
			gint numread = read(fifoIN, buf, 4096);
			//std::cout << "Data received: " << numread << std::endl;

			if(numread > 7) //-1) NOTE: 0->7 are the magicnumber
			{
				buf[numread] = '\0';

				//std::cout << "Data received: " << buf + 8 << std::endl;

				Glib::ustring message = buf + 8; //CRITICAL!!!
				m_signal_message_received(message);
			}

			//delete[] buf;

			//Not needed:
			//Glib::signal_io().connect(sigc::mem_fun(*this, &wxPipe::MyCallback), 
			//  fifoIN, Glib::IO_IN);
		}
		catch(...)
		{
			std::cout << "-->NAMEDPIPE ERROR" << std::endl;
		}
	}
	
	return true;
}










////////////////////////////////////////////////////////////////////////////////////////////
// CABBAGE EXPORT FUNCTIONS
////////////////////////////////////////////////////////////////////////////////////////////
//CABBAGE_EXPORT_VSTI
//CABBAGE_EXPORT_VST
//CABBAGE_EXPORT_AU

void wxCabbage::ExportToVSTI(Glib::ustring filename)
{
	CheckForCabbageConnection(filename);
	
	//First update Cabbage
	Glib::ustring message = 
		Glib::ustring::compose("CABBAGE_UPDATE| %1", filename);
	this->SendMessage(message.c_str());

	usleep(250000);
	
	//After send export message
	message = Glib::ustring::compose("CABBAGE_EXPORT_VSTI| %1", filename);
	this->SendMessage(message.c_str());
}

void wxCabbage::ExportToVST(Glib::ustring filename)
{
	CheckForCabbageConnection(filename);
	
	//First update Cabbage
	Glib::ustring message = 
		Glib::ustring::compose("CABBAGE_UPDATE| %1", filename);
	this->SendMessage(message.c_str());

	usleep(250000);
	
	//After send export message
	message = Glib::ustring::compose("CABBAGE_EXPORT_VST| %1", filename);
	this->SendMessage(message.c_str());
}

void wxCabbage::ExportToAU(Glib::ustring filename)
{
	CheckForCabbageConnection(filename);
	
	//First update Cabbage
	Glib::ustring message = 
		Glib::ustring::compose("CABBAGE_UPDATE| %1", filename);
	this->SendMessage(message.c_str());

	usleep(250000);
	
	//After send export message
	message = Glib::ustring::compose("CABBAGE_EXPORT_AU| %1", filename);
	this->SendMessage(message.c_str());
}

























////////////////////////////////////////////////////////////////////////////////
// CABBAGE INTERNET UPDATES 
////////////////////////////////////////////////////////////////////////////////
void wxCabbage::CheckForCabbageUpdatesOnInternet()
{
	this->CheckForCabbageUpdatesOnInternet(false);
}

void wxCabbage::CheckForCabbageUpdatesOnInternet(bool isAutoUpdateCall)
{

	mIsAutoUpdateCall = isAutoUpdateCall;
	
	//1. Check inside version.txt the right file to download
	//http://cabbage.googlecode.com/files/version.txt
	url = "http://cabbage.googlecode.com/files/version.txt";
    request = webkit_network_request_new(url.c_str());
	download = webkit_download_new(request);
	//if(webkit_download_get_status(download) < 0)
	//{
	//	std::cerr << "ERROR DOWNLOADING VERSION.TXT FILE" << std::endl;
	//	return;
	//}

	std::cerr << "DOWNLOADING VERSION.TXT FILE" << std::endl;
	
	Glib::ustring downloadPath = wxGLOBAL->getCabbagePath();
	downloadPath.append("/version.txt");
	webkit_download_set_destination_uri (download,
                                         Glib::filename_to_uri(downloadPath).c_str());
	webkit_download_start(download);
		
	Glib::signal_timeout().connect(sigc::mem_fun(*this, &wxCabbage::Timer_Tick_Step_1), 200);

}

bool wxCabbage::Timer_Tick_Step_1()
{
	std::cerr << "Timer_Tick_Step_1: " << webkit_download_get_current_size(download) << std::endl;
	
	if(webkit_download_get_status(download) != WEBKIT_DOWNLOAD_STATUS_FINISHED)
		return true;       //Continue to tick
	else
	{
		if(Glib::file_test(Glib::ustring::compose("%1/version.txt",wxGLOBAL->getCabbagePath()),
		                   Glib::FILE_TEST_EXISTS))
		{
			Step_2();
			return false; //Stop to tick
		}
		else 
			return true;  //Continue to tick
		
	}
}

void wxCabbage::Step_2()
{
	Glib::ustring versionTxtPath = wxGLOBAL->getCabbagePath();
	versionTxtPath.append("/version.txt");
	
	if(Glib::file_test(versionTxtPath, Glib::FILE_TEST_EXISTS))
	{
		Glib::ustring content = "";			
		content = Glib::file_get_contents(versionTxtPath);

		if (content != "")
		{
			//g_strsplit: returns :a newly-allocated NULL-terminated array of strings. 
			//Use g_strfreev() to free it. 
			gchar** files = g_strsplit(content.c_str(), "\n", 0);
			int length = wxGLOBAL->ArrayLength(files);
			for(int i=0; i < length; i++)
			{
				std::string f = files[i];
				gchar** temp = g_strsplit(f.c_str(), ":", 0);
				Glib::ustring value = temp[0];
				if(value == "LINUX")
				{
					webVersionName = temp[1];
					webVersionNumber = this->GetVersionNumber(webVersionName);
				}
				g_strfreev(temp);
			}	
			g_strfreev(files);
		}
	}
	else 
	{
		if (!mIsAutoUpdateCall)
			wxGLOBAL->ShowMessageBox("Unable to establish a connection to the "
			                         "'cabbage.googlecode.com' site.",
			                         "WinXound - Cabbage Update Info",
			                         Gtk::BUTTONS_OK);
		return;
	}   


    //COMPARE VERSIONS
	std::cerr << "webVersionName: " << webVersionName << std::endl;
	std::cout << "webVersionNumber: " << webVersionNumber << std::endl;

	Glib::ustring currentCabbageVersionPath = wxGLOBAL->getCabbagePath();
	currentCabbageVersionPath.append("/CurrentVersionTime.txt");

	gdouble currentVersion = 0.0;
	//if (File.Exists(Application.StartupPath + "\\Cabbage\\CurrentVersionTime.txt"))
	if(Glib::file_test(currentCabbageVersionPath, Glib::FILE_TEST_EXISTS))
	{
		//string temp = File.ReadAllText(Application.StartupPath + "\\Cabbage\\CurrentVersionTime.txt");
		Glib::ustring content = "";			
		content = Glib::file_get_contents(currentCabbageVersionPath);
		currentVersion = wxGLOBAL->StringToDouble(content);
	}

	
	if(currentVersion >= webVersionNumber)
	{
		if (!mIsAutoUpdateCall)
			wxGLOBAL->ShowMessageBox("Your Cabbage version is already up-to-date.",
			                         "WinXound - Cabbage Update Info",
			                         Gtk::BUTTONS_OK);
		return;
	}


	//ASK FOR THE DOWNLOAD:
	gint ret = wxGLOBAL->ShowMessageBox("There is a new Cabbage update available.\n"
                                		"Would you like to download and install it?",
			                     		"Cabbage update available ...",
			                     		Gtk::BUTTONS_YES_NO);
                                  

	if(ret != Gtk::RESPONSE_YES) return;

	
	//Check for cabbage on googlecode:
	url = "http://cabbage.googlecode.com/files/";
	url.append("CabbageWin32.zip");//(webVersionName); TODO:
	request = webkit_network_request_new(url.c_str());
	download = webkit_download_new(request);


	Glib::ustring downloadPath = wxGLOBAL->getCabbagePath();
	downloadPath.append("/CabbageLinux.zip");
	
	webkit_download_set_destination_uri (download,
                                         Glib::filename_to_uri(downloadPath).c_str());

	InterruptedByUser = false;
	
	webkit_download_start(download);
	totalDownloadSize = webkit_download_get_total_size (download);

	ActivateProgressFormEvents();
	
	infoWindow->set_title("Downloading Cabbage ...");
	infoWindow->showWindow();

	Glib::signal_timeout().connect(sigc::mem_fun(*this, &wxCabbage::Timer_Tick_Step_2), 200);
}

bool wxCabbage::Timer_Tick_Step_2()
{
	if(totalDownloadSize <= 0)
		totalDownloadSize = webkit_download_get_total_size (download);
	
	if(infoWindow != NULL)
	{
		int percentage = 1;
		if(totalDownloadSize > 0)
			percentage = (int)(100 * webkit_download_get_current_size(download) / totalDownloadSize);
		Glib::ustring percentageText =
			Glib::ustring::compose("Percentage: %1%%", percentage); 
		infoWindow->setLabelText(percentageText);
	}

	if(webkit_download_get_status(download) == WEBKIT_DOWNLOAD_STATUS_CANCELLED)
		return false;
	
	if(webkit_download_get_status(download) != WEBKIT_DOWNLOAD_STATUS_FINISHED)
		return true;  //Continue to tick
	else
	{
		Step_3();
		return false; //Stop to tick
	}
}

void wxCabbage::Step_3()
{
	//DECOMPRESS AND INSTALL CABBAGE
	DeactivateProgressFormEvents();

	if (InterruptedByUser) return;
	
	InterruptedByUser = false;
	infoWindow->setLabelText("Download completed! Installing ... (Please wait)");

	Glib::ustring ZippedCabbageFilename = wxGLOBAL->getCabbagePath();
	ZippedCabbageFilename.append("/CabbageLinux.zip");
	if(!Glib::file_test(ZippedCabbageFilename, Glib::FILE_TEST_EXISTS))
	{
		infoWindow->hideWindow();
		wxGLOBAL->ShowMessageBox("Cabbage Info: file not found.",
			                     "Cabbage update error",
			                     Gtk::BUTTONS_OK);
		return;
	}


	//1. DECOMPRESS THE DOWNLOADED FILE
	//Dir, program, arguments
	this->executeCommand(wxGLOBAL->getCabbagePath().c_str(),
	                     "unzip -o", 
	                     ZippedCabbageFilename.c_str());


	//Delete the zipped file
	infoWindow->setLabelText("Removing zipped file ...");
	if(Glib::file_test(ZippedCabbageFilename, Glib::FILE_TEST_EXISTS))
	{
		remove(ZippedCabbageFilename.c_str());
	}

	
	//Check the real unzipped folder name (could be different from the file name)
	Glib::ustring CabbageDirectory = wxGLOBAL->getCabbagePath();
	Glib::ustring UnzippedCabbageDirectory = "";

	Glib::Dir dir(CabbageDirectory);
	//foreach (string d in Directory.GetDirectories(CabbageDirectory))
	for(Glib::Dir::iterator p = dir.begin(); p != dir.end(); ++p)
	{
		Glib::ustring temp = *p;
				
		//if (d != Directory.GetParent(d) + "\\CabbageLinux" &&
		//    d != Directory.GetParent(d) + "\\CodeRepository")
		if(temp != "CabbageLinux" &&
		   temp != "CodeRepository" &&
		   temp != "version.txt" &&
		   !Glib::str_has_prefix(temp, ".") &&
		   !Glib::str_has_suffix(temp, "~"))                   
		{
			//Rename the ZippedCabbageFilename
			UnzippedCabbageDirectory = temp;
			break;
		}
	}

	std::cerr << "Unzipped dir: " << UnzippedCabbageDirectory << std::endl;


	//Rename the new unzipped folder to "CabbageLinux" (this is the default dirname)
	Glib::ustring CabbageNewPath = wxGLOBAL->getCabbagePath();
	CabbageNewPath.append("/CabbageLinux");
	Glib::ustring CabbageOldPath = CabbageNewPath;
	CabbageOldPath.append("_OLD");
	

	infoWindow->setLabelText("Renaming old folder ...");
	rename(CabbageNewPath.c_str(), CabbageOldPath.c_str());

	infoWindow->setLabelText("Renaming Unzipped folder to CabbageWin ...");
	rename(UnzippedCabbageDirectory.c_str(), CabbageNewPath.c_str());
	
	infoWindow->hideWindow();


	
	//UPDATE SUCCESSFULLY INSTALLED
	Glib::ustring executable = CabbageNewPath;
	executable.append("/Cabbage.exe"); //CABBAGE_NAME
	//if (File.Exists(Application.StartupPath + "\\Cabbage\\CabbageWin\\Cabbage.exe"))
	if(Glib::file_test(executable, Glib::FILE_TEST_EXISTS))
	{
		wxSETTINGS->Directory.CabbagePath = executable;
		wxSETTINGS->SaveSettings();		

		//SHOW THE FINAL STEP MESSAGE
		wxGLOBAL->ShowMessageBox("Cabbage updated successfully!",
			                     "Cabbage Information",
			                     Gtk::BUTTONS_OK);

		//Glib::file_set_contents params:
		//filename	- name of a file to write contents to, in the GLib file name encoding
		//contents	- string to write to the file
		//length	- length of contents, or -1 if contents is a nul-terminated string
		Glib::ustring currentVersionTimeTxt = CabbageDirectory;
		currentVersionTimeTxt.append("/CurrentVersionTime.txt");
		Glib::ustring content = wxGLOBAL->DoubleToString(webVersionNumber);
		Glib::file_set_contents(currentVersionTimeTxt,
		                        content.c_str(),
		                        content.size());

	}
	else //UPDATE FAILED
	{
		//Directory.Delete(CabbageNewPath, true);
		RemoveDirectory(CabbageNewPath.c_str());

		//Restore the old directory to the new one (from CabbageLinux_OLD to CabbageLinux)
		rename(CabbageOldPath.c_str(), CabbageNewPath.c_str());

		wxGLOBAL->ShowMessageBox("Cabbage update failed!",
			                     "Cabbage download error",
			                     Gtk::BUTTONS_OK);

	}


	//Perform a general CabbageDir cleanup!!!
	performCabbageDirectoryCleanup();
	
}

void wxCabbage::performCabbageDirectoryCleanup()
{
	//Perform a general CabbageDir cleanup!!!
	Glib::ustring CabbageDirectory = wxGLOBAL->getCabbagePath();
	Glib::Dir dir_final(CabbageDirectory);

	for(Glib::Dir::iterator p = dir_final.begin(); p != dir_final.end(); ++p)
	{
		Glib::ustring temp = *p;

		if(temp != "CabbageLinux" &&
		   temp != "CodeRepository" &&
		   temp != "CurrentVersionTime.txt")                   
		{
			//Remove temporary file
			temp.insert(0, "/");
			temp.insert(0, CabbageDirectory);

			//std::cerr << temp << std::endl;
			if(Glib::file_test(temp, Glib::FILE_TEST_IS_DIR))
			{
				//std::cerr << "was a dir" << std::endl;
				RemoveDirectory(temp.c_str());
				
			}
			else
				remove(temp.c_str());
		}
	}
}

void wxCabbage::RemoveDirectory(Glib::ustring dirname)
{
	executeCommand(wxGLOBAL->getCabbagePath().c_str(),
	               "rm -r", //-rf
	               dirname.c_str());
}

gdouble wxCabbage::GetVersionNumber(Glib::ustring versionText)
{
	gchar** value = g_strsplit(versionText.c_str(), "_", 0);
	Glib::ustring text = value[1];
	Glib::ustring number = "";

	/*
	for (guint i = 0; i < text.size(); i++)
	{
		if(isdigit(text[i]))
		{
			number += text[i];
		}
		else break;
	}
	*/

	number = text.substr(0, text.size() - 4);

	g_strfreev(value);

	return wxGLOBAL->StringToDouble(number);
}

void wxCabbage::CreateProgressDialog()
{
	/*
	gint infoWindowWidth = 400;
	gint infoWindowHeight= 100;
	
	infoWindow = new Gtk::Window();
	infoWindow->set_title("Downloading Cabbage ...");

	Glib::ustring iconfile = wxGLOBAL->getIconsPath();
	iconfile.append("/winxound_48.png");
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
		infoWindow->set_icon_from_file(iconfile);

	labelInfo = new Gtk::Label("Percentage: 0%");
	infoWindow->add(*labelInfo);
	infoWindow->show_all_children();

	infoWindow->set_size_request(infoWindowWidth,infoWindowHeight);;
	infoWindow->set_resizable(FALSE);
	//infoWindow->signal_hide().connect(
	//	sigc::mem_fun(*this, &wxCabbage::on_compilerWindow_closed)); 



	gint width = infoWindow->get_screen()->get_width();
	gint height = infoWindow->get_screen()->get_height();
	gint x = (width / 2) - (infoWindowWidth / 2);
	gint y = (height / 2) - (infoWindowHeight / 2);
	
	infoWindow->move(x,y);
	*/
	
	/*
	//SET DIALOG POSITION (CENTER PARENT)
	gint x,y;
	wxMainWindow->get_position(x, y);
	x += wxMainWindow->get_width() / 2;
	y += wxMainWindow->get_height() / 2;
	dialog.move(x - dialog.get_width() / 2,
	            y - dialog.get_height() / 2);
	*/

	

}

void wxCabbage::ActivateProgressFormEvents()
{
	close_signal_connection = infoWindow->signal_hide().connect(
		sigc::mem_fun(*this, &wxCabbage::on_compilerWindow_closed)); 
}
void wxCabbage::DeactivateProgressFormEvents()
{
	 //g_signal_handler_disconnect(window, handler_id);
	close_signal_connection.disconnect();

}




void wxCabbage::on_compilerWindow_closed()
{
	DeactivateProgressFormEvents();
	
	if(download != NULL)
		webkit_download_cancel(download);

	InterruptedByUser = true;

	wxGLOBAL->ShowMessageBox("Cabbage download aborted.",
			                 "Cabbage update",
			                 Gtk::BUTTONS_OK);

	
	Glib::ustring downloadPath = wxGLOBAL->getCabbagePath();
	downloadPath.append("/CabbageLinux.zip");
	if(Glib::file_test(downloadPath, Glib::FILE_TEST_EXISTS))
	{
		remove(downloadPath.c_str());
	}
}

//**************************************************
//* executes a command
//* returns TRUE if command was executed  (not the result of the command though..)
bool wxCabbage::executeCommand(const gchar* path, const gchar* cmd, const gchar* args)
{
	return this->executeCommand(path, cmd, args, false);
}
bool wxCabbage::executeCommand(const gchar* path, const gchar* cmd, const gchar* args, bool Async)
{
	gchar     cmd_line[256];
	gchar   **argv;
	gint      argp;
	bool      rc = FALSE;

	if (cmd == NULL)
		return FALSE;

	if (cmd[0] == '\0')
		return FALSE;

	if (path != NULL)
		gint z = chdir (path);

	snprintf (cmd_line, sizeof (cmd_line), "%s %s", cmd, args);

	rc = g_shell_parse_argv (cmd_line, &argp, &argv, NULL);
	if (!rc)
	{
		g_strfreev (argv);
		return rc;
	}


	/*g_spawn_sync
	(const gchar *working_directory,
	 gchar **argv,
	 gchar **envp,
	 GSpawnFlags flags,
	 GSpawnChildSetupFunc child_setup,
	 gpointer user_data,
	 gchar **standard_output,
	 gchar **standard_error,
	 gint *exit_status,
	 GError **error);
	*/

	if(Async)
	{
		rc = g_spawn_async (path, argv, NULL,
		                    GSpawnFlags(G_SPAWN_STDOUT_TO_DEV_NULL | G_SPAWN_SEARCH_PATH),
		                    NULL, NULL, NULL, NULL);
	}
	else
	{
		rc = g_spawn_sync(path, argv, NULL,
		                  GSpawnFlags(G_SPAWN_STDOUT_TO_DEV_NULL | G_SPAWN_SEARCH_PATH),
		                  NULL, NULL, NULL, NULL, NULL, NULL);
	}
	
	g_strfreev (argv);

	return rc;
}





//TEMP:
/*
 struct stat file_stat;
 stat(url.c_str(), &file_stat);

 std::cerr << "webVersionName date: " << file_stat.st_mtime
	 << "Size: " << file_stat.st_size << std::endl;

 struct tm  *ts;
 char buf[100];
 ts = localtime(&file_stat.st_mtime);	
 strftime(buf, sizeof(buf), "%a %Y-%m-%d %H:%M:%S %Z", ts);
 printf("%s\n", buf);
 */	