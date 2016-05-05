using HtmlAgilityPack;
using System;
using System.Net;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;



namespace WebScraper
{
    public partial class Form1 : Form
    {
        private Dictionary<string, string> audioFiles = new Dictionary<string, string>();   // may be scraped ??
        string audioPath = Environment.CurrentDirectory + "\\AudioClips";       // Folder path for our Audio Clips

        private System.Drawing.Point currentButtonpos = new System.Drawing.Point(0, 0); // Posistional Iterator for generated buttons
        private int xButtonOffset = 80;  // X offset for generated buttons
        private int yButtonOffset = 30;  // Y offset for generated buttons

        string urlTarget = "http://www.realmofdarkness.net/sb/bane/";   // Site that will be scraped

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Form1()
        {
            InitializeComponent();      // Initialize our form
            AddAllClips();      // Grab all audio clips
        }

        private void MakeaudioButton(string n)
        {
            Button b = new Button();
            b.Location = currentButtonpos;      // Set this new button's Location to our iterating value
            b.Name = n;     // Set this new button's Name to our iterating value
            b.Text = n;     // Set this new button's Displayed Text to our iterating value
            b.Click += new EventHandler(PlayAudio);     // Gets the on click event for the buttons
            this.Controls.Add(b);       // Add Button to the controls managed by our Form

            // Iterate our currentButtonpos
            if(currentButtonpos.X >= xButtonOffset * 10)    // if we have made 10 Buttons
            {
                currentButtonpos.X = 0;
                currentButtonpos.Y += yButtonOffset;        // Move to the next line
            }
            else
            {
                currentButtonpos.X += xButtonOffset;        // Move over by the offset value
            }
        }

        void AddAllClips()
        {
            var webRequest = new HtmlWeb();
            var webPage = webRequest.Load(urlTarget);                   // Open that url
            var nodes = webPage.DocumentNode.SelectNodes("//audio");    // All line with the div audio

        #region parse through each node on the selected site
            foreach (var n in nodes)
            {
                string rawInnerhtml = n.InnerHtml.ToString();   // node's Raw InnerHtml with stuff we dont want like,    <source> src ="
                string desiredURL = ""; // Container for our url, starting blank
                bool grab = false;      // 

                #region Parse through the rawInnerhtml to find our url
                foreach (char c in rawInnerhtml)
                {
                    if (grab && c != '"')   // storing the url from the innerhtml, second condition is so we don't grab the (") at the end
                        desiredURL += c;

                    if (c == '"')           // if it's a (") what will follow will be our url
                        grab = !grab;
                }
                #endregion

                
                if (Directory.GetFiles(audioPath).Length < 140)    // Rigid check to see if we already downloaded the files
                {
                    using (var client = new WebClient())    // 
                    {
                        //desired url is the url of the mp3
                        //audioPath is where we want to save it
                        string ap = audioPath + "\\" + n.Id + ".mp3";
                        client.DownloadFile(desiredURL, ap);  // stores audio clip


                        Mp3ToWav(ap, audioPath + "\\" + n.Id + ".wav");
                    }
                }

                MakeaudioButton(n.Id);  // Make button based of this node's info
            }
        #endregion

        #region Add all the audio files we found to the dictionary
            foreach (string clipPath in Directory.GetFiles(audioPath))
            {
                string clip = Path.GetFileName(clipPath);   // grab this file's entire File Path
                if(clip.EndsWith(".wav"))
                    audioFiles.Add(clip, audioPath + "\\" + clip.ToString());     // adds file to dictionary
            }
        #endregion
        }

        public void PlayAudio(object sender, EventArgs e)
        {
            string audioPath = audioFiles[((Button)sender).Name + ".wav"];      // Grab file path to the audio file

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = audioPath;
            player.Play();
        }

        public static void Mp3ToWav(string mp3File, string outputFile)
        {
            using (NAudio.Wave.Mp3FileReader reader = new NAudio.Wave.Mp3FileReader(mp3File))
            {
                using (NAudio.Wave.WaveStream pcmStream = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    NAudio.Wave.WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                }
            }
        }

    }
}
