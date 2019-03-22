using UnityEditor;
using UnityEngine.EventSystems;

namespace Bose.Wearable
{
	[CustomEditor(typeof(WearableConnectUIPanel))]
	public sealed class WearableConnectUIPanelInspector : UnityEditor.Editor
	{
		private EventSystem _eventSystem;
		
		private const string EventSystemNotFoundWarning = "EventSystem not found. Please create one or input will not "+
															"be detected.";

		public override void OnInspectorGUI()
		{			
			DrawDefaultInspector();
			
			WarnIfNoEventSystemPresent();
		}

		private void WarnIfNoEventSystemPresent()
		{
			if (_eventSystem == null)
			{
				_eventSystem = FindObjectOfType<EventSystem>();
				
				if (_eventSystem == null)
				{
					EditorGUILayout.Space();
					EditorGUILayout.HelpBox(EventSystemNotFoundWarning, MessageType.Warning);
					EditorGUILayout.Space();
				}
			}
		}
	}
}
