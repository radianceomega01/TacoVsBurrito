namespace TacoVsBurrito
{
    public interface ICardDropTarget
    {
        public bool CanDrop(CardBase card);
        public void DropCardAfterDrag(CardBase card);
    }
}