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

#include "wx-findcodestructure.h"
#include "wx-editor.h"
#include "wx-textEditor.h"
#include "wx-global.h"
#include "wx-settings.h"




wxFindCodeStructure::wxFindCodeStructure(wxEditor* owner, wxTextEditor* textEditor)
{
	mStop = false;
	mOwner = owner;
	mTextEditor = textEditor;

	CreatePixbufs();

	//Create base model
	MyRefTreeModel = Gtk::TreeStore::create(MyColumns);
}

wxFindCodeStructure::~wxFindCodeStructure()
{
	mStop = true;
}


void wxFindCodeStructure::Start()
{
	//Glib::Thread *const my_thread = Glib::Thread::create(
    //    sigc::bind( sigc::ptr_fun(&thread_func), "Hello World" ), true);
	//sigc::bind<Glib::ustring>( sigc::mem_fun(*this, &HelloWorld::on_button_clicked), "button 1"

	Glib::Thread::create(sigc::mem_fun(*this, &wxFindCodeStructure::FindStructure), false);
	
	//FindStructure();
}

void wxFindCodeStructure::Stop()
{
	mStop = true;
}

void wxFindCodeStructure::CreatePixbufs()
{
	pixbufBlue = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                 false, 8,
	                                 6,6);
	pixbufBlue->fill(0x0c5d88FF);

	
	pixbufGreen = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                  false, 8,
	                                  6,6);
	pixbufGreen->fill(0x007805FF);

	
	pixbufGrey = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                 false, 8,
	                                 6,6);
	pixbufGrey->fill(0xc82626FF);

	
	pixbufOrange = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                   false, 8,
	                                   6,6);
	pixbufOrange->fill(0xff6c00FF);

	
	pixbufRed = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                false, 8,
	                                6,6);
	pixbufRed->fill(0x8f0000FF);

	
	pixbufSand = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                 false, 8,
	                                 6,6);
	pixbufSand->fill(0xd8a365FF);

	
	pixbufViolet = Gdk::Pixbuf::create(Gdk::COLORSPACE_RGB,
	                                   false, 8,
	                                   6,6);
	pixbufViolet->fill(0xbe42bcFF);

	
}

void wxFindCodeStructure::FindStructure()
{
	try
	{

		//Create the Tree model:
		if(MyRefTreeModel)
			MyRefTreeModel.clear();	
		MyRefTreeModel = Gtk::TreeStore::create(MyColumns);


		//Fill the TreeView's model
		Gtk::TreeModel::Row rowSynthesizer = *(MyRefTreeModel->append());
		rowSynthesizer[MyColumns.key] = "<CsoundSynthesizer>";
		rowSynthesizer[MyColumns.value] = "<CsoundSynthesizer>";
		rowSynthesizer[MyColumns.image] = pixbufRed;

		
		Gtk::TreeModel::Row rowOptions = *(MyRefTreeModel->append());
		rowOptions[MyColumns.key] = "<CsOptions>";
		rowOptions[MyColumns.value] = "<CsOptions>";
		rowOptions[MyColumns.image] = pixbufRed;


		Gtk::TreeModel::Row rowInstruments = *(MyRefTreeModel->append());
		rowInstruments[MyColumns.key] = "<CsInstruments>";
		rowInstruments[MyColumns.value] = "<CsInstruments>";
		rowInstruments[MyColumns.image] = pixbufRed;


		Gtk::TreeModel::Row rowScore = *(MyRefTreeModel->append());
		rowScore[MyColumns.key] = "<CsScore>";
		rowScore[MyColumns.value] = "<CsScore>";
		rowScore[MyColumns.image] = pixbufRed;


		
		
		//CSOPTIONS
		//if([[wxDefaults valueForKey:@"ExplorerShowOptions"] boolValue])
		if(wxSETTINGS->EditorProperties.ExplorerShowOptions)
		{
			gint mStartCsOptions = 
				mTextEditor->FindText("<CsOptions>",
			                         true, true, false, false, false, true, 0, -1);


			if (mStartCsOptions > -1)
			{
				mStartCsOptions += 11;

				gint mEndCsOptions = 
					mTextEditor->FindText("</CsOptions>",
				                         true, true, false, false, false, true, 0, -1);

				if (mEndCsOptions > -1)
				{
					gint startLine = mTextEditor->getLineNumberFromPosition(mStartCsOptions) + 1;
					gint endLine = mTextEditor->getLineNumberFromPosition(mEndCsOptions);
					gint mFindRem = -1;
					Glib::ustring textOfLine = "";

					for (int i = startLine; i < endLine; i++)
					{
						//SKIP INVALID LINES (REM)
						textOfLine = mTextEditor->getTextOfLine(i);

						//mFindRem = textOfLine.Trim().IndexOf(";");
						textOfLine = g_strstrip((gchar*)textOfLine.c_str());
						mFindRem = textOfLine.find_first_of(";",0);

						if (mFindRem == 0)
						{
							continue;
						}
						else if (textOfLine.length() > 0)
						{
							Glib::ustring mFsValue = textOfLine; //textOfLine.Trim();
							Glib::ustring mFsKey = ParseLine(mFsValue);
							//Options.Nodes.Add(mFsValue, mFsKey);
							Gtk::TreeModel::Row childrow = *(MyRefTreeModel->append(rowOptions.children()));
							childrow[MyColumns.key] = mFsKey.c_str();
							childrow[MyColumns.value] = mFsValue.c_str();
							childrow[MyColumns.image] = pixbufGreen;
							break;
						}
					}
				}
			}
		}



		
		////CSINTRUMENTS: MACROS (#DEFINE)	
		if (wxSETTINGS->EditorProperties.ExplorerShowInstrMacros)
			findString("#define", rowInstruments, false);

		
		////CSINTRUMENTS: OPCODE	
		if (wxSETTINGS->EditorProperties.ExplorerShowInstrOpcodes)
			findString("opcode", rowInstruments, false);

		////CSINTRUMENTS: INSTR		
		if (wxSETTINGS->EditorProperties.ExplorerShowInstrInstruments)
			//this.findString(@"instr\s+\d+", Instruments, false);
			findString("instr", rowInstruments, false);
		

		////CSSCORE: Functions
		if (wxSETTINGS->EditorProperties.ExplorerShowScoreFunctions)
			findStringInScore("f", rowScore);

		
		////CSSCORE: Macros
		if (wxSETTINGS->EditorProperties.ExplorerShowScoreMacros)
			findString("#define", rowScore, true);


		////CSSCORE: Sections
		if (wxSETTINGS->EditorProperties.ExplorerShowScoreSections)
			findStringInScore("s", rowScore);


		//mOwner->FindCodeStructureWorkCompleted(); //RefTreeModel);
		sig_done();
		
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxFindCodeStructure - FindStructure Error ");
	}

	return;
}

Glib::ustring wxFindCodeStructure::ParseLine(Glib::ustring Text)
{
	try
	{
		//Text = Regex.Replace(Text, @"(\s+)", " ");
		Glib::RefPtr<Glib::Regex> my_regex = Glib::Regex::create("(\\s+)");
		Glib::ustring ret = my_regex->replace(Text, 0, " ", Glib::REGEX_MATCH_NOTBOL);
		ret = g_strstrip((gchar*)ret.c_str());

		//wxGLOBAL->DebugPrint("ParseLine", ret.c_str());
		
		return ret;
		
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxFindCodeStructure - ParseLine Error ");
		return Text;
	}
}


bool wxFindCodeStructure::findString(const gchar* stringToFind,
                                     Gtk::TreeModel::Row passedNode,
                                     bool isScore)
{
	Glib::ustring mFsKey = "";
	Glib::ustring mFsValue = "";
	gint mFsSearchFlags = 0;
	gint mFsStart = 0;
	gint mFsEnd = mTextEditor->getTextLength();
	gint mFsFindPos = -1;

	Gtk::TreeModel::Row childrow;

	Glib::ustring field = stringToFind;
	


	if (isScore) //Score
	{
		mFsStart = mTextEditor->FindText("<CsScore>",
		                                 true, true, false, false, false, true,
		                                 0, -1);
		mFsEnd = mTextEditor->FindText("</CsScore>",
		                               true, true, false, false, false, true,
		                               0, -1);
	}
	else //Instruments
	{
		mFsStart = mTextEditor->FindText("<CsInstruments>",
		                                 true, true, false, false, false, true,
		                                 0, -1);
		mFsEnd = mTextEditor->FindText("</CsInstruments>",
		                               true, true, false, false, false, true,
		                               0, -1);
	}



	//New: for Orc and Sco
	if (Glib::str_has_suffix(mOwner->FileName.lowercase(),".orc") && 
	    isScore == true)
		return false;

	if (Glib::str_has_suffix(mOwner->FileName.lowercase(),".orc") ||
	    Glib::str_has_suffix(mOwner->FileName.lowercase(),".sco"))
	{
		mFsStart = 0;
		mFsEnd = mTextEditor->getTextLength();
	}

	

	mFsSearchFlags |= SCFIND_WHOLEWORD;
	mFsSearchFlags |= SCFIND_MATCHCASE;

	do
	{
		if(mStop) break;

		mTextEditor->setTargetStart(mFsStart);
		mTextEditor->setTargetEnd(mFsEnd);

		mTextEditor->setSearchFlags(mFsSearchFlags);

		mFsFindPos = mTextEditor->searchInTarget(stringToFind);
		if (mFsFindPos > -1)
		{
			mFsStart = mFsFindPos + 5;

			if (mTextEditor->isRemAt(mFsFindPos) == false)
			{
				mFsValue = 
					mTextEditor->getTextOfLine(
						mTextEditor->getLineNumberFromPosition(mFsFindPos));
				mFsKey = ParseLine(mFsValue);
				//if (!TempHashTable.Contains(mFsKey))
				//if (!mHashTable.Contains(mFsKey))
				{
					//TempHashTable.Add(mFsKey, mFsValue);
					//mHashTable.Add(mFsKey, mFsValue);
					//passedNode.Nodes.Add(mFsValue, mFsKey);
					childrow = *(MyRefTreeModel->append(passedNode.children()));
					childrow[MyColumns.key] = mFsKey.c_str();
					childrow[MyColumns.value] = mFsValue.c_str();

					//Set pixbuf
					if(field == "instr")
						childrow[MyColumns.image] = pixbufBlue;
					else if(field == "#define")
						childrow[MyColumns.image] = pixbufViolet;
					else if(field == "opcode")
						childrow[MyColumns.image] = pixbufOrange;
				}
			}
		}
		else
		{
			break;
		}
	}
	while (true);


	return true;
}

bool wxFindCodeStructure::findStringInScore(const gchar* stringToFind,
                                			Gtk::TreeModel::Row passedNode)
{
	//CSSCORE

	
	Glib::ustring mFsKey = "";
	Glib::ustring mFsValue = "";
	gint mFsEnd = mTextEditor->getTextLength();
	gint mFindRem = 0;	

	Gtk::TreeModel::Row childrow;

	Glib::ustring field = stringToFind;


	gint mStartCsScore = 
		mTextEditor->FindText("<CsScore>",
		                      true, true, false, false, false, true, 0, -1);


	//New: for Orc and Sco
	if (Glib::str_has_suffix(mOwner->FileName.lowercase(),".sco"))
		mStartCsScore = 0;
	
	if (mStartCsScore > -1)
	{
		mStartCsScore += 8;
		gint mEndCsScore = 
			mTextEditor->FindText("</CsScore>", 
		                          true, true, false, false, false, true, 0, -1);

		if (mEndCsScore == -1) mEndCsScore = mFsEnd;

		if (mEndCsScore > -1)
		{

			gint f = -1;
			gint index = 1;
			gint startLine = mTextEditor->getLineNumberFromPosition(mStartCsScore);
			gint endLine = mTextEditor->getLineNumberFromPosition(mEndCsScore);
			Glib::ustring textOfLine = "";

			//wxGLOBAL->DebugPrint(wxGLOBAL->IntToString(startLine).c_str(),
			//                     wxGLOBAL->IntToString(endLine).c_str());
			
			for (gint i = startLine; i < endLine; i++)
			{
				//SKIP INVALID LINES (REM)
				textOfLine = mTextEditor->getTextOfLine(i);
				if(textOfLine.length() < 1) continue;

				//mFindRem = textOfLine.Trim().IndexOf(";");
				textOfLine = g_strstrip((gchar*)textOfLine.c_str());
				if(textOfLine.length() < 1) continue;
				mFindRem = textOfLine.find_first_of(";",0);

				if (mFindRem == 0)
				{
					continue;
				}
				else if (textOfLine.length() > 0)
				{

					//f = textOfLine.Trim().IndexOf(stringToFind);
					f = textOfLine.find_first_of(stringToFind, 0);
					if (f == 0)
					{
						mFsValue = textOfLine; //textOfLine.Trim();
						mFsKey = ParseLine(mFsValue); //[self parseString:mFsValue];

						//if (stringToFind == "s")
						if(g_str_equal(stringToFind, "s"))
						{
							//+= " " + index.ToString();
							mFsKey.append(" ");
							mFsKey.append(wxGLOBAL->IntToString(index));
							mFsValue = mFsKey;
							index++;
						}

						//passedNode.Nodes.Add(mFsValue, mFsKey)

						childrow = *(MyRefTreeModel->append(passedNode.children()));
						childrow[MyColumns.key] = mFsKey.c_str();
						childrow[MyColumns.value] = mFsValue.c_str();

						//Set pixbuf
						if(field == "f")
							childrow[MyColumns.image] = pixbufSand;
						else if(field == "s")
							childrow[MyColumns.image] = pixbufGrey;
					}
				}
			}
		}
	}

	return true;
}


