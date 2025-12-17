// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class LPSafeZoneBands : MonoBehaviour
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

public class LPSafeZoneBands : MonoBehaviour
{
    [Header("References")]
    public Transform human;   // 몸통 기준 (Torso/Pelvis 등)
    public Transform tipL3;
    public Transform tipL4;
    public Transform tipL5;

    [Header("Band Objects (Quad/Plane)")]
    public Transform bandL34;
    public Transform bandL45;

    [Header("Band Size (model units)")]
    public float bandWidth = 0.04f;   // 좌우 폭
    public float bandHeight = 0.02f;  // 상하(척추축) 폭

    [Header("Depth Offset")]
    public float offsetFromSkin = 0.002f; // z-fighting 방지용(등 바깥으로 약간 띄움)

    void LateUpdate()
    {
        if (!human || !tipL3 || !tipL4 || !tipL5 || !bandL34 || !bandL45) return;

        // interspace 중심점
        Vector3 mid34 = (tipL3.position + tipL4.position) * 0.5f;
        Vector3 mid45 = (tipL4.position + tipL5.position) * 0.5f;
        // Debug.DrawLine(tipL3.position, tipL4.position, Color.cyan);
        // Debug.DrawLine(tipL4.position, tipL5.position, Color.cyan);
        // Debug.Log($"mid34={mid34}, mid45={mid45}");

        // 척추의 대략 상하축(요추 구간)
        Vector3 spineUp = (tipL3.position - tipL5.position).normalized;

        // 몸통 기준 축 (이게 핵심: 자세/회전에 강함)
        Vector3 right = human.right;        // 좌우
        Vector3 back  = -human.forward;     // 등 바깥 방향(뒤쪽). human.forward가 앞(배꼽)이라면 -forward가 등쪽.

        PlaceBand(bandL34, mid34, spineUp, right, back);
        PlaceBand(bandL45, mid45, spineUp, right, back);
    }

    void PlaceBand(Transform band, Vector3 center, Vector3 spineUp, Vector3 right, Vector3 back)
    {
        // 띠가 "등 표면에 붙어있다"는 느낌: 등 바깥(back) 방향으로 살짝 띄움
        band.position = center + back * offsetFromSkin;

        // Quad의 local up을 spineUp, local right를 right로 맞추기
        // forward는 자동으로 up/right에 수직인 방향이 됨
        band.rotation = Quaternion.LookRotation(Vector3.Cross(right, spineUp).normalized, spineUp);

        // Quad 스케일 설정: (x=width, y=height)
        band.localScale = new Vector3(bandWidth, bandHeight, 1f);
    }


}
