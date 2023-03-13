using UnityEngine;
using System.Collections;
using CoreSystem;

public abstract class AssetBundleLoadOperation : IEnumerator
{
	public object Current
	{
		get
		{
			return null;
		}
	}
	public bool MoveNext()
	{
		return !IsDone();
	}
	
	public void Reset()
	{
	}
	
	abstract public bool Update ();
	
	abstract public bool IsDone ();

	abstract public bool HasCompletedSuccessfully();
}

public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
{	
	public AssetBundleLoadLevelSimulationOperation ()
	{
	}
	
	public override bool Update ()
	{
		return false;
	}
	
	public override bool IsDone ()
	{		
		return true;
	}

	public override bool HasCompletedSuccessfully ()
	{		
		return true;
	}
}

public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
{
	protected string 				m_AssetBundleName;
	protected string 				m_LevelName;
	protected bool 						m_IsAdditive;
	protected string 				m_DownloadingError;
	protected AsyncOperation		m_Request;

	public AssetBundleLoadLevelOperation (string assetbundleName, string levelName, bool isAdditive)
	{
		m_AssetBundleName = assetbundleName;
		m_LevelName = levelName;
		m_IsAdditive = isAdditive;
	}

	public override bool Update ()
	{
		if (m_Request != null)
			return false;
		
		LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if(string.IsNullOrEmpty(m_DownloadingError) == false)
		{
			Debug.LogError("Asset bundle download error: " + m_AssetBundleName + " - " + m_DownloadingError);
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_LOADING_FAILED);
			return false;
		}

		if (bundle != null)
		{
			if (m_IsAdditive)
				m_Request = Application.LoadLevelAdditiveAsync (m_LevelName);
			else
				m_Request = Application.LoadLevelAsync (m_LevelName);
			return false;
		}
		else
			return true;
	}
	
	public override bool IsDone ()
	{
		// Return if meeting downloading error.
		// m_DownloadingError might come from the dependency downloading.
		if (m_Request == null && m_DownloadingError != null)
		{
			return true;
		}
		
		return m_Request != null && m_Request.isDone;
	}

	public override bool HasCompletedSuccessfully ()
	{		
		return IsDone() && string.IsNullOrEmpty(m_DownloadingError);
	}
}

public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
{
	public abstract T GetAsset<T>() where T : UnityEngine.Object;
}

public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
{
	Object							m_SimulatedObject;
	
	public AssetBundleLoadAssetOperationSimulation (Object simulatedObject)
	{
		m_SimulatedObject = simulatedObject;
	}
	
	public override T GetAsset<T>()
	{
		return m_SimulatedObject as T;
	}
	
	public override bool Update ()
	{
		return false;
	}
	
	public override bool IsDone ()
	{
		return true;
	}

	public override bool HasCompletedSuccessfully ()
	{
		return true;
	}
}

public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
{
	protected string 				m_AssetBundleName;
	protected string 				m_AssetName;
	protected string 				m_DownloadingError;
	protected System.Type 			m_Type;
	protected AssetBundleRequest	m_Request = null;

	public AssetBundleLoadAssetOperationFull (string bundleName, string assetName, System.Type type)
	{
		m_AssetBundleName = bundleName;
		m_AssetName = assetName;
		m_Type = type;
	}
	
	public override T GetAsset<T>()
	{
		if (m_Request != null && m_Request.isDone)
			return m_Request.asset as T;
		else
			return null;
	}
	
	// Returns true if more Update calls are required.
	public override bool Update ()
	{
		if (m_Request != null)
			return false;

		LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if(string.IsNullOrEmpty(m_DownloadingError) == false)
		{
			Debug.LogError("Asset bundle download error: " + m_AssetBundleName + " - " + m_DownloadingError);
			return false;
		}

		if (bundle != null)
		{
			m_Request = bundle.m_AssetBundle.LoadAssetAsync (m_AssetName, m_Type);
			return false;
		}
		else
		{
			return true;
		}
	}
	
	public override bool IsDone ()
	{
		// Return if meeting downloading error.
		// m_DownloadingError might come from the dependency downloading.
		if (m_Request == null && m_DownloadingError != null)
		{
			return true;
		}

		return m_Request != null && m_Request.isDone;
	}

	public override bool HasCompletedSuccessfully ()
	{		
		return IsDone() && string.IsNullOrEmpty(m_DownloadingError) == false;
	}
}

public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
{
	public AssetBundleLoadManifestOperation (string bundleName, string assetName, System.Type type)
		: base(bundleName, assetName, type)
	{
	}

	public override bool Update ()
	{		
		LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if(string.IsNullOrEmpty(m_DownloadingError) == false)
		{
			Debug.LogError("Asset bundle download error: " + m_AssetBundleName + " - " + m_DownloadingError);
			return false;
		}
		
		if (bundle != null)
		{
			m_Request = bundle.m_AssetBundle.LoadAssetAsync (m_AssetName, m_Type);
			if (m_Request != null && m_Request.isDone)
			{
				AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
				return false;
			}
		}

		return true;
	}
}

