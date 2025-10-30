using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugPhysics2D 
{
    // 시작/끝 박스 + 스윕(네 모서리 레일) 그리기
    public static void DrawBoxCast(Vector2 origin, Vector2 size, float angleDeg, Vector2 dir, float distance,
                                   Color start, Color end, float duration = 0f)
    {
        dir = dir.normalized;
        var p0 = origin;
        var p1 = origin + dir * distance;

        DrawBox(p0, size, angleDeg, start, duration);
        DrawBox(p1, size, angleDeg, end, duration);

        var c0 = GetBoxCorners(p0, size, angleDeg);
        var c1 = GetBoxCorners(p1, size, angleDeg);
        for (int i = 0; i < 4; i++)
            Debug.DrawLine(c0[i], c1[i], Color.Lerp(start, end, 0.5f), duration, false);
    }

    // 히트 지점/노멀 그리기
    public static void DrawHits(RaycastHit2D[] hits, float normalLen = 0.25f, float duration = 0f)
    {
        foreach (var h in hits)
        {
            if (!h.collider) continue;
            Debug.DrawRay(h.point, h.normal * normalLen, Color.cyan, duration, false);
        }
    }

    public static void DrawBox(Bounds b, float angleDeg, Color col, float duration = 0f) =>
        DrawBox(b.center, b.size, angleDeg, col, duration);

    public static void DrawBox(Vector2 center, Vector2 size, float angleDeg, Color col, float duration = 0f)
    {
        var c = GetBoxCorners(center, size, angleDeg);
        for (int i = 0; i < 4; i++)
            Debug.DrawLine(c[i], c[(i + 1) & 3], col, duration, false);
    }

    static Vector2[] GetBoxCorners(Vector2 center, Vector2 size, float angleDeg)
    {
        var hw = size.x * 0.5f; var hh = size.y * 0.5f;
        var local = new Vector2[] { new(-hw, -hh), new(-hw, hh), new(hw, hh), new(hw, -hh) };
        var rad = angleDeg * Mathf.Deg2Rad; var s = Mathf.Sin(rad); var c = Mathf.Cos(rad);

        var outPts = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            var p = local[i];
            var r = new Vector2(p.x * c - p.y * s, p.x * s + p.y * c);
            outPts[i] = center + r;
        }
        return outPts;
    }
}
