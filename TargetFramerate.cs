using Gist.Performance;
using ModelDrivenGUISystem;
using ModelDrivenGUISystem.Factory;
using ModelDrivenGUISystem.Scope;
using ModelDrivenGUISystem.ValueWrapper;
using ModelDrivenGUISystem.View;
using nobnak.Gist.InputDevice;
using nobnak.Gist.Loader;
using UnityEngine;

namespace Gist.ModelDrivenGUI {

	public class TargetFramerate : MonoBehaviour {
		[SerializeField]
		protected Data data;
		[SerializeField]
		protected FilePath serialized;
		[SerializeField]
		protected KeycodeToggle toggleUI;
		[SerializeField]
		protected FrameRateJob frameRate;

		protected BaseView view;
		protected Rect viewWindow;
		protected Coroutine job;

		#region Unity
		private void OnEnable() {
			viewWindow = new Rect(10, 10, 200, 100);

			serialized.TryLoadOverwrite(ref data);
			data.Apply();

			var viewFactory = new SimpleViewFactory();
			view = ClassConfigurator.GenerateClassView(new BaseValue<object>(data), viewFactory);

			job = StartCoroutine(frameRate.GetEnumerator());
		}
		protected void Update() {
			toggleUI.Update();
		}
		protected void OnGUI() {
			if (toggleUI.Visible)
				viewWindow = GUILayout.Window(GetInstanceID(), viewWindow, Window, name);
		}
		protected void OnDisable() {
			if (view != null) {
				view.Dispose();
				view = null;
			}
			if (job != null) {
				StopCoroutine(job);
				job = null;
			}
		}
		#endregion

		protected void Window(int id) {
			using (new GUILayout.VerticalScope()) {
				GUILayout.Label(frameRate.ToString());

				using (new GUIChangedScope(Changed))
					view.Draw();

			}
			GUI.DragWindow();
		}
		protected void Changed() {
			serialized.TrySave(data);
			data.Apply();
		}

		[System.Serializable]
		public class Data {
			public int targetFrameRate = -1;
			public int vSyncCount = 0;

			public void Apply() {
				Application.targetFrameRate = (targetFrameRate >= 0 ? targetFrameRate : -1);
				QualitySettings.vSyncCount = Mathf.Clamp(vSyncCount, 0, 4);
			}
		}
	}
}
