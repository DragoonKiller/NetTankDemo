Shader "Hidden/UVRescale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _TargetSize ("Target Size", vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        
        Tags { "Queue" = "Transparent" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../Utils.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _TargetSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            // 计算一个维度的采样点.
            // texLen 是贴图长度.
            // targetLen 是最终的绘制大小.
            // 为了保持分辨率不变, 需要计算像素坐标.
            float GetSamplePoint(float texLen, float ratio, float targetLen)
            {
                float curLen = targetLen * ratio;
                if(curLen < texLen / 2) return curLen / texLen; // 取左半部分.
                else if(targetLen - curLen < texLen / 2) return 1 - (targetLen - curLen) / texLen; // 取右半部分.
                return 0.5; //  取中间.
            }
            
            
            float4 frag (v2f i) : SV_Target
            {
                // 这个 shader 用于扩展贴图的绘制平面同时保持贴图的分辨率.
                float2 uv = float2(
                    GetSamplePoint(_MainTex_TexelSize.z, i.uv.x, _TargetSize.x),
                    GetSamplePoint(_MainTex_TexelSize.w, i.uv.y, _TargetSize.y)
                );
                float4 color = tex2D(_MainTex, uv) * i.color;
                // float4 color = float4(uv.x, 0, uv.y, 1) * i.color;
                return color;
            }
            ENDCG
        }
    }
}
