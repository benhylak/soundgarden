using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bose.Wearable
{
	/// <summary>
	/// Loads and shows the most specific WearableModel configured on this instance.
	/// </summary>
	public class WearableModelLoader : MonoBehaviour
	{
		/// <summary>
		/// Invoked when a new WearableModel has been set.
		/// </summary>
		public event Action<GameObject> WearableModelChanged;

		[Serializable]
		private class WearableProductModelInfo
		{
			#pragma warning disable 0649
			public ProductType productType;
			[FormerlySerializedAs("gameObject")]
			public GameObject displayModel;
			#pragma warning disable 0649
		}

		[Serializable]
		private class WearableVariantModelInfo
		{
			#pragma warning disable 0649
			public VariantType variantType;
			[FormerlySerializedAs("gameObject")]
			public GameObject displayModel;
			#pragma warning disable 0649
		}

		/// <summary>
		/// The strategy that will be followed for instantiating the WearableModels.
		/// </summary>
		private enum LoadingStrategy
		{
			/// <summary>
			/// WearableModel prefabs will be instantiated on demand and then cached for reuse.
			/// </summary>
			LazyLoad,

			/// <summary>
			/// All WearableModel prefabs are instantiated on Awake
			/// </summary>
			AllAtOnce
		}

		[Header("Configuration")]
		[SerializeField]
		private LoadingStrategy _loadingStrategy;

		[Header("Scene Refs")]
		[SerializeField]
		private Transform _modelParentTransform;

		[FormerlySerializedAs("_originalDefaultWearableGameObject")]
		[Header("Prefabs"), Space(5)]
		[Header("General-Purpose Model (Final Fallback)")]
		public GameObject _generalFallbackWearableModel;

		[FormerlySerializedAs("_wearableProductModelInfo")]
		[Header("General Product Models (Fallback Models for Variants)")]
		[SerializeField]
		private WearableProductModelInfo[] _productFallbackWearableModels;

		[FormerlySerializedAs("_wearableVariantModelInfo")]
		[Header("Variant Models (Most Specific Models)")]
		[SerializeField]
		private WearableVariantModelInfo[] _variantWearableModels;

		// All private references to instances of WearableModel prefabs
		private GameObject _fallbackWearableGameObject;
		private Dictionary<VariantType, GameObject> _wearableGameObjectsByProductVariant;
		private Dictionary<ProductType, GameObject> _wearableGameObjectsByProduct;

		private WearableControl _wearableControl;

		private void Awake()
		{
			_wearableGameObjectsByProductVariant = new Dictionary<VariantType, GameObject>();
			_wearableGameObjectsByProduct = new Dictionary<ProductType, GameObject>();

			if (_loadingStrategy == LoadingStrategy.AllAtOnce)
			{
				InitializeAllWearableModels();
			}

			_wearableControl = WearableControl.Instance;
			_wearableControl.DeviceConnected += OnDeviceConnected;
		}

		private void Start()
		{
			SetWearableModel();
		}

		private void OnDestroy()
		{
			_wearableControl.DeviceConnected -= OnDeviceConnected;
		}

		/// <summary>
		/// When a device is connected, set a new WearableModel in case the connected device has changed or
		/// it is the first time connecting.
		/// </summary>
		/// <param name="obj"></param>
		private void OnDeviceConnected(Device obj)
		{
			SetWearableModel();
		}

		/// <summary>
		/// Initializes a single instance of one <see cref="GameObject"/> per wearable model prefab available
		/// in a lookup based on a <see cref="VariantType"/> key, <see cref="ProductType"/> key, and a
		/// fallback default model.
		/// </summary>
		private void InitializeAllWearableModels()
		{
			if (_variantWearableModels != null)
			{
				for (var i = 0; i < _variantWearableModels.Length; i++)
				{
					var wmi = _variantWearableModels[i];
					if (!_wearableGameObjectsByProductVariant.ContainsKey(wmi.variantType))
					{
						var newModelGameObject = CreateWearableModel(wmi.displayModel);

						_wearableGameObjectsByProductVariant.Add(wmi.variantType, newModelGameObject);
					}
					else
					{
						var msg = string.Format(WearableConstants.DuplicateWearableModelVariantTypeWarning, wmi.variantType);
						Debug.LogWarning(msg, this);
					}
				}
			}

			if (_productFallbackWearableModels != null)
			{
				for (var i = 0; i < _productFallbackWearableModels.Length; i++)
				{
					var wmi = _productFallbackWearableModels[i];
					if (!_wearableGameObjectsByProduct.ContainsKey(wmi.productType))
					{
						var newModelGameObject = CreateWearableModel(wmi.displayModel);

						_wearableGameObjectsByProduct.Add(wmi.productType, newModelGameObject);
					}
					else
					{
						var msg = string.Format(WearableConstants.DuplicateWearableModelProductTypeWarning, wmi.productType);
						Debug.LogWarning(msg, this);
					}
				}
			}

			_fallbackWearableGameObject = CreateWearableModel(_generalFallbackWearableModel);
		}

		/// <summary>
		/// Attempts to return the most specific <see cref="GameObject"/> for a WearableModel based on
		/// <see cref="VariantType"/>, then <see cref="ProductType"/>, then a default placeholder. Invokes
		/// <see cref="WearableModelChanged"/> if any subscribers are listening with the selected <see cref="GameObject"/>.
		/// </summary>
		public void SetWearableModel()
		{
			DisableAllWearableModels();

			var modelGameObject = GetWearableModel();
			modelGameObject.transform.SetParent(GetModelParentTransform());
			modelGameObject.gameObject.SetActive(true);

			if (WearableModelChanged != null)
			{
				WearableModelChanged.Invoke(modelGameObject);
			}
		}

		/// <summary>
		/// Disable all instances of WearableModels associated with this <see cref="WearableModelLoader"/>
		/// </summary>
		public void DisableAllWearableModels()
		{
			var parentTransform = GetModelParentTransform();
			var childCount = parentTransform.childCount;
			for (var i = 0; i < childCount; i++)
			{
				var child = parentTransform.GetChild(i);
				child.gameObject.SetActive(false);
			}
		}

		private Transform GetModelParentTransform()
		{
			return _modelParentTransform == null ? transform : _modelParentTransform;
		}

		/// <summary>
		/// Attempts to return the most specific <see cref="GameObject"/> for a WearableModel based on
		/// <see cref="VariantType"/>, then <see cref="ProductType"/>, then a default placeholder.
		/// </summary>
		/// <returns></returns>
		private GameObject GetWearableModel()
		{
			var modelGameObject = _fallbackWearableGameObject;

			if (_wearableControl != null && _wearableControl.ConnectedDevice.HasValue)
			{
				var variantType = _wearableControl.ConnectedDevice.Value.GetVariantType();
				var productType = _wearableControl.ConnectedDevice.Value.GetProductType();

				// If we cannot get either a variant or a product specific WearableModel,
				// assign the general fallback one.
				if (!TryGetVariant(variantType, out modelGameObject) &&
				    !TryGetProduct(productType, out modelGameObject))
				{
					if (_fallbackWearableGameObject == null)
					{
						_fallbackWearableGameObject = CreateWearableModel(_generalFallbackWearableModel);
					}

					modelGameObject = _fallbackWearableGameObject;
				}
			}

			return modelGameObject;
		}

		private GameObject CreateWearableModel(GameObject modelGameObject)
		{
			var newModelGameObject = Instantiate(modelGameObject);
			newModelGameObject.transform.SetParent(GetModelParentTransform());
			newModelGameObject.transform.localPosition = Vector3.zero;
			newModelGameObject.gameObject.SetActive(false);

			return newModelGameObject;
		}

		/// <summary>
		/// Returns true if a Variant <see cref="GameObject"/> <paramref name="modelGameObject"/> is available,
		/// otherwise returns false. If true and the matching <see cref="GameObject"/> <paramref name="modelGameObject"/>
		/// has not been instantiated yet, it will be instantiated and cached.
		/// </summary>
		/// <param name="variantType"></param>
		/// <param name="modelGameObject"></param>
		/// <returns></returns>
		private bool TryGetVariant(VariantType variantType, out GameObject modelGameObject)
		{
			modelGameObject = null;
			var foundVariantModel = false;
			for (var i = 0; i < _variantWearableModels.Length; i++)
			{
				var wmi = _variantWearableModels[i];
				if (wmi.variantType == variantType)
				{
					foundVariantModel = true;

					if (!_wearableGameObjectsByProductVariant.TryGetValue(variantType, out modelGameObject))
					{
						var newModelGameObject = CreateWearableModel(wmi.displayModel);

						_wearableGameObjectsByProductVariant.Add(wmi.variantType, newModelGameObject);

						modelGameObject = newModelGameObject;
					}

					break;
				}
			}

			return foundVariantModel;
		}

		/// <summary>
		/// Returns true if a Product <see cref="GameObject"/> <paramref name="modelGameObject"/> is available,
		/// otherwise returns false. If true and the matching <see cref="GameObject"/> <paramref name="modelGameObject"/>
		/// has not been instantiated yet, it will be instantiated and cached.
		/// </summary>
		/// <param name="productType"></param>
		/// <param name="modelGameObject"></param>
		/// <returns></returns>
		private bool TryGetProduct(ProductType productType, out GameObject modelGameObject)
		{
			modelGameObject = null;
			var foundVariantModel = false;
			for (var i = 0; i < _productFallbackWearableModels.Length; i++)
			{
				var wmi = _productFallbackWearableModels[i];
				if (wmi.productType == productType)
				{
					foundVariantModel = true;

					if (!_wearableGameObjectsByProduct.TryGetValue(productType, out modelGameObject))
					{
						var newModelGameObject = CreateWearableModel(wmi.displayModel);

						_wearableGameObjectsByProduct.Add(wmi.productType, newModelGameObject);

						modelGameObject = newModelGameObject;
					}

					break;
				}
			}

			return foundVariantModel;
		}
	}
}
