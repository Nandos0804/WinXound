//
//  wxAnalysis.m
//  WinXound
//
//  Created by Stefano Bonetti on 04/03/10.
//

#import "wxAnalysis.h"
#import "wxGlobal.h"
#import "wxMainController.h"
#import "wxCompiler.h"



@implementation wxAnalysis


#pragma mark Initialization and Overrides
//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------
- (void) initialize 
{	
	
	//PROBABLY TO REMOVE (FOR STORE USER SETTINGS)
	//Prepare data tabs 
	[self SetDefaultValues:@"ATSA"];
	[self SetDefaultValues:@"CVANAL"];
	[self SetDefaultValues:@"HETRO"];
	[self SetDefaultValues:@"LPANAL"];
	[self SetDefaultValues:@"PVANAL"];
	
	mCurrentIndex = 0;
	mStopBatch = true;
	
	compiler = [[wxCompiler alloc] init];
	
}

- (void) dealloc
{
	[compiler release];
	[super dealloc];
}


- (void) tabView:(NSTabView *)tabView didSelectTabViewItem:(NSTabViewItem *)tabViewItem
{
	[self setTabViewItemState:tabViewItem];
}


- (void) setTabViewItemState:(NSTabViewItem*)tabViewItem
{
	if([[tabViewItem identifier] integerValue] == 5 && 
	   [comboInput isEnabled])
	{
		[textOutput setEnabled:false];
		[browseOutput setEnabled:false];
	}
	else if([comboInput isEnabled])
	{
		[textOutput setEnabled:true];
		[browseOutput setEnabled:true];
	}
}

//--------------------------------------------------------------------------------------------------
// Initialization and Overrides
//--------------------------------------------------------------------------------------------------












#pragma mark Various
//--------------------------------------------------------------------------------------------------
// Various methods
//--------------------------------------------------------------------------------------------------
- (void) showAnalysisWindow
{
	if (analysisWindow == nil) 
	{
		[NSBundle loadNibNamed:@"wxAnalysisWindow" owner:self];
		[analysisWindow setShowsToolbarButton:NO];
	}
	
	[analysisWindow makeKeyAndOrderFront:self];
	
}

- (void) SetDefaultValues:(NSString*) mUtilityName
{
	NSInteger tab = 0;
	if([mUtilityName isEqualToString:@"ATSA"])
		tab = 0;
	else if([mUtilityName isEqualToString:@"CVANAL"])
		tab = 1;
	else if([mUtilityName isEqualToString:@"HETRO"])
		tab = 2;
	else if([mUtilityName isEqualToString:@"LPANAL"])
		tab = 3;
	else if([mUtilityName isEqualToString:@"PVANAL"])
		tab = 4;
	
	switch (tab)
	{
			
		case 0: //"ATSA":
			//[[wxDefaults valueForKey: @"Analysis"] setValue:value forKey:@"xyz"];
			[wxDefaults setValue:@"0" forKey:@"b_Atsa"]; //b_Atsa = 0;
			[wxDefaults setValue:@"0" forKey:@"e_Atsa"]; //e_Atsa = 0;
			[wxDefaults setValue:@"20" forKey:@"l_Atsa"]; //l_Atsa = 20;
			[wxDefaults setValue:@"20000" forKey:@"Max_Atsa"]; //Max_Atsa = 20000;
			[wxDefaults setValue:@"0.1" forKey:@"d_Atsa"]; //d_Atsa = 0.1;
			[wxDefaults setValue:@"4" forKey:@"c_Atsa"]; //c_Atsa = 4;
			[wxDefaults setValue:[NSNumber numberWithInt:1] forKey:@"w_Atsa"]; //[w_Atsa selectItemAtIndex:1]; //Selected index
			[wxDefaults setValue:[NSNumber numberWithInt:3] forKey:@"F_File_Atsa"]; //[F_File_Atsa selectItemAtIndex: 3]; //Selected index
			[wxDefaults setValue:@"0.25" forKey:@"h_Atsa"]; //h_Atsa = 0.25;
			[wxDefaults setValue:@"-60" forKey:@"m_Atsa"]; //m_Atsa = -60;
			[wxDefaults setValue:@"3" forKey:@"t_Atsa"]; //t_Atsa = 3;
			[wxDefaults setValue:@"3" forKey:@"s_Atsa"]; //s_Atsa = 3;
			[wxDefaults setValue:@"3" forKey:@"g_Atsa"]; //g_Atsa = 3;
			[wxDefaults setValue:@"30" forKey:@"T_SMR_Atsa"]; //T_SMR_Atsa = 30;
			[wxDefaults setValue:@"60" forKey:@"S_SMR_Atsa"]; //S_SMR_Atsa = 60;
			[wxDefaults setValue:@"0" forKey:@"P_Peak_Atsa"]; //P_Peak_Atsa = 0;
			[wxDefaults setValue:@"0.5" forKey:@"M_SMR_Atsa"]; //M_SMR_Atsa = 0.5;
			break;
			
		case 1: //"CVANAL":
			[wxDefaults setValue:@"10000" forKey:@"s_Cvanal"]; //s_Cvanal = 10000;
			[wxDefaults setValue:@"0" forKey:@"c_Cvanal"]; //c_Cvanal = 0;
			[wxDefaults setValue:@"0" forKey:@"b_Cvanal"]; //b_Cvanal = 0;
			[wxDefaults setValue:@"0" forKey:@"d_Cvanal"]; //d_Cvanal = 0;
			break;
			
		case 2: //"HETRO":
			[wxDefaults setValue:@"10000" forKey:@"s_Hetro"]; //s_Hetro = 10000;
			[wxDefaults setValue:@"1" forKey:@"c_Hetro"]; //c_Hetro = 1;
			[wxDefaults setValue:@"0" forKey:@"b_Hetro"]; //b_Hetro = 0;
			[wxDefaults setValue:@"0" forKey:@"d_Hetro"]; //d_Hetro = 0;
			[wxDefaults setValue:@"100" forKey:@"f_Hetro"]; //f_Hetro = 100;
			[wxDefaults setValue:@"32767" forKey:@"Max_Hetro"]; //Max_Hetro = 32767;
			[wxDefaults setValue:@"256" forKey:@"n_Hetro"]; //n_Hetro = 256;
			[wxDefaults setValue:@"10" forKey:@"h_Hetro"]; //h_Hetro = 10;
			[wxDefaults setValue:@"64" forKey:@"min_Hetro"]; //min_Hetro = 64;
			[wxDefaults setValue:@"0" forKey:@"l_Hetro"]; //l_Hetro = 0;
			[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"SDIF_Hetro"];
			break;
			
		case 3: //"LPANAL":
			[wxDefaults setValue:@"10000" forKey:@"s_Lpanal"]; //s_Lpanal = 10000;
			[wxDefaults setValue:@"1" forKey:@"c_Lpanal"]; //c_Lpanal = 1;
			[wxDefaults setValue:@"0" forKey:@"b_Lpanal"]; //b_Lpanal = 0;
			[wxDefaults setValue:@"0" forKey:@"d_Lpanal"]; //d_Lpanal = 0;
			[wxDefaults setValue:@"34" forKey:@"p_Lpanal"]; //p_Lpanal = 34;
			[wxDefaults setValue:@"70" forKey:@"Min_Lpanal"]; //Min_Lpanal = 70;
			[wxDefaults setValue:@"200" forKey:@"Max_Lpanal"]; //Max_Lpanal = 200;
			[wxDefaults setValue:@"200" forKey:@"h_Lpanal"]; //h_Lpanal = 200;
			[wxDefaults setValue:[NSNumber numberWithInt:0] forKey:@"v_Lpanal"]; //[v_Lpanal selectItemAtIndex:0]; //SelectedIndex
			[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"a_Lpanal"]; //[a_Lpanal setState:NSOnState];
			[wxDefaults setValue:@"" forKey:@"Comments_Lpanal"]; //Comments_Lpanal = @"";
			break;
			
		case 4: //"PVANAL":
			[wxDefaults setValue:@"10000" forKey:@"s_Pvanal"]; //s_Pvanal = 10000;
			[wxDefaults setValue:@"1" forKey:@"c_Pvanal"]; //c_Pvanal = 1;
			[wxDefaults setValue:@"0" forKey:@"b_Pvanal"]; //b_Pvanal = 0;
			[wxDefaults setValue:@"0" forKey:@"d_Pvanal"]; //d_Pvanal = 0;
			[wxDefaults setValue:[NSNumber numberWithInt:7] forKey:@"n_Pvanal"]; //[n_Pvanal selectItemAtIndex:7]; // 7 = 2048;
			[wxDefaults setValue:[NSNumber numberWithInt:0] forKey:@"w_Pvanal"]; //[w_Pvanal selectItemAtIndex:0];
			[wxDefaults setValue:[NSNumber numberWithInt:3] forKey:@"h_Pvanal"]; //[h_Pvanal selectItemAtIndex:3];//3 = 128;
			[wxDefaults setValue:[NSNumber numberWithInt:1] forKey:@"window_Pvanal"]; //[window_Pvanal selectItemAtIndex:1]; // 1 = Von Hann;
			[wxDefaults setValue:[NSNumber numberWithBool:false] forKey:@"PvocExFile_Pvanal"];
			break;
			
	}
	
	//[[NSUserDefaultsController sharedUserDefaultsController] save:self];
	
}

- (NSString*) ATSA
{
	NSMutableString* tFlags = [NSMutableString stringWithString:@""];
	
	if([[wxDefaults valueForKey:@"b_Atsa"] doubleValue] > 0)//if (b_Atsa.Value > 0)
	{
		//tFlags += "-b" + b_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-b%@ ",[wxDefaults valueForKey:@"b_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"e_Atsa"] doubleValue] > 0)//if (e_Atsa.Value > 0)
	{
		//tFlags += "-e" + e_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-e%@ ",[wxDefaults valueForKey:@"e_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"l_Atsa"] integerValue] != 20)//if (l_Atsa.Value != 20)
	{
		//tFlags += "-l" + l_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-l%@ ",[wxDefaults valueForKey:@"l_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"Max_Atsa"] integerValue] != 20000)//if (Max_Atsa.Value != 20000)
	{
		//tFlags += "-H" + Max_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-H%@ ",[wxDefaults valueForKey:@"Max_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"d_Atsa"] doubleValue] != 0.1)//if ((double)d_Atsa.Value != 0.1)
	{
		//tFlags += "-d" + d_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-d%@ ",[wxDefaults valueForKey:@"d_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"c_Atsa"] integerValue] != 4)//if (c_Atsa.Value != 4)
	{
		//tFlags += "-c" + c_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-c%@ ",[wxDefaults valueForKey:@"c_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"w_Atsa"] integerValue] != 1)//if (w_Atsa.SelectedIndex != 1)
	{
		//tFlags += "-w" + w_Atsa.SelectedIndex + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-w%@ ",[wxDefaults valueForKey:@"w_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"h_Atsa"] doubleValue] != 0.25)//if ((double)h_Atsa.Value != 0.25)
	{
		//tFlags += "-h" + h_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-h%@ ",[wxDefaults valueForKey:@"h_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"m_Atsa"] doubleValue] != -60)//if (m_Atsa.Value != -60)
	{
		//tFlags += "-m" + m_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-m%@ ",[wxDefaults valueForKey:@"m_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"t_Atsa"] integerValue] != 3)//if (t_Atsa.Value != 3)
	{
		//tFlags += "-t" + t_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-t%@ ",[wxDefaults valueForKey:@"t_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"s_Atsa"] integerValue] != 3)//if (s_Atsa.Value != 3)
	{
		//tFlags += "-s" + s_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-s%@ ",[wxDefaults valueForKey:@"b_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"g_Atsa"] integerValue] != 3)//if (g_Atsa.Value != 3)
	{
		//tFlags += "-g" + g_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-g%@ ",[wxDefaults valueForKey:@"g_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"T_SMR_Atsa"] doubleValue] != 30)//if (T_SMR_Atsa.Value != 30)
	{
		//tFlags += "-T" + T_SMR_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-T%@ ",[wxDefaults valueForKey:@"T_SMR_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"S_SMR_Atsa"] doubleValue] != 60)//if (S_SMR_Atsa.Value != 60)
	{
		//tFlags += "-S" + S_SMR_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-S%@ ",[wxDefaults valueForKey:@"S_SMR_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"P_Peak_Atsa"] doubleValue] > 0)//if (P_Peak_Atsa.Value > 0)
	{
		//tFlags += "-P" + P_Peak_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-P%@ ",[wxDefaults valueForKey:@"P_Peak_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"M_SMR_Atsa"] doubleValue] != 0.5)//if ((double)M_SMR_Atsa.Value != 0.5)
	{
		//tFlags += "-M" + M_SMR_Atsa.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-M%@ ",[wxDefaults valueForKey:@"M_SMR_Atsa"]]]; 
	}
	if([[wxDefaults valueForKey:@"F_File_Atsa"] integerValue] != 3)//if (F_File_Atsa.SelectedIndex != 3)
	{
		//tFlags += "-F" + F_File_Atsa.SelectedIndex + 1 + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-F%d ",
							  [[wxDefaults valueForKey:@"F_File_Atsa"] integerValue] + 1
							  ]]; 
	}
	
	NSString* result = [tFlags stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	return result;
}


- (NSString*) CVANAL
{
	NSMutableString* tFlags = [NSMutableString stringWithString:@""];
	
	if([[wxDefaults valueForKey:@"s_Cvanal"] integerValue] > 10000)//if (s_Cvanal.Value != 10000)
	{
		//tFlags += "-s" + s_Cvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-s%@ ",[wxDefaults valueForKey:@"s_Cvanal"]]]; 
	}
	if([[wxDefaults valueForKey:@"c_Cvanal"] integerValue] > 0)//if (c_Cvanal.Value > 0)
	{
		//tFlags += "-c" + c_Cvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-c%@ ",[wxDefaults valueForKey:@"c_Cvanal"]]]; 
	}
	if([[wxDefaults valueForKey:@"b_Cvanal"] doubleValue] > 0)//if (b_Cvanal.Value > 0)
	{
		//tFlags += "-b" + b_Cvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-b%@ ",[wxDefaults valueForKey:@"b_Cvanal"]]]; 
	}
	if([[wxDefaults valueForKey:@"d_Cvanal"] doubleValue] > 0)//if (d_Cvanal.Value > 0)
	{
		//tFlags += "-d" + d_Cvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-d%@ ",[wxDefaults valueForKey:@"d_Cvanal"]]]; 
	}
	
	NSString* result = [tFlags stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	return result;
}


- (NSString*) HETRO
{
	NSMutableString* tFlags = [NSMutableString stringWithString:@""];
	
	if([[wxDefaults valueForKey:@"s_Hetro"] integerValue] != 10000)//if (s_Hetro.Value != 10000)
	{
		//tFlags += "-s" + s_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-s%@ ",[wxDefaults valueForKey:@"s_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"c_Hetro"] integerValue] != 1)//if (c_Hetro.Value != 1)
	{
		//tFlags += "-c" + c_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-c%@ ",[wxDefaults valueForKey:@"c_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"b_Hetro"] doubleValue] > 0)//if (b_Hetro.Value > 0)
	{
		//tFlags += "-b" + b_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-b%@ ",[wxDefaults valueForKey:@"b_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"d_Hetro"] doubleValue] > 0)//if (d_Hetro.Value > 0)
	{
		//tFlags += "-d" + d_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-d%@ ",[wxDefaults valueForKey:@"d_Hetro"]]];
	}
	
	if([[wxDefaults valueForKey:@"f_Hetro"] integerValue] != 100)//if (f_Hetro.Value != 100)
	{
		//tFlags += "-f" + f_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-f%@ ",[wxDefaults valueForKey:@"f_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"h_Hetro"] integerValue] != 10)//if (h_Hetro.Value != 10)
	{
		//tFlags += "-h" + h_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-h%@ ",[wxDefaults valueForKey:@"h_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"Max_Hetro"] integerValue] != 32767)//if (Max_Hetro.Value != 32767)
	{
		//tFlags += "-M" + Max_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-M%@ ",[wxDefaults valueForKey:@"Max_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"min_Hetro"] integerValue] != 64)//if (min_Hetro.Value != 64)
	{
		//tFlags += "-m" + min_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-m%@ ",[wxDefaults valueForKey:@"min_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"n_Hetro"] integerValue] != 256)//if (n_Hetro.Value != 256)
	{
		//tFlags += "-n" + n_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-n%@ ",[wxDefaults valueForKey:@"n_Hetro"]]];
	}
	if([[wxDefaults valueForKey:@"l_Hetro"] integerValue] > 0)//if (l_Hetro.Value > 0)
	{
		//tFlags += "-l" + l_Hetro.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-l%@ ",[wxDefaults valueForKey:@"l_Hetro"]]];
	}
	
	NSString* result = [tFlags stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	return result;
}


- (NSString*) LPANAL
{
	NSMutableString* tFlags = [NSMutableString stringWithString:@""];
	
	if([[wxDefaults valueForKey:@"s_Lpanal"] integerValue] != 10000)//if (s_Lpanal.Value != 10000)
	{
		//tFlags += "-s" + s_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-s%@ ",[wxDefaults valueForKey:@"s_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"c_Lpanal"] integerValue] != 1)//if (c_Lpanal.Value != 1)
	{
		//tFlags += "-c" + c_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-c%@ ",[wxDefaults valueForKey:@"c_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"b_Lpanal"] doubleValue] > 0)//if (b_Lpanal.Value > 0)
	{
		//tFlags += "-b" + b_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-b%@ ",[wxDefaults valueForKey:@"b_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"d_Lpanal"] doubleValue] > 0)//if (d_Lpanal.Value > 0)
	{
		//tFlags += "-d" + d_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-d%@ ",[wxDefaults valueForKey:@"d_Lpanal"]]];
	}
	
	if([[wxDefaults valueForKey:@"p_Lpanal"] integerValue] != 34)//if (p_Lpanal.Value != 34)
	{
		//tFlags += "-p" + p_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-p%@ ",[wxDefaults valueForKey:@"p_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"h_Lpanal"] integerValue] != 200)//if (h_Lpanal.Value != 200)
	{
		//tFlags += "-h" + h_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-h%@ ",[wxDefaults valueForKey:@"h_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"Max_Lpanal"] integerValue] != 200)//if (Max_Lpanal.Value != 200)
	{
		//tFlags += "-Q" + Max_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-Q%@ ",[wxDefaults valueForKey:@"Max_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"Min_Lpanal"] integerValue] != 70)//if (Min_Lpanal.Value != 70)
	{
		//tFlags += "-P" + Min_Lpanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-P%@ ",[wxDefaults valueForKey:@"Min_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"a_Lpanal"] integerValue] > 0)//if (a_Lpanal.Checked)
	{
		//tFlags += "-a ";
		[tFlags appendString:@"-a "];
	}
	if([[wxDefaults valueForKey:@"v_Lpanal"] integerValue] > 0)//if (!v_Lpanal.Text.Contains("none"))
	{
		//tFlags += "-v" + v_Lpanal.SelectedIndex + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-v%@ ",[wxDefaults valueForKey:@"v_Lpanal"]]];
	}
	if([[wxDefaults valueForKey:@"Comments_Lpanal"] length] > 0)//if (Comments_Lpanal.Text.Length > 0)
	{
		//tFlags += "-C" + Comments_Lpanal.Text + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-C%@ ",[wxDefaults valueForKey:@"Comments_Lpanal"]]];
	}
	
	NSString* result = [tFlags stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	return result;
}


- (NSString*) PVANAL
{
	NSMutableString* tFlags = [NSMutableString stringWithString:@""];
	
	if([[wxDefaults valueForKey:@"s_Pvanal"] integerValue] != 10000)//if (s_Pvanal.Value != 10000)
	{
		//tFlags += "-s" + s_Pvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-s%@ ",[wxDefaults valueForKey:@"s_Pvanal"]]];
	}
	if([[wxDefaults valueForKey:@"c_Pvanal"] integerValue] != 1)//if (c_Pvanal.Value != 1)
	{
		//tFlags += "-c" + c_Pvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-c%@ ",[wxDefaults valueForKey:@"c_Pvanal"]]];
	}
	if([[wxDefaults valueForKey:@"b_Pvanal"] doubleValue] > 0)//if (b_Pvanal.Value > 0)
	{
		//tFlags += "-b" + b_Pvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-b%@ ",[wxDefaults valueForKey:@"b_Pvanal"]]];
	}
	if([[wxDefaults valueForKey:@"d_Pvanal"] doubleValue] > 0)//if (d_Pvanal.Value > 0)
	{
		//tFlags += "-d" + d_Pvanal.Value + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-d%@ ",[wxDefaults valueForKey:@"d_Pvanal"]]];
	}
	
	
	//tFlags += "-n" + n_Pvanal.Text + " ";
	NSInteger value = [[wxDefaults valueForKey:@"n_Pvanal"] integerValue];
	[tFlags appendString:[NSString stringWithFormat:@"-n%d ",
						  (NSInteger)(16 * pow(2,value))
						  ]];
	
	
	//if (w_Pvanal.Text.Contains("Hop"))
	if([[wxDefaults valueForKey:@"w_Pvanal"] integerValue] == 0)
	{
		value = [[wxDefaults valueForKey:@"h_Pvanal"] integerValue];
		//tFlags += "-h" + h_Pvanal.Text + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-h%d ",
							  (NSInteger)(16 * pow(2,value))
							  ]];
	}
	//else if (w_Pvanal.Text != "4")
	else if([[wxDefaults valueForKey:@"w_Pvanal"] integerValue] > 0)
	{
		value = [[wxDefaults valueForKey:@"w_Pvanal"] integerValue];
		//tFlags += "-w" + w_Pvanal.Text + " ";
		[tFlags appendString:[NSString stringWithFormat:@"-w%d ",
							  (NSInteger)(pow(2,value))
							  ]];
	}
	
	
	//if (!window_Pvanal.Text.Contains("Hann")) //Hann = 1
	if([[wxDefaults valueForKey:@"window_Pvanal"] integerValue] != 1)
	{
		//if (window_Pvanal.Text.Contains("Hamming")) //Hamming = 0
		if([[wxDefaults valueForKey:@"window_Pvanal"] integerValue] == 0)
		{
			//tFlags += "-H ";
			[tFlags appendString:@"-H "];
		}
		else
		{
			//tFlags += "-K ";
			[tFlags appendString:@"-K "];
		}
	}
	
	NSString* result = [tFlags stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	return result;
}

//--------------------------------------------------------------------------------------------------
// Various methods
//--------------------------------------------------------------------------------------------------









#pragma mark - IB Actions
//----------------------------------------------------------------------------------------------------------
// IB ACTIONS AND RELATED METHODS
//----------------------------------------------------------------------------------------------------------
- (IBAction) wxAnalysisButtonResetClick:(id)sender
{
	//NSLog(@"Analysis Reset CLICK %@", [[tabView selectedTabViewItem] label]);
	
	if([[[tabView selectedTabViewItem] label] isEqualToString:@"Atsa"])
	{
		[self SetDefaultValues:@"ATSA"];
		//NSLog(@"Reset ATSA");
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Cvanal"])
	{
		[self SetDefaultValues:@"CVANAL"];
		//NSLog(@"Reset CVANAL");
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Hetro"])
	{
		[self SetDefaultValues:@"HETRO"];
		//NSLog(@"Reset HETRO");
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Lpanal"])
	{
		[self SetDefaultValues:@"LPANAL"];
		//NSLog(@"Reset LPANAL");
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Pvanal"])
	{
		[self SetDefaultValues:@"PVANAL"];
		//NSLog(@"Reset PVANAL");
	}
}

- (IBAction) wxAnalysisStartAnalysisClick:(id)sender
{
	if([[wxDefaults valueForKey:@"CSoundConsolePath"] isEqualToString:@""])
	{
		[wxMAIN ShowMessage:@"CSound Compiler not found!" 
			informativeText:@"Please specify a valid path in your WinXound Preferences (Directories tab)." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	[self StartCompiler];
}

- (IBAction) wxAnalysisStopClick:(id)sender
{
	@try
	{
		if(compiler != nil)
			[compiler stopProcess];
	}
	@catch (NSException * e) 
	{
		NSLog(@"wxAnalysis -> wxAnalysisStopClick Error: %@ - %@", [e name], [e reason]);
	}
}										 

- (IBAction) wxAnalysisStopBatch:(id)sender
{
	//WxUtilityConsole1.buttonStopBatch.Text = "Stopping batch (Wait current) ...";
	[buttonStopBatch setTitle:@"Stopping batch (Wait current) ..."];

	mStopBatch = true;
	//[buttonStopCompiler performClick:self];
	
}


// Browse buttons implementation
//------------------------------
- (IBAction) wxAnalysisButtonInputClick:(id)sender
{
	
	//NSInteger tagSender = [(NSButton *)sender tag];
	//NSLog(@"%d",tagSender);
	
	NSOpenPanel *openPanel = [NSOpenPanel openPanel];
	
	[openPanel setResolvesAliases:YES];
	[openPanel setAllowsMultipleSelection:true];
	[openPanel setCanChooseFiles: true];
	[openPanel setCanChooseDirectories:false];
	
	
	//SHEET WINDOW
	[openPanel beginSheetForDirectory:nil
								 file:nil
								types:[NSArray arrayWithObjects:@"wav",@"aiff",@"aif",nil]
					   modalForWindow:analysisWindow
						modalDelegate:self
					   didEndSelector:@selector(openPanelDidEnd:
												returnCode:
												contextInfo:)
						  contextInfo:@"INPUT"];
	
	
}

- (IBAction) wxAnalysisButtonOutputClick:(id)sender
{	
	NSOpenPanel *openPanel = [NSOpenPanel openPanel];
	
	[openPanel setResolvesAliases:YES];	
	[openPanel setAllowsMultipleSelection:false];
	[openPanel setCanChooseFiles: false];
	[openPanel setCanChooseDirectories:true];
	
	
	//SHEET WINDOW
	[openPanel beginSheetForDirectory:nil
								 file:nil
								types:nil
					   modalForWindow:analysisWindow 
						modalDelegate:self
					   didEndSelector:@selector(openPanelDidEnd:
												returnCode:
												contextInfo:)
						  contextInfo:@"OUTPUT"];
	
}

- (void)openPanelDidEnd:(NSOpenPanel *)panel returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	@try
	{
		if (returnCode == NSOKButton)
		{
			NSArray *filesToOpen = [panel URLs];
			//NSString* filename = [[panel filenames] objectAtIndex:0];
			NSString* info = (NSString*)contextInfo;
			//NSLog(@"info: %@",info);
			
			if([info isEqualToString:@"INPUT"])
			{
				//NSComboBox* comboInput = [self getInputComboBox:[info integerValue]];
				//NSTextField* textOutput = [self getOutputTextField:[info integerValue] + 10];
				if(comboInput == nil || textOutput == nil) return;
				
				//Notify user that there are other files into the list
				//if (input.Items.Count > 0)
				if([comboInput numberOfItems] > 0)
				{
					NSInteger ret =
					[wxMAIN ShowMessage:@"Input Files List Alert:"
						informativeText:@"There are one or more files in the input list.\n" 
					 "Do you want to keep them (the new files will be added)?"
						  defaultButton:@"YES" 
						alternateButton:@"NO" 
							otherButton:@"Cancel"];
					
					if (ret == NSAlertThirdButtonReturn) return; //CANCEL
					
					if (ret == NSAlertSecondButtonReturn)
					{
						//input.Items.Clear();
						//output.Clear();
						[comboInput removeAllItems];
						[textOutput setStringValue:@""];
						
					}
				}
				
				
				for(NSURL* url in filesToOpen)
				{
					[comboInput addItemWithObjectValue:[url path]];
				}
				
				[comboInput selectItemAtIndex:0];
				
				if ([[textOutput stringValue] length] == 0)
				{
					NSString* path = [[comboInput itemObjectValueAtIndex:0] stringByDeletingLastPathComponent];
					[textOutput setStringValue:path];
				}
				
			}
			else if([info isEqualToString:@"OUTPUT"])
			{
				NSString* path = [[panel filenames] objectAtIndex:0];
				[textOutput setStringValue:path];
			}
		}
		
		
	}
	@catch (NSException * e) 
	{
		[wxMAIN ShowMessageError:@"wxAnalysis -> openPanelDidEnd:" 
						   error:[NSString stringWithFormat:@"%@\n%@", [e name], [e reason]]];
	}
}



- (IBAction) wxAnalysisButtonHelpClick:(id)sender
{
	NSString* utilityName = nil;
	
	if([[[tabView selectedTabViewItem] label] isEqualToString:@"Atsa"])
	{
		utilityName = @"UtilityAtsa";
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Cvanal"])
	{
		utilityName = @"cvanal";
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Hetro"])
	{
		utilityName = @"hetro";
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Lpanal"])
	{
		utilityName = @"lpanal";
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Pvanal"])
	{
		utilityName = @"pvanal";
	}
	else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Sndinfo"])
	{
		utilityName = @"sndinfo";
	}
	
	if(utilityName != nil)
		[wxMAIN showHelpFor:utilityName];
}

//----------------------------------------------------------------------------------------------------------
// IB ACTIONS
//----------------------------------------------------------------------------------------------------------















#pragma mark - Compiler methods
//----------------------------------------------------------------------------------------------------------
// COMPILER
//----------------------------------------------------------------------------------------------------------
- (void) StartCompiler  //string arguments)
{	
	if([[comboInput stringValue] length] == 0)
	{
		[wxMAIN ShowMessage:@"Input file not specified!" 
			informativeText:@"Please specify a valid input file before to start CSound analysis." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	if([[textOutput stringValue] length]== 0)
	{
		[wxMAIN ShowMessage:@"Output path not specified!" 
			informativeText:@"Please specify a valid output path before to start CSound analysis." 
			  defaultButton:@"OK"
			alternateButton:nil 
				otherButton:nil];
		return;
	}
	
	
	
	
	//csound -+msg_color=false -U atsa   "/Users/teto/Desktop/File CSD/fm_01.wav" "/Users/teto/Desktop/File CSD/fm_01.ats"
	
	//if([comboInput isEnabled]) 
	[comboInput selectItemAtIndex:mCurrentIndex];
	
	NSString* flags = @"";
	NSMutableString* filename1 = [NSMutableString stringWithString:[comboInput stringValue]];
	NSMutableString* filename2 = [NSMutableString stringWithString:@""];
	
	NSMutableString* arguments = [NSMutableString stringWithFormat:
								  @"-U%@ ",
								  [[[tabView selectedTabViewItem] label] lowercaseString]];
	
	BOOL isSndinfo = false;
	
	//Retrieve Flags
	switch ([[[tabView selectedTabViewItem] identifier] integerValue])
	{
		case 0://"Atsa":
			flags = [self ATSA]; 
			break;
		case 1://"Cvanal":
			flags = [self CVANAL];
			break;
		case 2://"Hetro":
			flags = [self HETRO];
			break;
		case 3://"Lpanal":
			flags = [self LPANAL];
			break;
		case 4://"Pvanal":
			flags = [self PVANAL];
			break;
		case 5:
			isSndinfo = true;
			break;
	}
	
	//arguments += flags + "\"" + input.Text.Trim() + "\"";
	[arguments appendString:[NSString stringWithFormat:
							 @"%@ ",
							 flags]];
	//[comboInput stringValue]]];
	
	
	
	//Create filename2 (output example: xyz.ats)
	if([[textOutput stringValue] length] > 0 && isSndinfo == false)
	{
		//arguments += " " + "\"" + output.Text.Trim();
		[filename2 appendString:[textOutput stringValue]];
		
		
		//string f = Path.GetFileName(input.Text.Trim());
		NSString* f = [[comboInput stringValue] lastPathComponent];
		//string o = "";
		NSMutableString* o = [NSMutableString stringWithFormat:@""];
		
		//if (f.Contains("."))
		if([f rangeOfString:@"."].location != NSNotFound)
		{
			//o = f.Substring(0, f.LastIndexOf("."));
			//o = [f stringByDeletingPathExtension];
			[o appendString:[f stringByDeletingPathExtension]];
		}
		else
		{
			//o = f;
			[o appendString:f];
		}
		
		
		//Output filename extension (check for pvanal and hetro tools)
		if([[[tabView selectedTabViewItem] label] isEqualToString:@"Pvanal"])
		{
			if([[wxDefaults valueForKey:@"PvocExFile_Pvanal"] integerValue] > 0) //Extended format (pvoc-ex)
				[o appendString:@".pvx"];
			else
				[o appendString:@".pv"]; //Simple format
			
		}
		else if([[[tabView selectedTabViewItem] label] isEqualToString:@"Hetro"] &&  //Sdif extension
				[[wxDefaults valueForKey:@"SDIF_Hetro"] integerValue] > 0)
		{
			[o appendString:@".sdif"];
		}
		else
		{
			[o appendString:[NSString stringWithFormat:
							 @".%@", 
							 [[[[tabView selectedTabViewItem] label] lowercaseString] substringToIndex:3]
							 ]];
		}
		
		
		if(![[textOutput stringValue] hasSuffix:@"/"])
		{
			[filename2 appendString:@"/"];
		}
		[filename2 appendString:o];
	}
	else if(isSndinfo == true)
	{
		[filename2 appendString:filename1];
	}
	
	
	
	
	//Replace "," with "." inside arguments string 
	//	 arguments = arguments.Replace(",", ".");
	[arguments replaceOccurrencesOfString:@"," 
							   withString:@"."
								  options:NSCaseInsensitiveSearch 
									range:NSMakeRange(0, [arguments length])];
	
	NSString* finalArguments = [arguments stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]]; 
	
	
	if([comboInput numberOfItems] > 1)
	{
		mStopBatch = false;
		[buttonStopBatch setTitle:@"Stop batch process"];
		[buttonStopBatch setHidden:false];
		[compilerHost setTitle:[NSString stringWithFormat:
								@"Batch processing: %@ [%d of %d]",
								filename2, //[comboInput stringValue],
								mCurrentIndex + 1,
								[comboInput numberOfItems]]];
	}
	else
	{
		[compilerHost setTitle:[NSString stringWithFormat:
								@"Compiler output: %@",
								//finalArguments,
								filename2]];
	}
	
	
	//CHECK CONTROLS APPEARANCE
	[comboInput setEnabled:false];
	[textOutput setEnabled:false];
	[browseInput setEnabled:false];
	[browseOutput setEnabled:false];
	
	[buttonStartCompiler setEnabled:false];
	[buttonStopCompiler setHidden:false];
	[buttonStopCompiler setHidden:false];
	
	
	//Call csound compiler [Utility -U] 
	//Example: " -U cvanal [FLAGS] inputfile outputfile" 
	NSString* compilerPath = [wxDefaults valueForKey:@"CSoundConsolePath"];
	
	//ONLY FOR DEBUG
	//NSLog(@"Compiler arguments: %@", finalArguments);
	//[self compilerCompleted];
	//return;
	
	if(isSndinfo) 
		filename2 = nil;
	
	
	//If the current batch file is < to 1 we must clear the output text
	if(mCurrentIndex < 1 || [[compilerOutputAnalysis string] length] >= NSIntegerMax)
		[compilerOutputAnalysis setString:@""];
	//else we must append two LF
	else 
		[[[compilerOutputAnalysis textStorage] mutableString] appendString:@"\n\n"];
	
	
	[compiler compile:compilerPath
		   parameters:finalArguments 
			filename1:filename1
			filename2:filename2
			   output:compilerOutputAnalysis
			   button:buttonStopCompiler
				owner:self];
	
	
	//TODO: RELEASE OBJECTS !!!
	
}



- (void) compilerCompleted
{
	
	//NSLog(@"wxAnalysis Compiler Completed");
	
	
	mCurrentIndex++;
	
	//if (mCurrentIndex < input.Items.Count && mStopBatch == false)
	if(mCurrentIndex < [comboInput numberOfItems] && mStopBatch == false)
	{
		////buttonStopBatch.Visible = true;
		//input.SelectedIndex = mCurrentIndex;
		[comboInput selectItemAtIndex:mCurrentIndex];
		
		//StartCompiler();
		[self StartCompiler];
		return;
	}
	else
	{
		mStopBatch = true;
		mCurrentIndex = 0;
		
		[comboInput setEnabled:true];
		[textOutput setEnabled:true];
		[buttonStartCompiler setEnabled:true];
		[buttonStopCompiler setHidden:true];
		[buttonStopBatch setHidden:true];
		[browseInput setEnabled:true];
		[browseOutput setEnabled:true];
		
		[compilerHost setTitle:[NSString stringWithFormat:@"%@ - COMPLETED", [compilerHost title]]];
		[self setTabViewItemState:[tabView selectedTabViewItem]];
	}
}



@end
