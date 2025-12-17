// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ForceURPTransparent : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }

using UnityEngine;
using UnityEngine.Rendering;

public class ForceURPTransparent : MonoBehaviour
{
    [Range(0f, 1f)] public float alpha = 0.3f;

    void Start()
    {
        var r = GetComponent<Renderer>();
        if (!r) return;

        var m = r.material; // 인스턴스 생성

        // URP Lit 투명 모드 강제
        m.SetFloat("_Surface", 1f);                 // 0 Opaque, 1 Transparent
        m.SetFloat("_Blend", 0f);                   // 0 Alpha
        m.SetFloat("_ZWrite", 0f);                  // Depth Write Off
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_ALPHATEST_ON");

        // Render queue
        m.renderQueue = (int)RenderQueue.Transparent;

        // Alpha 적용
        var c = m.GetColor("_BaseColor");
        c.a = alpha;
        m.SetColor("_BaseColor", c);
    }
}
