public static class GameProgression
{
    // Память о способностях, которая выдержит любой перезапуск уровня
    public static bool HasWings = false;
    public static bool HasEye = false;
    public static bool HasScepter = false;

    // Метод для полного сброса (вызовем, например, при выходе в главное меню)
    public static void ResetProgression()
    {
        HasWings = false;
        HasEye = false;
        HasScepter = false;
    }
}