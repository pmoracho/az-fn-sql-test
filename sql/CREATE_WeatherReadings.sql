CREATE TABLE WeatherReadings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Temp DECIMAL(5, 2),
    FeelsLike DECIMAL(5, 2),
    TempMin DECIMAL(5, 2),
    TempMax DECIMAL(5, 2),
    Pressure INT,
    Humidity INT,
    Visibility INT,
    WindSpeed DECIMAL(5, 2),
    WindDeg INT,
    Description NVARCHAR(255),
    ReadingTime DATETIME
);