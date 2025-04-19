/// <summary>
/// Итоговый ответ openweathermap
/// </summary>
public class WeatherApiResponse
{
    /// <summary>
    /// Информация о координатах
    /// </summary>
    public Coord Coord { get; set; }

    /// <summary>
    /// Массив погодных условий
    /// </summary>
    public Weather[] Weather { get; set; }

    /// <summary>
    /// Источник данных
    /// </summary>
    public string Base { get; set; }

    /// <summary>
    /// Основные метео-параметры
    /// </summary>
    public Main Main { get; set; }

    /// <summary>
    /// Видимость
    /// </summary>
    public int Visibility { get; set; }

    /// <summary>
    /// Информация о ветре
    /// </summary>
    public Wind Wind { get; set; }

    /// <summary>
    /// Облака
    /// </summary>
    public Clouds Clouds { get; set; }

    /// <summary>
    /// Время в формате UNIX
    /// </summary>
    public long Dt { get; set; }
    
    /// <summary>
    /// Системные данные
    /// </summary>
    public Sys Sys { get; set; }

    /// <summary>
    /// Смещение от UTC
    /// </summary>
    public int Timezone { get; set; }

    /// <summary>
    /// ID города
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название города
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Код ответа
    /// </summary>
    public int Cod { get; set; }
}
