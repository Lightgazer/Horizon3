namespace Horizon3.GameScene.Model
{
    public interface IReadOnlyBlock
    {
        int Type { get; }
        bool Alive { get; }
        BonusLogic Bonus { get; }
    }

    public class BlockData : IReadOnlyBlock
    {
        public int Type { get; set; }
        public bool Alive { get; set; } = true;
        /// <summary>
        /// Флаг которым пемечается последний передвинутый блок, на его месте может возникнуть бонус.
        /// </summary>
        public bool Suspect { get; set; }
        public BonusLogic Bonus { get; set; }
    }
}
