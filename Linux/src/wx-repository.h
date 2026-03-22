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


#include "../scintilla/Scintilla.h"
#include "../scintilla/SciLexer.h"
#include "../scintilla/ScintillaWidget.h"



#ifndef _WX_REPOSITORY_H_
#define _WX_REPOSITORY_H_


class wxRepository : public Gtk::EventBox //public Gtk::Frame
{
public:
	wxRepository();
	virtual ~wxRepository();

	void ConfigureEditor(GHashTable* Opcodes);
	void InsertText(Glib::ustring& text);
	Glib::ustring GetUdoOpcodesList();



protected:

	//Tree model columns:
	class repColumns : public Gtk::TreeModel::ColumnRecord
	{
	public:
		Gtk::TreeModelColumn<Glib::ustring> category;
		Gtk::TreeModelColumn<Glib::ustring> name;
		//Gtk::TreeModelColumn<Glib::ustring> description;
		//Gtk::TreeModelColumn<Glib::ustring> synopsis;

		repColumns()
		{
			add(category);
			add(name); 		
		}

	};	

	repColumns						rColumns;
	Glib::RefPtr<Gtk::TreeStore>	rRefTreeModel;

	Gtk::TreeView*					repStructure;
	Gtk::ScrolledWindow*			mScrolledWindow;

	Gtk::Button*					buttonSave;

	Glib::ustring					oldDrag;
	Glib::ustring					mOpcodes;

	void CreateNewRepository();

	//void on_treeStructure_row_activated(const Gtk::TreeModel::Path& path, Gtk::TreeViewColumn* column);
	//bool on_treeStructure_clicked(GdkEventButton* event);
	void on_rep_cursor_changed();
	
	void on_rep_check_resize(Gdk::Rectangle& rect);
	

	void on_rep_drag_data_get(
        const Glib::RefPtr<Gdk::DragContext>& dragcontext,
        Gtk::SelectionData& selection_data, guint info, guint time);

	void SciEditSetFontsAndStyles();
	gint getColorFromString(const gchar* stringColor);

	void Delete();
	void Rename();
	void CreateTreePopupMenu();
	bool on_tree_press_event(GdkEventButton* event);

	Gtk::Menu		treePopupMenu;
	Gtk::MenuItem   treeDelete;
	Gtk::MenuItem   treeRemove;

	void on_drop_data_received(const Glib::RefPtr<Gdk::DragContext>& context, int, int,
	                           const Gtk::SelectionData& selection_data, guint, guint time);

	void UpdateUdoList();

	void on_button_save_Clicked();

	static void on_SCI_NOTIFY(GtkWidget *widget, gint wParam, gpointer lParam, gpointer data);
	void SavePointLeft();
	void SavePointReached();
	
	
private:
	ScintillaObject*	textView;
	Gtk::Widget*		textView_Widget;

	


};

#endif // _WX_CSOUND_REPOSITORY_H_
