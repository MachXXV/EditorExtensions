using System;
using UnityEngine;
using KSP.UI.Screens;

namespace EditorExtensions
{
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class AppLauncherButton : MonoBehaviour
	{
		private ApplicationLauncherButton button;

		public static AppLauncherButton Instance;

		const string texPathDefault = "EditorExtensions/Textures/AppLauncherIcon";
		const string texPathOn = "EditorExtensions/Textures/AppLauncherIcon-On";
		const string texPathOff = "EditorExtensions/Textures/AppLauncherIcon-Off";

		private void Start ()
		{
			if (AppLauncherButton.Instance == null) {
				OnGuiAppLauncherReady();
			}
		}

		private void Awake ()
		{
			if (AppLauncherButton.Instance == null) {
				GameEvents.onGUIApplicationLauncherReady.Add(this.OnGuiAppLauncherReady);
				Instance = this;
			}
		}

		private void OnDestroy ()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove(this.OnGuiAppLauncherReady);
			if (this.button != null) {
				ApplicationLauncher.Instance.RemoveModApplication(this.button);
			}
		}

		private void OnGuiAppLauncherReady ()
		{
			try {
				this.button = ApplicationLauncher.Instance.AddModApplication(
					() => { OnButtonTrue(); },		//Callback onTrue
					() => { OnButtonFalse(); },		//Callback onFalse
					() => { EditorExtensions.Instance.ShowMenu(); },	//Callback onHover
					() => { EditorExtensions.Instance.HideMenu(); },	//Callback onHoverOut
					null, //Callback onEnable
					null, //Callback onDisable
					ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH, //visibleInScenes
					GameDatabase.Instance.GetTexture(texPathDefault, false) //texture
				);
					
				Log.Debug("Added ApplicationLauncher button");
			} catch (Exception ex) {
				Log.Error("Error adding ApplicationLauncher button: " + ex.Message);
			}

		}

		private void OnButtonTrue ()
		{
			EditorExtensions.Instance.Show();
			//this.button.SetTexture(GameDatabase.Instance.GetTexture (texPathOn, false));
		}

		private void OnButtonFalse ()
		{
			EditorExtensions.Instance.Hide();
			//this.button.SetTexture(GameDatabase.Instance.GetTexture (texPathOff, false));
		}

		private void Update ()
		{
			if (this.button == null) {
				return;
			}
		}
	}
}