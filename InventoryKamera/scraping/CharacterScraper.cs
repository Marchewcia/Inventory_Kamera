﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Accord.Imaging;

namespace InventoryKamera
{
	public static class CharacterScraper
	{
		private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		private static string firstCharacterName = null;

		public static void ScanCharacters(ref List<Character> characters)
		{
			// first character name is used to stop scanning characters
			int characterCount = 0;
			firstCharacterName = null; // Static variable might already be set
			UserInterface.ResetCharacterDisplay();
			while (ScanCharacter(out Character character) || characterCount <= 4)
			{
				if (character.IsValid())
				{
					characters.Add(character);
					UserInterface.IncrementCharacterCount();
					Logger.Info("Scanned {0} successfully", character.Name);
					characterCount++;
				}
				Navigation.SelectNextCharacter();
				UserInterface.ResetCharacterDisplay();
			}
		}

		private static bool ScanCharacter(out Character character)
		{
			character = new Character();
			Navigation.SelectCharacterAttributes();
			string name = null;
			string element = null;

			// Scan the Name and element of Character. Attempt 75 times max.
			ScanNameAndElement(ref name, ref element);

			if (string.IsNullOrWhiteSpace(name))
			{
				if (string.IsNullOrWhiteSpace(name)) UserInterface.AddError("Could not determine character's name");
				if (string.IsNullOrWhiteSpace(element)) UserInterface.AddError("Could not determine character's element");
				return false;
			}

			// Check if character was first scanned
			if (name != firstCharacterName)
			{
				if (string.IsNullOrWhiteSpace(firstCharacterName))
					firstCharacterName = name;

				bool ascended = false;
				// Scan Level and ascension
				int level = ScanLevel(ref ascended);
				if (level == -1)
				{
					UserInterface.AddError($"Could not determine {name}'s level");
					return false;
				}

				// Scan Experience
				//experience = ScanExperience();
				Navigation.SystemRandomWait(Navigation.Speed.Normal);

				// Scan Constellation
				Navigation.SelectCharacterConstellation();
				int constellation = ScanConstellations();
				Navigation.SystemRandomWait(Navigation.Speed.Normal);

				// Scan Talents
				Navigation.SelectCharacterTalents();
				int[] talents = ScanTalents(name);
				Navigation.SystemRandomWait(Navigation.Speed.Normal);

				// Scale down talents due to constellations
				if (constellation >= 3)
				{
					if (Scraper.Characters.ContainsKey(name.ToLower()))
					{
						// get talent if character
						if (constellation >= 5)
						{
							talents[1] -= 3;
							talents[2] -= 3;
						}
						else if ((string)Scraper.Characters[name.ToLower()]["ConstellationOrder"][0] == "skill")
						{
							talents[1] -= 3;
						}
						else
						{
							talents[2] -= 3;
						}
					}
					else
						return false;
				}

				var weaponType = Scraper.Characters[name.ToLower()]["WeaponType"].ToObject<int>();

				int experience = 0;
				character = new Character(name, element, level, ascended, experience, constellation, talents, (WeaponType)weaponType);
				return true;
			}
			Logger.Info("Repeat character {0} detected. Finishing character scan...", name);
			return false;
		}

		public static string ScanMainCharacterName()
		{
			var xReference = 1280.0;
			var yReference = 720.0;
			if (Navigation.GetAspectRatio() == new Size(8, 5))
			{
				yReference = 800.0;
			}

			RECT region = new RECT(
				Left:   (int)(185 / xReference * Navigation.GetWidth()),
				Top:    (int)(26  / yReference * Navigation.GetHeight()),
				Right:  (int)(460 / xReference * Navigation.GetWidth()),
				Bottom: (int)(60  / yReference * Navigation.GetHeight()));

			Bitmap nameBitmap = Navigation.CaptureRegion(region);

			//Image Operations
			Scraper.SetGamma(0.2, 0.2, 0.2, ref nameBitmap);
			Scraper.SetInvert(ref nameBitmap);
			Bitmap n = Scraper.ConvertToGrayscale(nameBitmap);

			UserInterface.SetNavigation_Image(nameBitmap);

			string text = Scraper.AnalyzeText(n).Trim();
			if (text != "")
			{
				// Only keep a-Z and 0-9
				text = Regex.Replace(text, @"[\W_]", string.Empty).ToLower();

				// Only keep text up until first space
				text = Regex.Replace(text, @"\s+\w*", string.Empty);

				UserInterface.SetMainCharacterName(text);
			}
			else
			{
				UserInterface.AddError(text);
			}
			n.Dispose();
			nameBitmap.Dispose();
			return text;
		}

		private static void ScanNameAndElement(ref string name, ref string element)
		{
			int attempts = 0;
			int maxAttempts = 75;
			Rectangle region = new RECT(
				Left:   (int)( 85  / 1280.0 * Navigation.GetWidth() ),
				Top:    (int)( 10  / 720.0 * Navigation.GetHeight() ),
				Right:  (int)( 305 / 1280.0 * Navigation.GetWidth() ),
				Bottom: (int)( 55  / 720.0 * Navigation.GetHeight() ));

			do
			{
				Navigation.SystemRandomWait(Navigation.Speed.Fast);
				using (Bitmap bm = Navigation.CaptureRegion(region))
				{
					Bitmap n = Scraper.ConvertToGrayscale(bm);
					Scraper.SetThreshold(110, ref n);
					Scraper.SetInvert(ref n);

					n = Scraper.ResizeImage(n, n.Width * 2, n.Height * 2);
					string block = Scraper.AnalyzeText(n, Tesseract.PageSegMode.Auto).ToLower().Trim();
					string line = Scraper.AnalyzeText(n, Tesseract.PageSegMode.SingleLine).ToLower().Trim();

					// Characters with wrapped names will not have a slash
					string nameAndElement = line.Contains("/") ? line : block;

					if (nameAndElement.Contains("/"))
					{
						var split = nameAndElement.Split('/');

						// Search for element and character name in block

						// Long name characters might look like
						// <Element>   <First Name>
						// /           <Last Name>
						element = !split[0].Contains(" ") ? Scraper.FindElementByName(split[0].Trim()) : Scraper.FindElementByName(split[0].Split(' ')[0].Trim());

						// Find character based on string after /
						// Long name characters might search by their last name only but it'll still work.
						name = Scraper.FindClosestCharacterName(Regex.Replace(split[1], @"[\W]", string.Empty));
						if (name == "Traveler")
						{
							foreach (var item in from item in Scraper.Characters
												 where item.Value["GOOD"].ToString() == "Traveler"
												 select item)
							{
								name = item.Key;
							}
						}
					}
					n.Dispose();

					if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(element))
					{
						UserInterface.SetCharacter_NameAndElement(bm, name, element);
						return;
					}
				}
				attempts++;
				Navigation.SystemRandomWait(Navigation.Speed.Normal);
			} while (( string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(element) ) && ( attempts < maxAttempts ));
			name = null;
			element = null;
		}

		private static int ScanLevel(ref bool ascended)
		{
			int level = -1;

			var xRef = 1280.0;
			var yRef = 720.0;
			if (Navigation.GetAspectRatio() == new Size(8, 5))
			{
				yRef = 800.0;
			}

			Rectangle region =  new RECT(
				Left:   (int)( 960  / xRef * Navigation.GetWidth() ),
				Top:    (int)( 135  / yRef * Navigation.GetHeight() ),
				Right:  (int)( 1125 / xRef * Navigation.GetWidth() ),
				Bottom: (int)( 163  / yRef * Navigation.GetHeight() ));

			do
			{
				Bitmap bm = Navigation.CaptureRegion(region);

				bm = Scraper.ResizeImage(bm, bm.Width * 2, bm.Height * 2);
				Bitmap n = Scraper.ConvertToGrayscale(bm);
				Scraper.SetInvert(ref n);
				Scraper.SetContrast(30.0, ref bm);

				string text = Scraper.AnalyzeText(n).Trim();

				text = Regex.Replace(text, @"(?![0-9/]).", string.Empty);
				if (text.Contains("/"))
				{
					var values = text.Split('/');
					if (int.TryParse(values[0], out level) && int.TryParse(values[1], out int maxLevel))
					{
						maxLevel = (int)Math.Round(maxLevel / 10.0, MidpointRounding.AwayFromZero) * 10;
						ascended = 20 <= level && level < maxLevel;
						UserInterface.SetCharacter_Level(bm, level, maxLevel);
						n.Dispose();
						bm.Dispose();
						return level;
					}
					n.Dispose();
					bm.Dispose();
				}
				Navigation.SystemRandomWait(Navigation.Speed.Normal);
			} while (level == -1);

			return -1;
		}

		private static int ScanExperience()
		{
			int experience = 0;

			int xOffset = 1117;
			int yOffset = 151;
			Bitmap bm = new Bitmap(90, 10);
			Graphics g = Graphics.FromImage(bm);
			int screenLocation_X = Navigation.GetPosition().Left + xOffset;
			int screenLocation_Y = Navigation.GetPosition().Top + yOffset;
			g.CopyFromScreen(screenLocation_X, screenLocation_Y, 0, 0, bm.Size);

			//Image Operations
			bm = Scraper.ResizeImage(bm, bm.Width * 6, bm.Height * 6);
			//Scraper.ConvertToGrayscale(ref bm);
			//Scraper.SetInvert(ref bm);
			Scraper.SetContrast(30.0, ref bm);

			string text = Scraper.AnalyzeText(bm);
			text = text.Trim();
			text = Regex.Replace(text, @"(?![0-9\s/]).", string.Empty);

			if (Regex.IsMatch(text, "/"))
			{
				string[] temp = text.Split('/');
				experience = Convert.ToInt32(temp[0]);
			}
			else
			{
				Debug.Print("Error: Found " + experience + " instead of experience");
				UserInterface.AddError("Found " + experience + " instead of experience");
			}

			return experience;
		}

		private static int ScanConstellations()
		{
			double yReference = 720.0;
			int constellation;

			if (Navigation.GetAspectRatio() == new Size(8, 5))
			{
				yReference = 800.0;
			}

			Rectangle constActivate =  new RECT(
				Left:   (int)( 70 / 1280.0 * Navigation.GetWidth() ),
				Top:    (int)( 665 / 720.0 * Navigation.GetHeight() ),
				Right:  (int)( 100 / 1280.0 * Navigation.GetWidth() ),
				Bottom: (int)( 695 / 720.0 * Navigation.GetHeight() ));

			for (constellation = 0; constellation < 6; constellation++)
			{
				// Select Constellation
				int yOffset = (int)( ( 180 + ( constellation * 75 ) ) / yReference * Navigation.GetHeight() );

				if (Navigation.GetAspectRatio() == new Size(8, 5))
				{
					yOffset = (int)( ( 225 + ( constellation * 75 ) ) / yReference * Navigation.GetHeight() );
				}

				Navigation.SetCursorPos(Navigation.GetPosition().Left + (int)( 1130 / 1280.0 * Navigation.GetWidth() ),
										Navigation.GetPosition().Top + yOffset);
				Navigation.Click();

				Navigation.Speed speed = constellation == 0 ? Navigation.Speed.Normal : Navigation.Speed.Fast;
				Navigation.SystemRandomWait(speed);

				// Grab Color
				using (Bitmap region = Navigation.CaptureRegion(constActivate))
				{
					// Check a small region next to the text "Activate"
					// for a mostly white backround
					ImageStatistics statistics = new ImageStatistics(region);
					if (statistics.Red.Mean >= 190 && statistics.Green.Mean >= 190 && statistics.Blue.Mean >= 190)
					{
						break;
					}
				}
			}

			Navigation.sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
			UserInterface.SetCharacter_Constellation(constellation);
			return constellation;
		}

		private static int[] ScanTalents(string name)
		{
			int[] talents = {-1,-1,-1};

			int specialOffset = 0;

			// Check if character has a movement talent like
			// Mona or Ayaka
			if (name.Contains("Mona") || name.Contains("Ayaka")) specialOffset = 1;

			var xRef = 1280.0;
			var yRef = 720.0;

			if (Navigation.GetAspectRatio() == new Size(8, 5))
			{
				yRef = 800.0;
			}

			Rectangle region =  new RECT(
				Left:   (int)( 160 / xRef * Navigation.GetWidth() ),
				Top:    (int)( 116 / yRef * Navigation.GetHeight() ),
				Right:  (int)( 225 / xRef * Navigation.GetWidth() ),
				Bottom: (int)( 141 / yRef * Navigation.GetHeight() ));

			for (int i = 0; i < 3; i++)
			{
				// Change y-offset for talent clicking
				int yOffset = (int)( 110 / yRef * Navigation.GetHeight() ) + ( i + ( ( i == 2 ) ? specialOffset : 0 ) ) * (int)(60 / yRef * Navigation.GetHeight() );

				Navigation.SetCursorPos(Navigation.GetPosition().Left + (int)( 1130 / xRef * Navigation.GetWidth() ), Navigation.GetPosition().Top + yOffset);
				Navigation.Click();
				Navigation.Speed speed = i == 0 ? Navigation.Speed.Normal : Navigation.Speed.Fast;
				Navigation.SystemRandomWait(speed);

				while (talents[i] < 1 || talents[i] > 15)
				{
					Bitmap talentLevel = Navigation.CaptureRegion(region);

					talentLevel = Scraper.ResizeImage(talentLevel, talentLevel.Width * 2, talentLevel.Height * 2);

					Bitmap n = Scraper.ConvertToGrayscale(talentLevel);
					Scraper.SetContrast(60, ref n);
					Scraper.SetInvert(ref n);

					string text = Scraper.AnalyzeText(n).Trim();
					text = Regex.Replace(text, @"\D", string.Empty);

					if (int.TryParse(text, out int level))
					{
						if (level >= 1 && level <= 15)
						{
							talents[i] = level;
							UserInterface.SetCharacter_Talent(talentLevel, text, i);
						}
					}

					n.Dispose();
					talentLevel.Dispose();
				}
			}

			Navigation.sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
			return talents;
		}
	}
}