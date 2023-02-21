using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

namespace cakeslice
{
    public class OutlineAnimation : MonoBehaviour
    {
        [SerializeField] float duration = 1;
        bool pingPong = false;

        OutlineEffect effect;

        // Use this for initialization
        void Start()
        {
            effect = GetComponent<OutlineEffect>();
        }

        // Update is called once per frame
        void Update()
        {
            Color c = effect.lineColor0;

            if(pingPong)
            {
                c.a += Time.deltaTime / duration;

                if(c.a >= 1)
                    pingPong = false;
            }
            else
            {
                c.a -= Time.deltaTime / duration;

                if(c.a <= 0)
                    pingPong = true;
            }

            c.a = Mathf.Clamp01(c.a);
            effect.lineColor0 = c;
            effect.UpdateMaterialsPublicProperties();
        }
    }
}