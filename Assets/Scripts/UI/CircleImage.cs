using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

[AddComponentMenu("UI/Circle Image")]
public class CircleImage : Image {
	
	// Update is called once per frame
	void Update () {

	}

    public float fillPercent = 1f;
    public bool fill = true;
    public int segements = 20;

    private List<Vector3> outterVertices = new List<Vector3>();

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        outterVertices.Clear();

        float degreeDelta = (float)(2 * Mathf.PI / segements);
        int curSegements = (int)(segements * fillPercent);

        float tw = rectTransform.rect.width;
        float th = rectTransform.rect.height;
        float outerRadius = rectTransform.pivot.x * tw;

        Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

        float uvCenterX = (uv.x + uv.z) * 0.5f;
        float uvCenterY = (uv.y + uv.w) * 0.5f;
        float uvScaleX = (uv.z - uv.x) / tw;
        float uvScaleY = (uv.w - uv.y) / th;

        float curDegree = 0;
        UIVertex uiVertex;
        int verticeCount;
        int triangleCount;
        Vector2 curVertice;

        curVertice = Vector2.zero;
        verticeCount = curSegements + 1;
        uiVertex = new UIVertex();
        uiVertex.color = color;
        uiVertex.position = curVertice;
        uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
        vh.AddVert(uiVertex);

        for (int i = 1; i < verticeCount; i++)
        {
            float cosA = Mathf.Cos(curDegree);
            float sinA = Mathf.Sin(curDegree);
            curVertice = new Vector2(cosA * outerRadius, sinA * outerRadius);
            curDegree += degreeDelta;

            uiVertex = new UIVertex();
            uiVertex.color = color;
            uiVertex.position = curVertice;
            uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
            vh.AddVert(uiVertex);

            outterVertices.Add(curVertice);
        }

        triangleCount = curSegements * 3;
        for (int i = 0, vIdx = 1; i < triangleCount - 3; i += 3, vIdx++)
        {
            vh.AddTriangle(vIdx, 0, vIdx + 1);
        }
        if (fillPercent == 1)
        {
            vh.AddTriangle(verticeCount - 1, 0, 1);
        }

    }

}
