
public interface ICOMovement : IMovable
{
    bool GetCanForge();

    void SetCO(ConstructObject baseCO_);
    void SetForging(bool isForging_);
}
