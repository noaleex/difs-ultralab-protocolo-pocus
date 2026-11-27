public static class FoodInventory
{
    public static ChoiceFood.TipoComida SelectedFood { get; private set; }
        = ChoiceFood.TipoComida.Nenhuma;

    public static bool HasFood => SelectedFood != ChoiceFood.TipoComida.Nenhuma;

    public static void Set(ChoiceFood.TipoComida food)
    {
        SelectedFood = food;
    }

    public static void Clear()
    {
        SelectedFood = ChoiceFood.TipoComida.Nenhuma;
    }
}
