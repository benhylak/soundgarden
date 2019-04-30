using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	/// <summary>
	/// Menu items for developers for Bose Wearable resources
	/// </summary>
	public static class DeveloperMenuItems
	{
		// Build Menu Items
		private const string BuildProxyServerMenuItem = "Tools/Bose Wearable/Build Proxy Server";
		private const string BuildWearableDemoMenuItem = "Tools/Bose Wearable/Build Wearable Demo";

		// Developer Help Menu Items
		private const string DeveloperPortalMenuItem = "Tools/Bose Wearable/Help/Developer Portal";
		private const string DeveloperForumsMenuItem = "Tools/Bose Wearable/Help/Forums";
		private const string DeveloperDocumentationMenuItem = "Tools/Bose Wearable/Help/Documentation";
		private const string DeveloperReportBugMenuItem = "Tools/Bose Wearable/Help/Report a Bug";
		private const string DeveloperAboutMenuItem = "Tools/Bose Wearable/Help/About";

		// Links
		private const string ForumLink = "https://bosedevs.bose.com/categories/bose-ar-unity-plugin";
		private const string DocumentationLink =
			"https://developer.bose.com/guides/bose-ar/unity-plug-high-level-overview";
		private const string PortalLink = "https://developer.bose.com/bose-ar";
		private const string ReportABugLink =
			"mailto:help@bosear.zendesk.com?subject=Bose%20AR%20Unity%20SDK%20Bug%20Report";

		[MenuItem(BuildProxyServerMenuItem, priority = 100)]
		public static void BuildWearableProxy()
		{
			BuildTools.BuildProxyServer();
		}

		[MenuItem(BuildProxyServerMenuItem, validate = true)]
		private static bool IsSupportedPlatformForProxyServer()
		{
			BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
			return activeTarget == BuildTarget.iOS || activeTarget == BuildTarget.Android;
		}

		[MenuItem(BuildWearableDemoMenuItem, priority = 100)]
		public static void BuildWearableDemo()
		{
			BuildTools.BuildWearableDemo();
		}

		[MenuItem(BuildWearableDemoMenuItem, validate = true)]
		private static bool IsSupportedPlatformForWearableDemo()
		{
			BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
			return activeTarget == BuildTarget.iOS || activeTarget == BuildTarget.Android;
		}

		[MenuItem(DeveloperPortalMenuItem)]
		public static void LaunchBoseWearablePortal()
		{
			Application.OpenURL(PortalLink);
		}

		[MenuItem(DeveloperDocumentationMenuItem)]
		public static void LaunchBoseWearableDocumentation()
		{
			Application.OpenURL(DocumentationLink);
		}

		[MenuItem(DeveloperForumsMenuItem)]
		public static void LaunchBoseWearableForum()
		{
			Application.OpenURL(ForumLink);
		}

		[MenuItem(DeveloperReportBugMenuItem)]
		public static void LaunchBoseWearableReportABug()
		{
			Application.OpenURL(ReportABugLink);
		}

		[MenuItem(DeveloperAboutMenuItem, priority = 100)]
		public static void LaunchAboutWindow()
		{
			DeveloperAboutWindow.LaunchWindow();
		}
	}
}
