Shader "Route1/TouchFlash" {
   Properties {
      _MainTex ("Main Tex", 2D) = "white" {}
      _Flash ("Flash", Range(0,1)) = 0
      _FlashColor ("Flash color", Color) = (1,1,1,1)
      _Alpha ("Alpha", Range(0.0,1.0)) = 1
   }
   SubShader {
   	
   	  Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
   
      Cull Back
      ZWrite On
      Blend SrcAlpha OneMinusSrcAlpha
      Lighting Off

      Pass {
         CGPROGRAM
         #pragma vertex vert_img
         #pragma fragment frag

         #include "UnityCG.cginc"

         uniform sampler2D _MainTex;
         uniform float _Flash;
         uniform float4 _FlashColor;
         float _Alpha;

         float4 frag(v2f_img i) : COLOR {
            float4 t = tex2D(_MainTex, i.uv);
            
            if (_Flash != 0) 
            {
               fixed luminance =  dot(t, fixed4(0.2126, 0.7152, 0.0722, 0));
            
	            if(luminance < 0.5)
	            	t.rgb = lerp(t.rgb, t.rgb+_FlashColor.rgb, _Flash);
	            else
	            	t.rgb = lerp(t.rgb, t.rgb*_FlashColor.rgb, _Flash);
            }
            
            t.a = _Alpha;
            return t;
         }
         ENDCG
      }
   }
   FallBack "Diffuse"
}