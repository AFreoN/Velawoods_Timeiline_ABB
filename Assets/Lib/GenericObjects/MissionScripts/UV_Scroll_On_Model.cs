using UnityEngine;
using System.Collections;

public class UV_Scroll_On_Model : MonoBehaviour 
{
	Renderer m_renderer = null;

	void Start()
	{
		m_renderer = GetComponent<Renderer> ();
	}

	public float scrollSpeed = 0.05f;
	Vector2 scrollVector  = new Vector2(0.0f, 1.0f);

	void Update () 
	{
		if (m_renderer) 
		{
			m_renderer.material.SetTextureOffset ("_MainTex", scrollVector * Time.time * scrollSpeed);
		}
	}
}