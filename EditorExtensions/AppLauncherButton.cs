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
			if (button == null) {
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

				//this.button.onTrue = OnButtonTrue;
				//this.button.onFalse = OnButtonFalse;
			} catch (Exception ex) {
				Log.Error("Error adding ApplicationLauncher button: " + ex.Message);
			}

		}

		private void OnButtonTrue ()
		{
			EditorExtensions.Instance.Show();
		}

		private void OnButtonFalse ()
		{
			EditorExtensions.Instance.Hide();
		}

		private void Update ()
		{
			if (this.button == null) {
				return;
			}

//			if(this.button.State != RUIToggleButton.ButtonState.TRUE)
//				this.button.SetTexture(GameDatabase.Instance.GetTexture (texPathOn, false));
//			else
//				this.button.SetTexture(GameDatabase.Instance.GetTexture (texPathOff, false));

//			try {
//				if (EditorLogic.fetch != null) {
//					if (EditorExtensions.Instance.Visible && this.button.State != RUIToggleButton.ButtonState.TRUE) {
//						this.button.SetTrue ();
//						//this.button.SetTexture(GameDatabase.Instance.GetTexture (texPathOn, false));
//					} else if (!EditorExtensions.Instance.Visible && this.button.State != RUIToggleButton.ButtonState.FALSE) {
//						this.button.SetFalse ();
//						//this.button.SetTexture(GameDatabase.Instance.GetTexture (texPathOff, false));
//					}
//				} else if (this.button.State != RUIToggleButton.ButtonState.DISABLED) {
//					this.button.Disable ();
//				}
//			} catch (Exception ex) {
//				Log.Error ("Error updating ApplicationLauncher button: " + ex.Message);
//			}
		}
	}
}