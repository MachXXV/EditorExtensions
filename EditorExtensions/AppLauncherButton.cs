﻿using System;
using UnityEngine;

namespace EditorExtensions
{
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class AppLauncherButton : MonoBehaviour
	{
		private ApplicationLauncherButton button;

		public static AppLauncherButton Instance;

		const string texPathDefault = "EditorExtensions/Textures/AppLauncherIcon";
		const string texPathOn = "EditorExtensions/Textures/AppLauncherIcon-On";
		const string texPathOff = "EditorExtensions/Textures/AppLauncherIcon-Off";




		private void Awake ()
		{
			GameEvents.onGUIApplicationLauncherReady.Add (this.OnGuiAppLauncherReady);
			Instance = this;
		}

		private void OnDestroy ()
		{
			GameEvents.onGUIApplicationLauncherReady.Remove (this.OnGuiAppLauncherReady);
			if (this.button != null) {
				ApplicationLauncher.Instance.RemoveModApplication (this.button);
			}
		}

		private void ButtonState(bool state) {
			Log.Debug ("ApplicationLauncher on" + state.ToString ());
			EditorExtensions.Instance.Visible = state;
		}

		private void OnGuiAppLauncherReady ()
		{
			try {
				this.button = ApplicationLauncher.Instance.AddModApplication (
					() => {EditorExtensions.Instance.Show();}, 	//RUIToggleButton.onTrue
					() => {EditorExtensions.Instance.Hide();},	//RUIToggleButton.onFalse
					() => {EditorExtensions.Instance.ShowMenu();}, //RUIToggleButton.OnHover
					() => {EditorExtensions.Instance.HideMenu();}, //RUIToggleButton.onHoverOut
					null, //RUIToggleButton.onEnable
					null, //RUIToggleButton.onDisable
					ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH, //visibleInScenes
					GameDatabase.Instance.GetTexture (texPathDefault, false) //texture
				);
				Log.Debug ("Added ApplicationLauncher button");
			} catch (Exception ex) {
				Log.Error ("Error adding ApplicationLauncher button: " + ex.Message);
			}

		}



		private void Update ()
		{
			if (this.button == null) {
				return;
			}

			try {
				if (EditorLogic.fetch != null) {
					if (EditorExtensions.Instance.Visible && this.button.State != RUIToggleButton.ButtonState.TRUE) {
						this.button.SetTrue ();
					} else if (!EditorExtensions.Instance.Visible && this.button.State != RUIToggleButton.ButtonState.FALSE) {
						this.button.SetFalse ();
					}
				} else if (this.button.State != RUIToggleButton.ButtonState.DISABLED) {
					this.button.Disable ();
				}
			} catch (Exception ex) {
				Log.Error ("Error updating ApplicationLauncher button: " + ex.Message);
			}
		}
	}
}