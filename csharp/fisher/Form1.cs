using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace fisher {
	public partial class Form1 : Form {

		#region Declarations

		private static bool steam;
		private static Point locationStartButton;
		private static Point locationCloseShellDialogBox;
		private static Point locationTradeFishButton;
		private static Point locationCloseItGotAwayButton;
		private static Point locationTimerCaughtFish;
		private static Point locationJunkItem;
		private static Point locationTopLeftWeightScreenshot;
		private static Point locationBottomRightWeightScreenshot;
		private static Point location100Position;

		/* Unity Fishing colours (obtained by playing through Kong site)
		 * Note: All item (button, fishing bars ..) colours are split in bands: top, sometimes middle, and bottom  
		 * Note: All item colours change when the mouse hovers over them
		 * Start button top			idle	165, 210, 50
		 * Start button bottom		idle	154, 207, 30
		 * Start button top 		hovered	202, 240, 96
		 * Start button bottom		hovered	197, 238, 80
		 * Cast button top 			hovered	97, 215, 239
		 * Cast buttom bottom 		hovered	79, 208, 237
		 * Cast meter full top				109, 175, 255
		 * Cast meter full mid				26, 120, 242
		 * Cast meter full bottom			17, 83, 171
		 * 100% text						78, 255, 0
		 * caught bottom 89, 156, 92
		 * trade bottom 196, 238, 80
		 * */
		Color startButtonGreenUnity		= Color.FromArgb(154, 207, 30);
		Color castButtonBlueUnity		= Color.FromArgb(79, 208, 80);
		Color castMeterFullUnity		= Color.FromArgb(26, 120, 242);
		Color oneHundredCatchColorUnity = Color.FromArgb(78, 255, 0);

		Color startButtonGreen			= Color.FromArgb(156, 208, 31);  //Color.FromArgb(155, 208, 30);
		Color castButtonBlue			= Color.FromArgb(79, 208, 80); //Color.FromArgb(030, 170, 208);
		Color colorCloseItGotAwayButton = Color.FromArgb(030, 170, 208);
		Color colorTimerCaughtFishKong = Color.FromArgb(40, 40, 40); //0, 255, 47); //56, 255, 56);
		Color colorTimerCaughtFishSteam = Color.FromArgb(59, 255, 59);
		Color colorJunkItem				= Color.FromArgb(255, 255, 255);
		Color oneHundredCatchColor		= Color.FromArgb(78, 255, 0); //Color.FromArgb(77, 254, 0);

		MethodHelper helper;
		#endregion

		public Form1() {

			InitializeComponent();

			backgroundThread.WorkerReportsProgress = true;
			backgroundThread.WorkerSupportsCancellation = true;
			backgroundThreadGetTimes.WorkerReportsProgress = true;
			backgroundThreadGetTimes.WorkerSupportsCancellation = true;

			kongButton.CheckedChanged += new EventHandler(platform_CheckedChanged);
			kartridgeButton.CheckedChanged += new EventHandler(platform_CheckedChanged);
			steamButton.CheckedChanged += new EventHandler(platform_CheckedChanged);

			castCatchLocationLbl.Text = "Cast/Catch Location:\nPress start button when on fishing start screen";

			helper = new MethodHelper(steam);

			baitToUseText.Enabled = false;
			findLocationBtn.Enabled = false;
			autoBtn.Enabled = false;
			cancelAutoModeBtn.Enabled = false;

		}
		/// <summary>
		/// This determines which radio button was clicked.
		/// This returns whether the user is using Steam or Kongregate to fish.
		/// </summary>
		private void platform_CheckedChanged(object sender, EventArgs e) {
			RadioButton radioButton = sender as RadioButton;
			if (kongButton.Checked || kartridgeButton.Checked) {
				steam = false;
			} else if (steamButton.Checked) {
				steam = true;
			}
			baitToUseText.Enabled = true;
			findLocationBtn.Enabled = true;
		}

		private void CastCatchLocation_Click(object sender, EventArgs e) {
			locationStartButton = helper.FindColor(startButtonGreen);
			if (locationStartButton == new Point()) {
				MessageBox.Show("Start button could not be found.\nPlease make sure you are on the fishing start screen.\nIf you believe this may be an error, please submit an issue on github.", "Start button not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} else {
				locationStartButton = helper.getScreenLocationPoint(locationStartButton);
				if (steam) {
					locationTradeFishButton = new Point(locationStartButton.X, locationStartButton.Y - 40);
					locationCloseShellDialogBox = new Point(locationStartButton.X + 270, locationStartButton.Y - 350);
					locationCloseItGotAwayButton = new Point(locationStartButton.X + 20, locationStartButton.Y - 130);
					locationTimerCaughtFish = new Point(locationStartButton.X - 200, locationStartButton.Y - 70);
					locationJunkItem = new Point(locationStartButton.X + 40, locationStartButton.Y - 182);
					locationTopLeftWeightScreenshot = new Point(locationStartButton.X - 25, locationStartButton.Y - 130);
					locationBottomRightWeightScreenshot = new Point(locationStartButton.X + 160, locationStartButton.Y - 50);
					location100Position = new Point(locationStartButton.X + 370, locationStartButton.Y - 81);
				} else {
					locationTradeFishButton = new Point(locationStartButton.X, locationStartButton.Y - 15);
					locationCloseShellDialogBox = new Point(locationStartButton.X + 265, locationStartButton.Y - 325);
					locationCloseItGotAwayButton = new Point(locationStartButton.X + 30, locationStartButton.Y - 100);
					//locationTimerCaughtFish = new Point(locationStartButton.X - 200, locationStartButton.Y - 70);
					locationTimerCaughtFish = new Point(locationStartButton.X - 185, locationStartButton.Y - 75);
					locationJunkItem = new Point(locationStartButton.X + 100, locationStartButton.Y - 155);
					locationTopLeftWeightScreenshot = new Point(locationStartButton.X - 25, locationStartButton.Y - 130);
					locationBottomRightWeightScreenshot = new Point(locationStartButton.X + 160, locationStartButton.Y - 50);
					//location100Position = new Point(locationStartButton.X + 373, locationStartButton.Y - 81);
					location100Position = new Point(locationStartButton.X + 374, locationStartButton.Y - 81);
				}
				castCatchLocationLbl.Text = "Cast/Catch Location:\n" + locationStartButton.ToString();
				autoBtn.Enabled = true;
				cancelAutoModeBtn.Enabled = true;
			}
		}

		private void autoBtn_Click(object sender, EventArgs e) {
			backgroundThread.RunWorkerAsync();
		}

		private void backgroundThread_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker worker = sender as BackgroundWorker;
			int baitToUse = int.Parse(baitToUseText.Text);
			int i = 0;
			int baitUsed = 0;
			while (i < baitToUse) {
				if (worker.CancellationPending == true) {
					e.Cancel = true;
					break;
				} else {
					//Performs cast
					bool caughtFish = true;
					bool fishGetAway = true;
					printMessage(baitUsed, baitToUse, " bait used.\nPerforming cast.");
					++baitUsed;
					Invoke(new Action(() => Refresh()));
					Invoke(new Action(() => helper.startCast(locationStartButton)));
					Invoke(new Action(() => Cursor.Position = locationTimerCaughtFish));
					Debug.WriteLine("locationTimerCaughtFish {0},{0}", locationTimerCaughtFish.X, locationTimerCaughtFish.Y);
					while (caughtFish) {
						if (worker.CancellationPending == true) {
							e.Cancel = true;
							break;
						}
						printMessage(baitUsed, baitToUse, " bait used.\nWaiting for cast result.");
						Invoke(new Action(() => Refresh()));
						//performs cast
						Color color = helper.GetPixelColor(locationTimerCaughtFish);
						//if (color == colorTimerCaughtFishKong || color == colorTimerCaughtFishSteam) {
						
						//Debug.WriteLine("C = {0}, {0}, {0}", color.R, color.G, color.B);
						//Invoke(new Action(() => Refresh()));
						//Invoke(new Action(() => helper.LeftClick(locationTimerCaughtFish)));

						if (helper.AreColorsSimilar(color, colorTimerCaughtFishKong, 20)) {
							if (worker.CancellationPending == true) {
								e.Cancel = true;
								break;
							}
							printMessage(baitUsed, baitToUse, " bait used.\nPerforming catch.");
							Invoke(new Action(() => Refresh()));
							Invoke(new Action(() => helper.catchFish(location100Position, oneHundredCatchColor)));
							Thread.Sleep(5000);
							while (fishGetAway) {

								//fish caught
								if (worker.CancellationPending == true) {
									e.Cancel = true;
									break;
								}
								if (helper.GetPixelColor(locationTradeFishButton) == startButtonGreen) {
									printMessage(baitUsed, baitToUse, " bait used.\nCaught.");
									Invoke(new Action(() => Refresh()));
									//helper.getFishWeight(locationTopLeftWeightScreenshot);
									Invoke(new Action(() => helper.tradeItemThenCloseClick(locationTradeFishButton, locationCloseShellDialogBox)));
									fishGetAway = false;

								}
								//fish got away
								else if (helper.GetPixelColor(locationCloseItGotAwayButton) == colorCloseItGotAwayButton) {
									printMessage(baitUsed, baitToUse, " bait used.\nFish got away. Sorry :(");
									Invoke(new Action(() => Refresh()));
									helper.fishGotAwayClick(locationCloseItGotAwayButton);
									fishGetAway = false;
								}
							}
							caughtFish = false;
						}
						// caught junk
						else if (helper.GetPixelColor(locationJunkItem) == colorJunkItem) {
							printMessage(baitUsed, baitToUse, " bait used.\nCaught junk");
							Invoke(new Action(() => Refresh()));
							Invoke(new Action(() => helper.tradeItemThenCloseClick(locationTradeFishButton, locationCloseShellDialogBox)));
							caughtFish = false;
						}
					}
					++i;
				}
			}
			printMessage(baitUsed, baitToUse, " bait used.\nFinished.");
			backgroundThread.CancelAsync();
		}

		private void cancelAutoModeBtn_Click(object sender, EventArgs e) {
			backgroundThread.CancelAsync();
		}

		private void printMessage(int baitUsed, int baitToUse, string msg) {
			debugAutoStepLbl.Invoke((MethodInvoker)delegate {
				debugAutoStepLbl.Text = baitUsed + "/" + baitToUse + msg;
			});
		}
	}
}
