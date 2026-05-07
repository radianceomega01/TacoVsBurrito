namespace TacoVsBurrito
{
    public interface ICardDropTarget
    {
        public abstract bool CanDrop(CardBase card);
        public abstract void DropCardAfterDrag(CardBase card);
    }
}