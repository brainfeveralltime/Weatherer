/// <summary>
/// Системная информация
/// </summary>
public class Sys
{
    /// <summary>
    /// Внутренний тип
    /// </summary>
    public int Type { get; set; }
    
    /// <summary>
    /// Внутренний ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Код страны
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// Время восхода
    /// </summary>
    public long Sunrise { get; set; }

    /// <summary>
    /// Время заката
    /// </summary>
    public long Sunset { get; set; }
}