using System;

namespace EnemyBalance
{
    public enum EnemyCycleStage { Normal, Boss, Rush, Complete }

    /// <summary>通常→中ボスのインターバル→大群を絶対時刻で進行する</summary>
    public sealed class EnemyCycleTimeline
    {
        private readonly double startsAt;
        private readonly double normalDuration;
        private readonly double rushDuration;
        private readonly double bossInterval;
        public EnemyCycleStage Stage { get; private set; }
        public bool HasStarted { get; private set; }
        public bool IsComplete => Stage == EnemyCycleStage.Complete;
        public double EndsAt => startsAt + normalDuration + bossInterval + rushDuration;

        public EnemyCycleTimeline(double startsAt, double normalDuration, double rushDuration, double bossInterval = 10d)
        {
            if (!Finite(startsAt) || startsAt < 0 || !Finite(normalDuration) || normalDuration < 0
                || !Finite(rushDuration) || rushDuration < 0 || !Finite(bossInterval) || bossInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(startsAt));
            this.startsAt = startsAt;
            this.normalDuration = normalDuration;
            this.rushDuration = rushDuration;
            this.bossInterval = bossInterval;
        }

        public void Advance(double time, bool bossDefeated)
        {
            if (!Finite(time) || time < startsAt || IsComplete) return;
            HasStarted = true;
            // 撃破通知は時刻を変えない。早期撃破でも未撃破でもインターバルは設定秒数を保つ。
            double elapsed = time - startsAt;
            EnemyCycleStage next = elapsed >= normalDuration + bossInterval + rushDuration ? EnemyCycleStage.Complete
                : elapsed >= normalDuration + bossInterval ? EnemyCycleStage.Rush
                : elapsed >= normalDuration ? EnemyCycleStage.Boss : EnemyCycleStage.Normal;
            if (next > Stage) Stage = next;
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
