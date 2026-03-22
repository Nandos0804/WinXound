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

#include "wx-codeformatter.h"
#include "wx-global.h"
#include "wx-settings.h"
#include "wx-textEditor.h"

#define SSM(sci, m, w, l) scintilla_send_message(sci, m, w, l)


wxCodeFormatter::wxCodeFormatter()  	
{
	CreateNewFormatterWindow();
}

wxCodeFormatter::~wxCodeFormatter()
{
	g_hash_table_destroy(opcodes);

	delete textView;	//wxTextEditor*
	delete formatterWindow;
	
	wxGLOBAL->DebugPrint("wxCodeFormatter released");
}


void wxCodeFormatter::FormatCode(wxTextEditor* editor, GHashTable* keywords)
{
	FormatCode(editor, keywords, 0, editor->getLinesCount());
}

void wxCodeFormatter::FormatCode(wxTextEditor* editor, GHashTable* keywords, gint start, gint end)
{
	try
	{	
		Glib::ustring textline = "";
		Glib::ustring tempString = "";
		bool isSinglelineRem = false;
		bool isMultilineRem = false;
		gint remIndex = 0;
		//bool isOrchestra = false;
		bool isInstrument = false;
		bool isScore = false;
		bool isReplaced = false;
		gint startpos = 0;
		//char[] delimiter = new char[] { ' ', '\t', '\r', '\n' };
		const gchar* delimiter = "\t\r\n ";


		//SET START - END 
		//Remember: passed arguments (start-end) are expressed as line number
		gint mStart = start;
		gint mEnd = end;

		//If the textEditor has some selected text we must
		//look before to find the right csound section (Score or Instruments)
		if (mStart > 0)
		{
			gint tempRet = editor->FindText("<CsScore>",
			                                 true,
			                                 true,
			                                 true,
			                                 false,
			                                 false,
			                                 true);

			if (tempRet > -1)
				isScore = true;
			else
			{
				tempRet = editor->FindText("instr",
				                           true,
				                           true,
				                           true,
				                           false,
				                           false,
				                           true);

				if (tempRet > -1)
					isInstrument = true;
			}
		}




		gint tempindex = 0;
		gint lineLength = 0;

		//editor.TabIndent = mCodeFormat.TabIndent;

		editor->BeginUndoAction();

		for (gint i = mStart; i <= mEnd; i++)
		{

			//Real line length without LineEndings chars = 
			//SCI_GETLINEENDPOSITION(line) - SCI_POSITIONFROMLINE(line)
			//Skip empty lines
			lineLength = editor->getLineEndPosition(i) - editor->getPositionFromLineNumber(i);
			if (lineLength == 0)
				continue;


			//Retrieve the text of the line
			textline = wxGLOBAL->TrimLeft(editor->getTextOfLine(i)); //.TrimStart();

			//Skip line without chars (after TrimStart())
			//if (string.IsNullOrEmpty(textline)) continue;
			if(textline.size() < 1) continue;
			

			isSinglelineRem = false;
			isReplaced = false;
			startpos = editor->getPositionFromLineNumber(i);


			gchar** words = g_strsplit_set(textline.c_str(), delimiter, 0);
			int length = wxGLOBAL->ArrayLength(words);
	
			//foreach (string word in textline.Split(delimiter))
			for(int i=0; i < length; i++)
			{
				//Skip empty words
				//if (string.IsNullOrEmpty(word)) continue;
				Glib::ustring word = words[i];
				if(word.size() < 1) continue;

				
				//Check for single line rem
				//if (word.Contains(";") && isMultilineRem == false)
				if(word.find(";") != Glib::ustring::npos &&
				   isMultilineRem == false)
				{
					isSinglelineRem = true;
					//remIndex = textline.IndexOf(";");
					remIndex = word.find(";");
					break;
				}

				//Check for Multiline rem ('/*' and '*/')
				//if (word.Contains(@"/*") && isSinglelineRem == false)
				if(word.find("/*") != Glib::ustring::npos &&
				   isSinglelineRem == false)
				{
					isMultilineRem = true;
				}
				//if (word.Contains(@"*/") && isMultilineRem == true /*&& 
				//     isSinglelineRem == false*/
				//    )
				if(word.find("*/") != Glib::ustring::npos)
				{
					isMultilineRem = false;
				}
				if (isMultilineRem == true) continue;

				////Check for CsInstruments section
				//if (word == "<CsInstruments>" && isSinglelineRem == false 
				//    && isMultilineRem == false)
				//    isOrchestra = true;
				//if (word == "</CsInstruments>" && isSinglelineRem == false 
				//    && isMultilineRem == false)
				//    isOrchestra = false;

				//Check for CsScore section
				if (word == "<CsScore>" && isSinglelineRem == false
				    && isMultilineRem == false)
					isScore = true;
				if (word == "</CsScore>" && isSinglelineRem == false
				    && isMultilineRem == false)
					isScore = false;
				//If is CsScore section skip opcodes check
				if (isScore == true) break;


				//Check for opcodes
				if (word == "instr" ||
				    word == "endin" ||
				    word == "opcode" ||
				    word == "endop")
				{
					//Set isInstruments for further opcodes search
					if (word == "instr" && isSinglelineRem == false && isMultilineRem == false)
						isInstrument = true;
					if (word == "endin" && isSinglelineRem == false && isMultilineRem == false)
						isInstrument = false;

					if (wxSETTINGS->CodeFormat.FormatInstruments)
					{
						//Style 1: Add tab to start
						if (wxSETTINGS->CodeFormat.InstrumentsType == 0)
						{
							//textline = "\t" + textline;
							textline.insert(0, "\t");
							editor->ReplaceTarget(startpos, lineLength, textline.c_str());
						}
						//Style 2: No tab space at start
						else
						{
							editor->ReplaceTarget(startpos, lineLength, textline.c_str());
						}
					}
					isReplaced = true;
					break;
				}

				//else if (word.Contains("sr") ||
				//         word.Contains("kr") ||
				//         word.Contains("ksmps") ||
				//         word.Contains("nchnls") ||
				//         word.Contains("0dbfs"))
				else if (word == "sr" ||
				         word == "kr" ||
				         word == "ksmps" ||
				         word == "nchnls" ||
				         word == "0dbfs" ||
				         word == "sr=" ||
				         word == "kr=" ||
				         word == "ksmps=" ||
				         word == "nchnls=" ||
				         word == "0dbfs=")
				{
					if (wxSETTINGS->CodeFormat.FormatHeader)
					{
						//textline = Regex.Replace(textline, @"\s+", " "); // +newline;
						textline = Replace(textline, "(\\s+)", " "); // +newline;

						//tempindex = textline.IndexOf("=");
						tempindex = textline.find("=");

						//if (tempindex > -1)
						//	textline = "\t" + textline.Substring(0, tempindex).Trim() +
						//	"\t" + textline.Substring(tempindex).Trim();// + newline;
						if(tempindex != (gint)Glib::ustring::npos)
							textline =
								Glib::ustring::compose("\t%1\t%2",
								                       wxGLOBAL->Trim(textline.substr(0, tempindex)),
								                       wxGLOBAL->Trim(textline.substr(tempindex)));

						editor->ReplaceTarget(startpos, lineLength, textline.c_str());
					}
					isReplaced = true;
					break;
				}

				else if (word == "if" ||
				         word == "elseif" ||
				         word == "endif")
				{
					editor->ReplaceTarget(startpos, lineLength, textline.c_str());
					isReplaced = true;
					break;
				}

				//else if (KeyWords.Contains(word) || word == "=")
				else if (g_hash_table_lookup(keywords, word.c_str()) != NULL ||
				         word == "=")
				{
					//Replace multispace with one space
					//textline = Regex.Replace(textline, @"\s+", " ");
					textline = Replace(textline, "(\\s+)", " ");

					//Split line in single words
					//string[] split = textline.Split(delimiter);
					gchar** split = g_strsplit_set(textline.c_str(), delimiter, 0);
					int splitLength = wxGLOBAL->ArrayLength(split);
					
					Glib::ustring mCompose = "";

					//Check inside instr/endin section
					if (wxSETTINGS->CodeFormat.FormatInstruments &&
					    isInstrument == true)
					{
						if (wxSETTINGS->CodeFormat.InstrumentsType == 0)
						{
							//for (Int32 n = 0; n < split.Length; n++)
							for (gint n = 0; n < splitLength; n++)
							{
								//if (split[n] == word)
								if(strcmp(split[n], word.c_str()) == 0)
								{
									//mCompose += "\t" + split[n] + "\t";
									mCompose.append( 
										Glib::ustring::compose("\t%1\t",
										                       split[n]));
								}
								else 
								{
									//mCompose += split[n] + " ";
									mCompose.append(
										Glib::ustring::compose("%1 ",
										                       split[n]));
								}
							}
						}
						else
						{
							//mCompose = "\t" + textline;
							mCompose = "\t";
							mCompose.append(textline);
						}

						////isReplaced = true;
						////break;
					}

					else if (isInstrument == true)
					{
						isReplaced = false;
						break;
					}

					//check outside (but inside CsOrchestra)
					else if (isInstrument == false)
					{
						//for (Int32 n = 0; n < split.Length; n++)
						for (gint n= 0; n < splitLength; n++)
						{
							//if (split[n] == word)
							if(strcmp(split[n], word.c_str()) == 0)
							{
								//opcode is after the first word
								if (n > 0)
								{
									//mCompose += "\t"; //+ split[n] + "\t";
									mCompose.append("\t");
								}
								//mCompose += split[n] + "\t";
								mCompose.append(
									Glib::ustring::compose("%1\t",
									                       split[n]));
							}
							else 
							{
								//mCompose += split[n] + " ";
								mCompose.append(
									Glib::ustring::compose("%1 ",
									                       split[n]));
							}
						}
					}

					//Finally replace the line
					editor->ReplaceTarget(startpos, 
					                      lineLength, 
					                      wxGLOBAL->TrimRight(mCompose).c_str());

					g_strfreev(split);
					
					isReplaced = true;
					break;
				}
			}

			if (isSinglelineRem == true &&
			    isMultilineRem == false &&
			    isScore == true)
			{

				if (CheckScoreForGoodLine(textline) == false) continue;

				//remIndex = textline.IndexOf(";");
				remIndex = textline.find(";");
				
				//tempString = textline.Substring(0, remIndex);
				tempString = textline.substr(0, remIndex);

				//textline = Regex.Replace(tempString, @"\s+", " ") +
				//	textline.Substring(remIndex);
				Glib::ustring tmp = textline;
				tmp = Replace(tempString, "(\\s+)", " ");
				tmp.append(textline.substr(remIndex));
				textline = tmp;

				
				//wxGLOBAL->DebugPrint("Format Line1",
		        //         			 textline.c_str());
			

				
				//remIndex = textline.IndexOf(";");
				remIndex = textline.find(";");
				
				//tempString = textline.Substring(0, remIndex);
				tempString = textline.substr(0, remIndex);
				
				//textline = Regex.Replace(tempString, " ", "\t") +
				//	textline.Substring(remIndex);
				tmp = Replace(tempString, " ", "\t");
				tmp.append(textline.substr(remIndex));
				textline = tmp;

				//wxGLOBAL->DebugPrint("Format Line2",
		        //         			 textline.c_str());
				
			}
			else if (isSinglelineRem == false &&
			         isMultilineRem == false &&
			         isScore == true)
			{
				if (CheckScoreForGoodLine(textline) == false) continue;

				//textline = Regex.Replace(textline, @"\s+", " ");// + newline;
				textline = Replace(textline, "(\\s+)", " ");
				
				//textline = Regex.Replace(textline, " ", "\t");
				textline = Replace(textline, " ", "\t");
			}

			//Finally replace line
			if (isReplaced == false)
			{
				editor->ReplaceTarget(startpos, lineLength, textline.c_str());
			}

			g_strfreev(words);

		}

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxCodeFormatter - FormatCode", "ERROR");
	}

	editor->EndUndoAction();

}

Glib::ustring wxCodeFormatter::Replace(Glib::ustring text, 
                                       Glib::ustring regex, 
                                       Glib::ustring replaceText)
{
	try
	{
		//Glib::RefPtr<Glib::Regex> my_regex = Glib::Regex::create("(\\s+)");
		Glib::RefPtr<Glib::Regex> my_regex = Glib::Regex::create(regex);
		//Glib::ustring ret = my_regex->replace(text, 0, " ", Glib::REGEX_MATCH_NOTBOL);
		Glib::ustring ret = my_regex->replace(text, 0, replaceText, Glib::REGEX_MATCH_NOTBOL);
		
		return ret;
		
	}
	catch (...)
	{
		return text;
	}
}


bool wxCodeFormatter::CheckScoreForGoodLine(Glib::ustring& textline)
{
	//if (textline.StartsWith("f") &&
	//    wxGlobal.Settings.CodeFormat.FormatFunctions == false)
	if (Glib::str_has_prefix(textline, "f") &&
	    wxSETTINGS->CodeFormat.FormatFunctions == false)
		return false;

	//if (textline.StartsWith("i") &&
	//    wxGlobal.Settings.CodeFormat.FormatScoreInstruments == false)
	if (Glib::str_has_prefix(textline, "i") &&
	    wxSETTINGS->CodeFormat.FormatScoreInstruments == false)
		return false;

	//if (textline.StartsWith("t") &&
	//    wxGlobal.Settings.CodeFormat.FormatTempo == false)
	if (Glib::str_has_prefix(textline, "t") &&
	    wxSETTINGS->CodeFormat.FormatTempo == false)
		return false;

	return true;
}


void wxCodeFormatter::CreateNewFormatterWindow()
{
	//Create WinXound Settings Window
	Glib::RefPtr<Gtk::Builder> builder;
	try
	{
		builder = Gtk::Builder::create_from_file(
			Glib::ustring::compose("%1/winxound_formatter.ui", wxGLOBAL->getSrcPath()));
	}
	catch (const Glib::FileError & ex)
	{
		wxGLOBAL->DebugPrint("wxCodeFormatter constructor - Builder Error - Critical",
		                     ex.what().c_str());
		return;
	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxCodeFormatter constructor - Builder Error - Critical");
		return;
	}

	//Locate and Associate wxMainWindow
	builder->get_widget("windowFormatter", formatterWindow);
	builder->get_widget("checkbuttonHeader", checkbuttonHeader);
	builder->get_widget("checkbuttonInstruments", checkbuttonInstruments);
	builder->get_widget("checkbuttonFunctions", checkbuttonFunctions);
	builder->get_widget("checkbuttonScoreInstruments", checkbuttonScoreInstruments);
	builder->get_widget("checkbuttonTempo", checkbuttonTempo);
	builder->get_widget("radiobuttonStyle1", radiobuttonStyle1);
	builder->get_widget("radiobuttonStyle2", radiobuttonStyle2);
	builder->get_widget("buttonReset", buttonReset);
	builder->get_widget("buttonCancel", buttonCancel);
	builder->get_widget("buttonSave", buttonSave);
	builder->get_widget("vboxScintilla", vboxScintilla);


	checkbuttonHeader->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	checkbuttonInstruments->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	checkbuttonFunctions->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	checkbuttonScoreInstruments->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	checkbuttonTempo->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	radiobuttonStyle1->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	radiobuttonStyle2->signal_toggled().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_checkbutton_toggled));
	
	
	buttonCancel->signal_clicked().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_buttonCancel_Clicked));
	buttonSave->signal_clicked().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_buttonSave_Clicked));
	buttonReset->signal_clicked().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_buttonReset_Clicked));


	//TEXTVIEW
	textView = new wxTextEditor();
   	vboxScintilla->pack_start(*textView, TRUE, TRUE, 0);
	vboxScintilla->set_size_request(200,-1);

	ConfigureTextView();

	
	formatterWindow->set_resizable(FALSE);
	formatterWindow->set_size_request(800, 450);

	Glib::ustring iconfile = Glib::ustring::compose("%1/winxound_48.png",wxGLOBAL->getIconsPath());
	if(Glib::file_test(iconfile, Glib::FILE_TEST_EXISTS))
	formatterWindow->set_icon_from_file(iconfile);

	//settingsWindow->set_border_width(2);
	formatterWindow->signal_key_press_event().connect(
		sigc::mem_fun(*this, &wxCodeFormatter::on_key_press_event));
}


void wxCodeFormatter::on_buttonReset_Clicked()
{
	checkbuttonHeader->set_active(TRUE);
	checkbuttonInstruments->set_active(TRUE);
	checkbuttonFunctions->set_active(TRUE);
	checkbuttonScoreInstruments->set_active(TRUE);
	checkbuttonTempo->set_active(TRUE);

	radiobuttonStyle1->set_active(TRUE);
}

bool wxCodeFormatter::on_key_press_event(GdkEventKey* event)
{
	//wxGLOBAL->DebugPrint("KEY", "PRESSED");
	if (event->keyval == GDK_Escape)
	{
		on_buttonCancel_Clicked();
	}
	return false;
}

void wxCodeFormatter::on_buttonCancel_Clicked()
{
	this->closeWindow();
}

void wxCodeFormatter::on_buttonSave_Clicked()
{
	wxSETTINGS->CodeFormat.FormatHeader = checkbuttonHeader->get_active();
	wxSETTINGS->CodeFormat.FormatInstruments = checkbuttonInstruments->get_active();
	wxSETTINGS->CodeFormat.FormatFunctions = checkbuttonFunctions->get_active();
	wxSETTINGS->CodeFormat.FormatScoreInstruments = checkbuttonScoreInstruments->get_active();
	wxSETTINGS->CodeFormat.FormatTempo = checkbuttonTempo->get_active();
	if (radiobuttonStyle1->get_active())
		wxSETTINGS->CodeFormat.InstrumentsType = 0;
	else
		wxSETTINGS->CodeFormat.InstrumentsType = 1;

	wxSETTINGS->SaveSettings();
	
	closeWindow();
}

void wxCodeFormatter::on_checkbutton_toggled()
{	
	//wxGLOBAL->DebugPrint("TOGGLED","");

	bool FormatHeader = wxSETTINGS->CodeFormat.FormatHeader;
	bool FormatInstruments = wxSETTINGS->CodeFormat.FormatInstruments;
	bool FormatFunctions = wxSETTINGS->CodeFormat.FormatFunctions;
	bool FormatScoreInstruments = wxSETTINGS->CodeFormat.FormatScoreInstruments;
	bool FormatTempo = wxSETTINGS->CodeFormat.FormatTempo;
	gint type = wxSETTINGS->CodeFormat.InstrumentsType;

	wxSETTINGS->CodeFormat.FormatHeader = checkbuttonHeader->get_active();
	wxSETTINGS->CodeFormat.FormatInstruments = checkbuttonInstruments->get_active();
	wxSETTINGS->CodeFormat.FormatFunctions = checkbuttonFunctions->get_active();
	wxSETTINGS->CodeFormat.FormatScoreInstruments = checkbuttonScoreInstruments->get_active();
	wxSETTINGS->CodeFormat.FormatTempo = checkbuttonTempo->get_active();

	if (radiobuttonStyle1->get_active())
		wxSETTINGS->CodeFormat.InstrumentsType = 0;
	else
		wxSETTINGS->CodeFormat.InstrumentsType = 1;

	//wxGLOBAL->DebugPrint(wxGLOBAL->IntToString(wxSETTINGS->CodeFormat.InstrumentsType).c_str(),"");
	
	textView->setReadOnly(FALSE);
	FillTextView();
	FormatCode(textView, opcodes);
	textView->setReadOnly(TRUE);

	wxSETTINGS->CodeFormat.FormatHeader = FormatHeader;
	wxSETTINGS->CodeFormat.FormatInstruments = FormatInstruments;
	wxSETTINGS->CodeFormat.FormatFunctions = FormatFunctions;
	wxSETTINGS->CodeFormat.FormatScoreInstruments = FormatScoreInstruments;
	wxSETTINGS->CodeFormat.FormatTempo = FormatTempo;
	wxSETTINGS->CodeFormat.InstrumentsType = type;
}


void wxCodeFormatter::showWindowAt(gint x, gint y)
{
	//Show window
	if(formatterWindow == NULL) return;
	
	formatterWindow->show_all();
	formatterWindow->move(x - (formatterWindow->get_width() / 2), 
	                      y - (formatterWindow->get_height() / 2));
	formatterWindow->present(); //Deiconify if necessary
	formatterWindow->grab_focus();

	checkbuttonHeader->set_active(wxSETTINGS->CodeFormat.FormatHeader);
	checkbuttonInstruments->set_active(wxSETTINGS->CodeFormat.FormatInstruments);
	checkbuttonFunctions->set_active(wxSETTINGS->CodeFormat.FormatFunctions);
	checkbuttonScoreInstruments->set_active(wxSETTINGS->CodeFormat.FormatScoreInstruments);
	checkbuttonTempo->set_active(wxSETTINGS->CodeFormat.FormatTempo);

	radiobuttonStyle1->set_active(wxSETTINGS->CodeFormat.InstrumentsType == 0);
	radiobuttonStyle2->set_active(wxSETTINGS->CodeFormat.InstrumentsType == 1);	
}

void wxCodeFormatter::closeWindow()
{	
	formatterWindow->hide();
}


void wxCodeFormatter::ConfigureTextView()
{
	try
	{
		//Set Keywords list
		//Glib::ustring KeyWordList = "";
		std::string KeyWordList = "sr kr ksmps nchnls instr oscili out endin ";
		textView->setKeyWords(0, KeyWordList.c_str());

		textView->setKeyWords(1, 
		                      "<CsVersion> </CsVersion> "
		                      "<CsoundSynthesizer> </CsoundSynthesizer> "
		                      "<CsOptions> </CsOptions> "
		                      "<CsInstruments> </CsInstruments> "
		                      "<CsScore> </CsScore> "
		                      "<CsVersion> </CsVersion> "
		                      "<CsLicence> </CsLicence> ");



		textView->setWordChars("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.$_");

		SciEditSetFontsAndStyles();

		FillTextView();

		textView->SetUndoCollection(FALSE);
		textView->emptyUndoBuffer();
		textView->setSavePoint();
		textView->setReadOnly(TRUE);

		textView->setHighlight("winxound");

		

		opcodes = g_hash_table_new(g_str_hash, g_str_equal);
		//sr kr ksmps nchnls instr oscili out endin 
		g_hash_table_insert(opcodes, (gchar*)"sr", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"kr", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"ksmps", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"nchnls", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"instr", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"oscili", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"out", (gchar*)"");
		g_hash_table_insert(opcodes, (gchar*)"endin", (gchar*)"");

	}
	catch (...)
	{
		wxGLOBAL->DebugPrint("wxCodeFormatter::ConfigureTextView - ERROR");
	}
}

void wxCodeFormatter::SciEditSetFontsAndStyles()
{

	Glib::ustring mLanguage = "csound";


	//Set Default Font (name and size) to styles 0 -> 32
	textView->setTextEditorFont(wxSETTINGS->EditorProperties.DefaultFontName.c_str(),
	                              wxSETTINGS->EditorProperties.DefaultFontSize);



	//mSciEdit.SetZoom(0);
	textView->setZoom(0);
	//mSciEdit.SetTabWidth(wxGlobal.Settings.EditorProperties.DefaultTabSize);
	textView->setTabIndent(wxSETTINGS->EditorProperties.DefaultTabSize);


	//Set styles from 0 to 33
	//STYLE_NUMBERS_MARGINS = 33;
	
	for (gint i = 0; i < 34; i++)
	{
		//const gchar* temp = wxSETTINGS->StyleGetForeColor(mLanguage, i).c_str();
		textView->StyleSetFore(i, wxSETTINGS->StyleGetForeColor(mLanguage, i).c_str());
		textView->StyleSetBack(i, wxSETTINGS->StyleGetBackColor(mLanguage, i).c_str());
		textView->StyleSetBold(i, wxSETTINGS->StyleGetBold(mLanguage, i));
		textView->StyleSetItalic(i, wxSETTINGS->StyleGetItalic(mLanguage, i));
	}


	//DEFAULT STYLE "34" STYLE_BRACELIGHT
	/*
	 mSciEdit.StyleSetFore(SciConst.STYLE_BRACELIGHT,
	                       mSciEdit.StyleGetFore(32)); //TextForeColor
						   mSciEdit.StyleSetBack(SciConst.STYLE_BRACELIGHT,
						                         wxGlobal.Settings.EditorProperties.StyleGetBackColor(
						                                                                              mLanguage, 256));   //TextSelectionBackColor
																									  */
	textView->StyleSetFore(STYLE_BRACELIGHT, 
	                         wxSETTINGS->StyleGetForeColor(mLanguage, 32).c_str());
	textView->StyleSetBack(STYLE_BRACELIGHT, 
	                         wxSETTINGS->StyleGetBackColor(mLanguage, 256).c_str());




	//TEXT SELECTION (style 256)
	//mSciEdit.SetSelFore(true,
	//                    wxGlobal.Settings.EditorProperties.StyleGetForeColor(
	//                    mLanguage, 256));
	//To leave syntax highlight colors, rem the following line:
	textView->setSelFore(true, wxSETTINGS->StyleGetForeColor(mLanguage, 256).c_str());

	//mSciEdit.SetSelBack(true,
	//                    wxGlobal.Settings.EditorProperties.StyleGetBackColor(
	//                    mLanguage, 256));
	textView->setSelBack(true,
	                       wxSETTINGS->StyleGetBackColor(mLanguage, 256).c_str());


	//BOOKMARKS (style 257)
	//mSciEdit.MarkerSetBack(0, wxGlobal.Settings.EditorProperties.StyleGetBackColor(
	//                       mLanguage, 257));
	textView->MarkerSetBack(0, wxSETTINGS->StyleGetBackColor(mLanguage, 257).c_str());
	//mSciEdit.MarkerSetAlpha(0, wxGlobal.Settings.EditorProperties.StyleGetAlpha(
	//                       mLanguage, 257));
	textView->MarkerSetAlpha(0, wxSETTINGS->StyleGetAlpha(mLanguage, 257));


	//VERTICAL RULER (style 258)
	//mSciEdit.SetEdgeColour(wxGlobal.Settings.EditorProperties.StyleGetForeColor(
	//                       mLanguage, 258));
	textView->setEdgeColor(wxSETTINGS->StyleGetForeColor(mLanguage, 258).c_str());


	//CARET LINE MARKER (style 259)
	//mSciEdit.SetCaretLineBack(wxGlobal.Settings.EditorProperties.StyleGetForeColor(
	//                          mLanguage, 259));
	textView->setCaretLineBack(wxSETTINGS->StyleGetForeColor(mLanguage, 259).c_str());

	//CARET COLOR (Same as Text Fore Color)
	//mSciEdit.SetCaretFore(mSciEdit.StyleGetFore(32));
	textView->setCaretFore(wxSETTINGS->StyleGetForeColor(mLanguage, 32).c_str());

}

void wxCodeFormatter::FillTextView()
{
	textView->setText("<CsoundSynthesizer>\n"
	                  "<CsOptions>\n"
	                  "-W -odac\n"
	                  "</CsOptions>\n"
	                  "<CsInstruments>\n"
	                  ";Simple Oscili\n"
	                  "sr = 44100\n" 
	                  "kr = 4410\n"
	                  "ksmps = 10\n"
	                  "nchnls = 1\n\n"
	                  "instr 1\n"
	                  "a1 oscili 10000, 440, 1\n"
	                  "out a1\n"
	                  "endin\n" 
	                  "</CsInstruments>\n"
	                  "<CsScore>\n"
	                  "f1 0 4096 10 1\n"
	                  "i1 0 3\n"
	                  "t 0 60 40 60 45 30\n"
	                  "</CsScore>\n"
	                  "</CsoundSynthesizer>\n");
}
