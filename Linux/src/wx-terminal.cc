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

#include "wx-terminal.h"
#include <iostream>
#include <vte/vte.h> 
#include "wx-global.h"
#include "wx-settings.h"
//#include <sys/wait.h> 

#include <fstream>
#include <vector>

#ifdef WINXOUND_MODERN_DEPS
static pid_t vte_terminal_fork_command_compat(VteTerminal* terminal,
	                                          const char* command,
	                                          char** argv,
	                                          char** envv,
	                                          const char* directory,
	                                          gboolean /*lastlog*/,
	                                          gboolean /*utmp*/,
	                                          gboolean /*wtmp*/)
{
	const char* spawn_dir = directory ? directory : Glib::get_home_dir().c_str();
	const char* spawn_cmd = command ? command : g_getenv("SHELL");
	if(spawn_cmd == NULL)
		spawn_cmd = "/bin/sh";

	std::vector<char*> local_argv;
	if(argv != NULL)
	{
		for(int i = 0; argv[i] != NULL; ++i)
			local_argv.push_back(argv[i]);
	}
	if(local_argv.empty())
		local_argv.push_back(const_cast<char*>(spawn_cmd));
	local_argv.push_back(NULL);

	GPid child_pid = -1;
	GError* err = NULL;
	vte_terminal_spawn_sync(terminal,
	                        VTE_PTY_DEFAULT,
	                        spawn_dir,
	                        local_argv.data(),
	                        envv,
	                        G_SPAWN_SEARCH_PATH,
	                        NULL,
	                        NULL,
	                        &child_pid,
	                        NULL,
	                        &err);
	if(err != NULL)
	{
		g_error_free(err);
		return -1;
	}
	return child_pid;
}

#define vte_terminal_fork_command vte_terminal_fork_command_compat
#define vte_terminal_write_contents vte_terminal_write_contents_sync
#define VTE_TERMINAL_WRITE_DEFAULT VTE_WRITE_DEFAULT
#endif


wxTerminal::wxTerminal(bool isCompiler)  	
{
	paused = false;
	ProcessActive = false;
	currentOutputPath = "";
	currentFilename = "";
	mIsCompiler = isCompiler;
	
	
	if(isCompiler)
		this->CreateNewCompiler();
	else
		this->CreateNewTerminal();

	if(vte != NULL)
	{
		//std::cerr << pango_font_description_to_string(vte_terminal_get_font(VTE_TERMINAL(vte))) << std::endl;
		/*
		PangoFontDescription *pfd = pango_font_description_new();
		{
			pango_font_description_set_family(pfd, "!Monospace"); //wxSETTINGS->General.CompilerFontName.c_str());
			pango_font_description_set_size(pfd, 10.0); //wxSETTINGS->General.CompilerFontSize);
			vte_terminal_set_font(VTE_TERMINAL(vte), pfd);
		}
		pango_font_description_free(pfd);
		*/

		/*
		//Default: "Monospace 10"
		Glib::ustring font = wxSETTINGS->General.CompilerFontName;
		font.append(" ");
		font.append(wxGLOBAL->IntToString(wxSETTINGS->General.CompilerFontSize));
		vte_terminal_set_font_from_string(VTE_TERMINAL(vte), font.c_str());
		*/

		SetCompilerFont(wxSETTINGS->General.CompilerFontName,
		                wxSETTINGS->General.CompilerFontSize);
		
	}
                     
	wxGLOBAL->DebugPrint("wxTerminal created");
}

wxTerminal::~wxTerminal(void)
{
	gtk_widget_destroy(vte);
	gtk_widget_destroy(frame);
	
	wxGLOBAL->DebugPrint("wxTerminal released");
}

bool wxTerminal::CreateNewCompiler()
{

	long size;
    char *buf;
    char *dir;

	size = pathconf(".", _PC_PATH_MAX);
    if ((buf = (char *)malloc((size_t)size)) != NULL) dir = getcwd(buf, (size_t)size); 

	vte = vte_terminal_new();

	//GtkWidget* scrollbar;
	//GtkAdjustment* adj = GTK_ADJUSTMENT(VTE_TERMINAL(vte)->adjustment);
	//gtk_adjustment_set_lower(GTK_ADJUSTMENT(VTE_TERMINAL(vte)->adjustment), 1);
	scrollbar = gtk_scrollbar_new(GTK_ORIENTATION_VERTICAL,
	                              gtk_scrollable_get_vadjustment(GTK_SCROLLABLE(vte)));
	gtk_widget_set_can_focus(scrollbar, FALSE);
	gtk_widget_set_can_focus(vte, FALSE);
	//GTK_WIDGET_SET_FLAGS(vte, GTK_CAN_FOCUS);

	/* set the default widget size first to prevent VTE expanding too much,
	 * sometimes causing the hscrollbar to be too big or out of view. */
	gtk_widget_set_size_request(GTK_WIDGET(vte), 10, 10);
	vte_terminal_set_size(VTE_TERMINAL(vte), 30, 1);	

	GtkWidget* hbox = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 1);
	GtkWidget* vbox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 1);
	
	buttonStop = gtk_button_new_with_label("Stop (Esc)");
	buttonPause = gtk_button_new_with_label("Pause");
	buttonPanic = gtk_button_new_with_label("Kill process");
	buttonSave = gtk_button_new_with_label("Save output");
	gtk_widget_set_size_request(GTK_WIDGET(buttonStop), -1, 28);
	gtk_widget_set_size_request(GTK_WIDGET(buttonPause), -1, 28);
	gtk_widget_set_size_request(GTK_WIDGET(buttonPanic), -1, 28);
	gtk_widget_set_size_request(GTK_WIDGET(buttonSave), -1, 28);
	gtk_widget_set_can_focus(buttonPanic, FALSE);
	
	GtkWidget* vboxButtons = gtk_box_new(GTK_ORIENTATION_VERTICAL, 1);
	gtk_box_pack_start(GTK_BOX(vboxButtons), buttonStop, FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(vboxButtons), buttonPause, FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(vboxButtons), gtk_label_new(""), FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(vboxButtons), buttonPanic, FALSE, FALSE, 0);
	//gtk_box_pack_start(GTK_BOX(vboxButtons), gtk_label_new(""), FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(vboxButtons), buttonSave, FALSE, FALSE, 0);

	gtk_box_pack_start(GTK_BOX(vbox), vboxButtons, FALSE, FALSE, 0);
	//gtk_box_pack_start(GTK_BOX(vbox), buttonStop, FALSE, FALSE, 0);
	//gtk_box_pack_start(GTK_BOX(vbox), buttonPause, FALSE, FALSE, 0);
	//gtk_box_pack_start(GTK_BOX(vbox), gtk_label_new(""), TRUE, TRUE, 0);


	gtk_box_pack_start(GTK_BOX(hbox), vbox, FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(hbox), vte, TRUE, TRUE, 0);
	gtk_box_pack_start(GTK_BOX(hbox), scrollbar, FALSE, FALSE, 0);


	GtkWidget* preframe = gtk_box_new(GTK_ORIENTATION_VERTICAL, 1);
	GtkWidget* minilabel = gtk_label_new("");
	gtk_widget_set_size_request(GTK_WIDGET(minilabel), 0, 1);
	gtk_box_pack_start(GTK_BOX(preframe), 
	                   minilabel, FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(preframe), 
	                   hbox, TRUE, TRUE, 0);
	
	//GtkWidget* frame = gtk_frame_new(NULL);
	frame = gtk_event_box_new();
	gtk_container_add(GTK_CONTAINER(frame), preframe);

	
	
	
	ConfigureTerminalForCompiler();
	g_signal_connect(vte, 
	                 "child-exited",//"cursor-moved",//"contents-changed", //"child-exited",
	                 G_CALLBACK(&wxTerminal::on_child_exited),
	                 this);

	g_signal_connect(buttonStop, 
	                 "clicked",//"cursor-moved",//"contents-changed", //"child-exited",
	                 G_CALLBACK(&wxTerminal::on_buttonStop_clicked),
	                 this);

	g_signal_connect(buttonPause, 
	                 "clicked",//"cursor-moved",//"contents-changed", //"child-exited",
	                 G_CALLBACK(&wxTerminal::on_buttonPause_clicked),
	                 this);

	g_signal_connect(buttonPanic, 
	                 "clicked",//"cursor-moved",//"contents-changed", //"child-exited",
	                 G_CALLBACK(&wxTerminal::on_buttonPanic_clicked),
	                 this);

	g_signal_connect(buttonSave, 
	                 "clicked",//"cursor-moved",//"contents-changed", //"child-exited",
	                 G_CALLBACK(&wxTerminal::on_buttonSave_clicked),
	                 this);

	gtk_widget_set_sensitive (GTK_WIDGET (buttonStop), FALSE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonPause), FALSE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonPanic), FALSE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonSave), FALSE);
	gtk_button_set_label (GTK_BUTTON(buttonPause), "Pause");
	

	//vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, NULL, dir, TRUE, TRUE, TRUE);
	vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, NULL, "/usr/bin", TRUE, TRUE, TRUE);

	g_signal_connect(frame, 
	                 "size-allocate",//"size-request", //"check-resize",
	                 G_CALLBACK(&wxTerminal::on_check_resize),
	                 this);


	
	gtk_widget_show_all(frame);
	wid = Glib::wrap(frame);
	//gtk_widget_show_all(hbox);
	//wid = Glib::wrap(hbox);

	
	//TODO:THIS SEEMS TO RESOLVE THE FLICKER ISSUE FOR CABBAGE (TRUE IS THE DEFAULT)
	//But introduces some artifacts during drag!!!
	//gtk_widget_set_double_buffered(GTK_WIDGET(vte), FALSE); 

	return true;
}

void  wxTerminal::on_check_resize (GtkWidget      *widget,
								   GtkAllocation  *allocation,
                                   gpointer        data) 
{
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->check_resize();
}
void  wxTerminal::check_resize()
{
	GdkRectangle rect;
	gtk_widget_get_allocation(vte, &rect);
	//wxGLOBAL->DebugPrint("TERMINAL on_check_resize", wxGLOBAL->IntToString(rect.height).c_str()); 

	if (rect.height < 50)
		gtk_widget_hide(scrollbar);
	else
		gtk_widget_show(scrollbar);
}

	
bool wxTerminal::CreateNewTerminal()
{

	long size;
    char *buf;
    char *dir; 

	size = pathconf(".", _PC_PATH_MAX);
    if ((buf = (char *)malloc((size_t)size)) != NULL) dir = getcwd(buf, (size_t)size); 


	GtkWidget *hbox; //, *frame;
	vte = vte_terminal_new();
	
	scrollbar = gtk_scrollbar_new(GTK_ORIENTATION_VERTICAL,
	                              gtk_scrollable_get_vadjustment(GTK_SCROLLABLE(vte)));
	gtk_widget_set_can_focus(scrollbar, FALSE);
	/* set the default widget size first to prevent VTE expanding too much,
	 * sometimes causing the hscrollbar to be too big or out of view. */
	gtk_widget_set_size_request(GTK_WIDGET(vte), 10, 10);
	vte_terminal_set_size(VTE_TERMINAL(vte), 30, 1);

	//frame = gtk_frame_new(NULL);
	frame = gtk_event_box_new();
	hbox = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 1);
	gtk_box_pack_start(GTK_BOX(hbox), vte, TRUE, TRUE, 0);
	gtk_box_pack_start(GTK_BOX(hbox), scrollbar, FALSE, FALSE, 0);

	
	GtkWidget* preframe = gtk_box_new(GTK_ORIENTATION_VERTICAL, 1);
	GtkWidget* minilabel = gtk_label_new("");
	gtk_widget_set_size_request(GTK_WIDGET(minilabel), 0, 1);
	gtk_box_pack_start(GTK_BOX(preframe), 
	                   minilabel, FALSE, FALSE, 0);
	gtk_box_pack_start(GTK_BOX(preframe), 
	                   hbox, TRUE, TRUE, 0);


	
	gtk_container_add(GTK_CONTAINER(frame), preframe);
	//gtk_frame_set_shadow_type(GTK_FRAME(frame),GTK_SHADOW_ETCHED_IN);
	

	vte_terminal_set_scrollback_lines(VTE_TERMINAL(vte), -1);
	vte_terminal_set_scroll_on_keystroke(VTE_TERMINAL (vte), FALSE);
	vte_terminal_set_scroll_on_output(VTE_TERMINAL (vte), TRUE);
	vte_terminal_set_default_colors (VTE_TERMINAL (vte));


	g_signal_connect(vte, 
	                 "child-exited", //"commit", //"child-exited",
	                 G_CALLBACK(&wxTerminal::on_RestartTerminalPage),
	                 this);



	g_signal_connect(frame, 
	                 "size-allocate",//"size-request", //"check-resize",
	                 G_CALLBACK(&wxTerminal::on_check_resize),
	                 this);

	

	//vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, 
	//                          g_strsplit("OPCODEDIR64=/usr/lib64/csound/plugins64-5.2/", " ", 0), 
	//                          "/usr/bin", TRUE, TRUE,TRUE);
	//Glib::get_current_dir().c_str()
	//vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, NULL, "/usr/bin", TRUE, TRUE,TRUE);
	vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, NULL, 
	                          Glib::get_home_dir().c_str(), TRUE, TRUE,TRUE);

	
	gtk_widget_show_all(frame);
	wid = Glib::wrap(frame);

	return true;
}


void wxTerminal::RestartTerminalPage()
{   
	vte_terminal_reset(VTE_TERMINAL(vte), TRUE, TRUE);
	//vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, NULL, "/usr/bin", TRUE, TRUE,TRUE);
	vte_terminal_fork_command(VTE_TERMINAL(vte), NULL, NULL, NULL, 
		                      Glib::get_home_dir().c_str(), TRUE, TRUE,TRUE);
}

void wxTerminal::on_RestartTerminalPage(GtkWidget *widget, gpointer data)
{
	//wxGLOBAL->DebugPrint("TERMINAL", "RestartTerminalPage"); 
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->RestartTerminalPage();
}

Gtk::Widget* wxTerminal::getTerminalWidget()
{
	return wid;
}




/*
 pid_t               vte_terminal_fork_command           (VteTerminal *terminal,
                                                         const char *command,
                                                         char **argv, //arguments
                                                         char **envv, //environment vars
                                                         const char *directory,
                                                         gboolean lastlog,
                                                         gboolean utmp,
                                                         gboolean wtmp);
*/



void wxTerminal::Compile(Glib::ustring compilerName,
                         Glib::ustring parameters,
                         Glib::ustring filename1,
                         Glib::ustring filename2)
{
	Compile(compilerName, parameters, filename1, filename2, TRUE);
}

void wxTerminal::Compile(Glib::ustring compilerName,
                         Glib::ustring parameters,
                         Glib::ustring filename1,
                         Glib::ustring filename2,
                         bool ClearBuffer)
{

	if(ProcessActive) return;

	
	if(ClearBuffer) 
		vte_terminal_reset(VTE_TERMINAL(vte), TRUE, TRUE);
	else
	{
		AppendCompilerText("\r\n"
		                  //012345678901234567890123456789012345678901234567890123456789
		                   "------------------------------------------------------------"
		                   "\r\n\r\n");
	}

	
	ConfigureTerminalForCompiler();	
	
	//wxGLOBAL->DebugPrint(compilerName.c_str(), parameters.c_str());

		
	//1.SET ARGS
	////parameters.append(" ");
	gchar** parSep = NULL; ////g_strsplit(parameters.c_str(), " ", 0);
	gchar** argv = NULL;
	
	if(filename2 == "")
	{
		//We have parameters:
		if(wxGLOBAL->Trim(parameters).size() > 0)
		{	
			//parSep = g_strsplit(parameters.c_str(), " ", 0);
			//parSep = g_strsplit(wxGLOBAL->RemoveDoubleSpaces(parameters).c_str(), " ", 0);
			parSep = g_strsplit(wxGLOBAL->RemoveDoubleSpaces(parameters).c_str(), " -", 0);
			
			int length = StringLength(parSep);
			argv = g_new(gchar*, length + 3);
			argv[0] = g_strdup(compilerName.c_str());
			for(int i=0; i < length; i++)
			{
				if(strlen(parSep[i]) > 0)
				{
					if(i == 0) //First value
						argv[i+1] = g_strdup(parSep[i]);
					else
					{
						Glib::ustring temp = parSep[i];
						temp.insert(0, "-");
						argv[i+1] = g_strdup(temp.c_str());
					}
						
				}
				else
					argv[i+1] = g_strdup("");
			}
			argv[length+1] = g_strdup(filename1.c_str());
			argv[length+2] = NULL;
		}
		else
		{
			int length = 3;
			argv = g_new(gchar*, length);
			argv[0] = g_strdup(compilerName.c_str());
			argv[1] = g_strdup(filename1.c_str());
			argv[2] = NULL;
		}			
		
	}
	else //Needed for Analysis tool (second filename)
	{
		//We have parameters:
		if(wxGLOBAL->Trim(parameters).size() > 0)
		{
			//parSep = g_strsplit(parameters.c_str(), " ", 0);
			//parSep = g_strsplit(wxGLOBAL->RemoveDoubleSpaces(parameters).c_str(), " ", 0);
			parSep = g_strsplit(wxGLOBAL->RemoveDoubleSpaces(parameters).c_str(), " -", 0);
			
			int length = StringLength(parSep);
			argv = g_new(gchar*, length + 4);
			argv[0] = g_strdup(compilerName.c_str());
			for(int i = 0; i < length; i++)
			{
				//OLD: argv[i+1] = g_strdup(parSep[i]);

				if(strlen(parSep[i]) > 0)
				{
					if(i == 0) //First value
						argv[i+1] = g_strdup(parSep[i]);
					else
					{
						Glib::ustring temp = parSep[i];
						temp.insert(0, "-");
						argv[i+1] = g_strdup(temp.c_str());
					}
						
				}
				else
					argv[i+1] = g_strdup("");
			}
			argv[length+1] = g_strdup(filename1.c_str());
			argv[length+2] = g_strdup(filename2.c_str());
			argv[length+3] = NULL;
		}
		else
		{
			int length = 4;
			argv = g_new(gchar*, length);
			argv[0] = g_strdup(compilerName.c_str());
			argv[1] = g_strdup(filename1.c_str());
			argv[2] = g_strdup(filename2.c_str());
			argv[3] = NULL;
		}
	}


	
	//2. SET ENVIRONMENT FOR CSOUND
	//If SFDIR is not defined or SFDIR checkbox is unchecked 
	//we redirect the compiled soundfile to the csd file directory
	Glib::ustring environment = "";
	gchar** envv = NULL;

	
	//if(compilerName.find("csound") != Glib::ustring::npos)
	{
		Glib::ustring divider = "*?*";
		if (wxSETTINGS->Directory.SFDIR == "" ||
		    wxSETTINGS->Directory.UseSFDIR == false)
		{
			environment = "SFDIR=";
			environment.append(Glib::path_get_dirname(filename1));
		}
		else
		{
			environment = wxSETTINGS->Directory.SFDIR;
		}

		envv = g_strsplit(environment.c_str(), divider.c_str(), 0);
	}
	

	
	////////
	//DEBUG:
	if(argv != NULL)
	{
		std::cout << "ARGUMENTS:" << std::endl;
		for(int i=0; i < StringLength(argv); i++)
		{
			std::cout << argv[i] << std::endl;
		}
	}
	if(envv != NULL)
	{
		std::cout << "ENVIRONMENT:" << std::endl;
		for(int i=0; i < StringLength(envv); i++)
		{
			std::cout << envv[i] << std::endl;
		}
	}
	////////
	

	//Store filename
	currentFilename = Glib::path_get_basename(filename1);
	
	//3. COMPILE: FORK COMMAND
	pid = vte_terminal_fork_command(VTE_TERMINAL(vte), 
	                                compilerName.c_str(), 
	                                argv, envv, 
	                                NULL, 
	                                FALSE, FALSE, FALSE);


	//4. SET WIDGETS
	ProcessActive = true;
	gtk_widget_set_sensitive (GTK_WIDGET (buttonStop), TRUE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonPause), TRUE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonPanic), TRUE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonSave), FALSE);


	//5. Free array pointers
	g_strfreev(parSep);
	g_strfreev(argv);
	g_strfreev(envv);
}



int wxTerminal::StringLength(gchar** array)
{

    int length = 0;
   
    for (int i = 0; ; i++)
	{
		if(array[i] == NULL) break;
        length++;
	}   
    return length;
}

void wxTerminal::ConfigureTerminalForCompiler()
{
	//gtk_widget_set_double_buffered(GTK_WIDGET(vte), TRUE);
	//gtk_widget_set_redraw_on_allocate(GTK_WIDGET(vte), TRUE); 
	vte_terminal_set_scrollback_lines(VTE_TERMINAL(vte), -1);
	vte_terminal_set_scroll_on_keystroke(VTE_TERMINAL (vte), FALSE);
	vte_terminal_set_scroll_on_output(VTE_TERMINAL (vte), TRUE);
	vte_terminal_set_default_colors (VTE_TERMINAL (vte));

	//vte_terminal_set_background_transparent(VTE_TERMINAL(vte), TRUE);
	/*
	GdkColor col;
	col.red = 0;
	col.green = 0;
	col.blue = 0;
	vte_terminal_set_color_cursor(VTE_TERMINAL(vte), &col);
	*/
	//vte_terminal_set_cursor_shape(VTE_TERMINAL(vte), VTE_CURSOR_SHAPE_UNDERLINE); //Default: VTE_CURSOR_SHAPE_BLOCK
	//vte_terminal_set_cursor_blinks (VTE_TERMINAL(vte), FALSE);
	vte_terminal_set_cursor_blink_mode (VTE_TERMINAL(vte), VTE_CURSOR_BLINK_SYSTEM);

	
	GdkRGBA colour;
	gdk_rgba_parse(&colour, "#ffffff");
	vte_terminal_set_color_foreground (VTE_TERMINAL(vte), &colour);

	//vte_terminal_set_colors (term, foreground, background, NULL, 0);

	/*
	GdkColor colour;
	colour.red = 0;
	colour.green = 0;
	colour.blue = 0;
	vte_terminal_set_color_foreground (VTE_TERMINAL(vte), &colour);

	colour.red = 65535; //65535
	colour.green = 65535;
	colour.blue = 65535;
	vte_terminal_set_color_background (VTE_TERMINAL(vte), &colour);
	*/
	
}

/*
bool wxTerminal::TimerCheck_Tick()
{

	//wxGLOBAL->DebugPrint("TICK", "TICK");
	if ( kill ( pid, 0 ) == -1) 
	{
		CompilerCompleted();
        return false;
	}
	else
        return true;
}
*/

void wxTerminal::on_child_exited(GtkWidget *widget, gpointer data)
{
	//wxGLOBAL->DebugPrint("COMPILER", "on_child_exited");
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->CompilerCompleted();
}

void wxTerminal::CompilerCompleted()
{
	//wxGLOBAL->DebugPrint("wxTerminal::CompilerCompleted");
	
	gtk_widget_set_sensitive (GTK_WIDGET (buttonStop), FALSE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonPause), FALSE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonPanic), FALSE);
	gtk_widget_set_sensitive (GTK_WIDGET (buttonSave), TRUE);
	gtk_button_set_label (GTK_BUTTON(buttonPause), "Pause");
	ProcessActive = false;


	//Refresh vte (sometimes it doesn't paint/redraw correctly)
	gtk_widget_draw(GTK_WIDGET(vte), NULL);	
	//Needed to refresh scrollbar!!!
	vte_terminal_fork_command(VTE_TERMINAL(vte), 
	                          NULL, NULL, NULL, 
	                          "/usr/bin", 
	                          TRUE, TRUE,TRUE);
	
	
	//GET COMPILER TEXT
	gchar* text = NULL;
	try
	{
		//This method retrieve all the text
		GOutputStream* os = g_memory_output_stream_new(NULL, 0, g_realloc, g_free);;
		vte_terminal_write_contents(VTE_TERMINAL(vte),
		                            G_OUTPUT_STREAM(os),
		                            VTE_TERMINAL_WRITE_DEFAULT,
		                            NULL,
		                            NULL);
		gpointer p = g_memory_output_stream_get_data(G_MEMORY_OUTPUT_STREAM(os));
		text = (gchar*)p;   
		
		g_output_stream_close (G_OUTPUT_STREAM(os), NULL, NULL);

	}
	catch(...)
	{
		//This method retrieve only the visible text
		wxGLOBAL->DebugPrint("CompilerCompleted", "[Warning] Get Compiler Text 2nd method.");
		try
		{
			text = vte_terminal_get_text(VTE_TERMINAL(vte),
			                             NULL,
			                             NULL,
			                             NULL);
		}
		catch(...){}		
	}
	

	Glib::ustring errorline = "";
	Glib::ustring soundfile = "";
	if(text != NULL)
	{
		Glib::ustring compilerText = text;
		g_free(text);

		//SEARCH FOR ERROR and SOUNDFILE
		errorline = FindError(compilerText);
		soundfile = FindSounds(compilerText);
	}
	
	m_compiler_completed.emit(errorline, soundfile);

}


void wxTerminal::on_buttonStop_clicked(GtkWidget *widget, gpointer data)
{
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->StopCompiler();
}

void wxTerminal::on_buttonPause_clicked(GtkWidget *widget, gpointer data)
{
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->PauseCompiler();
}

void wxTerminal::on_buttonPanic_clicked(GtkWidget *widget, gpointer data)
{
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->ForceKill();
}

void wxTerminal::on_buttonSave_clicked(GtkWidget *widget, gpointer data)
{
	wxTerminal* _this = reinterpret_cast<wxTerminal*>(data);
	_this->SaveOutput();
}

void wxTerminal::SaveOutput()
{
	//wxGLOBAL->DebugPrint("SaveOutput");

	//Old: Save to desktop
	//Glib::ustring filename = Glib::get_user_special_dir(G_USER_DIRECTORY_DESKTOP);
	//filename.append("/WinXound_Compiler_Output.txt");

	//New: SaveAs dialog
	Gtk::FileChooserDialog dialog("Save CSound Compiler Output",
		                          Gtk::FILE_CHOOSER_ACTION_SAVE);
	//dialog.set_transient_for(*this);
	dialog.set_modal(TRUE);

	if(currentOutputPath == "")
		currentOutputPath = wxSETTINGS->Directory.LastUsedPath;

	dialog.set_current_folder(currentOutputPath);

	//Add response buttons the the dialog:
	dialog.add_button(Gtk::Stock::CANCEL, Gtk::RESPONSE_CANCEL);
	dialog.add_button(Gtk::Stock::SAVE, Gtk::RESPONSE_OK);
	
	//Set dialog filename
	Glib::ustring proposedFilename = currentFilename + ".output";
	dialog.set_current_name(proposedFilename);

	/*
	//Add filters
	Gtk::FileFilter filter_text_files;
	filter_text_files.set_name("txt file");
	filter_text_files.add_pattern("*.txt");
	dialog.add_filter(filter_text_files);

	Gtk::FileFilter filter_any;
	filter_any.set_name("Any files");
	filter_any.add_pattern("*");
	dialog.add_filter(filter_any);
	*/

	dialog.set_do_overwrite_confirmation(TRUE);

	//If the WorkingDir is not empty and exists add it to the Open Dialog Box:
	if(Glib::file_test(wxSETTINGS->Directory.WorkingDir, 
	                   (Glib::FILE_TEST_EXISTS | Glib::FILE_TEST_IS_DIR)))
	{
		dialog.add_shortcut_folder(wxSETTINGS->Directory.WorkingDir);
	}

	
	
	int result = dialog.run();
	if (result != Gtk::RESPONSE_OK) return;


	std::string dialogFilename = dialog.get_filename();
	currentOutputPath = dialog.get_current_folder();
	Glib::ustring filename = dialogFilename;


	/*
	///////////////////////////////////
	//TODO: ???
	//COUNTER SYSTEM !!!
	dialog.set_do_overwrite_confirmation(FALSE); 	//We check it ourself
	Glib::ustring filename = dialogFilename + ".001";
	//If the filename already exist, increment count
	gint count = 1;
	while (true)
	{
		if(Glib::file_test(filename, Glib::FILE_TEST_EXISTS))
		{
			size_t ret = filename.rfind(".");
			
			if(ret != Glib::ustring::npos)
			{
				count++;
				if(count > 999)
				{
					filename.erase(ret, filename.length()-ret);
					filename.append(".001");
					break;
				}
				filename.erase(ret, filename.length()-ret);
				char buffer [3];				
				sprintf(buffer, "%03d", count);
				filename.append(".");
				filename.append(buffer);
				continue;
			}
			break;
		}
		break;
	}
	std::cout << "FILENAME: " << filename << std::endl;
	///////////////////////////////////
	*/


	
	//CREATE AND SAVE THE FILE
	//Create the file (empty)
	Glib::file_set_contents(filename, "");

	//Write vte content to the created file
	if(!Glib::file_test(filename, Glib::FILE_TEST_EXISTS)) return;

	GFile* file = g_file_new_for_path(filename.c_str());
	if(file == NULL) return;
	GFileOutputStream* os = g_file_replace(file, 
	                                       NULL,
	                                       FALSE,
	                                       G_FILE_CREATE_NONE, //G_FILE_CREATE_REPLACE_DESTINATION,
	                                       NULL,
	                                       NULL);
	
	vte_terminal_write_contents(VTE_TERMINAL(vte),
	                            G_OUTPUT_STREAM(os),
	                            VTE_TERMINAL_WRITE_DEFAULT,
	                            NULL,
	                            NULL);
	
	g_output_stream_close (G_OUTPUT_STREAM(os), NULL, NULL);
	g_object_unref(file);

	//wxGLOBAL->ShowMessageBox("File 'WinXound_Compiler_Output.txt' \nsaved on your desktop.",
	//                         "WinXound Informations",
	//                         Gtk::BUTTONS_OK);
	                         
	
}

void wxTerminal::ForceKill()
{
	try
	{
		kill(pid, SIGKILL); 
	}
	catch(...){}
}

void wxTerminal::PauseCompiler()
{
	if(paused == false)
	{
		int ret = kill(pid, SIGSTOP);
		if (ret == 0)
		{
			paused = true;
			gtk_button_set_label (GTK_BUTTON(buttonPause), "Resume");
		}
	}
	else
	{
		int ret = kill(pid, SIGCONT);
		if (ret == 0)
		{
			paused = false;
			gtk_button_set_label (GTK_BUTTON(buttonPause), "Pause");
		}
	}
}

void wxTerminal::StopCompiler()
{
	try
	{
		if(paused == true) kill(pid, SIGCONT);
		kill(pid, SIGQUIT);  //SIGTERM
	}
	catch(...){}
}


bool wxTerminal::HasFocus()
{
	return gtk_widget_has_focus(GTK_WIDGET(vte));
}

void wxTerminal::SetFocus()
{
	gtk_widget_grab_focus(GTK_WIDGET(vte));
}

void wxTerminal::SetCompilerFont(Glib::ustring name, gint size)
{
	if(vte != NULL)
	{
		//Default: "Monospace 10"
		Glib::ustring font = name;
		font.append(" ");
		font.append(wxGLOBAL->IntToString(size));
		PangoFontDescription* pfd = pango_font_description_from_string(font.c_str());
		if(pfd != NULL)
		{
			vte_terminal_set_font(VTE_TERMINAL(vte), pfd);
			pango_font_description_free(pfd);
		}
	}			
}

void wxTerminal::ClearCompilerText()
{
	if(mIsCompiler)
	{
		vte_terminal_reset(VTE_TERMINAL(vte), TRUE, TRUE);
		ConfigureTerminalForCompiler();
	}
}

void wxTerminal::AppendCompilerText(Glib::ustring text)
{
	if(!mIsCompiler) return;

	//std::cout << "APPENDCOMPILERTEXT RECEIVED: " << text << std::endl;

	glong col = 0;
	glong row = 0;
	vte_terminal_get_cursor_position(VTE_TERMINAL(vte),
	                                 &col,
	                                 &row);
	if(col > 0)
		text.insert(0, "\r\n");

	/*
	vte_terminal_feed_child(VTE_TERMINAL(vte), 
	                        text.c_str(),
	                        text.size());
	*/

	vte_terminal_feed(VTE_TERMINAL(vte), 
	                  text.c_str(),
	                  text.size());

	
	//Refresh vte (sometimes it doesn't paint/redraw correctly)
	//gtk_widget_draw(GTK_WIDGET(vte), NULL); //deprecated
	gtk_widget_queue_draw(GTK_WIDGET(vte));
	
}



////////////////////////////////////////////////////////////////////////////////

Glib::ustring wxTerminal::FindError(Glib::ustring text)
{
	Glib::ustring StringToFind = "error:";
	Glib::ustring temp = "";
	gint mFindPos = -1;

	try
	{

		Glib::ustring textOfLine = "";
		Glib::ustring returnValue = "";
		gchar** lines = g_strsplit(text.c_str(), "\n", 0);
		int length = wxGLOBAL->ArrayLength(lines);
		
		//for(int index = length - 1; index > -1; index --) //REVERSE FIND
		for(int index = 0; index < length; index ++) //FORWARD FIND
		{
			if(strlen(lines[index]) > 0)
			{
				textOfLine = lines[index];
				if(textOfLine.find(StringToFind, 0) != Glib::ustring::npos)
				{
					size_t findPos= textOfLine.find("line", 0);
					if(findPos != Glib::ustring::npos)
					{
						temp = "";
						
						//error text = line 23:
						//[currentLine substringFromIndex:mFsFindPos.location + 4];
						Glib::ustring returnString = textOfLine.substr(findPos + 4);
						returnString = textOfLine.substr(findPos + 4);

						//Check first char (space)
						if(!std::isdigit(*textOfLine.substr(0,1).c_str()))
							returnString.erase(0,1); 
						
						//Check and remove ":" char
						if(Glib::str_has_suffix(returnString, ":"))
							returnString.resize(returnString.size()-1); 

						
						//1. We add an highlighted line with the error info:
						//Red line:
						//Glib::ustring temp = "\e[0;31mERROR FOUND AT LINE: " + returnString + 
						//					 " [" + textOfLine + "]" +
						//					 "\e[m\r\n";
						//Highlighted line:
						//Glib::ustring temp = "\e[7mERROR FOUND AT LINE: " + returnString + "\e[m\r\n"
						//					  "--> " + textOfLine + "\r\n";

						if(returnValue == "")
						{
							Glib::ustring temp = "\r\n\e[7mCompiler errors:\e[m\r\n";
								
							vte_terminal_feed (VTE_TERMINAL(vte),
							                   temp.c_str(),
							                   -1);
						}

						//Errors info:
						temp = "--> " + textOfLine + "\r\n";
						vte_terminal_feed (VTE_TERMINAL(vte),
						                   temp.c_str(),
						                   -1);

						//Return the string as error line
						//std::cout << returnString << std::endl;
						//return returnString;

						if(returnValue == "")
							returnValue = returnString;
					}
				}
			}
		}
		g_strfreev(lines);

		return returnValue;

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxTerminal::FindError error");
	}

	return "";
}



Glib::ustring wxTerminal::FindSounds(Glib::ustring text)
{
	//OLD:
	/*
	try
	{
		size_t mFindPos = -1;
		Glib::ustring currentLine = "";

		//Example of compiler output:
		//writing 2048-byte blks of shorts to /Users/teto/Desktop/fm_01.wav (WAV)
		//1034 2048-byte soundblks of shorts written to /Users/teto/Desktop/fm_01.wav (WAV)

		gchar** lines = g_strsplit(text.c_str(), "\n", 0);
		int length = wxGLOBAL->ArrayLength(lines);

		for(int index = length - 1; index > -1; index --)
		{
			if(strlen(lines[index]) > 0)
			{
				currentLine = lines[index];

				if(currentLine.find("written", 0) != Glib::ustring::npos)
				{
					if(currentLine.find("writing", 0) != Glib::ustring::npos)
						continue;
				}


				//SEARCH FOR: .wav or .aiff or .aif
				gint extensionLength = 0;
				//mFindPos = [currentLine rangeOfString:@".wav"].location;
				mFindPos = currentLine.find(".wav");		
				extensionLength = 4;

				if(mFindPos == Glib::ustring::npos) //not found
				{
					//mFindPos = [currentLine rangeOfString:@".aiff"].location;
					mFindPos = currentLine.find(".aiff");	
					extensionLength = 5;
				}
				if(mFindPos == Glib::ustring::npos) //not found
				{
					//mFindPos = [currentLine rangeOfString:@".aif"].location;
					mFindPos = currentLine.find(".aif");
					extensionLength = 4;
				}


				Glib::ustring tempString = "";

				if (mFindPos != Glib::ustring::npos)
				{
					size_t mFindStart = -1;
					//mFindStart = [currentLine rangeOfString:@"written to"].location;
					mFindStart = currentLine.find("written to");

					if (mFindStart != Glib::ustring::npos)
					{

						//mLine = mLine.Remove(mFindPos + 1);
						//NSString* mLine = [currentLine substringToIndex:mFindPos + extensionLength];
						Glib::ustring mLine = currentLine.substr(0, mFindPos + extensionLength);
						
						//tempString = mLine.Substring(mFindStart + 11);
						//tempString = [mLine substringFromIndex:mFindStart + 11];
						tempString = mLine.substr(mFindStart + 11);
						return tempString;
					}
					else
					{
						//mFindStart = [currentLine rangeOfString:@"writing"].location;
						mFindStart = currentLine.find("writing");
						
						if(mFindStart != Glib::ustring::npos)
						{
							//mFindStart = [currentLine rangeOfString:@"to"].location;
							mFindStart = currentLine.find("to");
							
							if(mFindStart != Glib::ustring::npos)
							{
								//NSString* mLine = [currentLine substringToIndex:mFindPos + extensionLength];
								Glib::ustring mLine = currentLine.substr(0, mFindPos + extensionLength);

								//tempString = [mLine substringFromIndex:mFindStart + 3];
								tempString = mLine.substr(mFindStart + 3);
								return tempString;
							}
						}
					}
				}
			}
		}

		g_strfreev(lines);
		
	}
	catch (...) 
	{
		wxGLOBAL->DebugPrint("wxTerminal::FindSounds error");
	}
	*/

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

	//NEW:
	try
	{
		size_t mFindPos = -1;
		Glib::ustring currentLine = "";

		//Example of compiler output:
		//writing 2048-byte blks of shorts to /Users/teto/Desktop/fm_01.wav (WAV)
		//1034 2048-byte soundblks of shorts written to /Users/teto/Desktop/fm_01.wav (WAV)

		gchar** lines = g_strsplit(text.c_str(), "\n", 0);
		int length = wxGLOBAL->ArrayLength(lines);

		for(int index = length - 1; index > -1; index --)
		{
			if(strlen(lines[index]) > 0)
			{
				currentLine = lines[index];

				if(currentLine.find(".wav", 0) == Glib::ustring::npos) //not found
				{
					if(currentLine.find(".aif", 0) == Glib::ustring::npos) //not found
						continue;
				}


				//SEARCH FOR: .wav or .aiff or .aif
				gint extensionLength = 0;
				//mFindPos = [currentLine rangeOfString:@".wav"].location;
				mFindPos = currentLine.find(".wav");		
				extensionLength = 4;

				if(mFindPos == Glib::ustring::npos) //not found
				{
					//mFindPos = [currentLine rangeOfString:@".aiff"].location;
					mFindPos = currentLine.find(".aiff");	
					extensionLength = 5;
				}
				if(mFindPos == Glib::ustring::npos) //not found
				{
					//mFindPos = [currentLine rangeOfString:@".aif"].location;
					mFindPos = currentLine.find(".aif");
					extensionLength = 4;
				}
				

				if (mFindPos != Glib::ustring::npos)
				{
					//size_t mFindStart = -1;
					size_t mPosEnd = mFindPos + extensionLength;
					size_t mPosStart = currentLine.find("/");

					if (mPosStart != Glib::ustring::npos)
					{
						Glib::ustring tempString = currentLine.substr(mPosStart, mPosEnd - mPosStart);
						return tempString;
					}
				}
			}
		}

		g_strfreev(lines);

	}
	catch (...) 
	{
		wxGLOBAL->DebugPrint("wxTerminal::FindSounds error");
	}		
	
	
	return "";


}

