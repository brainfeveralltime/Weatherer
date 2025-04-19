/// <summary>
/// Погодные условия
/// </summary>
public class Weather
{
    /// <summary>
    /// Код погодного состояния
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Описание на английском
    /// </summary>
    public string Main { get; set; }

    /// <summary>
    /// Описание локализированное
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Код иконки
    /// </summary>
    public string Icon { get; set; }
}