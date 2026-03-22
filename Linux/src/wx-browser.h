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
#include <webkit/webkit.h>


#ifndef _WX_BROWSER_H_
#define _WX_BROWSER_H_


class wxBrowser
{
public:
	wxBrowser(bool history);
	virtual ~wxBrowser();
	
	Gtk::Widget* getBrowserWidget();
	void LoadUri(const gchar* uri);
	static void on_buttonBackward_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonForward_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonHome_clicked(GtkWidget *widget, gpointer data);
	static void on_load_status(WebKitWebView  *web_view,
	                           WebKitWebFrame *frame,
	                           gpointer        user_data);
	static void on_entry_activate(GtkWidget *widget, gpointer data);
	static void on_buttonZoomMinus_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonZoomPlus_clicked(GtkWidget *widget, gpointer data);
	static void on_buttonZoomReset_clicked(GtkWidget *widget, gpointer data);

	void Find(const gchar* text);
	void goBack();
	void goForward();
	void goHome();
	void refreshButtonState();
	void SetBrowserZoom(gfloat zoom);
	bool History;
	void zoomIn();
	void zoomOut();
	
	

	
	//Glib::Dispatcher signal_csound_file_clicked;
	typedef sigc::signal<void, const gchar*> type_signal_csound_file_clicked;
	type_signal_csound_file_clicked   signal_csound_file_clicked() {return m_signal_csound_file_clicked;};
	type_signal_csound_file_clicked   m_signal_csound_file_clicked;





	
protected:
	void CreateNewBrowser();
	Gtk::Widget*	wid;	
	WebKitWebView*  web_view;
	GtkWidget*		buttonBackward;
	GtkWidget*		buttonForward;
	GtkWidget*		entryFind;
	GtkWidget*		scrolled_window;
	GtkWidget*		vbox;
	GtkWidget*		frame;

	gfloat			zoomValue;
	//GtkWidget*		labelEsc;

	static void on_check_resize_browser (GtkWidget      *widget,
	                                     GtkRequisition *requisition,
	                                     gpointer        data);
	void check_resize();
	
	
	

private:
	

};

#endif // _WX_BROWSER_H_
