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

#include <iostream>
#include <fstream>

#include "wx-global.h"
#include "wx-editor.h"
#include <glib/gstdio.h>


using namespace Glib;



// Static data must be initialized.
wxGlobal* wxGlobal::instance = NULL;

wxGlobal* wxGlobal::getInstance(void)
{

	/*
	 * The first time getInstance() is called, create the wxSettings.
		 * Thereafter, just return a pointer to the already created one.
	 */
	if (instance == NULL)
	{
		instance = new wxGlobal();
	}

	return instance;
}

wxGlobal::wxGlobal()
{
	
}

wxGlobal::~wxGlobal()
{
	std::cout << "wxGlobal released" << std::endl;
}



bool wxGlobal::isSyntaxType(wxEditor* editor)
{
	bool ret = false;
	Glib::ustring temp;
	
	if (editor != NULL)
	{
		temp = editor->FileName.lowercase();
		
		if (Glib::str_has_suffix(temp.lowercase(), ".csd") ||
		    Glib::str_has_suffix(temp.lowercase(), ".orc") ||
		    Glib::str_has_suffix(temp.lowercase(), ".sco") ||
		    Glib::str_has_suffix(temp.lowercase(), ".inc") ||
		    Glib::str_has_suffix(temp.lowercase(), ".udo") ||
		    Glib::str_has_suffix(temp.lowercase(), ".py") ||
		    Glib::str_has_suffix(temp.lowercase(), ".pyw") ||
		    Glib::str_has_suffix(temp.lowercase(), ".lua"))
		{
			ret = true;
		}
	}
		
	return ret;
}

Glib::ustring wxGlobal::BoolToString(bool  b)
{ 
	return b ? "true" : "false"; 
}

bool wxGlobal::StringToBool(Glib::ustring value)
{
	return (value.lowercase() == "true");
}

Gdk::Point wxGlobal::StringToPoint(Glib::ustring value)
{
	gchar** valueArray = g_strsplit(value.c_str(), ",", 0);
	int x = atoi(valueArray[0]);
	int y = atoi(valueArray[1]);
	return Gdk::Point(x, y);
}

Glib::ustring wxGlobal::PointToString(Gdk::Point p)
{
	Glib::ustring temp = "";
	temp.append(IntToString(p.get_x()));
	temp.append(",");
	temp.append(IntToString(p.get_y()));
	
	return temp;
}

Glib::ustring wxGlobal::IntToString(int value)
{
	std::stringstream ss;
	ss << value;   
	return (gchar*)ss.str().c_str();
	//return ss.str();
}

gdouble wxGlobal::StringToDouble(Glib::ustring text)
{
	std::istringstream stream(text);
	gdouble t;
	stream >> t;
	return t;
}

Glib::ustring wxGlobal::DoubleToString(gdouble value)
{
	std::ostringstream oss;
	oss << value;
	return oss.str();
}

Glib::ustring wxGlobal::Trim(Glib::ustring text)
{
	return TrimLeft(TrimRight(text));
}


// trim from start
Glib::ustring wxGlobal::TrimLeft(Glib::ustring s) 
{
	s.erase(s.begin(), 
	        std::find_if(s.begin(), 
	                     s.end(), 
	                     std::not1(std::ptr_fun<int, int>(std::isspace))));
	return s;
}

// trim from end
Glib::ustring wxGlobal::TrimRight(Glib::ustring s) 
{
	s.erase(std::find_if(s.rbegin(), 
	                     s.rend(), 
	                     std::not1(std::ptr_fun<int, int>(std::isspace))).base(), 
	        s.end());
	return s;
}

Glib::ustring wxGlobal::RemoveDoubleSpaces(Glib::ustring s)
{
	try
	{
		while (true)
		{
			if(s.find("  ") != Glib::ustring::npos)
				s.erase(s.find("  "), 1);
			else
				break;
		}
	}
	catch (...) {}
	
	return s;
}


void wxGlobal::DebugPrint(const gchar* message)
{
	std::cout << message << std::endl;
}

void wxGlobal::DebugPrint(const gchar* message, const gchar* description)
{
	std::cout << message << " : " << description << std::endl;
}






////////////////////////////////////////////////////////////////////////////////
//gchar *package_prefix = PACKAGE_PREFIX; 
//gchar *package_data_dir = PACKAGE_DATA_DIR; 
//gchar *package_locale_dir = PACKAGE_LOCALE_DIR;

//Glib::get_user_config_dir():
//Returns a base directory in which to store user-specific application configuration 
//information such as user preferences and settings.

//DIRECTORIES !!!
Glib::ustring wxGlobal::getExePath()
{
	////wxGLOBAL->DebugPrint("getExePath -> CURRENT DIR", Glib::get_current_dir().c_str());
	char result[PATH_MAX];
	ssize_t count = readlink( "/proc/self/exe", result, PATH_MAX );
	return std::string( result, (count > 0) ? count : 0 );
}

Glib::ustring wxGlobal::getIconsPath()
{
	
	Glib::ustring iconPath = Glib::path_get_dirname(getExePath());
	iconPath.append("/src");

	if(!Glib::file_test(iconPath, Glib::FILE_TEST_EXISTS))
	{
		//return "/usr/local/share/winxound/src";
		iconPath = PACKAGE_DATA_DIR;
		iconPath.append("/winxound/src");
	}
	
	//DebugPrint("ICON_PATH",iconPath.c_str());
	return iconPath;
}

Glib::ustring wxGlobal::getSettingsPath()
{
	//return "/home/xyz/.config/winxound/Settings";
	//Glib::ustring settingsPath = g_get_user_config_dir();
	Glib::ustring settingsPath = Glib::path_get_dirname(getExePath());
	settingsPath.append("/Settings");

	if(!Glib::file_test(settingsPath, Glib::FILE_TEST_EXISTS))
	{
		//return "/home/teto/winxound/Settings";
		settingsPath = getHomePath();
		settingsPath.append("/winxound/Settings");
	}
	
	//DebugPrint("SETTINGS_PATH", settingsPath.c_str());
	return settingsPath;
}

Glib::ustring wxGlobal::getRepositoryPath()
{
	//return "/home/xyz/.config/winxound/Repository";
	//Glib::ustring repositoryPath = g_get_user_config_dir();
	Glib::ustring repositoryPath = Glib::path_get_dirname(getExePath());
	repositoryPath.append("/Repository");

	if(!Glib::file_test(repositoryPath, Glib::FILE_TEST_EXISTS))
	{
		//return "/home/teto/winxound/Repository";
		repositoryPath = getHomePath();
		repositoryPath.append("/winxound/Repository");
	}
	
	//DebugPrint("REPOSITORY_PATH", repositoryPath.c_str());
	return repositoryPath;
}

Glib::ustring wxGlobal::getHelpPath()
{
	//return "/home/xyz/.config/winxound/Help";
	//Glib::ustring helpPath = g_get_user_config_dir();
	Glib::ustring helpPath = Glib::path_get_dirname(getExePath());
	helpPath.append("/Help");

	if(!Glib::file_test(helpPath, Glib::FILE_TEST_EXISTS))
	{
		//return "/home/teto/winxound/Help";
		helpPath = getHomePath();
		helpPath.append("/winxound/Help");
	}
	
	//DebugPrint("HELP_PATH", helpPath.c_str());
	return helpPath;
}

Glib::ustring wxGlobal::getSrcPath()
{
	//return "/usr/local/share/winxound/src";
	//Glib::ustring helpPath = g_get_user_config_dir();
	Glib::ustring srcPath = Glib::path_get_dirname(getExePath());
	srcPath.append("/src");

	if(!Glib::file_test(srcPath, Glib::FILE_TEST_EXISTS))
	{
		//return "/usr/local/share/winxound/src";
		srcPath = PACKAGE_DATA_DIR;
		srcPath.append("/winxound/src");
	}
	
	//DebugPrint("SRC_PATH", srcPath.c_str());
	return srcPath;
}

Glib::ustring wxGlobal::getCabbagePath()
{
	//return "/usr/local/share/winxound/Cabbage";
	//Glib::ustring helpPath = g_get_user_config_dir();
	Glib::ustring cabbagePath = Glib::path_get_dirname(getExePath());
	cabbagePath.append("/Cabbage");

	if(!Glib::file_test(cabbagePath, Glib::FILE_TEST_EXISTS))
	{
		//return "/usr/local/share/winxound/Cabbage";
		cabbagePath = PACKAGE_DATA_DIR;
		cabbagePath.append("/winxound/Cabbage");
	}
	
	//DebugPrint("CABBAGE_PATH", cabbagePath.c_str());
	return cabbagePath;
}

Glib::ustring wxGlobal::getCabbageRepositoryPath()
{
	//return "/home/xyz/.config/winxound/Cabbage/CodeRepository";
	//Glib::ustring cabbageRepositoryPath = g_get_user_config_dir();
	Glib::ustring cabbageRepositoryPath = this->getCabbagePath();//Glib::path_get_dirname(getExePath());
	cabbageRepositoryPath.append("/CodeRepository");

	if(!Glib::file_test(cabbageRepositoryPath, Glib::FILE_TEST_EXISTS))
	{
		//return "/home/teto/winxound/Repository";
		cabbageRepositoryPath = getHomePath();
		cabbageRepositoryPath.append("/winxound/Cabbage/CodeRepository");
	}
	
	//DebugPrint("CABBAGE_REPOSITORY_PATH", cabbageRepositoryPath.c_str());
	return cabbageRepositoryPath;
}




//MAYBE TO REMOVE!!!
void wxGlobal::CheckWinXoundDirectories()
{
	if(!Glib::file_test(getSettingsPath(), Glib::FILE_TEST_EXISTS))
	{
		g_mkdir_with_parents(getSettingsPath().c_str(), 0700);
	}
	if(!Glib::file_test(getRepositoryPath(), Glib::FILE_TEST_EXISTS))
	{
		g_mkdir_with_parents(getRepositoryPath().c_str(), 0700);
	}
	if(!Glib::file_test(getHelpPath(), Glib::FILE_TEST_EXISTS))
	{
		g_mkdir_with_parents(getHelpPath().c_str(), 0700);
	}
}

////////////////////////////////////////////////////////////////////////////////







int wxGlobal::ShowMessageBox(const char* text, const char* title, Gtk::ButtonsType type)
{
	Gtk::MessageType dialogType = Gtk::MESSAGE_INFO;
	if (type & Gtk::BUTTONS_YES_NO)
        dialogType = Gtk::MESSAGE_QUESTION;
    else
        dialogType = Gtk::MESSAGE_INFO;

	
	Gtk::MessageDialog dialog(text,
	                          false, 
	                          //Gtk::MESSAGE_QUESTION,
	                          dialogType,
	                          //Gtk::BUTTONS_NONE);
	                          type);

	//dialog.set_secondary_text("And this is the secondary text that explains things.");
	dialog.set_title(title);
	
	int result = dialog.run();
	return result;
}

int wxGlobal::ShowMessageBox(Gtk::Window* ownerWindow, const char* text, const char* title, Gtk::ButtonsType type)
{
	//Glib::ustring message = "Document '";
	//message	+= "\n' has changed!\nDo you want to save it?";

	Gtk::MessageType dialogType = Gtk::MESSAGE_INFO;
	if (type & Gtk::BUTTONS_YES_NO)
        dialogType = Gtk::MESSAGE_QUESTION;
    else
        dialogType = Gtk::MESSAGE_INFO;

	
	Gtk::MessageDialog dialog(*ownerWindow, 
	                          text,
	                          false, 
	                          //Gtk::MESSAGE_QUESTION,
	                          dialogType,
	                          //Gtk::BUTTONS_NONE);
	                          type);

	//dialog.set_secondary_text("And this is the secondary text that explains things.");
	dialog.set_title(title);
	
	int result = dialog.run();
	return result;
}



GHashTable* wxGlobal::LoadOpcodes()
{
	std::string line;
	//Glib::ustring file = getSettingsPath();
	Glib::ustring file = getSrcPath();
	file.append("/opcodes.txt");

	GHashTable* opcodes = g_hash_table_new(g_str_hash, g_str_equal);
	if(!Glib::file_test(file, Glib::FILE_TEST_EXISTS)) return opcodes;

	try
	{
		std::ifstream myfile (file.c_str());

		Glib::ustring key;
		Glib::ustring value;
		Glib::ustring temp;

		if (myfile.is_open())
		{
			while (!myfile.eof() )
			{
				key="";
				value="";
				//std::cout << line << std::endl;

				//mString = reader.ReadLine();			
				getline (myfile,line);
				//if (string.IsNullOrEmpty(mString)) continue;
				if(line == "") continue;


				gchar* t = strtok((gchar*)line.c_str(),";");
				if (t != NULL)
				{
					temp = t;
					key = temp.substr(1,temp.length()-2);
				}

				t = strtok (NULL, ";");
				if (t != NULL)
				{
					temp = t;
					value = temp.substr(1,temp.length()-2);
				}

				t = strtok (NULL, ";");
				if (t != NULL)
				{
					value.append("|");
					temp = t;
					value.append(temp.substr(1,temp.length()-2));
				}

				t = strtok (NULL, ";");
				if (t != NULL)
				{
					value.append("|");
					temp = t;
					value.append(temp.substr(1,temp.length()-3));
				}

				gchar* k = new gchar[key.size()+1];
				strcpy (k, key.c_str());

				gchar* v = new gchar[value.size()+1];
				strcpy (v, value.c_str());

				//gchar* k = key.c_str();
				//gchar* v = value.c_str();

				if(opcodes != NULL)
					g_hash_table_insert(opcodes, k, v);

			}
			
			myfile.close();
		}
	}
	catch (...)
	{
		return opcodes;
	}

	return opcodes;

}

bool wxGlobal::FileIsReadable(const gchar* filename)
{
	//struct stat results;  
	//stat(filename, &results);
	//return (results.st_mode & S_IRUSR);

	Glib::RefPtr<Gio::File> gioFile = Gio::File::create_for_path(filename);
	Glib::RefPtr<Gio::FileInfo> info = gioFile->query_info();
	return info->get_attribute_boolean("access::can-read");
}

bool wxGlobal::FileIsWritable(const gchar* filename)
{
	//struct stat results;  
	//stat(filename, &results);
	//return (results.st_mode & S_IWUSR);
	
	Glib::RefPtr<Gio::File> gioFile = Gio::File::create_for_path(filename);
	Glib::RefPtr<Gio::FileInfo> info = gioFile->query_info();
	return info->get_attribute_boolean("access::can-write");
	//std::cout << ret << std::endl;
	//return ret;
}

bool wxGlobal::FileIsExecutable(const gchar* filename)
{
	//struct stat results;
	//stat(filename, &results);
	//return (results.st_mode & S_IXUSR);

	Glib::RefPtr<Gio::File> gioFile = Gio::File::create_for_path(filename);
	Glib::RefPtr<Gio::FileInfo> info = gioFile->query_info();
	return info->get_attribute_boolean("access::can-execute");
}

int wxGlobal::ArrayLength(gchar** array)
{

    int length = 0;
   
    for (int i = 0; ; i++)
	{
		if(array[i] == NULL) break;
        length++;
	}   
    return length;
}

Glib::ustring wxGlobal::getHomePath()
{
	//TODO: CHECK !!!
	/*
	//1. Pay attention to HOME
	Glib::ustring path = Glib::getenv("HOME");
	if (path == "")
		path = Glib::get_home_dir();
	*/

	//2. NORMAL GET HOME DIR
	Glib::ustring path = Glib::get_home_dir();
	//Glib::ustring path = Glib::get_user_special_dir(G_USER_DIRECTORY_DESKTOP);

	return path;
}

guint wxGlobal::StringToHex(Glib::ustring str)
{
	//int c = std::strtol(color.c_str(), NULL, 16);
	
	guint c;
	std::stringstream ss;
	ss << std::hex << str.c_str();
	ss >> c;
	
	return c;
}

Glib::ustring wxGlobal::HexToString(Gdk::Color color)
{
        gchar* rgb;
        gfloat r, g, b;

        r=color.get_red();
        g=color.get_green();
        b=color.get_blue();

        r=(r/65535)*255;
        g=(g/65535)*255;
        b=(b/65535)*255;

        rgb = g_strdup_printf("#%02x%02x%02x",(gint) r,(gint) g,(gint) b);

        return rgb;
}

Glib::ustring wxGlobal::getFileExtension(Glib::ustring filename)
{
	size_t index = filename.rfind('.');

	if(index != Glib::ustring::npos)
	{
		Glib::ustring extension = filename.substr(index);
		return extension;
	}

	return "";
}

Glib::ustring wxGlobal::getFileNameWithoutExtension(Glib::ustring filename)
{
	size_t index = filename.rfind('.');

	if(index != Glib::ustring::npos)
	{
		Glib::ustring newFilename = filename.substr(0, index);
		return newFilename;
	}

	return filename;
}
