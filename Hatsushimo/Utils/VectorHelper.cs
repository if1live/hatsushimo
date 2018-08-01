using System.Numerics;

namespace Hatsushimo
{
    public class VectorHelper
    {
        // 영역을 벗어난 경우 영역 안의 좌표로 바꾸기
        // 영역은 원점이 중심인 직사각형으로 가정
        public static Vector2 FilterPosition(Vector2 pos, float width, float height)
        {
            // 범위 확인
            var halfw = width * 0.5f;
            var halfh = height * 0.5f;
            var x = pos.X;
            var y = pos.Y;
            if (x < -halfw) { x = -halfw; }
            if (x > halfw) { x = halfw; }
            if (y < -halfh) { y = -halfh; }
            if (y > halfh) { y = halfh; }
            return new Vector2(x, y);
        }

        // pos -> targetPos로 움직이고 싶은데
        // pos...targetPos의 거리가 이번 프레임에 움직일 거리보다 길면
        // targetPos를 지나쳐서 p1...targetPos...p2 와 같이 진동하는 문제가 생길수 있다
        // 이동할수 있는 거리 안에 targetPos가 있으면 targetPos로 이동하는 함수
        public static Vector2 MoveToTarget(Vector2 pos, Vector2 targetPos, float distance)
        {
            var sqrDist = distance * distance;

            var diff = targetPos - pos;
            var sqrDiff = diff.LengthSquared();
            if (sqrDist > sqrDiff)
            {
                return targetPos;
            }

            var dir = Vector2.Normalize(diff);
            var delta = dir * distance;
            return pos + delta;
        }

        public static bool IsInRange(Vector2 a, Vector2 b, float distance)
        {
            var sqrDistance = distance * distance;
            var diff = a - b;
            return (sqrDistance > diff.LengthSquared());
        }
    }
}
