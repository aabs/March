namespace DomainModel.Visitor
{
    public interface IVisitable
    {
        void Visit(IVisitor visitor);
    }
}