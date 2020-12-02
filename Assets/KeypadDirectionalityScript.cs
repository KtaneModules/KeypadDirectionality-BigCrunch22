using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class KeypadDirectionalityScript : MonoBehaviour
{
	public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
	public KMBossModule Boss;
	
	public AudioSource SoundOutput;
	public KMSelectable[] NumberButtons;
	public TextMesh Arrows;
	public TextMesh NumberInput;
	public TextMesh Layer;
	public TextMesh[] LightUp;
	public AudioClip[] SFX;
	
	private string[] IgnoredModules;
	int ActualStage = 0;
	int MaxStage;
	int CodeCheck = 0;
	List<string> TheCode = new List<string>();
	List<string> StageRecovery = new List<string>();
	bool Playable = false;
	int[] TemporaryCoordinates = {0, 0};
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
		for (int x = 0; x < NumberButtons.Length; x++)
		{
			int Numbered = x;
            NumberButtons[Numbered].OnInteract += delegate
            {
                Press(Numbered);
				return false;
            };
		}
	}

	void Start()
	{
				if (IgnoredModules == null)
				IgnoredModules = Boss.GetIgnoredModules("Keypad Directionality", new string[]{
				"14",
				"42",
				"501",
				"A>N<D",
				"Bamboozling Time Keeper",
				"Brainf---",
				"Busy Beaver",
				"Don't Touch Anything",
				"Forget Any Color",
				"Forget Enigma",
				"Forget Everything",
				"Forget It Not",
				"Forget Me Later",
				"Forget Me Not",
				"Forget Perspective",
				"Forget The Colors",
				"Forget Them All",
				"Forget This",
				"Forget Us Not",
				"Iconic",
				"Keypad Directionality",
				"Kugelblitz",
				"Multitask",
				"OmegaDestroyer",
				"OmegaForget",
				"Organization",
				"Password Destroyer",
				"Purgatory",
				"RPS Judging",
				"Simon Forgets",
				"Simon's Stages",
				"Souvenir",
				"Tallordered Keys",
				"The Time Keeper",
				"The Troll",
				"The Twin",
				"The Very Annoying Button",
				"Timing is Everything",
				"Turn The Key",
				"Ultimate Custom Night",
				"Übermodule",
				"Whiteout"
            });
		Module.OnActivate += StartUp;
	}
	
	void Press(int Numbered)
	{
		NumberButtons[Numbered].AddInteractionPunch(.2f);
		Audio.PlaySoundAtTransform(SFX[0].name, transform);
		if (Playable && !ModuleSolved)
		{
			Arrows.text = "";
			NumberInput.text += (Numbered + 1).ToString();
			if (NumberInput.text.Length == 4)
			{
				Playable = false;
				StartCoroutine(Inspection());
			}
		}
	}
	
	IEnumerator Inspection()
	{
		yield return new WaitForSecondsRealtime(0.4f);
		if (NumberInput.text == TheCode[CodeCheck])
		{
			Debug.LogFormat("[Keypad Directionality #{0}] You submitted {1} for Layer {2}. That was correct.", moduleId, NumberInput.text, (CodeCheck+1).ToString());
			CodeCheck++;
			if (CodeCheck == MaxStage)
			{
				SoundOutput.clip = SFX[3];
				SoundOutput.Play();
				Layer.text = "PASS";
				while (SoundOutput.isPlaying)
				{
					for (int x = 0; x < 2; x++)
					{
						Layer.color = Color.green;
						yield return new WaitForSecondsRealtime(0.1f);
						Layer.color = Color.black;
						yield return new WaitForSecondsRealtime(0.1f);
					}
				}
				Layer.text = "UNLOCK";
				Layer.color = Color.green;
				ModuleSolved = true;
				Module.HandlePass();
				NumberInput.text = "";
			}
			
			else
			{
				SoundOutput.clip = SFX[1];
				SoundOutput.Play();
				Layer.text = "ACCEPT";
				while (SoundOutput.isPlaying)
				{
					for (int x = 0; x < 2; x++)
					{
						Layer.color = Color.green;
						yield return new WaitForSecondsRealtime(0.05f);
						Layer.color = Color.black;
						yield return new WaitForSecondsRealtime(0.05f);
					}
				}
				NumberInput.text = "";
				Layer.color = Color.white;
				Layer.text = "L" + (CodeCheck + 1).ToString();
				Playable = true;
			}
		}
		
		else
		{
			Debug.LogFormat("[Keypad Directionality #{0}] You submitted {1} for Layer {2}. That was incorrect.", moduleId, NumberInput.text, (CodeCheck+1).ToString());
			SoundOutput.clip = SFX[2];
			SoundOutput.Play();
			Layer.text = "DENIED";
			while (SoundOutput.isPlaying)
			{
				for (int x = 0; x < 2; x++)
				{
					Layer.color = Color.red;
					yield return new WaitForSecondsRealtime(0.05f);
					Layer.color = Color.black;
					yield return new WaitForSecondsRealtime(0.05f);
				}
			}
			NumberInput.text = "";
			Layer.color = Color.white;
			Layer.text = "L" + (CodeCheck + 1).ToString();
			Module.HandleStrike();
			Arrows.text = StageRecovery[CodeCheck];
			Playable = true;
		}
	}
	
	void StartUp()
	{
		MaxStage = Bomb.GetSolvableModuleNames().Where(x => !IgnoredModules.Contains(x)).Count();
		StartCoroutine(GenerateCode());
	}
	
	void Update()
	{
		if (ActualStage < Bomb.GetSolvedModuleNames().Where(a => !IgnoredModules.Contains(a)).Count() && !ModuleSolved)
        {
			ActualStage++;
			StartCoroutine(GenerateCode());
		}
	}
	
	IEnumerator GenerateCode()
	{
		string[] Orientation = {"a", "b", "c", "d", "e", "f", "g", "h"};
		string[] Pointers = {"W", "NW", "N", "NE", "E", "SE", "S", "SW"};
		string ToBeSent = "";
		int[] Coordinates = {0, 0};
		
		if (ActualStage == MaxStage)
		{
			if (MaxStage == 0)
			{
				Debug.LogFormat("[Keypad Directionality #{0}] Layer 0 is not possible. Automatically solving.", moduleId);
				SoundOutput.clip = SFX[5];
				SoundOutput.Play();
				Layer.text = "ERROR";
				while (SoundOutput.isPlaying)
				{
					for (int x = 0; x < 2; x++)
					{
						Layer.color = Color.red;
						yield return new WaitForSecondsRealtime(0.1f);
						Layer.color = Color.black;
						yield return new WaitForSecondsRealtime(0.1f);
					}
				}
				Layer.text = "BYPASS";
				Layer.color = Color.green;
				Module.HandlePass();
			}
			
			else
			{
				Arrows.text = "";
				Playable = true;
				Layer.text = "L" + (CodeCheck + 1).ToString();
				Audio.PlaySoundAtTransform(SFX[4].name, transform);
				Debug.LogFormat("[Keypad Directionality #{0}] The module is now checking", moduleId);
			}
		}
		
		else if (ActualStage > 999999)
		{
			Arrows.text = "";
			Playable = true;
			Layer.text = "L" + (CodeCheck + 1).ToString();
			Audio.PlaySoundAtTransform(SFX[4].name, transform);
			Debug.LogFormat("[Keypad Directionality #{0}] The module is now checking", moduleId);
		}
		
		else
		{
			Audio.PlaySoundAtTransform(SFX[4].name, transform);
			string CodeForTheLayer = "";
			Layer.text = "L" + (ActualStage + 1).ToString();
			int StarNumber = 0;
			if (ActualStage == 0)
			{
				for (int x = 0; x < 2; x++)
				{
					Coordinates[x] = UnityEngine.Random.Range(0,3);
				}
				LightUp[(Coordinates[0] * 3 + Coordinates[1])].color = Color.gray;
				StarNumber = (Coordinates[0] * 3 + Coordinates[1] + 1);
			}
			
			else
			{
				Arrows.text = "";
				for (int x = 0; x < Coordinates.Length; x++)
				{
					Coordinates[x] = TemporaryCoordinates[x];
				}
				StarNumber = (Coordinates[0] * 3 + Coordinates[1] + 1);
			}
			
			for (int x = 0; x < 4; x++)
			{
				int OrientationNumber = UnityEngine.Random.Range(0,8);
				Arrows.text += Orientation[OrientationNumber];
				switch (OrientationNumber)
				{
					case 0:
						Coordinates[1] = (Coordinates[1] - 1 + 3) % 3;
						break;
					case 1:
						Coordinates[0] = (Coordinates[0] - 1 + 3) % 3;
						Coordinates[1] = (Coordinates[1] - 1 + 3) % 3;
						break;
					case 2:
						Coordinates[0] = (Coordinates[0] - 1 + 3) % 3;
						break;
					case 3:
						Coordinates[0] = (Coordinates[0] - 1 + 3) % 3;
						Coordinates[1] = (Coordinates[1] + 1 + 3) % 3;
						break;
					case 4:
						Coordinates[1] = (Coordinates[1] + 1 + 3) % 3;
						break;
					case 5:
						Coordinates[0] = (Coordinates[0] + 1 + 3) % 3;
						Coordinates[1] = (Coordinates[1] + 1 + 3) % 3;
						break;
					case 6:
						Coordinates[0] = (Coordinates[0] + 1 + 3) % 3;
						break;
					case 7:
						Coordinates[0] = (Coordinates[0] + 1 + 3) % 3;
						Coordinates[1] = (Coordinates[1] - 1 + 3) % 3;
						break;
					default:
						break;
				}
				CodeForTheLayer += (((Coordinates[0] * 3) + Coordinates[1]) + 1).ToString();
				ToBeSent += x != 4 ? Pointers[OrientationNumber] + ", " : Pointers[OrientationNumber];
			}
			TheCode.Add(CodeForTheLayer);
			StageRecovery.Add(Arrows.text);
			Debug.LogFormat("[Keypad Directionality #{0}] Current Stage: {1} - Starting Number: {2} - Orientations: {3}", moduleId, (ActualStage+1).ToString(), StarNumber.ToString(), ToBeSent);
			Debug.LogFormat("[Keypad Directionality #{0}] Code Generated: {1}", moduleId, TheCode[ActualStage]);
		}
		
		for (int x = 0; x < Coordinates.Length; x++)
		{
			TemporaryCoordinates[x] = Coordinates[x];
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit the code on the module, use the command !{0} type <codes> (Every 4 digit codes must be separated by one space)";
    #pragma warning restore 414
	
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (!Playable)
		{
			yield return "sendtochaterror You can not interact with the module yet. The command was not processed.";
			yield break;
		}
		
		if (Regex.IsMatch(parameters[0], @"^\s*type\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			for (int x = 1; x < parameters.Length; x++)
			{
				if (!ModuleSolved)
				{
					int ParseTree;
					bool ParseCommand = Int32.TryParse(parameters[x], out ParseTree);
					if (parameters[x].Length != 4)
					{
						yield return "sendtochaterror The code being sent is not 4 digits long. The command was not processed.";
						yield break;
					}
					
					if (!ParseCommand)
					{
						yield return "sendtochaterror The code being sent is not a proper number. The command was not processed.";
						yield break;
					}
					
					for (int y = 0; y < parameters[x].Length; y++)
					{
						yield return new WaitForSecondsRealtime(0.1f);
						if (y == 3)
						{
							yield return "solve";
							yield return "strike";
						}
						NumberButtons[Int32.Parse(parameters[x][y].ToString()) - 1].OnInteract();
					}
					
					while (!Playable)
					{
						yield return null;
					}
				}
			}
		}
	}
}
