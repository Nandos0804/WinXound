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

#include "wx-pipe.h"
#include "wx-global.h"
#include <iostream>
#include <fstream>
#include <fcntl.h>
#include <stdint.h>




//#include <glibmm/iochannel.h>


wxPipe::wxPipe()  	
{
	/*
	if (access("testfifo", F_OK) == -1) {
		// fifo doesn't exit - create it
		if (mkfifo("testfifo", 0666) != 0) {
			std::cerr << "error creating fifo" << std::endl;
			return -1;
		}
	}
	*/

	fifoIN = -1;
	fifoOUT = -1;
	
	// connect the signal handler
	//Glib::signal_io().connect(sigc::mem_fun(*this, &wxPipe::MyCallback), fifoIN, Glib::IO_IN);

	// Creates a iochannel from the file descriptor
	//iochannelIN = Glib::IOChannel::create_from_fd(fifoIN);


	// now remove the temporary fifo
	//if(unlink("testfifo"))
	//	std::cerr << "error removing fifo" << std::endl;



}

wxPipe::~wxPipe()
{
	//wxGLOBAL->DebugPrint("wxPipe released");
}


void wxPipe::UpdateCabbage(const char* filename)
{

	//WRITE CHANNEL
	fifoOUT = open("/tmp/cabbage_in", O_WRONLY);
	//iochannelOUT = Glib::IOChannel::create_from_fd(fifoOUT);
	if(fifoOUT != -1)
	{
		//gsize bytesWritten = 0;
		const gchar* message = filename;//"PROVA";
		//const int messageMagicNumber = 0xb734128b;
		//guint32 messageMagicNumber = 0xb734128b; //4071923244U;
		//guint32 messageLength = (guint32)sizeof(message);
		
		guint32 messageHeader[2];
		messageHeader [0] = (guint32)4071923244;
		messageHeader [1] = (guint32)strlen(message);
	
		//MemoryBlock messageData (sizeof (messageHeader) + message.getSize());
		gint bufferLength = sizeof(messageHeader) + strlen(message);
	    gchar* buffer = new gchar[bufferLength];
		memset (buffer,'\0', bufferLength);

		memcpy(buffer, &messageHeader, sizeof(messageHeader));
		////copyFrom (buffer, message, sizeof(messageHeader), strlen(message));
		memcpy(buffer + 8, message, strlen(message));
		std::cout << "Size of MessageHeader: " << sizeof(messageHeader)
				  << "\nSize of Message: " << strlen(message)
			      << "\nData sent: " << buffer + 8 << std::endl;

		
		//iochannelOUT->write(data.c_str(), data.size(), bytesWritten);
		//iochannelOUT->write(buffer, strlen(buffer), bytesWritten);
		//iochannelOUT->close();

		write(fifoOUT, buffer, bufferLength);
		close(fifoOUT);
		delete[] buffer;
		
		std::cout << "Update Cabbage" << std::endl;
	}


	/*
	if(fifoIN == nil && [[NSFileManager defaultManager] fileExistsAtPath:@"/tmp/cabbage_out"])
	{
		//fifoIN = [[NSFileHandle fileHandleForReadingAtPath:@"/tmp/cabbage_out"] retain];
		const char * path = "/tmp/cabbage_out";
		int fd = open(path, O_RDONLY | O_NDELAY); //O_RDWR
		fifoIN = [[NSFileHandle alloc] initWithFileDescriptor:fd closeOnDealloc: YES]; //retain];
		
		[[NSNotificationCenter defaultCenter] removeObserver:self name:NSFileHandleReadCompletionNotification object:fifoIN];
		[[NSNotificationCenter defaultCenter] addObserver:self
												 selector:@selector(dataReady:)
													 name:NSFileHandleReadCompletionNotification
												   object:fifoIN];
		[fifoIN readInBackgroundAndNotify];
		
		NSLog(@"fifoIN created");
	}
	*/

	//READ CHANNEL
	if(fifoIN == -1)
	{
		fifoIN = open("/tmp/cabbage_out", O_RDONLY | O_NDELAY);
		//iochannelIN = Glib::IOChannel::create_from_fd(fifoIN);
		std::cout << "fifoIN Created: " << fifoIN << std::endl;
		//return;
		if (fifoIN != -1)
		{
			Glib::signal_io().connect(sigc::mem_fun(*this, &wxPipe::MyCallback), fifoIN, Glib::IO_IN);
		}
		else
			std::cerr << "error opening fifoIN" << std::endl;
	}
}

unsigned long int wxPipe::Endian_DWord_Conversion(unsigned long int dword) 
{
   return ((dword>>24)&0x000000FF) | 
		  ((dword>>8)&0x0000FF00) | 
		  ((dword<<8)&0x00FF0000) | 
		  ((dword<<24)&0xFF000000);
}




// this will be our signal handler for read operations
// it will print out the message sent to the fifo
// and quit the program if the message was 'Q'.
bool wxPipe::MyCallback(Glib::IOCondition io_condition)
{
	std::cout << "CALLBACK" << std::endl;
	//return false;
	
	if ((io_condition & Glib::IO_IN) == 0) 
	{
		std::cerr << "Invalid fifo response" << std::endl;
	}
	else 
	{
		//Glib::ustring buf;
		//iochannelIN->read_line(buf);
		//std::cout << buf;

		char buf[4096];
		gint numread = read(fifoIN, buf, 4096);
		buf[numread] = '\0';

		std::cout << "Data received: " << buf + 8 << std::endl;

		//Not needed:
		//Glib::signal_io().connect(sigc::mem_fun(*this, &wxPipe::MyCallback), fifoIN, Glib::IO_IN);
	}
	
	return true;
}







