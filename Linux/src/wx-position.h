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


#ifndef _WX_POSITION_H_
#define _WX_POSITION_H_

class wxPosition
{
public:
	wxPosition();
	virtual ~wxPosition();

	void StoreCursorPos(gint position);
	gint PreviousPos();
	gint NextPos();
	void AddValue(gint Value);
	gint Position();
	void ClearAll();


protected:

private:
	std::vector<int> pos_list;
	gint mIndex;
	gint oldPos;

};

#endif // _WX_POSITION_H_
