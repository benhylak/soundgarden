using UnityEditor;
using UnityEngine;
using Bose.Wearable.Proxy;

namespace Bose.Wearable.Editor.Inspectors
{

	[CustomPropertyDrawer(typeof(WearableProxyProvider))]
	public class WearableProxyProviderDrawer : PropertyDrawer
	{
		private const string TimeoutField = "_networkTimeout";
		private const string HostnameField = "_hostname";
		private const string PortField = "_portNumber";
		private const string DescriptionBox =
			"Allows access to a device through a proxy server. To use, start the server app and enter the hostname " +
			"and port number below. All provider commands will be automatically synced with the remote device.";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{			
			EditorGUI.BeginProperty(position, label, property);
			
			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.Space();
			
			EditorGUILayout.PropertyField(property.FindPropertyRelative(TimeoutField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(HostnameField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(PortField), WearableConstants.EmptyLayoutOptions);

			EditorGUI.EndProperty();
		}
	}
	
}
