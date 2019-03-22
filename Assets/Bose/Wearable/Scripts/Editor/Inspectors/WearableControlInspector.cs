using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomEditor(typeof(WearableControl))]
	public sealed class WearableControlInspector : UnityEditor.Editor
	{
		private SerializedProperty _editorProvider;
		private SerializedProperty _runtimeProvider;

		private const string WearableDeviceProviderUnsupportedEditorWarning =
			"The Wearable Device Provider is not supported in the editor.";
		private const string USBProviderUnsupportedOutsideEditorWarning =
			"The USB Provider is only supported in the editor.";
		private const string OverrideConfigPresentWarning =
			"The device config is currently overridden and does not reflect the normal device config present " +
			"during normal runtime usage.";

		private const string EditorDefaultTitle = "Editor Default";
		private const string RuntimeDefaultTitle = "Runtime Default";
		private const string ResolvedDeviceConfigTitle = "Resolved Device Config";
		private const string OverrideDeviceConfigTitle = "Override Device Config";
		private const string TitleSeparator = " - ";

		private const string EditorDefaultProviderField = "_editorDefaultProvider";
		private const string RuntimeDefaultProviderField = "_runtimeDefaultProvider";
		private const string UpdateModeField = "_updateMode";
		private const string DebugProviderField = "_debugProvider";
		private const string DeviceProviderField = "_deviceProvider";
		private const string MobileProviderField = "_mobileProvider";
		private const string USBProviderField = "_usbProvider";
		private const string ProxyProviderField = "_proxyProvider";

		private const string FinalWearableDeviceConfigField = "_finalWearableDeviceConfig";
		private const string OverrideWearableDeviceConfigField = "_overrideDeviceConfig";

		private WearableControl _wearableControl;

		private void OnEnable()
		{
			_editorProvider = serializedObject.FindProperty(EditorDefaultProviderField);
			_runtimeProvider = serializedObject.FindProperty(RuntimeDefaultProviderField);

			_wearableControl = (WearableControl)target;
		}

		private void DrawProviderBox(string field, ProviderId provider)
		{
			bool isEditorDefault = _editorProvider.enumValueIndex == (int) provider;
			bool isRuntimeDefault = _runtimeProvider.enumValueIndex == (int) provider;

			if (isEditorDefault || isRuntimeDefault)
			{
				GUILayoutTools.LineSeparator();

				StringBuilder titleBuilder = new StringBuilder();
				titleBuilder.Append(Enum.GetName(typeof(ProviderId), provider));

				if (isEditorDefault)
				{
					titleBuilder.Append(TitleSeparator);
					titleBuilder.Append(EditorDefaultTitle);
				}

				if (isRuntimeDefault)
				{
					titleBuilder.Append(TitleSeparator);
					titleBuilder.Append(RuntimeDefaultTitle);
				}

				EditorGUILayout.LabelField(titleBuilder.ToString(), WearableConstants.EmptyLayoutOptions);

				EditorGUILayout.PropertyField(serializedObject.FindProperty(field), WearableConstants.EmptyLayoutOptions);
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(UpdateModeField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(_editorProvider, WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(_runtimeProvider, WearableConstants.EmptyLayoutOptions);

			DrawProviderBox(DebugProviderField, ProviderId.DebugProvider);
			DrawProviderBox(ProxyProviderField, ProviderId.WearableProxy);
			DrawProviderBox(MobileProviderField, ProviderId.MobileProvider);
			DrawProviderBox(USBProviderField, ProviderId.USBProvider);
			DrawProviderBox(DeviceProviderField, ProviderId.WearableDevice);
			if (_editorProvider.enumValueIndex == (int) ProviderId.WearableDevice)
			{
				EditorGUILayout.HelpBox(WearableDeviceProviderUnsupportedEditorWarning, MessageType.Error);
			}
			if (_runtimeProvider.enumValueIndex == (int)ProviderId.USBProvider)
			{
				EditorGUILayout.HelpBox(USBProviderUnsupportedOutsideEditorWarning, MessageType.Error);
			}

			if (Application.isPlaying)
			{
				GUILayoutTools.LineSeparator();


				if (_wearableControl.IsOverridingDeviceConfig)
				{
					EditorGUILayout.LabelField(OverrideDeviceConfigTitle, WearableConstants.EmptyLayoutOptions);
					EditorGUILayout.HelpBox(OverrideConfigPresentWarning, MessageType.Warning);
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(
						serializedObject.FindProperty(OverrideWearableDeviceConfigField),
						WearableConstants.EmptyLayoutOptions);
				}
				else
				{
					EditorGUILayout.LabelField(ResolvedDeviceConfigTitle, WearableConstants.EmptyLayoutOptions);
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(
						serializedObject.FindProperty(FinalWearableDeviceConfigField),
						WearableConstants.EmptyLayoutOptions);
				}

				EditorGUI.EndDisabledGroup();
			}

			serializedObject.ApplyModifiedProperties();
		}

		public override bool RequiresConstantRepaint()
		{
			return Application.isPlaying;
		}
	}
}
