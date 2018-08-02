using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using Optional.Unsafe;

namespace Shigure.Agents
{
    // 아무것도 하지 않고 제자리에서 핑만 날리는 봇
    // 테스트용 표적으로 쓸수 있다
    public class IdleAgent : BaseAgent
    {
        float lifetime;

        public IdleAgent(Connection conn, float lifetime)
        : base(conn, "idle")
        {
            this.lifetime = lifetime;
        }

        protected override async Task<bool> Think(List<int> removeIDList)
        {
            if (lifetime < 0) { return false; }

            var ping = new PingPacket(TimeUtils.NowMillis);
            conn.Send(ping);
            var pong = await conn.Recv<PingPacket>();
            Log.Info($"ping: {TimeUtils.NowMillis - pong.ValueOrFailure().Millis}");

            var interval = 1.0f;
            lifetime -= interval;
            await Task.Delay(TimeSpan.FromSeconds(interval));

            return true;
        }
    }
}
