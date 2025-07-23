/**
 Whisper Speech To Text
**/


using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization; 
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Diagnostics;
using ScriptPortal.Vegas;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 

static class Globals
{
	public static string keuze;
}

public class EntryPoint
{
	Vegas myVegas;
	public void FromVegas(Vegas vegas)
	{
		myVegas = vegas;
		bool eventFound = false; //initate candidate for speech-to-text found to false
		foreach (Track myTrack in myVegas.Project.Tracks)
		{
			if (myTrack.Selected && myTrack.IsAudio())
			{	  
				foreach(TrackEvent myEvnt in myTrack.Events)
				{
					//determine if cursor is on selected speech event to be handled
					if (myEvnt.Selected)
					{
						//event is Selected and event track is audio
						eventFound = true; //candidate forspeech to text found
					    //determine path and name of stored media of selected event
						string myPathPlusFileName = myEvnt.ActiveTake.MediaPath;  
						string myPath = Path.GetDirectoryName(myPathPlusFileName);
						string myFile = Path.GetFileName(myPathPlusFileName);		                             
						//MessageBox.Show("myPath: " + myPath);
						//MessageBox.Show("myFile: " + myFile);
					    
						//**********************************************
						//ASK : select whisper model and start or cancel
						//**********************************************
						string title = "Whisper Model (Speed vs Accuracy)";
						string prompt = "Select Transcode Model";
						string value1 = " ";
						string resultaat = "";
						Globals.keuze = "";
						if (CreateUserDialog.InputBox(title, prompt, ref value1) == DialogResult.OK)
						{ 
							MessageBox.Show("testpunt 0 - should not come here- all buttons are defined CANCEL");
						}				 
						//*********************************	
						// end ask whisper model selection
                        //*********************************
						string modelOption = "";
						switch (Globals.keuze)
						{
							case "b1 balanced":						
								modelOption = " --model small.en";	
						        break;
						    case "b2 draft":						
								modelOption = " --model tiny.en";
						        break;
							case "b3 best":						
								modelOption = " --model medium.en";	
						        break;
						    case "b4 translate":						
								modelOption = " --model large --task translate";
						        break;						
							case "b5 balanced":						
								modelOption = " --model small";		
						        break;
						    case "b6 draft":						
								modelOption = " --model tiny";
						        break;
							case "b7 best":						
								modelOption = " --model large";
						        break;
						    case "b8 cancel":						
								modelOption = "CANCEL";	
						        break;
						}
                          
                        //MessageBox.Show("testpunt case: " + Globals.keuze);	
						
                        if (modelOption != "CANCEL")
						{ // = execute if not cancel!
                        
							//*****************************************						 
							//start process to to send windows commands
							//*****************************************  						
							try
							{	
								Process p = new Process();
								ProcessStartInfo info = new ProcessStartInfo();				
													
								//startInfo.CreateNoWindow = false;
								info.UseShellExecute = false;
								info.RedirectStandardOutput = true;
								info.RedirectStandardInput = true;
								info.WorkingDirectory = myPath; // update v4: start process in the path of the media iso path of Vegas
								info.FileName = "cmd.exe";
								
								p.StartInfo = info;
								p.Start();
								
								using (StreamWriter sw = p.StandardInput)
								{
									if (sw.BaseStream.CanWrite)
									{
										//change directory to stored media location of event 
										//update v4 : not needed any more since the process is now started in the path of the media iso path of Vegas 
										//sw.WriteLine("CD " + myPath);   
										
										//call phyton application whisper with event name as argument			
										sw.WriteLine("whisper " + "\"" + myFile + "\"" + modelOption); //temp remove for speed testing rest of APP

										// add progress bar 
										// end progress bar
										sw.Close();
										p.WaitForExit();
										p.Close();
									}
								}
								//********************************						 
							    //End of process (files generated)
							    //*********************************
								
								//Update V5
								//construct alternative (compatible) whisper result if result is without media type sufffix 
								//NOTE should not be, but some people have this issue?
								string myfileNoExtension = Path.GetFileNameWithoutExtension(myPathPlusFileName);
								string resultFile = Path.Combine(myPath, myfileNoExtension) + ".srt";  	
								string resultFileLong = myPathPlusFileName + ".srt";
								if (File.Exists(resultFile)) //only create new resultfile if missing suffix 
								{
									File.Copy(resultFile, resultFileLong, true);  //true means overwrites
									MessageBox.Show("Compatible Speech to Text files are saved!");
								} else {
									MessageBox.Show("Speech to Text files are saved!");
								}	// end V5									
								
							}
							catch (Exception e)
							{
								MessageBox.Show(e.Message);	
							}
							
                        } else {
							MessageBox.Show("No Text files were generated or saved!");
						}
						
					    //*********************************
						//ASK : INSERT TEXT in new track?
						//*********************************
							 
						string title2 = "Subtitle Insert";
						string prompt2 = "Do you want to Insert the Subtitle on a new Track?";
						if (modelOption == "CANCEL")
						{ 
							prompt2 = "Insert Existing Subtitle file (.srt) at event?" + Environment.NewLine + "(make sure it exists)";
					    }
					    string value2 = " ";
						string resultaat2 = "";
						string myText="";
						Timecode myEvStart = new Timecode("00:00:00:00");
						Timecode myEvEnd = new Timecode("00:00:00:00");
						Timecode myEvLength = new Timecode("00:00:00:00");
						
						if (CreateUserDialog.InputBox2(title2, prompt2, ref value2) == DialogResult.OK)
						{
							resultaat2 = value2;
							//MessageBox.Show("testpunt: insert yes");
							// ----------------------------------
							// if answer is Yes start insert here 
							//-----------------------------------
							
							// update v6: sets timeline timecode format to "Time" 
							var originalRulerFormat = myVegas.Project.Ruler.Format;
                            myVegas.Project.Ruler.Format = RulerFormat.Time;
                            myVegas.UpdateUI();
							
	                        //adds empty new video track at top
							myVegas.Project.AddVideoTrack();  
							Track myNewTrack = myVegas.Project.Tracks[0];
							
							//split the srt into a linked list		
							List<SrtInfo> subtitles = MakeLinkedList(myVegas, myPathPlusFileName);
							
                            // add subtitle events to "titles and text OFX" to the new first track
							foreach (SrtInfo x in subtitles) 
							{		  
							    //prepare text, position and duration
								myText = String.Format(x.getText());
								myEvStart = x.getStartTime();
								myEvEnd = x.getEndTime();
								myEvLength = x.getEndMinusStartTime();
								//execute add text
								AddTextEvent(myVegas, myNewTrack, myText, myEvStart, myEvLength);
							}//*** END INSERT TEXT  ****
							// update v6: sets timeline timecode format back to the user preference 
							myVegas.Project.Ruler.Format = originalRulerFormat;
						}
						break;			
					} //*** END of if event selected
				} //*** END of foreach(TrackEvent 
				break; 	  //NOTE: insert text creates new track, the new track must not be part of foreach loop			
			} //*** END of if (myTrack.Selected						
		} //*** END of foreach (Track myTrack in myVegas.Project.Tracks)
        if (!eventFound) //candidate forspeech to text not found		
		{
			MessageBox.Show("Please select correct Audio Event");
		}
	}
	
	void AddTextEvent(Vegas myVegas, Track myNewTrack, string myText, Timecode myEvStart, Timecode myEvLength )
	{
		string genUID = "{Svfx:com.vegascreativesoftware:titlesandtext}"; //Magix Titles & Text
        string vegVersion = myVegas.Version.PadRight(12).Substring(0,12).TrimEnd(); //to remove build version 
		//MessageBox.Show("vegVersion: " + vegVersion);
		if (vegVersion.Contains("14") || vegVersion.Contains("15") || vegVersion.Contains("16"))
		{
			genUID = "{Svfx:com.sonycreativesoftware:titlesandtext}"; //Sony Titles & Text
		} 
		
		PlugInNode plugIn = null;
		plugIn = myVegas.Generators.GetChildByUniqueID(genUID);
		
		Media media = new Media(plugIn);
		MediaStream stream = media.Streams.GetItemByMediaType(MediaType.Video, 0);
		//VideoEvent newEvent = new VideoEvent(myVegas.Transport.CursorPosition, Timecode.FromSeconds(15)); //15 seconds long !!!
		VideoEvent newEvent = new VideoEvent((myVegas.Transport.CursorPosition + myEvStart), myEvLength);  
		myNewTrack.Events.Add(newEvent);
		Take take = new Take(stream);
		newEvent.Takes.Add(take);
		
	    //add preset to generated event
		//get the actual OFX effect  (gEffect: generated effect)
		Effect gEffect = newEvent.ActiveTake.Media.Generator;
		OFXEffect fxo = gEffect.OFXEffect;
		
		//modify Text richfont .net type properties (font, size, fontstyle, justify alignment)
		OFXStringParameter tparm = (OFXStringParameter)fxo.FindParameterByName("Text");
		RichTextBox rtfText = new RichTextBox();
		
		//rtfText.Text = " Swapno has text";
		rtfText.Text = myText;
		rtfText.SelectAll();
		rtfText.SelectionFont = new Font("Arial", 10, FontStyle.Bold); //Bold-Italic-Regular-Strikeout-Underline
		rtfText.SelectionAlignment = HorizontalAlignment.Center;       //alignment of text in textbox itself (=justify)
		tparm.Value = rtfText.Rtf;

		//Text font color - keep default white
		
		//Text anchorPoint		
		OFXChoiceParameter anchorPoint = (OFXChoiceParameter)fxo.FindParameterByName("Alignment");
		anchorPoint.Value = anchorPoint.Choices[4]; //4=center
        
		//Text position
	    OFXDouble2DParameter position = (OFXDouble2DParameter)fxo.FindParameterByName("Location");
		OFXDouble2D Pos;
		Pos.X = .5;
		Pos.Y = .1;
		position.Value = Pos;
		
		//Text shadow
		OFXBooleanParameter shadow = (OFXBooleanParameter)fxo.FindParameterByName("ShadowEnable");
		shadow.Value = true;
	    OFXDoubleParameter shadowOffsetX = (OFXDoubleParameter)fxo.FindParameterByName("ShadowOffsetX");
		shadowOffsetX.Value = 0.01;
	    OFXDoubleParameter shadowOffsetY = (OFXDoubleParameter)fxo.FindParameterByName("ShadowOffsetY");
		shadowOffsetY.Value = 0.01;
	    OFXDoubleParameter shadowBlur= (OFXDoubleParameter)fxo.FindParameterByName("ShadowBlur");
		shadowBlur.Value = 0.01;
		
		//Text background
		OFXRGBAParameter rgbBackground = (OFXRGBAParameter)fxo.FindParameterByName("Background");
		Color bgColor = Color.White;
		
		OFXColor bgOFXColor;
		bgOFXColor.R = bgColor.R / 255.0;
		bgOFXColor.G = bgColor.G / 255.0;
		bgOFXColor.B = bgColor.B / 255.0;
        bgOFXColor.A = 0;       // make transparant
		rgbBackground.Value = bgOFXColor;
		
		
	    //OFXBooleanParameter cropBGTT = (OFXBooleanParameter)fxo.FindParameterByName("FitBackgroundColor");
		//cropBGTT.Value = true;
		
		//Line spacing (space between lines)
		OFXDoubleParameter lineSpacing = (OFXDoubleParameter)fxo.FindParameterByName("LineSpacing");
		lineSpacing.Value = 0.8;
		
        /* helpfull to find syntax and parameters 
		   foreach (OFXParameter fxparm in fxo.Parameters)
            {
                MessageBox.Show("Name: " + fxparm.Name + "  Label: " + fxparm.Label + "   Type: " + fxparm.ParameterType.ToString());
            }
		*/    
		
		//apply all changes
		fxo.AllParametersChanged();						
	  
	}//*** end add text event *********************************************************************
	 

	static List<SrtInfo> MakeLinkedList (Vegas myVegas, string myPathPlusFileName)
	{	
		// read in the file            
		List<SrtInfo> subtitles = new List<SrtInfo>();
		string s = "";
		int currentLineIndex = 0;
		// define the text generated filename to parse with full path (= add .srt)
		myPathPlusFileName = myPathPlusFileName + ".srt"; 
		// read in the file
		List<string> inputStrings = new List<string>();
		using (StreamReader sr = new StreamReader(myPathPlusFileName, Encoding.Default))
		{
			try
			{
				bool isNotEmptySubtitle = false;

				while ((s = sr.ReadLine()) != null)
				{
					currentLineIndex++;
					if (s.Length != 0)
					{
						inputStrings.Add(s);
						isNotEmptySubtitle = true;
					}
					else if (inputStrings.Count > 1)
					{
						subtitles.Add(new SrtInfo(inputStrings));
						inputStrings.Clear();
						isNotEmptySubtitle = false;
					}
				}

				if (isNotEmptySubtitle && inputStrings.Count > 1)
				{
					subtitles.Add(new SrtInfo(inputStrings));
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(String.Format("Error reading the file. Maybe an invalid *.srt file. Error occured at line: {0}\r\nError message: {1}\r\nDetailed error message for debugging: {2}", currentLineIndex, e.Message, e.ToString()));
				MessageBox.Show(String.Format("There has been an error while importing the subtitles. The subtitles and their timings may not be loaded correctly. Look it up, what could be the problem in your *.srt file around line {0}.\r\nFor example too many empty lines or the timecode format is wrong.", currentLineIndex));
			}
		}	
		/*
		foreach (SrtInfo x in subtitles) // for testpurposes: show linked list
		{		  
			MessageBox.Show(String.Format(x.getText()));
		}*/
		return subtitles;
		
	} // *** end of MakeLinkedList ***************************************************************
	
} //********* end of main procedure ********************************************************************************


public class CreateUserDialog
{
	public static DialogResult InputBox(string title, string promptText, ref string value)
	{
		// -------------------------------------------------------------
		// part 1 create form  & buttons for transcode model sellection
		// ------------------------------------------------------------- 
		
	    Form form = new Form();
	    Label label1 = new Label();
		Label label2 = new Label();
		  //TextBox textBox = new TextBox();  //no need for textbox
		Button button1 = new Button();
		Button button2 = new Button();
		Button button3 = new Button();
		Button button4 = new Button();
	    Button button5 = new Button();
		Button button6 = new Button();
		Button button7 = new Button();
		Button button8 = new Button();

		form.Text = title;
		label1.Text = promptText + " (English only):";
		label2.Text = promptText + " (International):";
		  //textBox.Text = value;   //no need for textbox
	       
		  // -------------------
          // what the user sees
		  // -------------------
		button1.Text = "Balanced";  
		button2.Text = "Draft" + Environment.NewLine + "(fast)";    
	    button3.Text = "Best" + Environment.NewLine + "(slower)";
		button4.Text = "Translate" + Environment.NewLine + "to English";
	    
		button5.Text = "Balanced";   
		button6.Text = "Draft" + Environment.NewLine + "(fast)";     
	    button7.Text = "Best" + Environment.NewLine + "(slowest)";
		button8.Text = "Cancel";
		
		   // ---------------------- 
		   //  what the user clicked
		   // ----------------------
	    button1.Name = "b1 balanced";  
		button2.Name = "b2 draft";    
		button3.Name = "b3 best";
		button4.Name = "b4 translate";
		
	    button5.Name = "b5 balanced";  
		button6.Name = "b6 draft";    
		button7.Name = "b7 best";
		button8.Name = "b8 cancel";
		   // ----------------------- 
		button1.DialogResult = DialogResult.Cancel;
		button2.DialogResult = DialogResult.Cancel;
        button3.DialogResult = DialogResult.Cancel;
		button4.DialogResult = DialogResult.Cancel;

		button5.DialogResult = DialogResult.Cancel;
		button6.DialogResult = DialogResult.Cancel;
        button7.DialogResult = DialogResult.Cancel;
		button8.DialogResult = DialogResult.Cancel;

        int lab1Ypos = 20;
		int lab2Ypos = 90+28;
		int labXpos = 9;

		
		label1.SetBounds(9, lab1Ypos, 372, 13);       //x pos,y pos, x length, y Height 
		label2.SetBounds(9, lab2Ypos, 372, 13);       //x pos,y pos, x length, y Height 
		
		  //textBox.SetBounds(12, 36, 372, 20);           //no need for textbox
	 
		button1.SetBounds(labXpos, lab1Ypos + 28, 90, 28*2);   //x pos,y pos, x length, y Height 
		button2.SetBounds(labXpos + 100, lab1Ypos + 28, 90, 28*2);   //x pos,y pos, x length, y Height 
		button3.SetBounds(labXpos + 100*2, lab1Ypos + 28, 90, 28*2);   //x pos,y pos, x length, y Height 
		button4.SetBounds(labXpos + 100*3, lab1Ypos + 28, 100, 28*2);  //x pos,y pos, x length, y Height 

        button5.SetBounds(labXpos, lab2Ypos + 28, 90, 28*2);   //x pos,y pos, x length, y Height 
		button6.SetBounds(labXpos + 100, lab2Ypos + 28, 90, 28*2);   //x pos,y pos, x length, y Height 
		button7.SetBounds(labXpos + 100*2, lab2Ypos + 28, 90, 28*2);   //x pos,y pos, x length, y Height 
		button8.SetBounds(labXpos + 100*3, lab2Ypos + 28, 100, 28*2);  //x pos,y pos, x length, y Height 

		label1.AutoSize = true;
		label2.AutoSize = true;
		
		  //textBox.Anchor = textBox.Anchor | AnchorStyles.Right;  //no need for textbox
		button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		
		//form.ClientSize = new Size(477, 107);
		form.ClientSize = new Size(420, 200+28); //x pos,y pos,
		
		  //form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });  //no need for textbox
		
		form.Controls.AddRange(new Control[] { label1, label2, button1, button2, button3, button4, button5, button6, button7, button8, });
		
		form.ClientSize = new Size(Math.Max(420, label1.Right + 10), form.ClientSize.Height);
	
		form.FormBorderStyle = FormBorderStyle.FixedDialog;
		form.StartPosition = FormStartPosition.CenterScreen;
		form.MinimizeBox = false;
		form.MaximizeBox = false;
		
		form.AcceptButton = button1;
		form.CancelButton = button2;
        form.CancelButton = button3;  
        form.CancelButton = button4;  			
	    form.CancelButton = button5;
        form.CancelButton = button6;  
        form.CancelButton = button7;  
		form.CancelButton = button8; 
		
		//give windows form a background color
		form.BackColor = Color.Black; 
		//give windows form text label a readable color on black
		label1.ForeColor = Color.White;
		label2.ForeColor = Color.White;
		
		//give buttons a meaningfull color
	    button1.BackColor = Color.Aqua;     //balanced Eng
		button2.BackColor = Color.Yellow;   //draft Eng
		button3.BackColor = Color.Gold;     //Best Eng 
		button4.BackColor = Color.Aqua;     //translate
		
		button5.BackColor = Color.Aqua;     //balanced intl
		button6.BackColor = Color.Yellow;   //draft Intl
		button7.BackColor = Color.Gold;     //Best Intl
		button8.BackColor = Color.Lime;     //cancel
		
		button1.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
        button2.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
        button3.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
		button4.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);	
		button5.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
        button6.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
        button7.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
		button8.Click +=  new EventHandler(CreateUserDialog.Buttons_OnClick);
		DialogResult dialogResult = form.ShowDialog();
		  //value = textBox.Text; //no need for textbox
  
		return dialogResult;
		  	  
	}
	
   
	static void Buttons_OnClick(object sender, EventArgs e)
	//create button click event
	{
		Button btn = sender as Button;
		Globals.keuze = btn.Name;
		//MessageBox.Show("You clicked " + btn.Name);    
	}
	
	
	public static DialogResult InputBox2(string title, string promptText, ref string value)
	{  	
		// ----------------------------------------------------------------------------------
		// part 2 create YES - NO buttons question to create track + import text as subtitle
		// ---------------------------------------------------------------------------------- 		
		Form form = new Form();
		Label label = new Label();
		//TextBox textBox = new TextBox();
		Button buttonOk = new Button();
		Button buttonCancel = new Button();

		form.Text = title;
		label.Text = promptText;
		//textBox.Text = value;

		buttonOk.Text = "Yes";
		buttonCancel.Text = "No";
		buttonOk.DialogResult = DialogResult.OK;
		buttonCancel.DialogResult = DialogResult.Cancel;

		label.SetBounds(9, 20, 372, 20);          //x pos,y pos, x length, y Height 
		//textBox.SetBounds(12, 36, 372, 20);  
		buttonOk.SetBounds(228, 65, 75, 35);      //x pos,y pos, x length, y Height   
		buttonCancel.SetBounds(309, 65, 75, 35);  //x pos,y pos, x length, y Height 
 
		
	    //give windows form a background color
		form.BackColor = Color.Black; 
		//give windows form text label a readable color on black
		label.ForeColor = Color.White;
		
		// button colors
		buttonOk.BackColor = Color.LightPink;
		buttonCancel.BackColor = Color.Lime;

		label.AutoSize = true;
		//textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
		buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

		form.ClientSize = new Size(396, 107+23);
		//form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
		form.Controls.AddRange(new Control[] { label, buttonOk, buttonCancel });
		form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
		form.FormBorderStyle = FormBorderStyle.FixedDialog;
		form.StartPosition = FormStartPosition.CenterScreen;
		form.MinimizeBox = false;
		form.MaximizeBox = false;
		form.AcceptButton = buttonOk;
		form.CancelButton = buttonCancel;

		DialogResult dialogResult = form.ShowDialog();
		//value = textBox.Text;
		return dialogResult;
	}
	
}

public class SrtInfo
{
    private Timecode startTime;
    private Timecode endTime;
    private List<string> subtitles;

    public Timecode getStartTime()
    {
        return startTime;
    }

    public Timecode getEndTime()
    {
        return endTime;
    }

    public Timecode getEndMinusStartTime()
    {
        return (endTime - startTime);
    }

    public string getText()
    {
		string s = "";
        int i;

        for(i=0; i<subtitles.Count; ++i)
        {
            s += subtitles[i];
            if (i != subtitles.Count - 1)
                s += "\r\n";
        }
				 	
		//optimized subtitle algoritm: add newline after 9 words (=spaces)
		//this is to avoid long sentences wich are only partly displayed without newline
		//maybe add selectable number as a user decision
		string newline = s;
		StringBuilder sb = new StringBuilder(newline);
		int spaces = 0;
		int length = sb.Length;
		for (i=0; i < length; i++)
		{
			if (sb[i] == ' ')
			{
				spaces++;
			}
			if (spaces == 9)  //seems optimal for ENGLISH
			{
				sb.Insert(i, Environment.NewLine);
				//break;
				spaces = 0; //if you want to insert new line after each 9 words
			}

		}
		s = sb.ToString();
	   	
		// ALTERNATIVE 1: only 1 NEWLINE AFTER FIRST ","
	    //var regex = new Regex(Regex.Escape(","));   
		//s = regex.Replace(s, "," + Environment.NewLine, 1);
		
		// ALTERNATIVE 2: add NEWLINE AFTER each "," or "."
        //s = s.Replace(",", "," + Environment.NewLine);  //add newline at , for display of longer subtitles
	    //s = s.Replace(".", "." + Environment.NewLine);  //add newline at . for display of longer subtitles	  
		
        return s;
    }

    public SrtInfo(List<string> input)
    {
        string[] timeStrings = input[1].Split(((string)" ").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        startTime = new Timecode(timeStrings[0]);
        endTime = new Timecode(timeStrings[2]);

        subtitles = new List<string>();
        for (int subtitIDX = 2; subtitIDX < input.Count; ++subtitIDX)
        {
            subtitles.Add(input[subtitIDX]);
        }
    }

}

	/*
	foreach (SrtInfo x in subtitles) // for testpurposes: show linked list
	{		  
		MessageBox.Show(String.Format(x.getText()));
	}	
	foreach (SrtInfo x in subtitles) // for testpurposes: show linked list
	{		  
		MessageBox.Show(String.Format("starttime: " + x.getStartTime() + " endtime: " + x.getEndTime()));
	} */