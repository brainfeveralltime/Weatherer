/// <summary>
/// Основная информация о погоде
/// </summary>
public class Main
{
    /// <summary>
    /// Температура
    /// </summary>
    public double Temp { get; set; }

    /// <summary>
    /// Температура по ощущениям
    /// </summary>
    public double Feels_Like { get; set; }

    /// <summary>
    /// Минимальная температура
    /// </summary>
    public double Temp_Min { get; set; }

    /// <summary>
    /// Максимальная температура
    /// </summary>
    public double Temp_Max { get; set; }

    /// <summary>
    /// Давление
    /// </summary>
    public int Pressure { get; set; }

    /// <summary>
    /// Влажность
    /// </summary>
    public int Humidity { get; set; }

    /// <summary>
    /// Давление на уровне моря
    /// </summary>
    public int Sea_Level { get; set; }

    /// <summary>
    /// Давление на уровне земли
    /// </summary>
    public int Grnd_Level { get; set; }
}